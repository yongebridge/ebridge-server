using System;
using System.Threading.Tasks;
using AElf.CrossChainServer.Chains;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Type = System.Type;

namespace AElf.CrossChainServer.Tokens
{
    [RemoteService(IsEnabled = false)]
    public class TokenAppService : CrossChainServerAppService, ITokenAppService
    {
        private readonly ITokenRepository _tokenRepository;
        private readonly IBlockchainAppService _blockchainAppService;

        public TokenAppService( ITokenRepository tokenRepository, IBlockchainAppService blockchainAppService)
        {
            _tokenRepository = tokenRepository;
            _blockchainAppService = blockchainAppService;
        }

        public async Task<TokenDto> GetAsync(Guid id)
        {
            var token = await _tokenRepository.GetAsync(id);
            return ObjectMapper.Map<Token, TokenDto>(token);
        }

        public async Task<TokenDto> GetAsync(GetTokenInput input)
        {
            var token = await _tokenRepository.FirstOrDefaultAsync(o =>
                o.ChainId == input.ChainId && 
                (input.Address.IsNullOrWhiteSpace() || o.Address == input.Address ) && 
                (input.Symbol.IsNullOrWhiteSpace() || o.Symbol == input.Symbol));
            if (token == null)
            {
                var tokenDto = await _blockchainAppService.GetTokenInfoAsync(input.ChainId, input.Address, input.Symbol);
                if (tokenDto == null)
                {
                    Logger.LogWarning(
                        $"Cannot get token! chain: {input.ChainId}, address: {input.Address ?? string.Empty}, symbol: {input.Symbol ?? string.Empty}.");
                    throw new EntityNotFoundException("Token not exist.");
                }

                token = await _tokenRepository.InsertAsync(new Token
                {
                    Address = tokenDto.Address,
                    Decimals = tokenDto.Decimals,
                    Symbol = tokenDto.Symbol,
                    ChainId = input.ChainId
                }, autoSave: true);
            }

            return ObjectMapper.Map<Token, TokenDto>(token);
        }

        public async Task<TokenDto> CreateAsync(TokenCreateInput input)
        {
            var token = ObjectMapper.Map<TokenCreateInput, Token>(input);

            token = await _tokenRepository.InsertAsync(token, autoSave: true);
            return ObjectMapper.Map<Token, TokenDto>(token);
        }
    }
}