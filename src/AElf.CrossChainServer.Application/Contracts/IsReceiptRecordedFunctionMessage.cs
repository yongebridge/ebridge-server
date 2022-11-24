using System.Collections.Generic;
using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace AElf.CrossChainServer.Contracts;

[Function("isReceiptRecorded", "bool")]
public class IsReceiptRecordedFunctionMessage: FunctionMessage
{
    [Parameter("bytes32", "receiptHash", 1)]
    public byte[] ReceiptHash { get; set; }
}

[FunctionOutput]
public class IsReceiptRecordedDTO: IFunctionOutputDTO
{
    [Parameter("bool", "", 1)]
    public bool IsReceiptRecorded { get; set; }
}