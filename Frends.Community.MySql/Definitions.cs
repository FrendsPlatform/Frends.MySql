using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#pragma warning disable 1591

namespace Frends.MySql
{

    /// <summary>
    /// Gets or sets a value indicating how the CommandText property is to be interpreted. https://dev.mysql.com/doc/dev/connector-net/8.0/html/P_MySql_Data_MySqlClient_MySqlCommand_CommandType.htm
    /// </summary>
    public enum MySqlCommandType { Text = 1, StoredProcedure = 4}

    /// <summary>
    /// Use ExecuteQueryAsync, ExecuteNonQueryAsync, ExecuteScalarAsync function from https://dev.mysql.com/doc/dev/connector-net/6.10/html/Methods_T_MySql_Data_MySqlClient_MySqlCommand.htm to execute the query.
    /// </summary>
    public enum MySqlCommandMethod { ExecuteQuery, ExecuteNonQuery, ExecuteScalar }

    /// <summary>
    /// Transaction isolation level to use: https://dev.mysql.com/doc/refman/8.0/en/innodb-transaction-isolation-levels.html
    /// </summary>
    public enum MySqlTransactionIsolationLevel { Default, ReadCommitted, None, Serializable, ReadUncommitted, RepeatableRead}


    public class InputQuery
    {
        /// <summary>
        /// Mysql connection string
        /// </summary>
        [PasswordPropertyText]
        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue("server=server;user=user;database=db;password=pw;")]
        public string ConnectionString { get; set; }

        /// <summary>
        /// Query type, Text or StoredProcedure
        /// </summary>
        public MySqlCommandType CommandType { get; set; }

        /// <summary>
        ///  SQL statement to execute at the data source. Usually query or name of a stored procedure. https://dev.mysql.com/doc/dev/connector-net/8.0/html/P_MySql_Data_MySqlClient_MySqlCommand_CommandText.htm
        /// </summary>
        [DisplayFormat(DataFormatString = "Sql")]
        [DefaultValue("SELECT ColumnName FROM TableName")]
        public string CommandText { get; set; }

        /// <summary>
        /// Parameters for the database query
        /// </summary>
        public Parameter[] Parameters { get; set; }
    }

    /// <summary>
    /// Set properties of parameters. More info https://dev.mysql.com/doc/dev/connector-net/8.0/html/T_MySql_Data_MySqlClient_MySqlParameterCollection.htm
    /// </summary>
    public class Parameter
    {
        /// <summary>
        /// The name of the parameter
        /// </summary>
        [DefaultValue("ParameterName")]
        [DisplayFormat(DataFormatString = "Text")]
        public string Name { get; set; }

        /// <summary>
        /// The value of the parameter
        /// </summary>
        [DefaultValue("Parameter value")]
        [DisplayFormat(DataFormatString = "Text")]
        public dynamic Value { get; set; }
    }

    public class Options
    {

        /// <summary>
        /// Timeout value in seconds
        /// </summary>
        [DefaultValue(30)]
        public int TimeoutSeconds { get; set; }

        /// <summary>
        /// Choose if error should be thrown if Task fails.
        /// Otherwise returns Object {Success = false }
        /// </summary>
        [DefaultValue(true)]
        public bool ThrowErrorOnFailure { get; set; }

        /// <summary>
        /// Transaction isolation level to use.
        /// </summary>
        public MySqlTransactionIsolationLevel MySqlTransactionIsolationLevel { get; set; }
}

    /// <summary>
    /// Result to be returned from task
    /// </summary>
    public class QueryOutput
    {
        public dynamic Result { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }

    }

}