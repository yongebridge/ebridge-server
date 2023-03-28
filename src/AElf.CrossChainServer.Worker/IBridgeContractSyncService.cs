using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.CrossChainServer.Tokens;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace AElf.CrossChainServer.Worker;

public interface IBridgeContractSyncService
{
    Task ExecuteAsync();
}

public class BridgeContractSyncService : IBridgeContractSyncService, ITransientDependency
{
    private readonly BridgeContractSyncOptions _bridgeContractSyncOptions;
    private readonly ITokenAppService _tokenAppService;
    private readonly IEnumerable<IBridgeContractSyncProvider> _bridgeContractSyncProviders;
    
    public ILogger<BridgeContractSyncService> Logger { get; set; }

    public BridgeContractSyncService(IOptionsSnapshot<BridgeContractSyncOptions> bridgeContractSyncOptions,
        ITokenAppService tokenAppService, IEnumerable<IBridgeContractSyncProvider> bridgeContractSyncProviders)
    {
        _bridgeContractSyncOptions = bridgeContractSyncOptions.Value;
        _tokenAppService = tokenAppService;
        _bridgeContractSyncProviders = bridgeContractSyncProviders.ToList();
        
        Logger = NullLogger<BridgeContractSyncService>.Instance;
    }
    
    public async Task ExecuteAsync()
    {
        foreach (var (key, value) in _bridgeContractSyncOptions.Tokens)
        {
            try
            {
                var chainId = key;
                foreach (var (transferType, tokenInfos) in value)
                {
                    var tokenIds = new List<Guid>();
                    var targetChainIds = new List<string>();
                    foreach (var token in tokenInfos)
                    {
                        var tokenInfo = await _tokenAppService.GetAsync(new GetTokenInput
                        {
                            Address = token.Address,
                            Symbol = token.Symbol,
                            ChainId = chainId
                        });

                        tokenIds.Add(tokenInfo.Id);
                        targetChainIds.Add(token.TargetChainId);
                    }

                    var provider = _bridgeContractSyncProviders.First(o => o.Type == transferType);
                    await provider.SyncAsync(chainId, tokenIds, targetChainIds);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e,"Bridge contract sync failed, ChainId: {key}, Message: {message}", key, e.Message);
            }
        }
    }
}