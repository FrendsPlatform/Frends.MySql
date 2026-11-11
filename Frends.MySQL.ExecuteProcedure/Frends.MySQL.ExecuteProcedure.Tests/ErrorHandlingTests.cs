using System.Threading;
using System.Threading.Tasks;
using Frends.MySQL.ExecuteProcedure.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Frends.MySQL.ExecuteProcedure.Tests;

/// <summary>
/// Tests standardized task error handling.
/// </summary>
[TestClass]
public class ErrorHandlingTests
{
    /// <summary>
    /// Verifies that failures are returned when configured not to throw.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ReturnsFailureResultWhenConfiguredNotToThrow()
    {
        var input = new Input
        {
            Query = "ProcedureName",
        };
        var connection = new Connection
        {
            ConnectionString = "not-a-valid-connection-string",
        };
        var options = new Options
        {
            ThrowErrorOnFailure = false,
            ErrorMessageOnFailure = "Procedure execution failed",
        };

        var result = await MySQL.ExecuteProcedure(input, connection, options, CancellationToken.None);

        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Error);
        Assert.IsTrue(result.Error.Message.StartsWith(options.ErrorMessageOnFailure));
        Assert.IsNotNull(result.Error.AdditionalInfo);
    }
}
