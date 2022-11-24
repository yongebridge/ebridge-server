using System.Collections.Concurrent;
using AElf.Client.Service;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace AElf.CrossChainServer.Chains
{
    public class EvmClientFactory : IBlockchainClientFactory<Nethereum.Web3.Web3>
    {
        private readonly ChainApiOptions _chainApiOptions;
        private readonly ConcurrentDictionary<string, Nethereum.Web3.Web3> _clientDic;

        public EvmClientFactory(IOptionsSnapshot<ChainApiOptions> apiOptions)
        {
            _chainApiOptions = apiOptions.Value;
            _clientDic = new ConcurrentDictionary<string, Nethereum.Web3.Web3>();
        }

        public Nethereum.Web3.Web3 GetClient(string chainId)
        {
            return new Nethereum.Web3.Web3(_chainApiOptions.ChainNodeApis[chainId]);
            // if (_clientDic.TryGetValue(chainId, out var client))
            // {
            //     return client;
            // }
            //
            // client = new Nethereum.Web3.Web3(_chainApiOptions.ChainNodeApis[chainId]);
            // _clientDic[chainId] = client;
            // return client;
        }
    }
}