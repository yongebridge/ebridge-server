using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.CrossChainServer.Chains;
using AElf.CrossChainServer.CrossChain;
using AElf.CrossChainServer.Settings;
using AElf.CrossChainServer.Tokens;
using GraphQL;
using GraphQL.Client.Abstractions;
using Volo.Abp.SettingManagement;

namespace AElf.CrossChainServer.Worker.IndexerSync;

public class CrossChainTransferIndexerSyncProvider : IndexerSyncProviderBase
{
    private readonly ICrossChainTransferAppService _crossChainTransferAppService;
    private readonly ITokenAppService _tokenAppService;
    private readonly IChainAppService _chainAppService;

    public CrossChainTransferIndexerSyncProvider(IGraphQLClient graphQlClient, ISettingManager settingManager,
        ICrossChainTransferAppService crossChainTransferAppService, IChainAppService chainAppService,
        ITokenAppService tokenAppService) : base(
        graphQlClient, settingManager)
    {
        _crossChainTransferAppService = crossChainTransferAppService;
        _chainAppService = chainAppService;
        _tokenAppService = tokenAppService;
    }

    protected override string SyncType { get; } = CrossChainServerSettings.CrossChainTransferIndexerSync;

    protected override async Task<long> HandleDataAsync(string chainId, long startHeight, long endHeight)
    {
        var processedHeight = startHeight;

        var data = await QueryDataAsync<CrossChainTransferInfoDto>(GetRequest(chainId, startHeight, endHeight));
        if (data == null || data.CrossChainTransferInfo.Count == 0)
        {
            return processedHeight;
        }

        foreach (var crossChainTransfer in data.CrossChainTransferInfo)
        {
            await HandleDataAsync(crossChainTransfer);
            processedHeight = crossChainTransfer.BlockHeight;
        }

        return processedHeight;
    }

    private async Task HandleDataAsync(CrossChainTransferInfo transfer)
    {
        var chain = await _chainAppService.GetByAElfChainIdAsync(ChainHelper.ConvertBase58ToChainId(transfer.ChainId));

        switch (transfer.TransferType)
        {
            case TransferType.Transfer:
                var toChain = await _chainAppService.GetByAElfChainIdAsync(ChainHelper.ConvertBase58ToChainId(transfer.ToChainId));
                if (toChain == null)
                {
                    return;
                }
                
                var transferToken = await _tokenAppService.GetAsync(new GetTokenInput
                {
                    ChainId = chain.Id,
                    Symbol = transfer.TransferTokenSymbol
                });
                
                await _crossChainTransferAppService.TransferAsync(new CrossChainTransferInput
                {
                    TransferAmount = transfer.TransferAmount / (decimal)Math.Pow(10, transferToken.Decimals),
                    FromAddress = transfer.FromAddress,
                    ToAddress = transfer.ToAddress,
                    TransferTokenId = transferToken.Id,
                    FromChainId = chain.Id,
                    ToChainId = toChain.Id,
                    TransferBlockHeight = transfer.BlockHeight,
                    TransferTime = transfer.TransferTime,
                    TransferTransactionId = transfer.TransferTransactionId
                });
                break;
            case TransferType.Receive:
                var fromChain = await _chainAppService.GetByAElfChainIdAsync(ChainHelper.ConvertBase58ToChainId(transfer.FromChainId));
                if (fromChain == null)
                {
                    return;
                }

                var receiveToken = await _tokenAppService.GetAsync(new GetTokenInput
                {
                    ChainId =chain.Id,
                    Symbol = transfer.ReceiveTokenSymbol
                });
                
                await _crossChainTransferAppService.ReceiveAsync(new CrossChainReceiveInput()
                {
                    ReceiveAmount = transfer.ReceiveAmount / (decimal)Math.Pow(10, receiveToken.Decimals),
                    ReceiveTime = transfer.ReceiveTime,
                    FromChainId = fromChain.Id,
                    ReceiveTransactionId = transfer.ReceiveTransactionId,
                    ToChainId = chain.Id,
                    TransferTransactionId = transfer.TransferTransactionId,
                    ReceiveTokenId = receiveToken.Id,
                    FromAddress = transfer.FromAddress,
                    ToAddress = transfer.ToAddress
                });
                break;
        }
    }

    private GraphQLRequest GetRequest(string chainId, long startHeight, long endHeight)
    {
        return new GraphQLRequest
        {
            Query =
                @"query($chainId:String,$startBlockHeight:Long!,$endBlockHeight:Long!,$methodNames: [String],$skipCount:Int!,$maxResultCount:Int!){
            caHolderTransactionInfo(dto: {chainId:$chainId,startBlockHeight:$startBlockHeight,endBlockHeight:$endBlockHeight, methodNames:$methodNames,skipCount:$skipCount,maxResultCount:$maxResultCount}){
                totalRecordCount,
                data{
                    blockHash,
                    blockHeight,
                    transactionId,
                    methodName,
                    transferInfo{
                        fromChainId,
                        toChainId
                    }
                }
            }
        }",
            Variables = new
            {
                chainId = chainId,
                startBlockHeight = startHeight,
                endBlockHeight = endHeight
            }
        };
    }
}

public class CrossChainTransferInfoDto
{
    public List<CrossChainTransferInfo> CrossChainTransferInfo { get; set; }
}

public class CrossChainTransferInfo : GraphQLDto
{
    public string FromChainId { get; set; }
    public string ToChainId { get; set; }
    public string FromAddress { get; set; }
    public string ToAddress { get; set; }
    public string TransferTransactionId { get; set; }
    public string ReceiveTransactionId { get; set; }
    public DateTime TransferTime { get; set; }
    public long TransferBlockHeight { get; set; }
    public DateTime ReceiveTime { get; set; }
    public long TransferAmount { get; set; }
    public long ReceiveAmount { get; set; }
    public string ReceiptId { get; set; }
    public string TransferTokenSymbol { get; set; }
    public string ReceiveTokenSymbol { get; set; }
    public TransferType TransferType { get; set; }
}

public enum TransferType
{
    Transfer,
    Receive
}