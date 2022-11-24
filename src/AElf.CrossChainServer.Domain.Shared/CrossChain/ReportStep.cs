namespace AElf.CrossChainServer.CrossChain;

public enum ReportStep
{
    ResendSucceeded = -2,
    Resending = -1,
    Proposed = 0,
    Confirmed = 1,
    Transmitted = 2,
}