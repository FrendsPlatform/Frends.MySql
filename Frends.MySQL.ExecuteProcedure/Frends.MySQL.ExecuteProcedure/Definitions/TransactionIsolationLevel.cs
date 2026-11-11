namespace Frends.MySQL.ExecuteProcedure.Definitions;

/// <summary>
/// Transaction isolation level.
/// </summary>
public enum TransactionIsolationLevel
{
    /// <summary>
    /// Uses the ReadCommitted transaction isolation level.
    /// </summary>
    ReadCommitted,

    /// <summary>
    /// Uses the Serializable transaction isolation level.
    /// </summary>
    Serializable,

    /// <summary>
    /// Uses the ReadUncommitted transaction isolation level.
    /// </summary>
    ReadUncommitted,

    /// <summary>
    /// Uses the RepeatableRead transaction isolation level.
    /// </summary>
    RepeatableRead,
}
