using System;
using System.Threading;
using System.Threading.Tasks;
using Frends.MySQL.ExecuteProcedure.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;

namespace Frends.MySQL.ExecuteProcedure.Tests
{
    public class UnitTests
    {
        /// <summary>
        /// THESE TESTS DO NOT WORK UNLESS YOU INSTALL MySql LOCALLY ON YOUR OWN COMPUTER!
        /// </summary>
        [TestClass]
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

            [TestInitialize]
            public async Task OneTimeSetUp()
            {
                _connectionString = "server=" + _server + ";uid=" + _uid + ";pwd=" + _pwd + ";database=" + _database + ";";

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
                    using (var command = new MySqlCommand("DROP PROCEDURE IF EXISTS UpdateHodorTest; CREATE PROCEDURE UpdateHodorTest() BEGIN UPDATE HodorTest SET name = 'jones' WHERE name LIKE 'jon'; END", connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }

#if true
            [TestCleanup]
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
                    using (var command = new MySqlCommand("DROP PROCEDURE UpdateHodorTest;", connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
#endif

            [TestMethod]
            public async Task TestCallStoredProcedure()
            {
                var input = new Input
                {
                    ConnectionString = _connectionString,
                    Query = @"UpdateHodorTest"
                };

                _options.TransactionIsolationLevel = TransactionIsolationLevel.Default;

                var result = await MySQL.ExecuteProcedure(input, _options, new CancellationToken());

                Assert.AreEqual(1, result.AffectedRows);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task TestThrowCallStoredProcedureAsync()
            {
                var input = new Input
                {
                    ConnectionString = _connectionString,
                    Query = @"call UpdateHodorTest00"

                };

                _options.TransactionIsolationLevel = TransactionIsolationLevel.Default;
                await MySQL.ExecuteProcedure(input, _options, new CancellationToken());
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task TestThrowCallStoredProcedure_FaultyConnectionString()
            {
                var input = new Input
                {
                    ConnectionString = "server=invalid;uid=invalid;pwd=invalid;database=invalid;",
                    Query = @"UpdateHodorTest"
                };

                _options.TransactionIsolationLevel = TransactionIsolationLevel.Default;
                await MySQL.ExecuteProcedure(input, _options, new CancellationToken());
            }
        }
    }
}
