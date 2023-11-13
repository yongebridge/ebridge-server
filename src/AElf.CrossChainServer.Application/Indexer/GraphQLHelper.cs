using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Client.Abstractions;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace AElf.CrossChainServer.Indexer;

public interface IGraphQLHelper
{
    Task<T> QueryAsync<T>(GraphQLRequest request);
}

public class GraphQLHelper : IGraphQLHelper
{
    private readonly IGraphQLClient _graphQLClient;
    private readonly ILogger _logger;

    public GraphQLHelper(IGraphQLClient graphQLClient, ILogger logger)
    {
        _graphQLClient = graphQLClient;
        _logger = logger;
    }

    public async Task<T> QueryAsync<T>(GraphQLRequest request)
    {
        var graphQlResponse = await _graphQLClient.SendQueryAsync<T>(request);
        if (graphQlResponse.Errors is not { Length: > 0 })
        {
            return graphQlResponse.Data;
        }

        _logger.LogError("query graphQL err, errors = {Errors}", string.Join(",", graphQlResponse.Errors.Select(e => e.Message).ToList()));
        return default;
    }
}