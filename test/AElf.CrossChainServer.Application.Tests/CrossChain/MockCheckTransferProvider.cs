using System;
using System.Threading.Tasks;

namespace AElf.CrossChainServer.CrossChain;

public class MockCheckTransferProvider : ICheckTransferProvider
{
    public async Task<bool> CheckTransferAsync(string chainId, Guid tokenId, decimal transferAmount)
    {
        return true;
    }
}