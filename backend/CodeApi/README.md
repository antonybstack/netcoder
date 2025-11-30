## References

### .net interactive

- https://github.com/dotnet/interactive
- Main kernel: https://github.com/dotnet/interactive/blob/main/src/Microsoft.DotNet.Interactive/Kernel.cs
- Main language
  service: https://github.com/dotnet/interactive/blob/main/src/Microsoft.DotNet.Interactive.CSharpProject/ILanguageService.cs
- ```c#
  public interface ILanguageService
  {
      Task<CompletionResult> GetCompletionsAsync(WorkspaceRequest request);
      Task<SignatureHelpResult> GetSignatureHelpAsync(WorkspaceRequest request);
      Task<DiagnosticResult> GetDiagnosticsAsync(WorkspaceRequest request);
  }
  ```

### Replay - A roslyn-powered editable REPL for C#.

- https://github.com/waf/replay-csharp/blob/master/Replay.Services/WorkspaceManager.cs

### RoslynEditor - A cross-platform C# editor based on Roslyn and AvalonEdit

- https://github.com/nickorzha/roslynedit/blob/main/src/RoslynPad.Editor.Shared/RoslynCodeEditorCompletionProvider.cs

### JitExplorer

- https://github.com/bitfaster/JitExplorer/blob/main/JitExplorer/Completion/RoslynCodeCompletion.cs

## Future Ideas

### Decompiler

- https://github.com/dotnet/interactive/blob/main/src/Microsoft.DotNet.Interactive.ExtensionLab/Inspector/JitAsmDecompiler/JitAsmDecompiler.cs
- 