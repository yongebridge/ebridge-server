namespace AElf.CrossChainServer.CrossChain;

public class ChainDailyLimitsOptions
{
    public ChainIdInfo ChainIdInfo { get; set; }
}

public class ChainIdInfo
{
    //ChainId with high priority
    public string TokenFirstChainId { get; set; }
    
    public string ToChainId { get; set; }
}
