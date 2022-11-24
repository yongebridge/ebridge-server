using System;
using AElf.CrossChainServer.Tokens;
using AElf.Indexing.Elasticsearch;
using Volo.Abp.Domain.Entities;

namespace AElf.CrossChainServer.CrossChain;

public class CrossChainTransferIndex : CrossChainTransferBase, IIndexBuild
{
    public Token TransferToken { get; set; }
    public Token ReceiveToken { get; set; }
}