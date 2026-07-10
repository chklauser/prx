# Project Overview

Prexonite Script is a dynamically-typed, embeddable .NET scripting language focused on meta-programming and DSL construction.
It features compile-time macros, optional JIT compilation to CIL, and rich interop with .NET objects.

# Build & Test Commands

```bash
# Build entire solution
dotnet build

# Run all tests (NUnit + .pxs integration tests)
dotnet test

# Run CLI interpreter
dotnet run --project Prx

# Run specific test
dotnet test --filter "FullyQualifiedName~TestName"
```

# Solution Structure

- **Prexonite**: Core language library (VM, compiler, type system, standard library)
- **PrexoniteTests**: NUnit test suite with .pxs integration tests
- **Prx**: Command-line REPL and script runner
- **PxCoco**: Modified Coco/R parser generator (build-time tool)

# High-Level Architecture

## Compilation Pipeline

1. **Lexical Analysis** (`Compiler/Lexer.cs`) - Generated from `Prexonite.lex` via CSFlex
2. **Parsing** (`Compiler/Parser.cs`) - Generated from merged `.atg` grammar fragments via PxCoco
3. **AST Construction** (`Compiler/AST/`) - 50+ node types representing language constructs
4. **Macro Expansion** (`Compiler/Macro/`) - Compile-time AST transformation (macros run during compilation)
5. **Symbol Resolution** (`Compiler/Symbolic/`) - Namespace imports and qualified name resolution
6. **Bytecode Generation** (`Compiler/CompilerTarget.cs`) - AST nodes emit stack-based instructions
7. **Optional CIL Compilation** (`Compiler/Cil/`) - JIT compilation to native .NET methods via Reflection.Emit

## Execution Model

**Stack-based Virtual Machine** with context switching:

- **Engine** (`Engine.cs`): Global component managing type registry, commands, and call stack
- **StackContext** hierarchy: Abstract execution context (stack frames) with implementations:
  - `FunctionContext`: Interpreted bytecode execution
  - `CilFunctionContext`: JIT-compiled native execution
  - `CoroutineContext`, `CooperativeContext`: Concurrency support
- **Instruction Set** (`Instruction.cs`): Stack-based opcodes (load, store, call, jump, operators)
- **PValue** (`PValue.cs`): Universal value container `(object Value, PType Type)` with dynamic dispatch

## Type System

All operations dispatch through **PType** hierarchy:
- Primitive types: `IntPType`, `RealPType`, `BoolPType`, `StringPType`, etc.
- Collection types: `ListPType`, `HashPType`, `StructurePType`
- CLR interop: `ObjectPType` wraps .NET objects
- All operators use runtime dispatch (no static optimization)

## Module System

- **Module** (`Modular/Module.cs`): Container for functions, variables, metadata
- **Application** (`Application.cs`): Runtime instance of a module with initialization
- **Build System** (`Compiler/Build/`):
  - `SelfAssemblingPlan`: Automatic dependency resolution
  - Standard library auto-included: `prx.prim`, `prx.core`, `sys`, `prx.v2.prelude`

## Commands (Built-in Functions)

Implemented as .NET types in `Commands/`:
- **Core**: Basic operations, type conversions (`Commands/Core/`)
- **List**: Functional programming (`map`, `filter`, `foldl`, `where`, etc.)
- **Concurrency**: Channels, async operations (`Commands/Concurrency/`)
- **Math/Text**: Domain-specific utilities

Interface: `PValue Run(StackContext sctx, PValue[] args)`

# Important Conventions

## Naming
- PTypes: `*PType` suffix (e.g., `IntPType`)
- AST nodes: `Ast*` prefix (e.g., `AstBlock`)
- Internal helpers: Leading underscore (e.g., `_emitCode()`)
- Meta keys: Backslash prefix for system keys (e.g., `\init`, `\sharedNames`)

## String Comparison
**Case-insensitive throughout** - use `Engine.StringsAreEqual()` or `StringComparer.OrdinalIgnoreCase`

## Code Organization
- Namespaces mirror directory structure
- Compiler components: `Prexonite.Compiler.*`
- Runtime components: directly under `Prexonite`

# Standard Library Structure

Embedded scripts in `prxlib/`:
- **prx.prim**: Primitive operations (direct command exports)
- **prx.core**: Core namespace with submodules:
  - `prx.core.seq`: Sequence operations
  - `prx.core.nonstrict`: Lazy evaluation
  - `prx.core.text`, `prx.core.math`: Domain utilities
- **prx.v2.prelude**: Default prelude (auto-imported in v2 mode)

# Testing Infrastructure

- **C# Unit Tests**: `PrexoniteTests/Tests/` - NUnit tests for compiler, VM, types
- **Integration Tests**: `PrexoniteTests/psr-tests/*.test.pxs` - Prexonite script test suites
- **Test Runner**: `run_tests.pxs` orchestrates .pxs test execution

# Performance Considerations

1. **JIT Compilation**: Optional CIL compilation via `Compiler/Cil/` for hot code paths
2. **Overload Resolution**: Happens at every call site (by design, performance impact)
3. **Caching**: Function/command reference caching available (opt-in)
4. **Ready-to-Run**: CLI uses R2R compilation in release mode

# Build-Time Code Generation

- **Grammar Assembly**: `.atg` fragments merged via MSBuild into `Prexonite__gen.atg`
- **Parser Generation**: PxCoco generates `Parser.cs` from grammar
- **Lexer Generation**: CSFlex generates `Lexer.cs` from `Prexonite.lex`
- **Test fixture generation**: `PrexoniteTests.Generators` generates NUnit fixtures from the JSON test configurations

# Meta-Programming Features

## Macros
- Execute at compile-time with full AST access via `MacroContext`
- Can generate arbitrary AST nodes or crash the compiler (unsafe by design)
- Built-in macros in `Compiler/Macro/Commands/`: `call\macro`, `pack`, `unpack`, partial application

## Compile-Time Evaluation
- Interpreter runs during compilation for constant folding and code generation
- See `Commands/CompileTimeValue.cs`

# Quick Orientation Checklist

1. **VM execution entry point**: `Engine.Process()` in `VM.cs`
2. **Instruction set**: `OpCode` enum in `Instruction.cs`
3. **AST structure**: `Compiler/AST/AstNode.cs` base class
4. **Type system core**: `PValue.cs` and `Types/PType.cs`
5. **Compilation flow**: `Loader.cs` → `CompilerTarget.cs` → `AstNode.DoEmitCode()`

# Common Development Tasks

## Add Built-in Function
1. Create `PCommand` implementation in `Commands/`
2. Register in `Engine.cs` constructor or via meta-programming

## Add AST Node
1. Inherit from `AstNode` in `Compiler/AST/`
2. Implement `DoEmitCode(CompilerTarget target)` to emit bytecode
3. Extend `Prexonite/Compiler/AST/AstFactoryBase.cs`
4. Update parser if adding new syntax

## Add OpCode
1. Add to `OpCode` enum in `Instruction.cs`
2. Implement execution in `FunctionContext.Run()` switch statement
3. Add CIL generation in `Compiler/Cil/` if needed

## Modify Grammar
1. Edit `.atg` fragments in `Compiler/Grammar/`
2. Rebuild to regenerate parser via PxCoco
