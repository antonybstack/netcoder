using Tapper;

namespace CodeApi.Models.Intellisense.Requests;

[TranspilationSource]
public class IntellisenseTextRequest
{
    public DocumentRef Doc { get; set; } = new();

    public TextState Text { get; set; } = new();
}