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

public class CrossChainIndexingInfoIndexerSyncProvider : IndexerSyncProviderBase
{
    private readonly ICrossChainIndexingInfoAppService _crossChainIndexingInfoAppService;
    private readonly IChainAppService _chainAppService;

    public CrossChainIndexingInfoIndexerSyncProvider(IGraphQLClient graphQlClient, ISettingManager settingManager,
        IChainAppService chainAppService,IJsonSerializer jsonSerializer,
        ICrossChainIndexingInfoAppService crossChainIndexingInfoAppService) : base(
        graphQlClient, settingManager,jsonSerializer)
    {
        _chainAppService = chainAppService;
        _crossChainIndexingInfoAppService = crossChainIndexingInfoAppService;
    }

    protected override string SyncType { get; } = CrossChainServerSettings.CrossChainIndexingIndexerSync;

    protected override async Task<long> HandleDataAsync(string chainId, long startHeight, long endHeight)
    {
        var processedHeight = startHeight;

        var data = await QueryDataAsync<CrossChainIndexingInfoResponse>(GetRequest(chainId, startHeight, endHeight));
        if (data == null || data.CrossChainIndexingInfoDto.Count == 0)
        {
            return processedHeight;
        }

        foreach (var indexing in data.CrossChainIndexingInfoDto)
        {
            await HandleDataAsync(indexing);
            processedHeight = indexing.BlockHeight;
        }

        return processedHeight;
    }

    private async Task HandleDataAsync(CrossChainIndexingInfoDto data)
    {
        var chain = await _chainAppService.GetByAElfChainIdAsync(ChainHelper.ConvertBase58ToChainId(data.ChainId));
        var indexChain =
            await _chainAppService.GetByAElfChainIdAsync(ChainHelper.ConvertBase58ToChainId(data.IndexChainId));
        if (indexChain == null)
        {
            return;
        }

        await _crossChainIndexingInfoAppService.CreateAsync(new CreateCrossChainIndexingInfoInput
        {
            BlockHeight = data.BlockHeight,
            BlockTime = data.BlockTime,
            ChainId = chain.Id,
            IndexChainId = indexChain.Id,
            IndexBlockHeight = data.IndexBlockHeight
        });
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

public class CrossChainIndexingInfoResponse
{
    public List<CrossChainIndexingInfoDto> CrossChainIndexingInfoDto { get; set; }
}

public class CrossChainIndexingInfoDto : GraphQLDto
{
    public DateTime BlockTime { get; set; }
    public string IndexChainId { get; set; }
    public long IndexBlockHeight { get; set; }
}