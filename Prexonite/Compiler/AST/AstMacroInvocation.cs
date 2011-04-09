using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Prexonite.Compiler.Macro;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public sealed class AstMacroInvocation : AstGetSet
    {
        private readonly string _macroId;

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
            MacroSession session = null;
            var isTopLevel = target.CurrentMacroSession == null;
            
            try
            {
                session = target.CurrentMacroSession = new MacroSession(target);
                session.ExpandMacro(this, justEffect);
            }
            finally
            {
                if (isTopLevel)
                {
                    if (session != null)
                        session.Dispose();
                    Debug.Assert(target.CurrentMacroSession == session);
                    target.CurrentMacroSession = null;
                }
            }
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