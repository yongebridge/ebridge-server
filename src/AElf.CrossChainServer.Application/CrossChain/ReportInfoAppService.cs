using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using Nest;
using System.Linq;
using AElf.Contracts.Report;
using AElf.CrossChainServer.Chains;
using AElf.CrossChainServer.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp;

namespace AElf.CrossChainServer.CrossChain;

[RemoteService(IsEnabled = false)]
public class ReportInfoAppService : CrossChainServerAppService,IReportInfoAppService
{
    private readonly IReportInfoRepository _reportInfoRepository;
    private readonly INESTRepository<ReportInfoIndex, Guid> _nestRepository;
    private readonly IBridgeContractAppService _bridgeContractAppService;
    private readonly IBlockchainAppService _blockchainAppService;
    private readonly IEventHandlerAppService _eventHandlerAppService;
    private readonly IReportContractAppService _reportContractAppService;
    private readonly ReportJobCategoryOptions _reportJobCategoryOptions;

    public ReportInfoAppService(IReportInfoRepository reportInfoRepository,
        INESTRepository<ReportInfoIndex, Guid> nestRepository, IBridgeContractAppService bridgeContractAppService,
        IBlockchainAppService blockchainAppService, IEventHandlerAppService eventHandlerAppService,
        IReportContractAppService reportContractAppService, IOptionsSnapshot<ReportJobCategoryOptions> reportJobCategoryOptions)
    {
        _reportInfoRepository = reportInfoRepository;
        _nestRepository = nestRepository;
        _bridgeContractAppService = bridgeContractAppService;
        _blockchainAppService = blockchainAppService;
        _eventHandlerAppService = eventHandlerAppService;
        _reportContractAppService = reportContractAppService;
        _reportJobCategoryOptions = reportJobCategoryOptions.Value;
    }

    public async Task CreateAsync(CreateReportInfoInput input)
    {
        var info = ObjectMapper.Map<CreateReportInfoInput, ReportInfo>(input);
        info.Step = ReportStep.Proposed;
        await _reportInfoRepository.InsertAsync(info);
    }

    public async Task UpdateStepAsync(string chainId, long roundId, string token, string targetChainId, ReportStep step, DateTime updateTime)
    {
        var info = await _reportInfoRepository.FindAsync(o =>
            o.ChainId == chainId && o.RoundId == roundId  && o.Token == token && o.TargetChainId == targetChainId);

        if (info == null || info.Step >= step)
        {
            Logger.LogDebug(
                $"Invalid report step. ChainId: {info?.ChainId}, RoundId: {roundId}, Step: {info?.Step}, Input Step: {step}");
            return;
        }

        info.Step = step;
        info.UpdateTime = updateTime;
        await _reportInfoRepository.UpdateAsync(info);
    }

    public async Task AddIndexAsync(AddReportInfoIndexInput input)
    {
        var index = ObjectMapper.Map<AddReportInfoIndexInput, ReportInfoIndex>(input);
        await _nestRepository.AddAsync(index);
    }

    public async Task UpdateIndexAsync(UpdateReportInfoIndexInput input)
    {
        var index = ObjectMapper.Map<UpdateReportInfoIndexInput, ReportInfoIndex>(input);
        await _nestRepository.UpdateAsync(index);
    }

    public async Task<double> CalculateCrossChainProgressAsync(string receiptId)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<ReportInfoIndex>, QueryContainer>>
        {
            q => q.Term(i => i.Field(f => f.ReceiptId).Value(receiptId)),
        };
        QueryContainer Query(QueryContainerDescriptor<ReportInfoIndex> f) => f.Bool(b => b.Must(mustQuery));
        
        var info = await _nestRepository.GetAsync(Query, sortExp: o=>o.Step, sortType: SortOrder.Descending);

        if (info == null || info.Step < ReportStep.Proposed)
        {
            return 0;
        }

        return ((int)info.Step + 1) * 100 / 3d;
    }

    public async Task UpdateStepAsync()
    {
        var q = await _reportInfoRepository.GetQueryableAsync();
        var list = await AsyncExecuter.ToListAsync(q
            .Where(o => o.Step == ReportStep.Confirmed && o.QueryTimes < CrossChainServerConsts.MaxReportQueryTimes));

        if (list.Count == 0)
        {
            return;
        }

        var chainIds = list.Select(o => o.TargetChainId).Distinct().ToList();
        
        var chainStatus = new Dictionary<string,ChainStatusDto>();
        foreach (var chainId in chainIds)
        {
            var status = await _blockchainAppService.GetChainStatusAsync(chainId);
            chainStatus.Add(chainId, status);
        }

        var toUpdateList = new List<ReportInfo>();
        foreach (var info in list)
        {
            if (info.TransmitHeight != 0 && info.TransmitHeight >= chainStatus[info.TargetChainId].ConfirmedBlockHeight)
            {
                continue;
            }
            
            var isTransmit = await _bridgeContractAppService.CheckTransmitAsync(info.TargetChainId, info.ReceiptHash);
            if (isTransmit)
            {
                if (info.TransmitHeight == 0)
                {
                    info.TransmitHeight = chainStatus[info.TargetChainId].BlockHeight;
                }
                else
                {
                    info.Step = ReportStep.Transmitted;
                }
            }
            
            info.QueryTimes += 1;
            toUpdateList.Add(info);
        }

        await _reportInfoRepository.UpdateManyAsync(toUpdateList);
    }

    public async Task ReSendQueryAsync()
    {
        var q = await _reportInfoRepository.GetQueryableAsync();
        var list = await AsyncExecuter.ToListAsync(q
            .Where(o => o.Step == ReportStep.Proposed));

        if (list.Count == 0)
        {
            return;
        }

        var toUpdateReports = new List<ReportInfo>();
        foreach (var item in list)
        {
            var latestSyncTime = await _eventHandlerAppService.GetLatestSyncTimeAsync(item.ChainId, _reportJobCategoryOptions.Mapping[item.ChainId]);
            if (item.UpdateTime > latestSyncTime.AddMinutes(-CrossChainServerConsts.ReportTimeout))
            {
                continue;
            }

            var txId = await SendQueryTransactionAsync(item);
            Logger.LogInformation($"ReSend Query, Resending Report: {item.Id}, Query Tx Id: {txId}");

            item.QueryTransactionId = txId;
            item.Step = ReportStep.Resending;
            toUpdateReports.Add(item);
        }

        if (toUpdateReports.Count > 0)
        {
            await _reportInfoRepository.UpdateManyAsync(toUpdateReports);
        }
    }

    public async Task CheckQueryTransactionAsync()
    {
        var q = await _reportInfoRepository.GetQueryableAsync();
        var list = await AsyncExecuter.ToListAsync(q
            .Where(o => o.Step == ReportStep.Resending));

        if (list.Count == 0)
        {
            return;
        }

        var toUpdateReports = new List<ReportInfo>();
        foreach (var item in list)
        {
            var txResult = await _blockchainAppService.GetTransactionResultAsync(item.ChainId, item.QueryTransactionId);
            if (txResult.IsMined)
            {
                var chainStatus = await _blockchainAppService.GetChainStatusAsync(item.ChainId);
                if (txResult.BlockHeight > chainStatus.ConfirmedBlockHeight)
                {
                    continue;
                }
                
                var block = await _blockchainAppService.GetBlockByHeightAsync(item.ChainId, txResult.BlockHeight);
                if (block.BlockHash == txResult.BlockHash)
                {
                    item.Step = ReportStep.ResendSucceeded;
                    toUpdateReports.Add(item);
                    continue;
                }
            }
            
            item.Step = ReportStep.Proposed;
            toUpdateReports.Add(item);
        }
        
        if (toUpdateReports.Count > 0)
        {
            await _reportInfoRepository.UpdateManyAsync(toUpdateReports);
        }
    }

    private async Task<string> SendQueryTransactionAsync(ReportInfo reportInfo)
    {
        return await _reportContractAppService.QueryOracleAsync(reportInfo.ChainId, reportInfo.TargetChainId,
            reportInfo.ReceiptId, reportInfo.ReceiptHash);
    }
}