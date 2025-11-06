namespace CoreBanking.Core.Enums
{
    public enum TransferStatus
    {
        Pending,      // Waiting to be processed
        Processing,   // Currently being processed  
        Completed,    // Successfully completed
        Failed,       // Failed (any reason)
        Cancelled,    // Cancelled by user/system
        Reversed      // Reversed after completion
    }
}
