using AElf.CrossChainServer.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Caching;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.EventBus.RabbitMq;
using Volo.Abp.Modularity;
using Microsoft.Extensions.Configuration;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Volo.Abp;
using Volo.Abp.BlobStoring;
using Volo.Abp.BlobStoring.Aliyun;
using Volo.Abp.Threading;

namespace AElf.CrossChainServer.FixTool
{
    [DependsOn(
        typeof(AbpAutofacModule),
        typeof(AbpCachingStackExchangeRedisModule),
        typeof(AbpAspNetCoreSerilogModule),
        typeof(CrossChainServerEntityFrameworkCoreModule),
        typeof(AbpEventBusRabbitMqModule),
        typeof(CrossChainServerApplicationModule)
    )]
    public class CrossChainServerEntityHandlerModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();
            ConfigureCache(configuration);
            ConfigureRedis(context, configuration);
            ConfigureGraphQl(context, configuration);
            context.Services.AddHostedService<CrossChainServerHostedService>();

            context.Services.AddSingleton<TransferFixProvider>();
        }
        
        private void ConfigureBlob(IConfiguration configuration)
        {
            Configure<AbpBlobStoringOptions>(options =>
            {
                options.Containers.ConfigureDefault(container =>
                {
                    container.UseAliyun(_ =>
                    {
                    });
                });
            });
        }

        private void ConfigureCache(IConfiguration configuration)
        {
            Configure<AbpDistributedCacheOptions>(options =>
            {
                options.KeyPrefix = "CrossChainServer:";
            });
        }

        private void ConfigureRedis(
            ServiceConfigurationContext context,
            IConfiguration configuration)
        {
            var config = configuration["Redis:Configuration"];
            var redis = ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]);
            context.Services
                .AddDataProtection()
                .PersistKeysToStackExchangeRedis(redis, "CrossChainServer-Protection-Keys");
        }
        
        private void ConfigureGraphQl(ServiceConfigurationContext context,
            IConfiguration configuration)
        {
            context.Services.AddSingleton<IGraphQLClient>(new GraphQLHttpClient(configuration["GraphQL:Configuration"],
                new NewtonsoftJsonSerializer()));
        }
        
        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            var provider = context.ServiceProvider.GetRequiredService<TransferFixProvider>();
            AsyncHelper.RunSync(async () => await provider.FixAsync());
        }
    }
}