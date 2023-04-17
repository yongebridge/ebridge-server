using System;
using AElf.CrossChainServer.Entities;

namespace AElf.CrossChainServer.BridgeContract;

public class BridgeContractSyncInfo: MultiChainEntity<Guid>
{
    public string TargetChainId { get; set; }
    public Guid TokenId { get; set; }
    public TransferType Type { get; set; }
    public long SyncIndex { get; set; }
}