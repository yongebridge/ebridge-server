using System.Threading.Tasks;
using AElf.CrossChainServer.Contracts;
using AElf.CrossChainServer.CrossChain;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace AElf.CrossChainServer.Worker;

public class ReportCheckWorker: AsyncPeriodicBackgroundWorkerBase
{
    private readonly IReportInfoAppService _reportInfoAppService;

    public ReportCheckWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory,
        IReportInfoAppService reportInfoAppService) : base(timer,
        serviceScopeFactory)
    {
        Timer.Period = 1000 * 60;
        
        _reportInfoAppService = reportInfoAppService;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        await _reportInfoAppService.CheckQueryTransactionAsync();
        await _reportInfoAppService.ReSendQueryAsync();
    }
}