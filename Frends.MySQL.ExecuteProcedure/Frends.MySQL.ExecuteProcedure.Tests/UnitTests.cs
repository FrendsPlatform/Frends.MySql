using System;
using System.Threading;
using System.Threading.Tasks;
using Frends.MySQL.ExecuteProcedure.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;

namespace Frends.MySQL.ExecuteProcedure.Tests;

public class UnitTests
{
    [TestClass]
    public class MySqlQueryTests
    {
        private readonly string _server = Environment.GetEnvironmentVariable("MySQL_server");
        private readonly string _uid = Environment.GetEnvironmentVariable("MySQL_uid");
        private readonly string _pwd = Environment.GetEnvironmentVariable("MySQL_pwd");
        private readonly string _database = Environment.GetEnvironmentVariable("MySQL_database");
        private Options _options;
        private string _connectionString;

        [TestInitialize]
        public async Task OneTimeSetUp()
        {
            _connectionString = "server=" + _server + ";uid=" + _uid + ";pwd=" + _pwd + ";database=" + _database + ";";
            _options = new Options
            {
                TimeoutSeconds = 300
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand("CREATE TABLE IF NOT EXISTS DecimalTest(DecimalValue decimal(38,30))", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                using (var command = new MySqlCommand("insert into DecimalTest (DecimalValue) VALUES (1.123456789123456789123456789123)", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                using (var command = new MySqlCommand("CREATE TABLE IF NOT EXISTS HodorTest(name varchar(15), value int(10))", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                using (var command = new MySqlCommand("insert into HodorTest (name, value) VALUES  ('hodor', 123), ('jon', 321);", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                using (var command = new MySqlCommand("DROP PROCEDURE IF EXISTS UpdateHodorTest; CREATE PROCEDURE UpdateHodorTest() BEGIN UPDATE HodorTest SET name = 'jones' WHERE name LIKE 'jon'; END", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                using (var command = new MySqlCommand("DROP PROCEDURE IF EXISTS InsertHodorTest; CREATE PROCEDURE InsertHodorTest(IN name varchar(15), IN value int(10)) BEGIN INSERT INTO HodorTest (name, value) VALUES (name, value); END", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
        
        [TestCleanup]
        public async Task OneTimeTearDown()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand("DROP TABLE HodorTest", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                using (var command = new MySqlCommand("DROP TABLE DecimalTest", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                using (var command = new MySqlCommand("DROP PROCEDURE UpdateHodorTest;", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                using (var command = new MySqlCommand("DROP PROCEDURE InsertHodorTest;", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        [TestMethod]
        public async Task TestCallStoredProcedure()
        {
            var input = new Input
            {
                ConnectionString = _connectionString,
                Query = @"UpdateHodorTest"
            };

            _options.TransactionIsolationLevel = TransactionIsolationLevel.RepeatableRead;

            var result = await MySQL.ExecuteProcedure(input, _options, new CancellationToken());
            Assert.AreEqual(1, result.AffectedRows);
        }

        [TestMethod]
        public async Task TestCallStoredProcedure_WithParameters()
        {
            var parameterName = new Parameter
            {
                Name = "Name",
                Value = "TestName"
            };

            var parameterValue = new Parameter
            {
                Name = "Value",
                Value = "123"
            };

            Parameter[] parameters = { parameterName, parameterValue };

            var input = new Input
            {
                ConnectionString = _connectionString,
                Query = @"InsertHodorTest",
                Parameters = parameters
            };

            _options.TransactionIsolationLevel = TransactionIsolationLevel.RepeatableRead;

            var result = await MySQL.ExecuteProcedure(input, _options, new CancellationToken());
            Assert.AreEqual(1, result.AffectedRows);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task TestThrowCallStoredProcedureAsync_InvalidQuery()
        {
            var input = new Input
            {
                ConnectionString = _connectionString,
                Query = @"call UpdateHodorTest00"

            };

            _options.TransactionIsolationLevel = TransactionIsolationLevel.RepeatableRead;
            await MySQL.ExecuteProcedure(input, _options, new CancellationToken());
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task TestThrowCallStoredProcedure_InvalidConnectionString()
        {
            var input = new Input
            {
                ConnectionString = "server=invalid;uid=invalid;pwd=invalid;database=invalid;",
                Query = @"UpdateHodorTest"
            };

            _options.TransactionIsolationLevel = TransactionIsolationLevel.RepeatableRead;
            await MySQL.ExecuteProcedure(input, _options, new CancellationToken());
        }
    }
}
