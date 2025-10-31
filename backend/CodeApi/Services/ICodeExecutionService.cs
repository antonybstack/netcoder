using System.Threading;
using System.Threading.Tasks;
using CodeApi.Models;

namespace CodeApi.Services;

public interface ICodeExecutionService
{
    Task<ExecutionResult> ExecuteAsync(string code, CancellationToken ct);
}
