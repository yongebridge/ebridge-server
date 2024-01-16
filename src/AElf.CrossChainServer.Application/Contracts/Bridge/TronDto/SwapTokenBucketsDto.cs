using System.Collections.Generic;
using TronNet.ABI.FunctionEncoding.Attributes;

namespace AElf.CrossChainServer.Contracts.Bridge.TronDto;

[FunctionOutput]
public class SwapTokenBucketsDto : IFunctionOutputDTO
{
    [Parameter("tuple[]", "_tokenBuckets", 1)]
    public List<SwapTokenBucketDto> SwapTokenBuckets { get; set; }
}