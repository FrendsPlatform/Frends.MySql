using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

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

        private string connectionString = "server=localhost;uid=root;pwd=pw;database=test;";

        Options options = new Options
        {
            TimeoutSeconds = 300
        };

        //[OneTimeSetUp]
        [Test, Order(1)]
        public async Task OneTimeSetUp()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand("create table DecimalTest(DecimalValue decimal(38,30))", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                using (var command = new MySqlCommand("insert into DecimalTest (DecimalValue) values (1.123456789123456789123456789123)", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                using (var command = new MySqlCommand("create table HodorTest(name varchar(15), value int(10))", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                using (var command = new MySqlCommand("insert into HodorTest (name, value) values ('hodor', 123), ('jon', 321);", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }

                using (var command = new MySqlCommand("CREATE PROCEDURE GetAllFromHodorTest() BEGIN SELECT * FROM HodorTest; END", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        //[OneTimeTearDown]
        [Test, Order(3)]
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


        //        [Test]
        [Test, Order(2)]
        public async Task ShouldReturnJsonString()
        {
            var q = new InputQuery {ConnectionString = connectionString, 
                Query = @"CALL GetAllFromHodorTest()" };

            options.ThrowErrorOnFailure = true;
            options.MySqlTransactionIsolationLevel = MySqlTransactionIsolationLevel.Default;

            QueryOutput result = await QueryTask.Query(q, options, new CancellationToken());

            Assert.IsTrue(string.Equals(result.Result.ToString(), @"[
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

        //        [Test]
        [Test, Order(2)]
        public async Task CallProcedure()
        {
            var q = new InputProcedure { ConnectionString = connectionString,
                Execute = @"GetAllFromHodorTest()" };

            options.ThrowErrorOnFailure = true;
            options.MySqlTransactionIsolationLevel = MySqlTransactionIsolationLevel.None;

            QueryOutput result = await QueryTask.ExecuteStoredProcedure(q, options, new CancellationToken());

            Assert.IsTrue(string.Equals(result.Result.ToString(), @"[
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

    }
}
