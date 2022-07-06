using System.ComponentModel;

namespace Frends.MySQL.ExecuteProcedure.Definitions;
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
    public TransactionIsolationLevel TransactionIsolationLevel { get; set; }
}
