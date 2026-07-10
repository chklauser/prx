using System.Collections.Concurrent;
using JetBrains.Annotations;
using Prexonite.Compiler;
using Prexonite.Compiler.Ast;
using Prexonite.Compiler.Cil;
using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;

namespace Prexonite.Commands.Core;

public class GetUnscopedAstFactory : PCommand, ICilCompilerAware
{
    class UnscopedFactory : AstFactoryBase
    {
        public UnscopedFactory(ModuleName compartment)
        {
            CurrentBlock = AstBlock.CreateRootBlock(
                NoSourcePosition.Instance,
                SymbolStore.Create(),
                compartment.ToString(),
                Guid.NewGuid().ToString("N")
            );
        }

        protected override AstBlock CurrentBlock { get; }

        protected override AstGetSet CreateNullNode(ISourcePosition position)
        {
            return IndirectCall(position, Null(position));
        }

        protected override bool IsOuterVariable(string id)
        {
            return false;
        }

        protected override void RequireOuterVariable(string id) { }

        public override void ReportMessage(Message message)
        {
            // Yes, this also fails for info and warning messages, but we really have no other choice.
            // Unscoped factories should not be used except when you know exactly what you are doing.
            throw new ErrorMessageException(message);
        }

        protected override CompilerTarget CompileTimeExecutionContext =>
            throw new InvalidOperationException(
                "Unscoped AST factory does not have access to a compiler instance."
            );
    }

    #region Singleton

    public static GetUnscopedAstFactory Instance { get; } = new();

    GetUnscopedAstFactory() { }

    public const string Alias = "get_unscoped_ast_factory";

    #endregion

    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        return RunStatically(sctx, args.ToArray());
    }

    static readonly ConcurrentDictionary<ModuleName, UnscopedFactory> _unscopedFactories = new();

    [PublicAPI]
    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        return sctx.CreateNativePValue(
            _unscopedFactories.GetOrAdd(sctx.ParentApplication.Module.Name, n => new(n))
        );
    }

    public CompilationFlags CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersRunStatically;
    }

    public void ImplementInCil(CompilerState state, Instruction ins)
    {
        throw new NotSupportedException(Alias + " does not provide a custom CIL implementation.");
    }
}
