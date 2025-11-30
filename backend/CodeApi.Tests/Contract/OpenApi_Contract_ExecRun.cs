using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class OpenApi_Contract_ExecRun
{
    [Fact]
    public async Task Response_Has_Required_Fields()
    {
        await using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        var payload = new { code = "Console.WriteLine(\"x\");" };
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/exec/run", payload);
        response.EnsureSuccessStatusCode();
        JsonObject? json = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.NotNull(json);
        Assert.True(json!.ContainsKey("outcome"));
        Assert.True(json.ContainsKey("stdout"));
        Assert.True(json.ContainsKey("stderr"));
        Assert.True(json.ContainsKey("diagnostics"));
        Assert.True(json.ContainsKey("durationMs"));
        Assert.True(json.ContainsKey("truncated"));

        Assert.IsType<string>(json["outcome"]!.GetValue<string>());
        Assert.IsType<string>(json["stdout"]!.GetValue<string>());
        Assert.IsType<string>(json["stderr"]!.GetValue<string>());
        Assert.IsType<int>(json["durationMs"]!.GetValue<int>());
        Assert.IsType<bool>(json["truncated"]!.GetValue<bool>());

        JsonArray diagnostics = json["diagnostics"]!.AsArray();
        foreach (JsonNode? diagnosticNode in diagnostics)
        {
            JsonObject diagnostic = diagnosticNode!.AsObject();
            Assert.True(diagnostic.ContainsKey("id"));
            Assert.True(diagnostic.ContainsKey("severity"));
            Assert.True(diagnostic.ContainsKey("message"));
            Assert.True(diagnostic.ContainsKey("line"));
            Assert.True(diagnostic.ContainsKey("column"));

            Assert.False(string.IsNullOrWhiteSpace(diagnostic["id"]!.GetValue<string>()));
            Assert.False(string.IsNullOrWhiteSpace(diagnostic["message"]!.GetValue<string>()));
            Assert.InRange(diagnostic["line"]!.GetValue<int>(), 1, int.MaxValue);
            Assert.InRange(diagnostic["column"]!.GetValue<int>(), 1, int.MaxValue);
        }
    }
}
