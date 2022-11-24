using System.Threading.Tasks;
using AElf.Standards.ACS7;
using Microsoft.Extensions.Options;

namespace AElf.CrossChainServer.Contracts;

public class CrossChainContractAppService : CrossChainServerAppService, ICrossChainContractAppService
{
    private readonly ICrossChainContractProvider _crossChainContractProvider;
    private readonly AccountOptions _accountOptions;
    private readonly CrossChainContractOptions _crossChainContractOptions;

    public CrossChainContractAppService(
        IOptionsSnapshot<AccountOptions> accountOptions,
        IOptionsSnapshot<CrossChainContractOptions> crossChainContractOptions,
        ICrossChainContractProvider aelfCrossChainContractProvider)
    {
        _crossChainContractProvider = aelfCrossChainContractProvider;
        _accountOptions = accountOptions.Value;
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