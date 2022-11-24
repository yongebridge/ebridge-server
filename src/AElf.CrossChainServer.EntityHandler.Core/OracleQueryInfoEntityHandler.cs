using System.Threading.Tasks;
using AElf.CrossChainServer.CrossChain;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.ObjectMapping;

namespace AElf.CrossChainServer.EntityHandler.Core
{
    public class OracleQueryInfoEntityHandler : ITransientDependency,
        IDistributedEventHandler<EntityCreatedEto<OracleQueryInfoEto>>,
        IDistributedEventHandler<EntityUpdatedEto<OracleQueryInfoEto>>
    {
        private readonly IOracleQueryInfoAppService _oracleQueryInfoAppService;
        private readonly IObjectMapper _objectMapper;

        public OracleQueryInfoEntityHandler(IObjectMapper objectMapper, IOracleQueryInfoAppService oracleQueryInfoAppService)
        {
            _objectMapper = objectMapper;
            _oracleQueryInfoAppService = oracleQueryInfoAppService;
        }

        public async Task HandleEventAsync(EntityCreatedEto<OracleQueryInfoEto> eventData)
        {
            var input = _objectMapper.Map<OracleQueryInfoEto, AddOracleQueryInfoIndexInput>(eventData.Entity);
            await _oracleQueryInfoAppService.AddIndexAsync(input);
        }

        public async Task HandleEventAsync(EntityUpdatedEto<OracleQueryInfoEto> eventData)
        {
            var input = _objectMapper.Map<OracleQueryInfoEto, UpdateOracleQueryInfoIndexInput>(eventData.Entity);
            await _oracleQueryInfoAppService.UpdateIndexAsync(input);
        }
    }
}