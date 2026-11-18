using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

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
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Transaction isolation level to use.
    /// </summary>
    /// <example>Default</example>
    public MySqlTransactionIsolationLevel MySqlTransactionIsolationLevel { get; set; }

    /// <summary>
    /// Type of query execution.
    /// </summary>
    /// <example>Auto</example>
    [DefaultValue(ExecuteType.Auto)]
    public ExecuteType ExecuteType { get; set; } = ExecuteType.Auto;

    /// <summary>
    /// Whether to throw an error on failure.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool ThrowErrorOnFailure { get; set; } = true;

    /// <summary>
    /// Overrides the error message on failure.
    /// </summary>
    /// <example>Failed to execute the query</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string ErrorMessageOnFailure { get; set; } = string.Empty;
}
