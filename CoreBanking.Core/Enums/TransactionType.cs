namespace CoreBanking.Core.Enums
{
    public enum TransactionType
    {
        Deposit = 1,
        Withdrawal = 2,
        TransferIn = 3,    // Money coming into account
        TransferOut = 4,   // Money leaving account
        Interest = 5
    }
}