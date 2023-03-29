using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using Microsoft.Extensions.Logging;
using Nest;
using Volo.Abp;

namespace AElf.CrossChainServer.CrossChain;

[RemoteService(IsEnabled = false)]
public class OracleQueryInfoAppService : CrossChainServerAppService, IOracleQueryInfoAppService
{
    private readonly INESTRepository<OracleQueryInfoIndex, Guid> _oracleQueryInfoIndexRepository;
    private readonly IOracleQueryInfoRepository _oracleQueryInfoRepository;

    public OracleQueryInfoAppService(IOracleQueryInfoRepository oracleQueryInfoRepository,
        INESTRepository<OracleQueryInfoIndex, Guid> oracleQueryInfoIndexRepository)
    {
        _oracleQueryInfoRepository = oracleQueryInfoRepository;
        _oracleQueryInfoIndexRepository = oracleQueryInfoIndexRepository;
    }

    public async Task CreateAsync(CreateOracleQueryInfoInput input)
    {
        var info = ObjectMapper.Map<CreateOracleQueryInfoInput, OracleQueryInfo>(input);
        await _oracleQueryInfoRepository.InsertAsync(info);
    }
    
    public async Task UpdateAsync(UpdateOracleQueryInfoInput input)
    {
        var infoList = await _oracleQueryInfoRepository.GetListAsync(o =>
            o.ChainId == input.ChainId && o.QueryId == input.QueryId);

        if (infoList.Count == 0)
        {
            return;
        }
        
        foreach (var info in infoList)
        {
            if (info.Step >= input.Step)
            {
                Logger.LogDebug(
                    "Invalid oracle step. ChainId: {chainId}, QueryId: {queryId}, Step: {step}, Input Step: {inputStep}",
                    info.ChainId, info.QueryId, info.Step, input.Step);
                continue;
            }
            
            info.Step = input.Step;
            info.UpdateTime = input.UpdateTime;
        }
        
        await _oracleQueryInfoRepository.UpdateManyAsync(infoList);
    }

    public async Task AddIndexAsync(AddOracleQueryInfoIndexInput input)
    {
        var index = ObjectMapper.Map<AddOracleQueryInfoIndexInput, OracleQueryInfoIndex>(input);
        await _oracleQueryInfoIndexRepository.AddAsync(index);
    }
    
    public async Task UpdateIndexAsync(UpdateOracleQueryInfoIndexInput input)
    {
        var index = ObjectMapper.Map<UpdateOracleQueryInfoIndexInput, OracleQueryInfoIndex>(input);
        await _oracleQueryInfoIndexRepository.UpdateAsync(index);
    }

    public async Task<int> CalculateCrossChainProgressAsync(string option)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<OracleQueryInfoIndex>, QueryContainer>>
        {
            q => q.Term(i => i.Field(f => f.Option).Value(option)),
        };
        QueryContainer Query(QueryContainerDescriptor<OracleQueryInfoIndex> f) => f.Bool(b => b.Must(mustQuery));
        
        var queryInfo = await _oracleQueryInfoIndexRepository.GetAsync(Query, sortExp: o=>o.Step, sortType: SortOrder.Descending);

        if (queryInfo == null)
        {
            return 0;
        }

        return ((int)queryInfo.Step + 1) * 20;
    }
}