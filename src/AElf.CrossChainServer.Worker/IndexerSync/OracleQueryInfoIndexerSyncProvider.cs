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

public class OracleQueryInfoIndexerSyncProvider : IndexerSyncProviderBase
{
    private readonly IOracleQueryInfoAppService _oracleQueryInfoAppService;
    private readonly IChainAppService _chainAppService;

    public OracleQueryInfoIndexerSyncProvider(IGraphQLClient graphQlClient, ISettingManager settingManager,
        IChainAppService chainAppService,IJsonSerializer jsonSerializer,
        IOracleQueryInfoAppService oracleQueryInfoAppService) : base(
        graphQlClient, settingManager,jsonSerializer)
    {
        _chainAppService = chainAppService;
        _oracleQueryInfoAppService = oracleQueryInfoAppService;
    }

    protected override string SyncType { get; } = CrossChainServerSettings.ReportIndexerSync;

    protected override async Task<long> HandleDataAsync(string chainId, long startHeight, long endHeight)
    {
        var processedHeight = startHeight;

        var data = await QueryDataAsync<OracleQueryInfoResponse>(GetRequest(chainId, startHeight, endHeight));
        if (data == null || data.OracleQueryInfo.Count == 0)
        {
            return processedHeight;
        }

        foreach (var oracleQueryInfo in data.OracleQueryInfo)
        {
            await HandleDataAsync(oracleQueryInfo);
            processedHeight = oracleQueryInfo.BlockHeight;
        }

        return processedHeight;
    }

    private async Task HandleDataAsync(OracleQueryInfoDto data)
    {
        var chain = await _chainAppService.GetByAElfChainIdAsync(ChainHelper.ConvertBase58ToChainId(data.ChainId));

        switch (data.Step)
        {
            case OracleStep.QueryCreated:
                for (var i = data.StartIndex; i <= data.EndIndex; i++)
                {
                    await _oracleQueryInfoAppService.CreateAsync(new CreateOracleQueryInfoInput
                    {
                        Option = $"{data.ReceiptHash}.{i}",
                        Step = OracleStep.QueryCreated,
                        ChainId = chain.Id,
                        QueryId = data.QueryId,
                        UpdateTime = data.BlockTime
                    });
                }
                break;
            default:
                await _oracleQueryInfoAppService.UpdateAsync(new UpdateOracleQueryInfoInput()
                {
                    Step = data.Step,
                    ChainId = chain.Id,
                    QueryId = data.QueryId,
                    UpdateTime = data.BlockTime
                });
                break;
        }
    }

    private GraphQLRequest GetRequest(string chainId, long startHeight, long endHeight)
    {
        return new GraphQLRequest
        {
            Query =
                @"query($chainId:String,$startBlockHeight:Long!,$endBlockHeight:Long!){
            oracleQueryInfo(dto: {chainId:$chainId,startBlockHeight:$startBlockHeight,endBlockHeight:$endBlockHeight}){
                data{
                    id,
                    chainId,
                    blockHash,
                    blockHeight,
                    blockTime,
                    queryId,
                    receiptHash,
                    startIndex,
                    endIndex,
                    step
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

public class OracleQueryInfoResponse
{
    public List<OracleQueryInfoDto> OracleQueryInfo { get; set; }
}

public class OracleQueryInfoDto : GraphQLDto
{
    public string QueryId { get; set; }
    public string ReceiptHash { get; set; }
    public long StartIndex { get; set; }
    public long EndIndex { get; set; }
    public OracleStep Step { get; set; }
}