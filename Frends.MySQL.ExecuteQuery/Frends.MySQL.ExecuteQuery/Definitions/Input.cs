using System.ComponentModel.DataAnnotations;

namespace Frends.MySQL.ExecuteQuery.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// SQL statement to execute at the data source. Usually query or name of a stored procedure.
    /// </summary>
    /// <example>SELECT ColumnName FROM TableName</example>
    [DisplayFormat(DataFormatString = "Sql")]
    public string CommandText { get; set; }

    /// <summary>
    /// Parameters for the database query.
    /// </summary>
    /// <example>foo, bar</example>
    public Parameter[] Parameters { get; set; }
}
