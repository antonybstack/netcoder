using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using CodeApi.Models.Intellisense;
using CodeApi.Models.Intellisense.Requests;
using CodeApi.Services;
using Microsoft.AspNetCore.SignalR;

namespace CodeApi.Hubs;

public partial class IntellisenseHub(IRoslynCompletionService service, ILogger<IntellisenseHub> logger) : Hub
{
    public const string CompletionsResponseMethod = "completions";
    private static readonly ConcurrentDictionary<string, SessionEntry> SessionTexts = new();
    private static readonly ConcurrentDictionary<string, DebounceState> DebounceStates = new();
    private static readonly TimeSpan DebounceWindow = TimeSpan.FromMilliseconds(35);
    private static readonly Meter HubMeter = new("CodeApi.IntellisenseHub");
    private static readonly Counter<long> ConnectionsCounter = HubMeter.CreateCounter<long>("intellisense_connections");
    private static readonly Counter<long> DisconnectionsCounter = HubMeter.CreateCounter<long>("intellisense_disconnections");
    private static readonly Histogram<double> RequestLatency = HubMeter.CreateHistogram<double>("intellisense_request_duration_ms");


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

    // _ct helper method that retrieves Context.ConnectionAborted
    private CancellationToken _ct => Context.ConnectionAborted;

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
        using var scope = BeginScope(payload.Doc.SessionId, correlationId);

        await ExecuteWithMetricsAsync(CompletionsResponseMethod, async (ct) =>
        {
            try
            {
                IReadOnlyList<AppCompletionItem> result = await service.GetCompletionsScript(payload.Text.Content, ct);
                await Clients.Caller.SendAsync(CompletionsResponseMethod, new CompletionsResponse(result), ct);
                await SendStatusAsync("connected", false, "Realtime service operational");
            }
            catch (IntellisenseUnavailableException ex)
            {
                logger.LogWarning(ex, "Intellisense unavailable; sending local fallback");
                await SendStatusAsync("reconnecting", true, ex.Message);
                IReadOnlyList<AppCompletionItem> fallback = BuildLocalFallback(payload.Text);
                await Clients.Caller.SendAsync(CompletionsResponseMethod, new CompletionsResponse(fallback), ct);
                await SendStatusAsync("connected", false, "Realtime service restored");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to compute {CompletionsResponseMethod}");
                await SendStatusAsync("error", false, $"Unable to provide {CompletionsResponseMethod}");
            }
        }, _ct);
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
        using var scope = BeginScope(payload.Doc.SessionId, correlationId);

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
        }, _ct);
    }

    public async Task RequestHover(IntellisensePositionRequest payload)
    {
        if (!Validate(payload))
        {
            return;
        }

        if (!TryGetSessionText(payload.Doc, out var text))
        {
            await SendStatusAsync("error", false, "No document state available for hover");
            return;
        }

        string correlationId = Guid.NewGuid().ToString("N");
        using var scope = BeginScope(payload.Doc.SessionId, correlationId);

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
        }, _ct);
    }

    public async Task RequestSignatureHelp(IntellisensePositionRequest payload)
    {
        if (!Validate(payload))
        {
            return;
        }

        if (!TryGetSessionText(payload.Doc, out var text))
        {
            await SendStatusAsync("error", false, "No document state available for signature help");
            return;
        }

        string correlationId = Guid.NewGuid().ToString("N");
        using var scope = BeginScope(payload.Doc.SessionId, correlationId);

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
        }, _ct);
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
        if (SessionTexts.TryGetValue(doc.SessionId, out var entry) && entry.ConnectionId == Context.ConnectionId)
        {
            text = entry.Text ?? new TextState();
            return true;
        }

        return false;
    }

    private void TrackText(DocumentRef doc, TextState text)
    {
        var snapshot = new TextState
        {
            Content = text.Content ?? string.Empty,
            CursorOffset = text.CursorOffset
        };
        SessionTexts[doc.SessionId] = new SessionEntry(Context.ConnectionId, snapshot);
    }

    private bool ShouldProcess(DocumentRef doc, TextState text)
    {
        var state = DebounceStates.GetOrAdd(doc.SessionId, _ => new DebounceState());
        int hash = HashCode.Combine(text.Content, text.CursorOffset);
        lock (state)
        {
            var now = DateTime.UtcNow;
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
        return Clients.Caller.SendAsync("statusChanged", new
        {
            status,
            localOnly,
            message
        }, _ct);
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

    private static IReadOnlyList<AppCompletionItem> BuildLocalFallback(TextState text)
    {
        return [];
        //{
        // new CustomCompletionItem()
        // {
        //     Label = "Console.WriteLine",
        //     InsertText = "Console.WriteLine",
        //     Kind = "method",
        //     Detail = "Local fallback suggestion (syntax-only)"
        // }
        //};
    }

    private async Task ExecuteWithMetricsAsync(string operation, Func<CancellationToken, Task> callback, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
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