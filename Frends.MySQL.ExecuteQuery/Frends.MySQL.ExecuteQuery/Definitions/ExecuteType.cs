namespace Frends.MySQL.ExecuteQuery.Definitions;

/// <summary>
/// Type of query to execute.
/// </summary>
public enum ExecuteType
{
    /// <summary>
    /// Tries to determine the type of query to execute between NonQuery and Reader.
    /// Uses ExecuteReader when the command contains "select", ExecuteNonQuery otherwise.
    /// Backward compatible with task version 1.3.0
    /// </summary>
    Auto = 1,

    /// <summary>
    /// Runs ExecuteNonQuery method.
    /// Use this when you care only about how many rows were affected. (e.g., UPDATE)
    /// </summary>
    NonQuery = 2,

    /// <summary>
    /// Runs ExecuteScalar method.
    /// Use this when you want to return a single value from the database. (e.g., COUNT)
    /// </summary>
    Scalar = 3,

    /// <summary>
    /// Runs ExecuteReader method.
    /// Use this when you want to return multiple values from the database. (e.g., SELECT)
    /// </summary>
    Reader = 4,
}
