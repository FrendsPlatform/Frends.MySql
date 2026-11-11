using System.ComponentModel;
using System.Data;
using Frends.MySQL.ExecuteQuery.Definitions;
using Frends.MySQL.ExecuteQuery.Helpers;
using MySqlConnector;
using Newtonsoft.Json.Linq;

namespace Frends.MySQL.ExecuteQuery;

/// <summary>
/// MySQL ExecuteQuery task.
/// </summary>
public static class MySQL
{
    /// <summary>
    /// Execute queries against the MySql database and return result of query in JToken.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.MySQL.ExecuteQuery)
    /// </summary>
    /// <param name="input">Essential parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, JToken ResultJtoken, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static async Task<Result> ExecuteQuery(
        [PropertyTab] Input input,
        [PropertyTab] Connection connection,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        var result = new Result
        {
            Success = true,
        };

        try
        {
            await using var conn = new MySqlConnection(connection.ConnectionString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            await using var command = new MySqlCommand();
            command.Connection = conn;
            command.CommandText = input.CommandText;

            if (input.Parameters != null)
            {
                command.CreateParameter();

                foreach (var parameter in input.Parameters)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    command.Parameters.Add(new MySqlParameter(parameter.Name, parameter.Value));
                }
            }

            command.CommandTimeout = options.TimeoutSeconds;

            var isolationLevel = options.MySqlTransactionIsolationLevel switch
            {
                MySqlTransactionIsolationLevel.ReadCommitted => IsolationLevel.ReadCommitted,
                MySqlTransactionIsolationLevel.ReadUncommitted => IsolationLevel.ReadUncommitted,
                MySqlTransactionIsolationLevel.RepeatableRead => IsolationLevel.RepeatableRead,
                MySqlTransactionIsolationLevel.Serializable => IsolationLevel.Serializable,
                _ => IsolationLevel.RepeatableRead,
            };
            await using var transaction = await conn.BeginTransactionAsync(isolationLevel, cancellationToken);
            command.Transaction = transaction;

            switch (options.ExecuteType)
            {
                case ExecuteType.Auto:
                    if (input.CommandText.ToLower().Contains("select"))
                    {
                        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                        var rows = new List<Dictionary<string, object>>();

                        while (await reader.ReadAsync(cancellationToken))
                        {
                            var row = new Dictionary<string, object>(reader.FieldCount);
                            for (var i = 0; i < reader.FieldCount; i++)
                                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                            rows.Add(row);
                        }

                        result.ResultJtoken = JToken.FromObject(rows);
                    }
                    else
                    {
                        var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
                        result.ResultJtoken = JToken.FromObject(affectedRows);
                    }

                    break;
                case ExecuteType.NonQuery:
                    {
                        var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
                        result.ResultJtoken = JToken.FromObject(new
                        {
                            AffectedRows = affectedRows,
                        });

                        break;
                    }

                case ExecuteType.Scalar:
                    {
                        var row = await command.ExecuteScalarAsync(cancellationToken);
                        result.ResultJtoken = JToken.FromObject(row ?? string.Empty);

                        break;
                    }

                case ExecuteType.Reader:
                    {
                        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                        var rows = new List<Dictionary<string, object>>();

                        while (await reader.ReadAsync(cancellationToken))
                        {
                            var row = new Dictionary<string, object>(reader.FieldCount);
                            for (var i = 0; i < reader.FieldCount; i++)
                                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                            rows.Add(row);
                        }

                        result.ResultJtoken = JToken.FromObject(rows);

                        break;
                    }

                default:
                    throw new ArgumentOutOfRangeException(nameof(options), "Invalid ExecuteType.");
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return ex.Handle(options);
        }

        return result;
    }
}
