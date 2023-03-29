using System;
using System.Threading.Tasks;

namespace AElf.CrossChainServer.CrossChain;

public interface IReportInfoAppService
{
    Task CreateAsync(CreateReportInfoInput input);

    Task UpdateStepAsync(string chainId, long roundId, string token, string targetChainId, ReportStep step,
        DateTime updateTime);
    Task AddIndexAsync(AddReportInfoIndexInput input);
    Task UpdateIndexAsync(UpdateReportInfoIndexInput input);
    Task<int> CalculateCrossChainProgressAsync(string receiptId);
    Task UpdateStepAsync();
    Task ReSendQueryAsync();
    Task CheckQueryTransactionAsync();
}