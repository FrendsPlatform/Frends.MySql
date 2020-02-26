using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            public static async Task<QueryOutput> Query(
                [PropertyTab]InputQuery query,
                [PropertyTab] Options options,
                CancellationToken cancellationToken)
            {
                return await GetMySqlCommandResult(query.Query, query.ConnectionString, query.Parameters, options, MySqlCommandType.Text, cancellationToken);
            }

            public static async Task<QueryOutput> ExecuteStoredProcedure(
                [PropertyTab]InputProcedure execute,
                [PropertyTab] Options options,
                CancellationToken cancellationToken)
            {
                return await GetMySqlCommandResult(execute.Execute, execute.ConnectionString, execute.Parameters, options, MySqlCommandType.StoredProcedure, cancellationToken);
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
                        try
                        {
                            await c.OpenAsync(cancellationToken);

                            using (var command = new MySqlCommand(query, c))
                            {
                                command.CommandTimeout = options.TimeoutSeconds;

                                // check for command parameters and set them
                                if (parameters != null)
                                    command.Parameters.AddRange(parameters.Select(CreateMySqlParameter)
                                        .ToArray());

                                else if (commandType == MySqlCommandType.Text)
                                {
                                    command.CommandType = CommandType.Text;

                                }
                                else if (commandType == MySqlCommandType.Text)
                                {
                                    command.CommandType = CommandType.StoredProcedure;
                                }

                                if (options.MySqlTransactionIsolationLevel == MySqlTransactionIsolationLevel.None)
                                {
                                    // declare Result object
                                    var queryResult = await command.ToJTokenAsync(cancellationToken);
                                    return new QueryOutput {Success = true, Result = queryResult};
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
                                        var queryResult = await command.ToJTokenAsync(cancellationToken);
                                        return new QueryOutput {Success = true, Result = queryResult};

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
                        finally
                        {
                            // Close connection
                            c.Dispose();
                            c.Close();
                            MySqlConnection.ClearPool(c);
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

        private static async Task<JToken> ToJTokenAsync(this MySqlCommand command , CancellationToken cancellationToken)
        {
            using (var reader = await command.ExecuteReaderAsync(cancellationToken) as MySqlDataReader)
            {
                using (var writer = new JTokenWriter() as JsonWriter)
                {
                    // start array
                    await writer.WriteStartArrayAsync(cancellationToken);

                    while (reader != null && reader.Read())
                    {
                        // start row object
                        await writer.WriteStartObjectAsync(cancellationToken);

                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            // add row element name
                            await writer.WritePropertyNameAsync(reader.GetName(i), cancellationToken);
                            await writer.WriteValueAsync(reader.GetValue(i) ?? string.Empty, cancellationToken);
                        }
                        await writer.WriteEndObjectAsync(cancellationToken); // end row object
                    }
                    // end array
                    await writer.WriteEndArrayAsync(cancellationToken);

                    return ((JTokenWriter)writer).Token;
                }
            }
        }

        /// <summary>
        /// Mysql parameters.
        /// </summary>
        private static MySqlParameter CreateMySqlParameter(Parameter parameter)
            {
                return new MySqlParameter()
                {
                    ParameterName = parameter.Name,
                    Value = parameter.Value
                };
            }
        }
}
