using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.CrossChainServer.Chains;
using AElf.CrossChainServer.Indexer;
using AElf.CrossChainServer.Settings;
using GraphQL;
using GraphQL.Client.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Json;

namespace AElf.CrossChainServer.Worker.IndexerSync;

public abstract class IndexerSyncProviderBase : IIndexerSyncProvider, ITransientDependency
{
    protected readonly IGraphQLClient GraphQlClient;
    protected readonly ISettingManager SettingManager;
    protected readonly IJsonSerializer JsonSerializer;
    protected readonly IIndexerAppService IndexerAppService;
    protected readonly IChainAppService ChainAppService;

    public ILogger<IndexerSyncProviderBase> Logger { get; set; }

    private const int MaxRequestCount = 1000;
    private const int SyncDelayLimit = 100;

    protected IndexerSyncProviderBase(IGraphQLClient graphQlClient, ISettingManager settingManager,
        IJsonSerializer jsonSerializer, IIndexerAppService indexerAppService, IChainAppService chainAppService)
    {
        GraphQlClient = graphQlClient;
        SettingManager = settingManager;
        JsonSerializer = jsonSerializer;
        IndexerAppService = indexerAppService;
        ChainAppService = chainAppService;
        Logger = NullLogger<IndexerSyncProviderBase>.Instance;
    }

    public async Task ExecuteAsync(string chainId)
    {
        var syncHeight = await GetSyncHeightAsync(chainId);
        var currentIndexHeight = await GetIndexBlockHeightAsync(chainId);
        var endHeight = Math.Min(syncHeight + MaxRequestCount, currentIndexHeight- SyncDelayLimit);
        var chain = await ChainAppService.GetAsync(chainId);
        var height = await HandleDataAsync(ChainHelper.ConvertChainIdToBase58(chain.AElfChainId), syncHeight+1, endHeight);
        
        await SetSyncHeightAsync(chainId, height);
    }

    protected async Task<T> QueryDataAsync<T>(GraphQLRequest request)
    {
        var data = await GraphQlClient.SendQueryAsync<T>(request);
        if (data.Errors == null || data.Errors.Length == 0)
        {
            return data.Data;
        }

        Logger.LogError("Query indexer failed. errors: {Errors}",
            string.Join(",", data.Errors.Select(e => e.Message).ToList()));
        throw new Exception("Query indexer failed. ");
    }

    private async Task<long> GetIndexBlockHeightAsync(string chainId)
    {
        return await IndexerAppService.GetLatestIndexHeightAsync(chainId);
    }

    private async Task<long> GetSyncHeightAsync(string chainId)
    {
        var setting = await SettingManager.GetOrNullAsync(chainId, SyncType);
        return setting == null ? 0 : long.Parse(setting);
    }

    private async Task SetSyncHeightAsync(string chainId, long height)
    {
        await SettingManager.SetAsync(chainId, SyncType, height.ToString());
    }
    
    

    protected abstract string SyncType { get; }

    protected abstract Task<long> HandleDataAsync(string aelfChainId, long startHeight, long endHeight);
}

