using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.MySQL.ExecuteProcedure.Definitions;

/// <summary>
/// MySQL connection settings.
/// </summary>
public class Connection
{
    /// <summary>
    /// MySQL connection string.
    /// </summary>
    /// <example>server=localhost;user=app;database=orders;</example>
    [PasswordPropertyText]
    [DefaultValue("server=localhost;user=app;database=orders;")]
    [Required]
    public string ConnectionString { get; set; }
}
