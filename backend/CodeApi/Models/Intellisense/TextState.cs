using Tapper;

namespace CodeApi.Models.Intellisense;

[TranspilationSource]
public class TextState
{
    public string Content { get; set; } = string.Empty;

    public int CursorOffset { get; set; }
}