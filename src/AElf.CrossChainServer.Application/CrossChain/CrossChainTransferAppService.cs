using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using AElf.CrossChainServer.Chains;
using AElf.CrossChainServer.Contracts;
using AElf.CrossChainServer.Tokens;
using AElf.Indexing.Elasticsearch;
using AElf.Types;
using Google.Protobuf;
using Nest;
using Nethereum.Util;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using AElf.Contracts.MultiToken;
using Microsoft.Extensions.Logging;

namespace AElf.CrossChainServer.CrossChain;

[RemoteService(IsEnabled = false)]
public class CrossChainTransferAppService : CrossChainServerAppService, ICrossChainTransferAppService
{
    private readonly ICrossChainTransferRepository _crossChainTransferRepository;
    private readonly IChainAppService _chainAppService;
    private readonly ICrossChainIndexingInfoAppService _crossChainIndexingInfoAppService;
    private readonly INESTRepository<CrossChainTransferIndex, Guid> _crossChainTransferIndexRepository;
    private readonly ITokenRepository _tokenRepository;
    private readonly IOracleQueryInfoAppService _oracleQueryInfoAppService;
    private readonly IReportInfoAppService _reportInfoAppService;
    private readonly IBlockchainAppService _blockchainAppService;
    private readonly IBridgeContractAppService _bridgeContractAppService;
    private readonly ITokenContractAppService _tokenContractAppService;
    private readonly ICrossChainContractAppService _crossChainContractAppService;
    private readonly ITokenSymbolMappingProvider _tokenSymbolMappingProvider;

    private const int PageCount = 1000;

    public CrossChainTransferAppService(ICrossChainTransferRepository crossChainTransferRepository,
        IChainAppService chainAppService, ICrossChainIndexingInfoAppService crossChainIndexingInfoAppService,
        INESTRepository<CrossChainTransferIndex, Guid> crossChainTransferIndexRepository,
        ITokenRepository tokenRepository, IOracleQueryInfoAppService oracleQueryInfoAppService,
        IReportInfoAppService reportInfoAppService, IBlockchainAppService blockchainAppService,
        IBridgeContractAppService bridgeContractAppService, ITokenContractAppService tokenContractAppService,
        ICrossChainContractAppService crossChainContractAppService,
        ITokenSymbolMappingProvider tokenSymbolMappingProvider)
    {
        _crossChainTransferRepository = crossChainTransferRepository;
        _chainAppService = chainAppService;
        _crossChainIndexingInfoAppService = crossChainIndexingInfoAppService;
        _crossChainTransferIndexRepository = crossChainTransferIndexRepository;
        _tokenRepository = tokenRepository;
        _oracleQueryInfoAppService = oracleQueryInfoAppService;
        _reportInfoAppService = reportInfoAppService;
        _blockchainAppService = blockchainAppService;
        _bridgeContractAppService = bridgeContractAppService;
        _tokenContractAppService = tokenContractAppService;
        _crossChainContractAppService = crossChainContractAppService;
        _tokenSymbolMappingProvider = tokenSymbolMappingProvider;
    }

    public async Task<PagedResultDto<CrossChainTransferIndexDto>> GetListAsync(GetCrossChainTransfersInput input)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<CrossChainTransferIndex>, QueryContainer>>();
        if (!input.FromChainId.IsNullOrWhiteSpace())
        {
            mustQuery.Add(q => q.Term(i => i.Field(f => f.FromChainId).Value(input.FromChainId)));
        }

        if (!input.ToChainId.IsNullOrWhiteSpace())
        {
            mustQuery.Add(q => q.Term(i => i.Field(f => f.ToChainId).Value(input.ToChainId)));
        }

        if (!input.FromAddress.IsNullOrWhiteSpace())
        {
            mustQuery.Add(q => q.Term(i => i.Field(f => f.FromAddress).Value(input.FromAddress)));
        }

        if (!input.ToAddress.IsNullOrWhiteSpace())
        {
            mustQuery.Add(q => q.Term(i => i.Field(f => f.ToAddress).Value(input.ToAddress)));
        }

        if (input.Status.HasValue)
        {
            mustQuery.Add(q => q.Term(i => i.Field(f => f.Status).Value(input.Status.Value)));
        }

        if (input.Type.HasValue)
        {
            mustQuery.Add(q => q.Term(i => i.Field(f => f.Type).Value(input.Type.Value)));
        }

        QueryContainer Filter(QueryContainerDescriptor<CrossChainTransferIndex> f) => f.Bool(b => b.Must(mustQuery));

        var list = await _crossChainTransferIndexRepository.GetListAsync(Filter, limit: input.MaxResultCount,
            skip: input.SkipCount, sortExp: o => o.TransferTime, sortType: SortOrder.Descending);
        var totalCount = await _crossChainTransferIndexRepository.CountAsync(Filter);

        return new PagedResultDto<CrossChainTransferIndexDto>
        {
            TotalCount = totalCount.Count,
            Items = ObjectMapper.Map<List<CrossChainTransferIndex>, List<CrossChainTransferIndexDto>>(list.Item2)
        };
    }

    public async Task<ListResultDto<CrossChainTransferStatusDto>> GetStatusAsync(GetCrossChainTransferStatusInput input)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<CrossChainTransferIndex>, QueryContainer>>();
        mustQuery.Add(q => q.Ids(i => i.Values(input.Ids)));

        QueryContainer Filter(QueryContainerDescriptor<CrossChainTransferIndex> f) => f.Bool(b => b.Must(mustQuery));

        var list = await _crossChainTransferIndexRepository.GetListAsync(Filter);
        return new ListResultDto<CrossChainTransferStatusDto>
        {
            Items = ObjectMapper.Map<List<CrossChainTransferIndex>, List<CrossChainTransferStatusDto>>(list.Item2)
        };
    }

    public async Task TransferAsync(CrossChainTransferInput input)
    {
        var transfer = await FindCrossChainTransferAsync(input.FromChainId, input.ToChainId,
            input.TransferTransactionId, input.ReceiptId);

        if (transfer == null)
        {
            transfer = ObjectMapper.Map<CrossChainTransferInput, CrossChainTransfer>(input);
            transfer.Type = await GetCrossChainTypeAsync(input.FromChainId, input.ToChainId);
            transfer.Progress = 0;
            transfer.ProgressUpdateTime = input.TransferTime;
            transfer.Status = CrossChainStatus.Transferred;

            await _crossChainTransferRepository.InsertAsync(transfer);
        }
        else
        {
            transfer.TransferTokenId = input.TransferTokenId;
            transfer.TransferTransactionId = input.TransferTransactionId;
            transfer.TransferAmount = input.TransferAmount;
            transfer.TransferTime = input.TransferTime;
            transfer.TransferBlockHeight = input.TransferBlockHeight;

            await _crossChainTransferRepository.UpdateAsync(transfer);
        }
    }

    public async Task ReceiveAsync(CrossChainReceiveInput input)
    {
        var transfer = await FindCrossChainTransferAsync(input.FromChainId, input.ToChainId,
            input.TransferTransactionId, input.ReceiptId);

        var isTransferExist = true;
        if (transfer == null)
        {
            isTransferExist = false;
            transfer = ObjectMapper.Map<CrossChainReceiveInput, CrossChainTransfer>(input);
            transfer.Type = await GetCrossChainTypeAsync(input.FromChainId, input.ToChainId);
        }
        else
        {
            transfer.ReceiveTokenId = input.ReceiveTokenId;
            transfer.ReceiveTransactionId = input.ReceiveTransactionId;
            transfer.ReceiveTime = input.ReceiveTime;
            transfer.ReceiveAmount = input.ReceiveAmount;
        }

        transfer.Status = CrossChainStatus.Received;
        transfer.Progress = 100;
        transfer.ProgressUpdateTime = input.ReceiveTime;

        if (isTransferExist)
        {
            await _crossChainTransferRepository.UpdateAsync(transfer);
        }
        else
        {
            await _crossChainTransferRepository.InsertAsync(transfer);
        }
    }

    private async Task<CrossChainTransfer> FindCrossChainTransferAsync(string fromChainId, string toChainId,
        string transferTransactionId, string receiptId)
    {
        var crossChainType = await GetCrossChainTypeAsync(fromChainId, toChainId);
        CrossChainTransfer transfer;
        switch (crossChainType)
        {
            case CrossChainType.Homogeneous:
                transfer = await _crossChainTransferRepository.FindAsync(o =>
                    o.FromChainId == fromChainId && o.ToChainId == toChainId &&
                    o.TransferTransactionId == transferTransactionId);
                break;
            case CrossChainType.Heterogeneous:
                transfer = await _crossChainTransferRepository.FindAsync(o =>
                    o.FromChainId == fromChainId && o.ToChainId == toChainId &&
                    o.ReceiptId == receiptId);
                break;
            default:
                throw new NotSupportedException();
        }

        return transfer;
    }

    public async Task AddIndexAsync(AddCrossChainTransferIndexInput input)
    {
        var index = ObjectMapper.Map<AddCrossChainTransferIndexInput, CrossChainTransferIndex>(input);

        if (input.TransferTokenId != Guid.Empty)
        {
            index.TransferToken = await _tokenRepository.GetAsync(input.TransferTokenId);
        }

        if (input.ReceiveTokenId != Guid.Empty)
        {
            index.ReceiveToken = await _tokenRepository.GetAsync(input.ReceiveTokenId);
        }

        await _crossChainTransferIndexRepository.AddAsync(index);
    }

    public async Task UpdateIndexAsync(UpdateCrossChainTransferIndexInput input)
    {
        var index = ObjectMapper.Map<UpdateCrossChainTransferIndexInput, CrossChainTransferIndex>(input);

        if (input.TransferTokenId != Guid.Empty)
        {
            index.TransferToken = await _tokenRepository.GetAsync(input.TransferTokenId);
        }

        if (input.ReceiveTokenId != Guid.Empty)
        {
            index.ReceiveToken = await _tokenRepository.GetAsync(input.ReceiveTokenId);
        }

        await _crossChainTransferIndexRepository.UpdateAsync(index);
    }

    public async Task UpdateProgressAsync()
    {
        var page = 0;
        var crossChainTransfers = await GetToUpdateProgressAsync(page);

        while (crossChainTransfers.Count != 0)
        {
            var now = DateTime.UtcNow;
            var toUpdate = new List<CrossChainTransfer>();
            foreach (var transfer in crossChainTransfers)
            {
                var progress = 0d;
                switch (transfer.Type)
                {
                    case CrossChainType.Homogeneous:
                        progress = await _crossChainIndexingInfoAppService.CalculateCrossChainProgressAsync(
                            transfer.FromChainId, transfer.ToChainId, transfer.TransferBlockHeight,
                            transfer.TransferTime);
                        break;
                    case CrossChainType.Heterogeneous:
                        var chain = await _chainAppService.GetAsync(transfer.ToChainId);
                        if (chain.Type == BlockchainType.AElf)
                        {
                            progress =
                                await _oracleQueryInfoAppService.CalculateCrossChainProgressAsync(transfer.ReceiptId);
                        }
                        else
                        {
                            progress = await _reportInfoAppService.CalculateCrossChainProgressAsync(transfer.ReceiptId);
                        }

                        break;
                }

                if (progress == transfer.Progress)
                {
                    continue;
                }

                transfer.Progress = progress;
                transfer.ProgressUpdateTime = now;
                if (progress == 100)
                {
                    transfer.Status = CrossChainStatus.Indexed;
                }

                toUpdate.Add(transfer);
            }

            await _crossChainTransferRepository.UpdateManyAsync(toUpdate, true);

            page++;
            crossChainTransfers = await GetToUpdateProgressAsync(page);
        }
    }

    private async Task<CrossChainType> GetCrossChainTypeAsync(string fromChainId, string toChainId)
    {
        var fromChain = await _chainAppService.GetAsync(fromChainId);
        var toChain = await _chainAppService.GetAsync(toChainId);
        return fromChain.Type == toChain.Type ? CrossChainType.Homogeneous : CrossChainType.Heterogeneous;
    }

    private async Task<List<CrossChainTransfer>> GetToUpdateProgressAsync(int page)
    {
        var q = await _crossChainTransferRepository.GetQueryableAsync();
        var crossChainTransfers = await AsyncExecuter.ToListAsync(q
            .Where(o => o.Status == CrossChainStatus.Transferred && o.ProgressUpdateTime > DateTime.UtcNow.AddDays(-3))
            .OrderBy(o => o.ProgressUpdateTime)
            .Skip(PageCount * page)
            .Take(PageCount));
        return crossChainTransfers;
    }

    public async Task UpdateReceiveTransactionAsync()
    {
        var page = 0;
        var toUpdate = new List<CrossChainTransfer>();
        var crossChainTransfers = await GetToUpdateReceiveTransactionAsync(page);
        while (crossChainTransfers.Count != 0)
        {
            foreach (var transfer in crossChainTransfers)
            {
                try
                {
                    var txResult =
                        await _blockchainAppService.GetTransactionResultAsync(transfer.ToChainId,
                            transfer.ReceiveTransactionId);
                    if (txResult.IsFailed)
                    {
                        transfer.ReceiveTransactionId = null;
                        toUpdate.Add(transfer);
                    }
                    else if (txResult.IsMined)
                    {
                        var chainStatus = await _blockchainAppService.GetChainStatusAsync(transfer.ToChainId);
                        if (chainStatus.ConfirmedBlockHeight >= txResult.BlockHeight)
                        {
                            var block = await _blockchainAppService.GetBlockByHeightAsync(transfer.ToChainId,
                                txResult.BlockHeight);
                            if (block.BlockHash != txResult.BlockHash)
                            {
                                transfer.ReceiveTransactionId = null;
                                toUpdate.Add(transfer);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Update receive transaction failed. Id: {transfer.Id}, Error: {ex.Message}");
                }
            }

            page++;
            crossChainTransfers = await GetToUpdateReceiveTransactionAsync(page);
        }

        if (toUpdate.Count > 0)
        {
            await _crossChainTransferRepository.UpdateManyAsync(toUpdate);
        }
    }

    private async Task<List<CrossChainTransfer>> GetToUpdateReceiveTransactionAsync(int page)
    {
        var q = await _crossChainTransferRepository.GetQueryableAsync();
        var crossChainTransfers = await AsyncExecuter.ToListAsync(q
            .Where(o => o.Status == CrossChainStatus.Indexed && o.Progress == 100 && o.ReceiveTransactionId != null)
            .OrderBy(o => o.ProgressUpdateTime)
            .Skip(PageCount * page)
            .Take(PageCount));
        return crossChainTransfers;
    }

    public async Task AutoReceiveAsync()
    {
        var page = 0;
        var toUpdate = new List<CrossChainTransfer>();
        var crossChainTransfers = await GetToReceivedAsync(page);
        while (crossChainTransfers.Count != 0)
        {
            foreach (var transfer in crossChainTransfers)
            {
                try
                {
                    var toChain = await _chainAppService.GetAsync(transfer.ToChainId);
                    if (toChain.Type != BlockchainType.AElf)
                    {
                        continue;
                    }

                    var txId = await SendReceiveTransactionAsync(transfer);
                    transfer.ReceiveTransactionId = txId;
                    toUpdate.Add(transfer);
                    Logger.LogDebug($"Send auto receive tx: {txId}, FromChain: {transfer.FromChainId}, ToChain: {transfer.ToChainId}, Id: {transfer.Id}");
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Send auto receive tx failed. Id: {transfer.Id}, Error: {ex.Message}");
                }
            }

            page++;
            crossChainTransfers = await GetToUpdateReceiveTransactionAsync(page);
        }

        if (toUpdate.Count > 0)
        {
            await _crossChainTransferRepository.UpdateManyAsync(toUpdate);
        }
    }

    private async Task<string> SendReceiveTransactionAsync(CrossChainTransfer transfer)
    {
        string txId;
        if (transfer.Type == CrossChainType.Homogeneous)
        {
            var txResult =
                await _blockchainAppService.GetTransactionResultAsync(transfer.FromChainId,
                    transfer.TransferTransactionId);
            var parentHeight = txResult.BlockHeight;
            
            var paramsJson = JsonNode.Parse(txResult.Transaction.Params);
            var param = new AElf.Contracts.MultiToken.CrossChainTransferInput
            {
                To = Address.FromBase58(paramsJson["to"].ToString()),
                Amount = long.Parse(paramsJson["amount"].ToString()),
                Memo = paramsJson["memo"].ToString(),
                Symbol = paramsJson["symbol"].ToString(),
                IssueChainId = int.Parse(paramsJson["issueChainId"].ToString()),
                ToChainId = int.Parse(paramsJson["toChainId"].ToString())
            };

            var transaction = new Transaction
            {
                From = Address.FromBase58(txResult.Transaction.From),
                To = Address.FromBase58(txResult.Transaction.To),
                Params = param.ToByteString(),
                Signature = ByteString.FromBase64(txResult.Transaction.Signature),
                MethodName = txResult.Transaction.MethodName,
                RefBlockNumber = txResult.Transaction.RefBlockNumber,
                RefBlockPrefix = ByteString.FromBase64(txResult.Transaction.RefBlockPrefix)
            };

            var merklePath = await GetMerklePathAsync(transfer.FromChainId, transfer.TransferTransactionId);
            if (transfer.FromChainId != CrossChainServerConsts.AElfMainChainId)
            {
                var merkleProofContext = await _crossChainContractAppService.GetBoundParentChainHeightAndMerklePathByHeightAsync(
                    transfer.FromChainId, txResult.BlockHeight);
                parentHeight = merkleProofContext.BoundParentChainHeight;
                merklePath.MerklePathNodes.AddRange(merkleProofContext.MerklePathFromParentChain.MerklePathNodes);
            }

            
            var fromChain = await _chainAppService.GetAsync(transfer.FromChainId);
            txId = await _tokenContractAppService.CrossChainReceiveTokenAsync(transfer.ToChainId,
                fromChain.AElfChainId, parentHeight, transaction.ToByteArray().ToHex(), merklePath);
        }
        else
        {
            var transferToken = await _tokenRepository.GetAsync(transfer.TransferTokenId);
            var symbol =
                _tokenSymbolMappingProvider.GetMappingSymbol(transfer.FromChainId, transfer.ToChainId,
                    transferToken.Symbol);
            var swapId = await _bridgeContractAppService.GetSwapIdByTokenAsync(transfer.ToChainId, transfer.FromChainId,
                symbol);
            var amount = (new BigDecimal(transfer.TransferAmount)) * BigInteger.Pow(10, transferToken.Decimals);
            txId = await _bridgeContractAppService.SwapTokenAsync(transfer.ToChainId, swapId, transfer.ReceiptId,
                amount.ToString(),
                transfer.ToAddress);
        }

        return txId;
    }

    private async Task<MerklePath> GetMerklePathAsync(string chainId, string txId)
    {
        var merklePathDto = await _blockchainAppService.GetMerklePathAsync(chainId,txId);
        var merklePath = new MerklePath();
        foreach (var node in merklePathDto.MerklePathNodes)
        {
            merklePath.MerklePathNodes.Add(new MerklePathNode()
            {
                Hash = new Hash() { Value = AElf.Types.Hash.LoadFromHex(node.Hash).Value },
                IsLeftChildNode = node.IsLeftChildNode
            });
        }

        return merklePath;
    }

    private async Task<List<CrossChainTransfer>> GetToReceivedAsync(int page)
    {
        var q = await _crossChainTransferRepository.GetQueryableAsync();
        var crossChainTransfers = await AsyncExecuter.ToListAsync(q
            .Where(o => o.Progress == 100 && o.ReceiveTransactionId == null)
            .OrderBy(o => o.ProgressUpdateTime)
            .Skip(PageCount * page)
            .Take(PageCount));
        return crossChainTransfers;
    }
}