using Volo.Abp.Application.Dtos;

namespace AElf.CrossChainServer.CrossChain;

public class GetCrossChainTransfersInput : PagedAndSortedResultRequestDto
{
    public string FromChainId { get; set; }
    public string ToChainId { get; set; }
    public string FromAddress { get; set; }
    public string ToAddress { get; set; }
    public CrossChainStatus? Status { get; set; }
    public CrossChainType? Type { get; set; }
}