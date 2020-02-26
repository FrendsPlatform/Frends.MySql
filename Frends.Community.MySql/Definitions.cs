using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#pragma warning disable 1591

namespace Frends.MySql
{

    public enum MySqlCommandType { Text = 1, StoredProcedure = 4 }

    public enum MySqlTransactionIsolationLevel { Default, ReadCommitted, None, Serializable, ReadUncommitted, RepeatableRead}
    public class InputQuery
    {

        /// <summary>
        /// Mysql connection string
        /// </summary>
        [PasswordPropertyText]
        [DefaultValue("server=server;user=user;database=db;password=pw;")]
        public string ConnectionString { get; set; }

        /// <summary>
        /// Querry 
        /// </summary>
        [DisplayFormat(DataFormatString = "Sql")]
        [DefaultValue("SELECT ColumnName FROM TableName")]
        public string Query { get; set; }

        /// <summary>
        /// Parameters for the database query
        /// </summary>
        public Parameter[] Parameters { get; set; }
    }

    public class InputProcedure
    {

        /// <summary>
        /// Mysql connection string
        /// </summary>
        [PasswordPropertyText]
        [DefaultValue("server=server;user=user;database=db;password=pw;")]
        public string ConnectionString { get; set; }
        /// <summary>
        /// Querry 
        /// </summary>
        [DisplayFormat(DataFormatString = "Sql")]
        [DefaultValue("SELECT ColumnName FROM TableName")]
        public string Execute { get; set; }

        /// <summary>
        /// Parameters for the database query
        /// </summary>
        public Parameter[] Parameters { get; set; }
    }

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
        /// Choose if error should be thrown if Task failes.
        /// Otherwise returns Object {Success = false }
        /// </summary>
        [DefaultValue(true)]
        public bool ThrowErrorOnFailure { get; set; }

        public MySqlTransactionIsolationLevel MySqlTransactionIsolationLevel;

    }

    /// <summary>
    /// Result to be returned from task
    /// </summary>
    public class QueryOutput
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public dynamic Result { get; set; }
    }

}