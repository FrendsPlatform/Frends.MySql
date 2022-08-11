using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.MySQL.ExecuteProcedure.Definitions;

/// <summary>
/// Input class
/// </summary>
public class Input
{
    /// <summary>
    /// Mysql connection string
    /// </summary>
    /// <example>server=server;user=user;database=db;password=pw;</example>
    [PasswordPropertyText]
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("server=server;user=user;database=db;password=pw;")]
    public string ConnectionString { get; set; }

    /// <summary>
    ///  SQL statement to execute at the data source. Usually query or name of a stored procedure. https://dev.mysql.com/doc/dev/connector-net/8.0/html/P_MySql_Data_MySqlClient_MySqlCommand_CommandText.htm
    /// </summary>
    /// <example>SELECT ColumnName FROM TableName</example>
    [DisplayFormat(DataFormatString = "Sql")]
    [DefaultValue("ProcedureName")]
    public string Query { get; set; }

    /// <summary>
    /// Parameters for the database query
    /// </summary>
    public Parameter[] Parameters { get; set; }
}
