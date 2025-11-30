using System;
using System.Text.Json;
using System.Threading.Tasks;
using CodeApi.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Xunit;

public class SignatureHelp_Argument_Navigation : SignalRTestBase
{
    public SignatureHelp_Argument_Navigation(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task ActiveParameter_Tracks_Cursor_Position()
    {
        await using HubConnection connection = await CreateAndStartConnectionAsync();
        const string code = "Console.WriteLine(\"value\", 123);";
        string sessionId = Guid.NewGuid().ToString("N");
        await connection.SendAsync("requestCompletions", BuildDocumentEnvelope(code, sessionId: sessionId));

        int firstArgPosition = code.IndexOf("\"value\"", StringComparison.Ordinal) + 1;
        int secondArgPosition = code.IndexOf("123", StringComparison.Ordinal) + 1;

        Task<JsonElement> firstPayloadTask = WaitForSignaturePayload(connection);
        await connection.SendAsync("requestSignatureHelp", new
        {
            doc = BuildDoc(sessionId),
            position = firstArgPosition
        });
        JsonElement firstPayload = await firstPayloadTask.WaitAsync(DefaultTimeout);
        Assert.Equal(0, firstPayload.GetProperty("activeParameter").GetInt32());

        Task<JsonElement> secondPayloadTask = WaitForSignaturePayload(connection);
        await connection.SendAsync("requestSignatureHelp", new
        {
            doc = BuildDoc(sessionId),
            position = secondArgPosition
        });
        JsonElement secondPayload = await secondPayloadTask.WaitAsync(DefaultTimeout);
        Assert.Equal(1, secondPayload.GetProperty("activeParameter").GetInt32());
    }

    private static Task<JsonElement> WaitForSignaturePayload(HubConnection connection)
    {
        TaskCompletionSource<JsonElement> tcs = NewTcs<JsonElement>();
        IDisposable? registration = null;
        registration = connection.On<JsonElement>("signatureHelp", payload =>
        {
            registration?.Dispose();
            tcs.TrySetResult(payload);
        });
        return tcs.Task;
    }
}
