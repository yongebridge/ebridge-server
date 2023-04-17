using System.Threading.Tasks;
using AElf.CrossChainServer.Indexer;

namespace AElf.CrossChainServer;

public class MockIndexerAppService: CrossChainServerAppService, IIndexerAppService
{
    public async Task<long> GetLatestIndexHeightAsync(string chainId)
    {
        return 100;
    }
}