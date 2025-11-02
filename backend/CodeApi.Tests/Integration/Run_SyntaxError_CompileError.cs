using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class Run_SyntaxError_CompileError
{
    [Fact]
    public async Task Should_Return_CompileError_With_Diagnostics()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var payload = new { code = "Console.WriteLine(\"OOPS\"" };
        var response = await client.PostAsJsonAsync("/api/exec/run", payload);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.NotNull(json);
        Assert.Equal("CompileError", json!["outcome"]!.GetValue<string>());
        var diagnostics = json["diagnostics"]!.AsArray();
        Assert.True(diagnostics.Count > 0);
        var diagnostic = diagnostics[0]!.AsObject();
        Assert.Equal("Error", diagnostic["severity"]!.GetValue<string>());
        Assert.InRange(diagnostic["line"]!.GetValue<int>(), 1, int.MaxValue);
        Assert.InRange(diagnostic["column"]!.GetValue<int>(), 1, int.MaxValue);
        Assert.False(string.IsNullOrWhiteSpace(diagnostic["message"]!.GetValue<string>()));
        Assert.False(string.IsNullOrWhiteSpace(diagnostic["id"]!.GetValue<string>()));
    }
}
