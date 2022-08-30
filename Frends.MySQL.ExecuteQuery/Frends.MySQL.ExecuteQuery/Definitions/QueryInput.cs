using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.MySQL.ExecuteQuery.Definitions;

/// <summary>
/// Query input.
/// </summary>
public class QueryInput
{
    /// <summary>
    /// Mysql connection string.
    /// </summary>
    /// <example>server=server;user=user;database=db;password=pw;</example>
    [PasswordPropertyText]
    [DisplayFormat(DataFormatString = "Text")]
    public string ConnectionString { get; set; }

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

/// <summary>
/// Set properties of parameters.
/// </summary>
public class Parameter
{
    /// <summary>
    /// The name of the parameter
    /// </summary>
    /// <example>foo</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string Name { get; set; }

    /// <summary>
    /// The value of the parameter
    /// </summary>
    /// <example>bar</example>
    [DisplayFormat(DataFormatString = "Text")]
    public dynamic Value { get; set; }
}