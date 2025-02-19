using System.Linq;
using System.Threading.Tasks;
using AElf.CrossChainServer.Chains;
using AElf.Indexing.Elasticsearch.Options;
using GraphQL;
using GraphQL.Client.Abstractions;
using Microsoft.Extensions.Logging;
using Volo.Abp;

namespace AElf.CrossChainServer.Indexer;

[RemoteService(IsEnabled = false)]
public class IndexerAppService: CrossChainServerAppService, IIndexerAppService
{
    private readonly IGraphQLClient _graphQlClient;
    private readonly IChainAppService _chainAppService;

    public IndexerAppService(IGraphQLClient graphQlClient, IChainAppService chainAppService)
    {
        _graphQlClient = graphQlClient;
        _chainAppService = chainAppService;
    }

    public async Task<long> GetLatestIndexHeightAsync(string chainId)
    {
        var chain = await _chainAppService.GetAsync(chainId);
        var data = await QueryDataAsync<ConfirmedBlockHeight>(new GraphQLRequest
        {
            Query = @"
			    query($chainId:String,$filterType:BlockFilterType!) {
                    syncState(dto: {chainId:$chainId,filterType:$filterType}){
                        confirmedBlockHeight}
                    }",
            Variables = new
            {
                chainId = ChainHelper.ConvertChainIdToBase58(chain.AElfChainId),
                filterType = BlockFilterType.LOG_EVENT
            }
        });

        return data.SyncState.ConfirmedBlockHeight;
    }
    
    private async Task<T> QueryDataAsync<T>(GraphQLRequest request)
    {
        var data = await _graphQlClient.SendQueryAsync<T>(request);
        if (data.Errors == null || data.Errors.Length == 0)
        {
            return data.Data;
        }

        Logger.LogError("Query indexer failed. errors: {Errors}",
            string.Join(",", data.Errors.Select(e => e.Message).ToList()));
        return default;
    }
}

public class ConfirmedBlockHeight
{
    public SyncState SyncState { get; set; }
}

public class SyncState
{
    public long ConfirmedBlockHeight { get; set; }
}

public enum BlockFilterType
{
    BLOCK,
    TRANSACTION,
    LOG_EVENT
}