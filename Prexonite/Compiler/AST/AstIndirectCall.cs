// Prexonite
// 
// Copyright (c) 2014, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, 
//          this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, 
//          this list of conditions and the following disclaimer in the 
//          documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or 
//          promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
//  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
//  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING 
//  IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using System;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Prexonite.Commands.Core.PartialApplication;
using Prexonite.Modular;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public class AstIndirectCall : AstGetSetImplBase, IAstPartiallyApplicable
    {
        public AstExpr Subject;

        public override AstExpr[] Expressions
        {
            get
            {
                var len = Arguments.Count;
                var ary = new AstExpr[len + 1];
                Array.Copy(Arguments.ToArray(), 0, ary, 1, len);
                ary[0] = Subject;
                return ary;
            }
        }

        public AstIndirectCall(
            string file, int line, int column, PCall call, AstExpr subject)
            : base(file, line, column, call)
        {
            if (subject == null)
                throw new ArgumentNullException(nameof(subject));
            Subject = subject;
        }

        public AstIndirectCall(ISourcePosition position, PCall call, AstExpr subject)
            : base(position,call)
        {
            Subject = subject;
        }

        internal AstIndirectCall(Parser p, PCall call, AstExpr subject)
            : this(p.scanner.File, p.t.line, p.t.col, call, subject)
        {
        }

        public AstIndirectCall(string file, int line, int column, PCall call)
            : this(file, line, column, call, null)
        {
        }

        public AstIndirectCall(string file, int line, int column)
            : this(file, line, column, PCall.Get)
        {
        }

        public AstIndirectCall(string file, int line, int column, AstExpr subject)
            : this(file, line, column, PCall.Get, subject)
        {
        }

        internal AstIndirectCall(Parser p, AstExpr subject)
            : this(p, PCall.Get, subject)
        {
        }

        public override int DefaultAdditionalArguments
        {
            get
            {
                if (_getDirectCallAction() == null)
                    return base.DefaultAdditionalArguments + 1; //include subject
                else
                    return base.DefaultAdditionalArguments; // is translated as a direct call
            }
        }

        private class EntityIndirectCallMatcher : EntityRefMatcher<object,Action<CompilerTarget,AstIndirectCall,PCall,bool>>
        {
            public static readonly EntityIndirectCallMatcher Instance = new EntityIndirectCallMatcher();

            protected override Action<CompilerTarget, AstIndirectCall, PCall, bool> OnNotMatched(EntityRef entity, object argument)
            {
                return null;
            }

            protected override Action<CompilerTarget, AstIndirectCall, PCall, bool> OnLocalVariable(EntityRef.Variable.Local variable, object argument)
            {
                return
                    (target, node, call, justEffect) =>
                    target.Emit(node.Position,Instruction.CreateLocalIndirectCall(node.Arguments.Count, variable.Id, justEffect));
            }

            protected override Action<CompilerTarget, AstIndirectCall, PCall, bool> OnGlobalVariable(EntityRef.Variable.Global variable, object argument)
            {
                return
                    (target, node, call, justEffect) =>
                    target.Emit(node.Position, Instruction.CreateGlobalIndirectCall(node.Arguments.Count, variable.Id, variable.ModuleName, justEffect));
            }
        }

        private class EntityCallMatcher : EntityRefMatcher<object,Action<CompilerTarget,AstIndirectCall,PCall,bool>>
        {
            public static readonly EntityCallMatcher Instance = new EntityCallMatcher();

            protected override Action<CompilerTarget, AstIndirectCall, PCall, bool> OnNotMatched(EntityRef entity, object argument)
            {
                return null;
            }

            public override Action<CompilerTarget, AstIndirectCall, PCall, bool> OnFunction(EntityRef.Function function, object argument)
            {
                return
                    (target, node, _, justEffect) =>
                    target.EmitFunctionCall(node.Position, node.Arguments.Count, function.Id, function.ModuleName,
                                            justEffect);
            }

            protected override Action<CompilerTarget, AstIndirectCall, PCall, bool> OnCommand(EntityRef.Command command, object argument)
            {
                return
                    (target, node, _, justEffect) =>
                    target.EmitCommandCall(node.Position, node.Arguments.Count, command.Id, justEffect);
            }

            protected override Action<CompilerTarget, AstIndirectCall, PCall, bool> OnLocalVariable(EntityRef.Variable.Local variable, object argument)
            {
                return
                    (target, node, call, justEffect) =>
                        {
                            switch (call)
                            {
                                case PCall.Get:
                                    if(node.Arguments.Count > 0)
                                        target.EmitPop(node.Position,node.Arguments.Count);
                                    if(!justEffect)
                                        target.EmitLoadLocal(node.Position,variable.Id);
                                    break;
                                case PCall.Set:
                                    Debug.Assert(node.Arguments.Count > 0, "Store local missing RHS");
                                    target.EmitStoreLocal(node.Position,variable.Id);
                                    if(node.Arguments.Count > 1)
                                        target.EmitPop(node.Position,node.Arguments.Count - 1);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException(nameof(call));
                            }
                        };
            }

            protected override Action<CompilerTarget, AstIndirectCall, PCall, bool> OnGlobalVariable(EntityRef.Variable.Global variable, object argument)
            {
                return
                    (target, node, call, justEffect) =>
                    {
                        switch (call)
                        {
                            case PCall.Get:
                                if (node.Arguments.Count > 0)
                                    target.EmitPop(node.Position, node.Arguments.Count);
                                if (!justEffect)
                                    target.EmitLoadGlobal(node.Position, variable.Id,variable.ModuleName);
                                break;
                            case PCall.Set:
                                Debug.Assert(node.Arguments.Count > 0, "Store local missing RHS");
                                target.EmitStoreGlobal(node.Position, variable.Id,variable.ModuleName);
                                if (node.Arguments.Count > 1)
                                    target.EmitPop(node.Position, node.Arguments.Count - 1);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(call));
                        }
                    };
            }
        }

        [CanBeNull]
        private Action<CompilerTarget, AstIndirectCall, PCall, bool> _getDirectCallAction()
        {
            // This method will be called at least twice per node. Once to indicate whether a direct call is available
            //  and then a second time to actually use the direct call implementation.

            var refNode = Subject as AstReference;
            var indCall = Subject as AstIndirectCall;
            if (refNode != null)
            {
                // Could be variable, function, command
                //  -> generates func, cmd, ldloc, stloc, ldglob, stglob
                return refNode.Entity.Match(EntityCallMatcher.Instance,null);
            }
            else if (indCall != null && (refNode = indCall.Subject as AstReference) != null)
            {
                // Could be indirectly accessed local or global variable 
                //  -> generates indloc, indglob
                return refNode.Entity.Match(EntityIndirectCallMatcher.Instance, null);
            }
            else
            {
                return null;
            }
        }

        protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
        {
            if(_getDirectCallAction() == null)
                Subject.EmitValueCode(target);
            base.DoEmitCode(target, stackSemantics);
        }

        protected override void EmitGetCode(CompilerTarget target, StackSemantics stackSemantics)
        {
            var action = _getDirectCallAction();
            var justEffect = stackSemantics == StackSemantics.Effect;
            if (action == null)
                target.EmitIndirectCall(Position, Arguments.Count, justEffect);
            else
                action(target, this, PCall.Get, justEffect);
        }

        protected override void EmitSetCode(CompilerTarget target)
        {
            //Indirect set does not have a return value, therefore justEffect is true
            var action = _getDirectCallAction();
            if (action == null)
                target.EmitIndirectCall(Position, Arguments.Count, true);
            else
                action(target, this, PCall.Set, true);
        }

        public override bool TryOptimize(CompilerTarget target, out AstExpr expr)
        {
            base.TryOptimize(target, out expr);
            _OptimizeNode(target, ref Subject);

            expr = null;
            return false;
        }

        public override AstGetSet GetCopy()
        {
            var copy = new AstIndirectCall(File, Line, Column, Call, Subject);
            CopyBaseMembers(copy);
            return copy;
        }

        #region Implementation of IAstPartiallyApplicable

        void IAstPartiallyApplicable.DoEmitPartialApplicationCode(CompilerTarget target)
        {
            var argv =
                AstPartiallyApplicable.PreprocessPartialApplicationArguments(
                    Subject.Singleton().Append(Arguments));
            var argc = argv.Count;
            AstPlaceholder p;
            if (argc == 0)
            {
                //There are no mappings at all, use default constructor
                target.EmitConstant(Position, 0);
                target.EmitCommandCall(Position, 1, Engine.PartialCallAlias);
            }
            else if (argc == 1 && !argv[0].IsPlaceholder())
            {
                //We have just a call target, this is actually the identity function
                Subject.EmitValueCode(target);
            }
            else if (
                argc >= 2
                    && !argv[0].IsPlaceholder()
                        && argv.Skip(2).All(expr => !expr.IsPlaceholder())
                            && ((p = argv[1] as AstPlaceholder) == null || p.Index == 0))
                //Matches the patterns 
                //  subj.(c_1, c_2,...,c_n, ?0,?1,?2,...,?m) 
                //and 
                //  subj.(?0, c_1,c_2,...,c_n, ?1,?2,?3,...,?m)
            {
                //This partial application was reduced to just closed arguments in prefix position
                //  with an optional open argument in front. No mapping is necessary in this case. 

                //Check for optional open argument
                if (p != null)
                {
                    //There is an open argument in front. This is handled by FlippedFunctionalPartialCall
                    argv[0].EmitValueCode(target);
                    foreach (var arg in argv.Skip(2))
                        arg.EmitValueCode(target);
                    target.EmitCommandCall(Position, argc - 1, FlippedFunctionalPartialCallCommand.Alias);
                }
                else
                {
                    //There is no open argument in front. This is implemented by FunctionalPartialCall
                    foreach (var arg in argv)
                        arg.EmitValueCode(target);
                    target.EmitCommandCall(Position, argc, FunctionalPartialCallCommand.Alias);
                }
            }
            else
            {
                //Use full-blown partial application mechanism for indirect calls.
                var ctorArgc = this.EmitConstructorArguments(target, argv);
                target.EmitCommandCall(Position, ctorArgc, Engine.PartialCallAlias);
            }
        }

        public override bool CheckForPlaceholders()
        {
            return base.CheckForPlaceholders() || Subject is AstPlaceholder;
        }

        public override string ToString()
        {
            return string.Format("{0}: ({1}).{2}",
                (Enum.GetName(typeof (PCall), Call) ?? "-").ToLowerInvariant(),
                Subject, ArgumentsToString());
        }

        #endregion

        public static AstGetSet Create(ISourcePosition position, AstExpr astExpr, PCall call = PCall.Get)
        {
            return new AstIndirectCall(position,call,astExpr);
        }
    }
}