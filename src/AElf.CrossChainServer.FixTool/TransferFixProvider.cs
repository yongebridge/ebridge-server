using System;
using System.Threading.Tasks;
using AElf.CrossChainServer.CrossChain;
using AElf.CrossChainServer.Tokens;

namespace AElf.CrossChainServer.FixTool;

public class TransferFixProvider
{
    private readonly ICrossChainTransferAppService _crossChainTransferAppService;
    private readonly IReportInfoAppService _reportInfoAppService;
    private readonly ITokenAppService _tokenAppService;

    public TransferFixProvider(ICrossChainTransferAppService crossChainTransferAppService,
        IReportInfoAppService reportInfoAppService, ITokenAppService tokenAppService)
    {
        _crossChainTransferAppService = crossChainTransferAppService;
        _reportInfoAppService = reportInfoAppService;
        _tokenAppService = tokenAppService;
    }

    public async Task FixAsync()
    {
        await Fix_04bd5f6f74666492b507b195c89dccef5187d98b0d917a40555a869011641a13_Async();
        await Fix_954021802ce3d7ccfd7a878c4b7ce73b8b780118e2cebd9691eaa9c5db0ed4b5_Async();
        await Fix_e68602283c49956db567962ccdf585f5fb88fae1911dac4b98652f51f31af254_Async();
        await Fix_d892be2336f5737ed5765533bbba1e07d10223778782f32d35d794aad09e1b16_Async();
    }
    
    public async Task Fix_04bd5f6f74666492b507b195c89dccef5187d98b0d917a40555a869011641a13_Async()
    {
        var fromChainId = "MainChain_AELF";
        var toChainId = "BSC";
        var fromAddress = "u3j1F7vreHfdC3FZvjeGZ1AVY19n6JJGJbRzJDJk3tgz8Xv7A";
        var toAddress = "0x75392dDfD264d645992bB22365B05b487cF565Eb";
        var receiptId = "fc958f44a28a4abe387f9be1ebec01c4fd52a457c0397d621b2d8648783a1e08.5";
        var receiptHash = "b788407f011292945b526d7b36bad5f88e3d3901a96abf7d34c92a87b7f04299";
        var reportToken = "0xE6d0B7B043217Da485BE30c57D36E02FDfa7cfc6";
        var roundId = 38;
        var transferTransactionId = "04bd5f6f74666492b507b195c89dccef5187d98b0d917a40555a869011641a13";
        var amount = 310;
        
        var token = await _tokenAppService.GetAsync(new GetTokenInput
        {
            Symbol = "ELF",
            ChainId = fromChainId
        });

        await _crossChainTransferAppService.TransferAsync(new CrossChainTransferInput
        {
            TransferAmount = amount,
            FromAddress = fromAddress,
            ToAddress = toAddress,
            TransferTokenId = token.Id,
            FromChainId = fromChainId,
            ToChainId = toChainId,
            TransferBlockHeight = 158991670,
            TransferTime = DateTime.Parse("2023-07-08T18:32:04.6023898Z"),
            TransferTransactionId = transferTransactionId,
            ReceiptId = receiptId
        });

        await _reportInfoAppService.CreateAsync(new CreateReportInfoInput
        {
            ChainId = fromChainId,
            ReceiptId = receiptId,
            ReceiptHash = receiptHash,
            RoundId = roundId,
            Token = reportToken,
            TargetChainId = toChainId,
            LastUpdateHeight = 158991670
        });

        await _reportInfoAppService.UpdateStepAsync(fromChainId, roundId, reportToken,
            toChainId, ReportStep.Confirmed, 158992168);
    }
    
    public async Task Fix_954021802ce3d7ccfd7a878c4b7ce73b8b780118e2cebd9691eaa9c5db0ed4b5_Async()
    {
        var fromChainId = "MainChain_AELF";
        var toChainId = "BSC";
        var fromAddress = "u3j1F7vreHfdC3FZvjeGZ1AVY19n6JJGJbRzJDJk3tgz8Xv7A";
        var toAddress = "0x75392dDfD264d645992bB22365B05b487cF565Eb";
        var receiptId = "fc958f44a28a4abe387f9be1ebec01c4fd52a457c0397d621b2d8648783a1e08.6";
        var transferTransactionId = "954021802ce3d7ccfd7a878c4b7ce73b8b780118e2cebd9691eaa9c5db0ed4b5";
        var amount = 9;
        
        var token = await _tokenAppService.GetAsync(new GetTokenInput
        {
            Symbol = "ELF",
            ChainId = fromChainId
        });

        await _crossChainTransferAppService.TransferAsync(new CrossChainTransferInput
        {
            TransferAmount = amount,
            FromAddress = fromAddress,
            ToAddress = toAddress,
            TransferTokenId = token.Id,
            FromChainId = fromChainId,
            ToChainId = toChainId,
            TransferBlockHeight = 158993674,
            TransferTime = DateTime.Parse("2023-07-08T18:45:01.6155169Z"),
            TransferTransactionId = transferTransactionId,
            ReceiptId = receiptId
        });
    }
    
    public async Task Fix_e68602283c49956db567962ccdf585f5fb88fae1911dac4b98652f51f31af254_Async()
    {
        var fromChainId = "MainChain_AELF";
        var toChainId = "BSC";
        var fromAddress = "u3j1F7vreHfdC3FZvjeGZ1AVY19n6JJGJbRzJDJk3tgz8Xv7A";
        var toAddress = "0x75392dDfD264d645992bB22365B05b487cF565Eb";
        var receiptId = "fc958f44a28a4abe387f9be1ebec01c4fd52a457c0397d621b2d8648783a1e08.7";
        var receiptHash = "78b32acc9db88ba95d8171bd4865f4ce49314d943292188625e0cdbf846a78e8";
        var reportToken = "0xE6d0B7B043217Da485BE30c57D36E02FDfa7cfc6";
        var roundId = 40;
        var transferTransactionId = "e68602283c49956db567962ccdf585f5fb88fae1911dac4b98652f51f31af254";
        var amount = (decimal)0.2;
        
        var token = await _tokenAppService.GetAsync(new GetTokenInput
        {
            Symbol = "ELF",
            ChainId = fromChainId
        });

        await _crossChainTransferAppService.TransferAsync(new CrossChainTransferInput
        {
            TransferAmount = amount,
            FromAddress = fromAddress,
            ToAddress = toAddress,
            TransferTokenId = token.Id,
            FromChainId = fromChainId,
            ToChainId = toChainId,
            TransferBlockHeight = 158999136,
            TransferTime = DateTime.Parse("2023-07-08T19:31:44.1528468Z"),
            TransferTransactionId = transferTransactionId,
            ReceiptId = receiptId
        });

        await _reportInfoAppService.CreateAsync(new CreateReportInfoInput
        {
            ChainId = fromChainId,
            ReceiptId = receiptId,
            ReceiptHash = receiptHash,
            RoundId = roundId,
            Token = reportToken,
            TargetChainId = toChainId,
            LastUpdateHeight = 158999136
        });
    }
    
    public async Task Fix_d892be2336f5737ed5765533bbba1e07d10223778782f32d35d794aad09e1b16_Async()
    {
        var fromChainId = "MainChain_AELF";
        var toChainId = "BSC";
        var fromAddress = "L3imsa38dskN2dbDngEpTPbsWopQNqVrk6snpH8RojPTGyS2D";
        var toAddress = "0xa1d69B7db19e75DD88473BBb99c904901c6d2fE4";
        var receiptId = "fc958f44a28a4abe387f9be1ebec01c4fd52a457c0397d621b2d8648783a1e08.15";
        var receiptHash = "6d0acdca16c4245cf0bd7ee367b842527efe70fa300f1000998263c5ef4f4e7b";
        var reportToken = "0xE6d0B7B043217Da485BE30c57D36E02FDfa7cfc6";
        var roundId = 48;
        var transferTransactionId = "d892be2336f5737ed5765533bbba1e07d10223778782f32d35d794aad09e1b16";
        var amount = 15;
        
        var token = await _tokenAppService.GetAsync(new GetTokenInput
        {
            Symbol = "ELF",
            ChainId = fromChainId
        });

        await _crossChainTransferAppService.TransferAsync(new CrossChainTransferInput
        {
            TransferAmount = amount,
            FromAddress = fromAddress,
            ToAddress = toAddress,
            TransferTokenId = token.Id,
            FromChainId = fromChainId,
            ToChainId = toChainId,
            TransferBlockHeight = 159130222,
            TransferTime = DateTime.Parse("2023-07-09T14:42:52.6799688Z"),
            TransferTransactionId = transferTransactionId,
            ReceiptId = receiptId
        });

        await _reportInfoAppService.CreateAsync(new CreateReportInfoInput
        {
            ChainId = fromChainId,
            ReceiptId = receiptId,
            ReceiptHash = receiptHash,
            RoundId = roundId,
            Token = reportToken,
            TargetChainId = toChainId,
            LastUpdateHeight = 159130222
        });
    }
}