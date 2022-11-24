using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Modularity;
using Volo.Abp.Threading;

namespace AElf.CrossChainServer.Worker
{
    [DependsOn(
        typeof(AbpBackgroundWorkersModule))]
    public class CrossChainServerWorkerModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();
            Configure<BridgeContractSyncOptions>(configuration.GetSection("BridgeContractSync"));
                        
            context.Services.AddTransient<IBridgeContractSyncProvider, BridgeContractTransferSyncProvider>();
            context.Services.AddTransient<IBridgeContractSyncProvider, BridgeContractReceiveSyncProvider>();
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            context.AddBackgroundWorkerAsync<TransferProgressUpdateWorker>();
            context.AddBackgroundWorkerAsync<CrossChainIndexingCleanWorker>();
            context.AddBackgroundWorkerAsync<BridgeContractSyncWorker>();
            context.AddBackgroundWorkerAsync<TransmitCheckWorker>();
            context.AddBackgroundWorkerAsync<ReportCheckWorker>();
            context.AddBackgroundWorkerAsync<TransferAutoReceiveWorker>();
        }
    }
}