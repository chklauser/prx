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
using Prexonite.Compiler.Symbolic;
using Prexonite.Compiler.Symbolic.Compatibility;
using Prexonite.Modular;
using Prexonite.Types;

// ReSharper disable InconsistentNaming

namespace Prexonite.Compiler
{
    internal partial class Parser
    {
        [DebuggerStepThrough]
        internal Parser(IScanner scanner, Loader loader)
            : this(scanner)
        {
            if (loader == null)
                throw new ArgumentNullException("loader");
            _loader = loader;
            _createTableOfInstructions();
            _astProxy = new AstProxy(this);
            _astFactory = new ParserAstFactory(this);
        }

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

        public Module TargetModule
        {
            [DebuggerStepThrough]
            get { return TargetApplication.Module; }
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

        public SymbolStore Symbols
        {
            [DebuggerStepThrough]
            get { return target != null ? target.CurrentBlock.Symbols : _loader.Symbols; }
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

        private CompilerTarget _target;

        public CompilerTarget target
        {
            [DebuggerStepThrough]
            get { return _target; }
        }

        public ISourcePosition GetPosition()
        {
            return new SourcePosition(scanner.File, t.line, t.col);
        }

        public AstBlock CurrentBlock
        {
            get { return target == null ? null : target.CurrentBlock; }
        }

        private readonly IAstFactory _astFactory;

        protected IAstFactory Create
        {
            get { return _astFactory; }
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

        public static bool TryParseVersion(string s, out Version version)
        {
            return System.Version.TryParse(s, out version);
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

        /// <summary>
        /// Defines a new global variable (or returns the existing declaration and instance)
        /// </summary>
        /// <param name="id">The physical name of the new global variable.</param>
        /// <param name="vari">The variable declaration for the new variable.</param>
        protected PVariable DefineGlobalVariable(string id, out VariableDeclaration vari)
        {
            if (TargetModule.Variables.TryGetVariable(id, out vari))
            {
                return TargetApplication.Variables[vari.Id];
            }
            else
            {
                vari = global::Prexonite.Modular.VariableDeclaration.Create(id);
                TargetModule.Variables.Add(vari);
                return TargetApplication.Variables[id] = new PVariable(vari);
            }
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
                buildBlockTarget.Ast.EmitCode(buildBlockTarget, true, StackSemantics.Effect);

                buildBlockTarget.Function.Meta["File"] = scanner.File;
                buildBlockTarget.FinishTarget();
                //Run the build block 
                var fctx = buildBlockTarget.Function.CreateFunctionContext(ParentEngine,
                    new PValue[] { },
                    new PVariable[] { }, true);
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

        #endregion //General

        #region Prexonite Script

        #region Symbol management
        
        internal AstGetSet _NullNode(ISourcePosition position)
        {
            var n = new AstNull(position.File, position.Line, position.Column);
            return new AstIndirectCall(position, PCall.Get, n);
        }

        private readonly ISymbolHandler<List<Message>, Symbol> _listMessages = new ListMessagesHandler();
        private class ListMessagesHandler : ISymbolHandler<List<Message>, Symbol>
        {
            #region Implementation of ISymbolHandler<in List<Message>,out Symbol>

            public Symbol HandleEntity(EntitySymbol symbol, List<Message> argument)
            {
                return symbol;
            }

            public Symbol HandleMessage(MessageSymbol symbol, List<Message> argument)
            {
                argument.Add(symbol.Message);
                return symbol.Symbol.HandleWith(this, argument);
            }

            public Symbol HandleMacroInstance(MacroInstanceSymbol symbol, List<Message> argument)
            {
                return symbol;
            }

            #endregion
        }

        internal bool _TryUseSymbol(string symbolicId, out Symbol symbol)
        {
            SymbolStore ss;
            if (target != null)
                ss = CurrentBlock.Symbols;
            else
                ss = Symbols;
            if (ss.TryGet(symbolicId, out symbol))
            {
                return _TryUseSymbol(ref symbol);
            }
            else
            {
                symbol = null;
                return false;
            }
        }

        internal bool _TryUseSymbol(ref Symbol symbol)
        {
            var msgs = new List<Message>(1);
            symbol = symbol.HandleWith(_listMessages, msgs);
            if (msgs.Count > 0)
            {
                var seen = new HashSet<String>();
                foreach (var message in msgs)
                {
                    var c = message.MessageClass;
                    if (c != null)
                        if (seen.Add(c))
                            continue;
                    _loader.ReportMessage(message);
                    if (message.Severity == MessageSeverity.Error)
                    {
                        symbol = null;
                        return false;
                    }
                }
            }
            return true;
        }

        internal bool _TryUseSymbolEntry(string symbolId, out SymbolEntry entry)
        {
            Symbol symbol;
            EntitySymbol entitySymbol;
            if (_TryUseSymbol(symbolId, out symbol))
            {
                if (symbol.TryGetEntitySymbol(out entitySymbol))
                {
                    entry = entitySymbol.ToSymbolEntry();
                    return true;
                }
                else
                {
                    SemErr(
                        string.Format(
                            "Legacy part of parser cannot deal with symbol {0}. An entity symbol was expected.", symbol));
                    entry = null;
                    return false;
                }
            }
            else
            {
                entry = null;
                return false;
            }
        }

        private readonly Stack<object> _scopeStack = new Stack<object>();

        internal void _PushScope(AstSubBlock block)
        {
            if (!ReferenceEquals(block.LexicalScope, CurrentBlock))
                throw new PrexoniteException("Cannot push scope of unrelated block.");
            _scopeStack.Push(block);
            _target.BeginBlock(block);
        }

        internal void _PushScope(CompilerTarget ct)
        {
            if (!ReferenceEquals(ct.ParentTarget, _target))
                throw new PrexoniteException("Cannot push scope of unrelated compiler target.");
            _scopeStack.Push(ct);
            _target = ct;
        }

        internal void _PopScope(AstSubBlock block)
        {
            if (!ReferenceEquals(_scopeStack.Peek(), block))
                throw new PrexoniteException(string.Format("Tried to pop scope of block {0} but {1} was on top.", block, _scopeStack.Peek()));
            _scopeStack.Pop();
            _target.EndBlock();
        }

        internal void _PopScope(CompilerTarget ct)
        {
            if (!ReferenceEquals(_scopeStack.Peek(), ct))
                throw new PrexoniteException(string.Format("Tried to pop scope of compiler target {0} but {1} was on top.", ct, _scopeStack.Peek()));
            _target = ct.ParentTarget;
            _scopeStack.Pop();
        }

        #endregion

        private bool _requiresModule(SymbolInterpretations kind)
        {
            switch (kind)
            {
               case SymbolInterpretations.Function:
                    case SymbolInterpretations.GlobalObjectVariable:
                    case SymbolInterpretations.GlobalReferenceVariable:
                    return true;
                    default:
                    return false;
            }
        }

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
            Symbol symbol;
            EntitySymbol entitySymbol;
            if(!(target.Symbols.TryGet(id,out symbol) && symbol.TryGetEntitySymbol(out entitySymbol)))
                return false;
            EntityRef.Variable _;
            return entitySymbol.Entity.TryGetVariable(out _);
        }

        public bool isLocalVariable(SymbolInterpretations interpretations)
        {
            return interpretations == SymbolInterpretations.LocalObjectVariable
                || interpretations == SymbolInterpretations.LocalReferenceVariable;
        }

        public bool isId(out string id)
        {
            // Warning: this code duplicates the Id<out string id> rule from the grammar.

            scanner.ResetPeek();
            var t1 = la;
            var t2 = scanner.Peek();

            switch (t1.kind)
            {
                case _id:
                case _add:
                case _enabled:
                case _disabled:
                case _build:
                    id = t1.val;
                    return true;
                case _anyId:
                    id = cache(t2.val);
                    return true;
                default:
                    id = "\\NoId\\";
                    return false;

            }
        }

        //id is like a function
        public bool isLikeFunction() //Context
        {
            string id;
            Symbol symbol;
            EntitySymbol entitySymbol;
            return
                isId(out id) &&
                target.Symbols.TryGet(id, out symbol) &&
                symbol.TryGetEntitySymbol(out entitySymbol) &&
                entitySymbol.Entity.Match(_isLikeFunction, entitySymbol.IsDereferenced);
        }

        private class IsLikeFunctionMatcher :  EntityRefMatcher<bool,bool>
        {
            #region Overrides of EntityRefMatcher<object,bool>

            protected override bool OnNotMatched(EntityRef entity, bool argument)
            {
                return false;
            }

            protected override bool OnCommand(EntityRef.Command command, bool argument)
            {
                return true;
            }

            public override bool OnFunction(EntityRef.Function function, bool argument)
            {
                return true;
            }

            protected override bool OnLocalVariable(EntityRef.Variable.Local variable, bool isDereferenced)
            {
                return isDereferenced;
            }

            protected override bool OnGlobalVariable(EntityRef.Variable.Global variable, bool isDereferenced)
            {
                return isDereferenced;
            }

            protected override bool OnMacroCommand(EntityRef.MacroCommand macroCommand, bool argument)
            {
                return true;
            }

            #endregion
        }
        private readonly IEntityRefMatcher<bool, bool> _isLikeFunction = new IsLikeFunctionMatcher();

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
            string id; 
            return isId(out id) && !target.Symbols.Contains(id);
        }

        private bool _tryResolveFunction(SymbolEntry entry, out PFunction func)
        {
            func = null;
            if(entry.Interpretation != SymbolInterpretations.Function)
                return false;

            return TargetApplication.TryGetFunction(entry.InternalId, entry.Module,out func);
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

        private Symbol ToModuleLocalSymbol(string physicalId, SymbolInterpretations kind)
        {
            switch (kind)
            {
                case SymbolInterpretations.Function:
                    return new EntitySymbol(EntityRef.Function.Create(physicalId,TargetModule.Name));
                case SymbolInterpretations.Command:
                    return new EntitySymbol(EntityRef.Command.Create(physicalId));
                case SymbolInterpretations.LocalObjectVariable:
                    return new EntitySymbol(EntityRef.Variable.Local.Create(physicalId));
                case SymbolInterpretations.LocalReferenceVariable:
                    return new EntitySymbol(EntityRef.Variable.Local.Create(physicalId), true);
                case SymbolInterpretations.GlobalObjectVariable:
                    return new EntitySymbol(EntityRef.Variable.Global.Create(physicalId, TargetModule.Name));
                case SymbolInterpretations.GlobalReferenceVariable:
                    return new EntitySymbol(EntityRef.Variable.Global.Create(physicalId, TargetModule.Name), true);
                case SymbolInterpretations.MacroCommand:
                    return new EntitySymbol(EntityRef.MacroCommand.Create(physicalId));
                default:
                    return
                        new MessageSymbol(
                            Message.Error(
                                string.Format("Invalid symbol interpretation {0}.",
                                              Enum.GetName(typeof (SymbolInterpretations), kind)), GetPosition(),
                                MessageClasses.InvalidSymbolInterpretation),
                            new EntitySymbol(EntityRef.Command.Create(physicalId)));
            }
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
                Symbol s;
                target.Symbols.Declare(logicalId, s = ToModuleLocalSymbol(physicalId,kind));
                EntitySymbol entitySym;
                EntityRef.Variable.Local local;
                if(s.TryGetEntitySymbol(out entitySym) 
                    && entitySym.Entity.TryGetLocalVariable(out local)
                    && !target.Function.Variables.Contains(local.Id))
                {
                    target.Function.Variables.Add(local.Id);
                }
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
                    .Union(new[] { (MetaEntry)local })
                    .ToArray();
        }

        #region Assemble Invocation of Symbol

        private AstGetSet _assembleInvocation(SymbolEntry sym)
        {
            if (isKnownMacroFunction(sym) || sym.Interpretation == SymbolInterpretations.MacroCommand)
            {
                return new AstMacroInvocation(this, sym);
            }
            else
            {
                return new AstGetSetSymbol(this, sym);
            }
        }

        private static readonly AssembleAstHandler AssembleAst = new AssembleAstHandler();

        private class AssembleAstHandler : ISymbolHandler<Tuple<Parser, PCall>, AstGetSet>
        {
            public AstGetSet HandleEntity(EntitySymbol symbol, Tuple<Parser, PCall> argument)
            {
                var access = AstGetSetEntity.Create(argument.Item1.GetPosition(), argument.Item2, symbol.Entity);
                if (symbol.IsDereferenced)
                {
                    return AstIndirectCall.Create(argument.Item1.GetPosition(), access);
                }
                else
                {
                    return access;
                }
            }

            public AstGetSet HandleMessage(MessageSymbol symbol, Tuple<Parser, PCall> argument)
            {
                throw new PrexoniteException(string.Format("Unexpected message still attached to symbol {0}.", symbol));
            }

            public AstGetSet HandleMacroInstance(MacroInstanceSymbol symbol, Tuple<Parser, PCall> argument)
            {
                throw new NotImplementedException("Assembly of macro instance invocations");
            }
        }

        private AstGetSet _assembleInvocation(Symbol sym)
        {
            if (_TryUseSymbol(ref sym))
            {
                return sym.HandleWith(AssembleAst, Tuple.Create(this, PCall.Get));
            }
            else
            {
                return _NullNode(GetPosition());
            }
        }

        public void EnsureInScope(Symbol symbol)
        {
            EntitySymbol es;
            EntityRef.Variable.Local local;
            if (symbol.TryGetEntitySymbol(out es)
                && es.Entity.TryGetLocalVariable(out local)
                && isOuterVariable(local.Id))
                target.RequireOuterVariable(local.Id);
        }

        private void _fallbackObjectCreation(Parser parser, AstTypeExpr type, out AstExpr expr,
            out ArgumentsProxy args)
        {
            var typeExpr = type as AstDynamicTypeExpression;
            Symbol fallbackSymbol;
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
                                target.Symbols.TryGet(
                                    Loader.ObjectCreationFallbackPrefix + typeExpr.TypeId,
                                    out fallbackSymbol))
            {

                EnsureInScope(fallbackSymbol);

                var call = _assembleInvocation(fallbackSymbol);
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
                args = new ArgumentsProxy(new List<AstExpr>());
            }
        }

        private AstExpr _createUnknownExpr()
        {
            return _createUnknownGetSet();
        }

        private AstGetSet _createUnknownGetSet()
        {
            return new AstIndirectCall(this, new AstNull(this));
        }


        #endregion


        #endregion

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

    }
}

// ReSharper restore InconsistentNaming