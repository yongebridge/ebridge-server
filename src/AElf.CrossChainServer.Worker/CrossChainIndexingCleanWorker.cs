using System;
using System.Threading.Tasks;
using AElf.CrossChainServer.CrossChain;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace AElf.CrossChainServer.Worker;

public class CrossChainIndexingCleanWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly ICrossChainIndexingInfoAppService _crossChainIndexingInfoAppService;

    public CrossChainIndexingCleanWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory,
        ICrossChainIndexingInfoAppService crossChainIndexingInfoAppService) : base(timer, serviceScopeFactory)
    {
        _crossChainIndexingInfoAppService = crossChainIndexingInfoAppService;
        Timer.Period = 1000 * 60 * 60;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        await _crossChainIndexingInfoAppService.CleanAsync(DateTime.UtcNow.AddDays(-7));
    }
}