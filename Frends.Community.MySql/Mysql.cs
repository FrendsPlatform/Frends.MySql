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

namespace Frends.MySql{
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
            try
            {
                using (var c = new MySqlConnection(connectionString))
                {
                    await c.OpenAsync(cancellationToken);

                    using (var command = new MySqlCommand(query, c))
                    {
                        command.CommandTimeout = options.TimeoutSeconds;

                        IDictionary<string, object> parameterObject = new ExpandoObject();

                        if (parameters != null)
                        {
                            foreach (var parameter in parameters)
                            {
                                parameterObject.Add(parameter.Name, parameter.Value);
                            }

                        }

                        else if (commandType == MySqlCommandType.Text)
                        {
                            command.CommandType = CommandType.Text;

                        }
                        else if (commandType == MySqlCommandType.StoredProcedure)
                        {
                            command.CommandType = CommandType.StoredProcedure;
                        }

                        if (options.MySqlTransactionIsolationLevel == MySqlTransactionIsolationLevel.None)
                        {
                            using (var result = await command.Connection.ExecuteReaderAsync(
                                    command.CommandText,
                                    parameterObject,
                                    commandType: command.CommandType,
                                    commandTimeout: command.CommandTimeout)
                                .ConfigureAwait(false))

                            {
                                var table = new DataTable();
                                table.Load(result);
                                var queryResult = JToken.FromObject(table);
                                return new QueryOutput {Success = true, Result = queryResult};
                            }
                        }
                        else
                        {
                            var transaction =
                                options.MySqlTransactionIsolationLevel == MySqlTransactionIsolationLevel.Default
                                    ? c.BeginTransaction()
                                    : c.BeginTransaction(options.MySqlTransactionIsolationLevel
                                        .GetMySqlTransactionIsolationLevel());

                            command.Transaction = transaction;

                            // declare Result object
                            try
                            {
                                using (var result = await command.Connection.ExecuteReaderAsync(
                                        command.CommandText,
                                        parameterObject,
                                        commandType: command.CommandType,
                                        commandTimeout: command.CommandTimeout)
                                    .ConfigureAwait(false))

                                {
                                    var table = new DataTable();
                                    table.Load(result);
                                    var queryResult = JToken.FromObject(table);
                                    transaction.Commit();

                                    return new QueryOutput {Success = true, Result = queryResult};
                                }

                            }
                            catch (MySqlException ex)
                            {
                                try
                                {
                                    transaction.Rollback();
                                }
                                catch
                                {
                                    if (transaction.Connection != null)
                                    {

                                        throw new Exception("An exception of type " + ex.GetType() +
                                                            " was encountered while attempting to roll back the transaction. Some data might be modified in the database.");
                                    }

                                    throw;
                                }

                                throw;
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