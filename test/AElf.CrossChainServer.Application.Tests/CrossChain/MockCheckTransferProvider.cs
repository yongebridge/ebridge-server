using System;
using System.Threading.Tasks;

namespace AElf.CrossChainServer.CrossChain;

public class MockCheckTransferProvider : ICheckTransferProvider
{
    public async Task<bool> CheckTransferAsync(string fromChainId, string toChainId, Guid tokenId, decimal transferAmount)
    {
        return true;
    }
}