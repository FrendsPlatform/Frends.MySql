namespace Frends.MySQL.ExecuteProcedure.Definitions;

/// <summary>
/// Result of the task.
/// </summary>
public class Result
{
    internal Result(int affectedRows)
    {
        AffectedRows = affectedRows;
        Success = true;
    }

    /// <summary>
    /// Value indicating whether the task completed successfully.
    /// </summary>
    /// <example>true.</example>
    public bool Success { get; set; }

    /// <summary>
    /// Count of affected rows.
    /// </summary>
    /// <example>10.</example>
    public int AffectedRows { get; private set; }

    /// <summary>
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; set; }
}
