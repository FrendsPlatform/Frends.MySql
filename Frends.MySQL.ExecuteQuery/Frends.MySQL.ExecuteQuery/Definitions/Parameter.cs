using System.ComponentModel.DataAnnotations;

namespace Frends.MySQL.ExecuteQuery.Definitions;

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
