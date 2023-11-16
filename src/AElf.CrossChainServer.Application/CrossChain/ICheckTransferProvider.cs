using System;
using System.Linq;
using System.Threading.Tasks;
using AElf.CrossChainServer.Chains;
using AElf.CrossChainServer.Indexer;
using AElf.CrossChainServer.Tokens;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AElf.CrossChainServer.CrossChain;

public interface ICheckTransferProvider
{
    Task<bool> CheckTransferAsync(string fromChainId, string toChainId, Guid tokenId, decimal transferAmount);
}

public class CheckTransferProvider : ICheckTransferProvider
{
    private readonly ITokenRepository _tokenRepository;
    private readonly IIndexerCrossChainLimitInfoService _indexerCrossChainLimitInfoService;
    private readonly IChainAppService _chainAppService;
    public ILogger<CheckTransferProvider> Logger { get; set; }


    public CheckTransferProvider(ITokenRepository tokenRepository,
        IIndexerCrossChainLimitInfoService indexerCrossChainLimitInfoService, IChainAppService chainAppService)
    {
        _tokenRepository = tokenRepository;
        _indexerCrossChainLimitInfoService = indexerCrossChainLimitInfoService;
        _chainAppService = chainAppService;
        Logger = NullLogger<CheckTransferProvider>.Instance;
    }

    public async Task<bool> CheckTransferAsync(string fromChainId, string toChainId, Guid tokenId,
        decimal transferAmount)
    {
        var transferToken = await _tokenRepository.GetAsync(tokenId);
        Logger.LogInformation(
            "Start to check limit. From chain:{fromChainId}, to chain:{toChainId}, token symbol:{symbol}, transfer amount:{amount}",
            fromChainId, toChainId, transferToken.Symbol, transferAmount);

        var chain = await _chainAppService.GetAsync(toChainId);
        toChainId = ChainHelper.ConvertChainIdToBase58(chain.AElfChainId);
        var limitInfo =
            (await _indexerCrossChainLimitInfoService.GetCrossChainLimitInfoIndexAsync(fromChainId, toChainId,
                transferToken.Symbol)).FirstOrDefault();
        if (limitInfo == null)
        {
            Logger.LogInformation("No limit info.");
            return true;
        }

        var rateLimit = Math.Min(limitInfo.Capacity,
            limitInfo.CurrentBucketTokenAmount +
            (DateTime.UtcNow - limitInfo.BucketUpdateTime).Seconds * limitInfo.RefillRate);
        Logger.LogInformation(
            "Limit info,daily limit:{dailyLimit},capacity:{capacity},bucketUpdateTime:{time},rate:{rate},rate limit:{limit}.",
            limitInfo.CurrentDailyLimit, limitInfo.Capacity, limitInfo.BucketUpdateTime, limitInfo.RefillRate, rateLimit);

        return transferAmount <= limitInfo.CurrentDailyLimit && transferAmount <= rateLimit;
    }
}