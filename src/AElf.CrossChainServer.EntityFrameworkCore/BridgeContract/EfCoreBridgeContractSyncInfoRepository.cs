using System;
using AElf.CrossChainServer.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace AElf.CrossChainServer.BridgeContract
{
    public class EfCoreBridgeContractSyncInfoRepository : EfCoreCacheRepository<CrossChainServerDbContext, BridgeContractSyncInfo, Guid>, IBridgeContractSyncInfoRepository
    {
        public EfCoreBridgeContractSyncInfoRepository(IDbContextProvider<CrossChainServerDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
            
        }
    }
}