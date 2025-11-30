using System;
using System.Text.Json;
using CodeApi.Hubs;
using CodeApi.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class Intellisense_SignalR_Contracts : SignalRTestBase
{
    public Intellisense_SignalR_Contracts(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Theory]
    [InlineData("Guid.")]
    // [InlineData("Console.WriteLi")] // TODO: not working yet
    public async Task RequestCompletions_Returns_Items_With_Required_Fields(string code)
    {
        await using var connection = await CreateAndStartConnectionAsync();
        TaskCompletionSource<JsonElement> tcs = NewTcs<JsonElement>();
        connection.On<JsonElement>(IntellisenseHub.CompletionsResponseMethod, payload => tcs.TrySetResult(payload));

        await connection.SendAsync(nameof(IntellisenseHub.RequestCompletions), BuildDocumentEnvelope(code));

        var payload = await tcs.Task.WaitAsync(DefaultTimeout);
        Assert.True(payload.TryGetProperty(nameof(IntellisenseHub.CompletionsResponse.items), out var items));
        Assert.True(items.GetArrayLength() > 0);
        foreach (var item in items.EnumerateArray())
        {
            Assert.True(item.TryGetProperty("displayText", out var displayText));
            ;
            Assert.False(string.IsNullOrWhiteSpace(displayText.GetString()));
            Assert.True(item.TryGetProperty("kind", out var kind));
            Assert.False(string.IsNullOrWhiteSpace(kind.GetString()));
        }
    }

    /*
    [Fact]
    public async Task RequestDiagnostics_Returns_Diagnostics_With_Required_Fields()
    {
        await using var connection = await CreateAndStartConnectionAsync();
        TaskCompletionSource<JsonElement> tcs = NewTcs<JsonElement>();
        connection.On<JsonElement>("diagnosticsUpdated", payload => tcs.TrySetResult(payload));

        const string code = "public class C { void M() { Console.WriteLine(\"x\" } }";
        await connection.SendAsync("requestDiagnostics", BuildDocumentEnvelope(code));

        var payload = await tcs.Task.WaitAsync(DefaultTimeout);
        Assert.True(payload.TryGetProperty("diagnostics", out var diagnostics));
        Assert.True(diagnostics.GetArrayLength() > 0);
        foreach (var diag in diagnostics.EnumerateArray())
        {
            Assert.True(diag.TryGetProperty("message", out var message));
            Assert.False(string.IsNullOrWhiteSpace(message.GetString()));
            Assert.True(diag.TryGetProperty("severity", out _));
            Assert.True(diag.TryGetProperty("range", out var range));
            Assert.True(range.TryGetProperty("start", out _));
            Assert.True(range.TryGetProperty("end", out _));
        }
    }

    [Fact]
    public async Task RequestHover_Returns_Content_For_Symbol()
    {
        await using var connection = await CreateAndStartConnectionAsync();
        TaskCompletionSource<JsonElement> hoverTcs = NewTcs<JsonElement>();
        connection.On<JsonElement>("hoverInfo", payload => hoverTcs.TrySetResult(payload));

        const string code = "Console.WriteLine(\"hello world\");";
        string sessionId = Guid.NewGuid().ToString("N");
        await connection.SendAsync("requestCompletions", BuildDocumentEnvelope(code, sessionId: sessionId));

        int position = code.IndexOf("WriteLine", StringComparison.Ordinal);
        Assert.True(position >= 0);
        await connection.SendAsync("requestHover", new
        {
            doc = BuildDoc(sessionId),
            position
        });

        var payload = await hoverTcs.Task.WaitAsync(DefaultTimeout);
        Assert.True(payload.TryGetProperty("contents", out var contents));
        Assert.Contains("Console.WriteLine", contents.GetString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task RequestSignatureHelp_Returns_Signatures()
    {
        await using var connection = await CreateAndStartConnectionAsync();
        TaskCompletionSource<JsonElement> signatureTcs = NewTcs<JsonElement>();
        connection.On<JsonElement>("signatureHelp", payload => signatureTcs.TrySetResult(payload));

        const string code = "Console.WriteLine(\"hello\", 42);";
        string sessionId = Guid.NewGuid().ToString("N");
        await connection.SendAsync("requestCompletions", BuildDocumentEnvelope(code, sessionId: sessionId));

        int position = code.IndexOf("(", StringComparison.Ordinal) + 1;
        await connection.SendAsync("requestSignatureHelp", new
        {
            doc = BuildDoc(sessionId),
            position
        });

        var payload = await signatureTcs.Task.WaitAsync(DefaultTimeout);
        Assert.True(payload.TryGetProperty("signatures", out var signatures));
        Assert.True(signatures.GetArrayLength() > 0);
        var first = signatures[0];
        Assert.True(first.TryGetProperty("label", out var label));
        Assert.False(string.IsNullOrWhiteSpace(label.GetString()));
    }
    */
}