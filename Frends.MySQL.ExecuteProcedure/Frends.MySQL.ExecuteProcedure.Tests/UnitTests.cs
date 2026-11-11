using System;
using System.Threading;
using System.Threading.Tasks;
using Frends.MySQL.ExecuteProcedure.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;

namespace Frends.MySQL.ExecuteProcedure.Tests;

/// <summary>
/// Integration tests for the MySQL ExecuteProcedure task.
/// </summary>
public class UnitTests
{
    /// <summary>
    /// Tests stored procedure execution behavior.
    /// </summary>
    [TestClass]
    public class MySqlQueryTests
    {
        // Use a local MySQL instance configured with the root password test.
        private readonly string connectionString = "server=127.0.0.1;uid=root;pwd=my-secret-pw;";
        private Options options;

        /// <summary>
        /// Creates the database objects used by the integration tests.
        /// </summary>
        /// <returns>A task that represents the asynchronous setup operation.</returns>
        [TestInitialize]
        public async Task OneTimeSetUp()
        {
            options = new Options
            {
                TimeoutSeconds = 300,
            };

            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand("CREATE DATABASE IF NOT EXISTS MySQLProcedureTest;", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }

                using (var command = new MySqlCommand("USE MySQLProcedureTest;", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }

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

        /// <summary>
        /// Removes the database objects created for the integration tests.
        /// </summary>
        /// <returns>A task that represents the asynchronous cleanup operation.</returns>
        [TestCleanup]
        public async Task OneTimeTearDown()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand("DROP PROCEDURE UpdateHodorTest;", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }

                using (var command = new MySqlCommand("DROP PROCEDURE InsertHodorTest;", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }

                using (var command = new MySqlCommand("DROP TABLE HodorTest", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }

                using (var command = new MySqlCommand("DROP TABLE DecimalTest", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }

                using (var command = new MySqlCommand("DROP DATABASE MySQLProcedureTest;", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Executes a stored procedure without parameters.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [TestMethod]
        public async Task TestCallStoredProcedure()
        {
            var input = new Input
            {
                ConnectionString = connectionString,
                Query = @"UpdateHodorTest",
            };

            options.TransactionIsolationLevel = TransactionIsolationLevel.RepeatableRead;

            var result = await MySQL.ExecuteProcedure(input, options, CancellationToken.None);
            Assert.AreEqual(1, result.AffectedRows);
        }

        /// <summary>
        /// Executes a stored procedure with parameters.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [TestMethod]
        public async Task TestCallStoredProcedure_WithParameters()
        {
            var parameterName = new Parameter
            {
                Name = "Name",
                Value = "TestName",
            };

            var parameterValue = new Parameter
            {
                Name = "Value",
                Value = "123",
            };

            Parameter[] parameters = { parameterName, parameterValue };

            var input = new Input
            {
                ConnectionString = connectionString,
                Query = @"InsertHodorTest",
                Parameters = parameters,
            };

            options.TransactionIsolationLevel = TransactionIsolationLevel.RepeatableRead;

            var result = await MySQL.ExecuteProcedure(input, options, CancellationToken.None);
            Assert.AreEqual(1, result.AffectedRows);
        }

        /// <summary>
        /// Throws when the requested stored procedure does not exist.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task TestThrowCallStoredProcedureAsync_InvalidQuery()
        {
            var input = new Input
            {
                ConnectionString = connectionString,
                Query = @"call UpdateHodorTest00",
            };

            options.TransactionIsolationLevel = TransactionIsolationLevel.RepeatableRead;
            await MySQL.ExecuteProcedure(input, options, CancellationToken.None);
        }

        /// <summary>
        /// Throws when the connection string is invalid.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task TestThrowCallStoredProcedure_InvalidConnectionString()
        {
            var input = new Input
            {
                ConnectionString = "server=invalid;uid=invalid;pwd=test;database=invalid;",
                Query = @"UpdateHodorTest",
            };

            options.TransactionIsolationLevel = TransactionIsolationLevel.RepeatableRead;
            await MySQL.ExecuteProcedure(input, options, CancellationToken.None);
        }

        /// <summary>
        /// Throws when the connection string is empty.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestThrowCallStoredProcedure_EmptyConnectionString()
        {
            var input = new Input
            {
                ConnectionString = string.Empty,
                Query = @"UpdateHodorTest",
            };

            options.TransactionIsolationLevel = TransactionIsolationLevel.RepeatableRead;
            await MySQL.ExecuteProcedure(input, options, CancellationToken.None);
        }

        /// <summary>
        /// Throws when the query is empty.
        /// </summary>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestThrowCallStoredProcedure_EmptyQuery()
        {
            var input = new Input
            {
                ConnectionString = "server=invalid;uid=invalid;pwd=test;database=invalid;",
                Query = string.Empty,
            };

            options.TransactionIsolationLevel = TransactionIsolationLevel.RepeatableRead;
            await MySQL.ExecuteProcedure(input, options, CancellationToken.None);
        }
    }
}
