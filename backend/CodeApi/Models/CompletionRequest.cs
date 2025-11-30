namespace CodeApi.Models;

public class CompletionRequest
{
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 1-based line number (Monaco style).
    /// </summary>
    public int LineNumber { get; set; } = 1;

    /// <summary>
    /// 1-based column number (Monaco style).
    /// </summary>
    public int Column { get; set; } = 1;
}
