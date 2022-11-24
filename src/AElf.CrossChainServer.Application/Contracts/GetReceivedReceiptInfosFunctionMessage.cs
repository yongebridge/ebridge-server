using System.Collections.Generic;
using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace AElf.CrossChainServer.Contracts;

[Function("getReceivedReceiptInfos", "tuple[]")]
public class GetReceivedReceiptInfosFunctionMessage: FunctionMessage
{
    [Parameter("address", "token", 1)]
    public string Token { get; set; }
    
    [Parameter("string", "fromChainId", 2)]
    public string FromChainId { get; set; }
    
    [Parameter("uint256", "fromIndex", 3)]
    public BigInteger FromIndex { get; set; }
    
    [Parameter("uint256", "endIndex", 4)]
    public BigInteger EndIndex { get; set; }
}

[FunctionOutput]
public class GetReceivedReceiptInfosDTO: IFunctionOutputDTO
{
    [Parameter("tuple[]", "_receipts", 1)]
    public List<ReceivedReceiptDTO> Receipts { get; set; }
}

[FunctionOutput]
public class ReceivedReceiptDTO : IFunctionOutputDTO
{
    [Parameter("address", "asset", 1)]
    public string Asset { get; set; }
    [Parameter("address", "targetAddress", 2)]
    public string TargetAddress { get; set; }
    [Parameter("uint256", "amount", 3)]
    public BigInteger Amount { get; set; }
    [Parameter("uint256", "blockHeight", 4)]
    public BigInteger BlockHeight { get; set; }
    [Parameter("uint256", "blockTime", 5)]
    public BigInteger BlockTime { get; set; }
    [Parameter("string", "fromChainId", 6)]
    public string FromChainId { get; set; }
    [Parameter("string", "receiptId", 7)]
    public string ReceiptId { get; set; }
}