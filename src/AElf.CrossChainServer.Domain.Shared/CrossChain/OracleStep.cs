namespace AElf.CrossChainServer.CrossChain;

public enum OracleStep
{
    QueryCreated,
    Committed,
    SufficientCommitmentsCollected,
    CommitmentRevealed,
    QueryCompleted
}