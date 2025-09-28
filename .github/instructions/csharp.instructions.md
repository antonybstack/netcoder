---
description: "Guidelines for building C# applications"
applyTo: "**/*.cs,**/*.csproj,**/*.sln"
---

# C# Development

## DO

- DO USE .NET 10 and C# 13 or later for building backend services.
- DO USE Native dependency injection (DI) MUST be used for managing dependencies.
- DO USE Asynchronous programming (async/await) MUST be used for I/O-bound operations.
- DO USE Cancellation tokens and pass them in all async methods to allow for graceful cancellation.
- DO USE LINQ for data manipulation and querying collections.
- DO USE task-based asynchronous patterns for concurrency.
- DO USE Exception handling with try/catch blocks MUST be implemented for robust error management.
- DO write tests that can be run in parallel.

## DO NOT

- DO NOT write comments
- DO NOT Use synchronous programming for I/O-bound operations.
- DO NOT Use blocking calls in async methods.
- DO NOT Use `Thread.Sleep` for delays; prefer `Task.Delay`.
- DO NOT Use `async void` except for event handlers.
