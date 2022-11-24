using System.Threading.Tasks;
using AElf.CrossChainServer.CrossChain;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.ObjectMapping;

namespace AElf.CrossChainServer.EntityHandler.Core
{
    public class ReportInfoEntityHandler : ITransientDependency,
        IDistributedEventHandler<EntityCreatedEto<ReportInfoEto>>,
        IDistributedEventHandler<EntityUpdatedEto<ReportInfoEto>>
    {
        private readonly IReportInfoAppService _reportInfoAppService;
        private readonly IObjectMapper _objectMapper;

        public ReportInfoEntityHandler(IObjectMapper objectMapper, IReportInfoAppService reportInfoAppService)
        {
            _objectMapper = objectMapper;
            _reportInfoAppService = reportInfoAppService;
        }

        public async Task HandleEventAsync(EntityCreatedEto<ReportInfoEto> eventData)
        {
            var input = _objectMapper.Map<ReportInfoEto, AddReportInfoIndexInput>(eventData.Entity);
            await _reportInfoAppService.AddIndexAsync(input);
        }

        public async Task HandleEventAsync(EntityUpdatedEto<ReportInfoEto> eventData)
        {
            var input = _objectMapper.Map<ReportInfoEto, UpdateReportInfoIndexInput>(eventData.Entity);
            await _reportInfoAppService.UpdateIndexAsync(input);
        }
    }
}