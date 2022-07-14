using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Frends.MySQL.ExecuteProcedure.Definitions;
using Microsoft.VisualStudio.TestPlatform.CoreUtilities.Extensions;
using MySql.Data.MySqlClient;
using NUnit.Framework;

namespace Frends.MySQL.ExecuteProcedure.Tests
{

    public class UnitTests
    {
        /// <summary>
        /// THESE TESTS DO NOT WORK UNLESS YOU INSTALL MySql LOCALLY ON YOUR OWN COMPUTER!
        /// </summary>
        [TestFixture]
        #if false
        [Ignore("Cannot be run unless you have a properly configured MySql DB running on your local computer")]
        #endif
        public class MySqlQueryTests
        {
            private readonly string _server = Environment.GetEnvironmentVariable("MySQL_server");
            private readonly string _uid = Environment.GetEnvironmentVariable("MySQL_uid");
            private readonly string _pwd = Environment.GetEnvironmentVariable("MySQL_pwd");
            private readonly string _database = Environment.GetEnvironmentVariable("MySQL_database");
            readonly Options _options = new Options
            {
                TimeoutSeconds = 300
            };
            private string _connectionString;

            //[OneTimeSetUp]
            [Test, Order(1)]
            public async Task OneTimeSetUp()
            {
                _connectionString = "server=" + _server + ";username=" + _uid + ";pwd=" + _pwd + ";database=" + _database + ";";

                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
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
            }

            //[OneTimeTearDown]
            [Test, Order(50)]
            public async Task OneTimeTearDown()
            {
                using (var connection = new MySqlConnection(_connectionString))
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
                var q = new Input
                {
                    ConnectionString = _connectionString,
                    Query = @"select  * from hodortest limit 2"

                };

                _options.TransactionIsolationLevel = TransactionIsolationLevel.Default;

                var result = await MySQL.ExecuteProcedure(q, _options, new CancellationToken());

                Assert.That(result.ToString(), Is.EqualTo(@"[
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

            [Test, Order(3)]
            public void ShouldThrowException_DoBasicQuery()
            {
                var q = new Input
                {
                    ConnectionString = _connectionString,
                    Query = @"select  * from tablex limit 2"
                };


                _options.TransactionIsolationLevel = TransactionIsolationLevel.Default;

                Exception ex = Assert.ThrowsAsync<Exception>(() => MySQL.ExecuteProcedure(q, _options, new CancellationToken()));
                Assert.That(ex != null && ex.Message.StartsWith("Query failed"));
            }



            [Test, Order(4)]
            public async Task ShouldSuccess_CallStoredProcedure()
            {
                var q = new Input
                {
                    ConnectionString = _connectionString,
                    Query = @"GetAllFromHodorTest"

                };

                _options.TransactionIsolationLevel = TransactionIsolationLevel.Default;

                var result = await MySQL.ExecuteProcedure(q, _options, new CancellationToken());

                Assert.That(result.ToString().Equals("TODO"));
            }
            [Test, Order(5)]
            public void ShouldThrowException_CallStoredProcedure()
            {
                var q = new Input
                {
                    ConnectionString = _connectionString,
                    Query = @"GetAllFromHodorTest00"

                };

                _options.TransactionIsolationLevel = TransactionIsolationLevel.Default;

                Exception ex = Assert.ThrowsAsync<Exception>(() => MySQL.ExecuteProcedure(q, _options, new CancellationToken()));
                Assert.That(ex != null && ex.Message.StartsWith("Query failed"));

            }



            [Test, Order(6)]
            public async Task ShouldSuccess_InsertValues()
            {

                string rndName = Path.GetRandomFileName();
                Random rnd = new Random();
                int rndValue = rnd.Next(1000);
                var q = new Input
                {
                    ConnectionString = _connectionString,
                    Query = "insert into HodorTest (name, value) values ( " + rndName.AddDoubleQuote() + " , " + rndValue + " );"

                };
                _options.TransactionIsolationLevel = TransactionIsolationLevel.Default;

                var result = await MySQL.ExecuteProcedure(q, _options, new CancellationToken());

                Assert.That(result.ToString().Equals("1"));

            }


            [Test, Order(7)]
            public async Task ShouldSuccess_DoBasicQueryOneValue()
            {
                var q = new Input
                {
                    ConnectionString = _connectionString,
                    Query = "SELECT value FROM HodorTest WHERE name LIKE 'hodor' limit 1 "
                };

                _options.TransactionIsolationLevel = TransactionIsolationLevel.Default;

                var result = await MySQL.ExecuteProcedure(q, _options, new CancellationToken());

                Assert.That(result.ToString(), Is.EqualTo(@"[
  {
    ""value"": 123
  }
]"));

            }

            [Test, Order(8)]
            public void ShouldThrowException_FaultyConnectionString()
            {
                var q = new Input
                {
                    ConnectionString = _connectionString + "nonsense",
                    Query = "SELECT value FROM HodorTest WHERE name LIKE 'hodor' limit 1 "

                };

                _options.TransactionIsolationLevel = TransactionIsolationLevel.Default;

                Exception ex = Assert.ThrowsAsync<Exception>(() => MySQL.ExecuteProcedure(q, _options, new CancellationToken()));
                Assert.That(ex != null && ex.Message.StartsWith("Format of the initialization string"));

            }
            [Test, Order(9)]
            public void ShouldThrowException_CancellationRequested()
            {
                var q = new Input
                {
                    ConnectionString = _connectionString + "nonsense",
                    Query = "SELECT value FROM HodorTest WHERE name LIKE 'hodor' limit 1 "
                };

                _options.TransactionIsolationLevel = TransactionIsolationLevel.Default;

                Exception ex = Assert.ThrowsAsync<TaskCanceledException>(() => MySQL.ExecuteProcedure(q, _options, new CancellationToken(true)));
                Assert.That(ex != null && ex.Message.StartsWith("A task was canceled"));
            }

        }
    }
}
