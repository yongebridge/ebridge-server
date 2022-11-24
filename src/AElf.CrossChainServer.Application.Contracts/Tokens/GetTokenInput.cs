namespace AElf.CrossChainServer.Tokens;

public class GetTokenInput
{
    public string ChainId { get; set; }
    public string Address { get; set; }
    public string Symbol { get; set; }
}