using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using AElf.CrossChainServer.Chains;
using AElf.CrossChainServer.Tokens;
using Nethereum.Util;
using TronClient;

namespace AElf.CrossChainServer.Contracts.Bridge;

public class TronBridgeContractProvider : TronClientProvider,IBridgeContractProvider
{
    private readonly ITokenAppService _tokenAppService;

    public TronBridgeContractProvider(IBlockchainClientFactory<TronClient.TronClient> blockchainClientFactory,
        ITokenAppService tokenAppService) : base(
        blockchainClientFactory)
    {
        _tokenAppService = tokenAppService;
    }

    public async Task<List<ReceiptInfoDto>> GetSendReceiptInfosAsync(string chainId, string contractAddress, string targetChainId, Guid tokenId,
        long fromIndex, long endIndex)
    {
        var token = await _tokenAppService.GetAsync(tokenId);
        var tronClient = BlockchainClientFactory.GetClient(chainId);
        var contractHandler = tronClient.GetContract(contractAddress);
        
        var tronGetReceiptInfos = await contractHandler.CallAsync<GetReceiptInfosFunctionMessage, TronDto.GetReceiptInfosDto>(new TronConstantContractFunctionMessage<GetReceiptInfosFunctionMessage>
        {
            FunctionMessage = new GetReceiptInfosFunctionMessage
            {
                Token = token.Address,
                TargetChainId = targetChainId,
                FromIndex = fromIndex,
                EndIndex = endIndex
            },
            Visible = true
        });

        var result = new List<ReceiptInfoDto>();
        foreach (var receipt in tronGetReceiptInfos.Receipts)
        {
            var receiptInfo = new ReceiptInfoDto();
            receiptInfo.ReceiptId = receipt.ReceiptId;
            receiptInfo.TokenId = tokenId;
            receiptInfo.FromAddress = receipt.Owner;
            receiptInfo.ToChainId = receipt.TargetChainId;
            receiptInfo.ToAddress = receipt.TargetAddress;
            receiptInfo.Amount = (decimal)((BigDecimal)receipt.Amount / BigInteger.Pow(10, token.Decimals));
            receiptInfo.BlockHeight = (long)receipt.BlockHeight;
            receiptInfo.BlockTime = DateTimeHelper.FromUnixTimeMilliseconds((long)receipt.BlockTime * 1000);

            result.Add(receiptInfo);
        }

        return result;
    }

    public async Task<List<ReceivedReceiptInfoDto>> GetReceivedReceiptInfosAsync(string chainId, string contractAddress, string fromChainId, Guid tokenId,
        long fromIndex, long endIndex)
    {
        var token = await _tokenAppService.GetAsync(tokenId);
        var tronClient = BlockchainClientFactory.GetClient(chainId);
        var contractHandler = tronClient.GetContract(contractAddress);
        
        var tronGetReceiptInfos = await contractHandler.CallAsync<GetReceivedReceiptInfosFunctionMessage, TronDto.GetReceivedReceiptInfosDto>(new TronConstantContractFunctionMessage<GetReceivedReceiptInfosFunctionMessage>
        {
            FunctionMessage = new GetReceivedReceiptInfosFunctionMessage
            {
                Token = token.Address,
                FromChainId = fromChainId,
                FromIndex = fromIndex,
                EndIndex = endIndex
            },
            Visible = true
        });

        var result = new List<ReceivedReceiptInfoDto>();
        foreach (var receipt in tronGetReceiptInfos.Receipts)
        {
            var receiptInfo = new ReceivedReceiptInfoDto();
            receiptInfo.ReceiptId = receipt.ReceiptId;
            receiptInfo.TokenId = tokenId;
            //receiptInfo.FromAddress = receipt.Owner;
            receiptInfo.FromChainId = receipt.FromChainId;
            receiptInfo.ToAddress = receipt.TargetAddress;
            receiptInfo.Amount = (decimal)((BigDecimal)receipt.Amount / BigInteger.Pow(10, token.Decimals));
            receiptInfo.BlockHeight = (long)receipt.BlockHeight;
            receiptInfo.BlockTime = DateTimeHelper.FromUnixTimeMilliseconds((long)receipt.BlockTime * 1000);

            result.Add(receiptInfo);
        }

        return result;
    }

    public async Task<List<ReceiptIndexDto>> GetTransferReceiptIndexAsync(string chainId, string contractAddress,
        List<Guid> tokenIds, List<string> targetChainIds)
    {
        var tokenAddress = new List<string>();
        foreach (var tokenId in tokenIds)
        {
            var token = await _tokenAppService.GetAsync(tokenId);
            tokenAddress.Add(token.Address);
        }

        var tronClient = BlockchainClientFactory.GetClient(chainId);
        var contractHandler = tronClient.GetContract(contractAddress);

        var indexes = await contractHandler.CallAsync<GetSendReceiptIndexFunctionMessage, TronDto.GetSendReceiptIndexDto>(new TronConstantContractFunctionMessage<GetSendReceiptIndexFunctionMessage>
        {
            FunctionMessage = new GetSendReceiptIndexFunctionMessage
            {
                Tokens = tokenAddress,
                TargetChainIds = targetChainIds
            },
            Visible = true
        });

        return indexes.Indexes.Select((t, i) => new ReceiptIndexDto
        {
            TargetChainId = targetChainIds[i],
            TokenId = tokenIds[i], 
            Index = (long)t
        }).ToList();
    }

    public async Task<List<ReceiptIndexDto>> GetReceiveReceiptIndexAsync(string chainId, string contractAddress, List<Guid> tokenIds, List<string> fromChainIds)
    {
        var tokenAddress = new List<string>();
        foreach (var tokenId in tokenIds)
        {
            var token = await _tokenAppService.GetAsync(tokenId);
            tokenAddress.Add(token.Address);
        }

        var tronClient = BlockchainClientFactory.GetClient(chainId);
        var contractHandler = tronClient.GetContract(contractAddress);
        
        var indexes = await contractHandler.CallAsync<GetReceiveReceiptIndexFunctionMessage, TronDto.GetReceiveReceiptIndexDto>(new TronConstantContractFunctionMessage<GetReceiveReceiptIndexFunctionMessage>
        {
            FunctionMessage = new GetReceiveReceiptIndexFunctionMessage
            {
                Tokens = tokenAddress,
                FromChainIds = fromChainIds
            },
            Visible = true
        });

        return indexes.Indexes.Select((t, i) => new ReceiptIndexDto
        {
            TargetChainId = fromChainIds[i],
            TokenId = tokenIds[i], 
            Index = (long)t
        }).ToList();
    }

    public async Task<bool> CheckTransmitAsync(string chainId, string contractAddress, string receiptHash)
    {
        var tronClient = BlockchainClientFactory.GetClient(chainId);
        var contractHandler = tronClient.GetContract(contractAddress);
        var isTransmit = await contractHandler.CallAsync<IsReceiptRecordedFunctionMessage, TronDto.IsReceiptRecordedDto>(new TronConstantContractFunctionMessage<IsReceiptRecordedFunctionMessage>
        {
            FunctionMessage = new IsReceiptRecordedFunctionMessage
            {
                ReceiptHash = ByteArrayHelper.HexStringToByteArray(receiptHash)
            },
            Visible = true
        });

        return isTransmit.IsReceiptRecorded;
    }

    public Task<string> GetSwapIdByTokenAsync(string chainId, string contractAddress, string fromChainId, string symbol)
    {
        throw new NotImplementedException();
    }

    public Task<string> SwapTokenAsync(string chainId, string contractAddress, string privateKey, string swapId, string receiptId, string originAmount,
        string receiverAddress)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsTransferCanReceiveAsync(string chainId, string contractAddress, string symbol, string amount)
    {
        throw new NotImplementedException();
    }
}