using System;

namespace Frends.MySQL.ExecuteProcedure.Definitions;

/// <summary>
/// Error that occurred during the task.
/// </summary>
public class Error
{
    /// <summary>
    /// Gets or sets a summary of the error.
    /// </summary>
    /// <example>Stored procedure execution failed.</example>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets additional information about the error.
    /// </summary>
    /// <example>MySqlException: Procedure was not found.</example>
    public Exception AdditionalInfo { get; set; }
}
