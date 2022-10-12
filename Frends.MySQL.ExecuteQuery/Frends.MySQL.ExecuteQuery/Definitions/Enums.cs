namespace Frends.MySQL.ExecuteQuery.Definitions;

/// <summary>
/// MySql transaction isolation levels.
/// </summary>
public enum MySqlTransactionIsolationLevel
{
    /// <summary>
    /// This is the default isolation level for InnoDB. Consistent reads within the same transaction read the snapshot established by the first read. This means that if you issue several plain (nonlocking) SELECT statements within the same transaction, these SELECT statements are consistent also with respect to each other.
    /// </summary>
    RepeatableRead,

    /// <summary>
    /// Each consistent read, even within the same transaction, sets and reads its own fresh snapshot.
    /// </summary>
    ReadCommitted,

    /// <summary>
    /// SELECT statements are performed in a nonlocking fashion, but a possible earlier version of a row might be used. Thus, using this isolation level, such reads are not consistent. This is also called a dirty read. Otherwise, this isolation level works like READ COMMITTED.
    /// </summary>
    ReadUncommitted,

    /// <summary>
    /// This level is like REPEATABLE READ, but InnoDB implicitly converts all plain SELECT statements to SELECT ... FOR SHARE if autocommit is disabled. If autocommit is enabled, the SELECT is its own transaction. It therefore is known to be read only and can be serialized if performed as a consistent (nonlocking) read and need not block for other transactions. (To force a plain SELECT to block if other transactions have modified the selected rows, disable autocommit.)
    /// </summary>
    Serializable
}