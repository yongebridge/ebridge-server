using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace AElf.CrossChainServer.CrossChain;

public class ReportInfoAppServiceTests : CrossChainServerApplicationTestBase
{
    private readonly IReportInfoAppService _reportInfoAppService;
    private readonly IReportInfoRepository _reportInfoRepository;

    public ReportInfoAppServiceTests()
    {
        _reportInfoAppService = GetRequiredService<IReportInfoAppService>();
        _reportInfoRepository = GetRequiredService<IReportInfoRepository>();
    }

    [Fact]
    public async Task CreateTest()
    {
        var input = new CreateReportInfoInput
        {
            ChainId = "MainChain_AELF",
            Token = "Token",
            ReceiptHash = "ReceiptHash",
            ReceiptId = "ReceiptId",
            RoundId = 1,
            UpdateTime = DateTime.UtcNow,
            TargetChainId = "SideChain_tDVV"
        };
        await _reportInfoAppService.CreateAsync(input);

        var reports = await _reportInfoRepository.GetListAsync();
        reports.Count.ShouldBe(1);
        reports[0].ChainId.ShouldBe(input.ChainId);
        reports[0].Token.ShouldBe(input.Token);
        reports[0].ReceiptHash.ShouldBe(input.ReceiptHash);
        reports[0].ReceiptId.ShouldBe(input.ReceiptId);
        reports[0].RoundId.ShouldBe(input.RoundId);
        reports[0].UpdateTime.ShouldBe(input.UpdateTime);
        reports[0].TargetChainId.ShouldBe(input.TargetChainId);
        reports[0].Step.ShouldBe(ReportStep.Proposed);

        var newDate = DateTime.UtcNow;
        await _reportInfoAppService.UpdateStepAsync("MainChain_AELF",1, "Token1", "SideChain_tDVV", ReportStep.Confirmed,  newDate);
        
        reports = await _reportInfoRepository.GetListAsync();
        reports.Count.ShouldBe(1);
        reports[0].UpdateTime.ShouldBe(input.UpdateTime);
        reports[0].Step.ShouldBe(ReportStep.Proposed);
        
        await _reportInfoAppService.UpdateStepAsync("MainChain_AELF",1, "Token", "SideChain_tDVV", ReportStep.Confirmed,  newDate);
        
        reports = await _reportInfoRepository.GetListAsync();
        reports.Count.ShouldBe(1);
        reports[0].UpdateTime.ShouldBe(newDate);
        reports[0].Step.ShouldBe(ReportStep.Confirmed);
        
        var newDate2 = DateTime.UtcNow;
        await _reportInfoAppService.UpdateStepAsync("MainChain_AELF",1, "Token", "SideChain_tDVV", ReportStep.Proposed,  newDate2);
        
        reports = await _reportInfoRepository.GetListAsync();
        reports.Count.ShouldBe(1);
        reports[0].UpdateTime.ShouldBe(newDate);
        reports[0].Step.ShouldBe(ReportStep.Confirmed);
    }
}