using Frends.MySQL.ExecuteProcedure.Definitions;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Frends.MySQL.ExecuteProcedure
{
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
        /// <returns>Object { int AffectedRows }</returns>
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
                                    command.Connection = conn;
                                    command.Transaction = trans;

                                    command.CommandText = input.Query;
                                    foreach (var value in parameterObject)
                                    {
                                        command.Parameters.AddWithValue(value.Key, value.Value);
                                    }

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
                        else
                        {
                            using (var trans = conn.BeginTransaction(isolationLevel))
                            {
                                try
                                {
                                    command.Connection = conn;
                                    command.Transaction = trans;

                                    command.CommandText = input.Query;
                                    foreach (var value in parameterObject)
                                    {
                                        command.Parameters.AddWithValue(value.Key, value.Value);
                                    }

                                    var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

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
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
