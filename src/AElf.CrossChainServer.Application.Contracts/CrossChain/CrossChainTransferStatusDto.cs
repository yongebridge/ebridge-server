using System;
using Volo.Abp.Application.Dtos;

namespace AElf.CrossChainServer.CrossChain;

public class CrossChainTransferStatusDto : EntityDto<Guid>
{
    public double Progress { get; set; }
}