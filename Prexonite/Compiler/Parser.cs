//SOURCE ARRAY
/*Header.atg:29*/using System.Collections.Generic;
using System.Linq;
using FatalError = Prexonite.Compiler.FatalCompilerException;
using StringBuilder = System.Text.StringBuilder;
using Prexonite.Compiler.Ast;
using Prexonite.Types;
using Prexonite.Modular;
using Prexonite.Compiler.Internal;
using Prexonite.Compiler.Symbolic;
using Prexonite.Compiler.Symbolic.Internal;
using Prexonite.Compiler.Symbolic.Compatibility;
using Prexonite.Properties;//END SOURCE ARRAY


#line 27 "C:\Users\Christian\Documents\GitHub\prx\Tools\Parser.frame" //FRAME

// ReSharper disable RedundantUsingDirective
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantNameQualifier
// ReSharper disable SuggestUseVarKeywordEvident
// ReSharper disable TooWideLocalVariableScope
// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable RedundantArgumentName
// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable RedundantArgumentDefaultValue

using System;
using System.Diagnostics;
using System.Globalization;
using JetBrains.Annotations;
using Prexonite.Internal;
using Prexonite.Compiler;

#line default //END FRAME -->namespace

namespace Prexonite.Compiler {


#line 44 "C:\Users\Christian\Documents\GitHub\prx\Tools\Parser.frame" //FRAME


[System.Runtime.CompilerServices.CompilerGenerated]
internal partial class Parser {

#line default //END FRAME -->constants

	public const int _EOF = 0;
	public const int _id = 1;
	public const int _anyId = 2;
	public const int _lid = 3;
	public const int _ns = 4;
	public const int _version = 5;
	public const int _integer = 6;
	public const int _real = 7;
	public const int _realLike = 8;
	public const int _string = 9;
	public const int _bitAnd = 10;
	public const int _assign = 11;
	public const int _comma = 12;
	public const int _dec = 13;
	public const int _div = 14;
	public const int _dot = 15;
	public const int _eq = 16;
	public const int _gt = 17;
	public const int _ge = 18;
	public const int _inc = 19;
	public const int _lbrace = 20;
	public const int _lbrack = 21;
	public const int _lpar = 22;
	public const int _lt = 23;
	public const int _le = 24;
	public const int _minus = 25;
	public const int _ne = 26;
	public const int _bitOr = 27;
	public const int _plus = 28;
	public const int _pow = 29;
	public const int _rbrace = 30;
	public const int _rbrack = 31;
	public const int _rpar = 32;
	public const int _tilde = 33;
	public const int _times = 34;
	public const int _semicolon = 35;
	public const int _colon = 36;
	public const int _doublecolon = 37;
	public const int _coalescence = 38;
	public const int _question = 39;
	public const int _pointer = 40;
	public const int _implementation = 41;
	public const int _at = 42;
	public const int _appendleft = 43;
	public const int _appendright = 44;
	public const int _var = 45;
	public const int _ref = 46;
	public const int _true = 47;
	public const int _false = 48;
	public const int _BEGINKEYWORDS = 49;
	public const int _mod = 50;
	public const int _is = 51;
	public const int _as = 52;
	public const int _not = 53;
	public const int _enabled = 54;
	public const int _disabled = 55;
	public const int _function = 56;
	public const int _command = 57;
	public const int _asm = 58;
	public const int _declare = 59;
	public const int _build = 60;
	public const int _return = 61;
	public const int _in = 62;
	public const int _to = 63;
	public const int _add = 64;
	public const int _continue = 65;
	public const int _break = 66;
	public const int _yield = 67;
	public const int _or = 68;
	public const int _and = 69;
	public const int _xor = 70;
	public const int _label = 71;
	public const int _goto = 72;
	public const int _static = 73;
	public const int _null = 74;
	public const int _if = 75;
	public const int _unless = 76;
	public const int _else = 77;
	public const int _new = 78;
	public const int _coroutine = 79;
	public const int _from = 80;
	public const int _do = 81;
	public const int _does = 82;
	public const int _while = 83;
	public const int _until = 84;
	public const int _for = 85;
	public const int _foreach = 86;
	public const int _try = 87;
	public const int _catch = 88;
	public const int _finally = 89;
	public const int _throw = 90;
	public const int _then = 91;
	public const int _uusing = 92;
	public const int _macro = 93;
	public const int _lazy = 94;
	public const int _let = 95;
	public const int _method = 96;
	public const int _this = 97;
	public const int _namespace = 98;
	public const int _export = 99;
	public const int _ENDKEYWORDS = 100;
	public const int _LPopExpr = 101;
	public enum Terminals
	{
		@EOF = 0,
		@id = 1,
		@anyId = 2,
		@lid = 3,
		@ns = 4,
		@version = 5,
		@integer = 6,
		@real = 7,
		@realLike = 8,
		@string = 9,
		@bitAnd = 10,
		@assign = 11,
		@comma = 12,
		@dec = 13,
		@div = 14,
		@dot = 15,
		@eq = 16,
		@gt = 17,
		@ge = 18,
		@inc = 19,
		@lbrace = 20,
		@lbrack = 21,
		@lpar = 22,
		@lt = 23,
		@le = 24,
		@minus = 25,
		@ne = 26,
		@bitOr = 27,
		@plus = 28,
		@pow = 29,
		@rbrace = 30,
		@rbrack = 31,
		@rpar = 32,
		@tilde = 33,
		@times = 34,
		@semicolon = 35,
		@colon = 36,
		@doublecolon = 37,
		@coalescence = 38,
		@question = 39,
		@pointer = 40,
		@implementation = 41,
		@at = 42,
		@appendleft = 43,
		@appendright = 44,
		@var = 45,
		@ref = 46,
		@true = 47,
		@false = 48,
		@BEGINKEYWORDS = 49,
		@mod = 50,
		@is = 51,
		@as = 52,
		@not = 53,
		@enabled = 54,
		@disabled = 55,
		@function = 56,
		@command = 57,
		@asm = 58,
		@declare = 59,
		@build = 60,
		@return = 61,
		@in = 62,
		@to = 63,
		@add = 64,
		@continue = 65,
		@break = 66,
		@yield = 67,
		@or = 68,
		@and = 69,
		@xor = 70,
		@label = 71,
		@goto = 72,
		@static = 73,
		@null = 74,
		@if = 75,
		@unless = 76,
		@else = 77,
		@new = 78,
		@coroutine = 79,
		@from = 80,
		@do = 81,
		@does = 82,
		@while = 83,
		@until = 84,
		@for = 85,
		@foreach = 86,
		@try = 87,
		@catch = 88,
		@finally = 89,
		@throw = 90,
		@then = 91,
		@uusing = 92,
		@macro = 93,
		@lazy = 94,
		@let = 95,
		@method = 96,
		@this = 97,
		@namespace = 98,
		@export = 99,
		@ENDKEYWORDS = 100,
		@LPopExpr = 101,
	}
	const int maxT = 102;

#line 48 "C:\Users\Christian\Documents\GitHub\prx\Tools\Parser.frame" //FRAME

	const bool T = true;
	const bool x = false;
	const int minErrDist = 2;
	
	internal readonly Prexonite.Internal.IScanner scanner;
	internal readonly Errors  errors;

	internal Token t;    // last recognized token
	internal Token la;   // lookahead token
	int errDist = minErrDist;


#line default //END FRAME -->declarations

//SOURCE ARRAY
//END SOURCE ARRAY

#line 60 "C:\Users\Christian\Documents\GitHub\prx\Tools\Parser.frame" //FRAME


    [DebuggerNonUserCode]
	private Parser(Prexonite.Internal.IScanner scanner) {
		this.scanner = scanner;
		errors = new Errors();
		errors.parentParser = this;
	}

  
  public Prexonite.Compiler.ISourcePosition GetPosition()
  {
      return new Prexonite.Compiler.SourcePosition(scanner.File, t.line, t.col);
  }

    [DebuggerNonUserCode]
	void SynErr (int n) {
		if (errDist >= minErrDist) errors.SynErr(la.line, la.col, n);
		errDist = 0;
	}

    [DebuggerNonUserCode]
    [Obsolete("Use Loader.ReportMessage instead.")]
	internal void SemErr (string msg) {
		if (errDist >= minErrDist) errors.SemErr(t.line, t.col, msg);
		errDist = 0;
	}
	
	[DebuggerNonUserCode]
	void Get () {
		for (;;) {
			t = la;
			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }

#line default //END FRAME -->pragmas


#line 94 "C:\Users\Christian\Documents\GitHub\prx\Tools\Parser.frame" //FRAME

			la = t;
		}
	}
	
	[DebuggerNonUserCode]
	void Expect (int n) {
		if (la.kind==n) Get(); else { SynErr(n); }
	}
	
	[DebuggerNonUserCode]
	bool StartOf (int s) {
		return set[s, la.kind];
	}
	
	[DebuggerNonUserCode,PublicAPI]
	void ExpectWeak (int n, int follow) {
		if (la.kind == n) Get();
		else {
			SynErr(n);
			while (!StartOf(follow)) Get();
		}
	}
	
	[DebuggerNonUserCode]
	bool WeakSeparator (int n, int syFol, int repFol) {
		bool[] s = new bool[maxT+1];
		if (la.kind == n) { Get(); return true; }
		else if (StartOf(repFol)) return false;
		else {
			for (int i=0; i <= maxT; i++) {
				s[i] = set[syFol, i] || set[repFol, i] || set[0, i];
			}
			SynErr(n);
			while (!s[la.kind]) Get();
			return StartOf(syFol);
		}
	}
	

#line default //END FRAME -->productions

	void AsmStatementBlock(/*Parser.Assembler.atg:28*/AstBlock block) {
		if (la.kind == _lbrace) {
			Get();
			while (StartOf(1)) {
				AsmInstruction(/*Parser.Assembler.atg:31*/block);
			}
			Expect(_rbrace);
		} else if (StartOf(1)) {
			AsmInstruction(/*Parser.Assembler.atg:34*/block);
			Expect(_semicolon);
		} else SynErr(103);
	}

	void AsmInstruction(/*Parser.Assembler.atg:37*/AstBlock block) {
		/*Parser.Assembler.atg:37*/int arguments = 0;
		string id;
		double dblArg;
		string insbase; string detail = null;
		bool bolArg;
		OpCode code;
		bool justEffect = false;
		int values;
		int rotations;
		int index;
		
		if (la.kind == _var || la.kind == _ref) {
			/*Parser.Assembler.atg:50*/SymbolInterpretations kind = SymbolInterpretations.LocalObjectVariable; 
			if (la.kind == _var) {
				Get();
			} else {
				Get();
				/*Parser.Assembler.atg:51*/kind = SymbolInterpretations.LocalReferenceVariable; 
			}
			AsmId(/*Parser.Assembler.atg:53*/out id);
			/*Parser.Assembler.atg:56*/target.Function.Variables.Add(id);
			SmartDeclareLocal(id, kind);
			
			while (la.kind == _comma) {
				Get();
				AsmId(/*Parser.Assembler.atg:60*/out id);
				/*Parser.Assembler.atg:62*/target.Function.Variables.Add(id);
				SmartDeclareLocal(id, kind);
				
			}
		} else if (/*Parser.Assembler.atg:68*/isInOpAliasGroup()) {
			AsmId(/*Parser.Assembler.atg:68*/out insbase);
			if (la.kind == _dot) {
				Get();
				AsmId(/*Parser.Assembler.atg:69*/out detail);
			}
			/*Parser.Assembler.atg:70*/addOpAlias(block, insbase, detail); 
		} else if (/*Parser.Assembler.atg:73*/isInNullGroup()) {
			AsmId(/*Parser.Assembler.atg:73*/out insbase);
			if (la.kind == _dot) {
				Get();
				AsmId(/*Parser.Assembler.atg:74*/out detail);
			}
			/*Parser.Assembler.atg:75*/code = getOpCode(insbase, detail);
			addInstruction(block, new Instruction(code));
			
		} else if (/*Parser.Assembler.atg:81*/isAsmInstruction("label",null) ) {
			AsmId(/*Parser.Assembler.atg:81*/out insbase);
			AsmId(/*Parser.Assembler.atg:84*/out id);
			/*Parser.Assembler.atg:85*/addLabel(block, id); 
		} else if (/*Parser.Assembler.atg:88*/isAsmInstruction("nop", null)) {
			AsmId(/*Parser.Assembler.atg:88*/out insbase);
			/*Parser.Assembler.atg:88*/var ins = new Instruction(OpCode.nop); 
			if (la.kind == _plus) {
				Get();
				AsmId(/*Parser.Assembler.atg:89*/out id);
				/*Parser.Assembler.atg:89*/ins = ins.With(id: id); 
			}
			/*Parser.Assembler.atg:91*/addInstruction(block, ins); 
		} else if (/*Parser.Assembler.atg:95*/isAsmInstruction("rot", null)) {
			AsmId(/*Parser.Assembler.atg:95*/out insbase);
			Expect(_dot);
			Integer(/*Parser.Assembler.atg:96*/out rotations);
			Expect(_comma);
			Integer(/*Parser.Assembler.atg:97*/out values);
			/*Parser.Assembler.atg:99*/addInstruction(block, Instruction.CreateRotate(rotations, values)); 
		} else if (/*Parser.Assembler.atg:103*/isAsmInstruction("indloci", null)) {
			if (la.kind == _at) {
				Get();
				/*Parser.Assembler.atg:103*/justEffect = true; 
			}
			AsmId(/*Parser.Assembler.atg:105*/out insbase);
			Expect(_dot);
			Integer(/*Parser.Assembler.atg:106*/out arguments);
			Integer(/*Parser.Assembler.atg:107*/out index);
			/*Parser.Assembler.atg:109*/addInstruction(block, Instruction.CreateIndLocI(index, arguments, justEffect)); 
		} else if (/*Parser.Assembler.atg:112*/isAsmInstruction("swap", null)) {
			AsmId(/*Parser.Assembler.atg:112*/out insbase);
			/*Parser.Assembler.atg:113*/addInstruction(block, Instruction.CreateExchange()); 
		} else if (/*Parser.Assembler.atg:118*/isAsmInstruction("ldc", "real")) {
			AsmId(/*Parser.Assembler.atg:118*/out insbase);
			Expect(_dot);
			AsmId(/*Parser.Assembler.atg:120*/out detail);
			SignedReal(/*Parser.Assembler.atg:121*/out dblArg);
			/*Parser.Assembler.atg:122*/addInstruction(block, Instruction.CreateConstant(dblArg)); 
		} else if (/*Parser.Assembler.atg:127*/isAsmInstruction("ldc", "bool")) {
			AsmId(/*Parser.Assembler.atg:127*/out insbase);
			Expect(_dot);
			AsmId(/*Parser.Assembler.atg:129*/out detail);
			Boolean(/*Parser.Assembler.atg:130*/out bolArg);
			/*Parser.Assembler.atg:131*/addInstruction(block, Instruction.CreateConstant(bolArg)); 
		} else if (/*Parser.Assembler.atg:136*/isInIntegerGroup()) {
			AsmId(/*Parser.Assembler.atg:136*/out insbase);
			if (la.kind == _dot) {
				Get();
				AsmId(/*Parser.Assembler.atg:137*/out detail);
			}
			SignedInteger(/*Parser.Assembler.atg:138*/out arguments);
			/*Parser.Assembler.atg:139*/code = getOpCode(insbase, detail);
			addInstruction(block, new Instruction(code, arguments));
			
		} else if (/*Parser.Assembler.atg:145*/isInJumpGroup()) {
			AsmId(/*Parser.Assembler.atg:145*/out insbase);
			if (la.kind == _dot) {
				Get();
				AsmId(/*Parser.Assembler.atg:146*/out detail);
			}
			/*Parser.Assembler.atg:147*/Instruction ins = null;
			code = getOpCode(insbase, detail);
			
			if (StartOf(2)) {
				AsmId(/*Parser.Assembler.atg:151*/out id);
				/*Parser.Assembler.atg:153*/ins = new Instruction(code, -1, id);
				
			} else if (la.kind == _integer) {
				Integer(/*Parser.Assembler.atg:155*/out arguments);
				/*Parser.Assembler.atg:155*/ins = new Instruction(code, arguments); 
			} else SynErr(104);
			/*Parser.Assembler.atg:156*/addInstruction(block, ins); 
		} else if (/*Parser.Assembler.atg:161*/isInIdGroup()) {
			AsmId(/*Parser.Assembler.atg:161*/out insbase);
			if (la.kind == _dot) {
				Get();
				AsmId(/*Parser.Assembler.atg:162*/out detail);
			}
			AsmId(/*Parser.Assembler.atg:163*/out id);
			/*Parser.Assembler.atg:164*/code = getOpCode(insbase, detail);
			addInstruction(block, new Instruction(code, id));
			
		} else if (/*Parser.Assembler.atg:171*/isInIdArgGroup()) {
			if (la.kind == _at) {
				Get();
				/*Parser.Assembler.atg:171*/justEffect = true; 
			}
			AsmId(/*Parser.Assembler.atg:173*/out insbase);
			if (la.kind == _dot) {
				Get();
				Integer(/*Parser.Assembler.atg:174*/out arguments);
			} else if (StartOf(2)) {
				/*Parser.Assembler.atg:175*/arguments = 0; 
			} else SynErr(105);
			AsmId(/*Parser.Assembler.atg:177*/out id);
			/*Parser.Assembler.atg:178*/code = getOpCode(insbase, null);
			addInstruction(block, new Instruction(code, arguments, id, justEffect));
			
		} else if (/*Parser.Assembler.atg:184*/isInArgGroup()) {
			if (la.kind == _at) {
				Get();
				/*Parser.Assembler.atg:184*/justEffect = true; 
			}
			AsmId(/*Parser.Assembler.atg:186*/out insbase);
			if (la.kind == _dot) {
				Get();
				Integer(/*Parser.Assembler.atg:187*/out arguments);
			} else if (StartOf(3)) {
				/*Parser.Assembler.atg:188*/arguments = 0; 
			} else SynErr(106);
			/*Parser.Assembler.atg:190*/code = getOpCode(insbase, null);
			addInstruction(block, new Instruction(code, arguments, null, justEffect));
			
		} else if (/*Parser.Assembler.atg:196*/isInQualidArgGroup()) {
			if (la.kind == _at) {
				Get();
				/*Parser.Assembler.atg:196*/justEffect = true; 
			}
			AsmId(/*Parser.Assembler.atg:198*/out insbase);
			if (la.kind == _dot) {
				Get();
				Integer(/*Parser.Assembler.atg:199*/out arguments);
			} else if (StartOf(2)) {
				/*Parser.Assembler.atg:200*/arguments = 0; 
			} else SynErr(107);
			AsmQualid(/*Parser.Assembler.atg:202*/out id);
			/*Parser.Assembler.atg:203*/code = getOpCode(insbase, null);
			addInstruction(block, new Instruction(code, arguments, id, justEffect));
			
		} else if (StartOf(2)) {
			AsmId(/*Parser.Assembler.atg:208*/out insbase);
			/*Parser.Assembler.atg:208*/SemErr("Invalid assembler instruction \"" + insbase + "\" (" + t + ")."); 
		} else SynErr(108);
	}

	void AsmId(/*Parser.Assembler.atg:212*/out string id) {
		/*Parser.Assembler.atg:212*/id = "\\NoId\\"; 
		if (la.kind == _string) {
			String(/*Parser.Assembler.atg:214*/out id);
		} else if (StartOf(4)) {
			Id(/*Parser.Assembler.atg:215*/out id);
		} else if (StartOf(5)) {
			switch (la.kind) {
			case _mod: {
				Get();
				break;
			}
			case _is: {
				Get();
				break;
			}
			case _not: {
				Get();
				break;
			}
			case _return: {
				Get();
				break;
			}
			case _in: {
				Get();
				break;
			}
			case _to: {
				Get();
				break;
			}
			case _continue: {
				Get();
				break;
			}
			case _break: {
				Get();
				break;
			}
			case _or: {
				Get();
				break;
			}
			case _and: {
				Get();
				break;
			}
			case _xor: {
				Get();
				break;
			}
			case _goto: {
				Get();
				break;
			}
			case _null: {
				Get();
				break;
			}
			case _else: {
				Get();
				break;
			}
			case _if: {
				Get();
				break;
			}
			case _unless: {
				Get();
				break;
			}
			case _new: {
				Get();
				break;
			}
			case _while: {
				Get();
				break;
			}
			case _until: {
				Get();
				break;
			}
			case _for: {
				Get();
				break;
			}
			case _foreach: {
				Get();
				break;
			}
			case _command: {
				Get();
				break;
			}
			case _as: {
				Get();
				break;
			}
			case _try: {
				Get();
				break;
			}
			case _throw: {
				Get();
				break;
			}
			}
			/*Parser.Assembler.atg:245*/id = cache(t.val); 
		} else SynErr(109);
	}

	void Integer(/*Parser.Helper.atg:43*/out int value) {
		Expect(_integer);
		/*Parser.Helper.atg:44*/if(!TryParseInteger(t.val, out value))
		   SemErr(t, "Cannot recognize integer " + t.val);
		
	}

	void SignedReal(/*Parser.Helper.atg:75*/out double value) {
		/*Parser.Helper.atg:75*/value = 0.0; double modifier = 1.0; int ival; 
		if (la.kind == _minus || la.kind == _plus) {
			if (la.kind == _plus) {
				Get();
			} else {
				Get();
				/*Parser.Helper.atg:78*/modifier = -1.0; 
			}
		}
		if (la.kind == _real || la.kind == _realLike) {
			Real(/*Parser.Helper.atg:79*/out value);
		} else if (la.kind == _integer) {
			Integer(/*Parser.Helper.atg:80*/out ival);
			/*Parser.Helper.atg:80*/value = ival; 
		} else SynErr(110);
		/*Parser.Helper.atg:82*/value = modifier * value; 
	}

	void Boolean(/*Parser.Helper.atg:36*/out bool value) {
		/*Parser.Helper.atg:36*/value = true; 
		if (la.kind == _true) {
			Get();
		} else if (la.kind == _false) {
			Get();
			/*Parser.Helper.atg:39*/value = false; 
		} else SynErr(111);
	}

	void SignedInteger(/*Parser.Helper.atg:49*/out int value) {
		/*Parser.Helper.atg:49*/int modifier = 1; 
		if (la.kind == _minus || la.kind == _plus) {
			if (la.kind == _plus) {
				Get();
			} else {
				Get();
				/*Parser.Helper.atg:52*/modifier = -1; 
			}
		}
		Integer(/*Parser.Helper.atg:53*/out value);
		/*Parser.Helper.atg:53*/value = modifier * value; 
	}

	void AsmQualid(/*Parser.Assembler.atg:249*/out string qualid) {
		
		AsmId(/*Parser.Assembler.atg:251*/out qualid);
	}

	void String(/*Parser.Helper.atg:86*/out string value) {
		Expect(_string);
		/*Parser.Helper.atg:87*/value = cache(t.val); 
	}

	void Id(/*Parser.Helper.atg:25*/out string id) {
		/*Parser.Helper.atg:25*/id = "\\NoId\\"; 
		if (la.kind == _id || la.kind == _anyId) {
			GlobalId(/*Parser.Helper.atg:27*/out id);
		} else if (StartOf(6)) {
			if (la.kind == _enabled) {
				Get();
			} else if (la.kind == _disabled) {
				Get();
			} else if (la.kind == _build) {
				Get();
			} else if (la.kind == _add) {
				Get();
			} else {
				Get();
			}
			/*Parser.Helper.atg:33*/id = cache(t.val); 
		} else SynErr(112);
	}

	void Expr(/*Parser.Expression.atg:26*/out AstExpr expr) {
		/*Parser.Expression.atg:26*/AstConditionalExpression cexpr; expr = null; 
		if (StartOf(7)) {
			AtomicExpr(/*Parser.Expression.atg:28*/out expr);
		} else if (la.kind == _if || la.kind == _unless) {
			/*Parser.Expression.atg:29*/bool isNegated = false; 
			if (la.kind == _if) {
				Get();
			} else {
				Get();
				/*Parser.Expression.atg:31*/isNegated = true; 
			}
			Expect(_lpar);
			Expr(/*Parser.Expression.atg:33*/out expr);
			Expect(_rpar);
			/*Parser.Expression.atg:33*/cexpr = new AstConditionalExpression(this, expr, isNegated); 
			Expr(/*Parser.Expression.atg:34*/out cexpr.IfExpression);
			Expect(_else);
			Expr(/*Parser.Expression.atg:36*/out cexpr.ElseExpression);
			/*Parser.Expression.atg:36*/expr = cexpr; 
		} else SynErr(113);
	}

	void AtomicExpr(/*Parser.Expression.atg:41*/out AstExpr expr) {
		/*Parser.Expression.atg:41*/AstExpr outerExpr; 
		AppendRightExpr(/*Parser.Expression.atg:43*/out expr);
		while (la.kind == _then) {
			Get();
			AppendRightExpr(/*Parser.Expression.atg:46*/out outerExpr);
			/*Parser.Expression.atg:46*/var thenExpr = Create.Call(GetPosition(), EntityRef.Command.Create(Engine.ThenAlias));
			thenExpr.Arguments.Add(expr);
			thenExpr.Arguments.Add(outerExpr);
			expr = thenExpr;
			
		}
	}

	void AppendRightExpr(/*Parser.Expression.atg:56*/out AstExpr expr) {
		/*Parser.Expression.atg:56*/AstGetSet complex; 
		KeyValuePairExpr(/*Parser.Expression.atg:58*/out expr);
		while (la.kind == _appendright) {
			Get();
			GetCall(/*Parser.Expression.atg:61*/out complex);
			/*Parser.Expression.atg:61*/_appendRight(expr,complex);
			expr = complex;										    
			
		}
	}

	void KeyValuePairExpr(/*Parser.Expression.atg:68*/out AstExpr expr) {
		OrExpr(/*Parser.Expression.atg:69*/out expr);
		if (la.kind == _colon) {
			Get();
			/*Parser.Expression.atg:70*/AstExpr value; 
			KeyValuePairExpr(/*Parser.Expression.atg:71*/out value);
			/*Parser.Expression.atg:71*/expr = new AstKeyValuePair(this, expr, value); 
		}
	}

	void GetCall(/*Parser.Statement.atg:475*/out AstGetSet complex) {
		/*Parser.Statement.atg:475*/AstGetSet getMember = null; 
		AstExpr expr;
		
		GetInitiator(/*Parser.Statement.atg:479*/out expr);
		/*Parser.Statement.atg:480*/complex = expr as AstGetSet;
		if(complex == null)
		{
		    var pos = GetPosition();
		    Loader.ReportMessage(Message.Error("Expected an LValue (Get/Set-Complex) for ++,-- or assignment statement.",pos,MessageClasses.LValueExpected));
		    complex = Create.IndirectCall(pos,Create.Null(pos));
		}  
		
		while (la.kind == _dot || la.kind == _lbrack) {
			GetSetExtension(/*Parser.Statement.atg:488*/complex, out getMember);
			/*Parser.Statement.atg:489*/complex = getMember; 
		}
	}

	void OrExpr(/*Parser.Expression.atg:76*/out AstExpr expr) {
		/*Parser.Expression.atg:76*/AstExpr lhs, rhs; 
		AndExpr(/*Parser.Expression.atg:78*/out lhs);
		/*Parser.Expression.atg:78*/expr = lhs; 
		if (la.kind == _or) {
			Get();
			OrExpr(/*Parser.Expression.atg:79*/out rhs);
			/*Parser.Expression.atg:79*/expr = new AstLogicalOr(this, lhs, rhs); 
		}
	}

	void AndExpr(/*Parser.Expression.atg:85*/out AstExpr expr) {
		/*Parser.Expression.atg:85*/AstExpr lhs, rhs; 
		BitOrExpr(/*Parser.Expression.atg:87*/out lhs);
		/*Parser.Expression.atg:87*/expr = lhs; 
		if (la.kind == _and) {
			Get();
			AndExpr(/*Parser.Expression.atg:88*/out rhs);
			/*Parser.Expression.atg:88*/expr = new AstLogicalAnd(this, lhs, rhs); 
		}
	}

	void BitOrExpr(/*Parser.Expression.atg:93*/out AstExpr expr) {
		/*Parser.Expression.atg:93*/AstExpr lhs, rhs; 
		BitXorExpr(/*Parser.Expression.atg:95*/out lhs);
		/*Parser.Expression.atg:95*/expr = lhs; 
		while (la.kind == _bitOr) {
			Get();
			BitXorExpr(/*Parser.Expression.atg:96*/out rhs);
			/*Parser.Expression.atg:96*/expr =Create.BinaryOperation(GetPosition(), expr, BinaryOperator.BitwiseOr, rhs); 
		}
	}

	void BitXorExpr(/*Parser.Expression.atg:101*/out AstExpr expr) {
		/*Parser.Expression.atg:101*/AstExpr lhs, rhs; 
		BitAndExpr(/*Parser.Expression.atg:103*/out lhs);
		/*Parser.Expression.atg:103*/expr = lhs; 
		while (la.kind == _xor) {
			Get();
			BitAndExpr(/*Parser.Expression.atg:104*/out rhs);
			/*Parser.Expression.atg:105*/expr = Create.BinaryOperation(GetPosition(), expr, BinaryOperator.ExclusiveOr, rhs); 
		}
	}

	void BitAndExpr(/*Parser.Expression.atg:110*/out AstExpr expr) {
		/*Parser.Expression.atg:110*/AstExpr lhs, rhs; 
		NotExpr(/*Parser.Expression.atg:112*/out lhs);
		/*Parser.Expression.atg:112*/expr = lhs; 
		while (la.kind == _bitAnd) {
			Get();
			NotExpr(/*Parser.Expression.atg:113*/out rhs);
			/*Parser.Expression.atg:114*/expr = Create.BinaryOperation(GetPosition(), expr, BinaryOperator.BitwiseAnd, rhs); 
		}
	}

	void NotExpr(/*Parser.Expression.atg:119*/out AstExpr expr) {
		/*Parser.Expression.atg:119*/AstExpr lhs; bool isNot = false; 
		if (la.kind == _not) {
			Get();
			/*Parser.Expression.atg:121*/isNot = true; 
		}
		EqlExpr(/*Parser.Expression.atg:123*/out lhs);
		/*Parser.Expression.atg:123*/expr = isNot ? Create.UnaryOperation(GetPosition(), UnaryOperator.LogicalNot, lhs) : lhs; 
	}

	void EqlExpr(/*Parser.Expression.atg:127*/out AstExpr expr) {
		/*Parser.Expression.atg:127*/AstExpr lhs, rhs; BinaryOperator op; 
		RelExpr(/*Parser.Expression.atg:129*/out lhs);
		/*Parser.Expression.atg:129*/expr = lhs; 
		while (la.kind == _eq || la.kind == _ne) {
			if (la.kind == _eq) {
				Get();
				/*Parser.Expression.atg:130*/op = BinaryOperator.Equality; 
			} else {
				Get();
				/*Parser.Expression.atg:131*/op = BinaryOperator.Inequality; 
			}
			RelExpr(/*Parser.Expression.atg:132*/out rhs);
			/*Parser.Expression.atg:132*/expr = Create.BinaryOperation(GetPosition(), expr, op, rhs); 
		}
	}

	void RelExpr(/*Parser.Expression.atg:137*/out AstExpr expr) {
		/*Parser.Expression.atg:137*/AstExpr lhs, rhs; BinaryOperator op;  
		CoalExpr(/*Parser.Expression.atg:139*/out lhs);
		/*Parser.Expression.atg:139*/expr = lhs; 
		while (StartOf(8)) {
			if (la.kind == _lt) {
				Get();
				/*Parser.Expression.atg:140*/op = BinaryOperator.LessThan;              
			} else if (la.kind == _le) {
				Get();
				/*Parser.Expression.atg:141*/op = BinaryOperator.LessThanOrEqual;       
			} else if (la.kind == _gt) {
				Get();
				/*Parser.Expression.atg:142*/op = BinaryOperator.GreaterThan;           
			} else {
				Get();
				/*Parser.Expression.atg:143*/op = BinaryOperator.GreaterThanOrEqual;    
			}
			CoalExpr(/*Parser.Expression.atg:144*/out rhs);
			/*Parser.Expression.atg:144*/expr = Create.BinaryOperation(GetPosition(), expr, op, rhs); 
		}
	}

	void CoalExpr(/*Parser.Expression.atg:149*/out AstExpr expr) {
		/*Parser.Expression.atg:149*/AstExpr lhs, rhs; AstCoalescence coal = new AstCoalescence(this); 
		AddExpr(/*Parser.Expression.atg:151*/out lhs);
		/*Parser.Expression.atg:151*/expr = lhs; coal.Expressions.Add(lhs); 
		while (la.kind == _coalescence) {
			Get();
			AddExpr(/*Parser.Expression.atg:154*/out rhs);
			/*Parser.Expression.atg:154*/expr = coal; coal.Expressions.Add(rhs); 
		}
	}

	void AddExpr(/*Parser.Expression.atg:159*/out AstExpr expr) {
		/*Parser.Expression.atg:159*/AstExpr lhs,rhs; BinaryOperator op; 
		MulExpr(/*Parser.Expression.atg:161*/out lhs);
		/*Parser.Expression.atg:161*/expr = lhs; 
		while (la.kind == _minus || la.kind == _plus) {
			if (la.kind == _plus) {
				Get();
				/*Parser.Expression.atg:162*/op = BinaryOperator.Addition;      
			} else {
				Get();
				/*Parser.Expression.atg:163*/op = BinaryOperator.Subtraction;   
			}
			MulExpr(/*Parser.Expression.atg:164*/out rhs);
			/*Parser.Expression.atg:164*/expr = Create.BinaryOperation(GetPosition(), expr, op, rhs); 
		}
	}

	void MulExpr(/*Parser.Expression.atg:169*/out AstExpr expr) {
		/*Parser.Expression.atg:169*/AstExpr lhs, rhs; BinaryOperator op; 
		PowExpr(/*Parser.Expression.atg:171*/out lhs);
		/*Parser.Expression.atg:171*/expr = lhs; 
		while (la.kind == _div || la.kind == _times || la.kind == _mod) {
			if (la.kind == _times) {
				Get();
				/*Parser.Expression.atg:172*/op = BinaryOperator.Multiply;      
			} else if (la.kind == _div) {
				Get();
				/*Parser.Expression.atg:173*/op = BinaryOperator.Division;        
			} else {
				Get();
				/*Parser.Expression.atg:174*/op = BinaryOperator.Modulus;       
			}
			PowExpr(/*Parser.Expression.atg:175*/out rhs);
			/*Parser.Expression.atg:175*/expr = Create.BinaryOperation(GetPosition(), expr, op, rhs); 
		}
	}

	void PowExpr(/*Parser.Expression.atg:180*/out AstExpr expr) {
		/*Parser.Expression.atg:180*/AstExpr lhs, rhs; 
		AssignExpr(/*Parser.Expression.atg:182*/out lhs);
		/*Parser.Expression.atg:182*/expr = lhs; 
		while (la.kind == _pow) {
			Get();
			AssignExpr(/*Parser.Expression.atg:183*/out rhs);
			/*Parser.Expression.atg:183*/expr = Create.BinaryOperation(GetPosition(), expr, BinaryOperator.Power, rhs); 
		}
	}

	void AssignExpr(/*Parser.Expression.atg:187*/out AstExpr expr) {
		/*Parser.Expression.atg:187*/AstGetSet assignment; BinaryOperator setModifier = BinaryOperator.None;
		      AstTypeExpr typeExpr;
		      ISourcePosition position;
		  
		/*Parser.Expression.atg:191*/position = GetPosition(); 
		PostfixUnaryExpr(/*Parser.Expression.atg:192*/out expr);
		if (/*Parser.Expression.atg:194*/isAssignmentOperator()) {
			/*Parser.Expression.atg:194*/assignment = expr as AstGetSet;
			if(assignment == null) 
			{
			    SemErr(string.Format("Cannot assign to a {0}",
			        expr.GetType().Name));
			    assignment = _NullNode(GetPosition()); //to prevent null references
			}
			assignment.Call = PCall.Set;
			
			if (StartOf(9)) {
				switch (la.kind) {
				case _assign: {
					Get();
					/*Parser.Expression.atg:204*/setModifier = BinaryOperator.None; 
					break;
				}
				case _plus: {
					Get();
					Expect(_assign);
					/*Parser.Expression.atg:205*/setModifier = BinaryOperator.Addition; 
					break;
				}
				case _minus: {
					Get();
					Expect(_assign);
					/*Parser.Expression.atg:206*/setModifier = BinaryOperator.Subtraction; 
					break;
				}
				case _times: {
					Get();
					Expect(_assign);
					/*Parser.Expression.atg:207*/setModifier = BinaryOperator.Multiply; 
					break;
				}
				case _div: {
					Get();
					Expect(_assign);
					/*Parser.Expression.atg:208*/setModifier = BinaryOperator.Division; 
					break;
				}
				case _bitAnd: {
					Get();
					Expect(_assign);
					/*Parser.Expression.atg:209*/setModifier = BinaryOperator.BitwiseAnd; 
					break;
				}
				case _bitOr: {
					Get();
					Expect(_assign);
					/*Parser.Expression.atg:210*/setModifier = BinaryOperator.BitwiseOr; 
					break;
				}
				case _coalescence: {
					Get();
					Expect(_assign);
					/*Parser.Expression.atg:211*/setModifier = BinaryOperator.Coalescence; 
					break;
				}
				}
				Expr(/*Parser.Expression.atg:212*/out expr);
			} else if (la.kind == _tilde) {
				Get();
				Expect(_assign);
				/*Parser.Expression.atg:214*/setModifier = BinaryOperator.Cast; 
				TypeExpr(/*Parser.Expression.atg:215*/out typeExpr);
				/*Parser.Expression.atg:215*/expr = typeExpr; 
			} else SynErr(114);
			/*Parser.Expression.atg:217*/assignment.Arguments.Add(expr); 
			if(setModifier == BinaryOperator.None)
			    expr = assignment;
			else
			    expr = Create.ModifyingAssignment(position,assignment,setModifier);
			
		} else if (StartOf(10)) {
		} else SynErr(115);
	}

	void PostfixUnaryExpr(/*Parser.Expression.atg:227*/out AstExpr expr) {
		/*Parser.Expression.atg:227*/AstTypeExpr type; AstGetSet extension; bool isInverted = false; 
		PrefixUnaryExpr(/*Parser.Expression.atg:229*/out expr);
		/*Parser.Expression.atg:229*/var position = GetPosition(); 
		while (StartOf(11)) {
			if (la.kind == _tilde) {
				Get();
				TypeExpr(/*Parser.Expression.atg:230*/out type);
				/*Parser.Expression.atg:230*/expr = new AstTypecast(this, expr, type); 
			} else if (la.kind == _is) {
				Get();
				if (la.kind == _not) {
					Get();
					/*Parser.Expression.atg:232*/isInverted = true; 
				}
				TypeExpr(/*Parser.Expression.atg:234*/out type);
				/*Parser.Expression.atg:234*/expr = new AstTypecheck(this, expr, type);
				if(isInverted)
				                              {
				                                  ((AstTypecheck)expr).IsInverted = true;
					expr = Create.UnaryOperation(position, UnaryOperator.LogicalNot, expr);
				                              }
				
			} else if (la.kind == _inc) {
				Get();
				/*Parser.Expression.atg:241*/expr = Create.UnaryOperation(position, UnaryOperator.PostIncrement, expr); 
			} else if (la.kind == _dec) {
				Get();
				/*Parser.Expression.atg:242*/expr = Create.UnaryOperation(position, UnaryOperator.PostDecrement, expr); 
			} else {
				GetSetExtension(/*Parser.Expression.atg:243*/expr, out extension);
				/*Parser.Expression.atg:244*/expr = extension; 
			}
		}
	}

	void TypeExpr(/*Parser.Expression.atg:494*/out AstTypeExpr type) {
		/*Parser.Expression.atg:494*/type = null; 
		if (StartOf(12)) {
			PrexoniteTypeExpr(/*Parser.Expression.atg:496*/out type);
		} else if (la.kind == _ns || la.kind == _doublecolon) {
			ClrTypeExpr(/*Parser.Expression.atg:497*/out type);
		} else SynErr(116);
	}

	void PrefixUnaryExpr(/*Parser.Expression.atg:249*/out AstExpr expr) {
		/*Parser.Expression.atg:249*/var prefixes = new Stack<UnaryOperator>(); 
		/*Parser.Expression.atg:250*/var position = GetPosition(); 
		while (StartOf(13)) {
			if (la.kind == _plus) {
				Get();
			} else if (la.kind == _minus) {
				Get();
				/*Parser.Expression.atg:252*/prefixes.Push(UnaryOperator.UnaryNegation); 
			} else if (la.kind == _inc) {
				Get();
				/*Parser.Expression.atg:253*/prefixes.Push(UnaryOperator.PreIncrement); 
			} else {
				Get();
				/*Parser.Expression.atg:254*/prefixes.Push(UnaryOperator.PreDecrement); 
			}
		}
		Primary(/*Parser.Expression.atg:256*/out expr);
		/*Parser.Expression.atg:257*/while(prefixes.Count > 0)
		   expr = Create.UnaryOperation(position, prefixes.Pop(), expr);
		
	}

	void GetSetExtension(/*Parser.Statement.atg:128*/AstExpr subject, out AstGetSet extension) {
		/*Parser.Statement.atg:128*/extension = null; string id;
		if(subject == null)
		{
			SemErr("Member access not preceded by a proper expression.");
			subject = new AstConstant(this,null);
		}
		                             
		if (/*Parser.Statement.atg:138*/isIndirectCall() ) {
			Expect(_dot);
			/*Parser.Statement.atg:138*/extension = new AstIndirectCall(this, PCall.Get, subject); 
			Arguments(/*Parser.Statement.atg:139*/extension.Arguments);
		} else if (la.kind == _dot) {
			Get();
			Id(/*Parser.Statement.atg:141*/out id);
			/*Parser.Statement.atg:141*/var ns = subject as AstNamespaceUsage;
			if(ns == null)
			{
			    // Ordinary member access
			    extension = new AstGetSetMemberAccess(this, PCall.Get, subject, id);
			}
			else
			{
			    // Namespace lookup
			    extension = _useSymbol(ns.Namespace, id, GetPosition());
			    // write down qualified path
			    AstNamespaceUsage subns = extension as AstNamespaceUsage;
			    if(subns != null && subns.ReferencePath == null)
			    {
			        subns.ReferencePath = ns.ReferencePath + new QualifiedId(id);
			    }
			}
			
			Arguments(/*Parser.Statement.atg:159*/extension.Arguments);
		} else if (la.kind == _lbrack) {
			/*Parser.Statement.atg:161*/AstExpr expr; 
			extension = new AstGetSetMemberAccess(this, PCall.Get, subject, ""); 
			
			Get();
			if (StartOf(14)) {
				Expr(/*Parser.Statement.atg:165*/out expr);
				/*Parser.Statement.atg:165*/extension.Arguments.Add(expr); 
				while (WeakSeparator(_comma,14,15) ) {
					Expr(/*Parser.Statement.atg:166*/out expr);
					/*Parser.Statement.atg:166*/extension.Arguments.Add(expr); 
				}
			}
			Expect(_rbrack);
		} else SynErr(117);
	}

	void Primary(/*Parser.Expression.atg:263*/out AstExpr expr) {
		/*Parser.Expression.atg:263*/expr = null;
		
		if (la.kind == _asm) {
			/*Parser.Expression.atg:266*/_pushLexerState(Lexer.Asm); 
			/*Parser.Expression.atg:266*/var blockExpr = Create.Block(GetPosition());
			_PushScope(blockExpr);
			
			Get();
			Expect(_lpar);
			while (StartOf(1)) {
				AsmInstruction(/*Parser.Expression.atg:269*/blockExpr);
			}
			Expect(_rpar);
			/*Parser.Expression.atg:270*/_popLexerState(); 
			/*Parser.Expression.atg:270*/expr = blockExpr; 
			_PopScope(blockExpr);
			
		} else if (StartOf(16)) {
			Constant(/*Parser.Expression.atg:273*/out expr);
		} else if (la.kind == _this) {
			ThisExpression(/*Parser.Expression.atg:274*/out expr);
		} else if (la.kind == _coroutine) {
			CoroutineCreation(/*Parser.Expression.atg:275*/out expr);
		} else if (la.kind == _lbrack) {
			ListLiteral(/*Parser.Expression.atg:276*/out expr);
		} else if (la.kind == _lbrace) {
			HashLiteral(/*Parser.Expression.atg:277*/out expr);
		} else if (StartOf(17)) {
			LoopExpr(/*Parser.Expression.atg:278*/out expr);
		} else if (la.kind == _throw) {
			/*Parser.Expression.atg:279*/AstThrow th; 
			ThrowExpression(/*Parser.Expression.atg:280*/out th);
			/*Parser.Expression.atg:280*/expr = th; 
		} else if (/*Parser.Expression.atg:282*/isLambdaExpression()) {
			LambdaExpression(/*Parser.Expression.atg:282*/out expr);
		} else if (la.kind == _lazy) {
			LazyExpression(/*Parser.Expression.atg:283*/out expr);
		} else if (la.kind == _lpar) {
			Get();
			Expr(/*Parser.Expression.atg:284*/out expr);
			Expect(_rpar);
		} else if (/*Parser.Expression.atg:285*/_isNotNewDecl()) {
			ObjectCreation(/*Parser.Expression.atg:285*/out expr);
		} else if (StartOf(18)) {
			GetInitiator(/*Parser.Expression.atg:286*/out expr);
		} else if (la.kind == _LPopExpr) {
			Get();
			Expect(_lpar);
			Expr(/*Parser.Expression.atg:287*/out expr);
			/*Parser.Expression.atg:292*/_popLexerState(); _inject(_plus); 
			Expect(_rpar);
		} else SynErr(118);
	}

	void Constant(/*Parser.Expression.atg:297*/out AstExpr expr) {
		/*Parser.Expression.atg:297*/expr = null; int vi; double vr; bool vb; string vs; 
		if (la.kind == _integer) {
			Integer(/*Parser.Expression.atg:299*/out vi);
			/*Parser.Expression.atg:299*/expr = new AstConstant(this, vi); 
		} else if (la.kind == _real || la.kind == _realLike) {
			Real(/*Parser.Expression.atg:300*/out vr);
			/*Parser.Expression.atg:300*/expr = new AstConstant(this, vr); 
		} else if (la.kind == _true || la.kind == _false) {
			Boolean(/*Parser.Expression.atg:301*/out vb);
			/*Parser.Expression.atg:301*/expr = new AstConstant(this, vb); 
		} else if (la.kind == _string) {
			String(/*Parser.Expression.atg:302*/out vs);
			/*Parser.Expression.atg:302*/expr = new AstConstant(this, vs); 
		} else if (la.kind == _null) {
			Null();
			/*Parser.Expression.atg:303*/expr = new AstConstant(this, null); 
		} else SynErr(119);
	}

	void ThisExpression(/*Parser.Expression.atg:481*/out AstExpr expr) {
		/*Parser.Expression.atg:481*/var position = GetPosition();
		expr = Create.IndirectCall(position,Create.Null(position));
		
		Expect(_this);
		/*Parser.Expression.atg:485*/Loader.ReportMessage(Message.Error("Illegal use of reserved keyword `this`.",position,MessageClasses.ThisReserved)); 
	}

	void CoroutineCreation(/*Parser.Expression.atg:375*/out AstExpr expr) {
		/*Parser.Expression.atg:376*/AstCreateCoroutine cor = new AstCreateCoroutine(this); 
		AstExpr iexpr;
		expr = cor;
		
		Expect(_coroutine);
		Expr(/*Parser.Expression.atg:381*/out iexpr);
		/*Parser.Expression.atg:381*/cor.Expression = iexpr; 
		if (la.kind == _for) {
			Get();
			Arguments(/*Parser.Expression.atg:382*/cor.Arguments);
		}
	}

	void ListLiteral(/*Parser.Expression.atg:307*/out AstExpr expr) {
		/*Parser.Expression.atg:307*/AstExpr iexpr; 
		AstListLiteral lst = new AstListLiteral(this);
		expr = lst;
		bool missingExpr = false;
		
		Expect(_lbrack);
		if (StartOf(14)) {
			Expr(/*Parser.Expression.atg:314*/out iexpr);
			/*Parser.Expression.atg:314*/lst.Elements.Add(iexpr); 
			while (la.kind == _comma) {
				Get();
				/*Parser.Expression.atg:315*/if(missingExpr)
				   SemErr("Missing expression in list literal (two consecutive commas).");
				
				if (StartOf(14)) {
					Expr(/*Parser.Expression.atg:318*/out iexpr);
					/*Parser.Expression.atg:318*/lst.Elements.Add(iexpr); 
					missingExpr = false; 
					
				} else if (la.kind == _comma || la.kind == _rbrack) {
					/*Parser.Expression.atg:321*/missingExpr = true; 
				} else SynErr(120);
			}
		}
		Expect(_rbrack);
	}

	void HashLiteral(/*Parser.Expression.atg:329*/out AstExpr expr) {
		/*Parser.Expression.atg:329*/AstExpr iexpr; 
		AstHashLiteral hash = new AstHashLiteral(this);
		expr = hash;
		                                 bool missingExpr = false;
		
		Expect(_lbrace);
		if (StartOf(14)) {
			Expr(/*Parser.Expression.atg:336*/out iexpr);
			/*Parser.Expression.atg:336*/hash.Elements.Add(iexpr); 
			while (la.kind == _comma) {
				Get();
				/*Parser.Expression.atg:337*/if(missingExpr)
				           SemErr("Missing expression in list literal (two consecutive commas).");
				   
				if (StartOf(14)) {
					Expr(/*Parser.Expression.atg:340*/out iexpr);
					/*Parser.Expression.atg:340*/hash.Elements.Add(iexpr); 
					      missingExpr = false;
					  
				} else if (la.kind == _comma || la.kind == _rbrace) {
					/*Parser.Expression.atg:343*/missingExpr = true; 
				} else SynErr(121);
			}
		}
		Expect(_rbrace);
	}

	void LoopExpr(/*Parser.Expression.atg:351*/out AstExpr expr) {
		/*Parser.Expression.atg:351*/var dummyBlock = Create.Block(GetPosition());
		_PushScope(dummyBlock);
		expr = _NullNode(GetPosition());
		
		if (la.kind == _do || la.kind == _while || la.kind == _until) {
			WhileLoop(/*Parser.Expression.atg:356*/dummyBlock);
		} else if (la.kind == _for) {
			ForLoop(/*Parser.Expression.atg:357*/dummyBlock);
		} else if (la.kind == _foreach) {
			ForeachLoop(/*Parser.Expression.atg:358*/dummyBlock);
		} else SynErr(122);
		/*Parser.Expression.atg:359*/_PopScope(dummyBlock);
		SemErr("Loop expressions are no longer supported.");
		
	}

	void ThrowExpression(/*Parser.Expression.atg:475*/out AstThrow th) {
		/*Parser.Expression.atg:475*/th = new AstThrow(this); 
		Expect(_throw);
		Expr(/*Parser.Expression.atg:478*/out th.Expression);
	}

	void LambdaExpression(/*Parser.Expression.atg:386*/out AstExpr expr) {
		/*Parser.Expression.atg:386*/PFunction func = TargetApplication.CreateFunction(generateLocalId());                                             
		func.Meta[Application.ImportKey] = target.Function.Meta[Application.ImportKey];
		func.Meta[PFunction.ParentFunctionKey] = target.Function.Id;
		Loader.CreateFunctionTarget(func, target, GetPosition());
		CompilerTarget ft = FunctionTargets[func];
		ISourcePosition position;
		
		/*Parser.Expression.atg:394*/position = GetPosition(); 
		if (StartOf(19)) {
			FormalArg(/*Parser.Expression.atg:395*/ft);
		} else if (la.kind == _lpar) {
			Get();
			if (StartOf(19)) {
				FormalArg(/*Parser.Expression.atg:397*/ft);
				while (la.kind == _comma) {
					Get();
					FormalArg(/*Parser.Expression.atg:399*/ft);
				}
			}
			Expect(_rpar);
		} else SynErr(123);
		/*Parser.Expression.atg:404*/_PushScope(ft); 
		Expect(_implementation);
		if (la.kind == _lbrace) {
			Get();
			while (StartOf(20)) {
				Statement(/*Parser.Expression.atg:407*/ft.Ast);
			}
			Expect(_rbrace);
		} else if (StartOf(14)) {
			/*Parser.Expression.atg:409*/AstReturn ret = new AstReturn(this, ReturnVariant.Exit); 
			Expr(/*Parser.Expression.atg:410*/out ret.Expression);
			/*Parser.Expression.atg:410*/ft.Ast.Add(ret); 
		} else SynErr(124);
		/*Parser.Expression.atg:413*/_PopScope(ft);
		if(errors.count == 0)
		{
		    try {
		        //Emit code for top-level block
		        Ast[func].EmitCode(FunctionTargets[func],true,StackSemantics.Effect);
		        FunctionTargets[func].FinishTarget();
		    } catch(Exception e) {
		        SemErr("Exception during compilation of lambda expression.\n" + e);
		    }
		}
		
		expr = Create.CreateClosure(position,EntityRef.Function.Create(func.Id,func.ParentApplication.Module.Name));
		
	}

	void LazyExpression(/*Parser.Expression.atg:430*/out AstExpr expr) {
		/*Parser.Expression.atg:430*/PFunction func = TargetApplication.CreateFunction(generateLocalId());
		func.Meta[Application.ImportKey] = target.Function.Meta[Application.ImportKey];
		func.Meta[PFunction.ParentFunctionKey] = target.Function.Id;
		Loader.CreateFunctionTarget(func, target, GetPosition());
		CompilerTarget ft = FunctionTargets[func];
		ISourcePosition position;
		
		//Switch to nested target
		_PushScope(ft);
		
		Expect(_lazy);
		/*Parser.Expression.atg:441*/position = GetPosition(); 
		if (la.kind == _lbrace) {
			Get();
			while (StartOf(20)) {
				Statement(/*Parser.Expression.atg:443*/ft.Ast);
			}
			Expect(_rbrace);
		} else if (StartOf(14)) {
			/*Parser.Expression.atg:445*/AstReturn ret = new AstReturn(this, ReturnVariant.Exit); 
			Expr(/*Parser.Expression.atg:446*/out ret.Expression);
			/*Parser.Expression.atg:446*/ft.Ast.Add(ret); 
		} else SynErr(125);
		/*Parser.Expression.atg:450*/var cap = ft._ToCaptureByValue(let_bindings(ft));
		
		//Restore parent target
		_PopScope(ft);
		
		//Finish nested function
		if(errors.count == 0)
		{
		    try {
		Ast[func].EmitCode(FunctionTargets[func],true,StackSemantics.Effect);
		FunctionTargets[func].FinishTarget();
		                                   } catch(Exception e) {
		                                       SemErr("Exception during compilation of lazy expression.\n" + e);
		                                   }
		                               }
		                               
		                               //Construct expr (appears in the place of lazy expression)
		                               var clo = Create.CreateClosure(position,EntityRef.Function.Create(func.Id,func.ParentApplication.Module.Name));
		                               var thunk = Create.IndirectCall(position,Create.Reference(position,EntityRef.Command.Create(Engine.ThunkAlias)));
		                               thunk.Arguments.Add(clo);
		                               thunk.Arguments.AddRange(cap(this)); //Add captured values
		                               expr = thunk;
		                           
	}

	void ObjectCreation(/*Parser.Expression.atg:366*/out AstExpr expr) {
		/*Parser.Expression.atg:366*/AstTypeExpr type; 
		ArgumentsProxy args; 
		
		Expect(_new);
		TypeExpr(/*Parser.Expression.atg:370*/out type);
		/*Parser.Expression.atg:370*/_fallbackObjectCreation(type, out expr, out args); 
		Arguments(/*Parser.Expression.atg:371*/args);
	}

	void GetInitiator(/*Parser.Statement.atg:173*/out AstExpr complex) {
		/*Parser.Statement.atg:173*/complex = null; 
		AstGetSet actualComplex = null;
		AstGetSetStatic staticCall = null;
		AstGetSet member = null;
		AstExpr expr;
		List<AstExpr> args = new List<AstExpr>();
		string id;
		int placeholderIndex = -1;
		ISourcePosition position;
		       
		if (StartOf(21)) {
			if (StartOf(4)) {
				SymbolicUsage(/*Parser.Statement.atg:185*/out actualComplex);
				/*Parser.Statement.atg:186*/complex = actualComplex; 
			} else if (StartOf(22)) {
				VariableDeclaration(/*Parser.Statement.atg:187*/out actualComplex);
				/*Parser.Statement.atg:188*/complex = actualComplex; 
			} else if (la.kind == _ns || la.kind == _tilde || la.kind == _doublecolon) {
				StaticCall(/*Parser.Statement.atg:189*/out staticCall);
			} else {
				Get();
				Expr(/*Parser.Statement.atg:190*/out expr);
				/*Parser.Statement.atg:190*/args.Add(expr); 
				while (la.kind == _comma) {
					Get();
					Expr(/*Parser.Statement.atg:191*/out expr);
					/*Parser.Statement.atg:191*/args.Add(expr); 
				}
				Expect(_rpar);
				if (la.kind == _dot || la.kind == _lbrack) {
					GetSetExtension(/*Parser.Statement.atg:194*/expr, out member);
					/*Parser.Statement.atg:195*/if(args.Count > 1)
					SemErr("A member access cannot have multiple subjects. (Did you mean '>>'?)");
					
				} else if (la.kind == _appendright) {
					Get();
					GetCall(/*Parser.Statement.atg:199*/out actualComplex);
					/*Parser.Statement.atg:199*/_appendRight(args,actualComplex);
					        complex = actualComplex;
					
				} else SynErr(126);
			}
			/*Parser.Statement.atg:204*/complex =  
			staticCall ?? 
			member ??
			complex; 
			
		} else if (la.kind == _pointer) {
			Get();
			/*Parser.Statement.atg:210*/var ptrCount = 1; 
			while (la.kind == _pointer) {
				Get();
				/*Parser.Statement.atg:211*/ptrCount++; 
			}
			Id(/*Parser.Statement.atg:213*/out id);
			/*Parser.Statement.atg:213*/complex = _assembleReference(id, ptrCount); 
		} else if (la.kind == _question) {
			Get();
			if (la.kind == _integer) {
				Integer(/*Parser.Statement.atg:215*/out placeholderIndex);
			}
			/*Parser.Statement.atg:215*/complex = new AstPlaceholder(this, 0 <= placeholderIndex ? (int?)placeholderIndex : null); 
		} else SynErr(127);
	}

	void Real(/*Parser.Helper.atg:57*/out double value) {
		if (la.kind == _real) {
			Get();
		} else if (la.kind == _realLike) {
			Get();
		} else SynErr(128);
		/*Parser.Helper.atg:58*/string real = t.val;
		if(!TryParseReal(real, out value))
		    SemErr(t, "Cannot recognize real " + real);
		
	}

	void Null() {
		Expect(_null);
	}

	void WhileLoop(/*Parser.Statement.atg:402*/AstBlock block) {
		/*Parser.Statement.atg:402*/AstWhileLoop loop = new AstWhileLoop(GetPosition(),CurrentBlock); 
		if (la.kind == _while || la.kind == _until) {
			if (la.kind == _while) {
				Get();
			} else {
				Get();
				/*Parser.Statement.atg:404*/loop.IsPositive = false; 
			}
			Expect(_lpar);
			Expr(/*Parser.Statement.atg:406*/out loop.Condition);
			Expect(_rpar);
			/*Parser.Statement.atg:407*/_PushScope(loop.Block); //EndBlock is common for both loops
			
			StatementBlock(/*Parser.Statement.atg:409*/loop.Block);
		} else if (la.kind == _do) {
			Get();
			/*Parser.Statement.atg:411*/_PushScope(loop.Block); 
			loop.IsPrecondition = false;
			
			StatementBlock(/*Parser.Statement.atg:414*/loop.Block);
			if (la.kind == _while) {
				Get();
			} else if (la.kind == _until) {
				Get();
				/*Parser.Statement.atg:415*/loop.IsPositive = false; 
			} else SynErr(129);
			Expect(_lpar);
			Expr(/*Parser.Statement.atg:417*/out loop.Condition);
			Expect(_rpar);
		} else SynErr(130);
		/*Parser.Statement.atg:418*/_PopScope(loop.Block); block.Add(loop); 
	}

	void ForLoop(/*Parser.Statement.atg:421*/AstBlock block) {
		/*Parser.Statement.atg:421*/AstForLoop loop;
		AstExpr condition;
		                       
		Expect(_for);
		/*Parser.Statement.atg:425*/loop = new AstForLoop(GetPosition(), CurrentBlock); 
		_PushScope(loop.Initialize);
		
		Expect(_lpar);
		StatementBlock(/*Parser.Statement.atg:428*/loop.Initialize);
		if (la.kind == _do) {
			/*Parser.Statement.atg:430*/_PushScope(loop.NextIteration); 
			
			Get();
			StatementBlock(/*Parser.Statement.atg:432*/loop.NextIteration);
			/*Parser.Statement.atg:433*/loop.IsPrecondition = false; 
			if (la.kind == _while) {
				Get();
			} else if (la.kind == _until) {
				Get();
				/*Parser.Statement.atg:435*/loop.IsPositive = false; 
			} else SynErr(131);
			/*Parser.Statement.atg:436*/_PopScope(loop.NextIteration); 
			Expr(/*Parser.Statement.atg:437*/out condition);
			/*Parser.Statement.atg:437*/loop.Condition = condition; 
			_PushScope(loop.NextIteration);
			
		} else if (StartOf(14)) {
			if (la.kind == _while || la.kind == _until) {
				if (la.kind == _while) {
					Get();
				} else {
					Get();
					/*Parser.Statement.atg:441*/loop.IsPositive = false; 
				}
			}
			Expr(/*Parser.Statement.atg:443*/out condition);
			/*Parser.Statement.atg:443*/loop.Condition = condition; 
			Expect(_semicolon);
			/*Parser.Statement.atg:444*/_PushScope(loop.NextIteration); 
			SimpleStatement(/*Parser.Statement.atg:445*/loop.NextIteration);
			if (la.kind == _semicolon) {
				Get();
			}
		} else SynErr(132);
		Expect(_rpar);
		/*Parser.Statement.atg:448*/_PushScope(loop.Block); 
		StatementBlock(/*Parser.Statement.atg:449*/loop.Block);
		/*Parser.Statement.atg:449*/_PopScope(loop.Block);
		_PopScope(loop.NextIteration);
		_PopScope(loop.Initialize);
		block.Add(loop);
		
	}

	void ForeachLoop(/*Parser.Statement.atg:457*/AstBlock block) {
		Expect(_foreach);
		/*Parser.Statement.atg:458*/AstForeachLoop loop = Create.ForeachLoop(GetPosition());
		_PushScope(loop.Block);
		
		Expect(_lpar);
		GetCall(/*Parser.Statement.atg:462*/out loop.Element);
		Expect(_in);
		Expr(/*Parser.Statement.atg:464*/out loop.List);
		Expect(_rpar);
		StatementBlock(/*Parser.Statement.atg:466*/loop.Block);
		/*Parser.Statement.atg:467*/_PopScope(loop.Block);
		block.Add(loop); 
		
	}

	void Arguments(/*Parser.Statement.atg:668*/ArgumentsProxy args) {
		/*Parser.Statement.atg:669*/AstExpr expr;
		                          bool missingArg = false;
		                      
		if (la.kind == _lpar) {
			Get();
			if (StartOf(14)) {
				Expr(/*Parser.Statement.atg:675*/out expr);
				/*Parser.Statement.atg:675*/args.Add(expr); 
				while (la.kind == _comma) {
					Get();
					/*Parser.Statement.atg:676*/if(missingArg)
					              SemErr("Missing argument expression (two consecutive commas)");
					      
					if (StartOf(14)) {
						Expr(/*Parser.Statement.atg:679*/out expr);
						/*Parser.Statement.atg:680*/args.Add(expr);
						missingArg = false;
						
					} else if (la.kind == _comma || la.kind == _rpar) {
						/*Parser.Statement.atg:683*/missingArg = true; 
					} else SynErr(133);
				}
			}
			Expect(_rpar);
		}
		/*Parser.Statement.atg:689*/args.RememberRightAppendPosition(); 
		if (la.kind == _appendleft) {
			Get();
			if (/*Parser.Statement.atg:694*/la.kind == _lpar && (!isLambdaExpression())) {
				Expect(_lpar);
				if (StartOf(14)) {
					Expr(/*Parser.Statement.atg:695*/out expr);
					/*Parser.Statement.atg:695*/args.Add(expr); 
					while (la.kind == _comma) {
						Get();
						Expr(/*Parser.Statement.atg:697*/out expr);
						/*Parser.Statement.atg:698*/args.Add(expr); 
					}
				}
				Expect(_rpar);
			} else if (StartOf(14)) {
				Expr(/*Parser.Statement.atg:702*/out expr);
				/*Parser.Statement.atg:702*/args.Add(expr); 
			} else SynErr(134);
		}
	}

	void FormalArg(/*Parser.GlobalScope.atg:847*/CompilerTarget ft) {
		/*Parser.GlobalScope.atg:847*/string id; SymbolInterpretations kind = SymbolInterpretations.LocalObjectVariable; 
		if (la.kind == _var || la.kind == _ref) {
			if (la.kind == _var) {
				Get();
			} else {
				Get();
				/*Parser.GlobalScope.atg:849*/kind = SymbolInterpretations.LocalReferenceVariable; 
			}
		}
		Id(/*Parser.GlobalScope.atg:851*/out id);
		/*Parser.GlobalScope.atg:854*/ft.Function.Parameters.Add(id); 
		ft.Symbols.Declare(id, new SymbolEntry(kind, id, null).ToSymbol());
		
	}

	void Statement(/*Parser.Statement.atg:31*/AstBlock block) {
		if (/*Parser.Statement.atg:34*/isLabel() ) {
			ExplicitLabel(/*Parser.Statement.atg:34*/block);
		} else if (StartOf(23)) {
			if (StartOf(24)) {
				SimpleStatement(/*Parser.Statement.atg:35*/block);
			}
			Expect(_semicolon);
		} else if (StartOf(25)) {
			StructureStatement(/*Parser.Statement.atg:36*/block);
		} else SynErr(135);
		while (la.kind == _and) {
			Get();
			Statement(/*Parser.Statement.atg:38*/block);
		}
	}

	void ExplicitTypeExpr(/*Parser.Expression.atg:488*/out AstTypeExpr type) {
		/*Parser.Expression.atg:488*/type = null; 
		if (la.kind == _tilde) {
			Get();
			PrexoniteTypeExpr(/*Parser.Expression.atg:490*/out type);
		} else if (la.kind == _ns || la.kind == _doublecolon) {
			ClrTypeExpr(/*Parser.Expression.atg:491*/out type);
		} else SynErr(136);
	}

	void PrexoniteTypeExpr(/*Parser.Expression.atg:516*/out AstTypeExpr type) {
		/*Parser.Expression.atg:516*/string id = null; 
		if (StartOf(4)) {
			Id(/*Parser.Expression.atg:518*/out id);
		} else if (la.kind == _null) {
			Get();
			/*Parser.Expression.atg:518*/id = NullPType.Literal; 
		} else SynErr(137);
		/*Parser.Expression.atg:520*/AstDynamicTypeExpression dType = new AstDynamicTypeExpression(this, id); 
		if (la.kind == _lt) {
			Get();
			if (StartOf(26)) {
				TypeExprElement(/*Parser.Expression.atg:522*/dType.Arguments);
				while (la.kind == _comma) {
					Get();
					TypeExprElement(/*Parser.Expression.atg:523*/dType.Arguments);
				}
			}
			Expect(_gt);
		}
		/*Parser.Expression.atg:527*/type = dType; 
	}

	void ClrTypeExpr(/*Parser.Expression.atg:501*/out AstTypeExpr type) {
		/*Parser.Expression.atg:501*/string id; 
		/*Parser.Expression.atg:503*/StringBuilder typeId = new StringBuilder(); 
		if (la.kind == _doublecolon) {
			Get();
		} else if (la.kind == _ns) {
			Get();
			/*Parser.Expression.atg:505*/typeId.Append(t.val); typeId.Append('.'); 
		} else SynErr(138);
		while (la.kind == _ns) {
			Get();
			/*Parser.Expression.atg:507*/typeId.Append(t.val); typeId.Append('.'); 
		}
		Id(/*Parser.Expression.atg:509*/out id);
		/*Parser.Expression.atg:509*/typeId.Append(id);
		type = new AstConstantTypeExpression(this, 
		    "Object(\"" + StringPType.Escape(typeId.ToString()) + "\")");
		
	}

	void TypeExprElement(/*Parser.Expression.atg:531*/List<AstExpr> args ) {
		/*Parser.Expression.atg:531*/AstExpr expr; AstTypeExpr type; 
		if (StartOf(16)) {
			Constant(/*Parser.Expression.atg:533*/out expr);
			/*Parser.Expression.atg:533*/args.Add(expr); 
		} else if (la.kind == _ns || la.kind == _tilde || la.kind == _doublecolon) {
			ExplicitTypeExpr(/*Parser.Expression.atg:534*/out type);
			/*Parser.Expression.atg:534*/args.Add(type); 
		} else if (la.kind == _lpar) {
			Get();
			Expr(/*Parser.Expression.atg:535*/out expr);
			Expect(_rpar);
			/*Parser.Expression.atg:535*/args.Add(expr); 
		} else SynErr(139);
	}

	void Prexonite() {
		/*Parser.GlobalScope.atg:26*/PFunction func; 
		DeclarationLevel();
		Expect(_EOF);
	}

	void DeclarationLevel() {
		/*Parser.GlobalScope.atg:33*/PFunction func; 
		while (StartOf(27)) {
			if (StartOf(28)) {
				if (StartOf(29)) {
					if (la.kind == _var || la.kind == _ref) {
						/*Parser.GlobalScope.atg:35*/if(PreflightModeEnabled)
						{
						    ViolentlyAbortParse();
						    return;
						} 
						
						GlobalVariableDefinition();
					} else {
						MetaAssignment(/*Parser.GlobalScope.atg:42*/TargetApplication);
					}
				}
				while (!(la.kind == _EOF || la.kind == _semicolon)) {SynErr(140); Get();}
				Expect(_semicolon);
			} else {
				/*Parser.GlobalScope.atg:44*/if(PreflightModeEnabled)
				{
				    ViolentlyAbortParse();
				    return;
				} 
				
				if (la.kind == _declare) {
					Declaration2();
				} else if (la.kind == _lbrace) {
					GlobalCode();
				} else if (la.kind == _build) {
					BuildBlock();
				} else if (StartOf(30)) {
					FunctionDefinition(/*Parser.GlobalScope.atg:53*/out func);
				} else if (la.kind == _namespace) {
					NamespaceDeclaration();
				} else SynErr(141);
			}
		}
	}

	void GlobalVariableDefinition() {
		/*Parser.GlobalScope.atg:131*/string id = null; 
		List<string> aliases = new List<string>();
		string primaryAlias = null;
		VariableDeclaration vari; 
		SymbolInterpretations type = SymbolInterpretations.GlobalObjectVariable; 
		SymbolEntry entry;
		
		if (la.kind == _var) {
			Get();
		} else if (la.kind == _ref) {
			Get();
			/*Parser.GlobalScope.atg:141*/type = SymbolInterpretations.GlobalReferenceVariable; 
		} else SynErr(142);
		/*Parser.GlobalScope.atg:142*/var position = GetPosition(); 
		if (la.kind == _id || la.kind == _anyId) {
			GlobalId(/*Parser.GlobalScope.atg:144*/out id);
			/*Parser.GlobalScope.atg:144*/primaryAlias = id; 
			if (la.kind == _as) {
				GlobalVariableAliasList(/*Parser.GlobalScope.atg:145*/aliases);
			}
		} else if (la.kind == _as) {
			GlobalVariableAliasList(/*Parser.GlobalScope.atg:146*/aliases);
			/*Parser.GlobalScope.atg:147*/id = Engine.GenerateName("v"); 
		} else SynErr(143);
		/*Parser.GlobalScope.atg:150*/entry = new SymbolEntry(type,id, TargetModule.Name);
		foreach(var alias in aliases)
		    Symbols.Declare(alias, entry.ToSymbol());
		  DefineGlobalVariable(id,out vari);
		
		if (la.kind == _lbrack) {
			Get();
			if (StartOf(31)) {
				MetaAssignment(/*Parser.GlobalScope.atg:156*/vari);
				while (la.kind == _semicolon) {
					Get();
					if (StartOf(31)) {
						MetaAssignment(/*Parser.GlobalScope.atg:158*/vari);
					}
				}
			}
			Expect(_rbrack);
		}
		/*Parser.GlobalScope.atg:162*/if(primaryAlias != null && !_suppressPrimarySymbol(vari))
		     Symbols.Declare(primaryAlias, entry.ToSymbol());
		
		if (la.kind == _assign) {
			/*Parser.GlobalScope.atg:165*/_pushLexerState(Lexer.Local); 
			Get();
			/*Parser.GlobalScope.atg:166*/_PushScope(FunctionTargets[Application.InitializationId]);
			AstExpr expr;
			
			Expr(/*Parser.GlobalScope.atg:169*/out expr);
			/*Parser.GlobalScope.atg:170*/_popLexerState();
			if(errors.count == 0)
			{
			  var complex = Create.Call(position, EntityRef.Variable.Global.Create(id,TargetModule.Name) );
			  complex.Call = PCall.Set;
			  complex.Arguments.Add(expr);
			  target.Ast.Add(complex);
			  TargetApplication._RequireInitialization();
			  Loader._EmitPartialInitializationCode();
			}
			_PopScope(FunctionTargets[Application.InitializationId]);
			
		}
	}

	void MetaAssignment(/*Parser.GlobalScope.atg:61*/IHasMetaTable metaTable) {
		/*Parser.GlobalScope.atg:61*/string key = null; MetaEntry entry = null; 
		if (la.kind == _is) {
			Get();
			/*Parser.GlobalScope.atg:63*/entry = true; 
			if (la.kind == _not) {
				Get();
				/*Parser.GlobalScope.atg:64*/entry = false; 
			}
			GlobalId(/*Parser.GlobalScope.atg:66*/out key);
		} else if (la.kind == _not) {
			Get();
			/*Parser.GlobalScope.atg:67*/entry = false; 
			GlobalId(/*Parser.GlobalScope.atg:68*/out key);
		} else if (la.kind == _id || la.kind == _anyId) {
			GlobalId(/*Parser.GlobalScope.atg:70*/out key);
			if (la.kind == _enabled) {
				Get();
				/*Parser.GlobalScope.atg:71*/entry = true; 
			} else if (la.kind == _disabled) {
				Get();
				/*Parser.GlobalScope.atg:72*/entry = false; 
			} else if (StartOf(32)) {
				MetaExpr(/*Parser.GlobalScope.atg:73*/out entry);
			} else if (la.kind == _rbrack || la.kind == _semicolon) {
				/*Parser.GlobalScope.atg:74*/entry = true; 
			} else SynErr(144);
		} else if (la.kind == _add) {
			Get();
			/*Parser.GlobalScope.atg:76*/MetaEntry subEntry; 
			MetaExpr(/*Parser.GlobalScope.atg:77*/out subEntry);
			/*Parser.GlobalScope.atg:77*/if(!subEntry.IsList) subEntry = (MetaEntry) subEntry.List; 
			Expect(_to);
			GlobalId(/*Parser.GlobalScope.atg:79*/out key);
			/*Parser.GlobalScope.atg:79*/if(metaTable.Meta.ContainsKey(key))
			{
			    entry = metaTable.Meta[key];
			    entry = entry.AddToList(subEntry.List);
			}
			else
			{
			   entry = subEntry;
			}
			
		} else SynErr(145);
		/*Parser.GlobalScope.atg:89*/if(entry == null || key == null) 
		                        SemErr("Meta assignment did not generate an entry.");
		                   else 
		                        metaTable.Meta[key] = entry; 
		                
	}

	void Declaration2() {
		/*Parser.GlobalScope.atg:226*/ModuleName module = TargetModule.Name;
		SymbolBuilder builder = new SymbolBuilder();
		Func<string,ModuleName,EntityRef> entityFactory;
		bool canBeRef = false;
		
		while (!(la.kind == _EOF || la.kind == _declare)) {SynErr(146); Get();}
		Expect(_declare);
		if (StartOf(33)) {
			while (la.kind == _pointer || la.kind == _ref) {
				SymbolPrefix(/*Parser.GlobalScope.atg:234*/builder, out canBeRef);
			}
			EntityFactory(/*Parser.GlobalScope.atg:235*/canBeRef, out entityFactory);
			/*Parser.GlobalScope.atg:236*/if(entityFactory == null) builder.AutoDereferenceEnabled = false; 
			if (la.kind == _colon) {
				Get();
			} else if (StartOf(34)) {
				/*Parser.GlobalScope.atg:238*/Loader.ReportMessage(Message.Warning(
				 Resources.Parser_DeclarationTypeShouldBeFollowedByColon,
				 GetPosition(),
				 MessageClasses.MissingColonInDeclaration));
				
			} else SynErr(147);
			DeclarationInstance2(/*Parser.GlobalScope.atg:244*/entityFactory,module,builder.Clone(),preventOverride:false);
			while (la.kind == _comma) {
				Get();
				if (StartOf(34)) {
					DeclarationInstance2(/*Parser.GlobalScope.atg:245*/entityFactory,module,builder.Clone(),preventOverride:false);
				}
			}
			Expect(_semicolon);
		} else if (la.kind == _lbrace) {
			Get();
			if (la.kind == _uusing) {
				Get();
				ModuleName(/*Parser.GlobalScope.atg:248*/out module);
			}
			while (StartOf(35)) {
				/*Parser.GlobalScope.atg:249*/SymbolBuilder runBuilder = builder.Clone(); 
				while (la.kind == _pointer || la.kind == _ref) {
					SymbolPrefix(/*Parser.GlobalScope.atg:250*/builder, out canBeRef);
				}
				EntityFactory(/*Parser.GlobalScope.atg:251*/canBeRef, out entityFactory);
				/*Parser.GlobalScope.atg:252*/if(entityFactory == null) runBuilder.AutoDereferenceEnabled = false; 
				Expect(_colon);
				DeclarationInstance2(/*Parser.GlobalScope.atg:254*/entityFactory,module,runBuilder.Clone(),preventOverride:true);
				while (la.kind == _comma) {
					Get();
					if (StartOf(34)) {
						DeclarationInstance2(/*Parser.GlobalScope.atg:255*/entityFactory,module,runBuilder.Clone(),preventOverride:true);
					}
				}
			}
			Expect(_rbrace);
		} else if (la.kind == _lpar) {
			Get();
			/*Parser.GlobalScope.atg:258*/bool wasComma = false; 
			if (StartOf(4)) {
				MExprBasedDeclaration();
				while (la.kind == _comma) {
					Get();
					/*Parser.GlobalScope.atg:260*/if(wasComma)
					{
					    Loader.ReportMessage(Message.Error("Double comma in declaration sequence.",GetPosition(),MessageClasses.DuplicateComma));
					}
					wasComma = true;
					
					if (StartOf(4)) {
						MExprBasedDeclaration();
						/*Parser.GlobalScope.atg:267*/wasComma = false; 
					}
				}
			}
			Expect(_rpar);
			if (la.kind == _semicolon) {
				Get();
			}
		} else SynErr(148);
	}

	void GlobalCode() {
		/*Parser.GlobalScope.atg:466*/PFunction func = TargetApplication._InitializationFunction;
		   CompilerTarget ft = FunctionTargets[func];
		   ISourcePosition position;
		   if(ft == null)
		       throw new PrexoniteException("Internal compilation error: InitializeFunction got lost.");
		
		/*Parser.GlobalScope.atg:473*/_PushScope(ft); 
		_pushLexerState(Lexer.Local);
		
		Expect(_lbrace);
		/*Parser.GlobalScope.atg:476*/position = GetPosition(); 
		while (StartOf(20)) {
			Statement(/*Parser.GlobalScope.atg:477*/target.Ast);
		}
		Expect(_rbrace);
		/*Parser.GlobalScope.atg:480*/try {
		  if(errors.count == 0)
		  {
		    TargetApplication._RequireInitialization();
		    Loader._EmitPartialInitializationCode();
		  }
		                  } catch(Exception e) {
		                      Loader.ReportMessage(Message.Error(
		                        "Exception during compilation of initialization code.\n" + e,
		                        position,
		                        MessageClasses.ExceptionDuringCompilation));
		                  } finally {
		  //Symbols defined in this block are not available to further global code blocks
		  target.Symbols.ClearLocalDeclarations();
		  _PopScope(ft);
		  _popLexerState();
		}
		
	}

	void BuildBlock() {
		while (!(la.kind == _EOF || la.kind == _build)) {SynErr(149); Get();}
		Expect(_build);
		/*Parser.GlobalScope.atg:447*/PFunction func = TargetApplication.CreateFunction();
		CompilerTarget buildBlockTarget = 
		  Loader.CreateFunctionTarget(func, sourcePosition: GetPosition());
		_PushScope(buildBlockTarget);
		Loader.DeclareBuildBlockCommands(target);
		_pushLexerState(Lexer.Local);                                
		
		if (la.kind == _does) {
			Get();
		}
		StatementBlock(/*Parser.GlobalScope.atg:456*/target.Ast);
		/*Parser.GlobalScope.atg:458*/_popLexerState();                                    
		 _PopScope(buildBlockTarget);
		 _compileAndExecuteBuildBlock(buildBlockTarget);
		
	}

	void FunctionDefinition(/*Parser.GlobalScope.atg:513*/out PFunction func) {
		/*Parser.GlobalScope.atg:514*/string primaryAlias = null;
		List<string> funcAliases = new List<string>();
		string id = null; //The logical id (given in the source code)
		string funcId; //The "physical" function id
		bool isNested = target != null; 
		bool isCoroutine = false;
		bool isMacro = false;
		bool isLazy = false;
		PFunction derBody = null; //The derived (coroutine/lazy) body function (carries a different name)
		PFunction derStub = null; //The derived (coroutine/lazy) stub function (carries the name(s) specified)
		string derId; //The name of the derived stub
		CompilerTarget ct = null;   //The compiler target for the function (as mentioned in the source code)
		CompilerTarget cst = null;  //The compiler target for a stub (coroutine/lazy)
		Symbol symEntry = null;
		ISourcePosition position;
		bool missingArg = false; //Allow trailing comma, but not (,,) in formal arg list
		
		if (la.kind == _lazy) {
			Get();
			if (la.kind == _function) {
				Get();
			}
			/*Parser.GlobalScope.atg:532*/isLazy = true; 
		} else if (la.kind == _function) {
			Get();
		} else if (la.kind == _coroutine) {
			Get();
			/*Parser.GlobalScope.atg:534*/isCoroutine = true; 
		} else if (la.kind == _macro) {
			Get();
			if (la.kind == _function) {
				Get();
			}
			/*Parser.GlobalScope.atg:535*/isMacro = true; 
		} else SynErr(150);
		/*Parser.GlobalScope.atg:536*/position = GetPosition(); 
		if (StartOf(4)) {
			Id(/*Parser.GlobalScope.atg:537*/out id);
			/*Parser.GlobalScope.atg:537*/primaryAlias = id; 
			if (la.kind == _as) {
				FunctionAliasList(/*Parser.GlobalScope.atg:538*/funcAliases);
			}
		} else if (la.kind == _as) {
			FunctionAliasList(/*Parser.GlobalScope.atg:539*/funcAliases);
		} else SynErr(151);
		/*Parser.GlobalScope.atg:541*/funcId = _assignPhysicalFunctionSlot(id);
		 if(Engine.StringsAreEqual(id, @"\init")) //Treat "\init" specially (that's the initialization code)
		 {
		     func = TargetApplication._InitializationFunction;
		     if(isNested)
		         Loader.ReportMessage(Message.Error("Cannot define initialization code inside another function.",GetPosition(),MessageClasses.IllegalInitializationFunction));
		     if(isCoroutine)
		         Loader.ReportMessage(Message.Error("Cannot define initialization code as a coroutine.",GetPosition(),MessageClasses.IllegalInitializationFunction));
		     if(isLazy)
		         Loader.ReportMessage(Message.Error("Cannot define initialization code as a lazy function.",GetPosition(),MessageClasses.IllegalInitializationFunction));
		     if(isMacro)
		         Loader.ReportMessage(Message.Error("Cannot define initialization code as a macro function.",GetPosition(),MessageClasses.IllegalInitializationFunction));
		 }
		 else
		 {
		     var localId = id;
		     
		     if(isNested)
		     {
		         if(isMacro)
		             Loader.ReportMessage(Message.Error("Inner macros are illegal. Macros must be top-level.",GetPosition(),MessageClasses.InnerMacrosIllegal));
		             
		         funcId = generateLocalId(funcId ?? "inner");
		         
		         if(string.IsNullOrEmpty(localId))
		         {
		             //Create shadow name
		             localId = generateLocalId(id ?? "inner");
		         }
		         SmartDeclareLocal(localId, SymbolInterpretations.LocalReferenceVariable);
		         foreach(var alias in funcAliases)
		                 SmartDeclareLocal(alias, localId, SymbolInterpretations.LocalReferenceVariable, false);
		         
		     }
		     
		     //Add function to application
		     if(TargetApplication.Functions.Contains(funcId) && !TargetApplication.Meta.GetDefault(Application.AllowOverridingKey,true))
		SemErr(t,"Application " + TargetApplication.Id + " does not allow overriding of function " + funcId + ".");
		                    TargetApplication.Functions.Remove(funcId);
		
		                                            func = TargetApplication.CreateFunction(funcId);
		                                            
		                                            if(isNested)
		                                            {
		                                                 func.Meta[PFunction.LogicalIdKey] = localId;
		                                                 if(isLazy)
		                                                    mark_as_let(target.Function,localId);
		                                            }
		                                            
		                                            Loader.CreateFunctionTarget(func, target, GetPosition());
		                                        }
		                                        CompilerTarget ft = FunctionTargets[func];
		                                        
		                                        //Generate derived stub
		                                        if(isCoroutine || isLazy)
		                                        {
		                                            derStub = func;
		                                            
		                                            //Create derived body function
		                                            derId = ft.GenerateLocalId();
		                                            derBody = TargetApplication.CreateFunction(derId);
		                                            Loader.CreateFunctionTarget(derBody, ft, GetPosition());
		                                            derBody.Meta[PFunction.LogicalIdKey] = id ?? funcId;
		                                            if(isCoroutine)
		                                            {
		                                                derBody.Meta[PFunction.VolatileKey] = true;
		                                                derBody.Meta[PFunction.DeficiencyKey] = "Coroutine body can only be executed by VM anyway.";
		                                                derBody.Meta[Coroutine.IsCoroutineKey] = true;
		                                            }
		
		                                            //Swap compiler target references
		                                            // -> Compile source code into derived body
		                                            // -> Let derived stub have the physical function id
		                                            ct = FunctionTargets[derBody];
		                                            cst = ft;
		                                        }
		                                        
		                                        if(isNested) //Link to parent in case of a nested function
		                                        {                                           
		                                            if(isLazy)
		                                                ft = ct;
		                                        }	                                    
		                                  
		if (StartOf(36)) {
			if (la.kind == _lpar) {
				Get();
				if (StartOf(19)) {
					FormalArg(/*Parser.GlobalScope.atg:625*/ft);
					while (la.kind == _comma) {
						Get();
						/*Parser.GlobalScope.atg:626*/if(missingArg)
						     {
						         SemErr("Missing formal argument (two consecutive commas).");
						     } 
						 
						if (StartOf(19)) {
							FormalArg(/*Parser.GlobalScope.atg:631*/ft);
							/*Parser.GlobalScope.atg:631*/missingArg = false; 
						} else if (la.kind == _comma || la.kind == _rpar) {
							/*Parser.GlobalScope.atg:632*/missingArg = true; 
						} else SynErr(152);
					}
				}
				Expect(_rpar);
			} else {
				FormalArg(/*Parser.GlobalScope.atg:637*/ft);
				while (StartOf(37)) {
					if (la.kind == _comma) {
						Get();
					}
					FormalArg(/*Parser.GlobalScope.atg:639*/ft);
				}
			}
		}
		/*Parser.GlobalScope.atg:641*/if(isNested && isLazy) // keep this assignment for maintainability
		// ReSharper disable RedundantAssignment
		    ft = cst;
		// ReSharper restore RedundantAssignment
		  
		  if(target == null && 
		      (!object.ReferenceEquals(func, TargetApplication._InitializationFunction)) &&
		      (!isNested))
		  {
		          //Add the name to the symbol table
		          symEntry = Symbol.CreateReference(EntityRef.Function.Create(func.Id, TargetModule.Name),GetPosition());
		          if(isMacro)
		            symEntry = Symbol.CreateExpand(symEntry);
		          else
		            symEntry = Symbol.CreateDereference(symEntry);
		
		                                              foreach(var alias in funcAliases)	                                                
		                                                  Symbols.Declare(alias, symEntry);
		                                              
		                                              //Store the original (logical id, mentioned in the source code)
		                                              func.Meta[PFunction.LogicalIdKey] = id ?? funcId;
		                                      }
		                                        else
		                                        {
		                                            primaryAlias = null;
		                                        }
		                                      
		                                      //Target the derived (coroutine/lazy) body instead of the stub
		                                        if(isCoroutine || isLazy)
		                                            func = derBody;
		                                  
		if (la.kind == _lbrack) {
			/*Parser.GlobalScope.atg:672*/_pushLexerState(Lexer.YYINITIAL); 
			Get();
			if (StartOf(31)) {
				MetaAssignment(/*Parser.GlobalScope.atg:674*/func);
				while (la.kind == _semicolon) {
					Get();
					if (StartOf(31)) {
						MetaAssignment(/*Parser.GlobalScope.atg:676*/func);
					}
				}
			}
			/*Parser.GlobalScope.atg:679*/_popLexerState(); 
			Expect(_rbrack);
		}
		/*Parser.GlobalScope.atg:684*/if(primaryAlias != null && !_suppressPrimarySymbol(func))
		   Symbols.Declare(primaryAlias, symEntry);
		
		                                        //Imprint certain meta keys from parent function
		                                        if(isNested)
		                                        {
		                                            func.Meta[Application.ImportKey] = target.Function.Meta[Application.ImportKey];
		                                        }
		
		                                        //Copy stub parameters to body of lazy function
		                                        if(isLazy && !isNested)
		                                      {
		                                          foreach(var kvp in cst.Symbols.LocalDeclarations)
		                                          {
		                                              var paramId = kvp.Key;
		                                              var s = kvp.Value.ToSymbolEntry();
		                                              //Lazy functions cannot have ref parameters
		                                              if(s.Interpretation != SymbolInterpretations.LocalObjectVariable)
		                                                  SemErr("Lazy functions can only have value parameters (ref is not allowed)");
		                                              ct.Function.Parameters.Add(s.InternalId);
		                                              ct.Symbols.Declare(paramId, kvp.Value);
		                                          }
		                                      }
		                                    
		                    if(isLazy || isCoroutine)
		                    {
		                      //Push the stub, because it is the lexical parent of the body
		                      _PushScope(cst);
		                    }
		                                        _PushScope(FunctionTargets[func]);
		                                        Debug.Assert(target != null); // Mostly to tell ReSharper that target is not null.
		                                        _pushLexerState(Lexer.Local);
		                                        if(isMacro)
		                                            target.SetupAsMacro();
		                                    
		if (StartOf(38)) {
			if (la.kind == _does) {
				Get();
			}
			StatementBlock(/*Parser.GlobalScope.atg:720*/target.Ast);
		} else if (/*Parser.GlobalScope.atg:722*/isFollowedByStatementBlock()) {
			Expect(_implementation);
			StatementBlock(/*Parser.GlobalScope.atg:723*/target.Ast);
		} else if (la.kind == _assign || la.kind == _implementation) {
			if (la.kind == _assign) {
				Get();
			} else {
				Get();
			}
			/*Parser.GlobalScope.atg:724*/AstReturn ret = new AstReturn(this, ReturnVariant.Exit); 
			Expr(/*Parser.GlobalScope.atg:725*/out ret.Expression);
			/*Parser.GlobalScope.atg:725*/target.Ast.Add(ret); 
			Expect(_semicolon);
		} else SynErr(153);
		/*Parser.GlobalScope.atg:727*/_popLexerState();
		_PopScope(FunctionTargets[func]);
		if(isLazy || isCoroutine)
		{
		  _PopScope(cst);
		}
		                    //Compile AST
		                    if(errors.count == 0)
		                    {
		                        if(Engine.StringsAreEqual(func.Id, @"\init"))
		                        {
		                            try {
		                            TargetApplication._RequireInitialization();
		                            Loader._EmitPartialInitializationCode();
		                            //Initialize function gets finished at the end of Loader.Load
		                            } catch(Exception e) {
		                                SemErr("Exception during compilation of initialization code." + e);
		                            }
		                        }
		                        else
		                        {
		                            try {
		                            //Apply compiler hooks for all kinds of functions (lazy/coroutine/macro)
		    FunctionTargets[func].ExecuteCompilerHooks();
		    //Emit code for top-level block
		                            Ast[func].EmitCode(FunctionTargets[func], true, StackSemantics.Effect);
		                            FunctionTargets[func].FinishTarget();
		                            } catch(Exception e) {
		                                SemErr("Exception during compilation of function body of " + id + ". " + e);
		                            }
		                        }                                       
		                        
		  if(isCoroutine)
		  {
		                            try {
		        //Stub has to be returned into the physical slot mentioned in the source code
		        func = derStub;
		        //Generate code for the stub
		        AstCreateCoroutine crcor = new AstCreateCoroutine(this);                                            
		        crcor.Expression = Create.CreateClosure(GetPosition(),EntityRef.Function.Create(derBody.Id,derBody.ParentApplication.Module.Name));
		
		                            AstReturn retst = new AstReturn(this, ReturnVariant.Exit);
		                            retst.Expression = crcor;
		                            cst.Ast.Add(retst);
		                            //Emit code for top-level block
		                            cst.Ast.EmitCode(cst,true,StackSemantics.Effect);
		                            cst.FinishTarget();
		                                                } catch(Exception e) {
		                                                    SemErr("Exception during compilation of coroutine stub for " + id + ". " + e);
		                                                }
		                      }
		                      else if(isLazy)
		                      {
		                          derStub.Meta[PFunction.LazyKey] = true;
		                          derStub.Meta["strict"] = true;
		                      
		                          //Stub has to be returned into the physical slot mentioned in the source code
		                          func = derStub;
		                          
		                          //Generate code for the stub
		                          AstExpr retVal;										    
		                            
		                            if(isNested)
		                            {
		                                //Nested lazy functions need a stub to capture their environment by value (handled by NestedFunction)
		                                
		                                //Generate stub code
		                                retVal = Create.CreateClosure(GetPosition(), EntityRef.Function.Create(ct.Function.Id, 
		                                                ct.Function.ParentApplication.Module.Name));
		                                
		                                //Inject asthunk-conversion code into body
		                                var inject = derStub.Parameters.Select(par => 
		                                {
		                                  var getParam = Create.Call(position, EntityRef.Variable.Local.Create(par));
		                                  var asThunkCall = Create.Call(position, EntityRef.Variable.Command.Create(Engine.AsThunkAlias));
		                                  asThunkCall.Arguments.Add(getParam);
		                                  var setParam = Create.Call(position, EntityRef.Variable.Local.Create(par));
		                                  setParam.Arguments.Add(asThunkCall);
		                                  return (AstNode) setParam;
		                                });
		                                ct.Ast.InsertRange(0,inject);
		                            }
		                            else
		                            {										            
		                                //Global lazy functions don't technically need a stub. Might be removed later on
		                                var call = Create.Call(position,EntityRef.Function.Create(ct.Function.Id, TargetModule.Name));
		                                
		                                //Generate code for arguments (each wrapped in a `asThunk` command call)
		                              foreach(var par in derStub.Parameters)
		                              {
		                                  var getParam = Create.Call(position, EntityRef.Variable.Local.Create(par));
		                                      
		                                  var asThunkCall = Create.Call(position, EntityRef.Command.Create(Engine.AsThunkAlias));
		                                      
		                                  asThunkCall.Arguments.Add(getParam);
		                                  call.Arguments.Add(asThunkCall);
		                              }
		                              
		                              retVal = call;
		                            }								    
		                          
		                          
		                          //Assemble return statement
		                          var ret = new AstReturn(this, ReturnVariant.Exit);
		                          ret.Expression = retVal;
		                          
		                          cst.Ast.Add(ret);
		                          
		                                                try {
		                          //Emit code for stub
		                          cst.Ast.EmitCode(cst,true,StackSemantics.Effect);
		                          cst.FinishTarget();
		                                                } catch(Exception e) {
		                                                    SemErr("Exception during compilation of lazy function stub for " + id + ". " + e);
		                                                }
		                      }                                        
		                                        }
		                                    
	}

	void NamespaceDeclaration() {
		/*Parser.GlobalScope.atg:887*/QualifiedId fullNsId = default(QualifiedId);
		DeclarationScope scope = null;
		ISourcePosition qualIdPos = null;
		
		
		Expect(_namespace);
		/*Parser.GlobalScope.atg:892*/qualIdPos = GetPosition(); 
		NsQualifiedId(/*Parser.GlobalScope.atg:893*/out fullNsId);
		Expect(_lbrace);
		/*Parser.GlobalScope.atg:896*/_pushDeclScope(fullNsId, qualIdPos); /* will also require import spec*/ 
		DeclarationLevel();
		Expect(_rbrace);
		/*Parser.GlobalScope.atg:898*/scope = _popDeclScope(); 
		if (la.kind == _export) {
			Get();
		} else if (StartOf(39)) {
		} else SynErr(154);
		/*Parser.GlobalScope.atg:902*/_updateNamespace(scope); /* will also require export spec*/ 
	}

	void GlobalId(/*Parser.GlobalScope.atg:859*/out string id) {
		/*Parser.GlobalScope.atg:859*/id = "...no freaking id..."; 
		if (la.kind == _id) {
			Get();
			/*Parser.GlobalScope.atg:861*/id = cache(t.val); 
		} else if (la.kind == _anyId) {
			Get();
			String(/*Parser.GlobalScope.atg:862*/out id);
			/*Parser.GlobalScope.atg:862*/id = cache(id); 
		} else SynErr(155);
	}

	void MetaExpr(/*Parser.GlobalScope.atg:97*/out MetaEntry entry) {
		/*Parser.GlobalScope.atg:97*/bool sw; int i; double r; entry = null; string str; Version v; 
		if (la.kind == _true || la.kind == _false) {
			Boolean(/*Parser.GlobalScope.atg:99*/out sw);
			/*Parser.GlobalScope.atg:99*/entry = sw; 
		} else if (la.kind == _integer) {
			Integer(/*Parser.GlobalScope.atg:100*/out i);
			/*Parser.GlobalScope.atg:100*/entry = i.ToString(CultureInfo.InvariantCulture); 
		} else if (la.kind == _real || la.kind == _realLike) {
			Real(/*Parser.GlobalScope.atg:101*/out r);
			/*Parser.GlobalScope.atg:101*/entry = r.ToString(CultureInfo.InvariantCulture); 
		} else if (StartOf(40)) {
			if (la.kind == _string) {
				String(/*Parser.GlobalScope.atg:102*/out str);
				/*Parser.GlobalScope.atg:102*/entry = str; 
			} else {
				GlobalQualifiedId(/*Parser.GlobalScope.atg:103*/out str);
				/*Parser.GlobalScope.atg:104*/entry = str; 
			}
			if (la.kind == _div) {
				Get();
				Version(/*Parser.GlobalScope.atg:107*/out v);
				/*Parser.GlobalScope.atg:107*/entry = new Prexonite.Modular.ModuleName(entry.Text,v); 
			}
		} else if (la.kind == _lbrace) {
			Get();
			/*Parser.GlobalScope.atg:109*/List<MetaEntry> lst = new List<MetaEntry>(); 
			MetaEntry subEntry; 
			bool lastWasEmpty = false;
			
			if (StartOf(32)) {
				MetaExpr(/*Parser.GlobalScope.atg:113*/out subEntry);
				/*Parser.GlobalScope.atg:113*/lst.Add(subEntry); 
				while (la.kind == _comma) {
					Get();
					/*Parser.GlobalScope.atg:114*/if(lastWasEmpty)
					    SemErr("Missing meta expression in list (two consecutive commas).");
					
					if (StartOf(32)) {
						MetaExpr(/*Parser.GlobalScope.atg:117*/out subEntry);
						/*Parser.GlobalScope.atg:118*/lst.Add(subEntry); 
						lastWasEmpty = false;
						
					} else if (la.kind == _comma || la.kind == _rbrace) {
						/*Parser.GlobalScope.atg:121*/lastWasEmpty = true; 
					} else SynErr(156);
				}
			}
			Expect(_rbrace);
			/*Parser.GlobalScope.atg:125*/entry = (MetaEntry) lst.ToArray(); 
		} else SynErr(157);
	}

	void GlobalQualifiedId(/*Parser.GlobalScope.atg:865*/out string id) {
		/*Parser.GlobalScope.atg:865*/id = "\\NoId\\"; 
		if (la.kind == _id || la.kind == _anyId) {
			GlobalId(/*Parser.GlobalScope.atg:867*/out id);
		} else if (la.kind == _ns) {
			Get();
			/*Parser.GlobalScope.atg:868*/StringBuilder buffer = new StringBuilder(t.val); buffer.Append('.'); 
			while (la.kind == _ns) {
				Get();
				/*Parser.GlobalScope.atg:869*/buffer.Append(t.val); buffer.Append('.'); 
			}
			GlobalId(/*Parser.GlobalScope.atg:871*/out id);
			/*Parser.GlobalScope.atg:871*/buffer.Append(id); 
			/*Parser.GlobalScope.atg:872*/id = cache(buffer.ToString()); 
		} else SynErr(158);
	}

	void Version(/*Parser.Helper.atg:65*/out Version version) {
		if (la.kind == _realLike) {
			Get();
		} else if (la.kind == _version) {
			Get();
		} else SynErr(159);
		/*Parser.Helper.atg:66*/var raw = t.val;
		if(!TryParseVersion(raw, out version))
		{
		                               SemErr(t,"Cannot recognize \"" + raw + "\" as a version literal.");
			version = new Version(0,0);
		}
		                       
	}

	void GlobalVariableAliasList(/*Parser.GlobalScope.atg:186*/IList<string> aliases ) {
		/*Parser.GlobalScope.atg:186*/string id; 
		Expect(_as);
		GlobalId(/*Parser.GlobalScope.atg:188*/out id);
		/*Parser.GlobalScope.atg:188*/aliases.Add(id); 
		while (la.kind == _comma) {
			Get();
			if (la.kind == _id || la.kind == _anyId) {
				GlobalId(/*Parser.GlobalScope.atg:190*/out id);
				/*Parser.GlobalScope.atg:190*/aliases.Add(id); 
			}
		}
	}

	void SymbolPrefix(/*Parser.GlobalScope.atg:198*/SymbolBuilder symbol, out bool canBeRef) {
		/*Parser.GlobalScope.atg:198*/canBeRef = true; 
		if (la.kind == _ref) {
			Get();
			/*Parser.GlobalScope.atg:200*/symbol.Dereference(); 
		} else if (la.kind == _pointer) {
			Get();
			/*Parser.GlobalScope.atg:201*/symbol.ReferenceTo(); canBeRef = false; 
		} else SynErr(160);
	}

	void EntityFactory(/*Parser.GlobalScope.atg:205*/bool canBeRef, out Func<string,ModuleName,EntityRef> entityFactory ) {
		/*Parser.GlobalScope.atg:205*/entityFactory = null; 
		if (la.kind == _var) {
			Get();
			/*Parser.GlobalScope.atg:206*/entityFactory = EntityRef.Variable.Global.Create; 
		} else if (la.kind == _function) {
			Get();
			/*Parser.GlobalScope.atg:207*/entityFactory = EntityRef.Function.Create; 
		} else if (la.kind == _command) {
			Get();
			/*Parser.GlobalScope.atg:208*/entityFactory = (id,_) => EntityRef.Command.Create(id); 
		} else if (la.kind == _macro) {
			Get();
			if (la.kind == _function) {
				Get();
				/*Parser.GlobalScope.atg:210*/entityFactory = EntityRef.Function.Create; 
			} else if (la.kind == _command) {
				Get();
				/*Parser.GlobalScope.atg:211*/entityFactory = (id,_) => EntityRef.MacroCommand.Create(id); 
			} else if (la.kind == _var) {
				Get();
				/*Parser.GlobalScope.atg:212*/entityFactory = EntityRef.Variable.Global.Create; 
			} else SynErr(161);
		} else if (StartOf(41)) {
			/*Parser.GlobalScope.atg:214*/if(canBeRef) 
			{
			  entityFactory = EntityRef.Variable.Global.Create;
			}
			else
			{
			  // entityFactory already set to null
			}                                
			
		} else SynErr(162);
	}

	void DeclarationInstance2(/*Parser.GlobalScope.atg:361*/Func<string,ModuleName,EntityRef> entityFactory, 
ModuleName module, 
SymbolBuilder builder,
bool preventOverride = false ) {
		/*Parser.GlobalScope.atg:361*/string lhsId;
		string rhsId; 
		ISourcePosition position = GetPosition(); 
		
		SymbolDirective(/*Parser.GlobalScope.atg:366*/entityFactory,module,builder,out lhsId,preventOverride:preventOverride);
		/*Parser.GlobalScope.atg:367*/rhsId = lhsId; 
		if (la.kind == _as) {
			Get();
			Id(/*Parser.GlobalScope.atg:368*/out rhsId);
		}
		/*Parser.GlobalScope.atg:370*/if(entityFactory == null) 
		{
		  // We are declaring an alias to an existing symbol
		  Symbol existing;   
		  if(lhsId == null)
		  {
		    if(rhsId == null)
		    {
		      Loader.ReportMessage(Message.Error(
		        "This symbol declaration requires an alias (e.g., `as theAlias`)", 
		        GetPosition(), 
		        MessageClasses.SymbolAliasMissing));
		      // Since there is no name involved, not acting on this
		      //  statement will not cause further errors
		    }
		    else
		    {
		      Symbols.Declare(rhsId, builder.WrapSymbol(null));
		    }
		  }                             
		  else if(Symbols.TryGet(lhsId,out existing)) 
		  {
		    // Declare $rhsId as an alias for the symbol that $lhsId points to
		    Symbols.Declare(rhsId, builder.WrapSymbol(existing));
		  } 
		  else
		  {
		    var msg = Message.Error(string.Format(Resources.Parser_Could_not_find_previous_declaration, lhsId),
		  position,MessageClasses.SymbolNotResolved);
		    // We report the message AND store it as a message symbol.
		    //  That way, the symbol is at least declared, avoiding a spurious 
		    //  symbol not found message.
		    Loader.ReportMessage(msg);
		    Symbols.Declare(rhsId, Symbol.CreateMessage(msg,Symbol.CreateNil(msg.Position)));                                      
		  }
		}
		else
		{
		  
		  if(lhsId == null)
		  {
		    // For instance `declare var error(...,null)` can get us here
		    var msg = Message.Error("Entity name missing for declaration of a fresh symbol",
		      GetPosition(),
		      MessageClasses.EntityNameMissing);
		    Loader.ReportMessage(msg);
		    // Also create an error symbol for the alias (if one was declared)
		    if(rhsId != null)
		      Symbols.Declare(rhsId, Symbol.CreateMessage(msg,Symbol.CreateNil(msg.Position)));
		  }
		  else
		  {
		    // Use the builder to create a new symbol.
		    Symbols.Declare(rhsId, builder.ToSymbol());
		  }                                  
		}
		
	}

	void ModuleName(/*Parser.GlobalScope.atg:430*/out ModuleName moduleName) {
		/*Parser.GlobalScope.atg:430*/_pushLexerState(Lexer.YYINITIAL); //need global scope for Version
		string id; 
		Version version = null;
		
		Id(/*Parser.GlobalScope.atg:435*/out id);
		if (la.kind == _div) {
			Get();
			Version(/*Parser.GlobalScope.atg:437*/out version);
		}
		/*Parser.GlobalScope.atg:438*/_popLexerState();
		moduleName = Loader.Cache[new ModuleName(id,version ?? new Version(0,0))];
		
	}

	void MExprBasedDeclaration() {
		/*Parser.GlobalScope.atg:275*/string alias;
		MExpr expr;
		
		Id(/*Parser.GlobalScope.atg:279*/out alias);
		Expect(_assign);
		MExpr(/*Parser.GlobalScope.atg:279*/out expr);
		/*Parser.GlobalScope.atg:280*/Symbol s = _parseSymbol(expr);
		Symbols.Declare(alias,s);
		
	}

	void MExpr(/*Parser.Helper.atg:155*/out MExpr expr) {
		/*Parser.Helper.atg:155*/expr = new MExpr.MAtom(NoSourcePosition.Instance,null);
		       bool lastWasComma = false;
		   
		if (StartOf(42)) {
			/*Parser.Helper.atg:158*/String id; 
			var args = new List<MExpr>();
			MExpr arg;
			
			MId(/*Parser.Helper.atg:162*/out id);
			if (la.kind == _lpar) {
				Get();
				if (StartOf(43)) {
					MExpr(/*Parser.Helper.atg:164*/out arg);
					/*Parser.Helper.atg:164*/args.Add(arg); 
					while (la.kind == _comma) {
						Get();
						/*Parser.Helper.atg:165*/if(lastWasComma)
						          {
						              Loader.ReportMessage(Message.Error("Double comma in MExpr list.",GetPosition(),MessageClasses.DuplicateComma));
						          }
						          lastWasComma = true; 
						      
						if (StartOf(43)) {
							MExpr(/*Parser.Helper.atg:171*/out arg);
							/*Parser.Helper.atg:172*/args.Add(arg); 
							lastWasComma = false;
							
						}
					}
				}
				Expect(_rpar);
			} else if (StartOf(43)) {
				MExpr(/*Parser.Helper.atg:179*/out arg);
				/*Parser.Helper.atg:179*/args.Add(arg); 
			} else if (la.kind == _comma || la.kind == _rpar) {
				
			} else SynErr(163);
			/*Parser.Helper.atg:181*/expr = new MExpr.MList(GetPosition(), id,args); 
		} else if (la.kind == _string) {
			/*Parser.Helper.atg:182*/String value; 
			String(/*Parser.Helper.atg:183*/out value);
			/*Parser.Helper.atg:183*/expr = new MExpr.MAtom(GetPosition(), value); 
		} else if (la.kind == _integer || la.kind == _minus || la.kind == _plus) {
			/*Parser.Helper.atg:184*/int intval; 
			SignedInteger(/*Parser.Helper.atg:185*/out intval);
			/*Parser.Helper.atg:185*/expr = new MExpr.MAtom(GetPosition(), intval); 
		} else if (la.kind == _true || la.kind == _false) {
			/*Parser.Helper.atg:186*/bool boolval; 
			Boolean(/*Parser.Helper.atg:187*/out boolval);
			/*Parser.Helper.atg:187*/expr = new MExpr.MAtom(GetPosition(), boolval); 
		} else if (la.kind == _version || la.kind == _realLike) {
			/*Parser.Helper.atg:188*/Version v; 
			Version(/*Parser.Helper.atg:189*/out v);
			/*Parser.Helper.atg:189*/expr = new MExpr.MAtom(GetPosition(), v); 
		} else if (la.kind == _null) {
			Null();
			/*Parser.Helper.atg:191*/expr = new MExpr.MAtom(GetPosition(), null); 
		} else SynErr(164);
	}

	void MessageDirective(/*Parser.GlobalScope.atg:292*/Func<string,ModuleName,EntityRef> entityFactory, 
ModuleName module, 
SymbolBuilder builder,
[CanBeNull] out string lhsId,
MessageSeverity severity,
bool preventOverride = false ) {
		/*Parser.GlobalScope.atg:292*/string message;
		string messageClass = null;
		ISourcePosition position = GetPosition();
		string file;
		int line;
		int column;
		
		Expect(_lpar);
		if (la.kind == _null) {
			Get();
		} else if (la.kind == _string) {
			String(/*Parser.GlobalScope.atg:301*/out messageClass);
		} else SynErr(165);
		if (la.kind == _colon) {
			Get();
			if (la.kind == _null) {
				Get();
			} else if (la.kind == _string) {
				String(/*Parser.GlobalScope.atg:304*/out file);
				Expect(_colon);
				Integer(/*Parser.GlobalScope.atg:304*/out line);
				Expect(_colon);
				Integer(/*Parser.GlobalScope.atg:304*/out column);
				/*Parser.GlobalScope.atg:305*/position = new SourcePosition(file,line,column); 
			} else SynErr(166);
		}
		Expect(_comma);
		String(/*Parser.GlobalScope.atg:309*/out message);
		Expect(_comma);
		/*Parser.GlobalScope.atg:310*/builder.AddMessage(Message.Create(severity,message,position,messageClass)); 
		SymbolDirective(/*Parser.GlobalScope.atg:311*/entityFactory,module,builder,out lhsId,preventOverride:preventOverride);
		Expect(_rpar);
	}

	void SymbolDirective(/*Parser.GlobalScope.atg:320*/Func<string,ModuleName,EntityRef> entityFactory, 
[CanBeNull] ModuleName module, 
SymbolBuilder builder,
[CanBeNull] out string lhsId,
bool preventOverride = false ) {
		/*Parser.GlobalScope.atg:320*/lhsId = null;  
		if (la.kind == _null) {
			Get();
			
		} else if (la.kind == _pointer || la.kind == _ref) {
			if (la.kind == _ref) {
				Get();
				/*Parser.GlobalScope.atg:323*/builder.Dereference(); 
			} else {
				Get();
				/*Parser.GlobalScope.atg:324*/builder.ReferenceTo(); 
			}
			SymbolDirective(/*Parser.GlobalScope.atg:326*/entityFactory,module,builder,out lhsId,preventOverride:preventOverride);
		} else if (/*Parser.GlobalScope.atg:328*/isSymbolDirective("INFO")) {
			Expect(_id);
			MessageDirective(/*Parser.GlobalScope.atg:329*/entityFactory,module,builder, out lhsId,MessageSeverity.Info,preventOverride:preventOverride);
		} else if (/*Parser.GlobalScope.atg:331*/isSymbolDirective("WARN")) {
			Expect(_id);
			MessageDirective(/*Parser.GlobalScope.atg:332*/entityFactory,module,builder, out lhsId,MessageSeverity.Warning,preventOverride:preventOverride);
		} else if (/*Parser.GlobalScope.atg:334*/isSymbolDirective("ERROR")) {
			Expect(_id);
			MessageDirective(/*Parser.GlobalScope.atg:335*/entityFactory,module,builder, out lhsId,MessageSeverity.Error,preventOverride:preventOverride);
		} else if (StartOf(4)) {
			/*Parser.GlobalScope.atg:336*/ISourcePosition position = GetPosition(); 
			Id(/*Parser.GlobalScope.atg:337*/out lhsId);
			if (la.kind == _div) {
				Get();
				ModuleName(/*Parser.GlobalScope.atg:338*/out module);
				/*Parser.GlobalScope.atg:339*/if(preventOverride) 
				{
				    Loader.ReportMessage(Message.Error(
				      "Specification of module name illegal at this point.",
				      position  ,
				      MessageClasses.UnexpectedModuleName)); 
				    // Let control fall through, this is not a fatal error,
				    //  just an enforcement of a stylistic rule.
				}
				
			}
			/*Parser.GlobalScope.atg:349*/if(entityFactory != null)
			{
			  builder.Entity = entityFactory(lhsId,module);
			}
			
		} else SynErr(167);
	}

	void StatementBlock(/*Parser.Statement.atg:26*/AstBlock block) {
		Statement(/*Parser.Statement.atg:27*/block);
	}

	void FunctionAliasList(/*Parser.GlobalScope.atg:503*/IList<string> aliases ) {
		/*Parser.GlobalScope.atg:503*/String id; 
		Expect(_as);
		Id(/*Parser.GlobalScope.atg:505*/out id);
		/*Parser.GlobalScope.atg:505*/aliases.Add(id); 
		while (la.kind == _comma) {
			Get();
			if (StartOf(4)) {
				Id(/*Parser.GlobalScope.atg:507*/out id);
				/*Parser.GlobalScope.atg:507*/aliases.Add(id); 
			}
		}
	}

	void NsQualifiedId(/*Parser.GlobalScope.atg:876*/out QualifiedId qualifiedId) {
		/*Parser.GlobalScope.atg:876*/qualifiedId = default(QualifiedId);
		String part = null;
		var parts = new List<String>();
		
		GlobalId(/*Parser.GlobalScope.atg:881*/out part);
		/*Parser.GlobalScope.atg:881*/parts.Add(part); 
		if (la.kind == _dot) {
			Get();
			GlobalId(/*Parser.GlobalScope.atg:883*/out part);
			/*Parser.GlobalScope.atg:883*/parts.Add(part); 
		}
		/*Parser.GlobalScope.atg:884*/qualifiedId = new QualifiedId(parts.ToArray()); 
	}

	void MId(/*Parser.Helper.atg:92*/out String id) {
		/*Parser.Helper.atg:92*/id = "an\\invalid\\MId"; 
		if (StartOf(44)) {
			switch (la.kind) {
			case _var: {
				Get();
				break;
			}
			case _ref: {
				Get();
				break;
			}
			case _true: {
				Get();
				break;
			}
			case _false: {
				Get();
				break;
			}
			case _mod: {
				Get();
				break;
			}
			case _is: {
				Get();
				break;
			}
			case _as: {
				Get();
				break;
			}
			case _not: {
				Get();
				break;
			}
			case _function: {
				Get();
				break;
			}
			case _command: {
				Get();
				break;
			}
			case _asm: {
				Get();
				break;
			}
			case _declare: {
				Get();
				break;
			}
			case _build: {
				Get();
				break;
			}
			case _return: {
				Get();
				break;
			}
			case _in: {
				Get();
				break;
			}
			case _to: {
				Get();
				break;
			}
			case _continue: {
				Get();
				break;
			}
			case _break: {
				Get();
				break;
			}
			case _yield: {
				Get();
				break;
			}
			case _or: {
				Get();
				break;
			}
			case _and: {
				Get();
				break;
			}
			case _xor: {
				Get();
				break;
			}
			case _label: {
				Get();
				break;
			}
			case _goto: {
				Get();
				break;
			}
			case _static: {
				Get();
				break;
			}
			case _if: {
				Get();
				break;
			}
			case _unless: {
				Get();
				break;
			}
			case _else: {
				Get();
				break;
			}
			case _new: {
				Get();
				break;
			}
			case _coroutine: {
				Get();
				break;
			}
			case _from: {
				Get();
				break;
			}
			case _do: {
				Get();
				break;
			}
			case _does: {
				Get();
				break;
			}
			case _while: {
				Get();
				break;
			}
			case _until: {
				Get();
				break;
			}
			case _for: {
				Get();
				break;
			}
			case _foreach: {
				Get();
				break;
			}
			case _try: {
				Get();
				break;
			}
			case _catch: {
				Get();
				break;
			}
			case _finally: {
				Get();
				break;
			}
			case _throw: {
				Get();
				break;
			}
			case _then: {
				Get();
				break;
			}
			case _uusing: {
				Get();
				break;
			}
			case _macro: {
				Get();
				break;
			}
			case _lazy: {
				Get();
				break;
			}
			case _let: {
				Get();
				break;
			}
			case _method: {
				Get();
				break;
			}
			case _this: {
				Get();
				break;
			}
			case _bitAnd: {
				Get();
				break;
			}
			case _pow: {
				Get();
				break;
			}
			case _times: {
				Get();
				break;
			}
			case _tilde: {
				Get();
				break;
			}
			case _question: {
				Get();
				break;
			}
			case _pointer: {
				Get();
				break;
			}
			case _at: {
				Get();
				break;
			}
			case _colon: {
				Get();
				break;
			}
			case _implementation: {
				Get();
				break;
			}
			}
			/*Parser.Helper.atg:151*/id = cache(t.val); 
		} else if (StartOf(4)) {
			Id(/*Parser.Helper.atg:152*/out id);
		} else SynErr(168);
	}

	void ExplicitLabel(/*Parser.Statement.atg:329*/AstBlock block) {
		/*Parser.Statement.atg:329*/string id = "--\\NotAnId\\--"; 
		if (StartOf(4)) {
			Id(/*Parser.Statement.atg:331*/out id);
			Expect(_colon);
		} else if (la.kind == _lid) {
			Get();
			/*Parser.Statement.atg:332*/id = cache(t.val.Substring(0,t.val.Length-1)); 
		} else SynErr(169);
		/*Parser.Statement.atg:333*/block.Statements.Add(new AstExplicitLabel(this, id)); 
	}

	void SimpleStatement(/*Parser.Statement.atg:43*/AstBlock block) {
		if (la.kind == _goto) {
			ExplicitGoTo(/*Parser.Statement.atg:44*/block);
		} else if (StartOf(18)) {
			GetSetComplex(/*Parser.Statement.atg:45*/block);
		} else if (StartOf(45)) {
			Return(/*Parser.Statement.atg:46*/block);
		} else if (la.kind == _throw) {
			Throw(/*Parser.Statement.atg:47*/block);
		} else if (la.kind == _let) {
			LetBindingStmt(/*Parser.Statement.atg:48*/block);
		} else SynErr(170);
	}

	void StructureStatement(/*Parser.Statement.atg:52*/AstBlock block) {
		switch (la.kind) {
		case _asm: {
			/*Parser.Statement.atg:53*/_pushLexerState(Lexer.Asm); 
			Get();
			AsmStatementBlock(/*Parser.Statement.atg:54*/block);
			/*Parser.Statement.atg:55*/_popLexerState(); 
			break;
		}
		case _if: case _unless: {
			Condition(/*Parser.Statement.atg:56*/block);
			break;
		}
		case _declare: {
			Declaration2();
			break;
		}
		case _do: case _while: case _until: {
			WhileLoop(/*Parser.Statement.atg:58*/block);
			break;
		}
		case _for: {
			ForLoop(/*Parser.Statement.atg:59*/block);
			break;
		}
		case _foreach: {
			ForeachLoop(/*Parser.Statement.atg:60*/block);
			break;
		}
		case _function: case _coroutine: case _macro: case _lazy: {
			NestedFunction(/*Parser.Statement.atg:61*/block);
			break;
		}
		case _try: {
			TryCatchFinally(/*Parser.Statement.atg:62*/block);
			break;
		}
		case _uusing: {
			Using(/*Parser.Statement.atg:63*/block);
			break;
		}
		case _lbrace: {
			Get();
			while (StartOf(20)) {
				Statement(/*Parser.Statement.atg:66*/block);
			}
			Expect(_rbrace);
			break;
		}
		default: SynErr(171); break;
		}
	}

	void ExplicitGoTo(/*Parser.Statement.atg:336*/AstBlock block) {
		/*Parser.Statement.atg:336*/string id; 
		Expect(_goto);
		Id(/*Parser.Statement.atg:339*/out id);
		/*Parser.Statement.atg:339*/block.Statements.Add(new AstExplicitGoTo(this, id)); 
	}

	void GetSetComplex(/*Parser.Statement.atg:72*/AstBlock block) {
		/*Parser.Statement.atg:72*/AstGetSet complex; 
		AstExpr expr;
		AstNode node;
		
		GetInitiator(/*Parser.Statement.atg:78*/out expr);
		/*Parser.Statement.atg:78*/complex = expr as AstGetSet; 
		while (la.kind == _dot || la.kind == _lbrack) {
			GetSetExtension(/*Parser.Statement.atg:81*/expr, out complex);
			/*Parser.Statement.atg:81*/expr = complex; 
		}
		if (la.kind == _rpar || la.kind == _semicolon) {
			/*Parser.Statement.atg:84*/if(expr != null) // Happens in case of an error
			   block.Add(expr); 
			
		} else if (StartOf(46)) {
			/*Parser.Statement.atg:87*/var pos = GetPosition(); 
			if(complex == null)
			{                                                
			    Loader.ReportMessage(Message.Error("Expected an LValue (Get/Set-Complex) for ++,-- or assignment statement.",pos,MessageClasses.LValueExpected));
			    complex = Create.IndirectCall(pos,Create.Null(pos));
			}                                            
			
			if (la.kind == _inc) {
				Get();
				/*Parser.Statement.atg:94*/block.Add(Create.UnaryOperation(pos, UnaryOperator.PostIncrement, complex)); 
			} else if (la.kind == _dec) {
				Get();
				/*Parser.Statement.atg:95*/block.Add(Create.UnaryOperation(pos, UnaryOperator.PostDecrement, complex)); 
			} else if (StartOf(47)) {
				Assignment(/*Parser.Statement.atg:96*/complex, out node);
				/*Parser.Statement.atg:96*/if(complex == null && node == null)
				{
				    // An error ocurred during parsing prior to this point.
				    // Don't add the null node to the block to avoid a ArgumentNullException
				}
				else if(node == null)
				{
				    Loader.ReportMessage(Message.Error("Internal error during translation of assignment. This is likely caused by an error reported previously.",GetPosition(),MessageClasses.ParserInternal));
				}
				else
				{
				    block.Add(node);
				}
				
			} else {
				AppendRightTermination(/*Parser.Statement.atg:110*/ref complex);
				while (la.kind == _appendright) {
					AppendRightTermination(/*Parser.Statement.atg:111*/ref complex);
				}
				/*Parser.Statement.atg:113*/block.Add(complex);  
			}
		} else SynErr(172);
	}

	void Return(/*Parser.Statement.atg:493*/AstBlock block) {
		/*Parser.Statement.atg:493*/AstReturn ret = null; 
		AstExplicitGoTo jump = null; 
		AstExpr expr; 
		AstLoopBlock bl = target.CurrentLoopBlock;
		
		if (la.kind == _return || la.kind == _yield) {
			if (la.kind == _return) {
				Get();
				/*Parser.Statement.atg:501*/ret = new AstReturn(this, ReturnVariant.Exit); 
			} else {
				Get();
				/*Parser.Statement.atg:502*/ret = new AstReturn(this, ReturnVariant.Continue); 
			}
			if (StartOf(48)) {
				if (StartOf(14)) {
					Expr(/*Parser.Statement.atg:504*/out expr);
					/*Parser.Statement.atg:504*/ret.Expression = expr; 
				} else {
					Get();
					/*Parser.Statement.atg:505*/ret.ReturnVariant = ReturnVariant.Set; 
					Expr(/*Parser.Statement.atg:506*/out expr);
					/*Parser.Statement.atg:506*/ret.Expression = expr; 
					/*Parser.Statement.atg:507*/SemErr("Return value assignment is no longer supported. You must use local variables instead."); 
				}
			}
		} else if (la.kind == _break) {
			Get();
			/*Parser.Statement.atg:509*/if(bl == null)
			   ret = new AstReturn(this, ReturnVariant.Break); 
			else
			    jump = new AstExplicitGoTo(this, bl.BreakLabel);
			
		} else if (la.kind == _continue) {
			Get();
			/*Parser.Statement.atg:514*/if(bl == null)
			   ret = new AstReturn(this, ReturnVariant.Continue); 
			else
			    jump = new AstExplicitGoTo(this, bl.ContinueLabel);
			
		} else SynErr(173);
		/*Parser.Statement.atg:519*/block.Add((AstNode)ret ?? jump); 
	}

	void Throw(/*Parser.Statement.atg:644*/AstBlock block) {
		/*Parser.Statement.atg:644*/AstThrow th; 
		ThrowExpression(/*Parser.Statement.atg:646*/out th);
		/*Parser.Statement.atg:647*/block.Add(th); 
	}

	void LetBindingStmt(/*Parser.Statement.atg:559*/AstBlock block) {
		Expect(_let);
		LetBinder(/*Parser.Statement.atg:560*/block);
		while (la.kind == _comma) {
			Get();
			LetBinder(/*Parser.Statement.atg:560*/block);
		}
	}

	void Condition(/*Parser.Statement.atg:379*/AstBlock block) {
		/*Parser.Statement.atg:379*/AstExpr expr; bool isNegative = false; 
		if (la.kind == _if) {
			Get();
			
		} else if (la.kind == _unless) {
			Get();
			/*Parser.Statement.atg:382*/isNegative = true; 
		} else SynErr(174);
		Expect(_lpar);
		Expr(/*Parser.Statement.atg:385*/out expr);
		Expect(_rpar);
		/*Parser.Statement.atg:385*/if(expr == null)
		   expr = _createUnknownExpr();
		AstCondition cond = Create.Condition(GetPosition(), expr, isNegative);
		_PushScope(cond.IfBlock);
		
		StatementBlock(/*Parser.Statement.atg:391*/cond.IfBlock);
		/*Parser.Statement.atg:392*/_PopScope(cond.IfBlock); 
		if (la.kind == _else) {
			Get();
			/*Parser.Statement.atg:395*/_PushScope(cond.ElseBlock); 
			StatementBlock(/*Parser.Statement.atg:396*/cond.ElseBlock);
			/*Parser.Statement.atg:397*/_PopScope(cond.ElseBlock); 
		}
		/*Parser.Statement.atg:398*/block.Add(cond); 
	}

	void NestedFunction(/*Parser.Statement.atg:523*/AstBlock block) {
		/*Parser.Statement.atg:523*/PFunction func; 
		FunctionDefinition(/*Parser.Statement.atg:525*/out func);
		/*Parser.Statement.atg:527*/string logicalId = func.Meta[PFunction.LogicalIdKey];
		func.Meta[PFunction.ParentFunctionKey] = target.Function.Id;
		
		CompilerTarget ft = FunctionTargets[func];
		var pos = GetPosition();
		var setVar = Create.IndirectCall(pos,Create.Reference(pos,EntityRef.Variable.Local.Create(logicalId)),PCall.Set);
		if(func.Meta[PFunction.LazyKey].Switch)
		{
		    //Capture environment by value                                        
		    var ps = ft._ToCaptureByValue(let_bindings(ft));
		    ft._DetermineSharedNames(); //Need to re-determine shared names since
		                                // _ToCaptureByValue does not automatically modify shared names
		    var clos = Create.CreateClosure(pos,  EntityRef.Function.Create(func.Id, 
		            func.ParentApplication.Module.Name));
		    var callStub = Create.IndirectCall(pos, clos);
		    callStub.Arguments.AddRange(ps(this));
		    setVar.Arguments.Add(callStub);
		}
		else if(ft.OuterVariables.Count > 0)
		{                                        
		    setVar.Arguments.Add( Create.CreateClosure(GetPosition(),  EntityRef.Function.Create(func.Id, 
		            func.ParentApplication.Module.Name)) );                                        
		}
		else
		{
		    setVar.Arguments.Add( new AstReference(GetPosition(), EntityRef.Function.Create(func.Id,func.ParentApplication.Module.Name)) );
		}
		block.Add(setVar);
		
	}

	void TryCatchFinally(/*Parser.Statement.atg:586*/AstBlock block) {
		/*Parser.Statement.atg:586*/var a = Create.TryCatchFinally(GetPosition());
		AstGetSet excVar;
		
		Expect(_try);
		/*Parser.Statement.atg:590*/_PushScope(a);
		_PushScope(a.TryBlock); 
		
		Expect(_lbrace);
		while (StartOf(20)) {
			Statement(/*Parser.Statement.atg:594*/a.TryBlock);
		}
		Expect(_rbrace);
		
		if (la.kind == _catch || la.kind == _finally) {
			if (la.kind == _catch) {
				Get();
				/*Parser.Statement.atg:597*/_PushScope(a.CatchBlock); 
				if (la.kind == _lpar) {
					Get();
					GetCall(/*Parser.Statement.atg:599*/out excVar);
					/*Parser.Statement.atg:599*/a.ExceptionVar = excVar; 
					Expect(_rpar);
				} else if (la.kind == _lbrace) {
					/*Parser.Statement.atg:601*/SemErr(la,"catch-clauses that don't store the exception are illegal."); 
				} else SynErr(175);
				Expect(_lbrace);
				while (StartOf(20)) {
					Statement(/*Parser.Statement.atg:604*/a.CatchBlock);
				}
				Expect(_rbrace);
				/*Parser.Statement.atg:606*/_PopScope(a.CatchBlock);
				if (la.kind == _finally) {
					Get();
					/*Parser.Statement.atg:609*/_PushScope(a.FinallyBlock); 
					Expect(_lbrace);
					while (StartOf(20)) {
						Statement(/*Parser.Statement.atg:611*/a.FinallyBlock);
					}
					Expect(_rbrace);
					/*Parser.Statement.atg:613*/_PopScope(a.FinallyBlock); 
				}
			} else {
				Get();
				/*Parser.Statement.atg:616*/_PushScope(a.FinallyBlock); 
				Expect(_lbrace);
				while (StartOf(20)) {
					Statement(/*Parser.Statement.atg:618*/a.FinallyBlock);
				}
				Expect(_rbrace);
				/*Parser.Statement.atg:620*/_PopScope(a.FinallyBlock); 
				if (la.kind == _catch) {
					/*Parser.Statement.atg:622*/_PushScope(a.CatchBlock); 
					Get();
					if (la.kind == _lpar) {
						Get();
						GetCall(/*Parser.Statement.atg:625*/out excVar);
						/*Parser.Statement.atg:626*/a.ExceptionVar = excVar; 
						Expect(_rpar);
					} else if (la.kind == _lbrace) {
						/*Parser.Statement.atg:628*/SemErr(la,"catch-clauses that don't store the exception are illegal."); 
					} else SynErr(176);
					Expect(_lbrace);
					while (StartOf(20)) {
						Statement(/*Parser.Statement.atg:631*/a.CatchBlock);
					}
					Expect(_rbrace);
					/*Parser.Statement.atg:634*/_PopScope(a.CatchBlock); 
				}
			}
		}
		/*Parser.Statement.atg:637*/_PopScope(a.TryBlock);
		_PopScope(a);
		block.Add(a); 
		
	}

	void Using(/*Parser.Statement.atg:651*/AstBlock block) {
		/*Parser.Statement.atg:651*/AstUsing use = Create.Using(GetPosition());
		AstExpr e;
		
		/*Parser.Statement.atg:655*/_PushScope(use);
		_PushScope(use.Block); 
		Expect(_uusing);
		Expect(_lpar);
		Expr(/*Parser.Statement.atg:657*/out e);
		Expect(_rpar);
		/*Parser.Statement.atg:658*/use.ResourceExpression = e; 
		StatementBlock(/*Parser.Statement.atg:660*/use.Block);
		/*Parser.Statement.atg:661*/_PopScope(use.Block);
		_PopScope(use);
		                           block.Add(use); 
		                       
	}

	void Assignment(/*Parser.Statement.atg:343*/AstGetSet lvalue, out AstNode node) {
		/*Parser.Statement.atg:343*/AstExpr expr = null;
		BinaryOperator setModifier = BinaryOperator.None;
		AstTypeExpr typeExpr;
		node = lvalue;
		                           ISourcePosition position;
		
		/*Parser.Statement.atg:349*/position = GetPosition(); 
		if (StartOf(9)) {
			switch (la.kind) {
			case _assign: {
				Get();
				/*Parser.Statement.atg:351*/setModifier = BinaryOperator.None; 
				break;
			}
			case _plus: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:352*/setModifier = BinaryOperator.Addition; 
				break;
			}
			case _minus: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:353*/setModifier = BinaryOperator.Subtraction; 
				break;
			}
			case _times: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:354*/setModifier = BinaryOperator.Multiply; 
				break;
			}
			case _div: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:355*/setModifier = BinaryOperator.Division; 
				break;
			}
			case _bitAnd: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:356*/setModifier = BinaryOperator.BitwiseAnd; 
				break;
			}
			case _bitOr: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:357*/setModifier = BinaryOperator.BitwiseOr; 
				break;
			}
			case _coalescence: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:358*/setModifier = BinaryOperator.Coalescence; 
				break;
			}
			}
			Expr(/*Parser.Statement.atg:359*/out expr);
		} else if (la.kind == _tilde) {
			Get();
			Expect(_assign);
			/*Parser.Statement.atg:361*/setModifier = BinaryOperator.Cast; 
			TypeExpr(/*Parser.Statement.atg:362*/out typeExpr);
			/*Parser.Statement.atg:362*/expr = typeExpr; 
		} else SynErr(177);
		/*Parser.Statement.atg:364*/if(expr == null)
		                       {
		                           Loader.ReportMessage(Message.Error("Internal error during translation of assignment. This is likely caused by an error reported previously.",GetPosition(),MessageClasses.ParserInternal));
		                       }
		                       else
		                       {
		                           lvalue.Arguments.Add(expr);
		                       }
		lvalue.Call = PCall.Set; 
		if(setModifier != BinaryOperator.None)
		    node = Create.ModifyingAssignment(position,lvalue,setModifier);
		
	}

	void AppendRightTermination(/*Parser.Statement.atg:119*/ref AstGetSet complex) {
		/*Parser.Statement.atg:119*/AstGetSet rhs; 
		Expect(_appendright);
		GetCall(/*Parser.Statement.atg:122*/out rhs);
		/*Parser.Statement.atg:122*/_appendRight(complex,rhs);
		complex = rhs;
		
	}

	void SymbolicUsage(/*Parser.Statement.atg:312*/out AstGetSet complex) {
		/*Parser.Statement.atg:312*/string id; ISourcePosition position; 
		
		/*Parser.Statement.atg:314*/position = GetPosition(); 
		Id(/*Parser.Statement.atg:315*/out id);
		/*Parser.Statement.atg:315*/complex = _useSymbol(Symbols, id, position); 
		Arguments(/*Parser.Statement.atg:316*/complex.Arguments);
	}

	void VariableDeclaration(/*Parser.Statement.atg:219*/out AstGetSet complex) {
		/*Parser.Statement.atg:219*/string id, physicalId;
		bool isOverrideDecl = false;
		bool seenVar = false;
		int refCount = 1;
		bool isUnbound = false;
		bool isStatic = false;
		Symbol sym, varSym;
		
		if (la.kind == _new) {
			Get();
			/*Parser.Statement.atg:228*/isUnbound = true; 
		}
		if (la.kind == _static) {
			Get();
			/*Parser.Statement.atg:230*/isStatic = true; 
			while (la.kind == _ref) {
				Get();
				/*Parser.Statement.atg:231*/refCount++; 
			}
			if (la.kind == _var) {
				Get();
			}
		} else if (la.kind == _var || la.kind == _ref) {
			if (la.kind == _var) {
				Get();
				/*Parser.Statement.atg:234*/seenVar = true; 
			} else {
				Get();
				/*Parser.Statement.atg:235*/refCount++; 
			}
			while (la.kind == _var || la.kind == _ref) {
				if (la.kind == _var) {
					Get();
					/*Parser.Statement.atg:237*/if(seenVar)
					{
					    Loader.ReportMessage(Message.Error("Duplicate ocurrence of `var` in local variable declaration.",GetPosition(),MessageClasses.DuplicateVar));
					    // This is just a stylistic rule. There are no consequences to having duplicate `var` keywords in a declaration.
					}
					seenVar = true;
					
				} else {
					Get();
					/*Parser.Statement.atg:244*/refCount++; 
				}
			}
		} else SynErr(178);
		if (la.kind == _new) {
			Get();
			/*Parser.Statement.atg:248*/isOverrideDecl = true; 
		}
		/*Parser.Statement.atg:249*/ISourcePosition position = GetPosition(); 
		Id(/*Parser.Statement.atg:250*/out id);
		/*Parser.Statement.atg:250*/physicalId = id;
		if(isStatic)
		{
		    physicalId = target.Function.Id + "\\static\\" + id;
		    VariableDeclaration vari;
		    if(isOverrideDecl)
		    {
		        if(TargetModule.Variables.Contains(physicalId))
		            physicalId = target.GenerateLocalId(physicalId);
		    }
		
		                                        DefineGlobalVariable(physicalId, out vari);
		                                        varSym = Symbol.CreateReference(EntityRef.Variable.Global.Create(physicalId,TargetModule.Name),position);
		
		                                        if(isUnbound)
		                                        {
		                                            Loader.ReportMessage(Message.Error("Unbinding of global (or static) variables is not currently supported.",position,MessageClasses.GlobalUnbindNotSupported));
		                                            isUnbound = false;
		                                        }
		                                    }
		                                    else
		                                    {
		                                        if(isOverrideDecl)
		                                        {
		                                            if(target.Function.Variables.Contains(physicalId))
		                                                physicalId = target.GenerateLocalId(physicalId);
		                                            target.Function.Variables.Add(physicalId);
		                                        }
		                                        else if(!isOuterVariable(physicalId))
		                                        {
		                                            target.Function.Variables.Add(physicalId);
		                                        }
		
		                                        varSym = Symbol.CreateReference(EntityRef.Variable.Local.Create(physicalId),position);
		                                        // Create.ExprFor will request outer variables where necessary.
		                                    }
		
		                                    // Apply ref's (one ref is implied, see initialization of refCount)
		                                    //  Note: we retain the original symbol to generate the AST node
		                                    sym = varSym;
		                                    while(refCount-- > 0)
		                                        sym = Symbol.CreateDereference(sym);
		                                    
		                                    // Declare the symbol in the current scope and assemble an AST node
		                                    Symbols.Declare(id,sym);
		                                    complex = Create.ExprFor(position, Symbol.CreateDereference(varSym)) as AstGetSet;
		
		                                    if(complex == null)
		                                    {
		                                        Loader.ReportMessage(Message.Error("Expected variable declaration to result in LValue.",position,MessageClasses.LValueExpected));
		                                        complex = Create.IndirectCall(position,Create.Null(position));
		                                    }
		                                    else if(isUnbound)
		                                    {
		                                        // Wrap variable access in NewDecl
		                                        var newDecl = new AstGetSetNewDecl(position,physicalId,complex);
		                                        complex = newDecl;
		                                    }
		                                
	}

	void StaticCall(/*Parser.Statement.atg:320*/out AstGetSetStatic staticCall) {
		/*Parser.Statement.atg:320*/AstTypeExpr typeExpr;
		string memberId;
		
		ExplicitTypeExpr(/*Parser.Statement.atg:324*/out typeExpr);
		Expect(_dot);
		Id(/*Parser.Statement.atg:325*/out memberId);
		/*Parser.Statement.atg:325*/staticCall = new AstGetSetStatic(this, PCall.Get, typeExpr, memberId); 
		Arguments(/*Parser.Statement.atg:326*/staticCall.Arguments);
	}

	void LetBinder(/*Parser.Statement.atg:564*/AstBlock block) {
		/*Parser.Statement.atg:564*/string id = null;
		AstExpr thunk;
		
		/*Parser.Statement.atg:567*/var position = GetPosition(); 
		Id(/*Parser.Statement.atg:568*/out id);
		/*Parser.Statement.atg:569*/SmartDeclareLocal(id, SymbolInterpretations.LocalObjectVariable);
		mark_as_let(target.Function, id);
		if(la.kind == _assign)
		    _inject(_lazy,"lazy"); 
		
		if (la.kind == _assign) {
			Get();
			LazyExpression(/*Parser.Statement.atg:575*/out thunk);
			/*Parser.Statement.atg:578*/var assign = Create.Call(position, EntityRef.Variable.Local.Create(id), PCall.Set);
			assign.Arguments.Add(thunk);
			block.Add(assign);
			
		}
	}


#line 133 "C:\Users\Christian\Documents\GitHub\prx\Tools\Parser.frame" //FRAME


	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();

#line default //END FRAME -->parseRoot

		Prexonite();

#line 139 "C:\Users\Christian\Documents\GitHub\prx\Tools\Parser.frame" //FRAME

    Expect(0);
	}
	
	private readonly bool[,] set = {

#line default //END FRAME -->initialization

		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,T,T,x, x,x,T,T, T,T,T,T, x,T,x,x, T,T,T,T, T,T,T,x, T,T,T,x, T,x,T,T, T,T,T,x, x,x,x,T, T,T,T,T, x,x,T,x, x,x,x,x, x,x,T,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,T,T,T, x,T,x,x, T,T,T,T, T,T,T,x, T,T,T,x, T,x,T,T, T,T,T,x, x,x,x,T, T,T,T,T, x,x,T,x, x,x,x,x, x,x,T,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, T,x,x,T, x,x,x,x, x,x,T,x, x,T,T,x, x,x,T,T, T,T,T,T, x,T,x,x, T,T,T,T, T,T,T,x, T,T,T,x, T,x,T,T, T,T,T,x, x,x,x,T, T,T,T,T, x,x,T,x, x,x,x,x, x,x,T,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,T,x,x, x,T,x,x, x,T,T,T, x,T,T,x, T,T,T,x, T,x,T,T, T,T,T,x, x,x,x,T, T,T,T,T, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x},
		{x,T,T,x, T,x,T,T, T,T,x,x, x,T,x,x, x,x,x,T, T,T,T,x, x,T,x,x, T,x,x,x, x,T,x,x, x,T,x,T, T,x,x,x, x,T,T,T, T,x,x,x, x,T,T,T, x,x,T,x, T,x,x,x, T,x,x,x, x,x,x,x, x,T,T,x, x,x,T,T, x,T,x,T, T,T,T,x, x,x,T,x, x,x,T,x, x,T,T,x, x,T,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,T,T, x,x,T,x, x,x,x,x, x,x,x,x, x,T,x,T, T,x,x,x, x,x,T,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,T,T, T,T,T,T, T,T,T,T, x,T,x,T, T,T,T,T, T,T,T,T, T,T,T,T, T,x,T,x, x,x,x,x, T,x,x,x, x,x,T,T, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, T,T,T,x, x,x,x,x, x,T,x,x, x,x,x,x, x,T,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,T, x,x,x,T, x,T,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,T, x,x,x,x, x,T,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, T,x,T,T, T,T,x,x, x,T,x,x, x,x,x,T, T,T,T,x, x,T,x,x, T,x,x,x, x,T,x,x, x,T,x,T, T,x,x,x, x,T,T,T, T,x,x,x, x,T,T,T, x,x,T,x, T,x,x,x, T,x,x,x, x,x,x,x, x,T,T,T, T,x,T,T, x,T,x,T, T,T,T,x, x,x,T,x, x,x,T,x, x,T,T,x, x,T,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,T, T,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,T,x,x, x,T,x,T, T,x,x,x, x,T,T,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,T,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x},
		{x,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,T,x, x,x,x,x, x,x,x,x, x,T,x,T, x,T,x,T, T,x,x,x, x,T,T,x, x,x,x,x, x,x,T,T, T,x,T,T, T,T,x,x, T,T,T,T, x,x,x,x, T,T,x,T, T,x,T,T, x,T,x,T, T,T,T,T, x,x,T,x, T,T,T,T, x,x,T,x, x,x,x,x},
		{x,T,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,T,x,x, x,T,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,T,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,T,x,T, x,T,x,T, T,x,x,x, x,T,T,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, T,T,T,T, x,x,x,x, T,T,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,T, x,x,T,x, x,x,x,x},
		{x,T,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,T,x,x, x,T,x,T, T,x,x,x, x,T,T,x, x,x,x,x, x,x,T,T, x,x,x,x, T,T,x,x, T,T,T,T, x,x,x,x, T,T,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,T, x,x,T,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,T, x,T,x,T, T,T,T,T, x,x,x,x, T,T,T,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, T,x,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,T,x,x, x,T,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, x,T,x,x, T,x,x,T, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,T,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, x,T,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, x,T,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,T,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, T,x,T,T, T,T,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, T,x,x,x, x,T,T,x, x,x,x,x, x,x,T,T, T,T,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,T,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,T,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, T,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x},
		{x,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,T,x, x,x,x,x, x,x,x,x, x,T,x,T, x,T,x,T, T,x,x,x, x,T,T,x, x,x,x,x, x,x,T,T, T,x,T,T, T,T,x,x, T,T,T,T, x,x,x,x, T,T,x,T, T,x,T,T, x,T,T,T, T,T,T,T, x,x,T,x, T,T,T,T, x,x,T,x, x,x,x,x},
		{T,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,T,x, x,x,x,T, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, x,T,x,x, T,x,x,T, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,T,x, x,x,x,x},
		{x,T,T,x, T,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, T,x,x,x, x,x,T,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,T,T,x, T,x,x,T, T,T,T,x, x,T,T,T, T,x,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,x,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,x, x,x,x,x},
		{x,T,T,x, x,T,T,x, T,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, T,T,x,x, x,T,T,x, T,x,x,T, T,T,T,x, x,T,T,T, T,x,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,T,T,x, T,x,x,T, T,T,T,x, x,T,T,T, T,x,T,T, T,T,x,x, T,T,T,T, T,T,T,T, x,T,T,T, T,T,T,T, T,T,x,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,T,T, x,T,T,x, x,x,x,T, x,x,x,x, x,T,x,T, T,x,x,x, x,T,T,x, x,x,T,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,T,T, x,x,T,x, x,x,x,x, x,x,x,x, x,T,x,T, T,x,x,x, x,T,T,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, T,x,T,T, T,T,x,T, x,T,x,x, x,x,x,T, T,T,T,x, x,T,x,x, T,x,x,x, x,T,x,x, x,T,x,T, T,x,x,x, x,T,T,T, T,x,x,x, x,T,T,T, x,x,T,x, T,x,x,x, T,x,x,x, x,x,x,x, x,T,T,T, T,x,T,T, x,T,x,T, T,T,T,x, x,x,T,x, x,x,T,x, x,T,T,x, x,T,x,x}

#line 144 "C:\Users\Christian\Documents\GitHub\prx\Tools\Parser.frame" //FRAME

	};
} // end Parser

[System.Diagnostics.DebuggerStepThrough]
internal class Errors : System.Collections.Generic.LinkedList<Message> {
    internal Parser parentParser;
  
    internal event EventHandler<MessageEventArgs> MessageReceived;
    protected void OnMessageReceived(Message message)
    {
        var handler = MessageReceived;
        if(handler != null)
            handler(this, new MessageEventArgs(message));
    }

    internal int count 
    {
        get 
        {
            return Count;
        }
    }

	internal void SynErr (int line, int col, int n) {
		string s;
		switch (n) {

#line default //END FRAME -->errors

			case 0: s = "EOF expected"; break;
			case 1: s = "id expected"; break;
			case 2: s = "anyId expected"; break;
			case 3: s = "lid expected"; break;
			case 4: s = "ns expected"; break;
			case 5: s = "version expected"; break;
			case 6: s = "integer expected"; break;
			case 7: s = "real expected"; break;
			case 8: s = "realLike expected"; break;
			case 9: s = "string expected"; break;
			case 10: s = "bitAnd expected"; break;
			case 11: s = "assign expected"; break;
			case 12: s = "comma expected"; break;
			case 13: s = "dec expected"; break;
			case 14: s = "div expected"; break;
			case 15: s = "dot expected"; break;
			case 16: s = "eq expected"; break;
			case 17: s = "gt expected"; break;
			case 18: s = "ge expected"; break;
			case 19: s = "inc expected"; break;
			case 20: s = "lbrace expected"; break;
			case 21: s = "lbrack expected"; break;
			case 22: s = "lpar expected"; break;
			case 23: s = "lt expected"; break;
			case 24: s = "le expected"; break;
			case 25: s = "minus expected"; break;
			case 26: s = "ne expected"; break;
			case 27: s = "bitOr expected"; break;
			case 28: s = "plus expected"; break;
			case 29: s = "pow expected"; break;
			case 30: s = "rbrace expected"; break;
			case 31: s = "rbrack expected"; break;
			case 32: s = "rpar expected"; break;
			case 33: s = "tilde expected"; break;
			case 34: s = "times expected"; break;
			case 35: s = "semicolon expected"; break;
			case 36: s = "colon expected"; break;
			case 37: s = "doublecolon expected"; break;
			case 38: s = "coalescence expected"; break;
			case 39: s = "question expected"; break;
			case 40: s = "pointer expected"; break;
			case 41: s = "implementation expected"; break;
			case 42: s = "at expected"; break;
			case 43: s = "appendleft expected"; break;
			case 44: s = "appendright expected"; break;
			case 45: s = "var expected"; break;
			case 46: s = "ref expected"; break;
			case 47: s = "true expected"; break;
			case 48: s = "false expected"; break;
			case 49: s = "BEGINKEYWORDS expected"; break;
			case 50: s = "mod expected"; break;
			case 51: s = "is expected"; break;
			case 52: s = "as expected"; break;
			case 53: s = "not expected"; break;
			case 54: s = "enabled expected"; break;
			case 55: s = "disabled expected"; break;
			case 56: s = "function expected"; break;
			case 57: s = "command expected"; break;
			case 58: s = "asm expected"; break;
			case 59: s = "declare expected"; break;
			case 60: s = "build expected"; break;
			case 61: s = "return expected"; break;
			case 62: s = "in expected"; break;
			case 63: s = "to expected"; break;
			case 64: s = "add expected"; break;
			case 65: s = "continue expected"; break;
			case 66: s = "break expected"; break;
			case 67: s = "yield expected"; break;
			case 68: s = "or expected"; break;
			case 69: s = "and expected"; break;
			case 70: s = "xor expected"; break;
			case 71: s = "label expected"; break;
			case 72: s = "goto expected"; break;
			case 73: s = "static expected"; break;
			case 74: s = "null expected"; break;
			case 75: s = "if expected"; break;
			case 76: s = "unless expected"; break;
			case 77: s = "else expected"; break;
			case 78: s = "new expected"; break;
			case 79: s = "coroutine expected"; break;
			case 80: s = "from expected"; break;
			case 81: s = "do expected"; break;
			case 82: s = "does expected"; break;
			case 83: s = "while expected"; break;
			case 84: s = "until expected"; break;
			case 85: s = "for expected"; break;
			case 86: s = "foreach expected"; break;
			case 87: s = "try expected"; break;
			case 88: s = "catch expected"; break;
			case 89: s = "finally expected"; break;
			case 90: s = "throw expected"; break;
			case 91: s = "then expected"; break;
			case 92: s = "uusing expected"; break;
			case 93: s = "macro expected"; break;
			case 94: s = "lazy expected"; break;
			case 95: s = "let expected"; break;
			case 96: s = "method expected"; break;
			case 97: s = "this expected"; break;
			case 98: s = "namespace expected"; break;
			case 99: s = "export expected"; break;
			case 100: s = "ENDKEYWORDS expected"; break;
			case 101: s = "LPopExpr expected"; break;
			case 102: s = "??? expected"; break;
			case 103: s = "invalid AsmStatementBlock"; break;
			case 104: s = "invalid AsmInstruction"; break;
			case 105: s = "invalid AsmInstruction"; break;
			case 106: s = "invalid AsmInstruction"; break;
			case 107: s = "invalid AsmInstruction"; break;
			case 108: s = "invalid AsmInstruction"; break;
			case 109: s = "invalid AsmId"; break;
			case 110: s = "invalid SignedReal"; break;
			case 111: s = "invalid Boolean"; break;
			case 112: s = "invalid Id"; break;
			case 113: s = "invalid Expr"; break;
			case 114: s = "invalid AssignExpr"; break;
			case 115: s = "invalid AssignExpr"; break;
			case 116: s = "invalid TypeExpr"; break;
			case 117: s = "invalid GetSetExtension"; break;
			case 118: s = "invalid Primary"; break;
			case 119: s = "invalid Constant"; break;
			case 120: s = "invalid ListLiteral"; break;
			case 121: s = "invalid HashLiteral"; break;
			case 122: s = "invalid LoopExpr"; break;
			case 123: s = "invalid LambdaExpression"; break;
			case 124: s = "invalid LambdaExpression"; break;
			case 125: s = "invalid LazyExpression"; break;
			case 126: s = "invalid GetInitiator"; break;
			case 127: s = "invalid GetInitiator"; break;
			case 128: s = "invalid Real"; break;
			case 129: s = "invalid WhileLoop"; break;
			case 130: s = "invalid WhileLoop"; break;
			case 131: s = "invalid ForLoop"; break;
			case 132: s = "invalid ForLoop"; break;
			case 133: s = "invalid Arguments"; break;
			case 134: s = "invalid Arguments"; break;
			case 135: s = "invalid Statement"; break;
			case 136: s = "invalid ExplicitTypeExpr"; break;
			case 137: s = "invalid PrexoniteTypeExpr"; break;
			case 138: s = "invalid ClrTypeExpr"; break;
			case 139: s = "invalid TypeExprElement"; break;
			case 140: s = "this symbol not expected in DeclarationLevel"; break;
			case 141: s = "invalid DeclarationLevel"; break;
			case 142: s = "invalid GlobalVariableDefinition"; break;
			case 143: s = "invalid GlobalVariableDefinition"; break;
			case 144: s = "invalid MetaAssignment"; break;
			case 145: s = "invalid MetaAssignment"; break;
			case 146: s = "this symbol not expected in Declaration2"; break;
			case 147: s = "invalid Declaration2"; break;
			case 148: s = "invalid Declaration2"; break;
			case 149: s = "this symbol not expected in BuildBlock"; break;
			case 150: s = "invalid FunctionDefinition"; break;
			case 151: s = "invalid FunctionDefinition"; break;
			case 152: s = "invalid FunctionDefinition"; break;
			case 153: s = "invalid FunctionDefinition"; break;
			case 154: s = "invalid NamespaceDeclaration"; break;
			case 155: s = "invalid GlobalId"; break;
			case 156: s = "invalid MetaExpr"; break;
			case 157: s = "invalid MetaExpr"; break;
			case 158: s = "invalid GlobalQualifiedId"; break;
			case 159: s = "invalid Version"; break;
			case 160: s = "invalid SymbolPrefix"; break;
			case 161: s = "invalid EntityFactory"; break;
			case 162: s = "invalid EntityFactory"; break;
			case 163: s = "invalid MExpr"; break;
			case 164: s = "invalid MExpr"; break;
			case 165: s = "invalid MessageDirective"; break;
			case 166: s = "invalid MessageDirective"; break;
			case 167: s = "invalid SymbolDirective"; break;
			case 168: s = "invalid MId"; break;
			case 169: s = "invalid ExplicitLabel"; break;
			case 170: s = "invalid SimpleStatement"; break;
			case 171: s = "invalid StructureStatement"; break;
			case 172: s = "invalid GetSetComplex"; break;
			case 173: s = "invalid Return"; break;
			case 174: s = "invalid Condition"; break;
			case 175: s = "invalid TryCatchFinally"; break;
			case 176: s = "invalid TryCatchFinally"; break;
			case 177: s = "invalid Assignment"; break;
			case 178: s = "invalid VariableDeclaration"; break;

#line 171 "C:\Users\Christian\Documents\GitHub\prx\Tools\Parser.frame" //FRAME

			default: s = "error " + n; break;
		}
		if(s.EndsWith(" expected"))
            s = "after \"" + parentParser.t.ToString(false) + "\", " + s.Replace("expected","is expected") + " and not \"" + parentParser.la.ToString(false) + "\"";
		else if(s.StartsWith("this symbol "))
		    s = "\"" + parentParser.t.val + "\"" + s.Substring(12);
        var msg = Message.Error(s, parentParser.GetPosition(), "E"+n);
		AddLast(msg);
        OnMessageReceived(msg);
	}

  [Obsolete("Use Loader.ReportMessage instead.")]
	internal void SemErr (int line, int col, string s) {
        var msg = Message.Error(s, parentParser.GetPosition() ,null);
		AddLast(msg);
        OnMessageReceived(msg);

	}
	
  [Obsolete("Use Loader.ReportMessage instead.")]
	internal void SemErr (string s) {
        var msg = Message.Error(s, parentParser.GetPosition(),null);
		AddLast(msg);
        OnMessageReceived(msg);
	}
	
  [Obsolete("Use Loader.ReportMessage instead.")]
	internal void Warning (int line, int col, string s) {
        var msg = Message.Warning(s, parentParser.GetPosition(),null);
		AddLast(msg);
        OnMessageReceived(msg);

	}

  [Obsolete("Use Loader.ReportMessage instead.")]
  internal void Info (int line, int col, string s) {
        var msg = Message.Info(s, parentParser.GetPosition(),null);
		AddLast(msg);
        OnMessageReceived(msg);
	}
	
  [Obsolete("Use Loader.ReportMessage instead.")]
	internal void Warning(string s) {
        var msg = Message.Warning(s, parentParser.GetPosition(),null);
		AddLast(msg);
        OnMessageReceived(msg);
	}

  [Obsolete("Use Loader.ReportMessage instead.")]
    internal void Info(string s) {
        var msg = Message.Info(s, parentParser.GetPosition(),null);
		AddLast(msg);
        OnMessageReceived(msg);
	}

    public int GetErrorCount() 
    {
        return System.Linq.Enumerable.Count(System.Linq.Enumerable.Where(this, pm => pm.Severity == MessageSeverity.Error));
    }

    public int GetWarningCount() 
    {
         return System.Linq.Enumerable.Count(System.Linq.Enumerable.Where(this, pm => pm.Severity == MessageSeverity.Warning));
    }
} // Errors


#line default //END FRAME $$$

}