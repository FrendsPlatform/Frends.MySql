using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.MySQL.ExecuteProcedure.Definitions;

/// <summary>
/// Stored procedure parameter.
/// </summary>
public class Parameter
{
    /// <summary>
    /// The name of the parameter
    /// </summary>
    /// <example>CustomerId.</example>
    [DefaultValue("ParameterName")]
    [DisplayFormat(DataFormatString = "Text")]
    public string Name { get; set; }

    /// <summary>
    /// The value of the parameter
    /// </summary>
    /// <example>123.</example>
    [DefaultValue("ParameterValue")]
    [DisplayFormat(DataFormatString = "Text")]
    public dynamic Value { get; set; }
}
