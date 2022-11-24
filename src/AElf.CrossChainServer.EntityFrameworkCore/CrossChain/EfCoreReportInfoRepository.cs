using System;
using AElf.CrossChainServer.Chains;
using AElf.CrossChainServer.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace AElf.CrossChainServer.CrossChain
{
    public class EfCoreReportInfoRepository : EfCoreCacheRepository<CrossChainServerDbContext, ReportInfo, Guid>, IReportInfoRepository
    {
        public EfCoreReportInfoRepository(IDbContextProvider<CrossChainServerDbContext> dbContextProvider)
            : base(dbContextProvider)
        {

        }
    }
}