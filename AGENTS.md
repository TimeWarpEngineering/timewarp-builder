# TimeWarp.Builder - Agent Development Guidelines

## Build & Test Commands
```bash
dotnet build                    # Build all projects
dotnet build source/timewarp-builder/timewarp-builder.csproj  # Build main project
dotnet pack                     # Create NuGet packages
dotnet test                     # Run all tests (if any test projects exist)
```

## Code Style Guidelines
- **Explicit types only**: Never use `var` - warnings enforced
- **Naming**: PascalCase for types, methods, properties; camelCase for parameters/locals; Interfaces start with 'I'
- **Fields**: PascalCase (NO underscore prefixes)
- **File-scoped namespaces**: `namespace TimeWarp.Builder;`
- **Using statements**: Inside namespace, not at file level
- **Nullability**: Enabled everywhere - use nullable reference types
- **AOT compatible**: All code must be AOT-compatible
- **Target framework**: .NET 10.0

## Project Structure
- Main library: `source/timewarp-builder/`
- Global usings in `global-usings.cs` (keep minimal)
- Fluent builder interfaces: `IBuilder<TBuilt>` for standalone, `INestedBuilder<TParent>` for nested
- Scope extensions for Kotlin-inspired chaining methods

## Error Handling
- Use pattern matching (`is null` instead of `== null`)
- Prefer null propagation and coalescing expressions
- All warnings are treated as errors in build