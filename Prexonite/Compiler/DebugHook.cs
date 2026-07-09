

using JetBrains.Annotations;
using Prexonite.Commands.Core;
using Prexonite.Compiler.Ast;
using Prexonite.Modular;

namespace Prexonite.Compiler;

/// <summary>
///     Implementation of a compiler hook that optimizes the using of the <see cref = "Debug" /> command.
/// </summary>
[PublicAPI]
public static class DebugHook
{
    /// <summary>
    ///     To determine whether a function allows debugging or not, this meta key is checked.
    /// </summary>
    public const string DebuggingMetaKey = "debugging";

    /// <summary>
    ///     Implementation of the debug hook.
    /// </summary>
    /// <param name = "t">The compiler target that is to be transformed.</param>
    public static void Hook(CompilerTarget t)
    {
        var debugging = IsDebuggingEnabled(t.Function);

        _replaceDebug(t, t.Ast, debugging);
    }

    static readonly CompilerHook _hook = new(Hook);

    /// <summary>
    ///     Installs the hook in the supplied <see cref = "Loader" />.
    /// </summary>
    /// <param name = "ldr">The loader.</param>
    [PublicAPI]
    public static void InstallHook(Loader ldr)
    {
        if (ldr == null)
            throw new ArgumentNullException(nameof(ldr));
        ldr.CompilerHooks.Add(_hook);
    }

    /// <summary>
    ///     Uninstalls the hook in the supplied <see cref = "Loader" />.
    /// </summary>
    /// <param name = "ldr">The loader.</param>
    [PublicAPI]
    public static void UninstallHook(Loader ldr)
    {
        if (ldr == null)
            throw new ArgumentNullException(nameof(ldr));
        ldr.CompilerHooks.Remove(_hook);
    }

    /// <summary>
    ///     Determines whether a function allows debugging or not.
    /// </summary>
    /// <param name = "function">The function to be checked for debugging settings.</param>
    /// <returns>True if the function allows debugging, false otherwise.</returns>
    public static bool IsDebuggingEnabled(PFunction function)
    {
        if (function.Meta.TryGetValue(DebuggingMetaKey, out var value))
            return value.Switch;
        else
            return function.ParentApplication.Meta[DebuggingMetaKey].Switch;
    }

    static void _replaceDebug(CompilerTarget t, IList<AstNode> block, bool debugging)
    {
        for (var i = 0; i < block.Count; i++)
        {
            var stmt = block[i] as AstGetSet;
            //look for calls
            if (_isDebugCall(stmt))
            {
                System.Diagnostics.Debug.Assert(stmt != null);
                //Found a call to debug
                block.RemoveAt(i);
                if (debugging)
                {
                    for (var j = 0; j < stmt.Arguments.Count; j++)
                    {
                        AstReference? refNode;
                        if (stmt.Arguments[j] is AstIndirectCall arg && (refNode = arg.Subject as AstReference) != null)
                        {
                            var printlnCall = t.Factory.Call(stmt.Position,
                                EntityRef.Command.Create(Engine.PrintLineAlias));
                            var concatCall = t.Factory.Call(stmt.Position,
                                EntityRef.Command.Create(Engine.ConcatenateAlias));
                            var consts =
                                new AstConstant(
                                    stmt.File,
                                    stmt.Line,
                                    stmt.Column,
                                    string.Concat("DEBUG ", refNode.Entity, " = "));
                            concatCall.Arguments.Add(consts);
                            concatCall.Arguments.Add(arg);
                            printlnCall.Arguments.Add(concatCall);

                            block.Insert(i, printlnCall);
                            i += 1;
                        } //end if arg not null
                    } //end for arguments             
                } //end if debugging

                continue;
            } //end if debug call

            //look for conditions
            if (block[i] is AstCondition cond)
            {
                AstReference? refNode;
                if (cond.Condition is AstIndirectCall expr 
                    && (refNode = expr.Subject as AstReference) != null 
                    && refNode.Entity.TryGetCommand(out var cmd) 
                    && Engine.StringsAreEqual(cmd.Id,Engine.DebugAlias) )
                    cond.Condition =
                        new AstConstant(expr.File, expr.Line, expr.Column, debugging);
            }

            //Recursively replace 'debug' in nested blocks.
            if (block[i] is IAstHasBlocks complex)
                foreach (var subBlock in complex.Blocks)
                    _replaceDebug(t, subBlock, debugging);
        } //end for statements
    }

    [ContractAnnotation("=>true,stmt:notnull;=>false,stmt:canbenull")]
    static bool _isDebugCall(AstGetSet? stmt)
    {
        return stmt != null && stmt.TryMatchCall(out var entityRef) && entityRef.TryGetCommand(out var cmdRef) && Engine.StringsAreEqual(cmdRef.Id,Engine.DebugAlias);
    }
}