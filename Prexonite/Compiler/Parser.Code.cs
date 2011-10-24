// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Prexonite.Commands.Core.Operators;
using Prexonite.Compiler.Ast;
using Prexonite.Types;

// ReSharper disable InconsistentNaming

namespace Prexonite.Compiler
{
    internal partial class Parser
    {
        #region Proxy interface

        private readonly Loader _loader;

        public Loader Loader
        {
            [DebuggerStepThrough]
            get { return _loader; }
        }

        public Application TargetApplication
        {
            [DebuggerStepThrough]
            get { return _loader.Options.TargetApplication; }
        }

        public LoaderOptions Options
        {
            [DebuggerStepThrough]
            get { return _loader.Options; }
        }

        public Engine ParentEngine
        {
            [DebuggerStepThrough]
            get { return _loader.Options.ParentEngine; }
        }

        public SymbolTable<SymbolEntry> Symbols
        {
            [DebuggerStepThrough]
            get { return _loader.Symbols; }
        }

        public Loader.FunctionTargetsIterator FunctionTargets
        {
            [DebuggerStepThrough]
            get { return _loader.FunctionTargets; }
        }

        private readonly AstProxy _astProxy;

        public AstProxy Ast
        {
            [DebuggerStepThrough]
            get { return _astProxy; }
        }

        [DebuggerStepThrough]
        public class AstProxy
        {
            private readonly Parser outer;

            internal AstProxy(Parser outer)
            {
                this.outer = outer;
            }

            public AstBlock this[PFunction func]
            {
                get { return outer._loader.FunctionTargets[func].Ast; }
            }

            public AstBlock this[string func]
            {
                get { return outer._loader.FunctionTargets[func].Ast; }
            }
        }

        [DebuggerStepThrough]
        internal Parser(IScanner scanner, Loader loader)
            : this(scanner)
        {
            if (loader == null)
                throw new ArgumentNullException("loader");
            _loader = loader;
            _createTableOfInstructions();
            _astProxy = new AstProxy(this);
        }

        #endregion

        #region String cache

        [DebuggerStepThrough]
        internal string cache(string toCache)

        {
            return _loader.CacheString(toCache);
        }

        #endregion

        #region Helper

        #region General

        private static string _removeSingleQuotes(string s)
        {
            return s.Replace("'", "");
        }

        public static bool TryParseInteger(string s, out int i)
        {
            return Int32.TryParse(_removeSingleQuotes(s), IntegerStyle, CultureInfo.InvariantCulture,
                out i);
        }

        public static bool TryParseReal(string s, out double d)
        {
            return Double.TryParse(_removeSingleQuotes(s), RealStyle, CultureInfo.InvariantCulture,
                out d);
        }

        public static NumberStyles RealStyle
        {
            get { return NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent; }
        }

        public static NumberStyles IntegerStyle
        {
            get { return NumberStyles.None; }
        }

        [DebuggerStepThrough]
        public void SemErr(int line, int col, string message)
        {
            errors.SemErr(line, col, message);
        }

        [DebuggerStepThrough]
        public void SemErr(Token tok, string s)
        {
            errors.SemErr(tok.line, tok.col, s);
        }

        [DebuggerStepThrough]
        public static bool InterpretationIsVariable(SymbolInterpretations interpretation)
        {
            return
                InterpretationIsGlobalVariable(interpretation) ||
                    InterpretationIsLocalVariable(interpretation);
        }

        [DebuggerStepThrough]
        public static bool InterpretationIsLocalVariable(SymbolInterpretations interpretation)
        {
            return
                interpretation == SymbolInterpretations.LocalReferenceVariable ||
                    interpretation == SymbolInterpretations.LocalObjectVariable;
        }

        [DebuggerStepThrough]
        public static bool InterpretationIsGlobalVariable(SymbolInterpretations interpretation)
        {
            return
                interpretation == SymbolInterpretations.GlobalReferenceVariable ||
                    interpretation == SymbolInterpretations.GlobalObjectVariable;
        }

        [DebuggerStepThrough]
        public static bool InterpretationIsObjectVariable(SymbolInterpretations interpretation)
        {
            return
                interpretation == SymbolInterpretations.LocalObjectVariable ||
                    interpretation == SymbolInterpretations.GlobalObjectVariable;
        }

        [DebuggerStepThrough]
        public static SymbolInterpretations InterpretAsObjectVariable(
            SymbolInterpretations interpretation)
        {
            if (InterpretationIsLocalVariable(interpretation))
                return SymbolInterpretations.LocalObjectVariable;
            else if (InterpretationIsGlobalVariable(interpretation))
                return SymbolInterpretations.GlobalObjectVariable;
            else
                return SymbolInterpretations.Undefined;
        }

        private void _pushLexerState(int state)
        {
            var lex = scanner as Lexer;
            if (lex == null)
                throw new PrexoniteException("The prexonite grammar requires a *Lex-scanner.");
            lex.PushState(state);
        }

        private void _popLexerState()
        {
            var lex = scanner as Lexer;
            if (lex == null)
                throw new PrexoniteException("The prexonite grammar requires a *Lex-scanner.");
            lex.PopState();
            //Might be id or keyword
            if ((la.kind > _BEGINKEYWORDS && la.kind < _ENDKEYWORDS) || la.kind == _id)
                la.kind = lex.checkKeyword(la.val);
        }

        private void _inject(Token c)
        {
            var lex = scanner as Lexer;
            if (lex == null)
                throw new PrexoniteException("The prexonite grammar requires a *Lex-scanner.");

            if (c == null)
                throw new ArgumentNullException("c");

            lex._InjectToken(c);
        }

        private void _inject(int kind, string val)
        {
            if (val == null)
                throw new ArgumentNullException("val");
            var c = new Token
                {
                    kind = kind,
                    val = val
                };
            _inject(c);
        }

        private void _inject(int kind)
        {
            _inject(kind, System.String.Empty);
        }

        #endregion //General

        #region Prexonite Script

        public CompilerTarget target;

        [DebuggerStepThrough]
        public bool isLabel() //LL(2)
        {
            scanner.ResetPeek();
            var c = la;
            var cla = scanner.Peek();

            return c.kind == _lid || (isId(c) && cla.kind == _colon);
        }

        [DebuggerStepThrough]
        public bool isVariableDeclaration() //LL(4)
        {
            /* Applies to:
             *  var id;
             *  new var id;
             *  static id;
             *  new static id;
             *  ref id;
             *  new ref id;
             *  static ref id;
             *  new static ref id;
             */

            scanner.ResetPeek();
            //current might optionally be `new`
            var c = la;
            if (c.kind == _new)
                c = scanner.Peek();

            //current la = static | var | ref
            if (c.kind != _var && c.kind != _ref && c.kind != _static)
                return false;
            c = scanner.Peek();

            //must not terminate here
            if (c.kind == _semicolon || c.kind == _comma) //id expected
                return false;

            var interpretation = c;

            c = scanner.Peek();
            if (c.kind == _semicolon || c.kind == _comma) //no interpretation
                return true;

            c = scanner.Peek();
            return interpretation.kind == _ref && (c.kind == _semicolon || c.kind == _comma);
        }

        public bool isAssignmentOperator() //LL2
        {
            /* Applies to:
             *  =
             *  +=
             *  -=
             *  *=
             *  /=
             *  ^=
             *  |=
             *  &=
             *  ??=
             *  ~=
             */
            scanner.ResetPeek();

            //current la = assign | plus | minus | times | div | pow | bitOr | bitAnd | coalesence | tilde

            switch (la.kind)
            {
                case _assign:
                    return true;
                case _plus:
                case _minus:
                case _times:
                case _div:
                case _pow:
                case _bitOr:
                case _bitAnd:
                case _coalescence:
                case _tilde:
                    var c = scanner.Peek();
                    if (c.kind == _assign)
                        return true;
                    else
                        return false;
                default:
                    return false;
            }
        }

        [DebuggerStepThrough]
        public bool isDeDereference() //LL(2)
        {
            scanner.ResetPeek();
            var c = la;
            var cla = scanner.Peek();

            return c.kind == _pointer && cla.kind == _pointer;
        }

        //id is object or reference variable
        [DebuggerStepThrough]
        public bool isLikeVariable(string id) //context
        {
            if (!target.Symbols.ContainsKey(id))
                return false;
            var kind = target.Symbols[id].Interpretation;
            return
                kind == SymbolInterpretations.LocalObjectVariable ||
                    kind == SymbolInterpretations.GlobalObjectVariable ||
                        kind == SymbolInterpretations.LocalReferenceVariable ||
                            kind == SymbolInterpretations.GlobalReferenceVariable;
        }

        public bool isLocalVariable(SymbolInterpretations interpretations)
        {
            return interpretations == SymbolInterpretations.LocalObjectVariable
                || interpretations == SymbolInterpretations.LocalReferenceVariable;
        }

        //id is like a function
        [DebuggerStepThrough]
        public bool isLikeFunction() //Context
        {
            var id = la.val;
            return
                la.kind == _id &&
                    target.Symbols.ContainsKey(id) &&
                        isLikeFunction(target.Symbols[id].Interpretation);
        }

        //context

        //interpretation is like function
        [DebuggerStepThrough]
        private static bool isLikeFunction(SymbolInterpretations interpretation) //Context
        {
            return
                interpretation == SymbolInterpretations.Function ||
                    interpretation == SymbolInterpretations.Command ||
                        interpretation == SymbolInterpretations.LocalReferenceVariable ||
                            interpretation == SymbolInterpretations.GlobalReferenceVariable ||
                                interpretation == SymbolInterpretations.MacroCommand;
        }

        //context

        [DebuggerStepThrough]
        public bool isUnknownId()
        {
            var id = la.val;
            return la.kind == _id && !target.Symbols.ContainsKey(id);
        }

        private bool _tryResolveFunction(SymbolEntry entry, out PFunction func)
        {
            func = null;
            if(entry.Interpretation != SymbolInterpretations.Function)
                return false;

            if(entry.Module == null)
            {
                return TargetApplication.Functions.TryGetValue(entry.InternalId, out func);
            }
            else
            {
                throw new NotImplementedException("Module lookup not implemented.");
            }
        }

        [DebuggerStepThrough]
        public bool isKnownMacroFunction(SymbolEntry symbol)
        {
            PFunction func;
            if (!_tryResolveFunction(symbol, out func))
                return false;

            return func.Meta[CompilerTarget.MacroMetaKey].Switch;
        }

        [DebuggerStepThrough]
        public string getTypeName(string typeId, bool staticPrefix)
        {
            if (staticPrefix) //already marked as CLR call
                return ObjectPType.Literal + "(\"" + StringPType.Escape(typeId) + "\")";
            else if (target.Function.ImportedNamespaces.Any(
                importedNamespace =>
                    typeId.StartsWith(importedNamespace, StringComparison.OrdinalIgnoreCase)))
                return ObjectPType.Literal + "(\"" + StringPType.Escape(typeId) + "\")";
            return typeId;
        }

        private bool isFollowedByStatementBlock()
        {
            scanner.ResetPeek();
            return scanner.Peek().kind == _lbrace;
        }

        //[DebuggerStepThrough]
        private bool isLambdaExpression() //LL(*)
        {
            scanner.ResetPeek();

            var current = la;
            if (!(current.kind == _lpar || isId(current)))
                return false;
            var next = scanner.Peek();

            var requirePar = false;
            if (current.kind == _lpar)
            {
                requirePar = true;
                current = next;
                next = scanner.Peek();

                //Check for lambda expression without arguments
                if (current.kind == _rpar && next.kind == _implementation)
                    return true;
            }

            if (isId(current))
            {
                //break if lookahead is not valid to save tokens
                if (
                    !(next.kind == _comma || next.kind == _implementation ||
                        (next.kind == _rpar && requirePar)))
                    return false;
                //Consume 1
                current = next;
                next = scanner.Peek();
            }
            else if ((current.kind == _var || current.kind == _ref) && isId(next))
            {
                //Consume 2
                current = scanner.Peek();
                //break if lookahead is not valid to save tokens
                if (
                    !(current.kind == _comma || current.kind == _implementation ||
                        (current.kind == _rpar && requirePar)))
                    return false;
                next = scanner.Peek();
            }
            else
            {
                return false;
            }

            while (current.kind == _comma && requirePar)
            {
                //Consume comma
                current = next;
                next = scanner.Peek();

                if (isId(current))
                {
                    //Consume 1
                    current = next;
                    next = scanner.Peek();
                }
                else if ((current.kind == _var || current.kind == _ref) && isId(next))
                {
                    //Consume 2
                    current = scanner.Peek();
                    next = scanner.Peek();
                }
                else
                {
                    return false;
                }
            }

            if (requirePar)
                if (current.kind == _rpar)
                {
                    current = next;
                    //cla = scanner.Peek();
                }
                else
                {
                    return false;
                }

            return current.kind == _implementation;
        }

        //LL(*)

        [DebuggerStepThrough]
        private bool isIndirectCall() //LL(2)
        {
            scanner.ResetPeek();
            var c = la;
            var cla = scanner.Peek();

            return c.kind == _dot && cla.kind == _lpar;
        }

        [DebuggerStepThrough]
        private bool isOuterVariable(string id) //context
        {
            //Check local function
            var func = target.Function;
            if (func.Variables.Contains(id) || func.Parameters.Contains(id))
                return false;

            //Check parents
            for (var parent = target.ParentTarget;
                 parent != null;
                 parent = parent.ParentTarget)
            {
                func = parent.Function;
                if (func.Variables.Contains(id) || func.Parameters.Contains(id) ||
                    parent.OuterVariables.Contains(id))
                    return true;
            }
            return false;
        }

        private string generateLocalId(string prefix = "")
        {
            return target.GenerateLocalId(prefix);
        }

        private void SmartDeclareLocal(string id, SymbolInterpretations kind)
        {
            SmartDeclareLocal(id, id, kind, false);
        }

        private void SmartDeclareLocal(string id, SymbolInterpretations kind, bool isOverrideDecl)
        {
            SmartDeclareLocal(id, id, kind, isOverrideDecl);
        }

        private void SmartDeclareLocal(string logicalId, string physicalId,
            SymbolInterpretations kind, bool isOverrideDecl)
        {
            if (!isOverrideDecl && !target.Function.Variables.Contains(physicalId) &&
                isOuterVariable(physicalId))
            {
                target.RequireOuterVariable(physicalId);
                target.DeclareModuleLocal(kind, logicalId, physicalId);
            }
            else
            {
                target.DefineModuleLocal(kind, logicalId, physicalId);
            }
        }

        [DebuggerStepThrough]
        private static bool isId(Token c)
        {
            if (isGlobalId(c))
                return true;
            switch (c.kind)
            {
                case _enabled:
                case _disabled:
                case _build:
                case _add:
                    return true;
                default:
                    return false;
            }
        }

        [DebuggerStepThrough]
        private static bool isGlobalId(Token c)
        {
            return c.kind == _id || c.kind == _anyId;
        }

        private bool _isNotNewDecl()
        {
            if (la.kind != _new)
                return false;

            scanner.ResetPeek();
            var varTok = scanner.Peek();

            return varTok.kind != _var && varTok.kind != _ref;
        }

        #endregion

        private static IEnumerable<string> let_bindings(CompilerTarget ft)
        {
            var lets = new HashSet<string>(Engine.DefaultStringComparer);
            for (var ct = ft; ct != null; ct = ct.ParentTarget)
                lets.UnionWith(ct.Function.Meta[PFunction.LetKey].List.Select(e => e.Text));
            return lets;
        }

        private static void mark_as_let(PFunction f, string local)
        {
            f.Meta[PFunction.LetKey] = (MetaEntry)
                f.Meta[PFunction.LetKey].List
                    .Union(new[] {(MetaEntry) local})
                    .ToArray();
        }

        private void _compileAndExecuteBuildBlock(CompilerTarget buildBlockTarget)
        {
            if (errors.count > 0)
            {
                SemErr("Cannot execute build block. Errors detected");
                return;
            }

            //Emit code for top-level build block
            try
            {
                buildBlockTarget.Ast.EmitCode(buildBlockTarget, true);

                buildBlockTarget.Function.Meta["File"] = scanner.File;
                buildBlockTarget.FinishTarget();
                //Run the build block 
                var fctx = buildBlockTarget.Function.CreateFunctionContext(ParentEngine,
                    new PValue[] {},
                    new PVariable[] {}, true);
                object token = null;
                try
                {
                    TargetApplication._SuppressInitialization = true;
                    token = Loader.RequestBuildCommands();
                    ParentEngine.Process(fctx);
                }
                finally
                {
                    if (token != null)
                        Loader.ReleaseBuildCommands(token);
                    TargetApplication._SuppressInitialization = false;
                }
            }
            catch (Exception e)
            {
                SemErr("Exception during compilation and execution of build block.\n" + e);
            }
        }

        private bool _suppressPrimarySymbol(IHasMetaTable ihmt)
        {
            return ihmt.Meta[Loader.SuppressPrimarySymbol].Switch;
        }

        #region Assembler

        [DebuggerStepThrough]
        public void addInstruction(AstBlock block, Instruction ins)
        {
            block.Add(new AstAsmInstruction(this, ins));
        }

        public void addOpAlias(AstBlock block, string insBase, string detail)
        {
            int argc;
            var alias = getOpAlias(insBase, detail, out argc);
            if (alias == null)
            {
                SemErr(string.Format("Unknown operator alias in assembler code: {0}.{1}", insBase,
                    detail));
                block.Add(new AstAsmInstruction(this, new Instruction(OpCode.nop)));
                return;
            }

            block.Add(new AstAsmInstruction(this, Instruction.CreateCommandCall(argc, alias)));
        }

        [DebuggerStepThrough]
        public void addLabel(AstBlock block, string label)
        {
            block.Statements.Add(new AstExplicitLabel(this, label));
        }

        [DebuggerStepThrough]
        private bool isAsmInstruction(string insBase, string detail) //LL(4)
        {
            scanner.ResetPeek();
            var la1 = la.kind == _at ? scanner.Peek() : la;
            var la2 = scanner.Peek();
            var la3 = scanner.Peek();
            return checkAsmInstruction(la1, la2, la3, insBase, detail);
        }

        private static bool checkAsmInstruction(
            Token la1, Token la2, Token la3, string insBase, string detail)
        {
            return
                la1.kind != _string && Engine.StringsAreEqual(la1.val, insBase) &&
                    (detail == null
                        ? (la2.kind == _dot ? la3.kind == _integer : true)
                        : (la2.kind == _dot && la3.kind != _string &&
                            Engine.StringsAreEqual(la3.val, detail))
                        );
        }

        [DebuggerStepThrough]
        private bool isInIntegerGroup()
        {
            return peekIsOneOf(asmIntegerGroup);
        }

        [DebuggerStepThrough]
        private bool isInJumpGroup()
        {
            return peekIsOneOf(asmJumpGroup);
        }

        private bool isInOpAliasGroup()
        {
            return peekIsOneOf(asmOpAliasGroup);
        }

        [DebuggerStepThrough]
        private bool isInNullGroup()
        {
            return peekIsOneOf(asmNullGroup);
        }

        [DebuggerStepThrough]
        private bool isInIdGroup()
        {
            return peekIsOneOf(asmIdGroup);
        }

        [DebuggerStepThrough]
        private bool isInIdArgGroup()
        {
            return peekIsOneOf(asmIdArgGroup);
        }

        [DebuggerStepThrough]
        private bool isInArgGroup()
        {
            return peekIsOneOf(asmArgGroup);
        }

        [DebuggerStepThrough]
        private bool isInQualidArgGroup()
        {
            return peekIsOneOf(asmQualidArgGroup);
        }

        //[NoDebug()]
        private bool peekIsOneOf(string[,] table)
        {
            scanner.ResetPeek();
            var la1 = la.kind == _at ? scanner.Peek() : la;
            var la2 = scanner.Peek();
            var la3 = scanner.Peek();
            for (var i = table.GetUpperBound(0); i >= 0; i--)
                if (checkAsmInstruction(la1, la2, la3, table[i, 0], table[i, 1]))
                    return true;
            return false;
        }

        private readonly SymbolTable<OpCode> _instructionNameTable = new SymbolTable<OpCode>(60);

        private readonly SymbolTable<Tuple<string, int>> _opAliasTable =
            new SymbolTable<Tuple<string, int>>(32);

        [DebuggerStepThrough]
        private void _createTableOfInstructions()
        {
            var tab = _instructionNameTable;
            //Add original names
            foreach (var code in Enum.GetNames(typeof (OpCode)))
                tab.Add(code.Replace('_', '.'), (OpCode) Enum.Parse(typeof (OpCode), code));

            //Add instruction aliases -- NOTE: You'll also have to add them to the respective groups
            tab.Add("new", OpCode.newobj);
            tab.Add("check", OpCode.check_const);
            tab.Add("cast", OpCode.cast_const);
            tab.Add("ret", OpCode.ret_value);
            tab.Add("ret.val", OpCode.ret_value);
            tab.Add("yield", OpCode.ret_continue);
            tab.Add("exit", OpCode.ret_exit);
            tab.Add("break", OpCode.ret_break);
            tab.Add("continue", OpCode.ret_continue);
            tab.Add("jump.true", OpCode.jump_t);
            tab.Add("jump.false", OpCode.jump_f);
            tab.Add("inc", OpCode.incloc);
            tab.Add("inci", OpCode.incloci);
            tab.Add("dec", OpCode.decloc);
            tab.Add("deci", OpCode.decloci);
            tab.Add("inda", OpCode.indarg);
            tab.Add("cor", OpCode.newcor);
            tab.Add("exception", OpCode.exc);
            tab.Add("ldnull", OpCode.ldc_null);

            //Add operator aliases
            var ops = _opAliasTable;
            ops.Add("neg", Tuple.Create(UnaryNegation.DefaultAlias, 1));
            ops.Add("not", Tuple.Create(LogicalNot.DefaultAlias, 1));
            ops.Add("add", Tuple.Create(Addition.DefaultAlias, 2));
            ops.Add("sub", Tuple.Create(Subtraction.DefaultAlias, 2));
            ops.Add("mul", Tuple.Create(Multiplication.DefaultAlias, 2));
            ops.Add("div", Tuple.Create(Division.DefaultAlias, 2));
            ops.Add("mod", Tuple.Create(Modulus.DefaultAlias, 2));
            ops.Add("pow", Tuple.Create(Power.DefaultAlias, 2));
            ops.Add("ceq", Tuple.Create(Equality.DefaultAlias, 2));
            ops.Add("cne", Tuple.Create(Inequality.DefaultAlias, 2));
            ops.Add("clt", Tuple.Create(LessThan.DefaultAlias, 2));
            ops.Add("cle", Tuple.Create(LessThanOrEqual.DefaultAlias, 2));
            ops.Add("cgt", Tuple.Create(GreaterThan.DefaultAlias, 2));
            ops.Add("cge", Tuple.Create(GreaterThanOrEqual.DefaultAlias, 2));
            ops.Add("or", Tuple.Create(BitwiseOr.DefaultAlias, 2));
            ops.Add("and", Tuple.Create(BitwiseAnd.DefaultAlias, 2));
            ops.Add("xor", Tuple.Create(ExclusiveOr.DefaultAlias, 2));
        }

        //[DebuggerStepThrough]
        private string getOpAlias(string insBase, string detail, out int argc)
        {
            var combined = insBase + (detail == null ? "" : "." + detail);
            var entry = _opAliasTable.GetDefault(combined, null);
            if (entry == null)
            {
                argc = -1;
                return null;
            }
            else
            {
                argc = entry.Item2;
                return entry.Item1;
            }
        }

        //[DebuggerStepThrough]
        private OpCode getOpCode(string insBase, string detail)
        {
            var combined = insBase + (detail == null ? "" : "." + detail);
            return _instructionNameTable.GetDefault(combined, OpCode.invalid);
        }

        #region Instruction tables

        private readonly string[,] asmIntegerGroup =
            {
                {"ldc", "int"},
                {"pop", null},
                {"dup", null},
                {"ldloci", null},
                {"stloci", null},
                {"incloci", null},
                {"inci", null},
                {"deci", null},
                {"ldr", "loci"}
            };

        private readonly string[,] asmJumpGroup =
            {
                {"jump", null},
                {"jump", "t"},
                {"jump", "f"},
                {"jump", "true"},
                {"jump", "false"},
                {"leave", null}
            };

        private readonly string[,] asmNullGroup =
            {
                {"ldc", "null"},
                {"ldnull", null},
                {"check", "arg"},
                {"check", "null"},
                {"cast", "arg"},
                {"ldr", "eng"},
                {"ldr", "app"},
                {"ret", null},
                {"ret", "set"},
                {"ret", "value"},
                {"ret", "val"},
                {"ret", "break"},
                {"ret", "continue"},
                {"ret", "exit"},
                {"break", null},
                {"continue", null},
                {"yield", null},
                {"exit", null},
                {"try", null},
                {"throw", null},
                {"exc", null},
                {"exception", null}
            };

        private readonly string[,] asmOpAliasGroup =
            {
                {"neg", null},
                {"not", null},
                {"add", null},
                {"sub", null},
                {"mul", null},
                {"div", null},
                {"mod", null},
                {"pow", null},
                {"ceq", null},
                {"cne", null},
                {"clt", null},
                {"cle", null},
                {"cgt", null},
                {"cge", null},
                {"or", null},
                {"and", null},
                {"xor", null},
            };

        private readonly string[,] asmIdGroup =
            {
                {"inc", null},
                {"incloc", null},
                {"incglob", null},
                {"dec", null},
                {"decloc", null},
                {"decglob", null},
                {"ldc", "string"},
                {"ldr", "func"},
                {"ldr", "cmd"},
                {"ldr", "glob"},
                {"ldr", "loc"},
                {"ldr", "type"},
                {"ldloc", null},
                {"stloc", null},
                {"ldglob", null},
                {"stglob", null},
                {"check", "const"},
                {"check", null},
                {"cast", "const"},
                {"cast", null},
                {"newclo", null}
            };

        private readonly string[,] asmIdArgGroup =
            {
                {"newtype", null},
                {"get", null},
                {"set", null},
                {"func", null},
                {"cmd", null},
                {"indloc", null},
                {"indglob", null}
            };

        private readonly string[,] asmArgGroup =
            {
                {"indarg", null},
                {"inda", null},
                {"newcor", null},
                {"cor", null},
                {"tail", null}
            };

        private readonly string[,] asmQualidArgGroup =
            {
                {"sget", null},
                {"sset", null},
                {"newobj", null},
                {"new", null}
            };

        #endregion

        #endregion

        #endregion

        private void _fallbackObjectCreation(Parser parser, IAstType type, out IAstExpression expr,
            out ArgumentsProxy args)
        {
            var typeExpr = type as AstDynamicTypeExpression;
            SymbolEntry fallbackSymbol;
            if (
                //is a type expression we understand (Parser currently only generates dynamic type expressions)
                //  constant type expressions are recognized during optimization
                typeExpr != null
                    //happens in case of parse failure
                    && typeExpr.TypeId != null
                        //there is no such thing as a parametrized struct
                        && typeExpr.Arguments.Count == 0
                            //built-in types take precedence
                            && !ParentEngine.PTypeRegistry.Contains(typeExpr.TypeId)
                                //in case neither the built-in type nor the struct constructor exists, 
                                //  stay with built-in types for predictibility
                                &&
                                target.Symbols.TryGetValue(
                                    Loader.ObjectCreationFallbackPrefix + typeExpr.TypeId,
                                    out fallbackSymbol))
            {
                if (isLocalVariable(fallbackSymbol.Interpretation) && isOuterVariable(fallbackSymbol.InternalId))
                    target.RequireOuterVariable(fallbackSymbol.InternalId);
                var call = new AstGetSetSymbol(parser, PCall.Get, fallbackSymbol);
                expr = call;
                args = call.Arguments;
            }
            else if (type != null)
            {
                var creation = new AstObjectCreation(parser, type);
                expr = creation;
                args = creation.Arguments;
            }
            else
            {
                SemErr("Failed to transform object creation expression.");
                expr = new AstNull(this);
                args = new ArgumentsProxy(new List<IAstExpression>());
            }
        }
    }
}

// ReSharper restore InconsistentNaming