using Frends.MySQL.ExecuteProcedure.Definitions;
using Microsoft.Xrm.Sdk.Messages;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Frends.MySQL.ExecuteQuery;
/// <summary>
/// Tasks class.
/// </summary>
public class MySQL
{

    [SuppressMessage("Security",
        "CA2100:Review SQL queries for security vulnerabilities", Justification =
            "One is able to write queries in FRENDS. It is up to a FRENDS process prevent injections.")]
    /// <summary>
    /// Execute a stored procedure to MySQL.
    /// [Documentation](https://tasks.frends.com/tasks#frends-tasks/Frends.MySQL.ExecuteProcedure)
    /// </summary>
    /// <param name="input"></param>
    /// <param name="options"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Object { bool Success, string Message, JToken Result }</returns>
    public static async Task<Result> ExecuteProcedure(
        [PropertyTab] Input input,
        [PropertyTab] Options options,
        CancellationToken cancellationToken
    )
    {
        var scalarReturnQueries = new[] { "update ", "insert ", "drop ", "truncate ", "create ", "alter " };

        try
        {
            using (var conn = new MySqlConnection(input.ConnectionString))
            {
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();

                IDictionary<string, object> parameterObject = new ExpandoObject();
                if (input.Parameters != null)
                {
                    foreach (var parameter in input.Parameters)
                    {
                        parameterObject.Add(parameter.Name, parameter.Value);
                    }

                }

                using (var command = new MySqlCommand(input.Query, conn))
                {
                    command.CommandTimeout = options.TimeoutSeconds;
                    command.CommandType = CommandType.StoredProcedure;

                    IsolationLevel isolationLevel;
                    switch (options.TransactionIsolationLevel)
                    {
                        case TransactionIsolationLevel.ReadCommitted:
                            isolationLevel = IsolationLevel.ReadCommitted;
                            break;
                        case TransactionIsolationLevel.ReadUncommitted:
                            isolationLevel = IsolationLevel.ReadUncommitted;
                            break;
                        case TransactionIsolationLevel.RepeatableRead:
                            isolationLevel = IsolationLevel.RepeatableRead;
                            break;
                        case TransactionIsolationLevel.Serializable:
                            isolationLevel = IsolationLevel.Serializable;
                            break;
                        default:
                            isolationLevel = IsolationLevel.RepeatableRead;
                            break;
                    }


                    if (scalarReturnQueries.Any(input.Query.TrimStart().ToLower().Contains) || command.CommandType == CommandType.StoredProcedure)
                    {
                        // scalar return
                        using (var trans = conn.BeginTransaction(isolationLevel))
                        {
                            try
                            {
                                var command1 = conn.CreateCommand();
                                command1.CommandTimeout = command.CommandTimeout;
                                command1.CommandText = input.Query;
                                command1.CommandType = command.CommandType;
                                var affectedRows = await conn.ExecuteAsync(command1, parameterObject, trans);

                                trans.Commit();

                                return JToken.FromObject(affectedRows);

                            }
                            catch (Exception ex)
                            {
                                trans.Rollback();
                                trans.Dispose();
                                throw new Exception("Query failed " + ex.Message);

                            }

                        }
                    }

                    using (var trans = conn.BeginTransaction(isolationLevel))
                    {
                        try
                        {
                            var result = await conn.QueryAsync(input.Query, parameterObject, trans, command.CommandTimeout, command.CommandType)
                                .ConfigureAwait(false);

                            trans.Commit();

                            return JToken.FromObject(result);
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
