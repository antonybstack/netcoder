using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CodeApi.Hubs;
using CodeApi.Models.Intellisense;
using CodeApi.Models.Intellisense.Requests;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
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

    protected async Task<HubConnection> CreateAndStartConnectionAsync(CancellationToken ct)
    {
        Uri hubUrl = new(Factory.Server.BaseAddress!, "/hubs/intellisense");
        HubConnection connection = new HubConnectionBuilder()
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
            .AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.PayloadSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            })
            .WithAutomaticReconnect()
            .Build();

        await connection.StartAsync(ct);
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

    protected static IntellisenseTextRequest BuildIntellisenseTextRequest(string content, int? cursorOffset = null, string? sessionId = null)
    {
        return new IntellisenseTextRequest
        {
            Doc = BuildDocumentRef(sessionId),
            Text = BuildTextState(content, cursorOffset)
        };
    }

    protected static DocumentRef BuildDocumentRef(string? sessionId = null)
    {
        return new DocumentRef
        {
            SessionId = sessionId ?? Guid.NewGuid().ToString("N"),
            LanguageVersion = "C#14"
        };
    }

    protected static TextState BuildTextState(string content, int? cursorOffset = null)
    {
        return new TextState
        {
            Content = content,
            CursorOffset = cursorOffset ?? content.Length
        };
    }

    protected static TaskCompletionSource<T> NewTcs<T>()
    {
        return new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}