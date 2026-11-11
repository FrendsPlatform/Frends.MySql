using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.MySQL.ExecuteProcedure.Definitions;

/// <summary>
/// Input parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Gets or sets the MySQL connection string.
    /// </summary>
    /// <example>server=localhost;user=app;database=orders.</example>
    [PasswordPropertyText]
    [DefaultValue("server=localhost;user=app;database=orders.")]
    public string ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the SQL statement or stored procedure name to execute.
    /// </summary>
    /// <example>SELECT OrderId FROM Orders WHERE OrderId = 1.</example>
    [DisplayFormat(DataFormatString = "Sql")]
    [DefaultValue("ProcedureName")]
    public string Query { get; set; }

    /// <summary>
    /// Gets or sets the parameters for the database query.
    /// </summary>
    /// <example>CustomerId: 123.</example>
    public Parameter[] Parameters { get; set; }
}
