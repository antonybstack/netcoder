namespace CodeApi.Models.Intellisense;

public class EditorSession
{
    public string SessionId { get; set; } = string.Empty;

    public string Status { get; set; } = "connected";

    public bool LocalOnly { get; set; }

    public double? LatencyP95 { get; set; }
}
