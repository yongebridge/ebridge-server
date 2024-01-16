using System.Collections.Generic;
using TronNet.ABI.FunctionEncoding.Attributes;

namespace AElf.CrossChainServer.Contracts.Bridge.TronDto;

[FunctionOutput]
public class ReceiptTokenBucketsDto : IFunctionOutputDTO
{
    [Parameter("tuple[]", "_tokenBuckets", 1)]
    public List<ReceiptTokenBucketDto> TokenBuckets { get; set; }
}