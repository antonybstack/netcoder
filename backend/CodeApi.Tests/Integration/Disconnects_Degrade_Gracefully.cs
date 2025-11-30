using System;
using System.Collections.Generic;
using System.Text.Json;
using CodeApi.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class Disconnects_Degrade_Gracefully : SignalRTestBase
{
    public Disconnects_Degrade_Gracefully(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task Status_Updates_And_Local_Fallback_Are_Emitted()
    {
        await using HubConnection connection = await CreateAndStartConnectionAsync();
        List<string> statuses = new List<string>();
        TaskCompletionSource<bool> reconnectTcs = NewTcs<bool>();
        connection.On<JsonElement>("statusChanged", payload =>
        {
            string? status = payload.GetProperty("status").GetString();
            if (status is null)
            {
                return;
            }

            statuses.Add(status);
            if (statuses.Contains("reconnecting") && status == "connected")
            {
                reconnectTcs.TrySetResult(true);
            }
        });

        TaskCompletionSource<JsonElement> completionsTcs = NewTcs<JsonElement>();
        connection.On<JsonElement>("completions", payload => completionsTcs.TrySetResult(payload));

        const string code = "// FORCE_DISCONNECT\nConsole.WriteLine(\"fallback\");";
        await connection.SendAsync("requestCompletions", BuildDocumentEnvelope(code));

        JsonElement completions = await completionsTcs.Task.WaitAsync(DefaultTimeout);
        await reconnectTcs.Task.WaitAsync(DefaultTimeout);

        Assert.Contains("connected", statuses);
        Assert.Contains("reconnecting", statuses);
        Assert.Equal("connected", statuses[^1]);

        JsonElement firstItem = completions.GetProperty("items")[0];
        Assert.Contains("local", firstItem.GetProperty("detail").GetString(), StringComparison.OrdinalIgnoreCase);
    }
}
