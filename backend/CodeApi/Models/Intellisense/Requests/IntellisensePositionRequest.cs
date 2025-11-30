namespace CodeApi.Models.Intellisense.Requests;

public class IntellisensePositionRequest
{
    public DocumentRef Doc { get; set; } = new();

    public int Position { get; set; }
}
