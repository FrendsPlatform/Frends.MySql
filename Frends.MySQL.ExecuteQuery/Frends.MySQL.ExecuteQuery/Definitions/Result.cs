using Newtonsoft.Json.Linq;

namespace Frends.MySQL.ExecuteQuery.Definitions;

/// <summary>
/// Result.
/// </summary>
public class Result
{
    /// <summary>
    /// Task status.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; private set; }

    /// <summary>
    /// Result value(s).
    /// </summary>
    /// <example>[{"name": "foo", "value": 123}]</example>
    public dynamic ResultJtoken { get; private set; }

    internal Result(bool success, JToken resultJtoken)
    {
        Success = success;
        ResultJtoken = resultJtoken;
    }
}
