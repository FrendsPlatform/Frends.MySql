using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
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
            /// <param name="connection"></param>
            /// <param name="options"></param>
            /// <param name="cancellationToken"></param>
            /// <returns>Object { bool Success, string Message, Jtoken Result }</returns>
            public static async Task<Output> Query(
                [PropertyTab] QueryProperties query,
                [PropertyTab] ConnectionProperties connection,
                [PropertyTab] Options options,
                CancellationToken cancellationToken)
            {
                try
                {
                    using (var c = new MySqlConnection(connection.ConnectionString))
                    {
                        try
                        {
                            await c.OpenAsync(cancellationToken);

                            using (var command = new MySqlCommand(query.Query, c))
                            {
                                command.CommandTimeout = connection.TimeoutSeconds;
                                //command.BindByNew = true; // is this xmlCommand specific?

                                // check for command parameters and set them
                                if (query.Parameters != null)
                                    command.Parameters.AddRange(query.Parameters.Select(p => CreateMySqlParameter(p)).ToArray());

                                // declare Result object

                                command.CommandType = CommandType.Text;




                                var queryResult = await command.ToJtokenAsync(cancellationToken);
                                    return new Output { Success = true, Result = queryResult };

                            

                            /*
                                // set commandType according to ReturnType
                                switch (output.ReturnType)
                                {
                                    case QueryReturnType.Xml:
                                        queryResult = await command.ToXmlAsync(output, cancellationToken);
                                        break;
                                    case QueryReturnType.Json:
                                        queryResult = await command.ToJsonAsync(output, cancellationToken);
                                        break;
                                    case QueryReturnType.Csv:
                                        queryResult = await command.ToCsvAsync(output, cancellationToken);
                                        break;
                                    default:
                                        throw new ArgumentException("Task 'Return Type' was invalid! Check task properties.");
                                }

                                return new Output { Success = true, Result = queryResult };

    */
                        }
                        
                        }
                        catch (Exception ex)
                        {
                            throw ex;
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
                        throw ex;
                    return new Output
                    {
                        Success = false,
                        Message = ex.Message
                    };
                }
            }


        /// <summary>
        /// Write query results to json string or file
        /// </summary>
        /// <param name="command"></param>
        /// <param name="output"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private static async Task<JToken> ToJtokenAsync(this MySqlCommand command, CancellationToken cancellationToken)
        {
            command.CommandType = CommandType.Text;


            using (var reader = await command.ExecuteReaderAsync(cancellationToken) as MySqlDataReader)
            {
                var culture = CultureInfo.InvariantCulture;

                using (var writer = new JTokenWriter() as JsonWriter)
                {
                    // start array
                    await writer.WriteStartArrayAsync(cancellationToken);

                    cancellationToken.ThrowIfCancellationRequested();

                    while (reader != null && reader.Read())
                    {
                        // start row object
                        await writer.WriteStartObjectAsync(cancellationToken);

                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            // add row element name
                            await writer.WritePropertyNameAsync(reader.GetName(i), cancellationToken);

                            await writer.WriteValueAsync(reader.GetValue(i) ?? string.Empty, cancellationToken);

                            cancellationToken.ThrowIfCancellationRequested();
                        }

                        await writer.WriteEndObjectAsync(cancellationToken); // end row object

                        cancellationToken.ThrowIfCancellationRequested();
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
        private static MySqlParameter CreateMySqlParameter(QueryParameter parameter)
            {
                return new MySqlParameter()
                {
                    ParameterName = parameter.Name,
                    Value = parameter.Value,
                    MySqlDbType = parameter.DataType.ConvertEnum<MySqlDbType>()
                };
            }


        }
}
