/*
 * Prexonite, a scripting engine (Scripting Language -> Bytecode -> Virtual Machine)
 *  Copyright (C) 2007  Christian "SealedSun" Klauser
 *  E-mail  sealedsun a.t gmail d.ot com
 *  Web     http://www.sealedsun.ch/
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  Please contact me (sealedsun a.t gmail do.t com) if you need a different license.
 * 
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Prexonite.Compiler.Ast;
using Prexonite.Types;

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

        public static bool TryParseInteger(string s, out int i)
        {
            return Int32.TryParse(s, IntegerStyle, CultureInfo.InvariantCulture, out i);
        }

        public static bool TryParseReal(string s, out double d)
        {
            return Double.TryParse(s, RealStyle, CultureInfo.InvariantCulture, out d);
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
            _inject(kind, "");
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
             *  static id;
             *  var interpretation id;
             *  static interpretation id;
             */

            scanner.ResetPeek();
            //current la = static | var | ref
            if (la.kind != _static && la.kind != _var && la.kind != _ref)
                return false;
            var c = scanner.Peek();
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

        //id is like a function
        [DebuggerStepThrough]
        public bool isLikeFunction() //Context
        {
            var id = la.val;
            return
                la.kind == _id &&
                target.Symbols.ContainsKey(id) &&
                isLikeFunction(target.Symbols[id].Interpretation);
        } //context

        //interpretation is like function
        [DebuggerStepThrough]
        private static bool isLikeFunction(SymbolInterpretations interpretation) //Context
        {
            return
                interpretation == SymbolInterpretations.Function ||
                interpretation == SymbolInterpretations.Command ||
                interpretation == SymbolInterpretations.LocalReferenceVariable ||
                interpretation == SymbolInterpretations.GlobalReferenceVariable;
        } //context

        [DebuggerStepThrough]
        public bool isUnknownId()
        {
            var id = la.val;
            return la.kind == _id && !target.Symbols.ContainsKey(id);
        }

        [DebuggerStepThrough]
        public bool isKnownMacro(SymbolEntry symbol)
        {
            if (symbol.Interpretation != SymbolInterpretations.Function)
                return false;

            PFunction func;
            if (!TargetApplication.Functions.TryGetValue(symbol.Id, out func))
                return false;

            return func.Meta[CompilerTarget.MacroMetaKey].Switch;
        }

        [DebuggerStepThrough]
        public string getTypeName(string typeId, bool staticPrefix)
        {
            if (staticPrefix) //already marked as CLR call
                return ObjectPType.Literal + "(\"" + StringPType.Escape(typeId) + "\")";
            else
                foreach (var importedNamespace in target.Function.ImportedNamespaces)
                    if (typeId.StartsWith(importedNamespace, StringComparison.OrdinalIgnoreCase))
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
        } //LL(*)

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
                if (func.Variables.Contains(id) || func.Parameters.Contains(id))
                    return true;
            }
            return false;
        }

        private string generateLocalId()
        {
            return generateLocalId("");
        }

        private string generateLocalId(string prefix)
        {
            return target.GenerateLocalId(prefix);
        }

        // Not currently used

        private void SmartDeclareLocal(string id, SymbolInterpretations kind)
        {
            SmartDeclareLocal(id, id, kind);
        }

        private void SmartDeclareLocal(string logicalId, string physicalId, SymbolInterpretations kind)
        {
            if (isOuterVariable(physicalId))
            {
                target.RequireOuterVariable(physicalId);
                target.Declare(kind, logicalId, physicalId);
            }
            else
                target.Define(kind, logicalId, physicalId);
        }

/*
        private bool mightBeVariableReference(Token n, Token m)
        {
            return isId(n) || ((n.kind == _var || n.kind == _ref) && isId(m));
        }
*/

        private Token d,
                      dla;

/*
        private  bool isTypeExpr()
        {
            return la.kind == _tilde || isQualification();
        }
*/

        [DebuggerStepThrough]
        private bool isQualification()
        {
            return la.kind == _doublecolon || isNs();
        }

        [DebuggerStepThrough]
        private bool isNs()
        {
            scanner.ResetPeek();
            d = la;
            dla = scanner.Peek();

            return isId(d) && dla.kind == _doublecolon;
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

        #endregion

        private IEnumerable<string> let_bindings(CompilerTarget ft)
        {
            var lets = new HashSet<string>(Engine.DefaultStringComparer);
            for (var ct = ft; ct != null; ct = ct.ParentTarget)
                lets.UnionWith(ct.Function.Meta[PFunction.LetKey].List.Select(e => e.Text));
            return lets;
        }

        private void mark_as_let(PFunction f, string local)
        {
            f.Meta[PFunction.LetKey] = (MetaEntry)
                                       f.Meta[PFunction.LetKey].List
                                           .Union(new[] {(MetaEntry) local})
                                           .ToArray();
        }

        #region Assembler

        [DebuggerStepThrough]
        public void addInstruction(AstBlock block, Instruction ins)
        {
            block.Add(new AstAsmInstruction(this, ins));
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
                     ?
                         (la2.kind == _dot ? la3.kind == _integer : true)
                     :
                         (la2.kind == _dot && la3.kind != _string && Engine.StringsAreEqual(la3.val, detail))
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

        private readonly SymbolTable<OpCode> _tableOfInstructionNames = new SymbolTable<OpCode>(60);

        [DebuggerStepThrough]
        private void _createTableOfInstructions()
        {
            var tab = _tableOfInstructionNames;
            //Add original names
            foreach (var code in Enum.GetNames(typeof (OpCode)))
                tab.Add(code.Replace('_', '.'), (OpCode) Enum.Parse(typeof (OpCode), code));

            //Add aliases -- NOTE: You'll also have to add them to the respective groups
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
        }

        //[DebuggerStepThrough]
        private OpCode getOpCode(string insBase, string detail)
        {
            var combined = insBase + (detail == null ? "" : "." + detail);
            return _tableOfInstructionNames.GetDefault(combined, OpCode.invalid);
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
    }
}