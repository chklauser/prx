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


#line 27 "C:\Cold\Users\Christian\Documents\GitHub\prx\Tools\Parser.frame" //FRAME

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


#line 44 "C:\Cold\Users\Christian\Documents\GitHub\prx\Tools\Parser.frame" //FRAME


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
	public const int _timessym = 35;
	public const int _semicolon = 36;
	public const int _colon = 37;
	public const int _doublecolon = 38;
	public const int _coalescence = 39;
	public const int _question = 40;
	public const int _pointer = 41;
	public const int _implementation = 42;
	public const int _at = 43;
	public const int _appendleft = 44;
	public const int _appendright = 45;
	public const int _var = 46;
	public const int _ref = 47;
	public const int _true = 48;
	public const int _false = 49;
	public const int _BEGINKEYWORDS = 50;
	public const int _mod = 51;
	public const int _is = 52;
	public const int _as = 53;
	public const int _not = 54;
	public const int _enabled = 55;
	public const int _disabled = 56;
	public const int _function = 57;
	public const int _command = 58;
	public const int _asm = 59;
	public const int _declare = 60;
	public const int _build = 61;
	public const int _return = 62;
	public const int _in = 63;
	public const int _to = 64;
	public const int _add = 65;
	public const int _continue = 66;
	public const int _break = 67;
	public const int _yield = 68;
	public const int _or = 69;
	public const int _and = 70;
	public const int _xor = 71;
	public const int _label = 72;
	public const int _goto = 73;
	public const int _static = 74;
	public const int _null = 75;
	public const int _if = 76;
	public const int _unless = 77;
	public const int _else = 78;
	public const int _new = 79;
	public const int _coroutine = 80;
	public const int _from = 81;
	public const int _do = 82;
	public const int _does = 83;
	public const int _while = 84;
	public const int _until = 85;
	public const int _for = 86;
	public const int _foreach = 87;
	public const int _try = 88;
	public const int _catch = 89;
	public const int _finally = 90;
	public const int _throw = 91;
	public const int _then = 92;
	public const int _uusing = 93;
	public const int _macro = 94;
	public const int _lazy = 95;
	public const int _let = 96;
	public const int _method = 97;
	public const int _this = 98;
	public const int _namespace = 99;
	public const int _export = 100;
	public const int _ENDKEYWORDS = 101;
	public const int _LPopExpr = 102;
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
		@timessym = 35,
		@semicolon = 36,
		@colon = 37,
		@doublecolon = 38,
		@coalescence = 39,
		@question = 40,
		@pointer = 41,
		@implementation = 42,
		@at = 43,
		@appendleft = 44,
		@appendright = 45,
		@var = 46,
		@ref = 47,
		@true = 48,
		@false = 49,
		@BEGINKEYWORDS = 50,
		@mod = 51,
		@is = 52,
		@as = 53,
		@not = 54,
		@enabled = 55,
		@disabled = 56,
		@function = 57,
		@command = 58,
		@asm = 59,
		@declare = 60,
		@build = 61,
		@return = 62,
		@in = 63,
		@to = 64,
		@add = 65,
		@continue = 66,
		@break = 67,
		@yield = 68,
		@or = 69,
		@and = 70,
		@xor = 71,
		@label = 72,
		@goto = 73,
		@static = 74,
		@null = 75,
		@if = 76,
		@unless = 77,
		@else = 78,
		@new = 79,
		@coroutine = 80,
		@from = 81,
		@do = 82,
		@does = 83,
		@while = 84,
		@until = 85,
		@for = 86,
		@foreach = 87,
		@try = 88,
		@catch = 89,
		@finally = 90,
		@throw = 91,
		@then = 92,
		@uusing = 93,
		@macro = 94,
		@lazy = 95,
		@let = 96,
		@method = 97,
		@this = 98,
		@namespace = 99,
		@export = 100,
		@ENDKEYWORDS = 101,
		@LPopExpr = 102,
	}
	const int maxT = 103;

#line 48 "C:\Cold\Users\Christian\Documents\GitHub\prx\Tools\Parser.frame" //FRAME

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

#line 60 "C:\Cold\Users\Christian\Documents\GitHub\prx\Tools\Parser.frame" //FRAME


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


#line 94 "C:\Cold\Users\Christian\Documents\GitHub\prx\Tools\Parser.frame" //FRAME

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
		} else SynErr(104);
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
		ISourcePosition pos;
		
		if (la.kind == _var || la.kind == _ref) {
			/*Parser.Assembler.atg:51*/var isAutodereference = false; 
			if (la.kind == _var) {
				Get();
			} else {
				Get();
				/*Parser.Assembler.atg:52*/isAutodereference = true; 
			}
			/*Parser.Assembler.atg:53*/pos = GetPosition(); 
			AsmId(/*Parser.Assembler.atg:54*/out id);
			/*Parser.Assembler.atg:57*/_ensureDefinedLocal(id,id,isAutodereference,pos,false);
			
			while (la.kind == _comma) {
				Get();
				/*Parser.Assembler.atg:59*/pos = GetPosition(); 
				AsmId(/*Parser.Assembler.atg:60*/out id);
				/*Parser.Assembler.atg:62*/_ensureDefinedLocal(id,id,isAutodereference,pos,false);
				
			}
		} else if (/*Parser.Assembler.atg:67*/isInOpAliasGroup()) {
			AsmId(/*Parser.Assembler.atg:67*/out insbase);
			if (la.kind == _dot) {
				Get();
				AsmId(/*Parser.Assembler.atg:68*/out detail);
			}
			/*Parser.Assembler.atg:69*/addOpAlias(block, insbase, detail); 
		} else if (/*Parser.Assembler.atg:72*/isInNullGroup()) {
			AsmId(/*Parser.Assembler.atg:72*/out insbase);
			if (la.kind == _dot) {
				Get();
				AsmId(/*Parser.Assembler.atg:73*/out detail);
			}
			/*Parser.Assembler.atg:74*/code = getOpCode(insbase, detail);
			addInstruction(block, new Instruction(code));
			
		} else if (/*Parser.Assembler.atg:80*/isAsmInstruction("label",null) ) {
			AsmId(/*Parser.Assembler.atg:80*/out insbase);
			AsmId(/*Parser.Assembler.atg:83*/out id);
			/*Parser.Assembler.atg:84*/addLabel(block, id); 
		} else if (/*Parser.Assembler.atg:87*/isAsmInstruction("nop", null)) {
			AsmId(/*Parser.Assembler.atg:87*/out insbase);
			/*Parser.Assembler.atg:87*/var ins = new Instruction(OpCode.nop); 
			if (la.kind == _plus) {
				Get();
				AsmId(/*Parser.Assembler.atg:88*/out id);
				/*Parser.Assembler.atg:88*/ins = ins.With(id: id); 
			}
			/*Parser.Assembler.atg:90*/addInstruction(block, ins); 
		} else if (/*Parser.Assembler.atg:94*/isAsmInstruction("rot", null)) {
			AsmId(/*Parser.Assembler.atg:94*/out insbase);
			Expect(_dot);
			Integer(/*Parser.Assembler.atg:95*/out rotations);
			Expect(_comma);
			Integer(/*Parser.Assembler.atg:96*/out values);
			/*Parser.Assembler.atg:98*/addInstruction(block, Instruction.CreateRotate(rotations, values)); 
		} else if (/*Parser.Assembler.atg:102*/isAsmInstruction("indloci", null)) {
			if (la.kind == _at) {
				Get();
				/*Parser.Assembler.atg:102*/justEffect = true; 
			}
			AsmId(/*Parser.Assembler.atg:104*/out insbase);
			Expect(_dot);
			Integer(/*Parser.Assembler.atg:105*/out arguments);
			Integer(/*Parser.Assembler.atg:106*/out index);
			/*Parser.Assembler.atg:108*/addInstruction(block, Instruction.CreateIndLocI(index, arguments, justEffect)); 
		} else if (/*Parser.Assembler.atg:111*/isAsmInstruction("swap", null)) {
			AsmId(/*Parser.Assembler.atg:111*/out insbase);
			/*Parser.Assembler.atg:112*/addInstruction(block, Instruction.CreateExchange()); 
		} else if (/*Parser.Assembler.atg:117*/isAsmInstruction("ldc", "real")) {
			AsmId(/*Parser.Assembler.atg:117*/out insbase);
			Expect(_dot);
			AsmId(/*Parser.Assembler.atg:119*/out detail);
			SignedReal(/*Parser.Assembler.atg:120*/out dblArg);
			/*Parser.Assembler.atg:121*/addInstruction(block, Instruction.CreateConstant(dblArg)); 
		} else if (/*Parser.Assembler.atg:126*/isAsmInstruction("ldc", "bool")) {
			AsmId(/*Parser.Assembler.atg:126*/out insbase);
			Expect(_dot);
			AsmId(/*Parser.Assembler.atg:128*/out detail);
			Boolean(/*Parser.Assembler.atg:129*/out bolArg);
			/*Parser.Assembler.atg:130*/addInstruction(block, Instruction.CreateConstant(bolArg)); 
		} else if (/*Parser.Assembler.atg:135*/isInIntegerGroup()) {
			AsmId(/*Parser.Assembler.atg:135*/out insbase);
			if (la.kind == _dot) {
				Get();
				AsmId(/*Parser.Assembler.atg:136*/out detail);
			}
			SignedInteger(/*Parser.Assembler.atg:137*/out arguments);
			/*Parser.Assembler.atg:138*/code = getOpCode(insbase, detail);
			addInstruction(block, new Instruction(code, arguments));
			
		} else if (/*Parser.Assembler.atg:144*/isInJumpGroup()) {
			AsmId(/*Parser.Assembler.atg:144*/out insbase);
			if (la.kind == _dot) {
				Get();
				AsmId(/*Parser.Assembler.atg:145*/out detail);
			}
			/*Parser.Assembler.atg:146*/Instruction ins = null;
			code = getOpCode(insbase, detail);
			
			if (StartOf(2)) {
				AsmId(/*Parser.Assembler.atg:150*/out id);
				/*Parser.Assembler.atg:152*/ins = new Instruction(code, -1, id);
				
			} else if (la.kind == _integer) {
				Integer(/*Parser.Assembler.atg:154*/out arguments);
				/*Parser.Assembler.atg:154*/ins = new Instruction(code, arguments); 
			} else SynErr(105);
			/*Parser.Assembler.atg:155*/addInstruction(block, ins); 
		} else if (/*Parser.Assembler.atg:160*/isInIdGroup()) {
			AsmId(/*Parser.Assembler.atg:160*/out insbase);
			if (la.kind == _dot) {
				Get();
				AsmId(/*Parser.Assembler.atg:161*/out detail);
			}
			AsmId(/*Parser.Assembler.atg:162*/out id);
			/*Parser.Assembler.atg:163*/code = getOpCode(insbase, detail);
			addInstruction(block, new Instruction(code, id));
			
		} else if (/*Parser.Assembler.atg:170*/isInIdArgGroup()) {
			if (la.kind == _at) {
				Get();
				/*Parser.Assembler.atg:170*/justEffect = true; 
			}
			AsmId(/*Parser.Assembler.atg:172*/out insbase);
			if (la.kind == _dot) {
				Get();
				Integer(/*Parser.Assembler.atg:173*/out arguments);
			} else if (StartOf(2)) {
				/*Parser.Assembler.atg:174*/arguments = 0; 
			} else SynErr(106);
			AsmId(/*Parser.Assembler.atg:176*/out id);
			/*Parser.Assembler.atg:177*/code = getOpCode(insbase, null);
			addInstruction(block, new Instruction(code, arguments, id, justEffect));
			
		} else if (/*Parser.Assembler.atg:183*/isInArgGroup()) {
			if (la.kind == _at) {
				Get();
				/*Parser.Assembler.atg:183*/justEffect = true; 
			}
			AsmId(/*Parser.Assembler.atg:185*/out insbase);
			if (la.kind == _dot) {
				Get();
				Integer(/*Parser.Assembler.atg:186*/out arguments);
			} else if (StartOf(3)) {
				/*Parser.Assembler.atg:187*/arguments = 0; 
			} else SynErr(107);
			/*Parser.Assembler.atg:189*/code = getOpCode(insbase, null);
			addInstruction(block, new Instruction(code, arguments, null, justEffect));
			
		} else if (/*Parser.Assembler.atg:195*/isInQualidArgGroup()) {
			if (la.kind == _at) {
				Get();
				/*Parser.Assembler.atg:195*/justEffect = true; 
			}
			AsmId(/*Parser.Assembler.atg:197*/out insbase);
			if (la.kind == _dot) {
				Get();
				Integer(/*Parser.Assembler.atg:198*/out arguments);
			} else if (StartOf(2)) {
				/*Parser.Assembler.atg:199*/arguments = 0; 
			} else SynErr(108);
			AsmQualid(/*Parser.Assembler.atg:201*/out id);
			/*Parser.Assembler.atg:202*/code = getOpCode(insbase, null);
			addInstruction(block, new Instruction(code, arguments, id, justEffect));
			
		} else if (StartOf(2)) {
			AsmId(/*Parser.Assembler.atg:207*/out insbase);
			/*Parser.Assembler.atg:207*/SemErr("Invalid assembler instruction \"" + insbase + "\" (" + t + ")."); 
		} else SynErr(109);
	}

	void AsmId(/*Parser.Assembler.atg:211*/out string id) {
		/*Parser.Assembler.atg:211*/id = "\\NoId\\"; 
		if (la.kind == _string) {
			String(/*Parser.Assembler.atg:213*/out id);
		} else if (StartOf(4)) {
			Id(/*Parser.Assembler.atg:214*/out id);
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
			/*Parser.Assembler.atg:244*/id = cache(t.val); 
		} else SynErr(110);
	}

	void Integer(/*Parser.Helper.atg:42*/out int value) {
		Expect(_integer);
		/*Parser.Helper.atg:43*/if(!TryParseInteger(t.val, out value))
		   SemErr(t, "Cannot recognize integer " + t.val);
		
	}

	void SignedReal(/*Parser.Helper.atg:74*/out double value) {
		/*Parser.Helper.atg:74*/value = 0.0; double modifier = 1.0; int ival; 
		if (la.kind == _minus || la.kind == _plus) {
			if (la.kind == _plus) {
				Get();
			} else {
				Get();
				/*Parser.Helper.atg:77*/modifier = -1.0; 
			}
		}
		if (la.kind == _real || la.kind == _realLike) {
			Real(/*Parser.Helper.atg:78*/out value);
		} else if (la.kind == _integer) {
			Integer(/*Parser.Helper.atg:79*/out ival);
			/*Parser.Helper.atg:79*/value = ival; 
		} else SynErr(111);
		/*Parser.Helper.atg:81*/value = modifier * value; 
	}

	void Boolean(/*Parser.Helper.atg:35*/out bool value) {
		/*Parser.Helper.atg:35*/value = true; 
		if (la.kind == _true) {
			Get();
		} else if (la.kind == _false) {
			Get();
			/*Parser.Helper.atg:38*/value = false; 
		} else SynErr(112);
	}

	void SignedInteger(/*Parser.Helper.atg:48*/out int value) {
		/*Parser.Helper.atg:48*/int modifier = 1; 
		if (la.kind == _minus || la.kind == _plus) {
			if (la.kind == _plus) {
				Get();
			} else {
				Get();
				/*Parser.Helper.atg:51*/modifier = -1; 
			}
		}
		Integer(/*Parser.Helper.atg:52*/out value);
		/*Parser.Helper.atg:52*/value = modifier * value; 
	}

	void AsmQualid(/*Parser.Assembler.atg:248*/out string qualid) {
		
		AsmId(/*Parser.Assembler.atg:250*/out qualid);
	}

	void String(/*Parser.Helper.atg:85*/out string value) {
		Expect(_string);
		/*Parser.Helper.atg:86*/value = cache(t.val); 
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
			} else {
				Get();
			}
			/*Parser.Helper.atg:32*/id = cache(t.val); 
		} else SynErr(113);
	}

	void Expr(/*Parser.Expression.atg:26*/out AstExpr expr) {
		/*Parser.Expression.atg:26*/AstConditionalExpression cexpr; expr = _NullNode(GetPosition()); 
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
		} else SynErr(114);
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

	void GetCall(/*Parser.Statement.atg:473*/out AstGetSet complex) {
		/*Parser.Statement.atg:473*/AstGetSet getMember = null; 
		AstExpr expr;
		
		GetInitiator(/*Parser.Statement.atg:477*/out expr);
		/*Parser.Statement.atg:478*/complex = expr as AstGetSet;
		if(complex == null)
		{
		    var pos = GetPosition();
		    Loader.ReportMessage(Message.Error("Expected an LValue (Get/Set-Complex) for ++,-- or assignment statement.",pos,MessageClasses.LValueExpected));
		    complex = Create.IndirectCall(pos,Create.Null(pos));
		}  
		
		while (la.kind == _dot || la.kind == _lbrack) {
			GetSetExtension(/*Parser.Statement.atg:486*/complex, out getMember);
			/*Parser.Statement.atg:487*/complex = getMember; 
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
			} else SynErr(115);
			/*Parser.Expression.atg:217*/assignment.Arguments.Add(expr); 
			if(setModifier == BinaryOperator.None)
			    expr = assignment;
			else
			    expr = Create.ModifyingAssignment(position,assignment,setModifier);
			
		} else if (StartOf(10)) {
		} else SynErr(116);
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
		} else SynErr(117);
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

	void GetSetExtension(/*Parser.Statement.atg:127*/AstExpr subject, out AstGetSet extension) {
		/*Parser.Statement.atg:127*/extension = null; string id;
		if(subject == null)
		{
			SemErr("Member access not preceded by a proper expression.");
			subject = new AstConstant(this,null);
		}
		                             
		if (/*Parser.Statement.atg:137*/isIndirectCall() ) {
			Expect(_dot);
			/*Parser.Statement.atg:137*/extension = new AstIndirectCall(this, PCall.Get, subject); 
			Arguments(/*Parser.Statement.atg:138*/extension.Arguments);
		} else if (la.kind == _dot) {
			Get();
			Id(/*Parser.Statement.atg:140*/out id);
			/*Parser.Statement.atg:140*/var ns = subject as AstNamespaceUsage;
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
			
			Arguments(/*Parser.Statement.atg:158*/extension.Arguments);
		} else if (la.kind == _lbrack) {
			/*Parser.Statement.atg:160*/AstExpr expr; 
			extension = new AstGetSetMemberAccess(this, PCall.Get, subject, ""); 
			
			Get();
			if (StartOf(14)) {
				Expr(/*Parser.Statement.atg:164*/out expr);
				/*Parser.Statement.atg:164*/extension.Arguments.Add(expr); 
				while (WeakSeparator(_comma,14,15) ) {
					Expr(/*Parser.Statement.atg:165*/out expr);
					/*Parser.Statement.atg:165*/extension.Arguments.Add(expr); 
				}
			}
			Expect(_rbrack);
		} else SynErr(118);
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
		} else SynErr(119);
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
		} else SynErr(120);
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
				} else SynErr(121);
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
				} else SynErr(122);
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
		} else SynErr(123);
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
		} else SynErr(124);
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
		} else SynErr(125);
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
		} else SynErr(126);
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

	void GetInitiator(/*Parser.Statement.atg:172*/out AstExpr complex) {
		/*Parser.Statement.atg:172*/complex = null; 
		AstGetSet actualComplex = null;
		AstGetSetStatic staticCall = null;
		AstGetSet member = null;
		AstExpr expr;
		List<AstExpr> args = new List<AstExpr>();
		string id;
		int placeholderIndex = -1;
		
		if (StartOf(21)) {
			if (StartOf(4)) {
				SymbolicUsage(/*Parser.Statement.atg:183*/out actualComplex);
				/*Parser.Statement.atg:184*/complex = actualComplex; 
			} else if (StartOf(22)) {
				VariableDeclaration(/*Parser.Statement.atg:185*/out actualComplex);
				/*Parser.Statement.atg:186*/complex = actualComplex; 
			} else if (la.kind == _ns || la.kind == _tilde || la.kind == _doublecolon) {
				StaticCall(/*Parser.Statement.atg:187*/out staticCall);
			} else {
				Get();
				Expr(/*Parser.Statement.atg:188*/out expr);
				/*Parser.Statement.atg:188*/args.Add(expr); 
				while (la.kind == _comma) {
					Get();
					Expr(/*Parser.Statement.atg:189*/out expr);
					/*Parser.Statement.atg:189*/args.Add(expr); 
				}
				Expect(_rpar);
				if (la.kind == _dot || la.kind == _lbrack) {
					GetSetExtension(/*Parser.Statement.atg:192*/expr, out member);
					/*Parser.Statement.atg:193*/if(args.Count > 1)
					SemErr("A member access cannot have multiple subjects. (Did you mean '>>'?)");
					
				} else if (la.kind == _appendright) {
					Get();
					GetCall(/*Parser.Statement.atg:197*/out actualComplex);
					/*Parser.Statement.atg:197*/_appendRight(args,actualComplex);
					        complex = actualComplex;
					
				} else SynErr(127);
			}
			/*Parser.Statement.atg:202*/complex =  
			staticCall ?? 
			member ??
			complex; 
			
		} else if (la.kind == _pointer) {
			Get();
			/*Parser.Statement.atg:208*/var ptrCount = 1; 
			while (la.kind == _pointer) {
				Get();
				/*Parser.Statement.atg:209*/ptrCount++; 
			}
			Id(/*Parser.Statement.atg:211*/out id);
			/*Parser.Statement.atg:211*/complex = _assembleReference(id, ptrCount); 
		} else if (la.kind == _question) {
			Get();
			if (la.kind == _integer) {
				Integer(/*Parser.Statement.atg:213*/out placeholderIndex);
			}
			/*Parser.Statement.atg:213*/complex = new AstPlaceholder(this, 0 <= placeholderIndex ? (int?)placeholderIndex : null); 
		} else SynErr(128);
	}

	void Real(/*Parser.Helper.atg:56*/out double value) {
		if (la.kind == _real) {
			Get();
		} else if (la.kind == _realLike) {
			Get();
		} else SynErr(129);
		/*Parser.Helper.atg:57*/string real = t.val;
		if(!TryParseReal(real, out value))
		    SemErr(t, "Cannot recognize real " + real);
		
	}

	void Null() {
		Expect(_null);
	}

	void WhileLoop(/*Parser.Statement.atg:400*/AstBlock block) {
		/*Parser.Statement.atg:400*/AstWhileLoop loop = new AstWhileLoop(GetPosition(),CurrentBlock); 
		if (la.kind == _while || la.kind == _until) {
			if (la.kind == _while) {
				Get();
			} else {
				Get();
				/*Parser.Statement.atg:402*/loop.IsPositive = false; 
			}
			Expect(_lpar);
			Expr(/*Parser.Statement.atg:404*/out loop.Condition);
			Expect(_rpar);
			/*Parser.Statement.atg:405*/_PushScope(loop.Block); //EndBlock is common for both loops
			
			StatementBlock(/*Parser.Statement.atg:407*/loop.Block);
		} else if (la.kind == _do) {
			Get();
			/*Parser.Statement.atg:409*/_PushScope(loop.Block); 
			loop.IsPrecondition = false;
			
			StatementBlock(/*Parser.Statement.atg:412*/loop.Block);
			if (la.kind == _while) {
				Get();
			} else if (la.kind == _until) {
				Get();
				/*Parser.Statement.atg:413*/loop.IsPositive = false; 
			} else SynErr(130);
			Expect(_lpar);
			Expr(/*Parser.Statement.atg:415*/out loop.Condition);
			Expect(_rpar);
		} else SynErr(131);
		/*Parser.Statement.atg:416*/_PopScope(loop.Block); block.Add(loop); 
	}

	void ForLoop(/*Parser.Statement.atg:419*/AstBlock block) {
		/*Parser.Statement.atg:419*/AstForLoop loop;
		AstExpr condition;
		                       
		Expect(_for);
		/*Parser.Statement.atg:423*/loop = new AstForLoop(GetPosition(), CurrentBlock); 
		_PushScope(loop.Initialize);
		
		Expect(_lpar);
		StatementBlock(/*Parser.Statement.atg:426*/loop.Initialize);
		if (la.kind == _do) {
			/*Parser.Statement.atg:428*/_PushScope(loop.NextIteration); 
			
			Get();
			StatementBlock(/*Parser.Statement.atg:430*/loop.NextIteration);
			/*Parser.Statement.atg:431*/loop.IsPrecondition = false; 
			if (la.kind == _while) {
				Get();
			} else if (la.kind == _until) {
				Get();
				/*Parser.Statement.atg:433*/loop.IsPositive = false; 
			} else SynErr(132);
			/*Parser.Statement.atg:434*/_PopScope(loop.NextIteration); 
			Expr(/*Parser.Statement.atg:435*/out condition);
			/*Parser.Statement.atg:435*/loop.Condition = condition; 
			_PushScope(loop.NextIteration);
			
		} else if (StartOf(14)) {
			if (la.kind == _while || la.kind == _until) {
				if (la.kind == _while) {
					Get();
				} else {
					Get();
					/*Parser.Statement.atg:439*/loop.IsPositive = false; 
				}
			}
			Expr(/*Parser.Statement.atg:441*/out condition);
			/*Parser.Statement.atg:441*/loop.Condition = condition; 
			Expect(_semicolon);
			/*Parser.Statement.atg:442*/_PushScope(loop.NextIteration); 
			SimpleStatement(/*Parser.Statement.atg:443*/loop.NextIteration);
			if (la.kind == _semicolon) {
				Get();
			}
		} else SynErr(133);
		Expect(_rpar);
		/*Parser.Statement.atg:446*/_PushScope(loop.Block); 
		StatementBlock(/*Parser.Statement.atg:447*/loop.Block);
		/*Parser.Statement.atg:447*/_PopScope(loop.Block);
		_PopScope(loop.NextIteration);
		_PopScope(loop.Initialize);
		block.Add(loop);
		
	}

	void ForeachLoop(/*Parser.Statement.atg:455*/AstBlock block) {
		Expect(_foreach);
		/*Parser.Statement.atg:456*/AstForeachLoop loop = Create.ForeachLoop(GetPosition());
		_PushScope(loop.Block);
		
		Expect(_lpar);
		GetCall(/*Parser.Statement.atg:460*/out loop.Element);
		Expect(_in);
		Expr(/*Parser.Statement.atg:462*/out loop.List);
		Expect(_rpar);
		StatementBlock(/*Parser.Statement.atg:464*/loop.Block);
		/*Parser.Statement.atg:465*/_PopScope(loop.Block);
		block.Add(loop); 
		
	}

	void Arguments(/*Parser.Statement.atg:666*/ArgumentsProxy args) {
		/*Parser.Statement.atg:667*/AstExpr expr;
		                          bool missingArg = false;
		                      
		if (la.kind == _lpar) {
			Get();
			if (StartOf(14)) {
				Expr(/*Parser.Statement.atg:673*/out expr);
				/*Parser.Statement.atg:673*/args.Add(expr); 
				while (la.kind == _comma) {
					Get();
					/*Parser.Statement.atg:674*/if(missingArg)
					              SemErr("Missing argument expression (two consecutive commas)");
					      
					if (StartOf(14)) {
						Expr(/*Parser.Statement.atg:677*/out expr);
						/*Parser.Statement.atg:678*/args.Add(expr);
						missingArg = false;
						
					} else if (la.kind == _comma || la.kind == _rpar) {
						/*Parser.Statement.atg:681*/missingArg = true; 
					} else SynErr(134);
				}
			}
			Expect(_rpar);
		}
		/*Parser.Statement.atg:687*/args.RememberRightAppendPosition(); 
		if (la.kind == _appendleft) {
			Get();
			if (/*Parser.Statement.atg:692*/la.kind == _lpar && (!isLambdaExpression())) {
				Expect(_lpar);
				if (StartOf(14)) {
					Expr(/*Parser.Statement.atg:693*/out expr);
					/*Parser.Statement.atg:693*/args.Add(expr); 
					while (la.kind == _comma) {
						Get();
						Expr(/*Parser.Statement.atg:695*/out expr);
						/*Parser.Statement.atg:696*/args.Add(expr); 
					}
				}
				Expect(_rpar);
			} else if (StartOf(14)) {
				Expr(/*Parser.Statement.atg:700*/out expr);
				/*Parser.Statement.atg:700*/args.Add(expr); 
			} else SynErr(135);
		}
	}

	void FormalArg(/*Parser.GlobalScope.atg:876*/CompilerTarget ft) {
		/*Parser.GlobalScope.atg:876*/string id; 
		bool isAutodereferenced = false; 
		ISourcePosition idPos;
		
		if (la.kind == _var || la.kind == _ref) {
			if (la.kind == _var) {
				Get();
			} else {
				Get();
				/*Parser.GlobalScope.atg:881*/isAutodereferenced = true; 
			}
		}
		/*Parser.GlobalScope.atg:882*/idPos = GetPosition(); 
		Id(/*Parser.GlobalScope.atg:883*/out id);
		/*Parser.GlobalScope.atg:885*/ft.Function.Parameters.Add(id); 
		var sym = Symbol.CreateDereference(Symbol.CreateReference(
		    EntityRef.Variable.Local.Create(id),idPos),idPos);
		if(isAutodereferenced)
		    sym = Symbol.CreateDereference(sym,idPos);
		ft.Symbols.Declare(id, sym);
		
	}

	void Statement(/*Parser.Statement.atg:31*/AstBlock block) {
		if (/*Parser.Statement.atg:33*/isLabel() ) {
			ExplicitLabel(/*Parser.Statement.atg:33*/block);
		} else if (StartOf(23)) {
			if (StartOf(24)) {
				SimpleStatement(/*Parser.Statement.atg:34*/block);
			}
			Expect(_semicolon);
		} else if (StartOf(25)) {
			StructureStatement(/*Parser.Statement.atg:35*/block);
		} else SynErr(136);
		while (la.kind == _and) {
			Get();
			Statement(/*Parser.Statement.atg:37*/block);
		}
	}

	void ExplicitTypeExpr(/*Parser.Expression.atg:488*/out AstTypeExpr type) {
		/*Parser.Expression.atg:488*/type = null; 
		if (la.kind == _tilde) {
			Get();
			PrexoniteTypeExpr(/*Parser.Expression.atg:490*/out type);
		} else if (la.kind == _ns || la.kind == _doublecolon) {
			ClrTypeExpr(/*Parser.Expression.atg:491*/out type);
		} else SynErr(137);
	}

	void PrexoniteTypeExpr(/*Parser.Expression.atg:516*/out AstTypeExpr type) {
		/*Parser.Expression.atg:516*/string id = null; 
		if (StartOf(4)) {
			Id(/*Parser.Expression.atg:518*/out id);
		} else if (la.kind == _null) {
			Get();
			/*Parser.Expression.atg:518*/id = NullPType.Literal; 
		} else SynErr(138);
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
		} else SynErr(139);
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
		} else SynErr(140);
	}

	void Prexonite() {
		DeclarationLevel();
		Expect(_EOF);
	}

	void DeclarationLevel() {
		/*Parser.GlobalScope.atg:32*/PFunction func; 
		while (StartOf(27)) {
			if (StartOf(28)) {
				if (StartOf(29)) {
					if (la.kind == _var || la.kind == _ref) {
						/*Parser.GlobalScope.atg:34*/if(PreflightModeEnabled)
						{
						    ViolentlyAbortParse();
						    return;
						} 
						
						GlobalVariableDefinition();
					} else {
						MetaAssignment(/*Parser.GlobalScope.atg:41*/TargetApplication);
					}
				}
				while (!(la.kind == _EOF || la.kind == _semicolon)) {SynErr(141); Get();}
				Expect(_semicolon);
			} else {
				/*Parser.GlobalScope.atg:43*/if(PreflightModeEnabled)
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
					FunctionDefinition(/*Parser.GlobalScope.atg:52*/out func);
				} else if (la.kind == _namespace) {
					NamespaceDeclaration();
				} else SynErr(142);
			}
		}
	}

	void GlobalVariableDefinition() {
		/*Parser.GlobalScope.atg:130*/string id = null; 
		List<string> aliases = new List<string>();
		string primaryAlias = null;
		VariableDeclaration vari; 
		bool isAutodereferenced = false;
		Symbol entry;
		
		if (la.kind == _var) {
			Get();
		} else if (la.kind == _ref) {
			Get();
			/*Parser.GlobalScope.atg:140*/isAutodereferenced = true; 
		} else SynErr(143);
		/*Parser.GlobalScope.atg:141*/var position = GetPosition(); 
		if (la.kind == _id || la.kind == _anyId) {
			GlobalId(/*Parser.GlobalScope.atg:143*/out id);
			/*Parser.GlobalScope.atg:143*/primaryAlias = id; 
			if (la.kind == _as) {
				GlobalVariableAliasList(/*Parser.GlobalScope.atg:144*/aliases);
			}
		} else if (la.kind == _as) {
			GlobalVariableAliasList(/*Parser.GlobalScope.atg:145*/aliases);
			/*Parser.GlobalScope.atg:146*/id = null; 
		} else SynErr(144);
		/*Parser.GlobalScope.atg:147*/id = _assignPhysicalGlobalVariableSlot(id);
		entry = Symbol.CreateDereference(Symbol.CreateReference(
		  EntityRef.Variable.Global.Create(id, TargetModule.Name), position),position);
		if(isAutodereferenced)
		{
		  entry = Symbol.CreateDereference(entry, position);
		}
		foreach(var alias in aliases)
		    Symbols.Declare(alias, entry);
		DefineGlobalVariable(id,out vari);
		
		if (la.kind == _lbrack) {
			Get();
			if (StartOf(31)) {
				MetaAssignment(/*Parser.GlobalScope.atg:159*/vari);
				while (la.kind == _semicolon) {
					Get();
					if (StartOf(31)) {
						MetaAssignment(/*Parser.GlobalScope.atg:161*/vari);
					}
				}
			}
			Expect(_rbrack);
		}
		/*Parser.GlobalScope.atg:165*/if(primaryAlias != null && !_suppressPrimarySymbol(vari))
		     Symbols.Declare(primaryAlias, entry);
		
		if (la.kind == _assign) {
			/*Parser.GlobalScope.atg:168*/_pushLexerState(Lexer.Local); 
			Get();
			/*Parser.GlobalScope.atg:169*/_PushScope(FunctionTargets[Application.InitializationId]);
			AstExpr expr;
			
			Expr(/*Parser.GlobalScope.atg:172*/out expr);
			/*Parser.GlobalScope.atg:173*/_popLexerState();
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

	void MetaAssignment(/*Parser.GlobalScope.atg:60*/IHasMetaTable metaTable) {
		/*Parser.GlobalScope.atg:60*/string key = null; MetaEntry entry = null; 
		if (la.kind == _is) {
			Get();
			/*Parser.GlobalScope.atg:62*/entry = true; 
			if (la.kind == _not) {
				Get();
				/*Parser.GlobalScope.atg:63*/entry = false; 
			}
			GlobalId(/*Parser.GlobalScope.atg:65*/out key);
		} else if (la.kind == _not) {
			Get();
			/*Parser.GlobalScope.atg:66*/entry = false; 
			GlobalId(/*Parser.GlobalScope.atg:67*/out key);
		} else if (la.kind == _id || la.kind == _anyId) {
			GlobalId(/*Parser.GlobalScope.atg:69*/out key);
			if (la.kind == _enabled) {
				Get();
				/*Parser.GlobalScope.atg:70*/entry = true; 
			} else if (la.kind == _disabled) {
				Get();
				/*Parser.GlobalScope.atg:71*/entry = false; 
			} else if (StartOf(32)) {
				MetaExpr(/*Parser.GlobalScope.atg:72*/out entry);
			} else if (la.kind == _rbrack || la.kind == _semicolon) {
				/*Parser.GlobalScope.atg:73*/entry = true; 
			} else SynErr(145);
		} else if (la.kind == _add) {
			Get();
			/*Parser.GlobalScope.atg:75*/MetaEntry subEntry; 
			MetaExpr(/*Parser.GlobalScope.atg:76*/out subEntry);
			/*Parser.GlobalScope.atg:76*/if(!subEntry.IsList) subEntry = (MetaEntry) subEntry.List; 
			Expect(_to);
			GlobalId(/*Parser.GlobalScope.atg:78*/out key);
			/*Parser.GlobalScope.atg:78*/if(metaTable.Meta.ContainsKey(key))
			{
			    entry = metaTable.Meta[key];
			    entry = entry.AddToList(subEntry.List);
			}
			else
			{
			   entry = subEntry;
			}
			
		} else SynErr(146);
		/*Parser.GlobalScope.atg:88*/if(entry == null || key == null) 
		                        SemErr("Meta assignment did not generate an entry.");
		                   else 
		                        metaTable.Meta[key] = entry; 
		                
	}

	void Declaration2() {
		/*Parser.GlobalScope.atg:239*/ModuleName module = TargetModule.Name;
		SymbolBuilder builder = new SymbolBuilder();
		Func<string,ModuleName,EntityRef> entityFactory;
		bool canBeRef = false;
		
		while (!(la.kind == _EOF || la.kind == _declare)) {SynErr(147); Get();}
		Expect(_declare);
		if (StartOf(33)) {
			while (la.kind == _pointer || la.kind == _ref) {
				SymbolPrefix(/*Parser.GlobalScope.atg:247*/builder, out canBeRef);
			}
			EntityFactory(/*Parser.GlobalScope.atg:248*/canBeRef, out entityFactory);
			/*Parser.GlobalScope.atg:249*/if(entityFactory == null) builder.AutoDereferenceEnabled = false; 
			if (la.kind == _colon) {
				Get();
			}
			DeclarationInstance2(/*Parser.GlobalScope.atg:251*/entityFactory,module,builder.Clone(),preventOverride:false);
			while (la.kind == _comma) {
				Get();
				if (StartOf(34)) {
					DeclarationInstance2(/*Parser.GlobalScope.atg:252*/entityFactory,module,builder.Clone(),preventOverride:false);
				}
			}
			Expect(_semicolon);
		} else if (la.kind == _lbrace) {
			Get();
			if (la.kind == _uusing) {
				Get();
				ModuleName(/*Parser.GlobalScope.atg:255*/out module);
			}
			while (StartOf(35)) {
				/*Parser.GlobalScope.atg:256*/SymbolBuilder runBuilder = builder.Clone(); 
				while (la.kind == _pointer || la.kind == _ref) {
					SymbolPrefix(/*Parser.GlobalScope.atg:257*/builder, out canBeRef);
				}
				EntityFactory(/*Parser.GlobalScope.atg:258*/canBeRef, out entityFactory);
				/*Parser.GlobalScope.atg:259*/if(entityFactory == null) runBuilder.AutoDereferenceEnabled = false; 
				Expect(_colon);
				DeclarationInstance2(/*Parser.GlobalScope.atg:261*/entityFactory,module,runBuilder.Clone(),preventOverride:true);
				while (la.kind == _comma) {
					Get();
					if (StartOf(34)) {
						DeclarationInstance2(/*Parser.GlobalScope.atg:262*/entityFactory,module,runBuilder.Clone(),preventOverride:true);
					}
				}
			}
			Expect(_rbrace);
		} else if (la.kind == _lpar) {
			Get();
			/*Parser.GlobalScope.atg:265*/bool wasComma = false; 
			if (StartOf(4)) {
				MExprBasedDeclaration();
				while (la.kind == _comma) {
					Get();
					/*Parser.GlobalScope.atg:267*/if(wasComma)
					{
					    Loader.ReportMessage(Message.Error("Double comma in declaration sequence.",GetPosition(),MessageClasses.DuplicateComma));
					}
					wasComma = true;
					
					if (StartOf(4)) {
						MExprBasedDeclaration();
						/*Parser.GlobalScope.atg:274*/wasComma = false; 
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
		/*Parser.GlobalScope.atg:473*/PFunction func = TargetApplication._InitializationFunction;
		   CompilerTarget ft = FunctionTargets[func];
		   ISourcePosition position;
		   if(ft == null)
		       throw new PrexoniteException("Internal compilation error: InitializeFunction got lost.");
		
		/*Parser.GlobalScope.atg:480*/_PushScope(ft); 
		_pushLexerState(Lexer.Local);
		
		Expect(_lbrace);
		/*Parser.GlobalScope.atg:483*/position = GetPosition(); 
		while (StartOf(20)) {
			Statement(/*Parser.GlobalScope.atg:484*/target.Ast);
		}
		Expect(_rbrace);
		/*Parser.GlobalScope.atg:487*/try {
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
		/*Parser.GlobalScope.atg:454*/PFunction func = TargetApplication.CreateFunction();
		CompilerTarget buildBlockTarget = 
		  Loader.CreateFunctionTarget(func, sourcePosition: GetPosition());
		_PushScope(buildBlockTarget);
		Loader.DeclareBuildBlockCommands(target);
		_pushLexerState(Lexer.Local);                                
		
		if (la.kind == _does) {
			Get();
		}
		StatementBlock(/*Parser.GlobalScope.atg:463*/target.Ast);
		/*Parser.GlobalScope.atg:465*/_popLexerState();                                    
		 _PopScope(buildBlockTarget);
		 _compileAndExecuteBuildBlock(buildBlockTarget);
		
	}

	void FunctionDefinition(/*Parser.GlobalScope.atg:520*/out PFunction func) {
		/*Parser.GlobalScope.atg:521*/string primaryAlias = null;
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
			/*Parser.GlobalScope.atg:539*/isLazy = true; 
		} else if (la.kind == _function) {
			Get();
		} else if (la.kind == _coroutine) {
			Get();
			/*Parser.GlobalScope.atg:541*/isCoroutine = true; 
		} else if (la.kind == _macro) {
			Get();
			if (la.kind == _function) {
				Get();
			}
			/*Parser.GlobalScope.atg:542*/isMacro = true; 
		} else SynErr(150);
		/*Parser.GlobalScope.atg:543*/position = GetPosition(); 
		if (StartOf(4)) {
			Id(/*Parser.GlobalScope.atg:544*/out id);
			/*Parser.GlobalScope.atg:544*/primaryAlias = id; 
			if (la.kind == _as) {
				FunctionAliasList(/*Parser.GlobalScope.atg:545*/funcAliases);
			}
		} else if (la.kind == _as) {
			FunctionAliasList(/*Parser.GlobalScope.atg:546*/funcAliases);
		} else SynErr(151);
		/*Parser.GlobalScope.atg:548*/funcId = _assignPhysicalFunctionSlot(id);
		 if(Engine.StringsAreEqual(id, @"\init")) //Treat "\init" specially (that's the initialization code)
		 {
		     func = TargetApplication._InitializationFunction;
		     if(isNested)
		         Loader.ReportMessage(Message.Error("Cannot define initialization code inside another function.",position,MessageClasses.IllegalInitializationFunction));
		     if(isCoroutine)
		         Loader.ReportMessage(Message.Error("Cannot define initialization code as a coroutine.",position,MessageClasses.IllegalInitializationFunction));
		     if(isLazy)
		         Loader.ReportMessage(Message.Error("Cannot define initialization code as a lazy function.",position,MessageClasses.IllegalInitializationFunction));
		     if(isMacro)
		         Loader.ReportMessage(Message.Error("Cannot define initialization code as a macro function.",position,MessageClasses.IllegalInitializationFunction));
		 }
		 else
		 {
		     var localId = id;
		     
		     if(isNested)
		     {
		         if(isMacro)
		             Loader.ReportMessage(Message.Error("Inner macros are illegal. Macros must be top-level.",position,MessageClasses.InnerMacrosIllegal));
		             
		         funcId = generateLocalId(funcId ?? "inner");
		         
		         if(string.IsNullOrEmpty(localId))
		         {
		             //Create shadow name
		             localId = generateLocalId(id ?? "inner");
		         }
		         var innerSym = _ensureDefinedLocal(localId, localId, true, position,true);
		         foreach(var alias in funcAliases)
		             Symbols.Declare(alias, innerSym);
		         
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
		                                            
		                                            Loader.CreateFunctionTarget(func, target, position);
		                                        }
		                                        CompilerTarget ft = FunctionTargets[func];
		                                        
		                                        //Generate derived stub
		                                        if(isCoroutine || isLazy)
		                                        {
		                                            derStub = func;
		                                            
		                                            //Create derived body function
		                                            derId = ft.GenerateLocalId();
		                                            derBody = TargetApplication.CreateFunction(derId);
		                                            Loader.CreateFunctionTarget(derBody, ft, position);
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
					FormalArg(/*Parser.GlobalScope.atg:632*/ft);
					while (la.kind == _comma) {
						Get();
						/*Parser.GlobalScope.atg:633*/if(missingArg)
						     {
						         SemErr("Missing formal argument (two consecutive commas).");
						     } 
						 
						if (StartOf(19)) {
							FormalArg(/*Parser.GlobalScope.atg:638*/ft);
							/*Parser.GlobalScope.atg:638*/missingArg = false; 
						} else if (la.kind == _comma || la.kind == _rpar) {
							/*Parser.GlobalScope.atg:639*/missingArg = true; 
						} else SynErr(152);
					}
				}
				Expect(_rpar);
			} else {
				FormalArg(/*Parser.GlobalScope.atg:644*/ft);
				while (StartOf(37)) {
					if (la.kind == _comma) {
						Get();
					}
					FormalArg(/*Parser.GlobalScope.atg:646*/ft);
				}
			}
		}
		/*Parser.GlobalScope.atg:648*/if(isNested && isLazy) // keep this assignment for maintainability
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
			/*Parser.GlobalScope.atg:679*/_pushLexerState(Lexer.YYINITIAL); 
			Get();
			if (StartOf(31)) {
				MetaAssignment(/*Parser.GlobalScope.atg:681*/func);
				while (la.kind == _semicolon) {
					Get();
					if (StartOf(31)) {
						MetaAssignment(/*Parser.GlobalScope.atg:683*/func);
					}
				}
			}
			/*Parser.GlobalScope.atg:686*/_popLexerState(); 
			Expect(_rbrack);
		}
		/*Parser.GlobalScope.atg:691*/if(primaryAlias != null && !_suppressPrimarySymbol(func))
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
		                                    
		if (la.kind == _namespace) {
			/*Parser.GlobalScope.atg:715*/_pushLexerState(Lexer.Transfer);
			ISourcePosition importKeywordPosition;
			var importBuilder = SymbolStoreBuilder.Create();
			
			Get();
			ImportContextualKeyword(/*Parser.GlobalScope.atg:720*/out importKeywordPosition);
			NsTransferSpec(/*Parser.GlobalScope.atg:721*/importBuilder);
			while (la.kind == _comma) {
				Get();
				NsTransferSpec(/*Parser.GlobalScope.atg:724*/importBuilder);
			}
			/*Parser.GlobalScope.atg:726*/{   // Copy imported symbols to import scope
			   var it = FunctionTargets[func]; 
			   foreach(var entry in importBuilder.ToSymbolStore())
			       it.ImportScope.Declare(entry.Key,entry.Value);
			}
			_popLexerState(); 
			
		}
		/*Parser.GlobalScope.atg:735*/if(isLazy || isCoroutine)
		{
		  //Push the stub, because it is the lexical parent of the body
		  _PushScope(cst);
		}
		_PushScope(FunctionTargets[func]);
		Debug.Assert(target != null); // Mostly to tell ReSharper that target is not null.
		_pushLexerState(Lexer.Local);
		if(isMacro)
		    target.SetupAsMacro();
		
		if (la.kind == _does) {
			Get();
			StatementBlock(/*Parser.GlobalScope.atg:747*/target.Ast);
		} else if (la.kind == _lbrace) {
			Get();
			while (StartOf(20)) {
				Statement(/*Parser.GlobalScope.atg:748*/target.Ast);
			}
			Expect(_rbrace);
		} else if (/*Parser.GlobalScope.atg:751*/isFollowedByStatementBlock()) {
			Expect(_implementation);
			StatementBlock(/*Parser.GlobalScope.atg:752*/target.Ast);
		} else if (la.kind == _assign || la.kind == _implementation) {
			if (la.kind == _assign) {
				Get();
			} else {
				Get();
			}
			/*Parser.GlobalScope.atg:753*/AstReturn ret = new AstReturn(this, ReturnVariant.Exit); 
			Expr(/*Parser.GlobalScope.atg:754*/out ret.Expression);
			/*Parser.GlobalScope.atg:754*/target.Ast.Add(ret); 
			Expect(_semicolon);
		} else SynErr(153);
		/*Parser.GlobalScope.atg:756*/_popLexerState();
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
		        crcor.Expression = Create.CreateClosure(position,EntityRef.Function.Create(derBody.Id,derBody.ParentApplication.Module.Name));
		
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
		                                retVal = Create.CreateClosure(position, EntityRef.Function.Create(ct.Function.Id, 
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
		/*Parser.GlobalScope.atg:1042*/QualifiedId fullNsId;
		DeclarationScope scope = null;
		ISourcePosition qualIdPos,exportPos;
		List<SymbolTransferDirective> directives;
		SymbolOrigin origin;
		var implicitSelfImport = true;
		
		
		Expect(_namespace);
		/*Parser.GlobalScope.atg:1050*/qualIdPos = GetPosition(); 
		NsQualifiedId(/*Parser.GlobalScope.atg:1051*/out fullNsId);
		/*Parser.GlobalScope.atg:1051*/var declBuilder = _prepareDeclScope(fullNsId, qualIdPos); 
		if (la.kind == _id || la.kind == _anyId) {
			/*Parser.GlobalScope.atg:1052*/ISourcePosition importKeywordPosition;
			_pushLexerState(Lexer.Transfer);
			
			ImportContextualKeyword(/*Parser.GlobalScope.atg:1055*/out importKeywordPosition);
			if (la.kind == _lpar || la.kind == _timessym) {
				/*Parser.GlobalScope.atg:1057*/var groupPos = GetPosition();
				origin = new SymbolOrigin.NamespaceImport(declBuilder.Prefix,groupPos);
				directives = new List<SymbolTransferDirective>();
				
				NsTransferDirectiveGroup(/*Parser.GlobalScope.atg:1061*/directives);
				/*Parser.GlobalScope.atg:1062*/declBuilder.LocalScopeBuilder.Forward(
				   origin, 
				   declBuilder.Namespace, 
				   directives); 
				implicitSelfImport = false;
				
			} else if (StartOf(38)) {
				if (la.kind == _id || la.kind == _anyId) {
					NsTransferSpec(/*Parser.GlobalScope.atg:1068*/declBuilder.LocalScopeBuilder);
				}
			} else SynErr(154);
			while (la.kind == _comma) {
				Get();
				if (la.kind == _id || la.kind == _anyId) {
					NsTransferSpec(/*Parser.GlobalScope.atg:1073*/declBuilder.LocalScopeBuilder);
				}
			}
			/*Parser.GlobalScope.atg:1075*/_popLexerState(); 
		}
		/*Parser.GlobalScope.atg:1077*/if(implicitSelfImport)
		{
		    origin = new SymbolOrigin.NamespaceImport(
		        declBuilder.Prefix,qualIdPos);
		    declBuilder.LocalScopeBuilder.Forward(
		        origin,
		        declBuilder.Namespace,
		        SymbolTransferDirective.CreateWildcard(qualIdPos).Singleton()
		    );
		}
		Loader.PushScope(declBuilder.ToDeclarationScope()); 
		
		Expect(_lbrace);
		DeclarationLevel();
		/*Parser.GlobalScope.atg:1090*/exportPos = GetPosition(); // this position is used when there is no export-spec
		
		Expect(_rbrace);
		/*Parser.GlobalScope.atg:1092*/scope = _popDeclScope();
		var exportBuilder = SymbolStoreBuilder.Create(); 
		if (la.kind == _export) {
			/*Parser.GlobalScope.atg:1094*/_pushLexerState(Lexer.Transfer); 
			Get();
			if (la.kind == _dot || la.kind == _lpar || la.kind == _timessym) {
				/*Parser.GlobalScope.atg:1096*/exportPos = GetPosition(); // this is a more accurate position
				origin = _privateDeclarationOrigin(exportPos,scope); 
				directives = new List<SymbolTransferDirective>();
				
				if (la.kind == _lpar || la.kind == _timessym) {
					NsTransferDirectiveGroup(/*Parser.GlobalScope.atg:1100*/directives);
					/*Parser.GlobalScope.atg:1101*/exportBuilder.Forward(origin, 
					   _indexExportedSymbols(scope.Store), directives); 
				} else {
					Get();
					Expect(_times);
					/*Parser.GlobalScope.atg:1103*/exportBuilder.Forward(origin,
					   _indexExportedSymbols(scope.Store), 
					   SymbolTransferDirective.CreateWildcard(exportPos).Singleton()); 
				}
				if (la.kind == _comma || la.kind == _semicolon) {
					while (la.kind == _comma) {
						Get();
						NsTransferSpec(/*Parser.GlobalScope.atg:1108*/exportBuilder);
					}
					Expect(_semicolon);
				}
			} else if (la.kind == _id || la.kind == _anyId) {
				NsTransferSpec(/*Parser.GlobalScope.atg:1112*/exportBuilder);
				Expect(_semicolon);
			} else SynErr(155);
			/*Parser.GlobalScope.atg:1114*/_popLexerState(); 
		} else if (StartOf(39)) {
			/*Parser.GlobalScope.atg:1118*/exportBuilder.Forward(
			   _privateDeclarationOrigin(exportPos,scope), 
			   _indexExportedSymbols(scope.Store), 
			   SymbolTransferDirective.CreateWildcard(exportPos).Singleton()); 
			
		} else SynErr(156);
		/*Parser.GlobalScope.atg:1123*/_updateNamespace(scope, exportBuilder); 
	}

	void GlobalId(/*Parser.GlobalScope.atg:894*/out string id) {
		/*Parser.GlobalScope.atg:894*/id = "...no freaking id..."; 
		if (la.kind == _id) {
			Get();
			/*Parser.GlobalScope.atg:896*/id = cache(t.val); 
		} else if (la.kind == _anyId) {
			Get();
			String(/*Parser.GlobalScope.atg:897*/out id);
			/*Parser.GlobalScope.atg:897*/id = cache(id); 
		} else SynErr(157);
	}

	void MetaExpr(/*Parser.GlobalScope.atg:96*/out MetaEntry entry) {
		/*Parser.GlobalScope.atg:96*/bool sw; int i; double r; entry = null; string str; Version v; 
		if (la.kind == _true || la.kind == _false) {
			Boolean(/*Parser.GlobalScope.atg:98*/out sw);
			/*Parser.GlobalScope.atg:98*/entry = sw; 
		} else if (la.kind == _integer) {
			Integer(/*Parser.GlobalScope.atg:99*/out i);
			/*Parser.GlobalScope.atg:99*/entry = i.ToString(CultureInfo.InvariantCulture); 
		} else if (la.kind == _real || la.kind == _realLike) {
			Real(/*Parser.GlobalScope.atg:100*/out r);
			/*Parser.GlobalScope.atg:100*/entry = r.ToString(CultureInfo.InvariantCulture); 
		} else if (StartOf(40)) {
			if (la.kind == _string) {
				String(/*Parser.GlobalScope.atg:101*/out str);
				/*Parser.GlobalScope.atg:101*/entry = str; 
			} else {
				GlobalQualifiedId(/*Parser.GlobalScope.atg:102*/out str);
				/*Parser.GlobalScope.atg:103*/entry = str; 
			}
			if (la.kind == _div) {
				Get();
				Version(/*Parser.GlobalScope.atg:106*/out v);
				/*Parser.GlobalScope.atg:106*/entry = new Prexonite.Modular.ModuleName(entry.Text,v); 
			}
		} else if (la.kind == _lbrace) {
			Get();
			/*Parser.GlobalScope.atg:108*/List<MetaEntry> lst = new List<MetaEntry>(); 
			MetaEntry subEntry; 
			bool lastWasEmpty = false;
			
			if (StartOf(32)) {
				MetaExpr(/*Parser.GlobalScope.atg:112*/out subEntry);
				/*Parser.GlobalScope.atg:112*/lst.Add(subEntry); 
				while (la.kind == _comma) {
					Get();
					/*Parser.GlobalScope.atg:113*/if(lastWasEmpty)
					    SemErr("Missing meta expression in list (two consecutive commas).");
					
					if (StartOf(32)) {
						MetaExpr(/*Parser.GlobalScope.atg:116*/out subEntry);
						/*Parser.GlobalScope.atg:117*/lst.Add(subEntry); 
						lastWasEmpty = false;
						
					} else if (la.kind == _comma || la.kind == _rbrace) {
						/*Parser.GlobalScope.atg:120*/lastWasEmpty = true; 
					} else SynErr(158);
				}
			}
			Expect(_rbrace);
			/*Parser.GlobalScope.atg:124*/entry = (MetaEntry) lst.ToArray(); 
		} else SynErr(159);
	}

	void GlobalQualifiedId(/*Parser.GlobalScope.atg:900*/out string id) {
		/*Parser.GlobalScope.atg:900*/id = "\\NoId\\"; 
		if (la.kind == _id || la.kind == _anyId) {
			GlobalId(/*Parser.GlobalScope.atg:902*/out id);
		} else if (la.kind == _ns) {
			Get();
			/*Parser.GlobalScope.atg:903*/StringBuilder buffer = new StringBuilder(t.val); buffer.Append('.'); 
			while (la.kind == _ns) {
				Get();
				/*Parser.GlobalScope.atg:904*/buffer.Append(t.val); buffer.Append('.'); 
			}
			GlobalId(/*Parser.GlobalScope.atg:906*/out id);
			/*Parser.GlobalScope.atg:906*/buffer.Append(id); 
			/*Parser.GlobalScope.atg:907*/id = cache(buffer.ToString()); 
		} else SynErr(160);
	}

	void Version(/*Parser.Helper.atg:64*/out Version version) {
		if (la.kind == _realLike) {
			Get();
		} else if (la.kind == _version) {
			Get();
		} else SynErr(161);
		/*Parser.Helper.atg:65*/var raw = t.val;
		if(!TryParseVersion(raw, out version))
		{
		                               SemErr(t,"Cannot recognize \"" + raw + "\" as a version literal.");
			version = new Version(0,0);
		}
		                       
	}

	void GlobalVariableAliasList(/*Parser.GlobalScope.atg:189*/IList<string> aliases ) {
		/*Parser.GlobalScope.atg:189*/string id; 
		Expect(_as);
		GlobalId(/*Parser.GlobalScope.atg:191*/out id);
		/*Parser.GlobalScope.atg:191*/aliases.Add(id); 
		while (la.kind == _comma) {
			Get();
			if (la.kind == _id || la.kind == _anyId) {
				GlobalId(/*Parser.GlobalScope.atg:193*/out id);
				/*Parser.GlobalScope.atg:193*/aliases.Add(id); 
			}
		}
	}

	void SymbolPrefix(/*Parser.GlobalScope.atg:201*/SymbolBuilder symbol, out bool canBeRef) {
		/*Parser.GlobalScope.atg:201*/canBeRef = true; 
		if (la.kind == _ref) {
			Get();
			/*Parser.GlobalScope.atg:203*/symbol.Dereference(); 
		} else if (la.kind == _pointer) {
			Get();
			/*Parser.GlobalScope.atg:204*/symbol.ReferenceTo(); canBeRef = false; 
		} else SynErr(162);
	}

	void EntityFactory(/*Parser.GlobalScope.atg:208*/bool canBeRef, out Func<string,ModuleName,EntityRef> entityFactory ) {
		/*Parser.GlobalScope.atg:208*/entityFactory = null; bool projectNamespace = false; 
		if (la.kind == _var) {
			Get();
			/*Parser.GlobalScope.atg:210*/entityFactory = EntityRef.Variable.Global.Create; projectNamespace = true; 
		} else if (la.kind == _function) {
			Get();
			/*Parser.GlobalScope.atg:211*/entityFactory = EntityRef.Function.Create; projectNamespace = true; 
		} else if (la.kind == _command) {
			Get();
			/*Parser.GlobalScope.atg:212*/entityFactory = (id,_) => EntityRef.Command.Create(id); 
		} else if (la.kind == _macro) {
			Get();
			if (la.kind == _function) {
				Get();
				/*Parser.GlobalScope.atg:214*/entityFactory = EntityRef.Function.Create; projectNamespace = true; 
			} else if (la.kind == _command) {
				Get();
				/*Parser.GlobalScope.atg:215*/entityFactory = (id,_) => EntityRef.MacroCommand.Create(id); 
			} else if (la.kind == _var) {
				Get();
				/*Parser.GlobalScope.atg:216*/entityFactory = EntityRef.Variable.Global.Create; projectNamespace = true; 
			} else SynErr(163);
		} else if (StartOf(41)) {
			/*Parser.GlobalScope.atg:218*/if(canBeRef) 
			{
			  projectNamespace = true;
			  entityFactory = EntityRef.Variable.Global.Create;
			}
			else
			{
			  // entityFactory already set to null
			}                                
			
		} else SynErr(164);
		/*Parser.GlobalScope.atg:231*/if(entityFactory != null && projectNamespace) {
		   var actualFactory = entityFactory;
		   entityFactory = (id,mn) => actualFactory(_assignPhysicalSlot(id),mn);
		}
		
	}

	void DeclarationInstance2(/*Parser.GlobalScope.atg:368*/Func<string,ModuleName,EntityRef> entityFactory, 
ModuleName module, 
SymbolBuilder builder,
bool preventOverride = false ) {
		/*Parser.GlobalScope.atg:368*/string lhsId;
		string rhsId; 
		ISourcePosition position = GetPosition(); 
		
		SymbolDirective(/*Parser.GlobalScope.atg:373*/entityFactory,module,builder,out lhsId,preventOverride:preventOverride);
		/*Parser.GlobalScope.atg:374*/rhsId = lhsId; 
		if (la.kind == _as) {
			Get();
			Id(/*Parser.GlobalScope.atg:375*/out rhsId);
		}
		/*Parser.GlobalScope.atg:377*/if(entityFactory == null) 
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

	void ModuleName(/*Parser.GlobalScope.atg:437*/out ModuleName moduleName) {
		/*Parser.GlobalScope.atg:437*/_pushLexerState(Lexer.YYINITIAL); //need global scope for Version
		string id; 
		Version version = null;
		
		Id(/*Parser.GlobalScope.atg:442*/out id);
		if (la.kind == _div) {
			Get();
			Version(/*Parser.GlobalScope.atg:444*/out version);
		}
		/*Parser.GlobalScope.atg:445*/_popLexerState();
		moduleName = Loader.Cache[new ModuleName(id,version ?? new Version(0,0))];
		
	}

	void MExprBasedDeclaration() {
		/*Parser.GlobalScope.atg:282*/string alias;
		MExpr expr;
		
		Id(/*Parser.GlobalScope.atg:286*/out alias);
		Expect(_assign);
		MExpr(/*Parser.GlobalScope.atg:286*/out expr);
		/*Parser.GlobalScope.atg:287*/Symbol s = _parseSymbol(expr);
		Symbols.Declare(alias,s);
		
	}

	void MExpr(/*Parser.Helper.atg:154*/out MExpr expr) {
		/*Parser.Helper.atg:154*/expr = new MExpr.MAtom(NoSourcePosition.Instance,null);
		       bool lastWasComma = false;
		   
		if (StartOf(42)) {
			/*Parser.Helper.atg:157*/String id; 
			var args = new List<MExpr>();
			MExpr arg;
			
			MId(/*Parser.Helper.atg:161*/out id);
			if (la.kind == _lpar) {
				Get();
				if (StartOf(43)) {
					MExpr(/*Parser.Helper.atg:163*/out arg);
					/*Parser.Helper.atg:163*/args.Add(arg); 
					while (la.kind == _comma) {
						Get();
						/*Parser.Helper.atg:164*/if(lastWasComma)
						          {
						              Loader.ReportMessage(Message.Error("Double comma in MExpr list.",GetPosition(),MessageClasses.DuplicateComma));
						          }
						          lastWasComma = true; 
						      
						if (StartOf(43)) {
							MExpr(/*Parser.Helper.atg:170*/out arg);
							/*Parser.Helper.atg:171*/args.Add(arg); 
							lastWasComma = false;
							
						}
					}
				}
				Expect(_rpar);
			} else if (StartOf(43)) {
				MExpr(/*Parser.Helper.atg:178*/out arg);
				/*Parser.Helper.atg:178*/args.Add(arg); 
			} else if (la.kind == _comma || la.kind == _rpar) {
				
			} else SynErr(165);
			/*Parser.Helper.atg:180*/expr = new MExpr.MList(GetPosition(), id,args); 
		} else if (la.kind == _string) {
			/*Parser.Helper.atg:181*/String value; 
			String(/*Parser.Helper.atg:182*/out value);
			/*Parser.Helper.atg:182*/expr = new MExpr.MAtom(GetPosition(), value); 
		} else if (la.kind == _integer || la.kind == _minus || la.kind == _plus) {
			/*Parser.Helper.atg:183*/int intval; 
			SignedInteger(/*Parser.Helper.atg:184*/out intval);
			/*Parser.Helper.atg:184*/expr = new MExpr.MAtom(GetPosition(), intval); 
		} else if (la.kind == _true || la.kind == _false) {
			/*Parser.Helper.atg:185*/bool boolval; 
			Boolean(/*Parser.Helper.atg:186*/out boolval);
			/*Parser.Helper.atg:186*/expr = new MExpr.MAtom(GetPosition(), boolval); 
		} else if (la.kind == _version || la.kind == _realLike) {
			/*Parser.Helper.atg:187*/Version v; 
			Version(/*Parser.Helper.atg:188*/out v);
			/*Parser.Helper.atg:188*/expr = new MExpr.MAtom(GetPosition(), v); 
		} else if (la.kind == _null) {
			Null();
			/*Parser.Helper.atg:190*/expr = new MExpr.MAtom(GetPosition(), null); 
		} else SynErr(166);
	}

	void MessageDirective(/*Parser.GlobalScope.atg:299*/Func<string,ModuleName,EntityRef> entityFactory, 
ModuleName module, 
SymbolBuilder builder,
[CanBeNull] out string lhsId,
MessageSeverity severity,
bool preventOverride = false ) {
		/*Parser.GlobalScope.atg:299*/string message;
		string messageClass = null;
		ISourcePosition position = GetPosition();
		string file;
		int line;
		int column;
		
		Expect(_lpar);
		if (la.kind == _null) {
			Get();
		} else if (la.kind == _string) {
			String(/*Parser.GlobalScope.atg:308*/out messageClass);
		} else SynErr(167);
		if (la.kind == _colon) {
			Get();
			if (la.kind == _null) {
				Get();
			} else if (la.kind == _string) {
				String(/*Parser.GlobalScope.atg:311*/out file);
				Expect(_colon);
				Integer(/*Parser.GlobalScope.atg:311*/out line);
				Expect(_colon);
				Integer(/*Parser.GlobalScope.atg:311*/out column);
				/*Parser.GlobalScope.atg:312*/position = new SourcePosition(file,line,column); 
			} else SynErr(168);
		}
		Expect(_comma);
		String(/*Parser.GlobalScope.atg:316*/out message);
		Expect(_comma);
		/*Parser.GlobalScope.atg:317*/builder.AddMessage(Message.Create(severity,message,position,messageClass)); 
		SymbolDirective(/*Parser.GlobalScope.atg:318*/entityFactory,module,builder,out lhsId,preventOverride:preventOverride);
		Expect(_rpar);
	}

	void SymbolDirective(/*Parser.GlobalScope.atg:327*/Func<string,ModuleName,EntityRef> entityFactory, 
[CanBeNull] ModuleName module, 
SymbolBuilder builder,
[CanBeNull] out string lhsId,
bool preventOverride = false ) {
		/*Parser.GlobalScope.atg:327*/lhsId = null;  
		if (la.kind == _null) {
			Get();
			
		} else if (la.kind == _pointer || la.kind == _ref) {
			if (la.kind == _ref) {
				Get();
				/*Parser.GlobalScope.atg:330*/builder.Dereference(); 
			} else {
				Get();
				/*Parser.GlobalScope.atg:331*/builder.ReferenceTo(); 
			}
			SymbolDirective(/*Parser.GlobalScope.atg:333*/entityFactory,module,builder,out lhsId,preventOverride:preventOverride);
		} else if (/*Parser.GlobalScope.atg:335*/isSymbolDirective("INFO")) {
			Expect(_id);
			MessageDirective(/*Parser.GlobalScope.atg:336*/entityFactory,module,builder, out lhsId,MessageSeverity.Info,preventOverride:preventOverride);
		} else if (/*Parser.GlobalScope.atg:338*/isSymbolDirective("WARN")) {
			Expect(_id);
			MessageDirective(/*Parser.GlobalScope.atg:339*/entityFactory,module,builder, out lhsId,MessageSeverity.Warning,preventOverride:preventOverride);
		} else if (/*Parser.GlobalScope.atg:341*/isSymbolDirective("ERROR")) {
			Expect(_id);
			MessageDirective(/*Parser.GlobalScope.atg:342*/entityFactory,module,builder, out lhsId,MessageSeverity.Error,preventOverride:preventOverride);
		} else if (StartOf(4)) {
			/*Parser.GlobalScope.atg:343*/ISourcePosition position = GetPosition(); 
			Id(/*Parser.GlobalScope.atg:344*/out lhsId);
			if (la.kind == _div) {
				Get();
				ModuleName(/*Parser.GlobalScope.atg:345*/out module);
				/*Parser.GlobalScope.atg:346*/if(preventOverride) 
				{
				    Loader.ReportMessage(Message.Error(
				      "Specification of module name illegal at this point.",
				      position  ,
				      MessageClasses.UnexpectedModuleName)); 
				    // Let control fall through, this is not a fatal error,
				    //  just an enforcement of a stylistic rule.
				}
				
			}
			/*Parser.GlobalScope.atg:356*/if(entityFactory != null)
			{
			  builder.Entity = entityFactory(lhsId,module);
			}
			
		} else SynErr(169);
	}

	void StatementBlock(/*Parser.Statement.atg:26*/AstBlock block) {
		Statement(/*Parser.Statement.atg:27*/block);
	}

	void FunctionAliasList(/*Parser.GlobalScope.atg:510*/IList<string> aliases ) {
		/*Parser.GlobalScope.atg:510*/String id; 
		Expect(_as);
		Id(/*Parser.GlobalScope.atg:512*/out id);
		/*Parser.GlobalScope.atg:512*/aliases.Add(id); 
		while (la.kind == _comma) {
			Get();
			if (StartOf(4)) {
				Id(/*Parser.GlobalScope.atg:514*/out id);
				/*Parser.GlobalScope.atg:514*/aliases.Add(id); 
			}
		}
	}

	void ImportContextualKeyword(/*Parser.GlobalScope.atg:1031*/out ISourcePosition importKeywordPosition) {
		/*Parser.GlobalScope.atg:1031*/string importKeyword; 
		/*Parser.GlobalScope.atg:1032*/importKeywordPosition = GetPosition(); 
		GlobalId(/*Parser.GlobalScope.atg:1034*/out importKeyword);
		/*Parser.GlobalScope.atg:1034*/if(!Engine.StringsAreEqual(importKeyword,"import")){
		   Loader.ReportMessage(Message.Error(
		       System.String.Format("Expected keyword 'import' or '{' instead of \"{0}\".",importKeyword),
		       importKeywordPosition, MessageClasses.ImportExpected));
		} 
		
	}

	void NsTransferSpec(/*Parser.GlobalScope.atg:995*/SymbolStoreBuilder builder) {
		/*Parser.GlobalScope.atg:995*/QualifiedId specRoot;
		ISourcePosition pos;
		ISymbolView<Symbol> sourceScope = null;
		var hasWildcard = false;
		
		NsTransferSource(/*Parser.GlobalScope.atg:1001*/out hasWildcard, out specRoot);
		/*Parser.GlobalScope.atg:1002*/var directives = new List<SymbolTransferDirective>();
		pos = GetPosition();
		
		if (la.kind == _lpar || la.kind == _timessym) {
			NsTransferDirectiveGroup(/*Parser.GlobalScope.atg:1005*/directives);
			/*Parser.GlobalScope.atg:1006*/sourceScope = _resolveNamespace(Symbols,pos, specRoot); 
		} else if (StartOf(44)) {
			/*Parser.GlobalScope.atg:1007*/if(hasWildcard)
			{
			    sourceScope = _resolveNamespace(Symbols,pos, specRoot);
			    directives.Add(SymbolTransferDirective.CreateWildcard(pos));
			}
			else
			{
			    var symId = specRoot[specRoot.Count-1];
			    specRoot = specRoot.WithSuffixDropped(1);
			    sourceScope = _resolveNamespace(Symbols, pos, specRoot);
			    directives.Add(SymbolTransferDirective.CreateRename(pos,symId,symId));
			}
			
		} else SynErr(170);
		/*Parser.GlobalScope.atg:1020*/if(sourceScope != null)
		   builder.Forward(
		       new SymbolOrigin.NamespaceImport(specRoot, pos), 
		       sourceScope, 
		       directives); 
		// if the source scope is null, something has gone wrong and the error should
		// have already been reported
		
	}

	void NsQualifiedId(/*Parser.GlobalScope.atg:911*/out QualifiedId qualifiedId) {
		/*Parser.GlobalScope.atg:911*/bool _; 
		NsQualifiedIdImpl(/*Parser.GlobalScope.atg:913*/false,out qualifiedId, out _);
	}

	void NsQualifiedIdImpl(/*Parser.GlobalScope.atg:923*/bool allowWildcard, out QualifiedId qualifiedId, out bool hasWildcard) {
		/*Parser.GlobalScope.atg:923*/qualifiedId = default(QualifiedId);
		String part = null;
		var parts = new List<String>();
		hasWildcard = false;
		
		GlobalId(/*Parser.GlobalScope.atg:929*/out part);
		/*Parser.GlobalScope.atg:929*/parts.Add(part); 
		while (la.kind == _dot) {
			/*Parser.GlobalScope.atg:930*/if(hasWildcard)
			{
			    Create.ReportMessage(Message.Error(
			        "Unexpected qualified id parts after wildcard.",
			        GetPosition(),
			        MessageClasses.QualifiedIdPartsAfterWildcard
			    ));
			} 
			
			Get();
			if (la.kind == _id || la.kind == _anyId) {
				GlobalId(/*Parser.GlobalScope.atg:940*/out part);
				/*Parser.GlobalScope.atg:940*/parts.Add(part); 
			} else if (la.kind == _timessym) {
				Get();
				/*Parser.GlobalScope.atg:941*/parts.Add(OperatorNames.Prexonite.Multiplication); 
			} else if (la.kind == _times) {
				/*Parser.GlobalScope.atg:942*/ISourcePosition starPos = GetPosition(); 
				Get();
				/*Parser.GlobalScope.atg:943*/hasWildcard = true;
				if(!allowWildcard)
				{
				    Create.ReportMessage(Message.Error(
				        "Unexpected wildcard in qualified namespace name.",
				        starPos,
				        MessageClasses.UnexpectedWildcard));
				}
				
			} else SynErr(171);
		}
		/*Parser.GlobalScope.atg:953*/qualifiedId = new QualifiedId(parts.ToArray()); 
	}

	void NsTransferSource(/*Parser.GlobalScope.atg:917*/out bool hasWildcard, out QualifiedId qualifiedId) {
		/*Parser.GlobalScope.atg:917*/hasWildcard = false; 
		NsQualifiedIdImpl(/*Parser.GlobalScope.atg:919*/true, out qualifiedId, out hasWildcard);
	}

	void NsTransferDirective(/*Parser.GlobalScope.atg:957*/ICollection<SymbolTransferDirective> directives) {
		/*Parser.GlobalScope.atg:957*/string externalId;
		string internalId = null; 
		/*Parser.GlobalScope.atg:959*/var pos = GetPosition(); 
		if (la.kind == _times) {
			Get();
			/*Parser.GlobalScope.atg:960*/directives.Add(SymbolTransferDirective.CreateWildcard(pos)); 
		} else if (la.kind == _id || la.kind == _anyId) {
			GlobalId(/*Parser.GlobalScope.atg:961*/out externalId);
			if (la.kind == _implementation) {
				Get();
				GlobalId(/*Parser.GlobalScope.atg:962*/out internalId);
			} else if (la.kind == _comma || la.kind == _rpar) {
				/*Parser.GlobalScope.atg:963*/internalId = externalId; 
			} else SynErr(172);
			/*Parser.GlobalScope.atg:964*/if(internalId == null) {                
			   // internalId is null in case of a syntax error
			   internalId = externalId;
			}
			directives.Add(SymbolTransferDirective.CreateRename(pos, externalId, internalId));
			
		} else if (la.kind == _not) {
			Get();
			GlobalId(/*Parser.GlobalScope.atg:971*/out externalId);
			/*Parser.GlobalScope.atg:971*/directives.Add(SymbolTransferDirective.CreateDrop(pos, externalId)); 
		} else SynErr(173);
	}

	void NsTransferDirectiveGroup(/*Parser.GlobalScope.atg:976*/ICollection<SymbolTransferDirective> directives) {
		if (la.kind == _lpar) {
			/*Parser.GlobalScope.atg:977*/_pushLexerState(Lexer.YYINITIAL); 
			Get();
			if (StartOf(45)) {
				NsTransferDirective(/*Parser.GlobalScope.atg:979*/directives);
				while (la.kind == _comma) {
					Get();
					if (StartOf(45)) {
						NsTransferDirective(/*Parser.GlobalScope.atg:981*/directives);
					}
				}
			}
			/*Parser.GlobalScope.atg:984*/_popLexerState(); 
			Expect(_rpar);
		} else if (la.kind == _timessym) {
			/*Parser.GlobalScope.atg:986*/var pos = GetPosition(); 
			Get();
			/*Parser.GlobalScope.atg:989*/directives.Add(SymbolTransferDirective.CreateWildcard(pos));
			
		} else SynErr(174);
	}

	void MId(/*Parser.Helper.atg:91*/out String id) {
		/*Parser.Helper.atg:91*/id = "an\\invalid\\MId"; 
		if (StartOf(46)) {
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
			/*Parser.Helper.atg:150*/id = cache(t.val); 
		} else if (StartOf(4)) {
			Id(/*Parser.Helper.atg:151*/out id);
		} else SynErr(175);
	}

	void ExplicitLabel(/*Parser.Statement.atg:327*/AstBlock block) {
		/*Parser.Statement.atg:327*/string id = "--\\NotAnId\\--"; 
		if (StartOf(4)) {
			Id(/*Parser.Statement.atg:329*/out id);
			Expect(_colon);
		} else if (la.kind == _lid) {
			Get();
			/*Parser.Statement.atg:330*/id = cache(t.val.Substring(0,t.val.Length-1)); 
		} else SynErr(176);
		/*Parser.Statement.atg:331*/block.Statements.Add(new AstExplicitLabel(this, id)); 
	}

	void SimpleStatement(/*Parser.Statement.atg:42*/AstBlock block) {
		if (la.kind == _goto) {
			ExplicitGoTo(/*Parser.Statement.atg:43*/block);
		} else if (StartOf(18)) {
			GetSetComplex(/*Parser.Statement.atg:44*/block);
		} else if (StartOf(47)) {
			Return(/*Parser.Statement.atg:45*/block);
		} else if (la.kind == _throw) {
			Throw(/*Parser.Statement.atg:46*/block);
		} else if (la.kind == _let) {
			LetBindingStmt(/*Parser.Statement.atg:47*/block);
		} else SynErr(177);
	}

	void StructureStatement(/*Parser.Statement.atg:51*/AstBlock block) {
		switch (la.kind) {
		case _asm: {
			/*Parser.Statement.atg:52*/_pushLexerState(Lexer.Asm); 
			Get();
			AsmStatementBlock(/*Parser.Statement.atg:53*/block);
			/*Parser.Statement.atg:54*/_popLexerState(); 
			break;
		}
		case _if: case _unless: {
			Condition(/*Parser.Statement.atg:55*/block);
			break;
		}
		case _declare: {
			Declaration2();
			break;
		}
		case _do: case _while: case _until: {
			WhileLoop(/*Parser.Statement.atg:57*/block);
			break;
		}
		case _for: {
			ForLoop(/*Parser.Statement.atg:58*/block);
			break;
		}
		case _foreach: {
			ForeachLoop(/*Parser.Statement.atg:59*/block);
			break;
		}
		case _function: case _coroutine: case _macro: case _lazy: {
			NestedFunction(/*Parser.Statement.atg:60*/block);
			break;
		}
		case _try: {
			TryCatchFinally(/*Parser.Statement.atg:61*/block);
			break;
		}
		case _uusing: {
			Using(/*Parser.Statement.atg:62*/block);
			break;
		}
		case _lbrace: {
			Get();
			while (StartOf(20)) {
				Statement(/*Parser.Statement.atg:65*/block);
			}
			Expect(_rbrace);
			break;
		}
		default: SynErr(178); break;
		}
	}

	void ExplicitGoTo(/*Parser.Statement.atg:334*/AstBlock block) {
		/*Parser.Statement.atg:334*/string id; 
		Expect(_goto);
		Id(/*Parser.Statement.atg:337*/out id);
		/*Parser.Statement.atg:337*/block.Statements.Add(new AstExplicitGoTo(this, id)); 
	}

	void GetSetComplex(/*Parser.Statement.atg:71*/AstBlock block) {
		/*Parser.Statement.atg:71*/AstGetSet complex; 
		AstExpr expr;
		AstNode node;
		
		GetInitiator(/*Parser.Statement.atg:77*/out expr);
		/*Parser.Statement.atg:77*/complex = expr as AstGetSet; 
		while (la.kind == _dot || la.kind == _lbrack) {
			GetSetExtension(/*Parser.Statement.atg:80*/expr, out complex);
			/*Parser.Statement.atg:80*/expr = complex; 
		}
		if (la.kind == _rpar || la.kind == _semicolon) {
			/*Parser.Statement.atg:83*/if(expr != null) // Happens in case of an error
			   block.Add(expr); 
			
		} else if (StartOf(48)) {
			/*Parser.Statement.atg:86*/var pos = GetPosition(); 
			if(complex == null)
			{                                                
			    Loader.ReportMessage(Message.Error("Expected an LValue (Get/Set-Complex) for ++,-- or assignment statement.",pos,MessageClasses.LValueExpected));
			    complex = Create.IndirectCall(pos,Create.Null(pos));
			}                                            
			
			if (la.kind == _inc) {
				Get();
				/*Parser.Statement.atg:93*/block.Add(Create.UnaryOperation(pos, UnaryOperator.PostIncrement, complex)); 
			} else if (la.kind == _dec) {
				Get();
				/*Parser.Statement.atg:94*/block.Add(Create.UnaryOperation(pos, UnaryOperator.PostDecrement, complex)); 
			} else if (StartOf(49)) {
				Assignment(/*Parser.Statement.atg:95*/complex, out node);
				/*Parser.Statement.atg:95*/if(complex == null && node == null)
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
				AppendRightTermination(/*Parser.Statement.atg:109*/ref complex);
				while (la.kind == _appendright) {
					AppendRightTermination(/*Parser.Statement.atg:110*/ref complex);
				}
				/*Parser.Statement.atg:112*/block.Add(complex);  
			}
		} else SynErr(179);
	}

	void Return(/*Parser.Statement.atg:491*/AstBlock block) {
		/*Parser.Statement.atg:491*/AstReturn ret = null; 
		AstExplicitGoTo jump = null; 
		AstExpr expr; 
		AstLoopBlock bl = target.CurrentLoopBlock;
		
		if (la.kind == _return || la.kind == _yield) {
			if (la.kind == _return) {
				Get();
				/*Parser.Statement.atg:499*/ret = new AstReturn(this, ReturnVariant.Exit); 
			} else {
				Get();
				/*Parser.Statement.atg:500*/ret = new AstReturn(this, ReturnVariant.Continue); 
			}
			if (StartOf(50)) {
				if (StartOf(14)) {
					Expr(/*Parser.Statement.atg:502*/out expr);
					/*Parser.Statement.atg:502*/ret.Expression = expr; 
				} else {
					Get();
					/*Parser.Statement.atg:503*/ret.ReturnVariant = ReturnVariant.Set; 
					Expr(/*Parser.Statement.atg:504*/out expr);
					/*Parser.Statement.atg:504*/ret.Expression = expr; 
					/*Parser.Statement.atg:505*/SemErr("Return value assignment is no longer supported. You must use local variables instead."); 
				}
			}
		} else if (la.kind == _break) {
			Get();
			/*Parser.Statement.atg:507*/if(bl == null)
			   ret = new AstReturn(this, ReturnVariant.Break); 
			else
			    jump = new AstExplicitGoTo(this, bl.BreakLabel);
			
		} else if (la.kind == _continue) {
			Get();
			/*Parser.Statement.atg:512*/if(bl == null)
			   ret = new AstReturn(this, ReturnVariant.Continue); 
			else
			    jump = new AstExplicitGoTo(this, bl.ContinueLabel);
			
		} else SynErr(180);
		/*Parser.Statement.atg:517*/block.Add((AstNode)ret ?? jump); 
	}

	void Throw(/*Parser.Statement.atg:642*/AstBlock block) {
		/*Parser.Statement.atg:642*/AstThrow th; 
		ThrowExpression(/*Parser.Statement.atg:644*/out th);
		/*Parser.Statement.atg:645*/block.Add(th); 
	}

	void LetBindingStmt(/*Parser.Statement.atg:557*/AstBlock block) {
		Expect(_let);
		LetBinder(/*Parser.Statement.atg:558*/block);
		while (la.kind == _comma) {
			Get();
			LetBinder(/*Parser.Statement.atg:558*/block);
		}
	}

	void Condition(/*Parser.Statement.atg:377*/AstBlock block) {
		/*Parser.Statement.atg:377*/AstExpr expr; bool isNegative = false; 
		if (la.kind == _if) {
			Get();
			
		} else if (la.kind == _unless) {
			Get();
			/*Parser.Statement.atg:380*/isNegative = true; 
		} else SynErr(181);
		Expect(_lpar);
		Expr(/*Parser.Statement.atg:383*/out expr);
		Expect(_rpar);
		/*Parser.Statement.atg:383*/if(expr == null)
		   expr = _createUnknownExpr();
		AstCondition cond = Create.Condition(GetPosition(), expr, isNegative);
		_PushScope(cond.IfBlock);
		
		StatementBlock(/*Parser.Statement.atg:389*/cond.IfBlock);
		/*Parser.Statement.atg:390*/_PopScope(cond.IfBlock); 
		if (la.kind == _else) {
			Get();
			/*Parser.Statement.atg:393*/_PushScope(cond.ElseBlock); 
			StatementBlock(/*Parser.Statement.atg:394*/cond.ElseBlock);
			/*Parser.Statement.atg:395*/_PopScope(cond.ElseBlock); 
		}
		/*Parser.Statement.atg:396*/block.Add(cond); 
	}

	void NestedFunction(/*Parser.Statement.atg:521*/AstBlock block) {
		/*Parser.Statement.atg:521*/PFunction func; 
		FunctionDefinition(/*Parser.Statement.atg:523*/out func);
		/*Parser.Statement.atg:525*/string logicalId = func.Meta[PFunction.LogicalIdKey];
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

	void TryCatchFinally(/*Parser.Statement.atg:584*/AstBlock block) {
		/*Parser.Statement.atg:584*/var a = Create.TryCatchFinally(GetPosition());
		AstGetSet excVar;
		
		Expect(_try);
		/*Parser.Statement.atg:588*/_PushScope(a);
		_PushScope(a.TryBlock); 
		
		Expect(_lbrace);
		while (StartOf(20)) {
			Statement(/*Parser.Statement.atg:592*/a.TryBlock);
		}
		Expect(_rbrace);
		
		if (la.kind == _catch || la.kind == _finally) {
			if (la.kind == _catch) {
				Get();
				/*Parser.Statement.atg:595*/_PushScope(a.CatchBlock); 
				if (la.kind == _lpar) {
					Get();
					GetCall(/*Parser.Statement.atg:597*/out excVar);
					/*Parser.Statement.atg:597*/a.ExceptionVar = excVar; 
					Expect(_rpar);
				} else if (la.kind == _lbrace) {
					/*Parser.Statement.atg:599*/SemErr(la,"catch-clauses that don't store the exception are illegal."); 
				} else SynErr(182);
				Expect(_lbrace);
				while (StartOf(20)) {
					Statement(/*Parser.Statement.atg:602*/a.CatchBlock);
				}
				Expect(_rbrace);
				/*Parser.Statement.atg:604*/_PopScope(a.CatchBlock);
				if (la.kind == _finally) {
					Get();
					/*Parser.Statement.atg:607*/_PushScope(a.FinallyBlock); 
					Expect(_lbrace);
					while (StartOf(20)) {
						Statement(/*Parser.Statement.atg:609*/a.FinallyBlock);
					}
					Expect(_rbrace);
					/*Parser.Statement.atg:611*/_PopScope(a.FinallyBlock); 
				}
			} else {
				Get();
				/*Parser.Statement.atg:614*/_PushScope(a.FinallyBlock); 
				Expect(_lbrace);
				while (StartOf(20)) {
					Statement(/*Parser.Statement.atg:616*/a.FinallyBlock);
				}
				Expect(_rbrace);
				/*Parser.Statement.atg:618*/_PopScope(a.FinallyBlock); 
				if (la.kind == _catch) {
					/*Parser.Statement.atg:620*/_PushScope(a.CatchBlock); 
					Get();
					if (la.kind == _lpar) {
						Get();
						GetCall(/*Parser.Statement.atg:623*/out excVar);
						/*Parser.Statement.atg:624*/a.ExceptionVar = excVar; 
						Expect(_rpar);
					} else if (la.kind == _lbrace) {
						/*Parser.Statement.atg:626*/SemErr(la,"catch-clauses that don't store the exception are illegal."); 
					} else SynErr(183);
					Expect(_lbrace);
					while (StartOf(20)) {
						Statement(/*Parser.Statement.atg:629*/a.CatchBlock);
					}
					Expect(_rbrace);
					/*Parser.Statement.atg:632*/_PopScope(a.CatchBlock); 
				}
			}
		}
		/*Parser.Statement.atg:635*/_PopScope(a.TryBlock);
		_PopScope(a);
		block.Add(a); 
		
	}

	void Using(/*Parser.Statement.atg:649*/AstBlock block) {
		/*Parser.Statement.atg:649*/AstUsing use = Create.Using(GetPosition());
		AstExpr e;
		
		/*Parser.Statement.atg:653*/_PushScope(use);
		_PushScope(use.Block); 
		Expect(_uusing);
		Expect(_lpar);
		Expr(/*Parser.Statement.atg:655*/out e);
		Expect(_rpar);
		/*Parser.Statement.atg:656*/use.ResourceExpression = e; 
		StatementBlock(/*Parser.Statement.atg:658*/use.Block);
		/*Parser.Statement.atg:659*/_PopScope(use.Block);
		_PopScope(use);
		                           block.Add(use); 
		                       
	}

	void Assignment(/*Parser.Statement.atg:341*/AstGetSet lvalue, out AstNode node) {
		/*Parser.Statement.atg:341*/AstExpr expr = null;
		BinaryOperator setModifier = BinaryOperator.None;
		AstTypeExpr typeExpr;
		node = lvalue;
		                           ISourcePosition position;
		
		/*Parser.Statement.atg:347*/position = GetPosition(); 
		if (StartOf(9)) {
			switch (la.kind) {
			case _assign: {
				Get();
				/*Parser.Statement.atg:349*/setModifier = BinaryOperator.None; 
				break;
			}
			case _plus: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:350*/setModifier = BinaryOperator.Addition; 
				break;
			}
			case _minus: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:351*/setModifier = BinaryOperator.Subtraction; 
				break;
			}
			case _times: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:352*/setModifier = BinaryOperator.Multiply; 
				break;
			}
			case _div: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:353*/setModifier = BinaryOperator.Division; 
				break;
			}
			case _bitAnd: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:354*/setModifier = BinaryOperator.BitwiseAnd; 
				break;
			}
			case _bitOr: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:355*/setModifier = BinaryOperator.BitwiseOr; 
				break;
			}
			case _coalescence: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:356*/setModifier = BinaryOperator.Coalescence; 
				break;
			}
			}
			Expr(/*Parser.Statement.atg:357*/out expr);
		} else if (la.kind == _tilde) {
			Get();
			Expect(_assign);
			/*Parser.Statement.atg:359*/setModifier = BinaryOperator.Cast; 
			TypeExpr(/*Parser.Statement.atg:360*/out typeExpr);
			/*Parser.Statement.atg:360*/expr = typeExpr; 
		} else SynErr(184);
		/*Parser.Statement.atg:362*/if(expr == null)
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

	void AppendRightTermination(/*Parser.Statement.atg:118*/ref AstGetSet complex) {
		/*Parser.Statement.atg:118*/AstGetSet rhs; 
		Expect(_appendright);
		GetCall(/*Parser.Statement.atg:121*/out rhs);
		/*Parser.Statement.atg:121*/_appendRight(complex,rhs);
		complex = rhs;
		
	}

	void SymbolicUsage(/*Parser.Statement.atg:310*/out AstGetSet complex) {
		/*Parser.Statement.atg:310*/string id; ISourcePosition position; 
		
		/*Parser.Statement.atg:312*/position = GetPosition(); 
		Id(/*Parser.Statement.atg:313*/out id);
		/*Parser.Statement.atg:313*/complex = _useSymbol(Symbols, id, position); 
		Arguments(/*Parser.Statement.atg:314*/complex.Arguments);
	}

	void VariableDeclaration(/*Parser.Statement.atg:217*/out AstGetSet complex) {
		/*Parser.Statement.atg:217*/string id, physicalId;
		bool isOverrideDecl = false;
		bool seenVar = false;
		int refCount = 1;
		bool isUnbound = false;
		bool isStatic = false;
		Symbol sym, varSym;
		
		if (la.kind == _new) {
			Get();
			/*Parser.Statement.atg:226*/isUnbound = true; 
		}
		if (la.kind == _static) {
			Get();
			/*Parser.Statement.atg:228*/isStatic = true; 
			while (la.kind == _ref) {
				Get();
				/*Parser.Statement.atg:229*/refCount++; 
			}
			if (la.kind == _var) {
				Get();
			}
		} else if (la.kind == _var || la.kind == _ref) {
			if (la.kind == _var) {
				Get();
				/*Parser.Statement.atg:232*/seenVar = true; 
			} else {
				Get();
				/*Parser.Statement.atg:233*/refCount++; 
			}
			while (la.kind == _var || la.kind == _ref) {
				if (la.kind == _var) {
					Get();
					/*Parser.Statement.atg:235*/if(seenVar)
					{
					    Loader.ReportMessage(Message.Error("Duplicate ocurrence of `var` in local variable declaration.",GetPosition(),MessageClasses.DuplicateVar));
					    // This is just a stylistic rule. There are no consequences to having duplicate `var` keywords in a declaration.
					}
					seenVar = true;
					
				} else {
					Get();
					/*Parser.Statement.atg:242*/refCount++; 
				}
			}
		} else SynErr(185);
		if (la.kind == _new) {
			Get();
			/*Parser.Statement.atg:246*/isOverrideDecl = true; 
		}
		/*Parser.Statement.atg:247*/ISourcePosition position = GetPosition(); 
		Id(/*Parser.Statement.atg:248*/out id);
		/*Parser.Statement.atg:248*/physicalId = id;
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

	void StaticCall(/*Parser.Statement.atg:318*/out AstGetSetStatic staticCall) {
		/*Parser.Statement.atg:318*/AstTypeExpr typeExpr;
		string memberId;
		
		ExplicitTypeExpr(/*Parser.Statement.atg:322*/out typeExpr);
		Expect(_dot);
		Id(/*Parser.Statement.atg:323*/out memberId);
		/*Parser.Statement.atg:323*/staticCall = new AstGetSetStatic(this, PCall.Get, typeExpr, memberId); 
		Arguments(/*Parser.Statement.atg:324*/staticCall.Arguments);
	}

	void LetBinder(/*Parser.Statement.atg:562*/AstBlock block) {
		/*Parser.Statement.atg:562*/string id = null;
		AstExpr thunk;
		
		/*Parser.Statement.atg:565*/var position = GetPosition(); 
		Id(/*Parser.Statement.atg:566*/out id);
		/*Parser.Statement.atg:567*/_ensureDefinedLocal(id,id,false,position,false);
		mark_as_let(target.Function, id);
		if(la.kind == _assign)
		    _inject(_lazy,"lazy"); 
		
		if (la.kind == _assign) {
			Get();
			LazyExpression(/*Parser.Statement.atg:573*/out thunk);
			/*Parser.Statement.atg:576*/var assign = Create.Call(position, EntityRef.Variable.Local.Create(id), PCall.Set);
			assign.Arguments.Add(thunk);
			block.Add(assign);
			
		}
	}


#line 133 "C:\Cold\Users\Christian\Documents\GitHub\prx\Tools\Parser.frame" //FRAME


	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();

#line default //END FRAME -->parseRoot

		Prexonite();

#line 139 "C:\Cold\Users\Christian\Documents\GitHub\prx\Tools\Parser.frame" //FRAME

    Expect(0);
	}
	
	private readonly bool[,] set = {

#line default //END FRAME -->initialization

		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,T,T, x,x,x,T, T,T,T,T, T,x,T,x, x,T,T,T, T,T,T,T, x,T,T,T, x,T,x,T, T,T,T,T, x,x,x,x, T,T,T,T, T,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,T,T,T, T,x,T,x, x,T,T,T, T,T,T,T, x,T,T,T, x,T,x,T, T,T,T,T, x,x,x,x, T,T,T,T, T,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, T,x,x,x, T,x,x,x, x,x,x,T, x,x,T,T, x,x,x,T, T,T,T,T, T,x,T,x, x,T,T,T, T,T,T,T, x,T,T,T, x,T,x,T, T,T,T,T, x,x,x,x, T,T,T,T, T,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,T,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,T,T,x, x,x,T,x, x,x,T,T, T,x,T,T, x,T,T,T, x,T,x,T, T,T,T,T, x,x,x,x, T,T,T,T, T,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,T,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, T,x,T,T, T,T,x,x, x,T,x,x, x,x,x,T, T,T,T,x, x,T,x,x, T,x,x,x, x,T,x,x, x,x,T,x, T,T,x,x, x,x,T,T, T,T,x,x, x,x,T,T, T,x,x,T, x,T,x,x, x,T,x,x, x,x,x,x, x,x,T,T, x,x,x,T, T,x,T,x, T,T,T,T, x,x,x,T, x,x,x,T, x,x,T,x, x,x,T,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,T,T, x,x,T,x, x,x,x,x, x,x,x,x, x,T,x,T, T,x,x,x, x,x,T,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,T,T, T,T,T,T, T,T,T,T, x,T,x,T, T,T,T,T, T,T,T,T, T,T,T,x, T,T,x,T, x,x,x,x, x,T,x,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,T,T,T, x,x,x,x, x,x,T,x, x,x,x,x, x,x,T,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,T, x,x,x,T, x,T,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,T,x,x, x,T,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,T, x,x,x,x, x,T,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, T,x,T,T, T,T,x,x, x,T,x,x, x,x,x,T, T,T,T,x, x,T,x,x, T,x,x,x, x,T,x,x, x,x,T,x, T,T,x,x, x,x,T,T, T,T,x,x, x,x,T,T, T,x,x,T, x,T,x,x, x,T,x,x, x,x,x,x, x,x,T,T, T,T,x,T, T,x,T,x, T,T,T,T, x,x,x,T, x,x,x,T, x,x,T,x, x,x,T,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, T,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,T,x, T,T,x,x, x,x,T,T, x,x,x,x, x,x,x,T, T,x,x,x, x,T,x,x, x,T,x,x, x,x,x,x, x,x,T,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, x,x,x,T, T,x,x,x, x,T,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,T,x, x,x,x,x, x,x,x,x, x,T,x,x, T,x,T,x, T,T,x,x, x,x,T,T, x,x,x,x, x,x,x,T, T,T,x,T, T,T,T,x, x,T,T,T, T,x,x,x, x,T,T,x, T,T,x,T, T,x,T,x, T,T,T,T, T,x,x,T, x,T,T,T, T,x,x,x, x,x,x,x, x},
		{x,T,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,T,x, x,x,x,x, x,x,T,T, x,x,x,x, x,x,x,T, T,x,x,x, x,T,x,x, x,T,x,x, x,x,x,x, x,x,T,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,T,x,x, T,x,T,x, T,T,x,x, x,x,T,T, x,x,x,x, x,x,x,T, T,x,x,x, x,T,T,x, x,T,T,T, T,x,x,x, x,T,T,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, T,x,x,x, x,x,x,x, x},
		{x,T,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,T,x, T,T,x,x, x,x,T,T, x,x,x,x, x,x,x,T, T,x,x,x, x,T,T,x, x,T,T,T, T,x,x,x, x,T,T,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, T,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, T,x,T,x, T,T,T,T, T,x,x,x, x,T,T,T, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, T,x,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,T,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,T,x, x,T,x,x, T,T,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,T, x,x,x,x, x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,T,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,T,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,T,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, T,x,T,T, T,T,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,T,x,x, x,x,T,T, x,x,x,x, x,x,x,T, T,T,T,x, x,T,x,x, x,T,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,T, x,x,x,x, x,x,x,T, T,x,x,x, x,T,x,x, x,T,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,T,x,x, x,x,T,T, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, x,x,x,T, T,x,x,x, x,T,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, x,x,x,T, T,x,x,x, x,T,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{T,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,T,x, x,T,x,x, T,T,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,T, x,x,x,x, x},
		{x,T,T,x, T,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,T,x,x, x,x,x,T, x,x,x,x, x,x,x,T, T,x,x,x, x,T,x,x, x,T,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,T,T,x, x,T,x,x, T,T,T,T, x,x,T,T, T,T,x,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,x, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,x, x,x,x,x, x},
		{x,T,T,x, x,T,T,x, T,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, T,T,x,x, x,T,T,x, x,T,x,x, T,T,T,T, x,x,T,T, T,T,x,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,T,T,x, x,T,x,x, T,T,T,T, x,x,T,T, T,T,x,T, T,T,T,x, x,T,T,T, T,T,T,T, T,x,T,T, T,T,T,T, T,T,T,x, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,T, T,T,T,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,T,T, x,T,T,x, x,x,x,T, x,x,x,x, x,T,x,T, T,x,x,x, x,T,T,x, x,x,x,T, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,T,T, x,x,T,x, x,x,x,x, x,x,x,x, x,T,x,T, T,x,x,x, x,T,T,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, T,x,T,T, T,T,x,T, x,T,x,x, x,x,x,T, T,T,T,x, x,T,x,x, T,x,x,x, x,T,x,x, x,x,T,x, T,T,x,x, x,x,T,T, T,T,x,x, x,x,T,T, T,x,x,T, x,T,x,x, x,T,x,x, x,x,x,x, x,x,T,T, T,T,x,T, T,x,T,x, T,T,T,T, x,x,x,T, x,x,x,T, x,x,T,x, x,x,T,x, x}

#line 144 "C:\Cold\Users\Christian\Documents\GitHub\prx\Tools\Parser.frame" //FRAME

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
			case 35: s = "timessym expected"; break;
			case 36: s = "semicolon expected"; break;
			case 37: s = "colon expected"; break;
			case 38: s = "doublecolon expected"; break;
			case 39: s = "coalescence expected"; break;
			case 40: s = "question expected"; break;
			case 41: s = "pointer expected"; break;
			case 42: s = "implementation expected"; break;
			case 43: s = "at expected"; break;
			case 44: s = "appendleft expected"; break;
			case 45: s = "appendright expected"; break;
			case 46: s = "var expected"; break;
			case 47: s = "ref expected"; break;
			case 48: s = "true expected"; break;
			case 49: s = "false expected"; break;
			case 50: s = "BEGINKEYWORDS expected"; break;
			case 51: s = "mod expected"; break;
			case 52: s = "is expected"; break;
			case 53: s = "as expected"; break;
			case 54: s = "not expected"; break;
			case 55: s = "enabled expected"; break;
			case 56: s = "disabled expected"; break;
			case 57: s = "function expected"; break;
			case 58: s = "command expected"; break;
			case 59: s = "asm expected"; break;
			case 60: s = "declare expected"; break;
			case 61: s = "build expected"; break;
			case 62: s = "return expected"; break;
			case 63: s = "in expected"; break;
			case 64: s = "to expected"; break;
			case 65: s = "add expected"; break;
			case 66: s = "continue expected"; break;
			case 67: s = "break expected"; break;
			case 68: s = "yield expected"; break;
			case 69: s = "or expected"; break;
			case 70: s = "and expected"; break;
			case 71: s = "xor expected"; break;
			case 72: s = "label expected"; break;
			case 73: s = "goto expected"; break;
			case 74: s = "static expected"; break;
			case 75: s = "null expected"; break;
			case 76: s = "if expected"; break;
			case 77: s = "unless expected"; break;
			case 78: s = "else expected"; break;
			case 79: s = "new expected"; break;
			case 80: s = "coroutine expected"; break;
			case 81: s = "from expected"; break;
			case 82: s = "do expected"; break;
			case 83: s = "does expected"; break;
			case 84: s = "while expected"; break;
			case 85: s = "until expected"; break;
			case 86: s = "for expected"; break;
			case 87: s = "foreach expected"; break;
			case 88: s = "try expected"; break;
			case 89: s = "catch expected"; break;
			case 90: s = "finally expected"; break;
			case 91: s = "throw expected"; break;
			case 92: s = "then expected"; break;
			case 93: s = "uusing expected"; break;
			case 94: s = "macro expected"; break;
			case 95: s = "lazy expected"; break;
			case 96: s = "let expected"; break;
			case 97: s = "method expected"; break;
			case 98: s = "this expected"; break;
			case 99: s = "namespace expected"; break;
			case 100: s = "export expected"; break;
			case 101: s = "ENDKEYWORDS expected"; break;
			case 102: s = "LPopExpr expected"; break;
			case 103: s = "??? expected"; break;
			case 104: s = "invalid AsmStatementBlock"; break;
			case 105: s = "invalid AsmInstruction"; break;
			case 106: s = "invalid AsmInstruction"; break;
			case 107: s = "invalid AsmInstruction"; break;
			case 108: s = "invalid AsmInstruction"; break;
			case 109: s = "invalid AsmInstruction"; break;
			case 110: s = "invalid AsmId"; break;
			case 111: s = "invalid SignedReal"; break;
			case 112: s = "invalid Boolean"; break;
			case 113: s = "invalid Id"; break;
			case 114: s = "invalid Expr"; break;
			case 115: s = "invalid AssignExpr"; break;
			case 116: s = "invalid AssignExpr"; break;
			case 117: s = "invalid TypeExpr"; break;
			case 118: s = "invalid GetSetExtension"; break;
			case 119: s = "invalid Primary"; break;
			case 120: s = "invalid Constant"; break;
			case 121: s = "invalid ListLiteral"; break;
			case 122: s = "invalid HashLiteral"; break;
			case 123: s = "invalid LoopExpr"; break;
			case 124: s = "invalid LambdaExpression"; break;
			case 125: s = "invalid LambdaExpression"; break;
			case 126: s = "invalid LazyExpression"; break;
			case 127: s = "invalid GetInitiator"; break;
			case 128: s = "invalid GetInitiator"; break;
			case 129: s = "invalid Real"; break;
			case 130: s = "invalid WhileLoop"; break;
			case 131: s = "invalid WhileLoop"; break;
			case 132: s = "invalid ForLoop"; break;
			case 133: s = "invalid ForLoop"; break;
			case 134: s = "invalid Arguments"; break;
			case 135: s = "invalid Arguments"; break;
			case 136: s = "invalid Statement"; break;
			case 137: s = "invalid ExplicitTypeExpr"; break;
			case 138: s = "invalid PrexoniteTypeExpr"; break;
			case 139: s = "invalid ClrTypeExpr"; break;
			case 140: s = "invalid TypeExprElement"; break;
			case 141: s = "this symbol not expected in DeclarationLevel"; break;
			case 142: s = "invalid DeclarationLevel"; break;
			case 143: s = "invalid GlobalVariableDefinition"; break;
			case 144: s = "invalid GlobalVariableDefinition"; break;
			case 145: s = "invalid MetaAssignment"; break;
			case 146: s = "invalid MetaAssignment"; break;
			case 147: s = "this symbol not expected in Declaration2"; break;
			case 148: s = "invalid Declaration2"; break;
			case 149: s = "this symbol not expected in BuildBlock"; break;
			case 150: s = "invalid FunctionDefinition"; break;
			case 151: s = "invalid FunctionDefinition"; break;
			case 152: s = "invalid FunctionDefinition"; break;
			case 153: s = "invalid FunctionDefinition"; break;
			case 154: s = "invalid NamespaceDeclaration"; break;
			case 155: s = "invalid NamespaceDeclaration"; break;
			case 156: s = "invalid NamespaceDeclaration"; break;
			case 157: s = "invalid GlobalId"; break;
			case 158: s = "invalid MetaExpr"; break;
			case 159: s = "invalid MetaExpr"; break;
			case 160: s = "invalid GlobalQualifiedId"; break;
			case 161: s = "invalid Version"; break;
			case 162: s = "invalid SymbolPrefix"; break;
			case 163: s = "invalid EntityFactory"; break;
			case 164: s = "invalid EntityFactory"; break;
			case 165: s = "invalid MExpr"; break;
			case 166: s = "invalid MExpr"; break;
			case 167: s = "invalid MessageDirective"; break;
			case 168: s = "invalid MessageDirective"; break;
			case 169: s = "invalid SymbolDirective"; break;
			case 170: s = "invalid NsTransferSpec"; break;
			case 171: s = "invalid NsQualifiedIdImpl"; break;
			case 172: s = "invalid NsTransferDirective"; break;
			case 173: s = "invalid NsTransferDirective"; break;
			case 174: s = "invalid NsTransferDirectiveGroup"; break;
			case 175: s = "invalid MId"; break;
			case 176: s = "invalid ExplicitLabel"; break;
			case 177: s = "invalid SimpleStatement"; break;
			case 178: s = "invalid StructureStatement"; break;
			case 179: s = "invalid GetSetComplex"; break;
			case 180: s = "invalid Return"; break;
			case 181: s = "invalid Condition"; break;
			case 182: s = "invalid TryCatchFinally"; break;
			case 183: s = "invalid TryCatchFinally"; break;
			case 184: s = "invalid Assignment"; break;
			case 185: s = "invalid VariableDeclaration"; break;

#line 171 "C:\Cold\Users\Christian\Documents\GitHub\prx\Tools\Parser.frame" //FRAME

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