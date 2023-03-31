using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.CrossChainServer.Chains;
using AElf.CrossChainServer.Indexer;
using GraphQL;
using GraphQL.Client.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Json;
using Volo.Abp.SettingManagement;

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
        var syncSetting = await GetSyncSettingAsync();
        
        syncSetting.TryGetValue(chainId, out var syncHeight);
        var currentIndexHeight = await GetIndexBlockHeightAsync(chainId);
        var endHeight = Math.Min(syncHeight + MaxRequestCount, currentIndexHeight);
        var chain = await ChainAppService.GetAsync(chainId);
        var height = await HandleDataAsync(ChainHelper.ConvertChainIdToBase58(chain.AElfChainId), syncHeight+1, endHeight);
        
        syncSetting[chainId] = height;
        await SetSyncSettingAsync(syncSetting);
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

    private async Task<Dictionary<string, long>> GetSyncSettingAsync()
    {
        var setting = await SettingManager.GetOrNullGlobalAsync(SyncType);
        return setting == null
            ? new Dictionary<string, long>()
            : JsonSerializer.Deserialize<Dictionary<string, long>>(setting);
    }

    private async Task SetSyncSettingAsync(Dictionary<string,long> setting)
    {
        await SettingManager.SetGlobalAsync(SyncType, JsonSerializer.Serialize(setting));
    }

    protected abstract string SyncType { get; }

    protected abstract Task<long> HandleDataAsync(string aelfChainId, long startHeight, long endHeight);
}

