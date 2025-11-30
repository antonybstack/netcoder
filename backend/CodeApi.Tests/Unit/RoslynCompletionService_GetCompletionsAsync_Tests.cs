using CodeApi.Models.Intellisense;
using CodeApi.Services;
using Xunit;

public class RoslynCompletionService_GetCompletionsAsync_Tests
{
    [Fact]
    public async Task Returns_Empty_For_Empty_Code()
    {
        IRoslynCompletionService service = new RoslynCompletionService();

        IReadOnlyList<AppCompletionItem> result = await service.GetCompletionsAsync(string.Empty, string.Empty);

        Assert.Empty(result);
    }

    [Theory]
    [MemberData(nameof(GetCompletionDataTestCase1))]
    [MemberData(nameof(GetCompletionDataTestCase2))]
    public async Task GetCompletionsAsync_FullCodeBlock_ReturnsCompletions((string code, string currentWord, string[] expected) input)
    {
        IRoslynCompletionService service = new RoslynCompletionService();
        (string code, string currentWord, string[] expected) = input;

        IReadOnlyList<AppCompletionItem> result = await service.GetCompletionsAsync(code, currentWord);

        string[] actual = result.Select(i => i.DisplayText).ToArray();

        Assert.NotEmpty(result);
        Assert.All(expected, item => Assert.Contains(item, actual));
    }


    public static TheoryData<(string, string, string[])> GetCompletionDataTestCase1()
    {
        string currentWord = "Guid.";
        string code = """
                      using System;

                      public class MyClass
                      {
                          public static void MyMethod(int value)
                          {
                              Guid.
                          }
                      }
                      """;
        string[] expected = ["NewGuid", "Empty", "TryParse"];
        return [(code, currentWord, expected)];
    }

    public static TheoryData<(string, string, string[])> GetCompletionDataTestCase2()
    {
        string currentWord = "Console.WriteL";
        string code = """
                      using System;

                      public class MyClass
                      {
                          public static void MyMethod(int value)
                          {
                              Console.WriteL
                          }
                      }
                      """;
        string[] expected = ["WriteLine", "Write"];
        return [(code, currentWord, expected)];
    }

    [Fact]
    public async Task GetCompletionsAsync_PartialCode_ReturnsCompletions()
    {
        IRoslynCompletionService service = new RoslynCompletionService();

        const string currentWord = "Guid.";

        IReadOnlyList<AppCompletionItem> result = await service.GetCompletionsScript(currentWord, CancellationToken.None);


        Assert.NotEmpty(result);
        Assert.Contains("NewGuid", result.Select(i => i.DisplayText));
        Assert.Contains("Empty", result.Select(i => i.DisplayText));
        Assert.Contains("TryParse", result.Select(i => i.DisplayText));
    }

    // [Fact]
    // public async Task Console_WriteLine_Suggestion_Is_Returned()
    // {
    //     IRoslynCompletionService service = new RoslynCompletionService();
    //     string code = "Console.Wri";
    //
    //     IReadOnlyList<CustomCompletionItem> result = await service.GetCompletionsAsync(code, code.Length - 1);
    //
    //     string[] labels = result.Select(i => i.DisplayText).ToArray();
    //     Assert.Contains("Console.WriteLine", labels);
    // }
}