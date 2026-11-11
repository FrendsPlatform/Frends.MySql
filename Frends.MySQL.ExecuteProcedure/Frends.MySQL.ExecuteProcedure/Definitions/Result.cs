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
    /// Gets or sets a value indicating whether the task completed successfully.
    /// </summary>
    /// <example>true.</example>
    public bool Success { get; set; }

    /// <summary>
    /// Gets the count of affected rows.
    /// </summary>
    /// <example>10.</example>
    public int AffectedRows { get; private set; }

    /// <summary>
    /// Gets or sets the error that occurred during task execution.
    /// </summary>
    /// <example>null.</example>
    public Error Error { get; set; }
}
