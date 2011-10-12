using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Prexonite.Compiler.Macro;
using Prexonite.Types;
using NotSupportedException = Prexonite.Commands.Concurrency.NotSupportedException;

namespace Prexonite.Compiler.Ast
{
    public sealed class AstMacroInvocation : AstGetSet
    {
        private readonly string _macroId;
        private readonly SymbolInterpretations _interpretation;

        public AstMacroInvocation(string file, int line, int column, string macroId, SymbolInterpretations interpretation) : base(file, line, column, PCall.Get)
        {
            if(String.IsNullOrEmpty(macroId))
                throw new ArgumentException("MacroId cannot be null or empty.");
            _macroId = macroId;
            _interpretation = interpretation;
        }

        internal AstMacroInvocation(Parser p, string macroId, SymbolInterpretations interpretation) : base(p, PCall.Get)
        {
            _macroId = macroId;
            _interpretation = interpretation;
        }

        public SymbolInterpretations Interpretation
        {
            [DebuggerStepThrough]
            get { return _interpretation; }
        }

        public string MacroId
        {
            [DebuggerStepThrough]
            get { return _macroId; }
        }

        protected override void EmitGetCode(CompilerTarget target, bool justEffect)
        {
            throw new NotSupportedException("Macro invocation requires a different mechanic. Use AstGetSet.EmitCode instead.");
        }

        protected override void EmitSetCode(CompilerTarget target)
        {
            throw new NotSupportedException("Macro invocation requires a different mechanic. Use AstGetSet.EmitCode instead.");
        }

        protected override void EmitCode(CompilerTarget target, bool justEffect)
        {
            //instantiate macro for the current target
            MacroSession session = null;

            try
            {
                //Acquire current macro session
                session = target.AcquireMacroSession();

                //Expand macro
                var node = session.ExpandMacro(this, justEffect);

                //Emit generated code
                var effect = node as IAstEffect;
                if(justEffect)
                    effect.EmitEffectCode(target);
                else
                    node.EmitCode(target);
            }
            finally
            {
                if (session != null)
                    target.ReleaseMacroSession(session);
            }
        }

        public override bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            //Do not optimize the macros arguments! They should be passed to the macro in their original form.
            //  the macro should decide whether or not to apply AST-optimization to the arguments or not.
            expr = null;
            return false;
        }

        public override AstGetSet GetCopy()
        {
            var macro = new AstMacroInvocation(File, Line, Column, _macroId, _interpretation);
            CopyBaseMembers(macro);
            return macro;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}{2}", base.ToString(), MacroId, ArgumentsToString());
        }
    }
}