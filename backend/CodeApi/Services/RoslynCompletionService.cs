using System.Collections.Immutable;
using System.Reflection;
using CodeApi.Models.Intellisense;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Recommendations;
using Microsoft.CodeAnalysis.Tags;
using AppDiagnostic = CodeApi.Models.Intellisense.Diagnostic;
using AppDiagnosticSeverity = CodeApi.Models.Intellisense.DiagnosticSeverity;
using Diagnostic = Microsoft.CodeAnalysis.Diagnostic;
using DiagnosticSeverity = Microsoft.CodeAnalysis.DiagnosticSeverity;

namespace CodeApi.Services;

public interface IRoslynCompletionService
{
    Task<IReadOnlyList<AppCompletionItem>> GetCompletionsAsync(
        string code,
        string currentWord,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AppDiagnostic>> GetDiagnosticsAsync(
        string code,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AppCompletionItem>> GetCompletionsScript(string code,
        CancellationToken ct);
}

public sealed class RoslynCompletionService : IRoslynCompletionService
{
    private readonly AdhocWorkspace _workspace;
    private readonly DocumentId _documentId;

    public RoslynCompletionService()
    {
//         /*
//          * Roslyn’s MEF services, as they are required to be populated for C# language services to work correctly.
//
//            The following assemblies are included in the default set:
//            “Microsoft.CodeAnalysis.Workspaces”,
//            “Microsoft.CodeAnalysis.CSharp.Workspaces”,
//            “Microsoft.CodeAnalysis.VisualBasic.Workspaces”,
//            “Microsoft.CodeAnalysis.Features”,
//            “Microsoft.CodeAnalysis.CSharp.Features”,
//            “Microsoft.CodeAnalysis.VisualBasic.Features”
//          */
        var host = MefHostServices.Create(MefHostServices.DefaultAssemblies);
        _workspace = new AdhocWorkspace(host);

        var projectInfo = ProjectInfo.Create(
                ProjectId.CreateNewId(),
                VersionStamp.Create(),
                "Intellisense",
                "Intellisense",
                LanguageNames.CSharp)
            .WithMetadataReferences(GetBaseReferences())
            .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithUsings("System", "System.Collections.Generic", "System.Linq", "System.Threading.Tasks"))
            .WithParseOptions(new CSharpParseOptions(LanguageVersion.Preview, DocumentationMode.Diagnose, SourceCodeKind.Script));

        var project = _workspace.AddProject(projectInfo);
        var document = _workspace.AddDocument(project.Id, "Script.cs", SourceText.From(string.Empty));
        _documentId = document.Id;
    }

    public async Task<IReadOnlyList<AppCompletionItem>> GetCompletionsAsync(
        string code,
        string currentWord,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return [];
        }

        var document = UpsertDocumentWithText(code);
        if (document is null)
        {
            return [];
        }

        var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        int cursorPositionPotential = text.ToString().LastIndexOf(currentWord, StringComparison.Ordinal) + currentWord.Length;
        int cursorPosition = Math.Clamp(cursorPositionPotential, 0, text.Length);

        var completionService = CompletionService.GetService(document);
        if (completionService is null)
        {
            return [];
        }

        var completionList = await completionService
            .GetCompletionsAsync(document, cursorPosition, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (completionList.ItemsList.Count == 0)
        {
            return [];
        }

        foreach (var i in completionList.ItemsList)
        {
            Console.WriteLine(i.DisplayText);

            foreach (KeyValuePair<string, string> prop in i.Properties)
            {
                Console.Write($"{prop.Key}:{prop.Value}  ");
            }

            Console.WriteLine();
            foreach (string tag in i.Tags)
            {
                Console.Write($"{tag}  ");
            }

            Console.WriteLine();
            Console.WriteLine();
        }

        List<AppCompletionItem> items = new(completionList.ItemsList.Count);
        foreach (var item in completionList.ItemsList)
        {
            var change = await completionService
                .GetChangeAsync(document, item, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            string? insertText = change.TextChange.NewText;
            if (string.IsNullOrEmpty(insertText))
            {
                insertText = item.DisplayText;
            }

            // items.Add(new CustomCompletionItem
            // {
            //     DisplayText = item.DisplayText,
            //     InsertText = insertText,
            //     Kind = MapRoslynTagToKind(item.Tags),
            //     Detail = item.InlieDescription,
            //     SortText = item.SortText ?? item.DisplayText
            // });
        }

        ImmutableArray<ISymbol> symbols = await Recommender.GetRecommendedSymbolsAtPositionAsync(
            document,
            cursorPosition,
            _workspace.Options,
            cancellationToken);

        Dictionary<(string, int), ISymbol> symbolToSymbolKey = new();

        foreach (var symbol in symbols)
        {
            var key = (symbol.Name, (int)symbol.Kind);
            symbolToSymbolKey.TryAdd(key, symbol);
        }

        // if (service is null)
        // {
        //     return new CompletionResult(requestId: request.RequestId, diagnostics: diagnostics);
        // }


        AppCompletionItem[] completionItems = completionList
            .ItemsList
            .Where(i => !i.IsComplexTextEdit)
            //.Deduplicate()
            .Select(item => item.ToModel(symbolToSymbolKey, document))
            .ToArray();

        // return new CompletionResult(
        //     items,
        //     requestId: request.RequestId);

        return completionItems;
    }

    public async Task<IReadOnlyList<AppCompletionItem>> GetCompletionsScript(string code,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return [];
        }

        var host = MefHostServices.Create(MefHostServices.DefaultAssemblies);
        var workspace = new AdhocWorkspace(host);

        var compilationOptions = new CSharpCompilationOptions(
            OutputKind.DynamicallyLinkedLibrary,
            usings: ["System"]);

        var scriptProjectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "Script", "Script", LanguageNames.CSharp, isSubmission: true)
            .WithMetadataReferences([
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            ])
            .WithCompilationOptions(compilationOptions);

        var scriptProject = workspace.AddProject(scriptProjectInfo);
        var scriptDocumentInfo = DocumentInfo.Create(
            DocumentId.CreateNewId(scriptProject.Id), "Script",
            sourceCodeKind: SourceCodeKind.Script,
            loader: TextLoader.From(TextAndVersion.Create(SourceText.From(code), VersionStamp.Create())));
        var scriptDocument = workspace.AddDocument(scriptDocumentInfo);

        // cursor position is at the end
        int position = (await scriptDocument.GetTextAsync(ct).ConfigureAwait(false)).Length;

        var completionService = CompletionService.GetService(scriptDocument);
        var completionList = await completionService.GetCompletionsAsync(scriptDocument, position, cancellationToken: ct);

        /*foreach (var i in completionList.ItemsList)
        {
            Console.WriteLine(i.DisplayText);

            foreach (KeyValuePair<string, string> prop in i.Properties)
            {
                Console.Write($"{prop.Key}:{prop.Value}  ");
            }

            Console.WriteLine();
            foreach (string tag in i.Tags)
            {
                Console.Write($"{tag}  ");
            }

            Console.WriteLine();
            Console.WriteLine();
        }*/


        List<AppCompletionItem> items = new(completionList.ItemsList.Count);
        foreach (var item in completionList.ItemsList)
        {
            var change = await completionService
                .GetChangeAsync(scriptDocument, item, cancellationToken: ct)
                .ConfigureAwait(false);

            string? insertText = change.TextChange.NewText;
            if (string.IsNullOrEmpty(insertText))
            {
                insertText = item.DisplayText;
            }

            // items.Add(new CustomCompletionItem
            // {
            //     DisplayText = item.DisplayText,
            //     InsertText = insertText,
            //     Kind = MapRoslynTagToKind(item.Tags),
            //     Detail = item.InlieDescription,
            //     SortText = item.SortText ?? item.DisplayText
            // });
        }

        ImmutableArray<ISymbol> symbols = await Recommender.GetRecommendedSymbolsAtPositionAsync(
            scriptDocument,
            position,
            workspace.Options,
            ct);

        Dictionary<(string, int), ISymbol> symbolToSymbolKey = new();

        foreach (var symbol in symbols)
        {
            var key = (symbol.Name, (int)symbol.Kind);
            symbolToSymbolKey.TryAdd(key, symbol);
        }

        // if (service is null)
        // {
        //     return new CompletionResult(requestId: request.RequestId, diagnostics: diagnostics);
        // }


        AppCompletionItem[] completionItems = completionList
            .ItemsList
            .Where(i => !i.IsComplexTextEdit)
            //.Deduplicate()
            .Select(item => item.ToModel(symbolToSymbolKey, scriptDocument))
            .ToArray();

        // return new CompletionResult(
        //     items,
        //     requestId: request.RequestId);

        return completionItems;
    }

    public async Task<IReadOnlyList<AppDiagnostic>> GetDiagnosticsAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return [];
        }

        var document = UpsertDocumentWithText(code);
        if (document is null)
        {
            return [];
        }

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
        {
            return [];
        }

        Diagnostic[] roslynDiagnostics = semanticModel
            .GetDiagnostics(cancellationToken: cancellationToken)
            .Where(d => d.Severity is DiagnosticSeverity.Error or DiagnosticSeverity.Warning)
            .ToArray();

        if (roslynDiagnostics.Length == 0)
        {
            return [];
        }

        return roslynDiagnostics.Select(d =>
        {
            var span = d.Location.SourceSpan;
            return new AppDiagnostic
            {
                Code = d.Id,
                Message = d.GetMessage(),
                Severity = d.Severity == DiagnosticSeverity.Error
                    ? AppDiagnosticSeverity.Error
                    : AppDiagnosticSeverity.Warning,
                Range = new TextRange
                {
                    Start = span.Start,
                    End = span.End
                }
            };
        }).ToList();
    }

    private Document? UpsertDocumentWithText(string code)
    {
        var document = _workspace.CurrentSolution.GetDocument(_documentId);
        return document?.WithText(SourceText.From(code));
    }

    private Document? GetDocumentWithText2(string code)
    {
        var projectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "MyProject", "MyProject", LanguageNames.CSharp).WithMetadataReferences(new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
        });
        var project = _workspace.AddProject(projectInfo);
        var document = _workspace.AddDocument(project.Id, "MyFile.cs", SourceText.From(code));
        return document;
    }

    private static IReadOnlyList<MetadataReference> GetBaseReferences()
    {
        List<MetadataReference> references = [];
        string coreLib = typeof(object).Assembly.Location;
        string? coreDir = Path.GetDirectoryName(coreLib);
        if (coreDir is null)
        {
            return references;
        }

        string[] dllsToLoad =
        [
            "System.Private.CoreLib.dll",
            "System.Runtime.dll",
            "System.Console.dll",
            "System.Linq.dll",
            "System.Collections.dll",
            "netstandard.dll"
        ];

        foreach (string dll in dllsToLoad)
        {
            string path = Path.Combine(coreDir, dll);
            if (File.Exists(path))
            {
                references.Add(MetadataReference.CreateFromFile(path));
            }
        }

        return references;
    }

    private static string MapRoslynTagToKind(ImmutableArray<string> tags)
    {
        string? tag = tags.FirstOrDefault();
        return tag switch
        {
            "Class" => "class",
            "Structure" => "struct",
            "Enum" => "enum",
            "Method" => "method",
            "Property" => "property",
            "Field" => "field",
            "Event" => "event",
            "Interface" => "interface",
            "Keyword" => "keyword",
            "Local" => "variable",
            "Parameter" => "variable",
            _ => "text"
        };
    }
}

internal static class CompletionExtensions
{
    private static readonly string SymbolCompletionProvider = "Microsoft.CodeAnalysis.CSharp.Completion.Providers.SymbolCompletionProvider";
    private static readonly string Provider = nameof(Provider);
    private static readonly string SymbolName = nameof(SymbolName);
    private static readonly string Symbols = nameof(Symbols);
    private static readonly string GetSymbolsAsync = nameof(GetSymbolsAsync);
    private static readonly PropertyInfo providerNameAccessor = typeof(CompletionItem).GetProperty("ProviderName", BindingFlags.NonPublic | BindingFlags.Instance);

    private static readonly ImmutableArray<string> KindTags =
    [
        WellKnownTags.Class,
        WellKnownTags.Constant,
        WellKnownTags.Delegate,
        WellKnownTags.Enum,
        WellKnownTags.EnumMember,
        WellKnownTags.Event,
        WellKnownTags.ExtensionMethod,
        WellKnownTags.Field,
        WellKnownTags.Interface,
        WellKnownTags.Intrinsic,
        WellKnownTags.Keyword,
        WellKnownTags.Label,
        WellKnownTags.Local,
        WellKnownTags.Method,
        WellKnownTags.Module,
        WellKnownTags.Namespace,
        WellKnownTags.Operator,
        WellKnownTags.Parameter,
        WellKnownTags.Property,
        WellKnownTags.RangeVariable,
        WellKnownTags.Reference,
        WellKnownTags.Structure,
        WellKnownTags.TypeParameter,
        WellKnownTags.Snippet
    ];

    extension(CompletionItem completionItem)
    {
        public string GetKind()
        {
            foreach (string tag in KindTags)
            {
                if (completionItem.Tags.Contains(tag))
                {
                    return tag;
                }
            }

            return null;
        }

        public AppCompletionItem ToModel(Dictionary<(string, int), ISymbol> recommendedSymbols,
            Document document)
        {
            var documentation = GetDocumentation(completionItem, recommendedSymbols, document);

            var symbol = GetCompletionSymbolAsync(completionItem, recommendedSymbols, document);

            return new AppCompletionItem(
                completionItem.DisplayText,
                completionItem.GetKind(),
                completionItem.FilterText,
                completionItem.SortText,
                completionItem.FilterText,
                InsertTextFormat.PlainText,
                symbol?.ToDisplayString() ?? string.Empty);
        }

        public ISymbol GetDocumentation(Dictionary<(string, int), ISymbol> recommendedSymbols,
            Document document)
        {
            var symbol = GetCompletionSymbolAsync(completionItem, recommendedSymbols, document);

            if (symbol is not null)
            {
                return symbol;
            }

            return null;
        }
    }

    public static ISymbol GetCompletionSymbolAsync(
        CompletionItem completionItem,
        Dictionary<(string, int), ISymbol> recommendedSymbols,
        Document document)
    {
        string provider = GetProviderName(completionItem);
        if (provider == SymbolCompletionProvider)
        {
            ImmutableDictionary<string, string> properties = completionItem.Properties;
            if (recommendedSymbols.TryGetValue((properties[SymbolName], int.Parse(properties[nameof(SymbolKind)])), out var symbol))
            {
                // We were able to match this SymbolCompletionProvider item with a recommended symbol
                return symbol;
            }
        }

        return null;
    }

    private static string GetProviderName(CompletionItem item)
    {
        return (string)providerNameAccessor.GetValue(item);
    }
}

// Simple DTO to return to the controller
public class CompletionResult
{
    public string Label { get; set; } = "";
    public int Kind { get; set; }
    public string InsertText { get; set; } = "";
}