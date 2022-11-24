using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace AElf.CrossChainServer.Tokens;

public class TokenAppServiceTests: CrossChainServerApplicationTestBase
{
    private readonly ITokenAppService _tokenAppService;

    public TokenAppServiceTests()
    {
        _tokenAppService = GetRequiredService<ITokenAppService>();
    }

    [Fact]
    public async Task CreateTest()
    {
        var tokenInput = new TokenCreateInput
        {
            Address = "Address",
            Decimals = 8,
            Symbol = "Symbol",
            ChainId = "MainChain_AELF"
        };

        var result = await _tokenAppService.CreateAsync(tokenInput);

        var token = await _tokenAppService.GetAsync(result.Id);
        token.Address.ShouldBe(tokenInput.Address);
        token.Decimals.ShouldBe(tokenInput.Decimals);
        token.Symbol.ShouldBe(tokenInput.Symbol);
        token.ChainId.ShouldBe(tokenInput.ChainId);
    }
}