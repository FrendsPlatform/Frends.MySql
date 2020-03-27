using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.Runtime.CompilerServices;

namespace Frends.MySql
{
    /// <summary>
    /// Example task package for handling files
    /// </summary>
    public static class QueryTask
    {


        /// <summary>
        /// Task for performing queries in Oracle databases. See documentation at https://github.com/CommunityHiQ/Frends.Community.Oracle.Query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Object { bool Success, string Message, JToken Result }</returns>
        public static async Task<QueryOutput> ExecuteQuery(
            [PropertyTab] InputQuery query,
            [PropertyTab] Options options,
            CancellationToken cancellationToken)
        {
            return await GetMySqlCommandResult(query.CommandText, query.ConnectionString, query.Parameters, options,
                MySqlCommandType.Text, cancellationToken);
        }

        /// <summary>
        /// Task for performing queries in Oracle databases. See documentation at https://github.com/CommunityHiQ/Frends.Community.Oracle.Query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Object { bool Success, string Message, JToken Result }</returns>
        public static async Task<QueryOutput> ExecuteProcedure(
            [PropertyTab] InputQuery query,
            [PropertyTab] Options options,
            CancellationToken cancellationToken)
        {
            return await GetMySqlCommandResult(query.CommandText, query.ConnectionString, query.Parameters, options,
                MySqlCommandType.StoredProcedure, cancellationToken);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security",
            "CA2100:Review SQL queries for security vulnerabilities", Justification =
                "One is able to write quereis in FRENDS. It is up to a FRENDS process prevent injections.")]
        private static async Task<QueryOutput> GetMySqlCommandResult(
            string query, string connectionString, IEnumerable<Parameter> parameters,
            Options options,
            MySqlCommandType commandType,
            CancellationToken cancellationToken)
        {

            var scalarReturnQueries = new[] { "update ", "insert ", "drop ", "truncate " };

            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
                    cancellationToken.ThrowIfCancellationRequested();

                    IDictionary<string, object> parameterObject = new ExpandoObject();
                    if (parameters != null)
                    {
                        foreach (var parameter in parameters)
                        {
                            parameterObject.Add(parameter.Name, parameter.Value);
                        }

                    }

                    using (var command = new MySqlCommand(query, conn))
                    {
                        command.CommandTimeout = options.TimeoutSeconds;

                        switch (commandType)
                        {
                            case MySqlCommandType.Text:
                                command.CommandType = CommandType.Text;
                                break;
                            case MySqlCommandType.StoredProcedure:
                                command.CommandType = CommandType.StoredProcedure;
                                break;
                        }

                        IsolationLevel isolationLevel;
                        switch (options.MySqlTransactionIsolationLevel)
                        {
                            case MySqlTransactionIsolationLevel.ReadCommitted:
                                isolationLevel = IsolationLevel.ReadCommitted;
                                break;
                            case MySqlTransactionIsolationLevel.ReadUncommitted:
                                isolationLevel = IsolationLevel.ReadUncommitted;
                                break;
                            case MySqlTransactionIsolationLevel.RepeatableRead:
                                isolationLevel = IsolationLevel.RepeatableRead;
                                break;
                            case MySqlTransactionIsolationLevel.Serializable:
                                isolationLevel = IsolationLevel.Serializable;
                                break;
                            default:
                                isolationLevel = IsolationLevel.Unspecified; // MySqlTransactionIsolationLevel.none, default
                                break;
                        }

                        switch (isolationLevel)
                        {
                            case IsolationLevel.Unspecified:
                                if (scalarReturnQueries.Any(query.Contains) || command.CommandType == CommandType.StoredProcedure)
                                {
                                    using (var trans = conn.BeginTransaction())
                                    {
                                        try
                                        {
                                            var affectedRows = conn.Execute(query, parameterObject, trans,
                                                command.CommandTimeout, command.CommandType);
                                                

                                            trans.Commit();

                                            return new QueryOutput { Success = true, Message = affectedRows.ToString() + " row(s) affected" };
                                        }
                                        catch (Exception ex)
                                        {
                                            trans.Rollback();
                                            trans.Dispose();
                                            if (options.ThrowErrorOnFailure)
                                                throw;
                                            return new QueryOutput
                                            {
                                                Success = false,
                                                Message = ex.Message
                                            };
                                        }

                                    }
                                }
                                else
                                {
                                    using (var trans = conn.BeginTransaction())
                                    {
                                        try
                                        {
                                            var result = await conn.QueryAsync(query, parameterObject, trans, command.CommandTimeout, command.CommandType)
                                                .ConfigureAwait(false);

                                            trans.Commit();

                                            var queryResult = JToken.FromObject(result);

                                            return new QueryOutput { Success = true, Result = queryResult };
                                        }
                                        catch (Exception ex)
                                        {
                                            trans.Rollback();
                                            trans.Dispose();
                                            if (options.ThrowErrorOnFailure)
                                                throw;
                                            return new QueryOutput
                                            {
                                                Success = false,
                                                Message = ex.Message
                                            };
                                        }

                                    }
                                }


                            default:

                                if (scalarReturnQueries.Any(query.Contains) || command.CommandType == CommandType.StoredProcedure)
                                {
                                    using (var trans = conn.BeginTransaction(isolationLevel))
                                    {
                                        try
                                        {
                                            var affectedRows = conn.Execute(query, parameterObject, trans,
                                                command.CommandTimeout, command.CommandType);
                                         

                                            trans.Commit();

                                            return new QueryOutput { Success = true, Message = affectedRows.ToString() + " row(s) affected" };
                                        }
                                        catch (Exception ex)
                                        {
                                            trans.Rollback();
                                            trans.Dispose();
                                            if (options.ThrowErrorOnFailure)
                                                throw;
                                            return new QueryOutput
                                            {
                                                Success = false,
                                                Message = ex.Message
                                            };
                                        }

                                    }
                                }
                                else
                                {
                                    using (var trans = conn.BeginTransaction(isolationLevel))
                                    {
                                        try
                                        {
                                            var result = await conn.QueryAsync(query, parameterObject, trans, command.CommandTimeout, command.CommandType)
                                                .ConfigureAwait(false);

                                            trans.Commit();

                                            var queryResult = JToken.FromObject(result);

                                            return new QueryOutput { Success = true, Result = queryResult };
                                        }
                                        catch (Exception ex)
                                        {
                                            trans.Rollback();
                                            trans.Dispose();
                                            if (options.ThrowErrorOnFailure)
                                                throw;
                                            return new QueryOutput
                                            {
                                                Success = false,
                                                Message = ex.Message
                                            };
                                        }

                                    }
                                }


                        }

                    }
                }
            }
            catch (Exception ex)
            {
                if (options.ThrowErrorOnFailure)
                    throw;
                return new QueryOutput
                {
                    Success = false,
                    Message = ex.Message
                };
            }


        }


    }


}

