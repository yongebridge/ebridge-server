using System;
using AElf.CrossChainServer.Tokens;
using Volo.Abp.Domain.Repositories;

namespace AElf.CrossChainServer.BridgeContract
{
    public interface IBridgeContractSyncInfoRepository : IRepository<BridgeContractSyncInfo, Guid>
    {
        
    }
}