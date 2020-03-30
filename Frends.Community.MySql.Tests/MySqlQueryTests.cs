using System;
using System.IO;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.CoreUtilities.Extensions;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace Frends.MySql.Tests
{
    /// <summary>
    /// THESE TESTS DO NOT WORK UNLESS YOU INSTALL MySql LOCALLY ON YOUR OWN COMPUTER!
    /// </summary>
    [TestFixture]
    // [Ignore("Cannot be run unless you have a properly configured MySql DB running on your local computer")]
    public class MySqlQueryTests
    {
        // Problems with local MySql, tests not implemented yet

        private string connectionString = "server=localhost;uid=root;pwd=kissa001;database=test;";

        Options options = new Options
        {
            TimeoutSeconds = 300
        };

        //[OneTimeSetUp]
        [Test, Order(1)] public async Task OneTimeSetUp()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                try
                {
                    using (var command = new MySqlCommand("CREATE TABLE IF NOT EXISTS DecimalTest(DecimalValue decimal(38,30))", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                using (var command = new MySqlCommand("insert into DecimalTest (DecimalValue) values (1.123456789123456789123456789123)", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                using (var command = new MySqlCommand("CREATE TABLE IF NOT EXISTS HodorTest(name varchar(15), value int(10))", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                using (var command = new MySqlCommand("insert into HodorTest (name, value) values ('hodor', 123), ('jon', 321);", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }

                using (var command = new MySqlCommand("DROP PROCEDURE IF EXISTS GetAllFromHodorTest; CREATE PROCEDURE GetAllFromHodorTest() BEGIN SELECT * FROM HodorTest; END", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                }
                catch (Exception)
                {
                   // table probably/procedure exist already
                    throw;
                }
            }
        }

        //[OneTimeTearDown]
        [Test, Order(50)]
        public async Task OneTimeTearDown()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand("drop table HodorTest", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                using (var command = new MySqlCommand("drop table DecimalTest", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                using (var command = new MySqlCommand("DROP PROCEDURE GetAllFromHodorTest;", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
        }


      
        [Test, Order(2)]
        public async Task ShouldSuccess_DoBasicQuery()
        {
            var q = new InputQuery {ConnectionString = connectionString, 
                CommandText = @"select  * from hodortest limit 2",
                CommandType = MySqlCommandType.Text
            };

            options.ThrowErrorOnFailure = true;
            options.MySqlTransactionIsolationLevel = MySqlTransactionIsolationLevel.Default;

            QueryOutput result = await QueryTask.ExecuteQuery(q, options, new CancellationToken());

            Assert.That(result.Success,Is.True);
            Assert.That(result.Result.ToString(), Is.EqualTo(@"[
  {
    ""name"": ""hodor"",
    ""value"": 123
  },
  {
    ""name"": ""jon"",
    ""value"": 321
  }
]"));

        }

        [Test, Order(2)]
        public async Task ShouldFail_DoBasicQuery()
        {
            var q = new InputQuery
            {
                ConnectionString = connectionString,
                CommandText = @"select  * from tablex limit 2",
                CommandType = MySqlCommandType.Text
            };

            options.ThrowErrorOnFailure = false;
            options.MySqlTransactionIsolationLevel = MySqlTransactionIsolationLevel.Default;

            QueryOutput result = await QueryTask.ExecuteQuery(q, options, new CancellationToken());

            Assert.That(result.Success, Is.False);
            Assert.That(result.Message.Contains("doesn't exist"));

        }

        [Test, Order(2)]
        public void ShouldThrowMySqlException_DoBasicQuery()
        {
            var q = new InputQuery
            {
                ConnectionString = connectionString,
                CommandText = @"select  * from tablex limit 2",
                CommandType = MySqlCommandType.Text
            };

            options.ThrowErrorOnFailure = true;
            options.MySqlTransactionIsolationLevel = MySqlTransactionIsolationLevel.Default;

            MySqlException ex = Assert.ThrowsAsync<MySqlException>(() => QueryTask.ExecuteQuery(q, options, new CancellationToken()));
            Assert.That(ex.Message.Contains("doesn't exist"));

        }





        [Test, Order(2)]
        public async Task ShouldSuccess_CallStoredProcedure()
        {
            var q = new InputQuery
            {
                ConnectionString = connectionString,
                CommandText = @"GetAllFromHodorTest",
                CommandType = MySqlCommandType.StoredProcedure
            };

            options.ThrowErrorOnFailure = true;
            options.MySqlTransactionIsolationLevel = MySqlTransactionIsolationLevel.Default;

            QueryOutput result = await QueryTask.ExecuteProcedure(q, options, new CancellationToken());

            Assert.That(result.Success, Is.True);
            Assert.That(result.Message.Equals("0 row(s) affected"));

        }



        [Test, Order(3)]
        public async Task ShouldSuccess_InsertValues()
        {

            string rndName = Path.GetRandomFileName();
            Random rnd = new Random();
            int rndValue = rnd.Next(1000);
            var q = new InputQuery
            {
                ConnectionString = connectionString,
                CommandText = "insert into HodorTest (name, value) values ( " + rndName.AddDoubleQuote() + " , " + rndValue + " );",
                CommandType = MySqlCommandType.Text

            };

            options.ThrowErrorOnFailure = true;
            options.MySqlTransactionIsolationLevel = MySqlTransactionIsolationLevel.Default;

            QueryOutput result = await QueryTask.ExecuteQuery(q, options, new CancellationToken());

            Assert.That(result.Success, Is.True);
            Assert.That(result.Message.Equals("1 row(s) affected"));

        }
        public static string AddDoubleQuotes(string value)
        {
            return "\"" + value + "\"";
        }

        //        [Test]
        [Test, Order(3)]
        public async Task ShouldSuccess_DoBasicQueryOneValue()
        {

            
            var q = new InputQuery
            {
                ConnectionString = connectionString,
                CommandText = "SELECT value FROM HodorTest WHERE name LIKE 'hodor' ",
                CommandType = MySqlCommandType.Text

            };

            options.ThrowErrorOnFailure = true;
            options.MySqlTransactionIsolationLevel = MySqlTransactionIsolationLevel.Default;

            QueryOutput result = await QueryTask.ExecuteQuery(q, options, new CancellationToken());
           
            Assert.That(result.Result.ToString(), Is.EqualTo(@"[
  {
    ""value"": 123
  }
]"));
          
        }



    }
}
