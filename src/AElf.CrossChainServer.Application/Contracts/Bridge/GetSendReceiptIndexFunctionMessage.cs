using System.Collections.Generic;
using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace AElf.CrossChainServer.Contracts.Bridge;

[Function("getSendReceiptIndex", "tuple[]")]
public class GetSendReceiptIndexFunctionMessage: FunctionMessage
{
    [Parameter("address[]", "tokens", 1)]
    public List<string> Tokens { get; set; }
    
    [Parameter("string[]", "targetChainIds", 2)]
    public List<string> TargetChainIds { get; set; }
}

[FunctionOutput]
public class GetSendReceiptIndexDto: IFunctionOutputDTO
{
    [Parameter("uint256[]", "indexes", 1)]
    public List<BigInteger> Indexes { get; set; }
}