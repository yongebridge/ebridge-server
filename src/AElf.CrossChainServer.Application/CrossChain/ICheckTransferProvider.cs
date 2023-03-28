using System;
using System.Numerics;
using System.Threading.Tasks;
using AElf.CrossChainServer.Chains;
using AElf.CrossChainServer.Contracts;
using AElf.CrossChainServer.Tokens;
using Nethereum.Util;

namespace AElf.CrossChainServer.CrossChain;

public interface ICheckTransferProvider
{
    Task<bool> CheckTransferAsync(string chainId, Guid tokenId,decimal transferAmount);
}

public class CheckTransferProvider : ICheckTransferProvider
{
    private readonly ITokenRepository _tokenRepository;
    private readonly IBridgeContractAppService _bridgeContractAppService;
    private readonly IChainAppService _chainAppService;

    public CheckTransferProvider(ITokenRepository tokenRepository, 
        IBridgeContractAppService bridgeContractAppService, IChainAppService chainAppService)
    {
        _tokenRepository = tokenRepository;
        _bridgeContractAppService = bridgeContractAppService;
        _chainAppService = chainAppService;
    }
    
    public async Task<bool> CheckTransferAsync(string chainId, Guid tokenId, decimal transferAmount)
    {
        var chain = await _chainAppService.GetAsync(chainId);
        if (chain.Type != BlockchainType.AElf)
        {
            return true;
        }

        var transferToken = await _tokenRepository.GetAsync(tokenId);
        var amount = (new BigDecimal(transferAmount)) * BigInteger.Pow(10, transferToken.Decimals); 
        return
            await _bridgeContractAppService.IsTransferCanReceiveAsync(chainId, transferToken.Symbol, amount.ToString());

    }
}