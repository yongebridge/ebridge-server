using System;
using Volo.Abp.Domain.Entities;

namespace AElf.CrossChainServer.CrossChain;

public class CrossChainTransfer : CrossChainTransferBase
{
    public Guid TransferTokenId { get; set; }
    public Guid ReceiveTokenId { get; set; }
}