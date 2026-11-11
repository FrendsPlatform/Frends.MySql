using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.MySQL.ExecuteProcedure.Definitions;

/// <summary>
/// Input parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// MySQL connection string
    /// </summary>
    /// <example>server=localhost;user=app;database=orders.</example>
    [PasswordPropertyText]
    [DefaultValue("server=localhost;user=app;database=orders.")]
    [Required]
    public string ConnectionString { get; set; }

    /// <summary>
    /// The SQL statement or stored procedure name to execute.
    /// </summary>
    /// <example>SELECT OrderId FROM Orders WHERE OrderId = 1.</example>
    [DisplayFormat(DataFormatString = "Sql")]
    [DefaultValue("ProcedureName")]
    [Required]
    public string Query { get; set; }

    /// <summary>
    /// Parameters for the database query
    /// </summary>
    /// <example>CustomerId: 123.</example>
    public Parameter[] Parameters { get; set; }
}
