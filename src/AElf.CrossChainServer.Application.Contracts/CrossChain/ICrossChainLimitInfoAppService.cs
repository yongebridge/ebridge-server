using System.Collections.Generic;
using System.Threading.Tasks;

namespace AElf.CrossChainServer.CrossChain;

public interface ICrossChainLimitInfoAppService
{
    /**
     * to get Daily Limits config
     */
    Task<List<CrossChainDailyLimitsDto>> GetCrossChainDailyLimitsAsync();
    /**
     *  to get Rate Limits Infos
     */
    Task<List<CrossChainRateLimitsDto>> GetCrossChainRateLimitsAsync();
}