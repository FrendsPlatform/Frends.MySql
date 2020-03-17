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
                [PropertyTab]InputQuery query,
                [PropertyTab] Options options,
                CancellationToken cancellationToken)
            {
                return await GetMySqlCommandResult(query.CommandText, query.ConnectionString, query.Parameters, options, query.CommandType, MySqlCommandMethod.ExecuteQuery, cancellationToken);
            }


        /// <summary>
        /// Task for performing queries in Oracle databases. See documentation at https://github.com/CommunityHiQ/Frends.Community.Oracle.Query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Object { bool Success, string Message, JToken Result }</returns>
        public static async Task<QueryOutput> ExecuteScalar(
            [PropertyTab]InputQuery query,
            [PropertyTab] Options options,
            CancellationToken cancellationToken)
        {
            return await GetMySqlCommandResult(query.CommandText, query.ConnectionString, query.Parameters, options, query.CommandType, MySqlCommandMethod.ExecuteScalar, cancellationToken);
        }

        /// <summary>
        /// Task for performing queries in Oracle databases. See documentation at https://github.com/CommunityHiQ/Frends.Community.Oracle.Query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Object { bool Success, string Message, JToken Result }</returns>
        public static async Task<QueryOutput> ExecuteNonQuery(
            [PropertyTab]InputQuery query,
            [PropertyTab] Options options,
            CancellationToken cancellationToken)
        {
            return await GetMySqlCommandResult(query.CommandText, query.ConnectionString, query.Parameters, options, query.CommandType, MySqlCommandMethod.ExecuteNonQuery, cancellationToken);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security",
                "CA2100:Review SQL queries for security vulnerabilities", Justification =
                    "One is able to write quereis in FRENDS. It is up to a FRENDS process prevent injections.")]
            private static async Task<QueryOutput> GetMySqlCommandResult(
                string query, string connectionString, IEnumerable<Parameter> parameters,
                Options options, 
                MySqlCommandType commandType, 
                MySqlCommandMethod commandMethod,
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

                            // check for command parameters and set them
                            if (parameters != null)
                                command.Parameters.AddRange(parameters.Select(CreateMySqlParameter)
                                    .ToArray());

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
                                // declare Result object
                                // var queryResult = await command.ToJTokenAsync(cancellationToken);

                                var queryResult = await CallRightFunction(command, commandMethod, cancellationToken);
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
                                    // var queryResult = await command.ToJTokenAsync(cancellationToken);
                                    var queryResult = await CallRightFunction(command, commandMethod, cancellationToken);

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

            private static async Task<JToken> CallRightFunction(this MySqlCommand command, MySqlCommandMethod commandMethod,
                CancellationToken cancellationToken)
            {
                var queryResult = JToken.FromObject(new JObject());
                switch (commandMethod)
                {
                    case MySqlCommandMethod.ExecuteQuery:
                    //command.CommandType = CommandType.StoredProcedure;
                    queryResult = await command.ExecuteQueryWithReaderAsync(cancellationToken);
                    break;
                case MySqlCommandMethod.ExecuteNonQuery:
                    queryResult = await command.ExecuteNonQueryAsync(cancellationToken);
                    break;

                case MySqlCommandMethod.ExecuteScalar:
                    queryResult = JToken.FromObject(await command.ExecuteScalarAsync(cancellationToken));
                    break;
                }
                return queryResult;
            }

            private static async Task<JToken> ExecuteQueryWithReaderAsync(this MySqlCommand command , CancellationToken cancellationToken)
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
