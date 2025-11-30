using System;
using System.Linq;
using System.Text.Json;
using CodeApi.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class Diagnostics_On_SyntaxError : SignalRTestBase
{
    public Diagnostics_On_SyntaxError(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task Missing_Paren_Produces_Error_Diagnostic()
    {
        await using HubConnection connection = await CreateAndStartConnectionAsync();
        TaskCompletionSource<JsonElement> diagnosticsTcs = NewTcs<JsonElement>();
        connection.On<JsonElement>("diagnosticsUpdated", payload => diagnosticsTcs.TrySetResult(payload));

        const string code = "public class C { void M() { Console.WriteLine(\"hello\" } }";
        await connection.SendAsync("requestDiagnostics", BuildDocumentEnvelope(code));

        JsonElement payload = await diagnosticsTcs.Task.WaitAsync(DefaultTimeout);
        JsonElement[] diagnostics = payload.GetProperty("diagnostics").EnumerateArray().ToArray();
        Assert.NotEmpty(diagnostics);
        JsonElement diagnostic = diagnostics.First();
        Assert.Equal("Error", diagnostic.GetProperty("severity").GetString());
        Assert.Contains(")", diagnostic.GetProperty("message").GetString(), StringComparison.Ordinal);
    }
}
