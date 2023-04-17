using System.Threading.Tasks;
using AElf.CrossChainServer.Contracts;
using AElf.CrossChainServer.CrossChain;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace AElf.CrossChainServer.Worker;

public class TransmitCheckWorker: AsyncPeriodicBackgroundWorkerBase
{
    private readonly IReportInfoAppService _reportInfoAppService;

    public TransmitCheckWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory,
        IBridgeContractAppService bridgeContractAppService, IReportInfoAppService reportInfoAppService) : base(timer,
        serviceScopeFactory)
    {
        Timer.Period = 1000 * 60;
        
        _reportInfoAppService = reportInfoAppService;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        await _reportInfoAppService.UpdateStepAsync();
    }
}