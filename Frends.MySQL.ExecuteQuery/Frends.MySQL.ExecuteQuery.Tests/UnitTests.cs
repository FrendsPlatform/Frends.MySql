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
        Assert.AreEqual(expect.Replace(@"\n\r", @"\n"), result.ResultJtoken.ToString());
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

        var connectionstring = await CreateConnectionString();
        var q = new QueryInput
        {
            ConnectionString = connectionstring,
            CommandText = "insert into FooTest (name, value) values ( " + rndName.AddDoubleQuote() + " , " + rndValue + " );"
        };

        var result = await MySQL.ExecuteQuery(q, _options, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(new JArray(), result.ResultJtoken);

        var cq = new QueryInput
        {
            ConnectionString = connectionstring,
            CommandText = "select * from FooTest;",
        };

        var check = await MySQL.ExecuteQuery(cq, _options, new CancellationToken());
        Assert.IsTrue(check.Success);
        Assert.IsTrue(check.ResultJtoken.ToString().Contains(rndName));
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
        Assert.AreEqual(expect["value"], result.ResultJtoken[0]["value"]);
    }

    [Test, Order(6)]
    public async Task ShouldThrowException_FaultyConnectionString()
    {
        var q = new QueryInput
        {
            ConnectionString = await CreateConnectionString() + "nonsense",
            CommandText = "SELECT value FROM FooTest WHERE name LIKE 'foo' limit 1 "
        };

        Exception ex = Assert.ThrowsAsync<Exception>(() => MySQL.ExecuteQuery(q, _options, new CancellationToken()));
        Assert.IsNotNull(ex);
        Assert.AreEqual("Unknown database 'unittestnonsense'", ex.Message);
    }

    [Test, Order(7)]
    public async Task ShouldThrowException_CancellationRequested()
    {
        var q = new QueryInput
        {
            ConnectionString = await CreateConnectionString() + "nonsense",
            CommandText = "SELECT value FROM FooTest WHERE name LIKE 'foo' limit 1 "
        };

        Exception ex = Assert.ThrowsAsync<Exception>(() => MySQL.ExecuteQuery(q, _options, new CancellationToken(true)));
        Assert.IsNotNull(ex);
    }

    [Test, Order(8)]
    public async Task ShouldSuccess_DoBasicScalar()
    {
        var q = new QueryInput
        {
            ConnectionString = await CreateConnectionString(),
            CommandText = "SELECT UPPER(name) FROM FooTest"
        };

        var result = await MySQL.ExecuteQuery(q, _options, new CancellationToken());
        Assert.AreEqual("FOO", result.ResultJtoken[0]["UPPER(name)"].ToString());
    }

    [Test, Order(9)]
    public async Task ShouldSuccess_DoBasicDelete()
    {
        var connectionstring = await CreateConnectionString();
        var q = new QueryInput
        {
            ConnectionString = connectionstring,
            CommandText = "delete from FooTest where value = 123"
        };

        var result = await MySQL.ExecuteQuery(q, _options, new CancellationToken());
        Assert.IsTrue(result.Success);

        var cq = new QueryInput
        {
            ConnectionString = connectionstring,
            CommandText = "select * from FooTest;",
        };

        var check = await MySQL.ExecuteQuery(cq, _options, new CancellationToken());
        Assert.IsTrue(check.Success);
        Assert.IsFalse(check.ResultJtoken.ToString().Contains("123"));
    }

    [Test, Order(10)]
    public async Task ShouldSuccess_DoBasicUpdate()
    {
        var connectionstring = await CreateConnectionString();
        var q = new QueryInput
        {
            ConnectionString = connectionstring,
            CommandText = "update FooTest set name = 'newName' where value = 123"
        };

        var result = await MySQL.ExecuteQuery(q, _options, new CancellationToken());
        Assert.IsTrue(result.Success);

        var cq = new QueryInput
        {
            ConnectionString = connectionstring,
            CommandText = "select * from FooTest;",
        };

        var check = await MySQL.ExecuteQuery(cq, _options, new CancellationToken());
        Assert.IsTrue(check.Success);
        Assert.IsTrue(check.ResultJtoken.ToString().Contains("newName"));
    }

    [Test, Order(10)]
    public async Task ShouldSuccess_DoTruncate()
    {
        var connectionstring = await CreateConnectionString();
        var q = new QueryInput
        {
            ConnectionString = connectionstring,
            CommandText = "truncate FooTest"
        };

        var result = await MySQL.ExecuteQuery(q, _options, new CancellationToken());
        Assert.IsTrue(result.Success);

        var cq = new QueryInput
        {
            ConnectionString = connectionstring,
            CommandText = "select * from FooTest;",
        };

        var check = await MySQL.ExecuteQuery(cq, _options, new CancellationToken());
        Assert.IsTrue(check.Success);
        Assert.AreEqual(new JArray(), result.ResultJtoken);
    }

    private static async Task<string> CreateConnectionString()
    {
        MySqlConnectionStringBuilder conn_string = new()
        {
            Server = "127.0.0.1",
            Port = 3306,
            UserID = "root",
            Password = "my-secret-pw",
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