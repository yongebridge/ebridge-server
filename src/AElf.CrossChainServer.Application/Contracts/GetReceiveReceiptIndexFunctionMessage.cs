using System.Collections.Generic;
using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace AElf.CrossChainServer.Contracts;

[Function("getReceiveReceiptIndex", "tuple[]")]
public class GetReceiveReceiptIndexFunctionMessage: FunctionMessage
{
    [Parameter("address[]", "tokens", 1)]
    public List<string> Tokens { get; set; }
    
    [Parameter("string[]", "fromChainIds", 2)]
    public List<string> FromChainIds { get; set; }
}

[FunctionOutput]
public class GetReceiveReceiptIndexDTO: IFunctionOutputDTO
{
    [Parameter("uint256[]", "indexes", 1)]
    public List<BigInteger> Indexes { get; set; }
}