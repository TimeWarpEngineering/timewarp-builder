# TimeWarp.Builder

[![NuGet](https://img.shields.io/nuget/vpre/TimeWarp.Builder.svg)](https://www.nuget.org/packages/TimeWarp.Builder)
[![NuGet Downloads](https://img.shields.io/nuget/dt/TimeWarp.Builder.svg)](https://www.nuget.org/packages/TimeWarp.Builder)
[![CI/CD](https://github.com/TimeWarpEngineering/timewarp-builder/actions/workflows/workflow.yml/badge.svg)](https://github.com/TimeWarpEngineering/timewarp-builder/actions/workflows/workflow.yml)
[![License: Unlicense](https://img.shields.io/badge/license-Unlicense-blue.svg)](LICENSE)

Fluent builder interfaces and Kotlin-inspired scope extensions for .NET.

## Installation

```bash
dotnet add package TimeWarp.Builder --prerelease
```

## Requirements

- .NET 10.0 or later
- Fully AOT- and trim-compatible (no reflection, no dynamic code)

## Interfaces

### IBuilder\<T\>

Interface for standalone builders that create objects via `Build()`. `TBuilt` is covariant, so an `IBuilder<Derived>` can be used wherever an `IBuilder<Base>` is expected.

```csharp
public class MyWidgetBuilder : IBuilder<Widget>
{
    public Widget Build() => new Widget(_options);
}

// Usage
Widget widget = new MyWidgetBuilder()
    .WithColor("blue")
    .WithSize(10)
    .Build();
```

### INestedBuilder\<TParent\>

Interface for nested builders that return to a parent context via `Done()`. `Done()` performs three things: builds the child, hands the result to the parent, and returns the parent for continued chaining.

```csharp
// Nested builder returns to parent after building
app.Map(route => route
    .WithLiteral("deploy")
    .WithParameter("env")
    .Done())  // Returns to parent builder
    .WithHandler(handler);
```

## Scope Extensions

Kotlin-inspired extension methods for fluent object manipulation. Because they attach to every type (unconstrained `T`), any object can participate in a fluent chain without its type opting in.

| Method | Returns | Use for |
|--------|---------|---------|
| `Also` | The original object | Side effects mid-chain (logging, diagnostics) |
| `Apply` | The original object | Configuring the object mid-chain |
| `Let` | The transform result | Converting to a different type/value |
| `Run` | Nothing (`void`) | Terminal action at the end of a chain |

### Also vs Apply

`Also` and `Apply` are mechanically identical — both execute an action and return the original object. They exist separately to signal *intent* at the call site, mirroring Kotlin's `also`/`apply` distinction: use `Apply` when the action configures the object itself, and `Also` when the action is an incidental side effect like logging.

```csharp
app.Map("status", handler)
   .Apply(r => r.AsQuery())                        // configures the route
   .Also(r => logger.LogDebug("mapped {r}", r));   // side effect, not configuration
```

### Also

Executes an action on the object and returns the original object.

```csharp
var builder = new AppBuilder()
    .Also(b => logger.LogDebug("Building app..."))
    .Configure(options);
```

### Apply

Configures the object and returns the original object.

```csharp
app.Map("status", handler)
   .Apply(r => r.AsQuery());
```

### Let

Transforms the object to a different type.

```csharp
int length = "hello".Let(s => s.Length);  // 5
```

### Run

Executes an action on the object with no return value. Terminal operation in a method chain.

```csharp
app.Build().Run(a => a.RunAsync(args));
```

All four methods throw `ArgumentNullException` when the delegate is null.

## Used By

- [TimeWarp.Nuru](https://github.com/TimeWarpEngineering/timewarp-nuru) — route, endpoint, group, and key-binding builders implement `IBuilder<T>` / `INestedBuilder<TParent>`
- [TimeWarp.Terminal](https://github.com/TimeWarpEngineering/timewarp-terminal)

## Testing

Tests are [TimeWarp.Jaribu](https://github.com/TimeWarpEngineering/timewarp-jaribu) runfiles under `tests/`. Run them all with `dev test`, or any file directly:

```bash
dotnet run tests/scope-extensions.also.cs
```

## Unlicense

This is free and unencumbered software released into the public domain — see [LICENSE](LICENSE).
