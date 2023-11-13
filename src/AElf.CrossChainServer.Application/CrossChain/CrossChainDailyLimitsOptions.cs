using System.Collections.Generic;

namespace AElf.CrossChainServer.CrossChain;

public class CrossChainDailyLimitsOptions
{
    
   public List<CrossChainDailyLimit> DailyLimitList { get; set; }
   
}

public class CrossChainDailyLimit
{
    public string Token { get; set; }
    
    public long Allowance { get; set; }
}