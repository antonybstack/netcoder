using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class OpenApi_Contract_ExecRun
{
    [Fact]
    public async Task Response_Has_Required_Fields()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var payload = new { code = "Console.WriteLine(\"x\");" };
        var response = await client.PostAsJsonAsync("/api/exec/run", payload);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.NotNull(json);
        Assert.True(json!.ContainsKey("outcome"));
        Assert.True(json!.ContainsKey("stdout"));
        Assert.True(json!.ContainsKey("stderr"));
        Assert.True(json!.ContainsKey("diagnostics"));
        Assert.True(json!.ContainsKey("durationMs"));
        Assert.True(json!.ContainsKey("truncated"));
    }
}
