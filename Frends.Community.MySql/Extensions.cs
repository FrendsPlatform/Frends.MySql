using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using MySql;
using MySql.Data;
using MySql.Data.MySqlClient;
using MySql.Data.Types;

namespace Frends.MySql
{
    static class Extensions
    {
        internal static TEnum ConvertEnum<TEnum>(this Enum source)
        {
            return (TEnum)Enum.Parse(typeof(TEnum), source.ToString(), true);
        }


        internal static IsolationLevel GetMySqlTransactionIsolationLevel(this MySqlTransactionIsolationLevel MysqlTransactionIsolationLevel)
        {
            return GetEnum<IsolationLevel>(MysqlTransactionIsolationLevel);
        }

        private static T GetEnum<T>(Enum enumValue)
        {
            return (T)Enum.Parse(typeof(T), enumValue.ToString());
        }
    }
}
