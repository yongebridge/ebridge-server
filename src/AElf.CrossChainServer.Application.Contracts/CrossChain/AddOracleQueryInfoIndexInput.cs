using System;

namespace AElf.CrossChainServer.CrossChain;

public class AddOracleQueryInfoIndexInput
{
    public Guid Id { get; set; }
    public string ChainId { get; set; }
    public string QueryId { get; set; }
    public string Option { get; set; }
    public OracleStep Step { get; set; }
    public long LastUpdateHeight { get; set; }
}