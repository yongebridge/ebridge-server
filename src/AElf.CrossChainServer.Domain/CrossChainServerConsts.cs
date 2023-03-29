namespace AElf.CrossChainServer;

public static class CrossChainServerConsts
{
    public const string DbTablePrefix = "App";

    public const string DbSchema = null;

    public const string AElfMainChainId = "MainChain_AELF";

    public const int MaxReportQueryTimes = 10;
    public const int ReportTimeout = 10; // 10 minutes
    public const int HalfOfTheProgress = 50;
    public const int FullOfTheProgress = 100;
}
