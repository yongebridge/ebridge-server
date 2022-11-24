using System;
using AElf.CrossChainServer.BridgeContract;

namespace AElf.CrossChainServer.Contracts;

public class BridgeContractSyncInfoDto
{
    public string ChainId { get; set; }
    public Guid TokenId { get; set; }
    public TransferType Type { get; set; }
    public long SyncIndex { get; set; }
}