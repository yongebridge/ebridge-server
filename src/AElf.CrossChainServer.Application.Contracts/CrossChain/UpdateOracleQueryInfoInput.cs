using System;

namespace AElf.CrossChainServer.CrossChain;

public class UpdateOracleQueryInfoInput
{
    public string ChainId { get; set; }
    public string QueryId { get; set; }
    public OracleStep Step { get; set; }
    public long LastUpdateHeight { get; set; }
}