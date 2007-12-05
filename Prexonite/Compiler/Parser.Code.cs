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
using Prexonite.Compiler.Ast;
using Prexonite.Types;
using System.Globalization;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite.Compiler
{
    internal partial class Parser
    {
        #region Proxy interface

        private Loader _loader;

        public Loader Loader
        {
            [NoDebug()]
            get { return _loader; }
        }

        public Application TargetApplication
        {
            [NoDebug()]
            get { return _loader.Options.TargetApplication; }
        }

        public LoaderOptions Options
        {
            [NoDebug()]
            get { return _loader.Options; }
        }

        public Engine ParentEngine
        {
            [NoDebug()]
            get { return _loader.Options.ParentEngine; }
        }

        public SymbolTable<SymbolEntry> Symbols
        {
            [NoDebug()]
            get { return _loader.Symbols; }
        }

        public Loader.FunctionTargetsIterator FunctionTargets
        {
            [NoDebug()]
            get { return _loader.FunctionTargets; }
        }

        private AstProxy _astProxy;

        public AstProxy Ast
        {
            [NoDebug()]
            get { return _astProxy; }
        }

        [NoDebug()]
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

        [NoDebug()]
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

        private Dictionary<int, string> _stringCache = new Dictionary<int, string>();

        [NoDebug()]
        internal string cache(string toCache)
        {
            int hash = toCache.GetHashCode();
            if (_stringCache.ContainsKey(hash))
                return _stringCache[hash];
            else
            {
                _stringCache.Add(hash, toCache);
                return toCache;
            }
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
            get
            {
                return NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent;
            }
        }

        public static NumberStyles IntegerStyle
        {
            get
            {
                return NumberStyles.None;
            }
        }

        [NoDebug]
        public void SemErr(int line, int col, string message)
        {
            errors.SemErr(line, col, message);
        }

        [NoDebug]
        public void SemErr(Token tok, string s)
        {
            errors.SemErr(tok.line, tok.col, s);
        }

        [NoDebug]
        public static bool InterpretationIsVariable(SymbolInterpretations interpretation)
        {
            return
                InterpretationIsGlobalVariable(interpretation) ||
                InterpretationIsLocalVariable(interpretation);
        }

        [NoDebug]
        public static bool InterpretationIsLocalVariable(SymbolInterpretations interpretation)
        {
            return
                interpretation == SymbolInterpretations.LocalReferenceVariable ||
                interpretation == SymbolInterpretations.LocalObjectVariable;
        }

        [NoDebug]
        public static bool InterpretationIsGlobalVariable(SymbolInterpretations interpretation)
        {
            return
                interpretation == SymbolInterpretations.GlobalReferenceVariable ||
                interpretation == SymbolInterpretations.GlobalObjectVariable;
        }

        [NoDebug]
        public static bool InterpretationIsObjectVariable(SymbolInterpretations interpretation)
        {
            return
                interpretation == SymbolInterpretations.LocalObjectVariable ||
                interpretation == SymbolInterpretations.GlobalObjectVariable;
        }

        [NoDebug]
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
            Lexer lex = scanner as Lexer;
            if (lex == null)
                throw new PrexoniteException("The prexonite grammar requires a *Lex-scanner.");
            lex.PushState(state);
        }

        private void _popLexerState()
        {
            Lexer lex = scanner as Lexer;
            if (lex == null)
                throw new PrexoniteException("The prexonite grammar requires a *Lex-scanner.");
            lex.PopState();
            //Might be id or keyword
            if ((la.kind > _BEGINKEYWORDS && la.kind < _ENDKEYWORDS) || la.kind == _id)
                la.kind = lex.checkKeyword(la.val);
        }

        private void _inject(Token c)
        {
            Lexer lex = scanner as Lexer;
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
            Token c = new Token();
            c.kind = kind;
            c.val = val;
            _inject(c);
        }

        private void _inject(int kind)
        {
            _inject(kind, "");
        }

        #endregion //General

        #region Prexonite Script

        public CompilerTarget target;

        [NoDebug]
        public bool isLabel() //LL(2)
        {
            scanner.ResetPeek();
            Token c = la;
            Token cla = scanner.Peek();

            return c.kind == _lid || (isId(c) && cla.kind == _colon);
        }

        [NoDebug]
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
            Token c;
            Token interpretation;
            c = scanner.Peek();
            if (c.kind == _semicolon || c.kind == _comma) //id expected
                return false;

            interpretation = c;

            c = scanner.Peek();
            if (c.kind == _semicolon || c.kind == _comma) //no interpretation
                return true;

            c = scanner.Peek();
            if (interpretation.kind == _ref && (c.kind == _semicolon || c.kind == _comma))
                //is "static ref"
                return true;

            return false; //something else, a GetSetComplex maybe?
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

            switch(la.kind)
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
                    Token c = scanner.Peek();
                    if(c.kind == _assign)
                        return true;
                    else
                        return false;
                default:
                    return false;
            }
        }

        [NoDebug]
        public bool isDeDereference() //LL(2)
        {
            scanner.ResetPeek();
            Token c = la;
            Token cla = scanner.Peek();

            return c.kind == _pointer && cla.kind == _pointer;
        }

        //id is object or reference variable
        [NoDebug]
        public bool isLikeVariable(string id) //context
        {
            if (!target.Symbols.ContainsKey(id))
                return false;
            SymbolInterpretations kind = target.Symbols[id].Interpretation;
            return
                kind == SymbolInterpretations.LocalObjectVariable ||
                kind == SymbolInterpretations.GlobalObjectVariable ||
                kind == SymbolInterpretations.LocalReferenceVariable ||
                kind == SymbolInterpretations.GlobalReferenceVariable;
        }

        //id is like a function
        [NoDebug]
        public bool isLikeFunction(string id) //Context
        {
            if (!target.Symbols.ContainsKey(id))
                return false;
            return isLikeFunction(target.Symbols[id].Interpretation);
        } //context
        //interpretation is like function
        [NoDebug]
        public bool isLikeFunction(SymbolInterpretations kind) //Context
        {
            return
                kind == SymbolInterpretations.Function ||
                kind == SymbolInterpretations.Command ||
                kind == SymbolInterpretations.LocalReferenceVariable ||
                kind == SymbolInterpretations.GlobalReferenceVariable;
        } //context
        [NoDebug]
        public string getTypeName(string typeId, bool staticPrefix)
        {
            if (staticPrefix) //already marked as CLR call
                return ObjectPType.Literal + "(\"" + StringPType.Escape(typeId) + "\")";
            else
                foreach (string importedNamespace in target.Function.ImportedNamespaces)
                    if (typeId.StartsWith(importedNamespace, StringComparison.OrdinalIgnoreCase))
                        return ObjectPType.Literal + "(\"" + StringPType.Escape(typeId) + "\")";
            return typeId;
        }

        private bool isFollowedByStatementBlock()
        {
            scanner.ResetPeek();
            return scanner.Peek().kind == _lbrace;
        }

        //[NoDebug]
        private bool isLambdaExpression() //LL(*)
        {
            scanner.ResetPeek();

            Token c = la;
            if (!(c.kind == _lpar || isId(c)))
                return false;
            Token cla = scanner.Peek();

            bool requirePar = false;
            if (c.kind == _lpar)
            {
                requirePar = true;
                c = cla;
                cla = scanner.Peek();

                //Check for lambda expression without arguments
                if (c.kind == _rpar && cla.kind == _implementation)
                    return true;
            }

            if (isId(c))
            {
                //break if lookahead is not valid to save tokens
                if (
                    !(cla.kind == _comma || cla.kind == _implementation ||
                      (cla.kind == _rpar && requirePar)))
                    return false;
                //Consume 1
                c = cla;
                cla = scanner.Peek();
            }
            else if ((c.kind == _var || c.kind == _ref) && isId(cla))
            {
                //Consume 2
                c = scanner.Peek();
                //break if lookahead is not valid to save tokens
                if (
                    !(c.kind == _comma || c.kind == _implementation ||
                      (c.kind == _rpar && requirePar)))
                    return false;
                cla = scanner.Peek();
            }
            else
            {
                return false;
            }

            while (c.kind == _comma && requirePar)
            {
                //Consume comma
                c = cla;
                cla = scanner.Peek();

                if (isId(c))
                {
                    //Consume 1
                    c = cla;
                    cla = scanner.Peek();
                }
                else if ((c.kind == _var || c.kind == _ref) && isId(cla))
                {
                    //Consume 2
                    c = scanner.Peek();
                    cla = scanner.Peek();
                }
                else
                {
                    return false;
                }
            }

            if (requirePar)
                if (c.kind == _rpar)
                {
                    c = cla;
                    //cla = scanner.Peek();
                }
                else
                {
                    return false;
                }

            return c.kind == _implementation;
        } //LL(*)

        [NoDebug]
        private bool isIndirectCall() //LL(2)
        {
            scanner.ResetPeek();
            Token c = la;
            Token cla = scanner.Peek();

            return c.kind == _dot && cla.kind == _lpar;
        }

        [NoDebug]
        private bool isOuterVariable(string id) //context
        {
            for (CompilerTarget parent = target.ParentTarget;
                 parent != null;
                 parent = parent.ParentTarget)
            {
                PFunction func = parent.Function;
                if (func.Variables.Contains(id) || func.Parameters.Contains(id))
                    return true;
            }
            return false;
        }

        private string generateNestedFunctionId()
        {
            return generateNestedFunctionId("");
        }

        private string generateNestedFunctionId(string prefix)
        {
            return generateNestedFunctionId(target, prefix);
        }

        // Not currently used
        private static string generateNestedFunctionId(CompilerTarget thisTarget)
        {
            return generateNestedFunctionId(thisTarget, "");
        }

        private static string generateNestedFunctionId(CompilerTarget thisTarget, string prefix)
        {
            if (thisTarget == null)
                throw new ArgumentNullException("thisTarget");
            if (prefix == null)
                prefix = "";
            return
                thisTarget.Function.Id + "\\nested\\" + prefix +
                (thisTarget.NestedFunctionCounter++);
        }

        private void SmartDeclareLocal(string id, SymbolInterpretations kind)
        {
            if (isOuterVariable(id))
            {
                target.RequireOuterVariable(id);
                target.Declare(kind, id, id);
            }
            else
                target.Define(kind, id);
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

        [NoDebug]
        private bool isQualification()
        {
            return la.kind == _doublecolon || isNs();
        }

        [NoDebug]
        private bool isNs()
        {
            scanner.ResetPeek();
            d = la;
            dla = scanner.Peek();

            return isId(d) && dla.kind == _doublecolon;
        }

        [NoDebug]
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

        [NoDebug]
        private static bool isGlobalId(Token c)
        {
            return c.kind == _id || c.kind == _anyId;
        }

        #endregion

        #region Assembler

        [NoDebug()]
        public void addInstruction(AstBlock block, Instruction ins)
        {
            block.Add(new AstAsmInstruction(this, ins));
        }

        [NoDebug()]
        public void addLabel(AstBlock block, string label)
        {
            block.Statements.Add(new AstExplicitLabel(this, label));
        }

        [NoDebug()]
        private bool isAsmInstruction(string insBase, string detail) //LL(4)
        {
            scanner.ResetPeek();
            Token la1 = la.kind == _at ? scanner.Peek() : la;
            Token la2 = scanner.Peek();
            Token la3 = scanner.Peek();
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

        [NoDebug()]
        private bool isInIntegerGroup()
        {
            return peekIsOneOf(asmIntegerGroup);
        }

        [NoDebug()]
        private bool isInJumpGroup()
        {
            return peekIsOneOf(asmJumpGroup);
        }

        //[NoDebug()]
        private bool isInNullGroup()
        {
            return peekIsOneOf(asmNullGroup);
        }

        [NoDebug()]
        private bool isInIdGroup()
        {
            return peekIsOneOf(asmIdGroup);
        }

        [NoDebug()]
        private bool isInIdArgGroup()
        {
            return peekIsOneOf(asmIdArgGroup);
        }

        [NoDebug]
        private bool isInArgGroup()
        {
            return peekIsOneOf(asmArgGroup);
        }

        [NoDebug()]
        private bool isInQualidArgGroup()
        {
            return peekIsOneOf(asmQualidArgGroup);
        }

        //[NoDebug()]
        private bool peekIsOneOf(string[,] table)
        {
            scanner.ResetPeek();
            Token la1 = la.kind == _at ? scanner.Peek() : la;
            Token la2 = scanner.Peek();
            Token la3 = scanner.Peek();
            for (int i = table.GetUpperBound(0); i >= 0; i--)
                if (checkAsmInstruction(la1, la2, la3, table[i, 0], table[i, 1]))
                    return true;
            return false;
        }

        private SymbolTable<OpCode> _tableOfInstructionNames = new SymbolTable<OpCode>(60);

        [NoDebug()]
        private void _createTableOfInstructions()
        {
            SymbolTable<OpCode> tab = _tableOfInstructionNames;
            //Add original names
            foreach (string code in Enum.GetNames(typeof(OpCode)))
                tab.Add(code, (OpCode) Enum.Parse(typeof(OpCode), code));

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
        }

        [NoDebug()]
        private OpCode getOpCode(string insBase, string detail)
        {
            string combined = insBase + (detail == null ? "" : "_" + detail);
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
                {"cor", null}
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