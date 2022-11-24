using System;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using AElf.Client.Dto;
using AElf.Client.Service;
using AElf.Contracts.MultiToken;
using AElf.CrossChainServer.Tokens;
using AElf.Types;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;
using GetTokenInfoInput = AElf.Client.MultiToken.GetTokenInfoInput;

namespace AElf.CrossChainServer.Chains
{
    public class AElfClientProvider : IBlockchainClientProvider
    {
        protected readonly IBlockchainClientFactory<AElfClient> BlockchainClientFactory;
        protected const string PrivateKey = "09da44778f8db2e602fb484334f37df19e221c84c4582ce5b7770ccfbc3ddbef";

        public AElfClientProvider(IBlockchainClientFactory<AElfClient> blockchainClientFactory)
        {
            BlockchainClientFactory = blockchainClientFactory;
        }

        public BlockchainType ChainType { get; } = BlockchainType.AElf;

        public async Task<TokenDto> GetTokenAsync(string chainId, string address, string symbol)
        {
            var client = BlockchainClientFactory.GetClient(chainId);
            if (address.IsNullOrWhiteSpace())
            {
                address = (await client.GetContractAddressByNameAsync(
                    HashHelper.ComputeFrom("AElf.ContractNames.Token"))).ToBase58();
            }
            
            var token = await GetTokenInfoAsync(chainId, address, symbol);
            if (token == null)
            {
                return null;
            }

            return new TokenDto
            {
                ChainId = chainId,
                Address = address,
                Decimals = token.Decimals,
                Symbol = token.Symbol
            };
        }

        public async Task<BlockDto> GetBlockByHeightAsync(string chainId, long height, bool includeTransactions = false)
        {
            var client = BlockchainClientFactory.GetClient(chainId);
            var block = await client.GetBlockByHeightAsync(height,includeTransactions);

            return new BlockDto
            {
                BlockHash = block.BlockHash,
                Header = new BlockHeaderDto
                {
                    Height = block.Header.Height,
                    Time = block.Header.Time,
                    ChainId = block.Header.ChainId,
                    PreviousBlockHash = block.Header.PreviousBlockHash
                },
                Body = new BlockBodyDto
                {
                    Transactions = block.Body.Transactions,
                    TransactionsCount = block.Body.TransactionsCount
                }
            };
        }

        public Task<long> GetChainHeightAsync(string chainId)
        {
            throw new NotImplementedException();
        }

        public async Task<ChainStatusDto> GetChainStatusAsync(string chainId)
        {
            var client = BlockchainClientFactory.GetClient(chainId);
            var status = await client.GetChainStatusAsync();
            return new ChainStatusDto
            {
                ChainId = chainId,
                BlockHeight = status.BestChainHeight,
                ConfirmedBlockHeight = status.LastIrreversibleBlockHeight
            };
        }

        public async Task<TransactionResultDto> GetTransactionResultAsync(string chainId, string transactionId)
        {
            var client = BlockchainClientFactory.GetClient(chainId);
            var result = await client.GetTransactionResultAsync(transactionId);
            
            return new TransactionResultDto
            {
                ChainId = chainId,
                IsMined = result.Status == TransactionResultStatus.Mined.ToString().ToUpper(),
                IsFailed = result.Status != TransactionResultStatus.Mined.ToString().ToUpper() &&
                            result.Status != TransactionResultStatus.Pending.ToString().ToUpper(),
                BlockHeight = result.BlockNumber,
                BlockHash = result.BlockHash,
                Transaction = result.Transaction
            };
        }

        public async Task<AElf.Contracts.MultiToken.TokenInfo> GetTokenInfoAsync(string chainId, string address, string symbol)
        {
            var client = BlockchainClientFactory.GetClient(chainId);

            var paramGetBalance = new GetTokenInfoInput
            {
                Symbol = symbol
            };
            var transactionGetToken =
                await client.GenerateTransactionAsync(client.GetAddressFromPrivateKey(PrivateKey), address,
                    "GetTokenInfo",
                    paramGetBalance);
            var txWithSignGetToken = client.SignTransaction(PrivateKey, transactionGetToken);
            var transactionGetTokenResult = await client.ExecuteTransactionAsync(new ExecuteTransactionDto
            {
                RawTransaction = txWithSignGetToken.ToByteArray().ToHex()
            });
            var token =
                AElf.Contracts.MultiToken.TokenInfo.Parser.ParseFrom(
                    ByteArrayHelper.HexStringToByteArray(transactionGetTokenResult));

            if (token.Symbol != symbol)
            {
                return null;
            }

            return token;
        }
        
        public async Task<MerklePathDto> GetMerklePathAsync(string chainId, string txId)
        {
            var client = BlockchainClientFactory.GetClient(chainId);
            return await client.GetMerklePathByTransactionIdAsync(txId);
        }
    }
}