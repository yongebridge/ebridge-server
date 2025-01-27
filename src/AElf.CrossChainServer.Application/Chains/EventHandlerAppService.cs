using System;
using System.Threading.Tasks;
using AElf.AElfNode.EventHandler.Core.Repositories;
using Volo.Abp;

namespace AElf.CrossChainServer.Chains;

[RemoteService(IsEnabled = false)]
public class EventHandlerAppService : CrossChainServerAppService, IEventHandlerAppService
{
    private readonly ISaveDataRepository _saveDataRepository;
    private readonly IChainAppService _chainAppService;

    public EventHandlerAppService(ISaveDataRepository saveDataRepository, IChainAppService chainAppService)
    {
        _saveDataRepository = saveDataRepository;
        _chainAppService = chainAppService;
    }

    public async Task<DateTime> GetLatestSyncTimeAsync(string chainId, string jobCategory)
    {
        var chain = await _chainAppService.GetAsync(chainId);
        var dataKey = $"{chain.AElfChainId}-{jobCategory}-LatestCheckTickKey";
        var date = await _saveDataRepository.FindAsync(o => o.Key == dataKey);
        if (date == null)
        {
            return DateTime.MinValue;
        }

        return new DateTime(long.Parse(date.Data));
    }
}