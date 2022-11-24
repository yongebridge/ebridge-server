using System;
using Volo.Abp.Domain.Repositories;

namespace AElf.CrossChainServer.CrossChain
{
    public interface IReportInfoRepository : IRepository<ReportInfo, Guid>
    {
        
    }
}