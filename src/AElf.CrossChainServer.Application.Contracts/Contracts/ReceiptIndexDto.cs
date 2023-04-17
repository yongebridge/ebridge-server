using System;

namespace AElf.CrossChainServer.Contracts;

public class ReceiptIndexDto
{
    public string TargetChainId { get; set; }
    public Guid TokenId { get; set; }
    public long Index { get; set; }
}