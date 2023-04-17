using System;

namespace AElf.CrossChainServer.Contracts;

public class ReceivedReceiptInfoDto
{
    public string ReceiptId { get; set; }

    public Guid TokenId { get; set; }
    
    public string FromAddress { get; set; }
    
    public string FromChainId { get; set; }
    
    public string ToAddress { get; set; }

    public decimal Amount { get; set; }
    
    public long BlockHeight { get; set; }
    
    public DateTime BlockTime { get; set; }
}