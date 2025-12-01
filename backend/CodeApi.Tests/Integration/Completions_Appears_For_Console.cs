/*
using System;
using System.Linq;
using System.Text.Json;
using CodeApi.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class Completions_Appears_For_Console : SignalRTestBase
{
    public Completions_Appears_For_Console(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task ConsoleWriteLine_Suggestion_Is_Returned()
    {
        await using HubConnection connection = await CreateAndStartConnectionAsync();
        TaskCompletionSource<JsonElement> completionsTcs = NewTcs<JsonElement>();
        connection.On<JsonElement>("completions", payload => completionsTcs.TrySetResult(payload));

        await connection.SendAsync("requestCompletions", BuildDocumentEnvelope("Console.Wri"));

        JsonElement payload = await completionsTcs.Task.WaitAsync(DefaultTimeout);
        string?[] labels = payload.GetProperty("items").EnumerateArray()
            .Select(item => item.GetProperty("label").GetString())
            .ToArray();

        Assert.Contains("Console.WriteLine", labels);
    }
}
*/

