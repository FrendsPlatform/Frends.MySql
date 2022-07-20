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

            [OneTimeSetUp]
            public async Task OneTimeSetUp()
            {
                _connectionString = "server=" + _server + ";uid=" + _uid + ";pwd=" + _pwd + ";database=" + _database + ";";
                Console.WriteLine(_connectionString);

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

#if false
            [OneTimeTearDown]
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
#endif

            [Test]
            public async Task ShouldSuccess_CallStoredProcedure()
            {
                var input = new Input
                {
                    ConnectionString = _connectionString,
                    Query = @"GetAllFromHodorTest"
                };

                _options.TransactionIsolationLevel = TransactionIsolationLevel.Default;

                var result = await MySQL.ExecuteProcedure(input, _options, new CancellationToken());
                Console.WriteLine(result.ToString());

                Assert.That(result.ToString().Equals("TODO"));
            }

            [Test]
            public void ShouldThrowException_CallStoredProcedure()
            {
                var input = new Input
                {
                    ConnectionString = _connectionString,
                    Query = @"call GetAllFromHodorTest00"

                };

                _options.TransactionIsolationLevel = TransactionIsolationLevel.Default;

                Exception ex = Assert.ThrowsAsync<Exception>(() => MySQL.ExecuteProcedure(input, _options, new CancellationToken()));
                Assert.That(ex != null && ex.Message.StartsWith("Query failed"));

            }

            [Test]
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
            [Test]
            public void ShouldThrowException_CancellationRequested()
            {
                var input = new Input
                {
                    ConnectionString = _connectionString + "nonsense",
                    Query = "SELECT value FROM HodorTest WHERE name LIKE 'hodor' limit 1 "
                };

                _options.TransactionIsolationLevel = TransactionIsolationLevel.Default;

                Exception ex = Assert.ThrowsAsync<TaskCanceledException>(() => MySQL.ExecuteProcedure(input, _options, new CancellationToken(true)));
                Assert.That(ex != null && ex.Message.StartsWith("A task was canceled"));
            }

        }
    }
}
