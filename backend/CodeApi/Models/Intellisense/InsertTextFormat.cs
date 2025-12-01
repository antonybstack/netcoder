using System.Text.Json.Serialization;

namespace CodeApi.Models.Intellisense;

// [JsonConverter(typeof(JsonStringEnumConverter))]
public enum InsertTextFormat
{
    PlainText = 1,
    Snippet = 2
}