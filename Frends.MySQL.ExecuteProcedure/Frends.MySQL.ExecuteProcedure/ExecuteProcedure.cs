using Frends.MySQL.ExecuteProcedure.Definitions;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;

namespace Frends.MySQL.ExecuteProcedure;

/// <summary>
/// Task class.
/// </summary>
public class MySQL
{
    /// <summary>
    /// Execute a stored procedure to MySQL.
    /// [Documentation](https://tasks.frends.com/tasks#frends-tasks/Frends.MySQL.ExecuteProcedure)
    /// </summary>
    /// <param name="input"></param>
    /// <param name="options"></param>
    /// <param name="cancellationToken"/>
    /// <returns>Object { int AffectedRows }</returns>
    public static async Task<Result> ExecuteProcedure(
        [PropertyTab] Input input,
        [PropertyTab] Options options,
        CancellationToken cancellationToken
    )
    {
        try
        {
            using (var conn = new MySqlConnection(input.ConnectionString + "UseAffectedRows=true;"))
            {
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

                IDictionary<string, object> parameterObject = new ExpandoObject();
                if (input.Parameters != null)
                {
                    foreach (var parameter in input.Parameters)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        parameterObject.Add(parameter.Name, parameter.Value);
                    }
                }

                using (var command = new MySqlCommand(input.Query, conn))
                {
                    command.CommandTimeout = options.TimeoutSeconds;
                    command.CommandType = CommandType.StoredProcedure;

                    foreach (var value in parameterObject)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        command.Parameters.AddWithValue(value.Key, value.GetType()).Value = value.Value;
                    }

                    IsolationLevel isolationLevel;
                    switch (options.TransactionIsolationLevel)
                    {
                        case TransactionIsolationLevel.ReadCommitted:
                            isolationLevel = IsolationLevel.ReadCommitted;
                            break;
                        case TransactionIsolationLevel.ReadUncommitted:
                            isolationLevel = IsolationLevel.ReadUncommitted;
                            break;
                        case TransactionIsolationLevel.Serializable:
                            isolationLevel = IsolationLevel.Serializable;
                            break;
                        default:
                            isolationLevel = IsolationLevel.RepeatableRead;
                            break;
                    }

                    using (var trans = conn.BeginTransaction(isolationLevel))
                    {
                        try
                        {
                            var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
                            trans.Commit();

                            return new Result(affectedRows);
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            trans.Dispose();

                            throw new Exception("Query failed " + ex.Message);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

}
