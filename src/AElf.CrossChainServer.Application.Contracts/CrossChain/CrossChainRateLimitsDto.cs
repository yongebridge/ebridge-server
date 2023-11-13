using System.Collections.Generic;

namespace AElf.CrossChainServer.CrossChain;

public class CrossChainRateLimitsDto
{
    public string FromChain { get; set; }
    public string ToChain { get; set; }
    public List<ReceiptRateLimitInfo> receiptRateLimitsInfo { get; set; }
    public List<SwapRateLimitInfo> swapRateLimitsInfo { get; set; }
}

public class ReceiptRateLimitInfo
{
    public string Token { get; set; }
    public long Capacity { get; set; }
    public long RefillRate { get; set; }
    
    public int MaximumTimeConsumed { get; set; }
}

public class SwapRateLimitInfo
{
    public string Token { get; set; }
    public long Capacity { get; set; }
    public long RefillRate { get; set; }
    public int MaximumTimeConsumed { get; set; }
}

