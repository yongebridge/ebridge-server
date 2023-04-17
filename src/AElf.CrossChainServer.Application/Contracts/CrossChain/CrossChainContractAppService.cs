using System.Threading.Tasks;
using AElf.Standards.ACS7;
using Microsoft.Extensions.Options;

namespace AElf.CrossChainServer.Contracts.CrossChain;

public class CrossChainContractAppService : CrossChainServerAppService, ICrossChainContractAppService
{
    private readonly ICrossChainContractProvider _crossChainContractProvider;
    private readonly CrossChainContractOptions _crossChainContractOptions;

    public CrossChainContractAppService(
        IOptionsSnapshot<CrossChainContractOptions> crossChainContractOptions,
        ICrossChainContractProvider aelfCrossChainContractProvider)
    {
        _crossChainContractProvider = aelfCrossChainContractProvider;
        _crossChainContractOptions = crossChainContractOptions.Value;
    }

    public async Task<CrossChainMerkleProofContext> GetBoundParentChainHeightAndMerklePathByHeightAsync(string chainId,
        long height)
    {
        var contractAddress = _crossChainContractOptions.ContractAddresses[chainId];

        return await _crossChainContractProvider.GetBoundParentChainHeightAndMerklePathByHeightAsync(chainId,
            contractAddress, height);
    }
}