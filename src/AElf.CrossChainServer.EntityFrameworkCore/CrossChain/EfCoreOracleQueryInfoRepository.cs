using System;
using AElf.CrossChainServer.Chains;
using AElf.CrossChainServer.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace AElf.CrossChainServer.CrossChain
{
    public class EfCoreOracleQueryInfoRepository : EfCoreCacheRepository<CrossChainServerDbContext, OracleQueryInfo, Guid>, IOracleQueryInfoRepository
    {
        public EfCoreOracleQueryInfoRepository(IDbContextProvider<CrossChainServerDbContext> dbContextProvider)
            : base(dbContextProvider)
        {

        }
    }
}