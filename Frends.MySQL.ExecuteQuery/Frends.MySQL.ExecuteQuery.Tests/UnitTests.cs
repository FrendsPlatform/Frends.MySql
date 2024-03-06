using Frends.MySQL.ExecuteQuery.Definitions;
using Microsoft.VisualStudio.TestPlatform.CoreUtilities.Extensions;
using MySqlConnector;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Frends.MySQL.ExecuteQuery.Tests;

/// <summary>
/// Setup MySQL to docker:
/// docker run -p 3306:3306 -e MYSQL_ROOT_PASSWORD=my-secret-pw -d mysql
/// </summary>
[TestFixture]
public class UnitTests
{
    readonly Options _options = new() { TimeoutSeconds = 300, MySqlTransactionIsolationLevel = MySqlTransactionIsolationLevel.RepeatableRead };

    //[OneTimeTearDown]
    [Test, Order(50)]

    public async Task OneTimeTearDown()
    {
        using var connection = new MySqlConnection(await CreateConnectionString());
        await connection.OpenAsync();

        using var database = new MySqlCommand("use Unittest", connection);
        await database.ExecuteNonQueryAsync();

        using var command = new MySqlCommand("drop table FooTest", connection);
        await command.ExecuteNonQueryAsync();
    }

    [Test, Order(1)]
    public async Task ShouldSuccess_DoBasicQuery()
    {
        var q = new QueryInput
        {
            ConnectionString = await CreateConnectionString(),
            CommandText = @"select * from FooTest limit 2"
        };

        var newline = Environment.NewLine;
        var expect = $"[{newline}  {{{newline}    \"name\": \"foo\",{newline}    \"value\": 123{newline}  }},{newline}  {{{newline}    \"name\": \"bar\",{newline}    \"value\": 321{newline}  }}{newline}]";
        var result = await MySQL.ExecuteQuery(q, _options, new CancellationToken());
        Assert.AreEqual(result.ResultJtoken.ToString(), expect.Replace(@"\n\r", @"\n"));
    }

    [Test, Order(3)]
    public async Task ShouldThrowException_DoBasicQuery()
    {
        var q = new QueryInput
        {
            ConnectionString = await CreateConnectionString(),
            CommandText = @"select * from tablex limit 2"
        };

        Exception ex = Assert.ThrowsAsync<Exception>(() => MySQL.ExecuteQuery(q, _options, new CancellationToken()));
        Assert.IsTrue(ex.Message.ToString().Contains("Table 'unittest.tablex' doesn't exist"));
    }

    [Test, Order(4)]

    public async Task ShouldSuccess_InsertValues()
    {
        string rndName = Path.GetRandomFileName();
        Random rnd = new();
        int rndValue = rnd.Next(1000);
        var q = new QueryInput
        {
            ConnectionString = await CreateConnectionString(),
            CommandText = "insert into FooTest (name, value) values ( " + rndName.AddDoubleQuote() + " , " + rndValue + " );"
        };

        var result = await MySQL.ExecuteQuery(q, _options, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(result.ResultJtoken, new JArray());
    }

    [Test, Order(5)]
    public async Task ShouldSuccess_DoBasicQueryOneValue()
    {
        var q = new QueryInput
        {
            ConnectionString = await CreateConnectionString(),
            CommandText = "SELECT value FROM FooTest WHERE name LIKE 'foo' limit 1 "
        };
        var expect = new JObject(new JProperty("value", 123));

        var result = await MySQL.ExecuteQuery(q, _options, new CancellationToken());
        Console.WriteLine(expect["value"]);
        Assert.AreEqual(result.ResultJtoken[0]["value"], expect["value"]);
    }

    [Test, Order(6)]
    public void ShouldThrowException_FaultyConnectionString()
    {
        var q = new QueryInput
        {
            ConnectionString = CreateConnectionString() + "nonsense",
            CommandText = "SELECT value FROM FooTest WHERE name LIKE 'foo' limit 1 "
        };

        Exception ex = Assert.ThrowsAsync<Exception>(() => MySQL.ExecuteQuery(q, _options, new CancellationToken()));
        Assert.That(ex != null && ex.Message.StartsWith("Format of the initialization string"));
    }

    [Test, Order(7)]
    public void ShouldThrowException_CancellationRequested()
    {
        var q = new QueryInput
        {
            ConnectionString = CreateConnectionString() + "nonsense",
            CommandText = "SELECT value FROM FooTest WHERE name LIKE 'foo' limit 1 "
        };

        Exception ex = Assert.ThrowsAsync<Exception>(() => MySQL.ExecuteQuery(q, _options, new CancellationToken(true)));
    }

    private static async Task<string> CreateConnectionString()
    {
        MySqlConnectionStringBuilder conn_string = new()
        {
            Server = "127.0.0.1",
            Port = 3360,
            UserID = "root",
            Password = "my-secret-password",
            Database = "unittest"
        };

        await HandleDB(conn_string.ToString());
        return conn_string.ToString();
    }

    private static async Task HandleDB(string conn_string)
    {
        using var connection = new MySqlConnection(conn_string);
        await connection.OpenAsync();
        using (var command = new MySqlCommand("CREATE DATABASE IF NOT EXISTS unittest;", connection))
        {
            await command.ExecuteNonQueryAsync();
        }
        using (var command = new MySqlCommand("USE unittest;", connection))
        {
            await command.ExecuteNonQueryAsync();
        }
        using (var command = new MySqlCommand("CREATE TABLE IF NOT EXISTS FooTest(name varchar(15), value int(10))", connection))
        {
            await command.ExecuteNonQueryAsync();
        }
        using (var command = new MySqlCommand("insert into FooTest (name, value) values ('foo', 123), ('bar', 321);", connection))
        {
            await command.ExecuteNonQueryAsync();
        }
    }
}