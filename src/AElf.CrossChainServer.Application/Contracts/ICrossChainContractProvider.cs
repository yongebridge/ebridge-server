using System.Threading.Tasks;
using AElf.CrossChainServer.Chains;
using AElf.Standards.ACS7;

namespace AElf.CrossChainServer.Contracts;

public interface ICrossChainContractProvider
{
    BlockchainType ChainType { get; }
    
    Task<CrossChainMerkleProofContext> GetBoundParentChainHeightAndMerklePathByHeightAsync(string chainId, string contractAddress, long height);
}