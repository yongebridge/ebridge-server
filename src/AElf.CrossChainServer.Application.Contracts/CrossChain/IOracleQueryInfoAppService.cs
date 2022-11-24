using System.Threading.Tasks;

namespace AElf.CrossChainServer.CrossChain;

public interface IOracleQueryInfoAppService
{
    Task CreateAsync(CreateOracleQueryInfoInput input);
    Task UpdateAsync(UpdateOracleQueryInfoInput input);
    Task AddIndexAsync(AddOracleQueryInfoIndexInput input);
    Task UpdateIndexAsync(UpdateOracleQueryInfoIndexInput input);
    Task<double> CalculateCrossChainProgressAsync(string option);
}