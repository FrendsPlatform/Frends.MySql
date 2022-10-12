using System.ComponentModel;
namespace Frends.MySQL.ExecuteQuery.Definitions;

/// <summary>
/// Options parameters.
/// </summary>
public class Options
{
    /// <summary>
    /// Timeout value in seconds
    /// </summary>
    /// <example>30</example>
    [DefaultValue(30)]
    public int TimeoutSeconds { get; set; }

    /// <summary>
    /// Transaction isolation level to use.
    /// </summary>
    /// <example>Default</example>
    public MySqlTransactionIsolationLevel MySqlTransactionIsolationLevel { get; set; }
}