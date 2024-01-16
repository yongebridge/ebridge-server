namespace AElf.CrossChainServer.Contracts;

public class TokenBucketDto
{
    public decimal Capacity { get; set; }
    public decimal RefillRate { get; set; }
    public int MaximumTimeConsumed { get; set; }
}