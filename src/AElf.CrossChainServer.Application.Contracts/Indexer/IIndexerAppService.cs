
using System.Threading.Tasks;

namespace AElf.CrossChainServer.Indexer;

public interface IIndexerAppService
{
    Task<long> GetLatestIndexHeightAsync(string chainId);
}