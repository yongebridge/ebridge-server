using Microsoft.Extensions.Options;

namespace AElf.CrossChainServer.Chains
{
    public class TronClientFactory : IBlockchainClientFactory<TronClient.TronClient>
    {
        private readonly ChainApiOptions _chainApiOptions;
        private readonly ChainExplorerApiOptions _chainExplorerApiOptions;

        public TronClientFactory(IOptionsSnapshot<ChainApiOptions> apiOptions, IOptionsSnapshot<ChainExplorerApiOptions> chainExplorerApiOptions)
        {
            _chainApiOptions = apiOptions.Value;
            _chainExplorerApiOptions = chainExplorerApiOptions.Value;
        }

        public TronClient.TronClient GetClient(string chainId)
        {
            return new TronClient.TronClient(_chainApiOptions.ChainNodeApis[chainId], _chainExplorerApiOptions.ApiKeys[chainId]);
        }
    }
}