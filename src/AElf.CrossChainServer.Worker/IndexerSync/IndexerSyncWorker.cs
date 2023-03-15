using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.SettingManagement;
using Volo.Abp.Threading;

namespace AElf.CrossChainServer.Worker.IndexerSync;

public class IndexerSyncWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly ISettingManager _settingManager;

    public IndexerSyncWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory,
        ISettingManager settingManager) : base(timer,
        serviceScopeFactory)
    {
        _settingManager = settingManager;
        Timer.Period = 1000 * 5;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        
    }
}