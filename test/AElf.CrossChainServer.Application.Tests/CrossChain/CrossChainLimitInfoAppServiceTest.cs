using System;
using System.Collections.Generic;
using AElf.CrossChainServer.Chains;
using AElf.CrossChainServer.Contracts;
using AElf.CrossChainServer.Indexer;
using AElf.CrossChainServer.Tokens;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace AElf.CrossChainServer.CrossChain;

public class CrossChainLimitInfoAppServiceTest
{
    private readonly ILogger<CrossChainLimitInfoAppService> _mockLogger;
    private readonly CrossChainLimitInfoAppService _service;
    private readonly IObjectMapper _mockObjectMapper;
    private readonly IIndexerCrossChainLimitInfoService _mockIndexerCrossChainLimitInfoService;
    private readonly IBridgeContractAppService _mockBridgeContractAppService;
    private readonly IOptionsMonitor<EvmTokensOptions> _mockEvmTokensOptions;
    private readonly ITokenAppService _mockTokenAppService;
    private readonly IChainAppService _mockChainAppService;

    public CrossChainLimitInfoAppServiceTest()
    {
        _mockLogger = Substitute.For<ILogger<CrossChainLimitInfoAppService>>();
        _mockObjectMapper = Substitute.For<IObjectMapper>();
        _mockIndexerCrossChainLimitInfoService = Substitute.For<IIndexerCrossChainLimitInfoService>();
        _mockBridgeContractAppService = Substitute.For<IBridgeContractAppService>();
        _mockEvmTokensOptions = Substitute.For<IOptionsMonitor<EvmTokensOptions>>();
        _mockTokenAppService = Substitute.For<ITokenAppService>();
        _mockChainAppService = Substitute.For<IChainAppService>();

        _service = new CrossChainLimitInfoAppService(
            _mockLogger,
            _mockIndexerCrossChainLimitInfoService,
            _mockBridgeContractAppService,
            _mockEvmTokensOptions,
            _mockTokenAppService,
            _mockChainAppService
        );
    }

    [Fact]
    public async void GetCrossChainDailyLimitsAsyncTest()
    {
        // Arrange
        var expectedDtoList = new List<CrossChainDailyLimitsDto>
        {
            new() { Token = "ELF", Allowance = 1099 },
            new() { Token = "BTC", Allowance = 12 },
        }; 
        // Act
        var result = await _service.GetCrossChainDailyLimitsAsync();

        Assert.Equal(expectedDtoList.Count, result.Items.Count);
    }


    [Fact]
    public async void GetCrossChainRateLimitsAsyncTest()
    {
        // Arrange
        var crossChainLimitInfos = MockIndexerCrossChainLimitInfos();
        _mockIndexerCrossChainLimitInfoService.GetAllCrossChainLimitInfoIndexAsync()
            .Returns(MockIndexerCrossChainLimitInfos());

        _mockEvmTokensOptions.CurrentValue.Returns(MockEvmTokensOptions());

        MockChainAppService();

        MockTokenAppService();

        MockBridgeContractAppService();

        // Act
        var result = await _service.GetCrossChainRateLimitsAsync();

        // Assert
        Assert.Equal(crossChainLimitInfos.Count, result.Items.Count * 2);
    }

    private EvmTokensOptions MockEvmTokensOptions()
    {
        var evmTokensOptions = new EvmTokensOptions
        {
            Tokens = new Dictionary<string, List<TokenInfo>>
            {
                {
                    "Evm", new List<TokenInfo>
                    {
                        new TokenInfo { Address = "0xTokenAddress1", TargetChainId = "AElf" },
                        new TokenInfo { Address = "0xTokenAddress2", TargetChainId = "AElf" }
                    }
                },
                {
                    "AElf", new List<TokenInfo>
                    {
                        new TokenInfo { Address = "0xTokenAddress3", TargetChainId = "Evm" },
                        new TokenInfo { Address = "0xTokenAddress4", TargetChainId = "Evm" }
                    }
                }
            }
        };
        return evmTokensOptions;
    }

    private List<IndexerCrossChainLimitInfo> MockIndexerCrossChainLimitInfos()
    {
        var crossChainLimitInfos = new List<IndexerCrossChainLimitInfo>();
        for (int i = 0; i < 4; i++)
        {
            string _from = "";
            string _to = "";
            string _symbol = "";
            var _limitType = CrossChainLimitType.Receipt;
            if (i == 0)
            {
                _from = "AELF";
                _to = "Evm";
                _symbol = "ELF";
            }
            else if (i == 1)
            {
                _from = "AELF";
                _to = "Evm";
                _symbol = "BTC";
            }
            else if (i == 2)
            {
                _from = "Evm";
                _to = "AELF";
                _symbol = "ELF";
                _limitType = CrossChainLimitType.Swap;
            }
            else if (i == 3)
            {
                _from = "Evm";
                _to = "AELF";
                _symbol = "BTC";
                _limitType = CrossChainLimitType.Swap;
            }

            var one = new IndexerCrossChainLimitInfo
            {
                FromChainId = _from,
                ToChainId = _to,
                Symbol = _symbol,
                LimitType = _limitType,
                DefaultDailyLimit = 10000000 + i,
                RefreshTime = DateTime.UtcNow,
                CurrentDailyLimit = 8000000 + i,
                Capacity = 5000000 + i,
                RefillRate = 100000 + i,
                IsEnable = true,
                CurrentBucketTokenAmount = 2500000 + i,
                BucketUpdateTime = DateTime.UtcNow
            };
            one.Id = one.FromChainId + "-" + one.ToChainId + "-" + one.Symbol;
            crossChainLimitInfos.Add(one);
        }

        return crossChainLimitInfos;
    }

    private void MockChainAppService()
    {
        _mockChainAppService.GetAsync(Arg.Is<string>(s => s == "AElf"))
            .Returns(new ChainDto
            {
                Type = BlockchainType.AElf
            });

        _mockChainAppService.GetAsync(Arg.Is<string>(s => s == "Evm"))
            .Returns(new ChainDto
            {
                Type = BlockchainType.Evm
            });

        _mockChainAppService.GetByAElfChainIdAsync(Arg.Any<int>())
            .Returns(new ChainDto()
            {
                Type = BlockchainType.AElf,
                Id = "AElf"
            });
    }

    private void MockTokenAppService()
    {
        _mockTokenAppService
            .GetAsync(Arg.Is<GetTokenInput>(input => "AElf,Evm".Contains(input.ChainId) && input.Symbol == "ELF"))
            .Returns(new TokenDto
            {
                Symbol = "ELF", Id = new Guid()
            });

        _mockTokenAppService
            .GetAsync(Arg.Is<GetTokenInput>(input => "AElf,Evm".Contains(input.ChainId) && input.Symbol == "BTC"))
            .Returns(new TokenDto
            {
                Symbol = "BTC", Id = new Guid()
            });
    }

    private void MockBridgeContractAppService()
    {
        _mockBridgeContractAppService
            .GetCurrentReceiptTokenBucketStatesAsync(Arg.Is("Evm"), Arg.Any<List<Guid>>(), Arg.Any<List<string>>())
            .Returns(new List<TokenBucketDto>()
            {
                new()
                {
                    Capacity = 121,
                    RefillRate = 102,
                    MaximumTimeConsumed = 3
                }
            });

        _mockBridgeContractAppService
            .GetCurrentSwapTokenBucketStatesAsync(Arg.Is("AElf"), Arg.Any<List<Guid>>(), Arg.Any<List<string>>())
            .Returns(new List<TokenBucketDto>()
            {
                new()
                {
                    Capacity = 122,
                    RefillRate = 103,
                    MaximumTimeConsumed = 4
                }
            });
    }
}