using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public sealed class AstMacroInvocation : AstGetSet
    {
        private readonly string _macroId;
        private SymbolCollection _releaseAfterEmit;

        public AstMacroInvocation(string file, int line, int column, string macroId) : base(file, line, column, PCall.Get)
        {
            _macroId = macroId;
        }

        internal AstMacroInvocation(Parser p, string macroId) : base(p, PCall.Get)
        {
            _macroId = macroId;
        }

        public string MacroId
        {
            [DebuggerStepThrough]
            get { return _macroId; }
        }

        protected override void EmitGetCode(CompilerTarget target, bool justEffect)
        {
            throw new NotImplementedException("Macro invocation requires a different mechanic. Use AstGetSet.EmitCode instead.");
        }

        protected override void EmitSetCode(CompilerTarget target)
        {
            throw new NotImplementedException("Macro invocation requires a different mechanic. Use AstGetSet.EmitCode instead.");
        }

        protected override void EmitCode(CompilerTarget target, bool justEffect)
        {
            //instantiate macro for the current target
            PFunction macroFunc;
            if (!target.Loader.Options.TargetApplication.Functions.TryGetValue(_macroId, out macroFunc))
                throw new PrexoniteException(
                    string.Format(
                        "The macro function {0} was called from function {1} but is not available at compile time.", _macroId,
                        target.Function.Id));

            if (_releaseAfterEmit != null)
                throw new PrexoniteException(
                    "AstMacroInvocation.EmitCode is not reentrant. Use GetCopy() to operate on a copy of this macro invocation.");

            try
            {
                _releaseAfterEmit = new SymbolCollection(5);

                var env = CompilerTarget.CreateEnvironment(target, this, justEffect);

                var sharedVariables = macroFunc.Meta[PFunction.SharedNamesKey].List.Select(entry => env[entry.Text]).ToArray();
                var macro = new Closure(macroFunc, sharedVariables);

                //Execute macro (argument nodes of the invocation node are passed as arguments to the macro)
                var arguments = Arguments.Select(target.Loader.CreateNativePValue).ToArray();
                var astRaw = macro.IndirectCall(target.Loader, arguments);

                //Optimize and then emit returned code.
                AstNode ast;
                if (astRaw == null || (ast = astRaw.Value as AstNode) == null)
                {
                    //If a value was expected, we need to at least make up null, otherwise
                    //  we risk stack corruption.
                    if (!justEffect)
                        target.Emit(OpCode.ldc_null);
                    return;
                }

                var expr = ast as IAstExpression;
                if (expr != null)
                {
                    OptimizeNode(target, ref expr);
                    ast = (AstNode) expr;
                }

                var effect = ast as IAstEffect;
                if (effect != null && justEffect)
                    effect.EmitEffectCode(target);
                else
                    ast.EmitCode(target);

                //In well-structured code (block based branching) it is now safe to release temporary variables.
                foreach (var temp in _releaseAfterEmit)
                    target.ReleaseTemporaryVariable(temp);
            }
            finally
            {
                _releaseAfterEmit = null;
            }
        }

        public void ReleaseAfterEmit(string temporaryVariable)
        {
            if (_releaseAfterEmit.Contains(temporaryVariable))
                throw new PrexoniteException("Cannot release temporary variable " + temporaryVariable + " twice!");
            _releaseAfterEmit.Add(temporaryVariable);
        }

        public override bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            //Do not optimize the macros arguments! They should be passed to the macro in their original form.
            //  the macro should decide whether or not to apply AST-optimization to the or not.
            expr = null;
            return false;
        }

        public override AstGetSet GetCopy()
        {
            var macro = new AstMacroInvocation(File, Line, Column, _macroId);
            CopyBaseMembers(macro);
            return macro;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}{2}", base.ToString(), MacroId, ArgumentsToString());
        }
    }
}