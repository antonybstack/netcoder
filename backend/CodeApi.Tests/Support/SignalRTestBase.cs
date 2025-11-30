using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CodeApi.Tests.Support;

public abstract class SignalRTestBase : IClassFixture<WebApplicationFactory<Program>>
{
    protected static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5); // TODO: revert to 5

    protected WebApplicationFactory<Program> Factory { get; }

    protected SignalRTestBase(WebApplicationFactory<Program> factory)
    {
        Factory = factory;
    }

    protected async Task<HubConnection> CreateAndStartConnectionAsync()
    {
        var hubUrl = new Uri(Factory.Server.BaseAddress!, "/hubs/intellisense");
        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
                options.Transports = HttpTransportType.LongPolling;
            })
            .ConfigureLogging(logging =>
            {
                // Log to the Console
                logging.AddConsole();

                // Log to the Output Window
                logging.AddDebug();

                // This will set ALL logging to Debug level
                logging.SetMinimumLevel(LogLevel.Debug);
            })
            .WithAutomaticReconnect()
            .Build();

        await connection.StartAsync();
        return connection;
    }

    protected static object BuildDoc(string? sessionId = null)
    {
        return new
        {
            sessionId = sessionId ?? Guid.NewGuid().ToString("N"),
            languageVersion = "C#14"
        };
    }

    protected static object BuildText(string content, int? cursorOffset = null)
    {
        return new
        {
            content,
            cursorOffset = cursorOffset ?? content.Length
        };
    }

    protected static object BuildDocumentEnvelope(string content, int? cursorOffset = null, string? sessionId = null)
    {
        return new
        {
            doc = BuildDoc(sessionId),
            text = BuildText(content, cursorOffset)
        };
    }

    protected static TaskCompletionSource<T> NewTcs<T>()
    {
        return new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}