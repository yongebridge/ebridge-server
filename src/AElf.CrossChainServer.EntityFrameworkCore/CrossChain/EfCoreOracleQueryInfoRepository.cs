using System;
using AElf.CrossChainServer.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace AElf.CrossChainServer.CrossChain
{
    public class EfCoreOracleQueryInfoRepository : EfCoreRepository<CrossChainServerDbContext, OracleQueryInfo, Guid>, IOracleQueryInfoRepository
    {
        public EfCoreOracleQueryInfoRepository(IDbContextProvider<CrossChainServerDbContext> dbContextProvider)
            : base(dbContextProvider)
        {

        }
    }
}