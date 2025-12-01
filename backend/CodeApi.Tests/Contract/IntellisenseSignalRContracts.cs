using CodeApi.Hubs;
using CodeApi.Models.Intellisense;
using CodeApi.Models.Intellisense.Requests;
using CodeApi.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CodeApi.Tests.Contract;

public class IntellisenseSignalRContracts : SignalRTestBase
{
    private readonly CancellationToken CancellationToken;

    public IntellisenseSignalRContracts(WebApplicationFactory<Program> factory) : base(factory)
    {
        CancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
    }

    [Theory]
    [InlineData("Guid.")]
    [InlineData("Console.")]
    [InlineData("Console.WriteLi")]
    public async Task RequestCompletions_Returns_Items_With_Required_Fields(string code)
    {
        await using HubConnection connection = await CreateAndStartConnectionAsync(CancellationToken);

        IntellisenseTextRequest request = BuildIntellisenseTextRequest(code);

        CompletionsResponse payload = await connection.SendAndWaitAsync<CompletionsResponse>(
            nameof(IntellisenseHub.RequestCompletions),
            request,
            IntellisenseHub.CompletionsResponseMethod,
            CancellationToken);

        Assert.True(payload.Items.Count > 0);
        foreach (AppCompletionItem item in payload.Items)
        {
            Assert.NotNull(item.DisplayText);
            Assert.False(string.IsNullOrWhiteSpace(item.DisplayText));
            Assert.NotNull(item.Kind);
            Assert.False(string.IsNullOrWhiteSpace(item.Kind));
        }
    }
}

public static class HubConnectionExtensions
{
    public static async Task<T> SendAndWaitAsync<T>(
        this HubConnection connection,
        string sendMethod,
        object? sendArgs,
        string responseMethod,
        CancellationToken ct)
    {
        TaskCompletionSource<T> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

        IDisposable? disposable = null;
        disposable = connection.On<T>(responseMethod, response =>
        {
            disposable?.Dispose();
            tcs.TrySetResult(response);
        });

        await connection.SendAsync(sendMethod, sendArgs, ct);

        return await tcs.Task.WaitAsync(ct);
    }
}