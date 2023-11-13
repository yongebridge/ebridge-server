using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.CrossChainServer.CrossChain;
using GraphQL;
using Microsoft.Extensions.Logging;

namespace AElf.CrossChainServer.Indexer;

public class IndexerCrossChainLimitInfoService : CrossChainServerAppService, IIndexerCrossChainLimitInfoService
{
    private readonly ILogger<IndexerCrossChainLimitInfoService> _logger;

    private readonly IGraphQLHelper _graphQlHelper;

    public IndexerCrossChainLimitInfoService(IGraphQLClientFactory graphQLClientFactory,
        ILogger<IndexerCrossChainLimitInfoService> logger)
    {
        _logger = logger;
        _graphQlHelper =
            new GraphQLHelper(graphQLClientFactory.GetClient(GraphQLClientEnum.CrossChainClient.ToString()), _logger);
    }

    public async Task<List<IndexerCrossChainLimitInfo>> GetAllCrossChainLimitInfoIndexAsync()
    {
        return await GetCrossChainLimitInfoIndexAsync(null, null, null);
    }

    public async Task<List<IndexerCrossChainLimitInfo>> GetCrossChainLimitInfoIndexAsync(string fromChainId, string toChainId, string symbol)
    {
        var skipCount = 0;
        var crossChainLimitInfos = new List<IndexerCrossChainLimitInfo>();
        List<IndexerCrossChainLimitInfo> crossChainLimitInfosTemp;
        do
        {
            _logger.LogInformation(
                "To get cross chain limit infos skipCount:{skipCount} toChainId:{toChainId} symbol:{symbol}", skipCount, toChainId, symbol);
            var indexerCrossChainLimitInfos =
                await QueryCrossChainLimitInfoIndexAsync(skipCount, fromChainId, toChainId, symbol);
            if (indexerCrossChainLimitInfos?.DataList.IsNullOrEmpty() != false)
            {
                break;
            }

            skipCount += indexerCrossChainLimitInfos.DataList.Count;
            crossChainLimitInfosTemp = indexerCrossChainLimitInfos.DataList;
            crossChainLimitInfos.AddRange(crossChainLimitInfosTemp);
        } while (!crossChainLimitInfosTemp.IsNullOrEmpty());

        return crossChainLimitInfos;
    }
    
    private async Task<IndexerCrossChainLimitInfos> QueryCrossChainLimitInfoIndexAsync(int skipCount,
        string fromChainId, string toChainId, string symbol)
    {
        var indexerCommonResult = await _graphQlHelper.QueryAsync<IndexerCrossChainLimitInfos>(new GraphQLRequest
        {
            Query = @"
			    query($skipCount:Int!,$fromChainId:String,$toChainId:String,$symbol:String) {
                    data:queryCrossChainLimitInfos(dto:{skipCount: $skipCount,fromChainId: $fromChainId,toChainId: $toChainId,symbol: $symbol}){
                        totalRecordCount,
                        dataList:data {
                                            id,
                                            fromChainId,
                                            toChainId,
                                            symbol,
                                            defaultDailyLimit,
                                            refreshTime,
                                            currentDailyLimit,
                                            capacity,
                                            refillRate,
                                            IsEnable,
                                            currentBucketTokenAmount,
                                            bucketUpdateTime
                                   
                                 }
                    }
                }",
            Variables = new
            {
                skipCount,
                fromChainId,
                toChainId,
                symbol
            }
        });
        return indexerCommonResult?.Data;
    }
}