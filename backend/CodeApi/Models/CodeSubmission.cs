namespace CodeApi.Models;

public class CodeSubmission
{
    public string Code { get; set; } = string.Empty;
    public string? RequestId { get; set; }
    public DateTime SubmittedAt { get; set; }
}
