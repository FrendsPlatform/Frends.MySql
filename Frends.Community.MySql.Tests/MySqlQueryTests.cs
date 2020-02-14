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

        ConnectionProperties _conn = new ConnectionProperties
        {
            ConnectionString = "server=localhost;uid=root;pwd=pw;database=test;",
            TimeoutSeconds = 300
        };


        
        //[OneTimeSetUp]
        [Test, Order(1)]
        public async Task OneTimeSetUp()
        {
            using (var connection = new MySqlConnection(_conn.ConnectionString))
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
            }
        }


        //[OneTimeTearDown]
        [Test, Order(3)]
        public async Task OneTimeTearDown()
        {
            using (var connection = new MySqlConnection(_conn.ConnectionString))
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
            }
        }


        //        [Test]
        [Test, Order(2)]
        [Category("Json tests")]
        public async Task ShouldReturnJsonString()
        {
            var q = new QueryProperties { Query = @"select name as ""name"", value as ""value"" from HodorTest" };

            var options = new Options { ThrowErrorOnFailure = true };

            Output result = await QueryTask.Query(q, _conn, options, new CancellationToken());

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
