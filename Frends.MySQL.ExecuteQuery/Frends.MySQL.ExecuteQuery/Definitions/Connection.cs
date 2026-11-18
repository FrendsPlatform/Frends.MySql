using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.MySQL.ExecuteQuery.Definitions;

/// <summary>
/// Connection parameters.
/// </summary>
public class Connection
{
    /// <summary>
    /// Mysql connection string.
    /// </summary>
    /// <example>server=server;user=user;database=db;******;</example>
    [PasswordPropertyText]
    [DisplayFormat(DataFormatString = "Text")]
    public string ConnectionString { get; set; }
}
