using System.Threading.Tasks;

namespace AElf.CrossChainServer.Worker.IndexerSync;

public interface IIndexerSyncProvider
{
    Task ExecuteAsync(string chainId);
}