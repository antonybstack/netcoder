using Tapper;

namespace CodeApi.Models.Intellisense;

[TranspilationSource]
public class DocumentRef
{
    public string SessionId { get; set; } = string.Empty;

    public string LanguageVersion { get; set; } = "C#14";
}