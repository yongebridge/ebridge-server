using Microsoft.Extensions.Options;

namespace AElf.CrossChainServer.Chains
{
    public class EvmClientFactory : IBlockchainClientFactory<Nethereum.Web3.Web3>
    {
        private readonly ChainApiOptions _chainApiOptions;

        public EvmClientFactory(IOptionsSnapshot<ChainApiOptions> apiOptions)
        {
            _chainApiOptions = apiOptions.Value;
        }

        public Nethereum.Web3.Web3 GetClient(string chainId)
        {
            return new Nethereum.Web3.Web3(_chainApiOptions.ChainNodeApis[chainId]);
        }
    }
}