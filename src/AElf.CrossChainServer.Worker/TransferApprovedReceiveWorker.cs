using System.Threading.Tasks;
using AElf.CrossChainServer.CrossChain;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace AElf.CrossChainServer.Worker;

public class TransferApprovedReceiveWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly ICrossChainTransferAppService _crossChainTransferAppService;

    public TransferApprovedReceiveWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory,
        ICrossChainTransferAppService crossChainTransferAppService) : base(timer,
        serviceScopeFactory)
    {
        Timer.Period = 1000 * 180;
        
        _crossChainTransferAppService = crossChainTransferAppService;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        await _crossChainTransferAppService.UpdateTransferApprovedReceiveAsync();
    }
}