using Frends.MySQL.ExecuteQuery.Definitions;
using Microsoft.VisualStudio.TestPlatform.CoreUtilities.Extensions;
using MySqlConnector;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Frends.MySQL.ExecuteQuery.Tests;

/// <summary>
/// Set up MySQL to docker:
/// docker run -p 3306:3306 -e MYSQL_ROOT_PASSWORD=my-secret-pw -d mysql
/// </summary>
[TestFixture]
public class UnitTests
{
    private static Options options;

    private const string DbConnectionString =
        "Server=127.0.0.1;Port=3306;User ID=root;Password=my-secret-pw;Database=unittest";

    private const string ServerConnectionString = "Server=127.0.0.1;Port=3306;User ID=root;Password=my-secret-pw";
    private static readonly string Newline = Environment.NewLine;
    private static QueryInput queryInput;


    [SetUp]
    public static async Task PrepareDb()
    {
        options = new Options
        {
            TimeoutSeconds = 300,
            MySqlTransactionIsolationLevel = MySqlTransactionIsolationLevel.RepeatableRead
        };
        queryInput = new QueryInput
        {
            ConnectionString = DbConnectionString
        };
        await using var connection = new MySqlConnection(ServerConnectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand("CREATE DATABASE IF NOT EXISTS unittest;", connection);
        await command.ExecuteNonQueryAsync();
        command.CommandText = "USE unittest;";
        await command.ExecuteNonQueryAsync();
        command.CommandText = "CREATE TABLE IF NOT EXISTS FooTest(name varchar(15), value int(10))";
        await command.ExecuteNonQueryAsync();
        command.CommandText = "CREATE TABLE IF NOT EXISTS FooTest2(name varchar(15), value int(10))";
        await command.ExecuteNonQueryAsync();
        command.CommandText = "insert into FooTest (name, value) values ('foo', 123), ('bar', 321);";
        await command.ExecuteNonQueryAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        await using var connection = new MySqlConnection(DbConnectionString);
        await connection.OpenAsync();

        await using var database = new MySqlCommand("use unittest", connection);
        await database.ExecuteNonQueryAsync();

        await using var command = new MySqlCommand("DROP TABLE IF EXISTS FooTest", connection);
        await command.ExecuteNonQueryAsync();

        command.CommandText = "DROP TABLE IF EXISTS FooTest2";
        await command.ExecuteNonQueryAsync();
    }

    [Test]
    public async Task ShouldSuccess_DoBasicQuery()
    {
        queryInput.CommandText = "select * from FooTest limit 2";
        var expect =
            $"[{Newline}  {{{Newline}    \"name\": \"foo\",{Newline}    \"value\": 123{Newline}  }},{Newline}  {{{Newline}    \"name\": \"bar\",{Newline}    \"value\": 321{Newline}  }}{Newline}]";
        var result = await MySQL.ExecuteQuery(queryInput, options, CancellationToken.None);
        ClassicAssert.AreEqual(expect.Replace(@"\n\r", @"\n"), result.ResultJtoken.ToString());
    }

    [Test]
    public async Task ExecuteReader_ShouldSuccess()
    {
        queryInput.CommandText = "SHOW TABLES FROM unittest;";
        options.ExecuteType = ExecuteType.Reader;

        var expect =
            $"[{Newline}  {{{Newline}    \"Tables_in_unittest\": \"FooTest\"{Newline}  }},{Newline}  {{{Newline}    \"Tables_in_unittest\": \"FooTest2\"{Newline}  }}{Newline}]";
        var result = await MySQL.ExecuteQuery(queryInput, options, CancellationToken.None);
        ClassicAssert.AreEqual(expect.Replace(@"\n\r", @"\n"), result.ResultJtoken.ToString());
    }

    [Test]
    public async Task ExecuteScalar_ShouldSuccess()
    {
        queryInput.CommandText = "SELECT SUM(value) FROM FooTest;";
        options.ExecuteType = ExecuteType.Scalar;
        const string expect = "444";
        var result = await MySQL.ExecuteQuery(queryInput, options, CancellationToken.None);
        ClassicAssert.AreEqual(expect.Replace(@"\n\r", @"\n"), result.ResultJtoken.ToString());
    }

    [Test]
    public async Task ExecuteNonQuery_ShouldSuccess()
    {
        queryInput.CommandText = "UPDATE FooTest SET value = 1;";
        options.ExecuteType = ExecuteType.NonQuery;

        var expect = $"{{{Newline}  \"AffectedRows\": 2{Newline}}}";
        var result = await MySQL.ExecuteQuery(queryInput, options, CancellationToken.None);
        ClassicAssert.AreEqual(expect.Replace(@"\n\r", @"\n"), result.ResultJtoken.ToString());
    }

    [Test]
    public void ShouldThrowException_DoBasicQuery()
    {
        queryInput.CommandText = "select * from tablex limit 2";
        var ex = Assert.ThrowsAsync<Exception>(() => MySQL.ExecuteQuery(queryInput, options, CancellationToken.None));
        ClassicAssert.IsTrue(ex.Message.Contains("Table 'unittest.tablex' doesn't exist"));
    }

    [Test]
    public async Task ShouldSuccess_InsertValues()
    {
        var rndName = Path.GetRandomFileName();
        Random rnd = new();
        var rndValue = rnd.Next(1000);

        queryInput.CommandText =
            $"insert into FooTest (name, value) values ( {rndName.AddDoubleQuote()}, {rndValue} );";
        var result = await MySQL.ExecuteQuery(queryInput, options, CancellationToken.None);
        ClassicAssert.IsTrue(result.Success);
        ClassicAssert.AreEqual(new JArray(), result.ResultJtoken);

        queryInput.CommandText = "select * from FooTest;";
        var check = await MySQL.ExecuteQuery(queryInput, options, CancellationToken.None);
        ClassicAssert.IsTrue(check.Success);
        ClassicAssert.IsTrue(check.ResultJtoken.ToString().Contains(rndName));
    }

    [Test]
    public async Task ShouldSuccess_DoBasicQueryOneValue()
    {
        queryInput.CommandText = "SELECT value FROM FooTest WHERE name LIKE 'foo' limit 1 ";
        var expect = new JObject(new JProperty("value", 123));
        var result = await MySQL.ExecuteQuery(queryInput, options, CancellationToken.None);
        ClassicAssert.AreEqual(expect["value"], result.ResultJtoken[0]["value"]);
    }

    [Test]
    public void ShouldThrowException_FaultyConnectionString()
    {
        queryInput.ConnectionString =
            "Server=127.0.0.1;Port=3306;User ID=root;Password=my-secret-pw;Database=invalid;";
        queryInput.CommandText = "SELECT value FROM FooTest WHERE name LIKE 'foo' limit 1 ";
        var ex = Assert.ThrowsAsync<Exception>(() => MySQL.ExecuteQuery(queryInput, options, CancellationToken.None));
        Assert.That(ex, Is.Not.Null);
        ClassicAssert.AreEqual("Unknown database 'invalid'", ex.Message);
    }

    [Test]
    public void ShouldThrowException_CancellationRequested()
    {
        queryInput.CommandText = "SELECT value FROM FooTest WHERE name LIKE 'foo' limit 1 ";
        var ex = Assert.ThrowsAsync<Exception>(() =>
            MySQL.ExecuteQuery(queryInput, options, new CancellationToken(true)));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task ShouldSuccess_DoBasicScalar()
    {
        queryInput.CommandText = "SELECT UPPER(name) FROM FooTest";
        var result = await MySQL.ExecuteQuery(queryInput, options, CancellationToken.None);
        ClassicAssert.AreEqual("FOO", result.ResultJtoken[0]["UPPER(name)"].ToString());
    }

    [Test]
    public async Task ShouldSuccess_DoBasicDelete()
    {
        queryInput.CommandText = "delete from FooTest where value = 123";
        var result = await MySQL.ExecuteQuery(queryInput, options, CancellationToken.None);
        ClassicAssert.IsTrue(result.Success);

        queryInput.CommandText = "select * from FooTest;";
        var check = await MySQL.ExecuteQuery(queryInput, options, CancellationToken.None);
        ClassicAssert.IsTrue(check.Success);
        ClassicAssert.IsFalse(check.ResultJtoken.ToString().Contains("123"));
    }

    [Test]
    public async Task ShouldSuccess_DoBasicUpdate()
    {
        queryInput.CommandText = "update FooTest set name = 'newName' where value = 123";
        var result = await MySQL.ExecuteQuery(queryInput, options, CancellationToken.None);
        ClassicAssert.IsTrue(result.Success);

        queryInput.CommandText = "select * from FooTest;";
        var check = await MySQL.ExecuteQuery(queryInput, options, CancellationToken.None);
        ClassicAssert.IsTrue(check.Success);
        ClassicAssert.IsTrue(check.ResultJtoken.ToString().Contains("newName"));
    }

    [Test]
    public async Task ShouldSuccess_DoTruncate()
    {
        queryInput.CommandText = "truncate FooTest";
        var result = await MySQL.ExecuteQuery(queryInput, options, CancellationToken.None);
        ClassicAssert.IsTrue(result.Success);

        queryInput.CommandText = "select * from FooTest;";
        var check = await MySQL.ExecuteQuery(queryInput, options, CancellationToken.None);
        ClassicAssert.IsTrue(check.Success);
        ClassicAssert.AreEqual(new JArray(), result.ResultJtoken);
    }

    [Test]
    public async Task ShouldSuccess_InsertValuesWithParameters()
    {
        string rndName = Path.GetRandomFileName();
        Random rnd = new();
        int rndValue = rnd.Next(1000);

        queryInput.CommandText = "insert into FooTest (name, value) values (@rndName , @rndValue);";
        queryInput.Parameters = new Parameter[]
        {
            new()
            {
                Name = "@rndName",
                Value = rndName.AddDoubleQuote()
            },
            new()
            {
                Name = "@rndValue",
                Value = rndValue
            }
        };
        var result = await MySQL.ExecuteQuery(queryInput, options, CancellationToken.None);
        ClassicAssert.IsTrue(result.Success);
        ClassicAssert.AreEqual(new JArray(), result.ResultJtoken);

        queryInput.CommandText = "select * from FooTest;";
        var check = await MySQL.ExecuteQuery(queryInput, options, CancellationToken.None);
        ClassicAssert.IsTrue(check.Success);
        ClassicAssert.IsTrue(check.ResultJtoken.ToString().Contains(rndName));
    }

    [Test]
    public void TimeoutShortQuery_ShouldThrow()
    {
        options.TimeoutSeconds = 1;
        queryInput.CommandText = @"
            SELECT SLEEP(1), 'row1'
            UNION ALL
            SELECT SLEEP(1), 'row2';";
        var ex = Assert.ThrowsAsync<Exception>(async () =>
            await MySQL.ExecuteQuery(queryInput, options, CancellationToken.None));

        Assert.That(ex!.Message, Does.Contain("timeout").IgnoreCase);
    }

    [Test]
    public async Task TimeoutWorksCorrectly()
    {
        options.TimeoutSeconds = 1;
        queryInput.CommandText = @"
            SELECT SLEEP(35), 'row1'
            UNION ALL
            SELECT SLEEP(35), 'row2';";
        var ex = Assert.ThrowsAsync<Exception>(async () =>
        {
            await MySQL.ExecuteQuery(queryInput, options, CancellationToken.None);
        });
        Assert.That(ex!.Message, Does.Contain("timeout").IgnoreCase);

        options.TimeoutSeconds = 3601;
        var result = await MySQL.ExecuteQuery(queryInput, options, CancellationToken.None);
        Assert.That(result.Success, Is.True);
    }
}
