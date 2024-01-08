using System;
using System.Numerics;
using System.Threading.Tasks;
using AElf.Client.Dto;
using AElf.CrossChainServer.Tokens;
using Microsoft.Extensions.Options;
using TronClient;
using TronNet.ABI;

namespace AElf.CrossChainServer.Chains
{
    public class TronClientProvider : IBlockchainClientProvider
    {
        protected readonly IBlockchainClientFactory<TronClient.TronClient> BlockchainClientFactory;
        public IOptionsSnapshot<BlockConfirmationOptions> BlockConfirmationOptions { get; set; }

        public TronClientProvider(IBlockchainClientFactory<TronClient.TronClient> blockchainClientFactory)
        {
            BlockchainClientFactory = blockchainClientFactory;
        }

        public BlockchainType ChainType { get; } = BlockchainType.Tron;

        public async Task<TokenDto> GetTokenAsync(string chainId, string address, string symbol)
        {
            var client = BlockchainClientFactory.GetClient(chainId);
            var contractHandler = client.GetContract(address);
            
            var decimals = await GetDecimals(contractHandler);
            symbol = await GetSymbol(contractHandler);

            return new TokenDto
            {
                ChainId = chainId,
                Address = address,
                Decimals = (int)decimals,
                Symbol = symbol
            };
        }

        public Task<BlockDto> GetBlockByHeightAsync(string chainId, long height, bool includeTransactions = false)
        {
            throw new NotImplementedException();
        }

        public async Task<long> GetChainHeightAsync(string chainId)
        {
            var client = BlockchainClientFactory.GetClient(chainId);
            var latestBlock = await client.GetNowBlockAsync();
            return latestBlock.block_header.raw_data.number;
        }
        
        public async Task<ChainStatusDto> GetChainStatusAsync(string chainId)
        {
            var client = BlockchainClientFactory.GetClient(chainId);
            var latestBlock = await client.GetNowBlockAsync();
            var blockNumber = latestBlock.block_header.raw_data.number;
            
            return new ChainStatusDto
            {
                ChainId = chainId,
                BlockHeight = blockNumber,
                ConfirmedBlockHeight = blockNumber - BlockConfirmationOptions.Value.ConfirmationCount[chainId]
            };
        }

        public Task<TransactionResultDto> GetTransactionResultAsync(string chainId, string transactionId)
        {
            throw new NotImplementedException();
        }

        public Task<MerklePathDto> GetMerklePathAsync(string chainId, string txId)
        {
            throw new NotImplementedException();
        }
        
        private static async Task<string> GetSymbol(IContract contractHandler)
        {
            var response = await contractHandler.CallAsync<ConstantTransactionResponse>(new TronConstantContractFunctionMessage
            {
                FunctionSelector = "symbol()"
            });
            
            var symbolInBytes = Convert.FromHexString(response.constant_result[0]);
            symbolInBytes = symbolInBytes.Slice(32);
            
            var symbolAbi = ABIType.CreateABIType("string");
            var symbol = symbolAbi.Decode<string>(symbolInBytes);
            return symbol;
        }

        private static async Task<BigInteger> GetDecimals(IContract contractHandler)
        {
            var response = await contractHandler.CallAsync<ConstantTransactionResponse>(new TronConstantContractFunctionMessage
            {
                FunctionSelector = "decimals()"
            });
            
            var decimalsAbi = ABIType.CreateABIType("uint256");
            var decimals = decimalsAbi.Decode<BigInteger>(response.constant_result[0]);
            return decimals;
        }
    }
}