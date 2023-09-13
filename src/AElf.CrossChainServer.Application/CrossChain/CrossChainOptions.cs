namespace AElf.CrossChainServer.CrossChain;

public class CrossChainOptions
{
    public long ReportTimeoutHeightThreshold { get; set; } = CrossChainServerConsts.DefaultReportTimeoutHeightThreshold;
    public long MaxReportResendTimes { get; set; } = CrossChainServerConsts.DefaultMaxReportResendTimes;
}