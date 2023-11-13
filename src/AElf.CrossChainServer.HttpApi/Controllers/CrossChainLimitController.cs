using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.CrossChainServer.CrossChain;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace AElf.CrossChainServer;

[RemoteService]
[Area("app")]
[ControllerName("CrossChainLimit")]
[Route("api/app/limiter")]
public class CrossChainLimitController
{
    private readonly ICrossChainLimitInfoAppService _crossChainLimitInfoAppService;

    public CrossChainLimitController(ICrossChainLimitInfoAppService crossChainLimitInfoAppService)
    {
        _crossChainLimitInfoAppService = crossChainLimitInfoAppService;
    }

    [HttpGet]
    [Route("dailyLimits")]
    public Task<List<CrossChainDailyLimitsDto>> GetCrossChainDailyLimitsAsync()
    {
        return _crossChainLimitInfoAppService.GetCrossChainDailyLimitsAsync();
    }
    
    [HttpGet]
    [Route("rateLimits")]
    public Task<List<CrossChainRateLimitsDto>> GetCrossChainRateLimitsAsync()
    {
        return _crossChainLimitInfoAppService.GetCrossChainRateLimitsAsync();
    }
}