using System.Threading.Tasks;
using AElf.CrossChainServer.Chains;
using AElf.Types;

namespace AElf.CrossChainServer.Contracts;

public interface ITokenContractProvider
{
    BlockchainType ChainType { get; }

    Task<string> CrossChainReceiveTokenAsync(string chainId, string contractAddress, string privateKey, int fromChainId, long parentChainHeight,
        string transferTransaction, MerklePath merklePath);
}