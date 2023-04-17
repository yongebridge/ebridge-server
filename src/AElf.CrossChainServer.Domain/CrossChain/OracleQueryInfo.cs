using System;
using AElf.CrossChainServer.Entities;
using Nest;

namespace AElf.CrossChainServer.CrossChain;

public class OracleQueryInfo : MultiChainEntity<Guid>
{
    [Keyword]
    public string QueryId { get; set; }
    [Keyword]
    public string Option { get; set; }
    public OracleStep Step { get; set; }
    public long LastUpdateHeight { get; set; }
}