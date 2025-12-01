using System.Text.Json;
using CodeApi.Hubs;
using CodeApi.Models.Intellisense;
using CodeApi.Models.Intellisense.Requests;
using CodeApi.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CodeApi.Tests.Contract;

public class IntellisenseSignalRContracts : SignalRTestBase
{
    private readonly CancellationToken _cancellationToken;

    public IntellisenseSignalRContracts(WebApplicationFactory<Program> factory) : base(factory)
    {
        _cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
    }

    [Theory]
    [InlineData("Guid.")]
    public async Task RequestCompletions_Returns_Items_With_Required_Fields(string code)
    {
        await using HubConnection connection = await CreateAndStartConnectionAsync(_cancellationToken);
        connection.On<CompletionsResponse>(IntellisenseHub.CompletionsResponseMethod, payload =>
        {
            Assert.True(payload.Items.Count > 0);
            foreach (AppCompletionItem item in payload.Items)
            {
                Assert.NotNull(item.DisplayText);
                Assert.False(string.IsNullOrWhiteSpace(item.DisplayText));
                Assert.NotNull(item.Kind);
                Assert.False(string.IsNullOrWhiteSpace(item.Kind));
            }
        });

        IntellisenseTextRequest buildIntellisenseTextRequest = BuildIntellisenseTextRequest(code);

        await connection.SendAsync(nameof(IntellisenseHub.RequestCompletions), buildIntellisenseTextRequest, _cancellationToken);
    }

    [Theory]
    // [InlineData("Console.WriteLi")] // TODO: not working yet
    [InlineData("Console.")]
    public async Task RequesConsoleWriteLineCompletions_Returns_Items_With_Required_Fields(string code)
    {
        await using HubConnection connection = await CreateAndStartConnectionAsync(_cancellationToken);
        connection.On<CompletionsResponse>(IntellisenseHub.CompletionsResponseMethod, payload =>
        {
            Assert.True(payload.Items.Count > 0);
            foreach (AppCompletionItem item in payload.Items)
            {
                Assert.NotNull(item.DisplayText);
                Assert.False(string.IsNullOrWhiteSpace(item.DisplayText));
                Assert.NotNull(item.Kind);
                Assert.False(string.IsNullOrWhiteSpace(item.Kind));
            }
        });

        IntellisenseTextRequest buildIntellisenseTextRequest = BuildIntellisenseTextRequest(code);
        await connection.SendAsync(nameof(IntellisenseHub.RequestCompletions), buildIntellisenseTextRequest, _cancellationToken);
    }

    [Theory]
    [InlineData("Guid.")]
    [InlineData("Console.")]
    [InlineData("Console.WriteLi")]
    // [InlineData("Guid.")]
    public async Task RequestCompletions_Returns_Items_With_Required_Fields_Test(string code)
    {
        await using HubConnection connection = await CreateAndStartConnectionAsync(_cancellationToken);

        TaskCompletionSource<CompletionsResponse> tcs = NewTcs<CompletionsResponse>();
        connection.On<CompletionsResponse>(IntellisenseHub.CompletionsResponseMethod, payload => { tcs.TrySetResult(payload); });

        await connection.SendAsync(nameof(IntellisenseHub.RequestCompletions), BuildIntellisenseTextRequest(code), _cancellationToken);

        CompletionsResponse payload = await tcs.Task.WaitAsync(DefaultTimeout, _cancellationToken);

        Assert.True(payload.Items.Count > 0);
        foreach (AppCompletionItem item in payload.Items)
        {
            Assert.NotNull(item.DisplayText);
            Assert.False(string.IsNullOrWhiteSpace(item.DisplayText));
            Assert.NotNull(item.Kind);
            Assert.False(string.IsNullOrWhiteSpace(item.Kind));
        }
    }

    /*[Theory]
    // [InlineData("Console.WriteLi")] // TODO: not working yet
    [InlineData("Guid.")]
    public async Task RequestCompletions_Returns_Items_With_Required_Fields_Dynamic(string code)
    {
        await using HubConnection connection = await CreateAndStartConnectionAsync();

        TaskCompletionSource<CompletionsResponse> tcs = NewTcs<CompletionsResponse>();
        connection.On<dynamic>(IntellisenseHub.CompletionsResponseMethod, payload =>
        {
            Console.WriteLine($"Received {payload.Items.Count} completion items.");
            tcs.TrySetResult(payload);
        });

        await connection.SendAsync(nameof(IntellisenseHub.RequestCompletions), BuildIntellisenseTextRequest(code));

        CompletionsResponse payload = await tcs.Task.WaitAsync(DefaultTimeout);
        Assert.NotNull(item.Kind);
        Assert.False(string.IsNullOrWhiteSpace(item.Kind));
    }*/
}

/*[Theory]
[InlineData("Guid.")]
public async Task RequestCompletions_Returns_Json_Items_With_Required_Fields(string code)
{
    await using HubConnection connection = await CreateAndStartConnectionAsync();
    TaskCompletionSource<JsonElement> tcs = NewTcs<JsonElement>();
    connection.On<JsonElement>(IntellisenseHub.CompletionsResponseMethod, payload => tcs.TrySetResult(payload));

    await connection.SendAsync(nameof(IntellisenseHub.RequestCompletions), BuildDocumentEnvelope(code));

    JsonElement payload = await tcs.Task.WaitAsync(DefaultTimeout);
    // JSON produced by the hub uses camelCase (items)
    Assert.True(payload.TryGetProperty("items", out JsonElement items));
    Assert.True(items.GetArrayLength() > 0);
    foreach (JsonElement item in items.EnumerateArray())
    {
        Assert.True(item.TryGetProperty("displayText", out JsonElement displayText));
        Assert.True(payload.TryGetProperty(nameof(CompletionsResponse.Items), out JsonElement items));
        Assert.False(string.IsNullOrWhiteSpace(kind.GetString()));
    }
}*/


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