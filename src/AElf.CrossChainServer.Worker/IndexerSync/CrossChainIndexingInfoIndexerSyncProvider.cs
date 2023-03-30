using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.CrossChainServer.Chains;
using AElf.CrossChainServer.CrossChain;
using AElf.CrossChainServer.Indexer;
using AElf.CrossChainServer.Settings;
using GraphQL;
using GraphQL.Client.Abstractions;
using Volo.Abp.Json;
using Volo.Abp.SettingManagement;

namespace AElf.CrossChainServer.Worker.IndexerSync;

public class CrossChainIndexingInfoIndexerSyncProvider : IndexerSyncProviderBase
{
    private readonly ICrossChainIndexingInfoAppService _crossChainIndexingInfoAppService;

    public CrossChainIndexingInfoIndexerSyncProvider(IGraphQLClient graphQlClient, ISettingManager settingManager,
        IChainAppService chainAppService,IJsonSerializer jsonSerializer,IIndexerAppService indexerAppService,
        ICrossChainIndexingInfoAppService crossChainIndexingInfoAppService) : base(
        graphQlClient, settingManager,jsonSerializer,indexerAppService, chainAppService)
    {
        _crossChainIndexingInfoAppService = crossChainIndexingInfoAppService;
    }

    protected override string SyncType { get; } = CrossChainServerSettings.CrossChainIndexingIndexerSync;

    protected override async Task<long> HandleDataAsync(string aelfChainId, long startHeight, long endHeight)
    {
        var processedHeight = startHeight;

        var data = await QueryDataAsync<CrossChainIndexingInfoResponse>(GetRequest(aelfChainId, startHeight, endHeight));
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
        var chain = await ChainAppService.GetByAElfChainIdAsync(ChainHelper.ConvertBase58ToChainId(data.ChainId));
        var indexChain =
            await ChainAppService.GetByAElfChainIdAsync(ChainHelper.ConvertBase58ToChainId(data.IndexChainId));
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
                @"query($chainId:String,$startBlockHeight:Long!,$endBlockHeight:Long!){
            oracleQueryInfo(dto: {chainId:$chainId,startBlockHeight:$startBlockHeight,endBlockHeight:$endBlockHeight}){
                data{
                    id,
                    chainId,
                    blockHash,
                    blockHeight,
                    blockTime,
                    indexChainId,
                    indexBlockHeight
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
    public string IndexChainId { get; set; }
    public long IndexBlockHeight { get; set; }
}