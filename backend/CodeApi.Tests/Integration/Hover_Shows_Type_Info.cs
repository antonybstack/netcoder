/*
using System;
using System.Text.Json;
using CodeApi.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class Hover_Shows_Type_Info : SignalRTestBase
{
    public Hover_Shows_Type_Info(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task Hovering_WriteLine_Displays_Type_Info()
    {
        await using HubConnection connection = await CreateAndStartConnectionAsync();
        TaskCompletionSource<JsonElement> hoverTcs = NewTcs<JsonElement>();
        connection.On<JsonElement>("hoverInfo", payload => hoverTcs.TrySetResult(payload));

        const string code = "Console.WriteLine(\"hover me\");";
        string sessionId = Guid.NewGuid().ToString("N");
        await connection.SendAsync("requestCompletions", BuildDocumentEnvelope(code, sessionId: sessionId));

        int position = code.IndexOf("WriteLine", StringComparison.Ordinal);
        await connection.SendAsync("requestHover", new
        {
            doc = BuildDoc(sessionId),
            position
        });

        JsonElement payload = await hoverTcs.Task.WaitAsync(DefaultTimeout);
        string? contents = payload.GetProperty("contents").GetString();
        Assert.Contains("Console.WriteLine", contents, StringComparison.Ordinal);
        Assert.Contains("string value", contents, StringComparison.OrdinalIgnoreCase);
    }
}
*/

