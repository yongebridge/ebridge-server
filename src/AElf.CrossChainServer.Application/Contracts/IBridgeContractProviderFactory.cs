using System.Threading.Tasks;

namespace AElf.CrossChainServer.Contracts
{
    public interface IBridgeContractProviderFactory
    {
        Task<IBridgeContractProvider> GetBridgeContractProviderAsync(string chainId);
    }
}