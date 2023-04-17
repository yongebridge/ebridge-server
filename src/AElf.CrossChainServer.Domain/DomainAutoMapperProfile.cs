using AElf.CrossChainServer.CrossChain;
using AutoMapper;

namespace AElf.CrossChainServer
{
    public class DomainAutoMapperProfile : Profile
    {
        public DomainAutoMapperProfile()
        {
            CreateMap<CrossChainTransfer, CrossChainTransferEto>();
            CreateMap<CrossChainIndexingInfo, CrossChainIndexingInfoEto>();
            CreateMap<OracleQueryInfo, OracleQueryInfoEto>();
            CreateMap<ReportInfo, ReportInfoEto>();
        }
    }
}