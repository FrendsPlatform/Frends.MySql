using System.ComponentModel;

namespace Frends.MySQL.ExecuteProcedure.Definitions;

/// <summary>
/// Additional execution options.
/// </summary>
public class Options
{
    /// <summary>
    /// Gets or sets the command timeout in seconds.
    /// </summary>
    /// <example>30.</example>
    [DefaultValue(30)]
    public int TimeoutSeconds { get; set; }

    /// <summary>
    /// Gets or sets the transaction isolation level to use.
    /// </summary>
    /// <example>ReadCommitted.</example>
    [DefaultValue(TransactionIsolationLevel.RepeatableRead)]
    public TransactionIsolationLevel TransactionIsolationLevel { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to throw an error on failure.
    /// </summary>
    /// <example>true.</example>
    [DefaultValue(true)]
    public bool ThrowErrorOnFailure { get; set; } = true;

    /// <summary>
    /// Gets or sets the error message prefix on failure.
    /// </summary>
    /// <example>Stored procedure execution failed.</example>
    [DefaultValue("")]
    public string ErrorMessageOnFailure { get; set; } = string.Empty;
}
