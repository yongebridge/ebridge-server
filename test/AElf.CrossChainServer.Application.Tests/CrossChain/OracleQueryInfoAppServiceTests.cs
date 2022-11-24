using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using Nest;
using Shouldly;
using Xunit;

namespace AElf.CrossChainServer.CrossChain;

public class OracleQueryInfoAppServiceTests : CrossChainServerApplicationTestBase
{
    private readonly IOracleQueryInfoAppService _oracleQueryInfoAppService;
    private readonly INESTRepository<OracleQueryInfoIndex, Guid> _oracleQueryInfoIndexRepository;
    private readonly IOracleQueryInfoRepository _oracleQueryInfoRepository;

    public OracleQueryInfoAppServiceTests()
    {
        _oracleQueryInfoAppService = GetRequiredService<IOracleQueryInfoAppService>();
        _oracleQueryInfoIndexRepository = GetRequiredService<INESTRepository<OracleQueryInfoIndex, Guid>>();
        _oracleQueryInfoRepository = GetRequiredService<IOracleQueryInfoRepository>();
    }

    [Fact]
    public async Task CreateTest()
    {
        var createInput = new CreateOracleQueryInfoInput
        {
            Option = "Option",
            Step = OracleStep.QueryCreated,
            ChainId = "MainChain_AELF",
            QueryId = "QueryId",
            UpdateTime = DateTime.UtcNow
        };
        await _oracleQueryInfoAppService.CreateAsync(createInput);

        var progress = await _oracleQueryInfoAppService.CalculateCrossChainProgressAsync(createInput.Option);
        progress.ShouldBe(20);
        
        var updateInput = new UpdateOracleQueryInfoInput
        {
            Step = OracleStep.Committed,
            ChainId = "MainChain_AELF",
            QueryId = "QueryIdNotExist",
            UpdateTime = DateTime.UtcNow
        };
        await _oracleQueryInfoAppService.UpdateAsync(updateInput);
        
        progress = await _oracleQueryInfoAppService.CalculateCrossChainProgressAsync(createInput.Option);
        progress.ShouldBe(20);

        updateInput = new UpdateOracleQueryInfoInput
        {
            Step = OracleStep.Committed,
            ChainId = "MainChain_AELF",
            QueryId = "QueryId",
            UpdateTime = DateTime.UtcNow
        };
        await _oracleQueryInfoAppService.UpdateAsync(updateInput);
        
        progress = await _oracleQueryInfoAppService.CalculateCrossChainProgressAsync(createInput.Option);
        progress.ShouldBe(40);
        
        updateInput = new UpdateOracleQueryInfoInput
        {
            Step = OracleStep.SufficientCommitmentsCollected,
            ChainId = "MainChain_AELF",
            QueryId = "QueryId",
            UpdateTime = DateTime.UtcNow
        };
        await _oracleQueryInfoAppService.UpdateAsync(updateInput);
        
        progress = await _oracleQueryInfoAppService.CalculateCrossChainProgressAsync(createInput.Option);
        progress.ShouldBe(60);
        
        updateInput = new UpdateOracleQueryInfoInput
        {
            Step = OracleStep.Committed,
            ChainId = "MainChain_AELF",
            QueryId = "QueryId",
            UpdateTime = DateTime.UtcNow
        };
        await _oracleQueryInfoAppService.UpdateAsync(updateInput);
        
        progress = await _oracleQueryInfoAppService.CalculateCrossChainProgressAsync(createInput.Option);
        progress.ShouldBe(60);
    }
    
    [Fact]
    public async Task RepeatedQueryTest()
    {
        var createInput1 = new CreateOracleQueryInfoInput
        {
            Option = "Option",
            Step = OracleStep.QueryCreated,
            ChainId = "MainChain_AELF",
            QueryId = "QueryId1",
            UpdateTime = DateTime.UtcNow.AddSeconds(-10)
        };
        await _oracleQueryInfoAppService.CreateAsync(createInput1);

        var progress = await _oracleQueryInfoAppService.CalculateCrossChainProgressAsync(createInput1.Option);
        progress.ShouldBe(20);
        
        var createInput2 = new CreateOracleQueryInfoInput
        {
            Option = "Option",
            Step = OracleStep.QueryCreated,
            ChainId = "MainChain_AELF",
            QueryId = "QueryId2",
            UpdateTime = DateTime.UtcNow.AddSeconds(-5)
        };
        await _oracleQueryInfoAppService.CreateAsync(createInput2);

        progress = await _oracleQueryInfoAppService.CalculateCrossChainProgressAsync(createInput1.Option);
        progress.ShouldBe(20);

        var updateInput = new UpdateOracleQueryInfoInput
        {
            Step = OracleStep.Committed,
            ChainId = "MainChain_AELF",
            QueryId = "QueryId2",
            UpdateTime = DateTime.UtcNow
        };
        await _oracleQueryInfoAppService.UpdateAsync(updateInput);
        
        progress = await _oracleQueryInfoAppService.CalculateCrossChainProgressAsync(createInput1.Option);
        progress.ShouldBe(40);
    }
}