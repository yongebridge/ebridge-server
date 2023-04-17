using System;
using System.Collections.Generic;

namespace AElf.CrossChainServer.CrossChain;

public class GetCrossChainTransferStatusInput
{
    public List<Guid> Ids { get; set; } = new();
}