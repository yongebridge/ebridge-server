using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.CrossChainServer.Chains;
using AElf.CrossChainServer.CrossChain;
using AElf.CrossChainServer.Settings;
using AElf.CrossChainServer.Tokens;
using GraphQL;
using GraphQL.Client.Abstractions;
using Volo.Abp.Json;
using Volo.Abp.SettingManagement;

namespace AElf.CrossChainServer.Worker.IndexerSync;

public class ReportInfoIndexerSyncProvider : IndexerSyncProviderBase
{
    private readonly IReportInfoAppService _reportInfoAppService;
    private readonly IChainAppService _chainAppService;

    public ReportInfoIndexerSyncProvider(IGraphQLClient graphQlClient, ISettingManager settingManager,
        IChainAppService chainAppService,IJsonSerializer jsonSerializer,
        IReportInfoAppService reportInfoAppService) : base(
        graphQlClient, settingManager,jsonSerializer)
    {
        _chainAppService = chainAppService;
        _reportInfoAppService = reportInfoAppService;
    }

    protected override string SyncType { get; } = CrossChainServerSettings.ReportIndexerSync;

    protected override async Task<long> HandleDataAsync(string chainId, long startHeight, long endHeight)
    {
        var processedHeight = startHeight;

        var data = await QueryDataAsync<ReportInfoResponse>(GetRequest(chainId, startHeight, endHeight));
        if (data == null || data.ReportInfo.Count == 0)
        {
            return processedHeight;
        }

        foreach (var reportInfo in data.ReportInfo)
        {
            await HandleDataAsync(reportInfo);
            processedHeight = reportInfo.BlockHeight;
        }

        return processedHeight;
    }

    private async Task HandleDataAsync(ReportInfoDto report)
    {
        var chain = await _chainAppService.GetByAElfChainIdAsync(ChainHelper.ConvertBase58ToChainId(report.ChainId));

        switch (report.Step)
        {
            case ReportStep.Proposed:
                await _reportInfoAppService.CreateAsync(new CreateReportInfoInput
                {
                    ChainId = chain.Id,
                    ReceiptId = report.ReceiptId,
                    ReceiptHash = report.ReceiptHash,
                    RoundId = report.RoundId,
                    Token = report.Token,
                    TargetChainId = report.TargetChainId,
                    UpdateTime = report.BlockTime
                });
                break;
            case ReportStep.Confirmed:
                await _reportInfoAppService.UpdateStepAsync(chain.Id, report.RoundId, report.Token,
                    report.TargetChainId, ReportStep.Confirmed, report.BlockTime);
                break;
        }
    }

    private GraphQLRequest GetRequest(string chainId, long startHeight, long endHeight)
    {
        return new GraphQLRequest
        {
            Query =
                @"query($chainId:String,$startBlockHeight:Long!,$endBlockHeight:Long!,$methodNames: [String],$skipCount:Int!,$maxResultCount:Int!){
            caHolderTransactionInfo(dto: {chainId:$chainId,startBlockHeight:$startBlockHeight,endBlockHeight:$endBlockHeight, methodNames:$methodNames,skipCount:$skipCount,maxResultCount:$maxResultCount}){
                totalRecordCount,
                data{
                    blockHash,
                    blockHeight,
                    transactionId,
                    methodName,
                    transferInfo{
                        fromChainId,
                        toChainId
                    }
                }
            }
        }",
            Variables = new
            {
                chainId = chainId,
                startBlockHeight = startHeight,
                endBlockHeight = endHeight
            }
        };
    }
}

public class ReportInfoResponse
{
    public List<ReportInfoDto> ReportInfo { get; set; }
}

public class ReportInfoDto : GraphQLDto
{
    public long RoundId { get; set; }
    public string Token { get; set; }
    public string TargetChainId { get; set; }
    public string ReceiptId { get; set; }
    public string ReceiptHash { get; set; }
    public ReportStep Step { get; set; }
    public DateTime BlockTime { get; set; }
}