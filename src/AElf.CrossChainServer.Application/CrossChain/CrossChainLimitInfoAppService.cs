using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.CrossChainServer.Indexer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.ObjectMapping;

namespace AElf.CrossChainServer.CrossChain;

[RemoteService(IsEnabled = false)]
public class CrossChainLimitInfoAppService : CrossChainServerAppService, ICrossChainLimitInfoAppService
{
    private readonly ILogger<CrossChainLimitInfoAppService> _logger;
    private readonly IOptionsMonitor<CrossChainDailyLimitsOptions> _chainDailyLimitsOptionsMonitor;
    private readonly IObjectMapper _objectMapper;
    private readonly IIndexerCrossChainLimitInfoService _indexerCrossChainLimitInfoService;
    
    public CrossChainLimitInfoAppService(ILogger<CrossChainLimitInfoAppService> logger,
        IOptionsMonitor<CrossChainDailyLimitsOptions> chainDailyLimitsOptionsMonitor,
        IObjectMapper objectMapper,
        IIndexerCrossChainLimitInfoService indexerCrossChainLimitInfoService)
    {
        _logger = logger;
        _chainDailyLimitsOptionsMonitor = chainDailyLimitsOptionsMonitor;
        _objectMapper = objectMapper;
        _indexerCrossChainLimitInfoService = indexerCrossChainLimitInfoService;
    }

    public async Task<List<CrossChainDailyLimitsDto>> GetCrossChainDailyLimitsAsync()
    {
        var dailyLimits = _chainDailyLimitsOptionsMonitor.CurrentValue.DailyLimitList;
        return _objectMapper.Map<List<CrossChainDailyLimit>, List<CrossChainDailyLimitsDto>>(dailyLimits);
    }

    public async Task<List<CrossChainRateLimitsDto>> GetCrossChainRateLimitsAsync()
    {
        var crossChainLimitInfos = await GetCrossChainLimitInfosAsync();
        var result = crossChainLimitInfos.Select(pair => new CrossChainRateLimitsDto
        {
            FromChain = pair.Key.FromChainId,
            ToChain = pair.Key.ToChainId,
            receiptRateLimitsInfo = OfReceiptRateLimitInfos(pair.Value),
            swapRateLimitsInfo = OfSwapRateLimitInfos(pair.Value)
        }).ToList();
        //TODO add query eth chain data
        return result;
    }

    private async Task<Dictionary<CrossChainLimitKey, Dictionary<string, IndexerCrossChainLimitInfo>>> GetCrossChainLimitInfosAsync()
    {
        var crossChainLimitInfoDictionary = new Dictionary<CrossChainLimitKey, Dictionary<string, IndexerCrossChainLimitInfo>>();
        var indexerCrossChainLimitInfos = await _indexerCrossChainLimitInfoService.GetAllCrossChainLimitInfoIndexAsync();
        foreach (var item in indexerCrossChainLimitInfos)
        {
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

    private static List<ReceiptRateLimitInfo> OfReceiptRateLimitInfos(
        Dictionary<string, IndexerCrossChainLimitInfo> tokenDictionary)
    {
        return tokenDictionary.Select(pair => new ReceiptRateLimitInfo
        {
            Token = pair.Key,
            Capacity = pair.Value.Capacity,
            RefillRate = pair.Value.RefillRate
        }).ToList();
    }

    private static List<SwapRateLimitInfo> OfSwapRateLimitInfos(
        Dictionary<string, IndexerCrossChainLimitInfo> tokenDictionary)
    {
        return tokenDictionary.Select(pair => new SwapRateLimitInfo
        {
            Token = pair.Key,
            Capacity = pair.Value.Capacity,
            RefillRate = pair.Value.RefillRate
        }).ToList();
    }
}

public class CrossChainLimitKey
{
    public string FromChainId { get; set; }

    public string ToChainId { get; set; }
}