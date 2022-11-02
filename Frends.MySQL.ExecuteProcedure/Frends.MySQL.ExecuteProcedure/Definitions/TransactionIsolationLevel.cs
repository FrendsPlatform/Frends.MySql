namespace Frends.MySQL.ExecuteProcedure.Definitions;

/// <summary>
/// Transaction isolation level
/// </summary>
public enum TransactionIsolationLevel
{
    /// <summary>
    /// Uses ReadCommited level of transaction
    /// </summary>
    ReadCommitted,
    /// <summary>
    /// Uses Serializable level of transaction
    /// </summary>
    Serializable,
    /// <summary>
    /// Uses ReadUncommitted level of transaction
    /// </summary>
    ReadUncommitted,
    /// <summary>
    /// Uses RepeatableRead level of transaction
    /// </summary>
    RepeatableRead
}
