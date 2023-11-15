using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.CrossChainServer.Chains;
using AElf.CrossChainServer.Contracts;
using AElf.CrossChainServer.Indexer;
using AElf.CrossChainServer.Tokens;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Application.Dtos;

namespace AElf.CrossChainServer.CrossChain;

[RemoteService(IsEnabled = false)]
public class CrossChainLimitInfoAppService : CrossChainServerAppService, ICrossChainLimitInfoAppService
{
    private readonly ILogger<CrossChainLimitInfoAppService> _logger;
    private readonly IIndexerCrossChainLimitInfoService _indexerCrossChainLimitInfoService;
    private readonly IBridgeContractAppService _bridgeContractAppService;
    private readonly IOptionsMonitor<EvmTokensOptions> _evmTokensOptions;
    private readonly ITokenAppService _tokenAppService;
    private readonly IChainAppService _chainAppService;


    public CrossChainLimitInfoAppService(
        ILogger<CrossChainLimitInfoAppService> logger,
        IIndexerCrossChainLimitInfoService indexerCrossChainLimitInfoService,
        IBridgeContractAppService bridgeContractAppService,
        IOptionsMonitor<EvmTokensOptions> evmTokensOptions, ITokenAppService tokenAppService,
        IChainAppService chainAppService)
    {
        _logger = logger;
        _indexerCrossChainLimitInfoService = indexerCrossChainLimitInfoService;
        _bridgeContractAppService = bridgeContractAppService;
        _evmTokensOptions = evmTokensOptions;
        _tokenAppService = tokenAppService;
        _chainAppService = chainAppService;
    }

    public async Task<ListResultDto<CrossChainDailyLimitsDto>> GetCrossChainDailyLimitsAsync()
    {
        var indexerCrossChainLimitInfos =
            await _indexerCrossChainLimitInfoService.GetAllCrossChainLimitInfoIndexAsync();

        var dailyLimits = new Dictionary<string, CrossChainDailyLimitsDto>();
        string convertedToChainId = null;
        foreach (var info in indexerCrossChainLimitInfos)
        {
            if (info.ToChainId != BlockchainType.AElf.ToString().ToUpper()
                || dailyLimits.ContainsKey(info.Symbol))
            {
                //avoid repeated get
                continue;
            }

            if (convertedToChainId.IsNullOrEmpty())
            {
                var chain = await _chainAppService.GetByAElfChainIdAsync(
                    ChainHelper.ConvertBase58ToChainId(info.ToChainId));
                convertedToChainId = chain.Id;
            }

            var token = await _tokenAppService.GetAsync(new GetTokenInput
            {
                ChainId = convertedToChainId,
                Symbol = info.Symbol
            });
            var limitsDto = new CrossChainDailyLimitsDto
            {
                Token = info.Symbol,
                Allowance = info.DefaultDailyLimit / (decimal)Math.Pow(10, token.Decimals)
            };
            dailyLimits.Add(info.Symbol, limitsDto);
        }

        return new ListResultDto<CrossChainDailyLimitsDto>
        {
            Items = dailyLimits.Values.ToList()
        };
    }
    
    public async Task<ListResultDto<CrossChainRateLimitsDto>> GetCrossChainRateLimitsAsync()
    {
        var result = new List<CrossChainRateLimitsDto>();
        var crossChainLimitInfos = await GetCrossChainLimitInfosAsync();
        var evmLimitInfos = await GetEvmRateLimitInfosAsync();
        foreach (var crossChainLimitInfo in crossChainLimitInfos)
        {
            var chain = await _chainAppService.GetAsync(crossChainLimitInfo.Key.FromChainId);
            if (chain.Type == BlockchainType.AElf)
            {
                _logger.LogInformation("Limit data processing，From chain:{fromChainId}, to chain:{toChainId}",
                    crossChainLimitInfo.Key.FromChainId, crossChainLimitInfo.Key.ToChainId);
                var receiptRateLimits =
                    await OfRateLimitInfos(crossChainLimitInfo.Value, crossChainLimitInfo.Key.FromChainId);
                var swapRateLimits = new List<RateLimitInfo>();
                if (evmLimitInfos.TryGetValue(crossChainLimitInfo.Key, out var value))
                {
                    swapRateLimits = OfEvmRateLimitInfos(value);
                }

                result.Add(new CrossChainRateLimitsDto
                {
                    FromChain = crossChainLimitInfo.Key.FromChainId,
                    ToChain = crossChainLimitInfo.Key.ToChainId,
                    ReceiptRateLimitsInfo = receiptRateLimits,
                    SwapRateLimitsInfo = swapRateLimits
                });
            }
            else
            {
                _logger.LogInformation("Limit data processing，From chain:{fromChainId}, to chain:{toChainId}",
                    crossChainLimitInfo.Key.FromChainId, crossChainLimitInfo.Key.ToChainId);
                var receiptRateLimits = new List<RateLimitInfo>();
                if (evmLimitInfos.TryGetValue(crossChainLimitInfo.Key, out var value))
                {
                    receiptRateLimits = OfEvmRateLimitInfos(value);
                }

                var swapRateLimits =
                    await OfRateLimitInfos(crossChainLimitInfo.Value, crossChainLimitInfo.Key.ToChainId);
                result.Add(new CrossChainRateLimitsDto
                {
                    FromChain = crossChainLimitInfo.Key.FromChainId,
                    ToChain = crossChainLimitInfo.Key.ToChainId,
                    ReceiptRateLimitsInfo = receiptRateLimits,
                    SwapRateLimitsInfo = swapRateLimits
                });
            }
        }

        return new ListResultDto<CrossChainRateLimitsDto>
        {
            Items = result
        };
    }

    private async Task<Dictionary<CrossChainLimitKey, Dictionary<string, IndexerCrossChainLimitInfo>>>
        GetCrossChainLimitInfosAsync()
    {
        var crossChainLimitInfoDictionary =
            new Dictionary<CrossChainLimitKey, Dictionary<string, IndexerCrossChainLimitInfo>>();
        var indexerCrossChainLimitInfos =
            await _indexerCrossChainLimitInfoService.GetAllCrossChainLimitInfoIndexAsync();
        foreach (var item in indexerCrossChainLimitInfos)
        {
            _logger.LogInformation(
                "Start to get limit info. From chain:{fromChainId}, to chain:{toChainId}, symbol:{symbol}",
                item.FromChainId, item.ToChainId, item.Symbol);
            if (item.LimitType == CrossChainLimitType.Receipt)
            {
                var chain = await _chainAppService.GetByAElfChainIdAsync(
                    ChainHelper.ConvertBase58ToChainId(item.FromChainId));
                item.FromChainId = chain.Id;
            }
            else
            {
                var chain = await _chainAppService.GetByAElfChainIdAsync(
                    ChainHelper.ConvertBase58ToChainId(item.ToChainId));
                item.ToChainId = chain.Id;
            }

            var key = new CrossChainLimitKey
            {
                FromChainId = item.FromChainId,
                ToChainId = item.ToChainId
            };
            if (!crossChainLimitInfoDictionary.TryGetValue(key, out var tokenDictionary))
            {
                tokenDictionary = new Dictionary<string, IndexerCrossChainLimitInfo>();
                crossChainLimitInfoDictionary[key] = tokenDictionary;
            }

            tokenDictionary[item.Symbol] = item;
        }

        return crossChainLimitInfoDictionary;
    }

    private async Task<List<RateLimitInfo>> OfRateLimitInfos(
        Dictionary<string, IndexerCrossChainLimitInfo> tokenDictionary, string chainId)
    {
        var result = new List<RateLimitInfo>();
        foreach (var pair in tokenDictionary)
        {
            var token = await _tokenAppService.GetAsync(new GetTokenInput
            {
                ChainId = chainId,
                Symbol = pair.Key
            });
            var capacity = (decimal)pair.Value.Capacity;
            var refillRate = (decimal)pair.Value.RefillRate;
            var time = 0;
            if (capacity != 0 && refillRate != 0)
            {
                capacity = capacity / (decimal)Math.Pow(10, token.Decimals);
                refillRate = refillRate / (decimal)Math.Pow(10, token.Decimals);
                time = (int)Math.Ceiling(capacity / refillRate / 60);
                
            }
            result.Add(new RateLimitInfo
            {
                Token = pair.Key,
                Capacity = capacity,
                RefillRate = refillRate,
                MaximumTimeConsumed = time
            });
        }

        return result;
    }

    private static List<RateLimitInfo> OfEvmRateLimitInfos(
        Dictionary<string, TokenBucketDto> tokenDictionary)
    {
        return tokenDictionary.Select(pair => new RateLimitInfo
        {
            Token = pair.Key,
            Capacity = pair.Value.Capacity,
            RefillRate = pair.Value.RefillRate,
            MaximumTimeConsumed = pair.Value.MaximumTimeConsumed
        }).ToList();
    }

    private async Task<Dictionary<CrossChainLimitKey, Dictionary<string, TokenBucketDto>>> GetEvmRateLimitInfosAsync()
    {
        var result = new Dictionary<CrossChainLimitKey, Dictionary<string, TokenBucketDto>>();
        foreach (var (chainId, tokenInfos) in _evmTokensOptions.CurrentValue.Tokens)
        {
            try
            {
                var targetChainIds = tokenInfos.Select(t => t.TargetChainId).ToList();
                var tokenIds = new List<Guid>();
                var tokenSymbols = new List<string>();
                foreach (var token in tokenInfos)
                {
                    var tokenInfo = await _tokenAppService.GetAsync(new GetTokenInput
                    {
                        Address = token.Address,
                        ChainId = chainId
                    });
                    tokenIds.Add(tokenInfo.Id);
                    tokenSymbols.Add(tokenInfo.Symbol);
                }

                _logger.LogInformation(
                    "Start to get receipt limit info. From chain:{fromChainId}, to chain list:{toChainId}, symbol list:{symbol}",
                    chainId, targetChainIds, tokenSymbols);
                var receiptRateLimits =
                    await GetEvmReceiptRateLimitsAsync(chainId, targetChainIds, tokenIds,
                        tokenSymbols);
                ConcatRateLimits(ref result, receiptRateLimits);
                _logger.LogInformation(
                    "Start to get swap limit info. From chain list:{fromChainId}, to chain:{toChainId}, symbol:{symbol}",
                    targetChainIds, chainId, tokenSymbols);
                var swapRateLimits =
                    await GetEvmSwapRateLimitsAsync(targetChainIds, chainId, tokenIds,
                        tokenSymbols);
                ConcatRateLimits(ref result, swapRateLimits);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Get evm receipt rate limits failed, ChainId: {key}, Message: {message}", chainId,
                    e.Message);
            }
        }

        return result;
    }

    private async Task<Dictionary<CrossChainLimitKey, Dictionary<string, TokenBucketDto>>> GetEvmReceiptRateLimitsAsync(
        string chainId, List<string> targetChainIds, List<Guid> tokenIds, List<string> symbols)
    {
        var result = new Dictionary<CrossChainLimitKey, Dictionary<string, TokenBucketDto>>();
        try
        {
            var receiptTokenBucketDto = await _bridgeContractAppService.GetCurrentReceiptTokenBucketStatesAsync(chainId,
                tokenIds, targetChainIds);
            for (var i = 0; i < receiptTokenBucketDto.Count; i++)
            {
                var limitKey = new CrossChainLimitKey
                {
                    FromChainId = chainId,
                    ToChainId = targetChainIds[i]
                };
                var tokenDictionary = new Dictionary<string, TokenBucketDto>
                {
                    [symbols[i]] = receiptTokenBucketDto[i]
                };
                if (result.ContainsKey(limitKey))
                {
                    result[limitKey] = result[limitKey].Concat(tokenDictionary).ToDictionary(k => k.Key, v => v.Value);
                }
                else
                {
                    result[limitKey] = tokenDictionary;
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Get evm receipt rate limits failed, FromChainId: {key}, TargetChainId list:{targetChainIds}, Token list:{symbols}, Message: {message}",
                chainId, targetChainIds, symbols, e.Message);
        }

        return result;
    }

    private async Task<Dictionary<CrossChainLimitKey, Dictionary<string, TokenBucketDto>>> GetEvmSwapRateLimitsAsync(
        List<string> fromChainIds, string toChainId, List<Guid> tokenIds, List<string> symbols)
    {
        var result = new Dictionary<CrossChainLimitKey, Dictionary<string, TokenBucketDto>>();
        try
        {
            var swapTokenBucketDto = await _bridgeContractAppService.GetCurrentSwapTokenBucketStatesAsync(toChainId,
                tokenIds, fromChainIds);
            for (var i = 0; i < swapTokenBucketDto.Count; i++)
            {
                var limitKey = new CrossChainLimitKey
                {
                    FromChainId = fromChainIds[i],
                    ToChainId = toChainId
                };
                var tokenDictionary = new Dictionary<string, TokenBucketDto>
                {
                    [symbols[i]] = swapTokenBucketDto[i]
                };
                if (result.ContainsKey(limitKey))
                {
                    result[limitKey] = result[limitKey].Concat(tokenDictionary).ToDictionary(k => k.Key, v => v.Value);
                }
                else
                {
                    result[limitKey] = tokenDictionary;
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Get evm swap rate limits failed,FromChainIds:{fromChainIds}, TargetChainId:{targetChainId}, Token list:{symbol}, Message: {message}",
                fromChainIds, toChainId, symbols, e.Message);
        }

        return result;
    }

    private static void ConcatRateLimits(ref Dictionary<CrossChainLimitKey, Dictionary<string, TokenBucketDto>> result,
        Dictionary<CrossChainLimitKey, Dictionary<string, TokenBucketDto>> rateLimits)
    {
        foreach (var pair in rateLimits)
        {
            if (result.ContainsKey(pair.Key))
            {
                result[pair.Key] = result[pair.Key].Concat(pair.Value).ToDictionary(k => k.Key, v => v.Value);
            }
            else
            {
                result[pair.Key] = pair.Value;
            }
        }
    }
}

public class CrossChainLimitKey
{
    public string FromChainId { get; set; }

    public string ToChainId { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is not CrossChainLimitKey p)
        {
            return false;
        }

        return FromChainId == p.FromChainId && ToChainId == p.ToChainId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(FromChainId, ToChainId);
    }
}