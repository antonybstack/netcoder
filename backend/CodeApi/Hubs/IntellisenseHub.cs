using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using CodeApi.Models.Intellisense;
using CodeApi.Models.Intellisense.Requests;
using CodeApi.Services;
using Microsoft.AspNetCore.SignalR;
using TypedSignalR.Client;

namespace CodeApi.Hubs;

[Hub]
public interface IIntellisenseHub
{
    // Task OnConnectedAsync();
    // Task OnDisconnectedAsync(Exception? exception);
    // Task RequestDiagnostics(IntellisenseTextRequest payload);
    // Task RequestHover(IntellisensePositionRequest payload);
    // Task RequestSignatureHelp(IntellisensePositionRequest payload);
}

[Receiver]
public interface IIntellisenseReceiver
{
    // Task OnConnectedAsync();
    // Task OnDisconnectedAsync(Exception? exception);
    [HubMethodName("RequestCompletions")]
    Task RequestCompletions(IntellisenseTextRequest payload);
    // Task RequestDiagnostics(IntellisenseTextRequest payload);
    // Task RequestHover(IntellisensePositionRequest payload);
    // Task RequestSignatureHelp(IntellisensePositionRequest payload);
}

public interface IIntellisenseSender
{
    // Task OnConnectedAsync();
    // Task OnDisconnectedAsync(Exception? exception);
    [HubMethodName("Completions")]
    Task Completions(CompletionsResponse response);

    Task StatusChanged(object status, CancellationToken ct);
    // Task RequestDiagnostics(IntellisenseTextRequest payload);
    // Task RequestHover(IntellisensePositionRequest payload);
    // Task RequestSignatureHelp(IntellisensePositionRequest payload);
}

public partial class IntellisenseHub(
    IRoslynCompletionService service,
    ILogger<IntellisenseHub> logger)
    : Hub<IIntellisenseSender>, IIntellisenseReceiver
{
    public const string CompletionsResponseMethod = nameof(IIntellisenseSender.Completions);
    private static readonly ConcurrentDictionary<string, SessionEntry> SessionTexts = new();
    private static readonly ConcurrentDictionary<string, DebounceState> DebounceStates = new();
    private static readonly TimeSpan DebounceWindow = TimeSpan.FromMilliseconds(35);
    private static readonly Meter HubMeter = new("CodeApi.IntellisenseHub");
    private static readonly Counter<long> ConnectionsCounter = HubMeter.CreateCounter<long>("intellisense_connections");
    private static readonly Counter<long> DisconnectionsCounter = HubMeter.CreateCounter<long>("intellisense_disconnections");
    private static readonly Histogram<double> RequestLatency = HubMeter.CreateHistogram<double>("intellisense_request_duration_ms");

    private CancellationToken CancellationToken => Context.ConnectionAborted;

    public override Task OnConnectedAsync()
    {
        logger.LogInformation("Connection {ConnectionId} connected to Intellisense hub", Context.ConnectionId);
        ConnectionsCounter.Add(1);
        return SendStatusAsync("connected", false, "Realtime C# intellisense ready");
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        DisconnectionsCounter.Add(1);
        foreach (KeyValuePair<string, SessionEntry> kvp in SessionTexts.Where(kvp => kvp.Value.ConnectionId == Context.ConnectionId).ToList())
        {
            SessionTexts.TryRemove(kvp.Key, out _);
            DebounceStates.TryRemove(kvp.Key, out _);
        }

        return Task.CompletedTask;
    }

    public async Task RequestCompletions(IntellisenseTextRequest payload)
    {
        if (!Validate(payload))
        {
            return;
        }

        if (!ShouldProcess(payload.Doc, payload.Text))
        {
            return;
        }

        TrackText(payload.Doc, payload.Text);
        string correlationId = Guid.NewGuid().ToString("N");
        using IDisposable scope = BeginScope(payload.Doc.SessionId, correlationId);

        await ExecuteWithMetricsAsync(CompletionsResponseMethod, async (ct) =>
        {
            try
            {
                IReadOnlyList<AppCompletionItem> result = await service.GetCompletionsScript(payload.Text.Content, ct);
                // await Clients.Caller.SendAsync(IntellisenseHub.CompletionsResponseMethod, new CompletionsResponse(result), ct);
                CompletionsResponse response = new(result);
                await Clients.Caller.Completions(response);
                // await SendStatusAsync("connected", false, "Realtime service operational");
            }
            catch (IntellisenseUnavailableException ex)
            {
                logger.LogWarning(ex, "Intellisense unavailable");
                // await SendStatusAsync("reconnecting", true, ex.Message);
                // await Clients.Caller.Completions(new CompletionsResponse(, ct);
                // await SendStatusAsync("connected", false, "Realtime service restored");
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to compute {CompletionsResponseMethod}");
                await SendStatusAsync("error", false, $"Unable to provide {CompletionsResponseMethod}");
                throw;
            }
        }, CancellationToken);
    }

    public async Task RequestDiagnostics(IntellisenseTextRequest payload)
    {
        if (!Validate(payload))
        {
            return;
        }

        if (!ShouldProcess(payload.Doc, payload.Text))
        {
            return;
        }

        TrackText(payload.Doc, payload.Text);
        string correlationId = Guid.NewGuid().ToString("N");
        using IDisposable scope = BeginScope(payload.Doc.SessionId, correlationId);

        await ExecuteWithMetricsAsync("diagnostics", async (ct) =>
        {
            try
            {
                // TODO
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to compute diagnostics");
                await SendStatusAsync("error", false, "Unable to provide diagnostics");
            }
        }, CancellationToken);
    }

    public async Task RequestHover(IntellisensePositionRequest payload)
    {
        if (!Validate(payload))
        {
            return;
        }

        if (!TryGetSessionText(payload.Doc, out TextState text))
        {
            await SendStatusAsync("error", false, "No document state available for hover");
            return;
        }

        string correlationId = Guid.NewGuid().ToString("N");
        using IDisposable scope = BeginScope(payload.Doc.SessionId, correlationId);

        await ExecuteWithMetricsAsync("hover", async (ct) =>
        {
            try
            {
                // TODO
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to compute hover info");
                await SendStatusAsync("error", false, "Unable to provide hover info");
            }
        }, CancellationToken);
    }

    public async Task RequestSignatureHelp(IntellisensePositionRequest payload)
    {
        if (!Validate(payload))
        {
            return;
        }

        if (!TryGetSessionText(payload.Doc, out TextState text))
        {
            await SendStatusAsync("error", false, "No document state available for signature help");
            return;
        }

        string correlationId = Guid.NewGuid().ToString("N");
        using IDisposable scope = BeginScope(payload.Doc.SessionId, correlationId);

        await ExecuteWithMetricsAsync("signatureHelp", async (ct) =>
        {
            try
            {
                // TODO
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to compute signature help");
                await SendStatusAsync("error", false, "Unable to provide signature help");
            }
        }, CancellationToken);
    }

    private bool Validate(IntellisenseTextRequest? payload)
    {
        if (payload?.Doc is null || payload.Text is null)
        {
            logger.LogWarning("Received invalid text payload");
            return false;
        }

        return !string.IsNullOrWhiteSpace(payload.Doc.SessionId);
    }

    private bool Validate(IntellisensePositionRequest? payload)
    {
        if (payload?.Doc is null)
        {
            logger.LogWarning("Received invalid position payload");
            return false;
        }

        return !string.IsNullOrWhiteSpace(payload.Doc.SessionId);
    }

    private bool TryGetSessionText(DocumentRef doc, out TextState text)
    {
        text = new TextState();
        if (SessionTexts.TryGetValue(doc.SessionId, out SessionEntry? entry) && entry.ConnectionId == Context.ConnectionId)
        {
            text = entry.Text ?? new TextState();
            return true;
        }

        return false;
    }

    private void TrackText(DocumentRef doc, TextState text)
    {
        TextState snapshot = new()
        {
            Content = text.Content ?? string.Empty,
            CursorOffset = text.CursorOffset
        };
        SessionTexts[doc.SessionId] = new SessionEntry(Context.ConnectionId, snapshot);
    }

    private bool ShouldProcess(DocumentRef doc, TextState text)
    {
        DebounceState state = DebounceStates.GetOrAdd(doc.SessionId, _ => new DebounceState());
        int hash = HashCode.Combine(text.Content, text.CursorOffset);
        lock (state)
        {
            DateTime now = DateTime.UtcNow;
            if (state.Hash == hash && now - state.Timestamp < DebounceWindow)
            {
                return false;
            }

            state.Hash = hash;
            state.Timestamp = now;
            return true;
        }
    }

    private Task SendStatusAsync(string status, bool localOnly, string? message = null)
    {
        return Clients.Caller.StatusChanged(new
        {
            status,
            localOnly,
            message
        }, CancellationToken);
    }

    private IDisposable BeginScope(string sessionId, string correlationId)
    {
        Dictionary<string, object> scope = new()
        {
            ["ConnectionId"] = Context.ConnectionId,
            ["SessionId"] = sessionId,
            ["CorrelationId"] = correlationId
        };

        return logger.BeginScope(scope)!;
    }

    private async Task ExecuteWithMetricsAsync(string operation, Func<CancellationToken, Task> callback, CancellationToken cancellationToken)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await callback(cancellationToken);
        }
        finally
        {
            stopwatch.Stop();
            RequestLatency.Record(stopwatch.Elapsed.TotalMilliseconds, new KeyValuePair<string, object?>("operation", operation));
        }
    }

    private sealed record SessionEntry(string ConnectionId, TextState Text);

    private sealed class DebounceState
    {
        public int Hash { get; set; }

        public DateTime Timestamp { get; set; }
    }
}