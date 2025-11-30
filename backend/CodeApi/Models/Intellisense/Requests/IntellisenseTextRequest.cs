namespace CodeApi.Models.Intellisense.Requests;

public class IntellisenseTextRequest
{
    public DocumentRef Doc { get; set; } = new();

    public TextState Text { get; set; } = new();
}
