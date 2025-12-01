using Tapper;

namespace CodeApi.Models.Intellisense;

// [JsonConverter(typeof(JsonStringEnumConverter))]
[TranspilationSource]
public enum InsertTextFormat
{
    PlainText = 1,
    Snippet = 2
}