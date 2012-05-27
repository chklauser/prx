//SOURCE ARRAY
/*Header.atg:29*/using System.IO;
using Prexonite;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using FatalError = Prexonite.Compiler.FatalCompilerException;
using StringBuilder = System.Text.StringBuilder;
using Prexonite.Compiler.Ast;
using Prexonite.Types;
using Prexonite.Modular;//END SOURCE ARRAY


#line 27 "D:\DotNetProjects\Prexonite-Hg\prx-assembla-hg\Tools\Parser.frame" //FRAME

using System;


#line default //END FRAME -->namespace

namespace Prexonite.Compiler {


#line 30 "D:\DotNetProjects\Prexonite-Hg\prx-assembla-hg\Tools\Parser.frame" //FRAME


using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

internal interface IScanner
{
    Token Scan();
    Token Peek();
    void ResetPeek();
    string File { get; }
}

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
	public const int _ENDKEYWORDS = 96;
	public const int _LPopExpr = 97;
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
		@ENDKEYWORDS = 96,
		@LPopExpr = 97,
	}
	const int maxT = 98;

#line 44 "D:\DotNetProjects\Prexonite-Hg\prx-assembla-hg\Tools\Parser.frame" //FRAME

	const bool T = true;
	const bool x = false;
	const int minErrDist = 2;
	
	internal IScanner scanner;
	internal Errors  errors;

	internal Token t;    // last recognized token
	internal Token la;   // lookahead token
	int errDist = minErrDist;


#line default //END FRAME -->declarations

//SOURCE ARRAY
//END SOURCE ARRAY

#line 56 "D:\DotNetProjects\Prexonite-Hg\prx-assembla-hg\Tools\Parser.frame" //FRAME


    [NoDebug()]
	private Parser(IScanner scanner) {
		this.scanner = scanner;
		errors = new Errors();
		errors.parentParser = this;
	}

    [NoDebug()]
	void SynErr (int n) {
		if (errDist >= minErrDist) errors.SynErr(la.line, la.col, n);
		errDist = 0;
	}

    [NoDebug()]
	internal void SemErr (string msg) {
		if (errDist >= minErrDist) errors.SemErr(t.line, t.col, msg);
		errDist = 0;
	}
	
	[NoDebug()]
	void Get () {
		for (;;) {
			t = la;
			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }

#line default //END FRAME -->pragmas


#line 83 "D:\DotNetProjects\Prexonite-Hg\prx-assembla-hg\Tools\Parser.frame" //FRAME

			la = t;
		}
	}
	
	[NoDebug()]
	void Expect (int n) {
		if (la.kind==n) Get(); else { SynErr(n); }
	}
	
	[NoDebug()]
	bool StartOf (int s) {
		return set[s, la.kind];
	}
	
	[NoDebug()]
	void ExpectWeak (int n, int follow) {
		if (la.kind == n) Get();
		else {
			SynErr(n);
			while (!StartOf(follow)) Get();
		}
	}
	
	[NoDebug()]
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
		} else SynErr(99);
	}

	void AsmInstruction(/*Parser.Assembler.atg:37*/AstBlock block) {
		/*Parser.Assembler.atg:37*/int arguments = 0;
		string id = null;
		double dblArg = 0.0;
		string insbase = null; string detail = null;
		bool bolArg = false;
		OpCode code;
		bool justEffect = false;
		int values = 0;
		int rotations = 0;
		int index = 0;
		
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
			SmartDeclareLocal(kind, id);
			
			while (la.kind == _comma) {
				Get();
				AsmId(/*Parser.Assembler.atg:60*/out id);
				/*Parser.Assembler.atg:62*/target.Function.Variables.Add(id);
				SmartDeclareLocal(kind, id);
				
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
			/*Parser.Assembler.atg:88*/Instruction ins = new Instruction(OpCode.nop); 
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
			} else SynErr(100);
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
			} else SynErr(101);
			AsmId(/*Parser.Assembler.atg:177*/out id);
			/*Parser.Assembler.atg:178*/code = getOpCode(insbase, detail);
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
			} else SynErr(102);
			/*Parser.Assembler.atg:190*/code = getOpCode(insbase, detail);
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
			} else SynErr(103);
			AsmQualid(/*Parser.Assembler.atg:202*/out id);
			/*Parser.Assembler.atg:203*/code = getOpCode(insbase, detail);
			addInstruction(block, new Instruction(code, arguments, id, justEffect));
			
		} else if (StartOf(2)) {
			AsmId(/*Parser.Assembler.atg:208*/out insbase);
			/*Parser.Assembler.atg:208*/SemErr("Invalid assembler instruction \"" + insbase + "\" (" + t + ")."); 
		} else SynErr(104);
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
		} else SynErr(105);
	}

	void Integer(/*Parser.Helper.atg:42*/out int value) {
		Expect(_integer);
		/*Parser.Helper.atg:43*/if(!TryParseInteger(t.val, out value))
		   SemErr(t, "Cannot recognize integer " + t.val);
		
	}

	void SignedReal(/*Parser.Helper.atg:71*/out double value) {
		/*Parser.Helper.atg:71*/value = 0.0; double modifier = 1.0; int ival; 
		if (la.kind == _minus || la.kind == _plus) {
			if (la.kind == _plus) {
				Get();
			} else {
				Get();
				/*Parser.Helper.atg:74*/modifier = -1.0; 
			}
		}
		if (la.kind == _real || la.kind == _realLike) {
			Real(/*Parser.Helper.atg:75*/out value);
		} else if (la.kind == _integer) {
			Integer(/*Parser.Helper.atg:76*/out ival);
			/*Parser.Helper.atg:76*/value = ival; 
		} else SynErr(106);
		/*Parser.Helper.atg:78*/value = modifier * value; 
	}

	void Boolean(/*Parser.Helper.atg:35*/out bool value) {
		/*Parser.Helper.atg:35*/value = true; 
		if (la.kind == _true) {
			Get();
		} else if (la.kind == _false) {
			Get();
			/*Parser.Helper.atg:38*/value = false; 
		} else SynErr(107);
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

	void AsmQualid(/*Parser.Assembler.atg:249*/out string qualid) {
		
		AsmId(/*Parser.Assembler.atg:251*/out qualid);
	}

	void String(/*Parser.Helper.atg:82*/out string value) {
		Expect(_string);
		/*Parser.Helper.atg:83*/value = cache(t.val); 
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
		} else SynErr(108);
	}

	void Expr(/*Parser.Expression.atg:26*/out AstExpr expr) {
		/*Parser.Expression.atg:26*/AstConditionalExpression cexpr = null; expr = null; 
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
		} else SynErr(109);
	}

	void AtomicExpr(/*Parser.Expression.atg:41*/out AstExpr expr) {
		/*Parser.Expression.atg:41*/AstExpr outerExpr = null; 
		AppendRightExpr(/*Parser.Expression.atg:43*/out expr);
		while (la.kind == _then) {
			Get();
			AppendRightExpr(/*Parser.Expression.atg:46*/out outerExpr);
			/*Parser.Expression.atg:46*/AstGetSetSymbol thenExpr = new AstGetSetSymbol(
			   this, PCall.Get, SymbolEntry.Command(Engine.ThenAlias));
			thenExpr.Arguments.Add(expr);
			thenExpr.Arguments.Add(outerExpr);
			expr = thenExpr;
			
		}
	}

	void AppendRightExpr(/*Parser.Expression.atg:57*/out AstExpr expr) {
		/*Parser.Expression.atg:57*/AstGetSet complex = null; 
		KeyValuePairExpr(/*Parser.Expression.atg:59*/out expr);
		while (la.kind == _appendright) {
			Get();
			GetCall(/*Parser.Expression.atg:62*/out complex);
			/*Parser.Expression.atg:62*/complex.Arguments.RightAppend(expr); 
			complex.Arguments.ReleaseRightAppend();
			if(complex is AstGetSetSymbol 
			                                      && ((AstGetSetSymbol)complex).IsObjectVariable)
			    complex.Call = PCall.Set;
			expr = complex;										    
			
		}
	}

	void KeyValuePairExpr(/*Parser.Expression.atg:73*/out AstExpr expr) {
		OrExpr(/*Parser.Expression.atg:74*/out expr);
		if (la.kind == _colon) {
			Get();
			/*Parser.Expression.atg:75*/AstExpr value; 
			KeyValuePairExpr(/*Parser.Expression.atg:76*/out value);
			/*Parser.Expression.atg:76*/expr = new AstKeyValuePair(this, expr, value); 
		}
	}

	void GetCall(/*Parser.Statement.atg:504*/out AstGetSet complex) {
		/*Parser.Statement.atg:504*/AstGetSet getMember = null; bool isDeclaration; 
		GetInitiator(/*Parser.Statement.atg:506*/out complex, out isDeclaration);
		while (la.kind == _dot || la.kind == _lbrack) {
			GetSetExtension(/*Parser.Statement.atg:507*/complex, out getMember);
		}
		/*Parser.Statement.atg:509*/if(getMember != null) 
		{
		    complex = getMember; 
		}
		else
		{
		    AstGetSetSymbol symbol = complex as AstGetSetSymbol;
		    if(symbol != null 
		        && InterpretationIsVariable(symbol.Implementation.Interpretation) 
		        && isDeclaration)
		    {
		        symbol.Implementation = symbol.Implementation.With(InterpretAsObjectVariable(symbol.Implementation.Interpretation));
		        complex = symbol;
		    }                                        
		} 
	}

	void OrExpr(/*Parser.Expression.atg:81*/out AstExpr expr) {
		/*Parser.Expression.atg:81*/AstExpr lhs, rhs; 
		AndExpr(/*Parser.Expression.atg:83*/out lhs);
		/*Parser.Expression.atg:83*/expr = lhs; 
		if (la.kind == _or) {
			Get();
			OrExpr(/*Parser.Expression.atg:84*/out rhs);
			/*Parser.Expression.atg:84*/expr = new AstLogicalOr(this, lhs, rhs); 
		}
	}

	void AndExpr(/*Parser.Expression.atg:90*/out AstExpr expr) {
		/*Parser.Expression.atg:90*/AstExpr lhs, rhs; 
		BitOrExpr(/*Parser.Expression.atg:92*/out lhs);
		/*Parser.Expression.atg:92*/expr = lhs; 
		if (la.kind == _and) {
			Get();
			AndExpr(/*Parser.Expression.atg:93*/out rhs);
			/*Parser.Expression.atg:93*/expr = new AstLogicalAnd(this, lhs, rhs); 
		}
	}

	void BitOrExpr(/*Parser.Expression.atg:98*/out AstExpr expr) {
		/*Parser.Expression.atg:98*/AstExpr lhs, rhs; 
		BitXorExpr(/*Parser.Expression.atg:100*/out lhs);
		/*Parser.Expression.atg:100*/expr = lhs; 
		while (la.kind == _bitOr) {
			Get();
			BitXorExpr(/*Parser.Expression.atg:101*/out rhs);
			/*Parser.Expression.atg:101*/expr = AstBinaryOperator.Create(this, expr, BinaryOperator.BitwiseOr, rhs); 
		}
	}

	void BitXorExpr(/*Parser.Expression.atg:106*/out AstExpr expr) {
		/*Parser.Expression.atg:106*/AstExpr lhs, rhs; 
		BitAndExpr(/*Parser.Expression.atg:108*/out lhs);
		/*Parser.Expression.atg:108*/expr = lhs; 
		while (la.kind == _xor) {
			Get();
			BitAndExpr(/*Parser.Expression.atg:109*/out rhs);
			/*Parser.Expression.atg:110*/expr = AstBinaryOperator.Create(this, expr, BinaryOperator.ExclusiveOr, rhs); 
		}
	}

	void BitAndExpr(/*Parser.Expression.atg:115*/out AstExpr expr) {
		/*Parser.Expression.atg:115*/AstExpr lhs, rhs; 
		NotExpr(/*Parser.Expression.atg:117*/out lhs);
		/*Parser.Expression.atg:117*/expr = lhs; 
		while (la.kind == _bitAnd) {
			Get();
			NotExpr(/*Parser.Expression.atg:118*/out rhs);
			/*Parser.Expression.atg:119*/expr = AstBinaryOperator.Create(this, expr, BinaryOperator.BitwiseAnd, rhs); 
		}
	}

	void NotExpr(/*Parser.Expression.atg:124*/out AstExpr expr) {
		/*Parser.Expression.atg:124*/AstExpr lhs; bool isNot = false; 
		if (la.kind == _not) {
			Get();
			/*Parser.Expression.atg:126*/isNot = true; 
		}
		EqlExpr(/*Parser.Expression.atg:128*/out lhs);
		/*Parser.Expression.atg:128*/expr = isNot ? AstUnaryOperator._Create(this, UnaryOperator.LogicalNot, lhs) : lhs; 
	}

	void EqlExpr(/*Parser.Expression.atg:132*/out AstExpr expr) {
		/*Parser.Expression.atg:132*/AstExpr lhs, rhs; BinaryOperator op = BinaryOperator.None; 
		RelExpr(/*Parser.Expression.atg:134*/out lhs);
		/*Parser.Expression.atg:134*/expr = lhs; 
		while (la.kind == _eq || la.kind == _ne) {
			if (la.kind == _eq) {
				Get();
				/*Parser.Expression.atg:135*/op = BinaryOperator.Equality; 
			} else {
				Get();
				/*Parser.Expression.atg:136*/op = BinaryOperator.Inequality; 
			}
			RelExpr(/*Parser.Expression.atg:137*/out rhs);
			/*Parser.Expression.atg:137*/expr = AstBinaryOperator.Create(this, expr, op, rhs); 
		}
	}

	void RelExpr(/*Parser.Expression.atg:142*/out AstExpr expr) {
		/*Parser.Expression.atg:142*/AstExpr lhs, rhs; BinaryOperator op = BinaryOperator.None;  
		CoalExpr(/*Parser.Expression.atg:144*/out lhs);
		/*Parser.Expression.atg:144*/expr = lhs; 
		while (StartOf(8)) {
			if (la.kind == _lt) {
				Get();
				/*Parser.Expression.atg:145*/op = BinaryOperator.LessThan;              
			} else if (la.kind == _le) {
				Get();
				/*Parser.Expression.atg:146*/op = BinaryOperator.LessThanOrEqual;       
			} else if (la.kind == _gt) {
				Get();
				/*Parser.Expression.atg:147*/op = BinaryOperator.GreaterThan;           
			} else {
				Get();
				/*Parser.Expression.atg:148*/op = BinaryOperator.GreaterThanOrEqual;    
			}
			CoalExpr(/*Parser.Expression.atg:149*/out rhs);
			/*Parser.Expression.atg:149*/expr = AstBinaryOperator.Create(this, expr, op, rhs); 
		}
	}

	void CoalExpr(/*Parser.Expression.atg:154*/out AstExpr expr) {
		/*Parser.Expression.atg:154*/AstExpr lhs, rhs; AstCoalescence coal = new AstCoalescence(this); 
		AddExpr(/*Parser.Expression.atg:156*/out lhs);
		/*Parser.Expression.atg:156*/expr = lhs; coal.Expressions.Add(lhs); 
		while (la.kind == _coalescence) {
			Get();
			AddExpr(/*Parser.Expression.atg:159*/out rhs);
			/*Parser.Expression.atg:159*/expr = coal; coal.Expressions.Add(rhs); 
		}
	}

	void AddExpr(/*Parser.Expression.atg:164*/out AstExpr expr) {
		/*Parser.Expression.atg:164*/AstExpr lhs,rhs; BinaryOperator op = BinaryOperator.None; 
		MulExpr(/*Parser.Expression.atg:166*/out lhs);
		/*Parser.Expression.atg:166*/expr = lhs; 
		while (la.kind == _minus || la.kind == _plus) {
			if (la.kind == _plus) {
				Get();
				/*Parser.Expression.atg:167*/op = BinaryOperator.Addition;      
			} else {
				Get();
				/*Parser.Expression.atg:168*/op = BinaryOperator.Subtraction;   
			}
			MulExpr(/*Parser.Expression.atg:169*/out rhs);
			/*Parser.Expression.atg:169*/expr = AstBinaryOperator.Create(this, expr, op, rhs); 
		}
	}

	void MulExpr(/*Parser.Expression.atg:174*/out AstExpr expr) {
		/*Parser.Expression.atg:174*/AstExpr lhs, rhs; BinaryOperator op = BinaryOperator.None; 
		PowExpr(/*Parser.Expression.atg:176*/out lhs);
		/*Parser.Expression.atg:176*/expr = lhs; 
		while (la.kind == _div || la.kind == _times || la.kind == _mod) {
			if (la.kind == _times) {
				Get();
				/*Parser.Expression.atg:177*/op = BinaryOperator.Multiply;      
			} else if (la.kind == _div) {
				Get();
				/*Parser.Expression.atg:178*/op = BinaryOperator.Division;        
			} else {
				Get();
				/*Parser.Expression.atg:179*/op = BinaryOperator.Modulus;       
			}
			PowExpr(/*Parser.Expression.atg:180*/out rhs);
			/*Parser.Expression.atg:180*/expr = AstBinaryOperator.Create(this, expr, op, rhs); 
		}
	}

	void PowExpr(/*Parser.Expression.atg:185*/out AstExpr expr) {
		/*Parser.Expression.atg:185*/AstExpr lhs, rhs; 
		AssignExpr(/*Parser.Expression.atg:187*/out lhs);
		/*Parser.Expression.atg:187*/expr = lhs; 
		while (la.kind == _pow) {
			Get();
			AssignExpr(/*Parser.Expression.atg:188*/out rhs);
			/*Parser.Expression.atg:188*/expr = AstBinaryOperator.Create(this, expr, BinaryOperator.Power, rhs); 
		}
	}

	void AssignExpr(/*Parser.Expression.atg:192*/out AstExpr expr) {
		/*Parser.Expression.atg:192*/AstGetSet assignment; BinaryOperator setModifier = BinaryOperator.None;
		      AstTypeExpr T;
		  
		PostfixUnaryExpr(/*Parser.Expression.atg:196*/out expr);
		if (/*Parser.Expression.atg:198*/isAssignmentOperator()) {
			/*Parser.Expression.atg:198*/assignment = expr as AstGetSet;
			if(assignment == null) 
			{
			    SemErr(string.Format("Cannot assign to a {0}",
			        expr.GetType().Name));
			    assignment = new AstGetSetSymbol(this, PCall.Get, 
			        SymbolEntry.LocalObjectVariable("SEMANTIC_ERROR")); //to prevent null references
			}
			assignment.Call = PCall.Set;
			
			if (StartOf(9)) {
				switch (la.kind) {
				case _assign: {
					Get();
					/*Parser.Expression.atg:209*/setModifier = BinaryOperator.None; 
					break;
				}
				case _plus: {
					Get();
					Expect(_assign);
					/*Parser.Expression.atg:210*/setModifier = BinaryOperator.Addition; 
					break;
				}
				case _minus: {
					Get();
					Expect(_assign);
					/*Parser.Expression.atg:211*/setModifier = BinaryOperator.Subtraction; 
					break;
				}
				case _times: {
					Get();
					Expect(_assign);
					/*Parser.Expression.atg:212*/setModifier = BinaryOperator.Multiply; 
					break;
				}
				case _div: {
					Get();
					Expect(_assign);
					/*Parser.Expression.atg:213*/setModifier = BinaryOperator.Division; 
					break;
				}
				case _bitAnd: {
					Get();
					Expect(_assign);
					/*Parser.Expression.atg:214*/setModifier = BinaryOperator.BitwiseAnd; 
					break;
				}
				case _bitOr: {
					Get();
					Expect(_assign);
					/*Parser.Expression.atg:215*/setModifier = BinaryOperator.BitwiseOr; 
					break;
				}
				case _coalescence: {
					Get();
					Expect(_assign);
					/*Parser.Expression.atg:216*/setModifier = BinaryOperator.Coalescence; 
					break;
				}
				}
				Expr(/*Parser.Expression.atg:217*/out expr);
			} else if (la.kind == _tilde) {
				Get();
				Expect(_assign);
				/*Parser.Expression.atg:219*/setModifier = BinaryOperator.Cast; 
				TypeExpr(/*Parser.Expression.atg:220*/out T);
				/*Parser.Expression.atg:220*/expr = T; 
			} else SynErr(110);
			/*Parser.Expression.atg:222*/assignment.Arguments.Add(expr); 
			if(setModifier == BinaryOperator.None)
			    expr = assignment;
			else
			    expr = AstModifyingAssignment.Create(this,setModifier, assignment);
			
		} else if (StartOf(10)) {
		} else SynErr(111);
	}

	void PostfixUnaryExpr(/*Parser.Expression.atg:232*/out AstExpr expr) {
		/*Parser.Expression.atg:232*/AstTypeExpr type = null; AstGetSet extension; bool isInverted = false; 
		PrefixUnaryExpr(/*Parser.Expression.atg:234*/out expr);
		while (StartOf(11)) {
			if (la.kind == _tilde) {
				Get();
				TypeExpr(/*Parser.Expression.atg:235*/out type);
				/*Parser.Expression.atg:235*/expr = new AstTypecast(this, expr, type); 
			} else if (la.kind == _is) {
				Get();
				if (la.kind == _not) {
					Get();
					/*Parser.Expression.atg:236*/isInverted = true; 
				}
				TypeExpr(/*Parser.Expression.atg:237*/out type);
				/*Parser.Expression.atg:237*/expr = new AstTypecheck(this, expr, type);
				if(isInverted)
				                              {
				                                  ((AstTypecheck)expr).IsInverted = true;
					expr = AstUnaryOperator._Create(this, UnaryOperator.LogicalNot, expr);
				                              }
				
			} else if (la.kind == _inc) {
				Get();
				/*Parser.Expression.atg:244*/expr = AstUnaryOperator._Create(this, UnaryOperator.PostIncrement, expr); 
			} else if (la.kind == _dec) {
				Get();
				/*Parser.Expression.atg:245*/expr = AstUnaryOperator._Create(this, UnaryOperator.PostDecrement, expr); 
			} else {
				GetSetExtension(/*Parser.Expression.atg:246*/expr, out extension);
				/*Parser.Expression.atg:247*/expr = extension; 
			}
		}
	}

	void TypeExpr(/*Parser.Expression.atg:493*/out AstTypeExpr type) {
		/*Parser.Expression.atg:493*/type = null; 
		if (StartOf(12)) {
			PrexoniteTypeExpr(/*Parser.Expression.atg:495*/out type);
		} else if (la.kind == _ns || la.kind == _doublecolon) {
			ClrTypeExpr(/*Parser.Expression.atg:496*/out type);
		} else SynErr(112);
	}

	void PrefixUnaryExpr(/*Parser.Expression.atg:252*/out AstExpr expr) {
		/*Parser.Expression.atg:252*/var prefixes = new Stack<UnaryOperator>(); 
		while (StartOf(13)) {
			if (la.kind == _plus) {
				Get();
			} else if (la.kind == _minus) {
				Get();
				/*Parser.Expression.atg:255*/prefixes.Push(UnaryOperator.UnaryNegation); 
			} else if (la.kind == _inc) {
				Get();
				/*Parser.Expression.atg:256*/prefixes.Push(UnaryOperator.PreIncrement); 
			} else {
				Get();
				/*Parser.Expression.atg:257*/prefixes.Push(UnaryOperator.PreDecrement); 
			}
		}
		Primary(/*Parser.Expression.atg:259*/out expr);
		/*Parser.Expression.atg:260*/while(prefixes.Count > 0)
		   expr = AstUnaryOperator._Create(this, prefixes.Pop(), expr);
		
	}

	void GetSetExtension(/*Parser.Statement.atg:122*/AstExpr subject, out AstGetSet extension) {
		/*Parser.Statement.atg:122*/extension = null; string id;
		if(subject == null)
		{
			SemErr("Member access not preceded by a proper expression.");
			subject = new AstConstant(this,null);
		}
		                             
		if (/*Parser.Statement.atg:132*/isIndirectCall() ) {
			Expect(_dot);
			/*Parser.Statement.atg:132*/extension = new AstIndirectCall(this, PCall.Get, subject); 
			Arguments(/*Parser.Statement.atg:133*/extension.Arguments);
		} else if (la.kind == _dot) {
			Get();
			Id(/*Parser.Statement.atg:135*/out id);
			/*Parser.Statement.atg:135*/extension = new AstGetSetMemberAccess(this, PCall.Get, subject, id); 
			Arguments(/*Parser.Statement.atg:136*/extension.Arguments);
		} else if (la.kind == _lbrack) {
			/*Parser.Statement.atg:138*/AstExpr expr; 
			extension = new AstGetSetMemberAccess(this, PCall.Get, subject, ""); 
			
			Get();
			if (StartOf(14)) {
				Expr(/*Parser.Statement.atg:142*/out expr);
				/*Parser.Statement.atg:142*/extension.Arguments.Add(expr); 
				while (WeakSeparator(_comma,14,15) ) {
					Expr(/*Parser.Statement.atg:143*/out expr);
					/*Parser.Statement.atg:143*/extension.Arguments.Add(expr); 
				}
			}
			Expect(_rbrack);
		} else SynErr(113);
	}

	void Primary(/*Parser.Expression.atg:266*/out AstExpr expr) {
		/*Parser.Expression.atg:266*/expr = null;
		AstGetSet complex = null; bool declared; 
		if (la.kind == _asm) {
			/*Parser.Expression.atg:269*/_pushLexerState(Lexer.Asm); 
			/*Parser.Expression.atg:269*/AstBlock blockExpr = target.BeginBlock("asm"); 
			Get();
			Expect(_lpar);
			while (StartOf(1)) {
				AsmInstruction(/*Parser.Expression.atg:270*/blockExpr);
			}
			Expect(_rpar);
			/*Parser.Expression.atg:271*/_popLexerState(); 
			/*Parser.Expression.atg:271*/expr = blockExpr; 
			target.EndBlock(); 
			
		} else if (StartOf(16)) {
			Constant(/*Parser.Expression.atg:274*/out expr);
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
			GetInitiator(/*Parser.Expression.atg:286*/out complex, out declared);
			/*Parser.Expression.atg:287*/expr = complex; 
		} else if (la.kind == _LPopExpr) {
			Get();
			Expect(_lpar);
			Expr(/*Parser.Expression.atg:288*/out expr);
			/*Parser.Expression.atg:293*/_popLexerState(); _inject(_plus); 
			Expect(_rpar);
		} else SynErr(114);
	}

	void Constant(/*Parser.Expression.atg:298*/out AstExpr expr) {
		/*Parser.Expression.atg:298*/expr = null; int vi; double vr; bool vb; string vs; 
		if (la.kind == _integer) {
			Integer(/*Parser.Expression.atg:300*/out vi);
			/*Parser.Expression.atg:300*/expr = new AstConstant(this, vi); 
		} else if (la.kind == _real || la.kind == _realLike) {
			Real(/*Parser.Expression.atg:301*/out vr);
			/*Parser.Expression.atg:301*/expr = new AstConstant(this, vr); 
		} else if (la.kind == _true || la.kind == _false) {
			Boolean(/*Parser.Expression.atg:302*/out vb);
			/*Parser.Expression.atg:302*/expr = new AstConstant(this, vb); 
		} else if (la.kind == _string) {
			String(/*Parser.Expression.atg:303*/out vs);
			/*Parser.Expression.atg:303*/expr = new AstConstant(this, vs); 
		} else if (la.kind == _null) {
			Null();
			/*Parser.Expression.atg:304*/expr = new AstConstant(this, null); 
		} else SynErr(115);
	}

	void CoroutineCreation(/*Parser.Expression.atg:373*/out AstExpr expr) {
		/*Parser.Expression.atg:374*/AstCreateCoroutine cor = new AstCreateCoroutine(this); 
		AstExpr iexpr;
		expr = cor;
		
		Expect(_coroutine);
		Expr(/*Parser.Expression.atg:379*/out iexpr);
		/*Parser.Expression.atg:379*/cor.Expression = iexpr; 
		if (la.kind == _for) {
			Get();
			Arguments(/*Parser.Expression.atg:380*/cor.Arguments);
		}
	}

	void ListLiteral(/*Parser.Expression.atg:308*/out AstExpr expr) {
		/*Parser.Expression.atg:308*/AstExpr iexpr = null; 
		AstListLiteral lst = new AstListLiteral(this);
		expr = lst;
		bool missingExpr = false;
		
		Expect(_lbrack);
		if (StartOf(14)) {
			Expr(/*Parser.Expression.atg:315*/out iexpr);
			/*Parser.Expression.atg:315*/lst.Elements.Add(iexpr); 
			while (la.kind == _comma) {
				Get();
				/*Parser.Expression.atg:316*/if(missingExpr)
				   SemErr("Missing expression in list literal (two consecutive commas).");
				
				if (StartOf(14)) {
					Expr(/*Parser.Expression.atg:319*/out iexpr);
					/*Parser.Expression.atg:319*/lst.Elements.Add(iexpr); 
					missingExpr = false; 
					
				} else if (la.kind == _comma || la.kind == _rbrack) {
					/*Parser.Expression.atg:322*/missingExpr = true; 
				} else SynErr(116);
			}
		}
		Expect(_rbrack);
	}

	void HashLiteral(/*Parser.Expression.atg:330*/out AstExpr expr) {
		/*Parser.Expression.atg:330*/AstExpr iexpr = null; 
		AstHashLiteral hash = new AstHashLiteral(this);
		expr = hash;
		                                 bool missingExpr = false;
		
		Expect(_lbrace);
		if (StartOf(14)) {
			Expr(/*Parser.Expression.atg:337*/out iexpr);
			/*Parser.Expression.atg:337*/hash.Elements.Add(iexpr); 
			while (la.kind == _comma) {
				Get();
				/*Parser.Expression.atg:338*/if(missingExpr)
				           SemErr("Missing expression in list literal (two consecutive commas).");
				   
				if (StartOf(14)) {
					Expr(/*Parser.Expression.atg:341*/out iexpr);
					/*Parser.Expression.atg:341*/hash.Elements.Add(iexpr); 
					      missingExpr = false;
					  
				} else if (la.kind == _comma || la.kind == _rbrace) {
					/*Parser.Expression.atg:344*/missingExpr = true; 
				} else SynErr(117);
			}
		}
		Expect(_rbrace);
	}

	void LoopExpr(/*Parser.Expression.atg:352*/out AstExpr expr) {
		/*Parser.Expression.atg:352*/AstBlock dummyBlock = target.BeginBlock("loopExpr");
		
		if (la.kind == _do || la.kind == _while || la.kind == _until) {
			WhileLoop(/*Parser.Expression.atg:355*/dummyBlock);
		} else if (la.kind == _for) {
			ForLoop(/*Parser.Expression.atg:356*/dummyBlock);
		} else if (la.kind == _foreach) {
			ForeachLoop(/*Parser.Expression.atg:357*/dummyBlock);
		} else SynErr(118);
		/*Parser.Expression.atg:358*/expr = new AstLoopExpression(this, (AstLoop) dummyBlock.Statements[0]);
		target.EndBlock();
		
	}

	void ThrowExpression(/*Parser.Expression.atg:481*/out AstThrow th) {
		/*Parser.Expression.atg:481*/th = new AstThrow(this); 
		Expect(_throw);
		Expr(/*Parser.Expression.atg:484*/out th.Expression);
	}

	void LambdaExpression(/*Parser.Expression.atg:384*/out AstExpr expr) {
		/*Parser.Expression.atg:384*/expr = null;
		PFunction func = TargetApplication.CreateFunction(generateLocalId());                                             
		func.Meta[Application.ImportKey] = target.Function.Meta[Application.ImportKey];
		func.Meta[PFunction.ParentFunctionKey] = target.Function.Id;
		Loader.CreateFunctionTarget(func, target, GetPosition());
		CompilerTarget ft = FunctionTargets[func];
		
		if (StartOf(19)) {
			FormalArg(/*Parser.Expression.atg:392*/ft);
		} else if (la.kind == _lpar) {
			Get();
			if (StartOf(19)) {
				FormalArg(/*Parser.Expression.atg:394*/ft);
				while (la.kind == _comma) {
					Get();
					FormalArg(/*Parser.Expression.atg:396*/ft);
				}
			}
			Expect(_rpar);
		} else SynErr(119);
		/*Parser.Expression.atg:402*/CompilerTarget oldTarget = target;
		target = ft;
		
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
		} else SynErr(120);
		/*Parser.Expression.atg:413*/target = oldTarget;
		if(errors.count == 0)
		{
		    try {
		        //Emit code for top-level block
		        Ast[func].EmitCode(FunctionTargets[func],true,StackSemantics.Effect);
		        FunctionTargets[func].FinishTarget();
		    } catch(Exception e) {
		        SemErr("Exception during compilation of lambda expression.\n" + e.ToString());
		    }
		}
		
		expr = new AstCreateClosure(this, 
		    new SymbolEntry(SymbolInterpretations.Function, func.Id, 
		    func.ParentApplication.Module.Name));                                         
		
	}

	void LazyExpression(/*Parser.Expression.atg:432*/out AstExpr expr) {
		/*Parser.Expression.atg:432*/expr = null;
		PFunction func = TargetApplication.CreateFunction(generateLocalId());
		func.Meta[Application.ImportKey] = target.Function.Meta[Application.ImportKey];
		func.Meta[PFunction.ParentFunctionKey] = target.Function.Id;
		Loader.CreateFunctionTarget(func, new AstBlock(this), target, GetPosition());
		CompilerTarget ft = FunctionTargets[func];
		
		//Switch to nested target
		CompilerTarget oldTarget = target;
		target = ft;
		
		Expect(_lazy);
		if (la.kind == _lbrace) {
			Get();
			while (StartOf(20)) {
				Statement(/*Parser.Expression.atg:446*/ft.Ast);
			}
			Expect(_rbrace);
		} else if (StartOf(14)) {
			/*Parser.Expression.atg:448*/AstReturn ret = new AstReturn(this, ReturnVariant.Exit); 
			Expr(/*Parser.Expression.atg:449*/out ret.Expression);
			/*Parser.Expression.atg:449*/ft.Ast.Add(ret); 
		} else SynErr(121);
		/*Parser.Expression.atg:453*/var cap = ft.ToCaptureByValue(let_bindings(ft));
		
		//Restore parent target
		target = oldTarget;
		
		//Finish nested function
		if(errors.count == 0)
		{
		    try {
		    Ast[func].EmitCode(FunctionTargets[func],true,StackSemantics.Effect);
		    FunctionTargets[func].FinishTarget();
		    } catch(Exception e) {
		        SemErr("Exception during compilation of lazy expression.\n" + e.ToString());
		    }
		}
		
		//Construct expr (appears in the place of lazy expression)
		var clo = new AstCreateClosure(this,  
		    new SymbolEntry(SymbolInterpretations.Function, func.Id, 
		    func.ParentApplication.Module.Name));
		var thunk = new AstGetSetSymbol(this, 
		    SymbolEntry.Command(Engine.ThunkAlias));
		thunk.Arguments.Add(clo);
		thunk.Arguments.AddRange(cap(this)); //Add captured values
		expr = thunk;
		
	}

	void ObjectCreation(/*Parser.Expression.atg:365*/out AstExpr expr) {
		/*Parser.Expression.atg:365*/AstTypeExpr type; expr = null;
		ArgumentsProxy args; 
		Expect(_new);
		TypeExpr(/*Parser.Expression.atg:368*/out type);
		/*Parser.Expression.atg:368*/_fallbackObjectCreation(this, type, out expr, out args); 
		Arguments(/*Parser.Expression.atg:369*/args);
	}

	void GetInitiator(/*Parser.Statement.atg:150*/out AstGetSet complex, out bool isDeclaration) {
		/*Parser.Statement.atg:150*/complex = null; 
		AstGetSetSymbol symbol = null;
		AstGetSetStatic staticCall = null;
		AstGetSet member = null;
		AstExpr expr;
		List<AstExpr> args = new List<AstExpr>();
		isDeclaration = false;                                            
		string id;
		int placeholderIndex = -1;
		
		if (StartOf(21)) {
			if (/*Parser.Statement.atg:163*/isLikeFunction() || isUnknownId() ) {
				Function(/*Parser.Statement.atg:163*/out complex);
			} else if (StartOf(22)) {
				Variable(/*Parser.Statement.atg:164*/out complex, out isDeclaration);
			} else if (la.kind == _ns || la.kind == _tilde || la.kind == _doublecolon) {
				StaticCall(/*Parser.Statement.atg:165*/out staticCall);
			} else {
				Get();
				Expr(/*Parser.Statement.atg:166*/out expr);
				/*Parser.Statement.atg:166*/args.Add(expr); 
				while (la.kind == _comma) {
					Get();
					Expr(/*Parser.Statement.atg:167*/out expr);
					/*Parser.Statement.atg:167*/args.Add(expr); 
				}
				Expect(_rpar);
				if (la.kind == _dot || la.kind == _lbrack) {
					GetSetExtension(/*Parser.Statement.atg:170*/expr, out member);
					/*Parser.Statement.atg:171*/if(args.Count > 1)
					SemErr("A member access cannot have multiple subjects. (Did you mean '>>'?)");
					
				} else if (la.kind == _appendright) {
					Get();
					GetCall(/*Parser.Statement.atg:175*/out complex);
					/*Parser.Statement.atg:175*/complex.Arguments.RightAppend(args);
					complex.Arguments.ReleaseRightAppend();
					                                 //WARNING: the following code is duplicated in Parser.Expression.atg/RightAppendExpr
					if(complex is AstGetSetSymbol 
					                                         && ((AstGetSetSymbol)complex).IsObjectVariable)
					       complex.Call = PCall.Set;
					member = complex;
					
				} else SynErr(122);
			}
			/*Parser.Statement.atg:185*/complex = 
			(AstGetSet)symbol ?? 
			(AstGetSet)staticCall ?? 
			(AstGetSet)member ??
			complex; 
			
		} else if (/*Parser.Statement.atg:193*/isDeDereference() ) {
			Expect(_pointer);
			Expect(_pointer);
			Id(/*Parser.Statement.atg:193*/out id);
			/*Parser.Statement.atg:193*/SymbolEntry s = target.Symbols[id];
			SymbolInterpretations kind;
			if(s == null)
			{   
			    SemErr("The symbol " + id + " is not defined"); 
			    s = SymbolEntry.LocalObjectVariable(id);
			    kind = s.Interpretation;
			}
			else
			{
			    kind = s.Interpretation;
			    if(s.Interpretation == SymbolInterpretations.LocalReferenceVariable)
			        kind = SymbolInterpretations.LocalObjectVariable;
			    else if(s.Interpretation == SymbolInterpretations.GlobalReferenceVariable)
			        kind = SymbolInterpretations.GlobalObjectVariable;
			    else
			        SemErr("Only reference variables can be dereferenced twice.");
			}
			complex = new AstGetSetReference(this, s.With(interpretation:kind));
			
		} else if (la.kind == _pointer) {
			Get();
			Id(/*Parser.Statement.atg:213*/out id);
			/*Parser.Statement.atg:213*/SymbolEntry s = target.Symbols[id];
			if(s == null)
			{   
			    SemErr("The symbol " + id + " is not defined"); 
			    s = SymbolEntry.LocalObjectVariable(id);
			}
			else if(InterpretationIsLocalVariable(s.Interpretation))
			{
			    if(isOuterVariable(s.InternalId))
			        target.RequireOuterVariable(s.InternalId);
			}
			complex = new AstGetSetReference(this, s);
			
		} else if (la.kind == _question) {
			Get();
			if (la.kind == _integer) {
				Integer(/*Parser.Statement.atg:227*/out placeholderIndex);
			}
			/*Parser.Statement.atg:227*/complex = new AstPlaceholder(this, 0 <= placeholderIndex ? (int?)placeholderIndex : null); 
		} else SynErr(123);
	}

	void Real(/*Parser.Helper.atg:56*/out double value) {
		if (la.kind == _real) {
			Get();
		} else if (la.kind == _realLike) {
			Get();
		} else SynErr(124);
		/*Parser.Helper.atg:57*/string real = t.val;
		if(!TryParseReal(real, out value))
		    SemErr(t, "Cannot recognize real " + real);
		
	}

	void Null() {
		Expect(_null);
	}

	void WhileLoop(/*Parser.Statement.atg:444*/AstBlock block) {
		/*Parser.Statement.atg:444*/AstWhileLoop loop = new AstWhileLoop(GetPosition(),CurrentBlock); 
		if (la.kind == _while || la.kind == _until) {
			if (la.kind == _while) {
				Get();
			} else {
				Get();
				/*Parser.Statement.atg:446*/loop.IsPositive = false; 
			}
			Expect(_lpar);
			Expr(/*Parser.Statement.atg:448*/out loop.Condition);
			Expect(_rpar);
			/*Parser.Statement.atg:449*/target.BeginBlock(loop.Block); //EndBlock is common for both loops
			
			StatementBlock(/*Parser.Statement.atg:451*/loop.Block);
		} else if (la.kind == _do) {
			Get();
			/*Parser.Statement.atg:453*/target.BeginBlock(loop.Block); 
			loop.IsPrecondition = false;
			
			StatementBlock(/*Parser.Statement.atg:456*/loop.Block);
			if (la.kind == _while) {
				Get();
			} else if (la.kind == _until) {
				Get();
				/*Parser.Statement.atg:457*/loop.IsPositive = false; 
			} else SynErr(125);
			Expect(_lpar);
			Expr(/*Parser.Statement.atg:459*/out loop.Condition);
			Expect(_rpar);
		} else SynErr(126);
		/*Parser.Statement.atg:460*/target.EndBlock(); block.Add(loop); 
	}

	void ForLoop(/*Parser.Statement.atg:463*/AstBlock block) {
		/*Parser.Statement.atg:463*/AstForLoop loop;
		
		Expect(_for);
		/*Parser.Statement.atg:466*/loop = new AstForLoop(GetPosition(), CurrentBlock); target.BeginBlock(loop.Block); 
		Expect(_lpar);
		StatementBlock(/*Parser.Statement.atg:467*/loop.Initialize);
		if (la.kind == _do) {
			Get();
			StatementBlock(/*Parser.Statement.atg:469*/loop.NextIteration);
			/*Parser.Statement.atg:470*/loop.IsPrecondition = false; 
			if (la.kind == _while) {
				Get();
			} else if (la.kind == _until) {
				Get();
				/*Parser.Statement.atg:472*/loop.IsPositive = false; 
			} else SynErr(127);
			Expr(/*Parser.Statement.atg:474*/out loop.Condition);
		} else if (StartOf(14)) {
			if (la.kind == _while || la.kind == _until) {
				if (la.kind == _while) {
					Get();
				} else {
					Get();
					/*Parser.Statement.atg:476*/loop.IsPositive = false; 
				}
			}
			Expr(/*Parser.Statement.atg:478*/out loop.Condition);
			Expect(_semicolon);
			SimpleStatement(/*Parser.Statement.atg:480*/loop.NextIteration);
			if (la.kind == _semicolon) {
				Get();
			}
		} else SynErr(128);
		Expect(_rpar);
		StatementBlock(/*Parser.Statement.atg:484*/loop.Block);
		/*Parser.Statement.atg:484*/target.EndBlock(); block.Add(loop); 
	}

	void ForeachLoop(/*Parser.Statement.atg:488*/AstBlock block) {
		Expect(_foreach);
		/*Parser.Statement.atg:489*/AstForeachLoop loop = new AstForeachLoop(this);
		target.BeginBlock(loop.Block);
		
		Expect(_lpar);
		GetCall(/*Parser.Statement.atg:493*/out loop.Element);
		Expect(_in);
		Expr(/*Parser.Statement.atg:495*/out loop.List);
		Expect(_rpar);
		StatementBlock(/*Parser.Statement.atg:497*/loop.Block);
		/*Parser.Statement.atg:498*/target.EndBlock();
		block.Add(loop); 
		
	}

	void Arguments(/*Parser.Statement.atg:693*/ArgumentsProxy args) {
		/*Parser.Statement.atg:694*/AstExpr expr;
		                          bool missingArg = false;
		                      
		if (la.kind == _lpar) {
			Get();
			if (StartOf(14)) {
				Expr(/*Parser.Statement.atg:700*/out expr);
				/*Parser.Statement.atg:700*/args.Add(expr); 
				while (la.kind == _comma) {
					Get();
					/*Parser.Statement.atg:701*/if(missingArg)
					              SemErr("Missing argument expression (two consecutive commas)");
					      
					if (StartOf(14)) {
						Expr(/*Parser.Statement.atg:704*/out expr);
						/*Parser.Statement.atg:705*/args.Add(expr);
						missingArg = false;
						
					} else if (la.kind == _comma || la.kind == _rpar) {
						/*Parser.Statement.atg:708*/missingArg = true; 
					} else SynErr(129);
				}
			}
			Expect(_rpar);
		}
		/*Parser.Statement.atg:714*/args.RememberRightAppendPosition(); 
		if (la.kind == _appendleft) {
			Get();
			if (/*Parser.Statement.atg:719*/la.kind == _lpar && (!isLambdaExpression())) {
				Expect(_lpar);
				if (StartOf(14)) {
					Expr(/*Parser.Statement.atg:720*/out expr);
					/*Parser.Statement.atg:720*/args.Add(expr); 
					while (la.kind == _comma) {
						Get();
						Expr(/*Parser.Statement.atg:722*/out expr);
						/*Parser.Statement.atg:723*/args.Add(expr); 
					}
				}
				Expect(_rpar);
			} else if (StartOf(14)) {
				Expr(/*Parser.Statement.atg:727*/out expr);
				/*Parser.Statement.atg:727*/args.Add(expr); 
			} else SynErr(130);
		}
	}

	void FormalArg(/*Parser.GlobalScope.atg:674*/CompilerTarget ft) {
		/*Parser.GlobalScope.atg:674*/string id; SymbolInterpretations kind = SymbolInterpretations.LocalObjectVariable; 
		if (la.kind == _var || la.kind == _ref) {
			if (la.kind == _var) {
				Get();
			} else {
				Get();
				/*Parser.GlobalScope.atg:676*/kind = SymbolInterpretations.LocalReferenceVariable; 
			}
		}
		Id(/*Parser.GlobalScope.atg:678*/out id);
		/*Parser.GlobalScope.atg:681*/ft.Function.Parameters.Add(id); 
		ft.Symbols.Add(id, new SymbolEntry(kind, id, null));
		
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
		} else SynErr(131);
		while (la.kind == _and) {
			Get();
			Statement(/*Parser.Statement.atg:38*/block);
		}
	}

	void ExplicitTypeExpr(/*Parser.Expression.atg:487*/out AstTypeExpr type) {
		/*Parser.Expression.atg:487*/type = null; 
		if (la.kind == _tilde) {
			Get();
			PrexoniteTypeExpr(/*Parser.Expression.atg:489*/out type);
		} else if (la.kind == _ns || la.kind == _doublecolon) {
			ClrTypeExpr(/*Parser.Expression.atg:490*/out type);
		} else SynErr(132);
	}

	void PrexoniteTypeExpr(/*Parser.Expression.atg:515*/out AstTypeExpr type) {
		/*Parser.Expression.atg:515*/string id = null; type = null; 
		if (StartOf(4)) {
			Id(/*Parser.Expression.atg:517*/out id);
		} else if (la.kind == _null) {
			Get();
			/*Parser.Expression.atg:517*/id = NullPType.Literal; 
		} else SynErr(133);
		/*Parser.Expression.atg:519*/AstDynamicTypeExpression dType = new AstDynamicTypeExpression(this, id); 
		if (la.kind == _lt) {
			Get();
			if (StartOf(26)) {
				TypeExprElement(/*Parser.Expression.atg:521*/dType.Arguments);
				while (la.kind == _comma) {
					Get();
					TypeExprElement(/*Parser.Expression.atg:522*/dType.Arguments);
				}
			}
			Expect(_gt);
		}
		/*Parser.Expression.atg:526*/type = dType; 
	}

	void ClrTypeExpr(/*Parser.Expression.atg:500*/out AstTypeExpr type) {
		/*Parser.Expression.atg:500*/string id; 
		/*Parser.Expression.atg:502*/StringBuilder typeId = new StringBuilder(); 
		if (la.kind == _doublecolon) {
			Get();
		} else if (la.kind == _ns) {
			Get();
			/*Parser.Expression.atg:504*/typeId.Append(t.val); typeId.Append('.'); 
		} else SynErr(134);
		while (la.kind == _ns) {
			Get();
			/*Parser.Expression.atg:506*/typeId.Append(t.val); typeId.Append('.'); 
		}
		Id(/*Parser.Expression.atg:508*/out id);
		/*Parser.Expression.atg:508*/typeId.Append(id);
		type = new AstConstantTypeExpression(this, 
		    "Object(\"" + StringPType.Escape(typeId.ToString()) + "\")");
		
	}

	void TypeExprElement(/*Parser.Expression.atg:530*/List<AstExpr> args ) {
		/*Parser.Expression.atg:530*/AstExpr expr; AstTypeExpr type; 
		if (StartOf(16)) {
			Constant(/*Parser.Expression.atg:532*/out expr);
			/*Parser.Expression.atg:532*/args.Add(expr); 
		} else if (la.kind == _ns || la.kind == _tilde || la.kind == _doublecolon) {
			ExplicitTypeExpr(/*Parser.Expression.atg:533*/out type);
			/*Parser.Expression.atg:533*/args.Add(type); 
		} else if (la.kind == _lpar) {
			Get();
			Expr(/*Parser.Expression.atg:534*/out expr);
			Expect(_rpar);
			/*Parser.Expression.atg:534*/args.Add(expr); 
		} else SynErr(135);
	}

	void Prexonite() {
		/*Parser.GlobalScope.atg:26*/PFunction func; 
		while (StartOf(27)) {
			if (StartOf(28)) {
				if (StartOf(29)) {
					if (la.kind == _var || la.kind == _ref) {
						GlobalVariableDefinition();
					} else if (la.kind == _declare) {
						Declaration();
					} else {
						MetaAssignment(/*Parser.GlobalScope.atg:30*/TargetApplication);
					}
				}
				while (!(la.kind == _EOF || la.kind == _semicolon)) {SynErr(136); Get();}
				Expect(_semicolon);
			} else if (la.kind == _lbrace) {
				GlobalCode();
			} else if (la.kind == _build) {
				BuildBlock();
			} else {
				FunctionDefinition(/*Parser.GlobalScope.atg:34*/out func);
			}
		}
		Expect(_EOF);
	}

	void GlobalVariableDefinition() {
		/*Parser.GlobalScope.atg:106*/string id = null; 
		List<string> aliases = new List<string>();
		string primaryAlias = null;
		VariableDeclaration vari; 
		SymbolInterpretations type = SymbolInterpretations.GlobalObjectVariable; 
		SymbolEntry entry;
		
		if (la.kind == _var) {
			Get();
		} else if (la.kind == _ref) {
			Get();
			/*Parser.GlobalScope.atg:116*/type = SymbolInterpretations.GlobalReferenceVariable; 
		} else SynErr(137);
		if (la.kind == _id || la.kind == _anyId) {
			GlobalId(/*Parser.GlobalScope.atg:119*/out id);
			/*Parser.GlobalScope.atg:119*/primaryAlias = id; 
			if (la.kind == _as) {
				GlobalVariableAliasList(/*Parser.GlobalScope.atg:120*/aliases);
			}
		} else if (la.kind == _as) {
			GlobalVariableAliasList(/*Parser.GlobalScope.atg:121*/aliases);
			/*Parser.GlobalScope.atg:122*/id = Engine.GenerateName("v"); 
		} else SynErr(138);
		/*Parser.GlobalScope.atg:125*/entry = new SymbolEntry(type,id, TargetModule.Name);
		foreach(var alias in aliases)
		    Symbols[alias] = entry;
		   DefineGlobalVariable(id,out vari);
		
		if (la.kind == _lbrack) {
			Get();
			if (StartOf(30)) {
				MetaAssignment(/*Parser.GlobalScope.atg:131*/vari);
				while (la.kind == _semicolon) {
					Get();
					if (StartOf(30)) {
						MetaAssignment(/*Parser.GlobalScope.atg:133*/vari);
					}
				}
			}
			Expect(_rbrack);
		}
		/*Parser.GlobalScope.atg:137*/if(primaryAlias != null && !_suppressPrimarySymbol(vari))
		      Symbols[primaryAlias] = entry;
		
		if (la.kind == _assign) {
			/*Parser.GlobalScope.atg:140*/_pushLexerState(Lexer.Local); 
			Get();
			/*Parser.GlobalScope.atg:141*/CompilerTarget lastTarget = target;
			  target=FunctionTargets[Application.InitializationId];
			  AstExpr expr;
			
			Expr(/*Parser.GlobalScope.atg:145*/out expr);
			/*Parser.GlobalScope.atg:146*/_popLexerState();
			if(errors.count == 0)
			{
				AstGetSet complex = new AstGetSetSymbol(this, PCall.Set, 
			                                new SymbolEntry(InterpretAsObjectVariable(type), id, TargetModule.Name));
				complex.Arguments.Add(expr);
				target.Ast.Add(complex);
			                            TargetApplication._RequireInitialization();
				Loader._EmitPartialInitializationCode();
			                  }
			                  target = lastTarget;
			              
		}
	}

	void Declaration() {
		/*Parser.GlobalScope.atg:174*/SymbolInterpretations type = SymbolInterpretations.Undefined;
		ModuleName module = TargetModule.Name;
		
		while (!(la.kind == _EOF || la.kind == _declare)) {SynErr(139); Get();}
		Expect(_declare);
		if (StartOf(31)) {
			if (StartOf(32)) {
				if (la.kind == _var) {
					Get();
					/*Parser.GlobalScope.atg:180*/type = SymbolInterpretations.GlobalObjectVariable; 
				} else if (la.kind == _ref) {
					Get();
					/*Parser.GlobalScope.atg:181*/type = SymbolInterpretations.GlobalReferenceVariable; 
				} else if (la.kind == _function) {
					Get();
					/*Parser.GlobalScope.atg:182*/type = SymbolInterpretations.Function; 
				} else {
					Get();
					/*Parser.GlobalScope.atg:183*/type = SymbolInterpretations.Command; 
				}
			}
			DeclarationInstance(/*Parser.GlobalScope.atg:185*/type,module);
			while (la.kind == _comma) {
				Get();
				if (StartOf(4)) {
					DeclarationInstance(/*Parser.GlobalScope.atg:186*/type,module);
				}
			}
		} else if (la.kind == _lbrace) {
			Get();
			if (la.kind == _uusing) {
				Get();
				ModuleName(/*Parser.GlobalScope.atg:188*/ref module);
			}
			while (StartOf(32)) {
				if (la.kind == _var) {
					Get();
					/*Parser.GlobalScope.atg:190*/type = SymbolInterpretations.GlobalObjectVariable; 
				} else if (la.kind == _ref) {
					Get();
					/*Parser.GlobalScope.atg:191*/type = SymbolInterpretations.GlobalReferenceVariable; 
				} else if (la.kind == _function) {
					Get();
					/*Parser.GlobalScope.atg:192*/type = SymbolInterpretations.Function; 
				} else {
					Get();
					/*Parser.GlobalScope.atg:193*/type = SymbolInterpretations.Command; 
				}
				Expect(_colon);
				DeclarationInstance(/*Parser.GlobalScope.atg:196*/type,module);
				while (la.kind == _comma) {
					Get();
					if (StartOf(4)) {
						DeclarationInstance(/*Parser.GlobalScope.atg:197*/type,module);
					}
				}
			}
			Expect(_rbrace);
		} else SynErr(140);
	}

	void MetaAssignment(/*Parser.GlobalScope.atg:41*/IHasMetaTable target) {
		/*Parser.GlobalScope.atg:41*/string key = null; MetaEntry entry = null; 
		if (la.kind == _is) {
			Get();
			/*Parser.GlobalScope.atg:43*/entry = true; 
			if (la.kind == _not) {
				Get();
				/*Parser.GlobalScope.atg:44*/entry = false; 
			}
			GlobalId(/*Parser.GlobalScope.atg:46*/out key);
		} else if (la.kind == _not) {
			Get();
			/*Parser.GlobalScope.atg:47*/entry = false; 
			GlobalId(/*Parser.GlobalScope.atg:48*/out key);
		} else if (la.kind == _id || la.kind == _anyId) {
			GlobalId(/*Parser.GlobalScope.atg:50*/out key);
			if (la.kind == _enabled) {
				Get();
				/*Parser.GlobalScope.atg:51*/entry = true; 
			} else if (la.kind == _disabled) {
				Get();
				/*Parser.GlobalScope.atg:52*/entry = false; 
			} else if (StartOf(33)) {
				MetaExpr(/*Parser.GlobalScope.atg:53*/out entry);
			} else if (la.kind == _rbrack || la.kind == _semicolon) {
				/*Parser.GlobalScope.atg:54*/entry = true; 
			} else SynErr(141);
		} else if (la.kind == _add) {
			Get();
			/*Parser.GlobalScope.atg:56*/MetaEntry subEntry; 
			MetaExpr(/*Parser.GlobalScope.atg:57*/out subEntry);
			/*Parser.GlobalScope.atg:57*/if(!subEntry.IsList) subEntry = (MetaEntry) subEntry.List; 
			Expect(_to);
			GlobalId(/*Parser.GlobalScope.atg:59*/out key);
			/*Parser.GlobalScope.atg:59*/if(target.Meta.ContainsKey(key))
			{
			    entry = target.Meta[key];
			    entry = entry.AddToList(subEntry.List);
			}
			else
			{
			   entry = subEntry;
			}
			
		} else SynErr(142);
		/*Parser.GlobalScope.atg:69*/if(entry == null || key == null) 
		                        SemErr("Meta assignment did not generate an entry.");
		                   else 
		                        target.Meta[key] = entry; 
		                
	}

	void GlobalCode() {
		/*Parser.GlobalScope.atg:297*/PFunction func = TargetApplication._InitializationFunction;
		CompilerTarget ft = FunctionTargets[func];
		if(ft == null)
		    throw new PrexoniteException("Internal compilation error: InitializeFunction got lost.");
		
		/*Parser.GlobalScope.atg:304*/target = ft; 
		                             _pushLexerState(Lexer.Local);
		                         
		Expect(_lbrace);
		while (StartOf(20)) {
			Statement(/*Parser.GlobalScope.atg:308*/target.Ast);
		}
		Expect(_rbrace);
		/*Parser.GlobalScope.atg:311*/try {
		if(errors.count == 0)
		{
		 TargetApplication._RequireInitialization();
		 Loader._EmitPartialInitializationCode();
		}
		                          } catch(Exception e) {
		                              SemErr("Exception during compilation of initialization code.\n" + e.ToString());
		                          } finally {
		//Symbols defined in this block are not available to further global code blocks
		target.Symbols.Clear();
		target = null;
		_popLexerState();
		                          }
		
	}

	void BuildBlock() {
		while (!(la.kind == _EOF || la.kind == _build)) {SynErr(143); Get();}
		Expect(_build);
		/*Parser.GlobalScope.atg:275*/PFunction func = TargetApplication.CreateFunction();
		  CompilerTarget lastTarget = target; 
		  CompilerTarget buildBlockTarget = 
		Loader.CreateFunctionTarget(func, new AstBlock(this), sourcePosition: GetPosition());
		                          target = buildBlockTarget;
		                          Loader.DeclareBuildBlockCommands(target);
		                          _pushLexerState(Lexer.Local);                                
		                      
		if (la.kind == _does) {
			Get();
		}
		StatementBlock(/*Parser.GlobalScope.atg:285*/target.Ast);
		/*Parser.GlobalScope.atg:288*/_popLexerState();                                    
		  target = lastTarget;
		  _compileAndExecuteBuildBlock(buildBlockTarget);
		
	}

	void FunctionDefinition(/*Parser.GlobalScope.atg:341*/out PFunction func) {
		/*Parser.GlobalScope.atg:342*/func = null; 
		string primaryAlias = null;
		List<string> funcAliases = new List<string>();
		string id = null; //The logical id (given in the source code)
		string funcId; //The "physical" function id
		bool isNested = target != null; 
		bool isCoroutine = false;
		bool isMacro = false;
		bool isLazy = false;
		PFunction derBody = null; //The derived (coroutine/lazy) body function (carries a different name)
		PFunction derStub = null; //The derived (coroutine/lazy) stub function (carries the name(s) specified)
		string derId = null; //The name of the derived stub
		CompilerTarget ct = null;   //The compiler target for the function (as mentioned in the source code)
		CompilerTarget cst = null;  //The compiler target for a stub (coroutine/lazy)
		SymbolEntry symEntry = null;
		
		                                        bool missingArg = false; //Allow trailing comma, but not (,,) in formal arg list
		                                    
		if (la.kind == _lazy) {
			Get();
			if (la.kind == _function) {
				Get();
			}
			/*Parser.GlobalScope.atg:361*/isLazy = true; 
		} else if (la.kind == _function) {
			Get();
		} else if (la.kind == _coroutine) {
			Get();
			/*Parser.GlobalScope.atg:363*/isCoroutine = true; 
		} else if (la.kind == _macro) {
			Get();
			if (la.kind == _function) {
				Get();
			}
			/*Parser.GlobalScope.atg:364*/isMacro = true; 
		} else SynErr(144);
		if (StartOf(4)) {
			Id(/*Parser.GlobalScope.atg:366*/out id);
			/*Parser.GlobalScope.atg:366*/primaryAlias = id; 
			if (la.kind == _as) {
				FunctionAliasList(/*Parser.GlobalScope.atg:367*/funcAliases);
			}
		} else if (la.kind == _as) {
			FunctionAliasList(/*Parser.GlobalScope.atg:368*/funcAliases);
		} else SynErr(145);
		/*Parser.GlobalScope.atg:370*/funcId = id ?? Engine.GenerateName("f");
		  if(Engine.StringsAreEqual(id, @"\init")) //Treat "\init" specially (that's the initialization code)
		  {
		      func = TargetApplication._InitializationFunction;
		      if(isNested)
		          SemErr("Cannot define initialization code inside another function.");
		      if(isCoroutine)
		          SemErr("Cannot define initialization code as a coroutine.");
		      if(isLazy)
		          SemErr("Cannot define initialization code as a lazy function.");
		      if(isMacro)
		          SemErr("Cannot define initialization code as a macro function.");
		  }
		  else
		  {
		      var localId = id;
		      
		      if(isNested)
		      {
		          if(isMacro)
		              SemErr("Inner macros are illegal. Macros must be top-level.");
		              
		          funcId = generateLocalId(id ?? "inner");
		          
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
		                                            
		                                            Loader.CreateFunctionTarget(func, new AstBlock(this), target, GetPosition());
		                                        }
		                                        CompilerTarget ft = FunctionTargets[func];
		                                        
		                                        //Generate derived stub
		                                        if(isCoroutine || isLazy)
		                                        {
		                                            derStub = func;
		                                            
		                                            //Create derived body function
		                                            derId = ft.GenerateLocalId();
		                                            derBody = TargetApplication.CreateFunction(derId);
		                                            Loader.CreateFunctionTarget(derBody, new AstBlock(this), ft, GetPosition());
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
		                                            ft.ParentTarget = target;	                                           
		                                            if(isLazy)
		                                                ft = ct;
		                                        }	                                    
			                                
		if (StartOf(34)) {
			if (la.kind == _lpar) {
				Get();
				if (StartOf(19)) {
					FormalArg(/*Parser.GlobalScope.atg:455*/ft);
					while (la.kind == _comma) {
						Get();
						/*Parser.GlobalScope.atg:456*/if(missingArg)
						       {
						           SemErr("Missing formal argument (two consecutive commas).");
						       } 
						   
						if (StartOf(19)) {
							FormalArg(/*Parser.GlobalScope.atg:461*/ft);
							/*Parser.GlobalScope.atg:461*/missingArg = false; 
						} else if (la.kind == _comma || la.kind == _rpar) {
							/*Parser.GlobalScope.atg:462*/missingArg = true; 
						} else SynErr(146);
					}
				}
				Expect(_rpar);
			} else {
				FormalArg(/*Parser.GlobalScope.atg:467*/ft);
				while (StartOf(35)) {
					if (la.kind == _comma) {
						Get();
					}
					FormalArg(/*Parser.GlobalScope.atg:469*/ft);
				}
			}
		}
		/*Parser.GlobalScope.atg:472*/if(isNested && isLazy)
		   ft = cst;
		  
		  if(target == null && 
		      (!object.ReferenceEquals(func, TargetApplication._InitializationFunction)) &&
		      (!isNested))
		  {
		          //Add the name to the symboltable
		             symEntry = 
		                 new SymbolEntry(SymbolInterpretations.Function, func.Id, TargetModule.Name);
		          foreach(var alias in funcAliases)	                                                
		              Symbols[alias] = symEntry;
		          
		          //Store the original (logical id, mentioned in the source code)
		          if((!string.IsNullOrEmpty(id)))
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
			/*Parser.GlobalScope.atg:498*/_pushLexerState(Lexer.YYINITIAL); 
			Get();
			if (StartOf(30)) {
				MetaAssignment(/*Parser.GlobalScope.atg:500*/func);
				while (la.kind == _semicolon) {
					Get();
					if (StartOf(30)) {
						MetaAssignment(/*Parser.GlobalScope.atg:502*/func);
					}
				}
			}
			/*Parser.GlobalScope.atg:505*/_popLexerState(); 
			Expect(_rbrack);
		}
		/*Parser.GlobalScope.atg:510*/if(primaryAlias != null && !_suppressPrimarySymbol(func))
		   Symbols[primaryAlias] = symEntry;
		
		                                        //Imprint certain meta keys from parent function
		                                        if(isNested)
		                                        {
		                                            func.Meta[Application.ImportKey] = target.Function.Meta[Application.ImportKey];
		                                        }
		
		                                        //Copy stub parameters to body of lazy function
		                                        if(isLazy && !isNested)
			                                    {
			                                        foreach(var kvp in ft.LocalSymbols)
			                                        {
			                                            var paramId = kvp.Key;
			                                            var s = kvp.Value;
			                                            //Lazy functions cannot have ref parameters
			                                            if(s.Interpretation != SymbolInterpretations.LocalObjectVariable)
			                                                SemErr("Lazy functions can only have value parameters (ref is not allowed)");
			                                            ct.Function.Parameters.Add(s.InternalId);
			                                            ct.Symbols.Add(paramId, s);
			                                        }
			                                    }
		                                    
		                                        CompilerTarget lastTarget = target;
		                                        target = FunctionTargets[func]; 
		                                        _pushLexerState(Lexer.Local);
		                                        if(isMacro)
		                                            target.SetupAsMacro();
		                                    
		if (StartOf(36)) {
			if (la.kind == _does) {
				Get();
			}
			StatementBlock(/*Parser.GlobalScope.atg:541*/target.Ast);
		} else if (/*Parser.GlobalScope.atg:543*/isFollowedByStatementBlock()) {
			Expect(_implementation);
			StatementBlock(/*Parser.GlobalScope.atg:544*/target.Ast);
		} else if (la.kind == _assign || la.kind == _implementation) {
			if (la.kind == _assign) {
				Get();
			} else {
				Get();
			}
			/*Parser.GlobalScope.atg:545*/AstReturn ret = new AstReturn(this, ReturnVariant.Exit); 
			Expr(/*Parser.GlobalScope.atg:546*/out ret.Expression);
			/*Parser.GlobalScope.atg:546*/target.Ast.Add(ret); 
			Expect(_semicolon);
		} else SynErr(147);
		/*Parser.GlobalScope.atg:548*/_popLexerState();
		target = lastTarget; 
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
			    crcor.Expression = new AstCreateClosure(this, 
		                                             new SymbolEntry(SymbolInterpretations.Function, derBody.Id, 
		                                             derBody.ParentApplication.Module.Name));
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
		           retVal = new AstCreateClosure(this,  new SymbolEntry(SymbolInterpretations.Function, ct.Function.Id, 
		                                     ct.Function.ParentApplication.Module.Name));
		           
		           //Inject asthunk-conversion code into body
		           var inject = derStub.Parameters.Select(par => 
		           {
		               var getParam =
		                   new AstGetSetSymbol(this, PCall.Get, 
		                                                     SymbolEntry.LocalObjectVariable(par));
		               var asThunkCall = 
		                new AstGetSetSymbol(this, PCall.Get, 
		                                                     SymbolEntry.Command(Engine.AsThunkAlias));
		            asThunkCall.Arguments.Add(getParam);
		            var setParam =
		                new AstGetSetSymbol(this, PCall.Set, 
		                                                     SymbolEntry.LocalObjectVariable(par));
		            setParam.Arguments.Add(asThunkCall);
		            return (AstNode) setParam;
		           });
		           ct.Ast.InsertRange(0,inject);
		       }
		       else
		       {										            
		           //Global lazy functions don't technically need a stub. Might be removed later on
		           var call = new AstGetSetSymbol(this, 
		                                             new SymbolEntry(SymbolInterpretations.Function, ct.Function.Id, TargetModule.Name));
		           
		           //Generate code for arguments (each wrapped in a `asThunk` command call)
		        foreach(var par in derStub.Parameters)
		        {
		            var getParam = 
		                new AstGetSetSymbol(this, PCall.Get, 
		                                                     SymbolEntry.LocalObjectVariable(par));
		            var asThunkCall = 
		                new AstGetSetSymbol(this, PCall.Get, 
		                                                     SymbolEntry.Command(Engine.AsThunkAlias));
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

	void GlobalId(/*Parser.GlobalScope.atg:686*/out string id) {
		/*Parser.GlobalScope.atg:686*/id = "...no freaking id..."; 
		if (la.kind == _id) {
			Get();
			/*Parser.GlobalScope.atg:688*/id = cache(t.val); 
		} else if (la.kind == _anyId) {
			Get();
			String(/*Parser.GlobalScope.atg:689*/out id);
			/*Parser.GlobalScope.atg:689*/id = cache(id); 
		} else SynErr(148);
	}

	void MetaExpr(/*Parser.GlobalScope.atg:77*/out MetaEntry entry) {
		/*Parser.GlobalScope.atg:77*/bool sw; int i; double r; entry = null; string str; 
		switch (la.kind) {
		case _true: case _false: {
			Boolean(/*Parser.GlobalScope.atg:79*/out sw);
			/*Parser.GlobalScope.atg:79*/entry = sw; 
			break;
		}
		case _integer: {
			Integer(/*Parser.GlobalScope.atg:80*/out i);
			/*Parser.GlobalScope.atg:80*/entry = i.ToString(); 
			break;
		}
		case _real: case _realLike: {
			Real(/*Parser.GlobalScope.atg:81*/out r);
			/*Parser.GlobalScope.atg:81*/entry = r.ToString(); 
			break;
		}
		case _string: {
			String(/*Parser.GlobalScope.atg:82*/out str);
			/*Parser.GlobalScope.atg:82*/entry = str; 
			break;
		}
		case _id: case _anyId: case _ns: {
			GlobalQualifiedId(/*Parser.GlobalScope.atg:83*/out str);
			/*Parser.GlobalScope.atg:83*/entry = str; 
			break;
		}
		case _lbrace: {
			Get();
			/*Parser.GlobalScope.atg:84*/List<MetaEntry> lst = new List<MetaEntry>(); 
			MetaEntry subEntry; 
			bool lastWasEmpty = false;
			
			if (StartOf(33)) {
				MetaExpr(/*Parser.GlobalScope.atg:88*/out subEntry);
				/*Parser.GlobalScope.atg:88*/lst.Add(subEntry); 
				while (la.kind == _comma) {
					Get();
					/*Parser.GlobalScope.atg:89*/if(lastWasEmpty)
					    SemErr("Missing meta expression in list (two consecutive commas).");
					
					if (StartOf(33)) {
						MetaExpr(/*Parser.GlobalScope.atg:92*/out subEntry);
						/*Parser.GlobalScope.atg:93*/lst.Add(subEntry); 
						lastWasEmpty = false;
						
					} else if (la.kind == _comma || la.kind == _rbrace) {
						/*Parser.GlobalScope.atg:96*/lastWasEmpty = true; 
					} else SynErr(149);
				}
			}
			Expect(_rbrace);
			/*Parser.GlobalScope.atg:100*/entry = (MetaEntry) lst.ToArray(); 
			break;
		}
		default: SynErr(150); break;
		}
	}

	void GlobalQualifiedId(/*Parser.GlobalScope.atg:692*/out string id) {
		/*Parser.GlobalScope.atg:692*/id = "\\NoId\\"; 
		if (la.kind == _id || la.kind == _anyId) {
			GlobalId(/*Parser.GlobalScope.atg:694*/out id);
		} else if (la.kind == _ns) {
			Get();
			/*Parser.GlobalScope.atg:695*/StringBuilder buffer = new StringBuilder(t.val); buffer.Append('.'); 
			while (la.kind == _ns) {
				Get();
				/*Parser.GlobalScope.atg:696*/buffer.Append(t.val); buffer.Append('.'); 
			}
			GlobalId(/*Parser.GlobalScope.atg:698*/out id);
			/*Parser.GlobalScope.atg:698*/buffer.Append(id); 
			/*Parser.GlobalScope.atg:699*/id = cache(buffer.ToString()); 
		} else SynErr(151);
	}

	void GlobalVariableAliasList(/*Parser.GlobalScope.atg:162*/IList<string> aliases ) {
		/*Parser.GlobalScope.atg:162*/string id = "\\NoId_In_GlobalVariableAliasList_\\"; 
		Expect(_as);
		GlobalId(/*Parser.GlobalScope.atg:164*/out id);
		/*Parser.GlobalScope.atg:164*/aliases.Add(id); 
		while (la.kind == _comma) {
			Get();
			if (la.kind == _id || la.kind == _anyId) {
				GlobalId(/*Parser.GlobalScope.atg:166*/out id);
				/*Parser.GlobalScope.atg:166*/aliases.Add(id); 
			}
		}
	}

	void DeclarationInstance(/*Parser.GlobalScope.atg:204*/SymbolInterpretations type, ModuleName module, bool preventOverride = false) {
		/*Parser.GlobalScope.atg:204*/string id; string aId; 
		Id(/*Parser.GlobalScope.atg:206*/out id);
		/*Parser.GlobalScope.atg:206*/aId = id; 
		if (la.kind == _div) {
			Get();
			ModuleName(/*Parser.GlobalScope.atg:207*/ref module);
			/*Parser.GlobalScope.atg:208*/if(preventOverride) 
			   SemErr("Specification of module name illegal at this point."); 
			
		}
		if (la.kind == _as) {
			Get();
			Id(/*Parser.GlobalScope.atg:212*/out aId);
		}
		/*Parser.GlobalScope.atg:213*/SymbolEntry inferredType;
		if(target == null) //global symbol
		{
		    if(type == SymbolInterpretations.Undefined)
		        if(Symbols.TryGetValue(id, out inferredType))
		           {
		            type = inferredType.Interpretation;
		               module = inferredType.Module;
		           }
		        else if(Symbols.TryGetValue(aId, out inferredType))
		           {
		            type = inferredType.Interpretation;
		               module = inferredType.Module;
		           }
		        else
		           {
		            SemErr("Interpretation of symbol " + id + " as " + aId + " cannot be inferred.");
		           }
		    Symbols[aId] = new SymbolEntry(type, id, module);
		}
		else
		{
		    if(type == SymbolInterpretations.Undefined)
		        if(target.Symbols.TryGetValue(id, out inferredType))
		           {
		            type = inferredType.Interpretation;
		               module = inferredType.Module;
		           }
		        else if(target.Symbols.TryGetValue(aId, out inferredType))
		           {
		            type = inferredType.Interpretation;
		               module = inferredType.Module;
		           }
		        else 
		           {
		            SemErr("Interpretation of symbol " + id + " as " + aId + " cannot be inferred.");                            }
		    target.Symbols[aId] = new SymbolEntry(type, id, module);
		}
		
		                                if(_requiresModule(type) && module == null)
		                                    SemErr("Module cannot be inferred for declaration " + id + " as + " + aId + ".");
			                        
	}

	void ModuleName(/*Parser.GlobalScope.atg:258*/ref ModuleName moduleName) {
		/*Parser.GlobalScope.atg:258*/_pushLexerState(Lexer.YYINITIAL); //need global scope for Version
		string id = null; 
		Version version = default(Version);
		
		Id(/*Parser.GlobalScope.atg:263*/out id);
		if (la.kind == _div) {
			Get();
			Version(/*Parser.GlobalScope.atg:265*/out version);
		}
		/*Parser.GlobalScope.atg:266*/_popLexerState();
		moduleName = Loader.Cache[new ModuleName(id,version)];
		
	}

	void Version(/*Parser.Helper.atg:64*/out Version version) {
		if (la.kind == _realLike) {
			Get();
		} else if (la.kind == _version) {
			Get();
		} else SynErr(152);
		/*Parser.Helper.atg:65*/var raw = t.val;
		if(!TryParseVersion(raw, out version))
		    SemErr(t,"Cannot recognize \"" + raw + "\" as a version literal.");
		
	}

	void StatementBlock(/*Parser.Statement.atg:26*/AstBlock block) {
		Statement(/*Parser.Statement.atg:27*/block);
	}

	void FunctionAliasList(/*Parser.GlobalScope.atg:331*/IList<string> aliases ) {
		/*Parser.GlobalScope.atg:331*/String id; 
		Expect(_as);
		Id(/*Parser.GlobalScope.atg:333*/out id);
		/*Parser.GlobalScope.atg:333*/aliases.Add(id); 
		while (la.kind == _comma) {
			Get();
			if (StartOf(4)) {
				Id(/*Parser.GlobalScope.atg:335*/out id);
				/*Parser.GlobalScope.atg:335*/aliases.Add(id); 
			}
		}
	}

	void ExplicitLabel(/*Parser.Statement.atg:379*/AstBlock block) {
		/*Parser.Statement.atg:379*/string id = "--\\NotAnId\\--"; 
		if (StartOf(4)) {
			Id(/*Parser.Statement.atg:381*/out id);
			Expect(_colon);
		} else if (la.kind == _lid) {
			Get();
			/*Parser.Statement.atg:382*/id = cache(t.val.Substring(0,t.val.Length-1)); 
		} else SynErr(153);
		/*Parser.Statement.atg:383*/block.Statements.Add(new AstExplicitLabel(this, id)); 
	}

	void SimpleStatement(/*Parser.Statement.atg:43*/AstBlock block) {
		if (la.kind == _goto) {
			ExplicitGoTo(/*Parser.Statement.atg:44*/block);
		} else if (la.kind == _declare) {
			Declaration();
		} else if (/*Parser.Statement.atg:47*/isVariableDeclaration() ) {
			VariableDeclarationStatement(/*Parser.Statement.atg:47*/block);
		} else if (StartOf(18)) {
			GetSetComplex(/*Parser.Statement.atg:48*/block);
		} else if (StartOf(37)) {
			Return(/*Parser.Statement.atg:49*/block);
		} else if (la.kind == _throw) {
			Throw(/*Parser.Statement.atg:50*/block);
		} else if (la.kind == _let) {
			LetBindingStmt(/*Parser.Statement.atg:51*/block);
		} else SynErr(154);
	}

	void StructureStatement(/*Parser.Statement.atg:55*/AstBlock block) {
		switch (la.kind) {
		case _asm: {
			/*Parser.Statement.atg:56*/_pushLexerState(Lexer.Asm); 
			Get();
			AsmStatementBlock(/*Parser.Statement.atg:57*/block);
			/*Parser.Statement.atg:58*/_popLexerState(); 
			break;
		}
		case _if: case _unless: {
			Condition(/*Parser.Statement.atg:59*/block);
			break;
		}
		case _do: case _while: case _until: {
			WhileLoop(/*Parser.Statement.atg:60*/block);
			break;
		}
		case _for: {
			ForLoop(/*Parser.Statement.atg:61*/block);
			break;
		}
		case _foreach: {
			ForeachLoop(/*Parser.Statement.atg:62*/block);
			break;
		}
		case _function: case _coroutine: case _macro: case _lazy: {
			NestedFunction(/*Parser.Statement.atg:63*/block);
			break;
		}
		case _try: {
			TryCatchFinally(/*Parser.Statement.atg:64*/block);
			break;
		}
		case _uusing: {
			Using(/*Parser.Statement.atg:65*/block);
			break;
		}
		case _lbrace: {
			Get();
			while (StartOf(20)) {
				Statement(/*Parser.Statement.atg:68*/block);
			}
			Expect(_rbrace);
			break;
		}
		default: SynErr(155); break;
		}
	}

	void ExplicitGoTo(/*Parser.Statement.atg:386*/AstBlock block) {
		/*Parser.Statement.atg:386*/string id; 
		Expect(_goto);
		Id(/*Parser.Statement.atg:389*/out id);
		/*Parser.Statement.atg:389*/block.Statements.Add(new AstExplicitGoTo(this, id)); 
	}

	void VariableDeclarationStatement(/*Parser.Statement.atg:316*/AstBlock block) {
		/*Parser.Statement.atg:316*/AstGetSet variable;
		bool isNewDecl = false;
		
		if (la.kind == _new) {
			Get();
			/*Parser.Statement.atg:319*/isNewDecl = true; 
		}
		VariableDeclaration(/*Parser.Statement.atg:321*/out variable, isNewDecl);
		/*Parser.Statement.atg:322*/if(isNewDecl)
		{
		    block.Add(variable);
		}
		else
		{
		    //No additional action is required. This is just a platform
		    //  for variable declarations without assignment.
		}
		
	}

	void GetSetComplex(/*Parser.Statement.atg:74*/AstBlock block) {
		/*Parser.Statement.atg:74*/AstGetSet complex = null; 
		AstGetSetSymbol symbol = null;
		bool isDeclaration = false;
		AstNode node = null;
		
		GetInitiator(/*Parser.Statement.atg:81*/out complex, out isDeclaration);
		while (la.kind == _dot || la.kind == _lbrack) {
			GetSetExtension(/*Parser.Statement.atg:84*/complex, out complex);
		}
		if (la.kind == _rpar || la.kind == _semicolon) {
			/*Parser.Statement.atg:87*/block.Add(complex); 
		} else if (la.kind == _inc) {
			Get();
			/*Parser.Statement.atg:88*/block.Add(AstUnaryOperator._Create(this, UnaryOperator.PostIncrement, complex)); 
		} else if (la.kind == _dec) {
			Get();
			/*Parser.Statement.atg:89*/block.Add(AstUnaryOperator._Create(this, UnaryOperator.PostDecrement, complex)); 
		} else if (StartOf(38)) {
			Assignment(/*Parser.Statement.atg:90*/complex, out node);
			/*Parser.Statement.atg:90*/symbol = node as AstGetSetSymbol;
			if(symbol != null 
			    && InterpretationIsLocalVariable(symbol.Implementation.Interpretation) 
			    && isDeclaration)
			{
			    symbol.Implementation = symbol.Implementation.With(
			        interpretation:InterpretAsObjectVariable(symbol.Implementation.Interpretation));
			}
			block.Add(node);
			
		} else if (la.kind == _appendright) {
			AppendRightTermination(/*Parser.Statement.atg:100*/ref complex);
			while (la.kind == _appendright) {
				AppendRightTermination(/*Parser.Statement.atg:101*/ref complex);
			}
			/*Parser.Statement.atg:103*/block.Add(complex);  
		} else SynErr(156);
	}

	void Return(/*Parser.Statement.atg:526*/AstBlock block) {
		/*Parser.Statement.atg:526*/AstReturn ret = null; 
		AstExplicitGoTo jump = null; 
		AstExpr expr = null; 
		AstLoopBlock bl = target.CurrentLoopBlock;
		
		if (la.kind == _return || la.kind == _yield) {
			if (la.kind == _return) {
				Get();
				/*Parser.Statement.atg:534*/ret = new AstReturn(this, ReturnVariant.Exit); 
			} else {
				Get();
				/*Parser.Statement.atg:535*/ret = new AstReturn(this, ReturnVariant.Continue); 
			}
			if (StartOf(39)) {
				if (StartOf(14)) {
					Expr(/*Parser.Statement.atg:537*/out expr);
					/*Parser.Statement.atg:537*/ret.Expression = expr; 
				} else {
					Get();
					/*Parser.Statement.atg:538*/ret.ReturnVariant = ReturnVariant.Set; 
					Expr(/*Parser.Statement.atg:539*/out expr);
					/*Parser.Statement.atg:539*/ret.Expression = expr; 
					/*Parser.Statement.atg:540*/SemErr("Return value assignment is no longer supported. You must use local variables instead."); 
				}
			}
		} else if (la.kind == _break) {
			Get();
			/*Parser.Statement.atg:542*/if(bl == null)
			   ret = new AstReturn(this, ReturnVariant.Break); 
			else
			    jump = new AstExplicitGoTo(this, bl.BreakLabel);
			
		} else if (la.kind == _continue) {
			Get();
			/*Parser.Statement.atg:547*/if(bl == null)
			   ret = new AstReturn(this, ReturnVariant.Continue); 
			else
			    jump = new AstExplicitGoTo(this, bl.ContinueLabel);
			
		} else SynErr(157);
		/*Parser.Statement.atg:552*/block.Add((AstNode)ret ?? (AstNode)jump); 
	}

	void Throw(/*Parser.Statement.atg:675*/AstBlock block) {
		/*Parser.Statement.atg:675*/AstThrow th; 
		ThrowExpression(/*Parser.Statement.atg:677*/out th);
		/*Parser.Statement.atg:678*/block.Add(th); 
	}

	void LetBindingStmt(/*Parser.Statement.atg:594*/AstBlock block) {
		Expect(_let);
		LetBinder(/*Parser.Statement.atg:595*/block);
		while (la.kind == _comma) {
			Get();
			LetBinder(/*Parser.Statement.atg:595*/block);
		}
	}

	void Condition(/*Parser.Statement.atg:421*/AstBlock block) {
		/*Parser.Statement.atg:421*/AstExpr expr = null; bool isNegative = false; 
		if (la.kind == _if) {
			Get();
			/*Parser.Statement.atg:423*/isNegative = false; 
		} else if (la.kind == _unless) {
			Get();
			/*Parser.Statement.atg:424*/isNegative = true; 
		} else SynErr(158);
		Expect(_lpar);
		Expr(/*Parser.Statement.atg:427*/out expr);
		Expect(_rpar);
		/*Parser.Statement.atg:427*/if(expr == null)
		   expr = _createUnknownExpr();
		AstCondition cond = new AstCondition(this, expr, isNegative);
		target.BeginBlock(cond.IfBlock);
		
		StatementBlock(/*Parser.Statement.atg:433*/cond.IfBlock);
		/*Parser.Statement.atg:434*/target.EndBlock(); 
		if (la.kind == _else) {
			Get();
			/*Parser.Statement.atg:437*/target.BeginBlock(cond.ElseBlock); 
			StatementBlock(/*Parser.Statement.atg:438*/cond.ElseBlock);
			/*Parser.Statement.atg:439*/target.EndBlock(); 
		}
		/*Parser.Statement.atg:440*/block.Add(cond); 
	}

	void NestedFunction(/*Parser.Statement.atg:556*/AstBlock block) {
		/*Parser.Statement.atg:556*/PFunction func; 
		FunctionDefinition(/*Parser.Statement.atg:558*/out func);
		/*Parser.Statement.atg:560*/string logicalId = func.Meta[PFunction.LogicalIdKey];
		func.Meta[PFunction.ParentFunctionKey] = target.Function.Id;
		
		CompilerTarget ft = FunctionTargets[func];
		AstGetSetSymbol setVar = new AstGetSetSymbol(this, PCall.Set, 
		    SymbolEntry.LocalObjectVariable(logicalId));
		if(func.Meta[PFunction.LazyKey].Switch)
		{
		    //Capture environment by value                                        
		    var ps = ft.ToCaptureByValue(let_bindings(ft));
		    ft._DetermineSharedNames(); //Need to re-determine shared names since
		                                // ToCaptureByValue does not automatically modify shared names
		    var clos = new AstCreateClosure(this,  new SymbolEntry
		            (SymbolInterpretations.Function, func.Id, 
		            func.ParentApplication.Module.Name));
		    var callStub = new AstIndirectCall(this, clos);
		    callStub.Arguments.AddRange(ps(this));
		    setVar.Arguments.Add(callStub);
		}
		else if(ft.OuterVariables.Count > 0)
		{                                        
		    setVar.Arguments.Add( new AstCreateClosure(this,  new SymbolEntry(SymbolInterpretations.Function, func.Id, 
		            func.ParentApplication.Module.Name)) );                                        
		}
		else
		{
		    setVar.Arguments.Add( new AstGetSetReference(this, 
		        new SymbolEntry(SymbolInterpretations.Function, func.Id, func.ParentApplication.Module.Name)) );
		}
		block.Add(setVar);
		
	}

	void TryCatchFinally(/*Parser.Statement.atg:622*/AstBlock block) {
		/*Parser.Statement.atg:622*/AstTryCatchFinally a = new AstTryCatchFinally(this);
		AstGetSet excVar = null;
		
		Expect(_try);
		/*Parser.Statement.atg:626*/target.BeginBlock(a.TryBlock); 
		Expect(_lbrace);
		while (StartOf(20)) {
			Statement(/*Parser.Statement.atg:628*/a.TryBlock);
		}
		Expect(_rbrace);
		/*Parser.Statement.atg:630*/target.EndBlock(); 
		if (la.kind == _catch || la.kind == _finally) {
			if (la.kind == _catch) {
				Get();
				/*Parser.Statement.atg:631*/target.BeginBlock(a.CatchBlock); 
				if (la.kind == _lpar) {
					Get();
					GetCall(/*Parser.Statement.atg:633*/out excVar);
					/*Parser.Statement.atg:633*/a.ExceptionVar = excVar; 
					Expect(_rpar);
				} else if (la.kind == _lbrace) {
					/*Parser.Statement.atg:635*/SemErr(la,"catch-clauses that don't store the exception are illegal."); 
				} else SynErr(159);
				Expect(_lbrace);
				while (StartOf(20)) {
					Statement(/*Parser.Statement.atg:638*/a.CatchBlock);
				}
				Expect(_rbrace);
				/*Parser.Statement.atg:640*/target.EndBlock(); 
				if (la.kind == _finally) {
					Get();
					/*Parser.Statement.atg:643*/target.BeginBlock(a.FinallyBlock); 
					Expect(_lbrace);
					while (StartOf(20)) {
						Statement(/*Parser.Statement.atg:645*/a.FinallyBlock);
					}
					Expect(_rbrace);
					/*Parser.Statement.atg:647*/target.EndBlock(); 
				}
			} else {
				Get();
				/*Parser.Statement.atg:650*/target.BeginBlock(a.FinallyBlock); 
				Expect(_lbrace);
				while (StartOf(20)) {
					Statement(/*Parser.Statement.atg:652*/a.FinallyBlock);
				}
				Expect(_rbrace);
				/*Parser.Statement.atg:654*/target.EndBlock(); 
				if (la.kind == _catch) {
					/*Parser.Statement.atg:656*/target.BeginBlock(a.CatchBlock); 
					Get();
					if (la.kind == _lpar) {
						Get();
						GetCall(/*Parser.Statement.atg:659*/out excVar);
						/*Parser.Statement.atg:660*/a.ExceptionVar = excVar; 
						Expect(_rpar);
					} else if (la.kind == _lbrace) {
						/*Parser.Statement.atg:662*/SemErr(la,"catch-clauses that don't store the exception are illegal."); 
					} else SynErr(160);
					Expect(_lbrace);
					while (StartOf(20)) {
						Statement(/*Parser.Statement.atg:665*/a.CatchBlock);
					}
					Expect(_rbrace);
					/*Parser.Statement.atg:668*/target.EndBlock(); 
				}
			}
		}
		/*Parser.Statement.atg:671*/block.Add(a); 
	}

	void Using(/*Parser.Statement.atg:682*/AstBlock block) {
		/*Parser.Statement.atg:682*/AstUsing use = new AstUsing(this); 
		Expect(_uusing);
		Expect(_lpar);
		Expr(/*Parser.Statement.atg:684*/out use.Expression);
		Expect(_rpar);
		/*Parser.Statement.atg:685*/target.BeginBlock(use.Block); 
		StatementBlock(/*Parser.Statement.atg:686*/use.Block);
		/*Parser.Statement.atg:687*/target.EndBlock();
		block.Add(use); 
		
	}

	void Assignment(/*Parser.Statement.atg:393*/AstGetSet lvalue, out AstNode node) {
		/*Parser.Statement.atg:393*/AstExpr expr = null;
		BinaryOperator setModifier = BinaryOperator.None;
		AstTypeExpr T;
		node = lvalue;
		
		if (StartOf(9)) {
			switch (la.kind) {
			case _assign: {
				Get();
				/*Parser.Statement.atg:400*/setModifier = BinaryOperator.None; 
				break;
			}
			case _plus: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:401*/setModifier = BinaryOperator.Addition; 
				break;
			}
			case _minus: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:402*/setModifier = BinaryOperator.Subtraction; 
				break;
			}
			case _times: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:403*/setModifier = BinaryOperator.Multiply; 
				break;
			}
			case _div: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:404*/setModifier = BinaryOperator.Division; 
				break;
			}
			case _bitAnd: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:405*/setModifier = BinaryOperator.BitwiseAnd; 
				break;
			}
			case _bitOr: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:406*/setModifier = BinaryOperator.BitwiseOr; 
				break;
			}
			case _coalescence: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:407*/setModifier = BinaryOperator.Coalescence; 
				break;
			}
			}
			Expr(/*Parser.Statement.atg:408*/out expr);
		} else if (la.kind == _tilde) {
			Get();
			Expect(_assign);
			/*Parser.Statement.atg:410*/setModifier = BinaryOperator.Cast; 
			TypeExpr(/*Parser.Statement.atg:411*/out T);
			/*Parser.Statement.atg:411*/expr = T; 
		} else SynErr(161);
		/*Parser.Statement.atg:413*/lvalue.Arguments.Add(expr);
		lvalue.Call = PCall.Set; 
		if(setModifier != BinaryOperator.None)
		    node = AstModifyingAssignment.Create(this,setModifier,lvalue);
		
	}

	void AppendRightTermination(/*Parser.Statement.atg:108*/ref AstGetSet complex) {
		/*Parser.Statement.atg:108*/AstGetSet actualComplex; 
		Expect(_appendright);
		GetCall(/*Parser.Statement.atg:111*/out actualComplex);
		/*Parser.Statement.atg:111*/actualComplex.Arguments.RightAppend(complex);
		actualComplex.Arguments.ReleaseRightAppend();
		                                 //WARNING: Code is duplicated in ./GetInitiator and Parser.Expression.atg/RightAppendExpr and 
		if(actualComplex is AstGetSetSymbol 
		                                         && ((AstGetSetSymbol)actualComplex).IsObjectVariable)
		       actualComplex.Call = PCall.Set;
		   complex = actualComplex;
		
	}

	void Function(/*Parser.Statement.atg:335*/out AstGetSet function) {
		/*Parser.Statement.atg:335*/function = null; string id; 
		Id(/*Parser.Statement.atg:337*/out id);
		/*Parser.Statement.atg:337*/if(!target.Symbols.ContainsKey(id))
		{
		    function = new AstUnresolved(this, id);
		}
		else
		{
		    if(isOuterVariable(id))
		        target.RequireOuterVariable(id);
		    SymbolEntry sym = target.Symbols[id];
		    function = _assembleInvocation(sym);
		}
		
		Arguments(/*Parser.Statement.atg:349*/function.Arguments);
	}

	void Variable(/*Parser.Statement.atg:277*/out AstGetSet complex, out bool isDeclared) {
		/*Parser.Statement.atg:277*/string id; 
		isDeclared = false; 
		complex = null; 
		bool isNewDecl = false;
		
		if (StartOf(40)) {
			if (la.kind == _new) {
				Get();
				/*Parser.Statement.atg:283*/isNewDecl = true; 
			}
			VariableDeclaration(/*Parser.Statement.atg:285*/out complex, isNewDecl);
			/*Parser.Statement.atg:285*/isDeclared = true; 
		} else if (StartOf(4)) {
			Id(/*Parser.Statement.atg:286*/out id);
			/*Parser.Statement.atg:287*/SymbolEntry varSym;
			if(target.Symbols.TryGetValue(id, out varSym))
			{
			    if(InterpretationIsLocalVariable(varSym.Interpretation))
			    {
			        if(isOuterVariable(id))
			            target.RequireOuterVariable(id);                            
			    }
			    else if(!InterpretationIsVariable(varSym.Interpretation))
			    {
			        SemErr(t.line, t.col, "Variable name expected but was " + 
			            Enum.GetName(typeof(SymbolInterpretations),
			                varSym.Interpretation));
			    }
			    complex = new AstGetSetSymbol(this, varSym);
			}
			else
			{
			    //Unknown symbols are treated as functions. 
			    //  See production Function for details.
			    SemErr(t.line, t.col, 
			        "Internal compiler error. Did not catch unknown identifier.");
			    complex = new AstGetSetSymbol(this, 
			        SymbolEntry.LocalObjectVariable("Not_a_Variable_Id"));
			}
			
		} else SynErr(162);
	}

	void StaticCall(/*Parser.Statement.atg:353*/out AstGetSetStatic staticCall) {
		/*Parser.Statement.atg:353*/AstTypeExpr typeExpr;
		string memberId;
		staticCall = null;
		
		ExplicitTypeExpr(/*Parser.Statement.atg:358*/out typeExpr);
		Expect(_dot);
		Id(/*Parser.Statement.atg:359*/out memberId);
		/*Parser.Statement.atg:359*/staticCall = new AstGetSetStatic(this, PCall.Get, typeExpr, memberId); 
		Arguments(/*Parser.Statement.atg:360*/staticCall.Arguments);
	}

	void VariableDeclaration(/*Parser.Statement.atg:231*/out AstGetSet variable, bool isNewDecl) {
		/*Parser.Statement.atg:231*/variable = null; 
		string staticId = null; 
		string id = null;
		bool isOverrideDecl = false;
		
		/*Parser.Statement.atg:236*/SymbolInterpretations kind = SymbolInterpretations.Undefined; 
		if (la.kind == _var || la.kind == _ref) {
			if (la.kind == _var) {
				Get();
				/*Parser.Statement.atg:237*/kind = SymbolInterpretations.LocalObjectVariable; 
			} else {
				Get();
				/*Parser.Statement.atg:238*/kind = SymbolInterpretations.LocalReferenceVariable; 
			}
			if (la.kind == _new) {
				Get();
				/*Parser.Statement.atg:240*/isOverrideDecl = true; 
			}
			Id(/*Parser.Statement.atg:242*/out id);
			/*Parser.Statement.atg:243*/SmartDeclareLocal(id, kind, isOverrideDecl);
			staticId = id; 
			
		} else if (la.kind == _static) {
			Get();
			/*Parser.Statement.atg:246*/kind = SymbolInterpretations.GlobalObjectVariable; 
			if (la.kind == _var || la.kind == _ref) {
				if (la.kind == _var) {
					Get();
				} else {
					Get();
					/*Parser.Statement.atg:248*/kind = SymbolInterpretations.GlobalReferenceVariable; 
				}
			}
			Id(/*Parser.Statement.atg:250*/out id);
			/*Parser.Statement.atg:250*/staticId = target.Function.Id + "\\static\\" + id;
			target.DeclareModuleLocal(kind, id, staticId);
			VariableDeclaration vari;
			DefineGlobalVariable(staticId, out vari);
			
		} else SynErr(163);
		/*Parser.Statement.atg:255*/if(id == null)
		{ // there was an error parsing the variable kind/id; return a dummy node
		    variable = _createUnknownGetSet();
		}
		else
		{
		    variable = InterpretationIsObjectVariable(kind) ?
		new AstGetSetSymbol(this, PCall.Get, target.Symbols[id])
		:
		 new AstGetSetReference(this, PCall.Get, target.Symbols[id].With(interpretation: target.Symbols[id].Interpretation.ToObjectVariable())); 
		                                 
		                                 if(isNewDecl)
		                                     variable = new AstGetSetNewDecl(this)
		                                     {
		                                         Expression = variable,
		                                         Id = target.Symbols[id].InternalId
		                                     };              
		                             }
		                         
	}

	void LetBinder(/*Parser.Statement.atg:599*/AstBlock block) {
		/*Parser.Statement.atg:599*/string id = null;
		AstExpr thunk;
		
		Id(/*Parser.Statement.atg:603*/out id);
		/*Parser.Statement.atg:604*/SmartDeclareLocal(id, SymbolInterpretations.LocalObjectVariable);
		mark_as_let(target.Function, id);
		if(la.kind == _assign)
		    _inject(_lazy,"lazy"); 
		
		if (la.kind == _assign) {
			Get();
			LazyExpression(/*Parser.Statement.atg:610*/out thunk);
			/*Parser.Statement.atg:613*/var assign = new AstGetSetSymbol(this, PCall.Set, 
			   SymbolEntry.LocalObjectVariable(id));
			assign.Arguments.Add(thunk);
			block.Add(assign);
			
		}
	}


#line 122 "D:\DotNetProjects\Prexonite-Hg\prx-assembla-hg\Tools\Parser.frame" //FRAME


	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();

#line default //END FRAME -->parseRoot

		Prexonite();

#line 128 "D:\DotNetProjects\Prexonite-Hg\prx-assembla-hg\Tools\Parser.frame" //FRAME

    Expect(0);
	}
	
	bool[,] set = {

#line default //END FRAME -->initialization

		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,T,T,x, x,x,T,T, T,T,T,T, x,T,x,x, T,T,T,T, T,T,T,x, T,T,T,x, T,x,T,T, T,T,T,x, x,x,x,T, T,T,T,T, x,x,T,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,T,T,T, x,T,x,x, T,T,T,T, T,T,T,x, T,T,T,x, T,x,T,T, T,T,T,x, x,x,x,T, T,T,T,T, x,x,T,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, T,x,x,T, x,x,x,x, x,x,T,x, x,T,T,x, x,x,T,T, T,T,T,T, x,T,x,x, T,T,T,T, T,T,T,x, T,T,T,x, T,x,T,T, T,T,T,x, x,x,x,T, T,T,T,T, x,x,T,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,T,x,x, x,T,x,x, x,T,T,T, x,T,T,x, T,T,T,x, T,x,T,T, T,T,T,x, x,x,x,T, T,T,T,T, x,x,T,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, T,x,T,T, T,T,x,x, x,T,x,x, x,x,x,T, T,T,T,x, x,T,x,x, T,x,x,x, x,T,x,x, x,T,x,T, T,x,x,x, x,T,T,T, T,x,x,x, x,T,T,T, x,x,T,x, T,x,x,x, T,x,x,x, x,x,x,x, x,T,T,x, x,x,T,T, x,T,x,T, T,T,T,x, x,x,T,x, x,x,T,x, x,T,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,T,T, x,x,T,x, x,x,x,x, x,x,x,x, x,T,x,T, T,x,x,x, x,x,T,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,T,T, T,T,T,T, T,T,T,T, x,T,x,T, T,T,T,T, T,T,T,T, T,T,T,T, T,x,T,x, x,x,x,x, T,x,x,x, x,x,T,T, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, T,T,T,x, x,x,x,x, x,T,x,x, x,x,x,x, x,T,x,x, x,x,x,T, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,T, x,x,x,T, x,T,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,T, x,x,x,x, x,T,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, T,x,T,T, T,T,x,x, x,T,x,x, x,x,x,T, T,T,T,x, x,T,x,x, T,x,x,x, x,T,x,x, x,T,x,T, T,x,x,x, x,T,T,T, T,x,x,x, x,T,T,T, x,x,T,x, T,x,x,x, T,x,x,x, x,x,x,x, x,T,T,T, T,x,T,T, x,T,x,T, T,T,T,x, x,x,T,x, x,x,T,x, x,T,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,T, T,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,T,x,x, x,T,x,T, T,x,x,x, x,T,T,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,T,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,T,x, x,x,x,x, x,x,x,x, x,T,x,T, x,T,x,T, T,x,x,x, x,T,T,x, x,x,x,x, x,x,T,T, T,x,T,T, T,T,x,x, T,T,T,T, x,x,x,x, T,T,x,T, T,x,T,T, x,T,x,T, T,T,T,T, x,x,T,x, T,T,T,T, x,x,x,x},
		{x,T,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,T,x,x, x,T,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,T,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,T,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,T,x,T, x,T,x,T, T,x,x,x, x,T,T,x, x,x,x,x, x,x,T,T, x,x,x,T, T,T,x,x, T,T,T,T, x,x,x,x, T,T,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,T, x,x,x,x},
		{x,T,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,T,x,x, x,T,x,T, T,x,x,x, x,T,T,x, x,x,x,x, x,x,T,T, x,x,x,T, T,T,x,x, T,T,T,T, x,x,x,x, T,T,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,T, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,T, x,T,x,T, T,T,T,T, x,x,x,x, T,T,T,x, x,x,x,x},
		{x,x,x,x, T,x,T,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,T,x,x, x,T,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, x,T,x,x, T,x,x,T, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, x,T,x,x, x,x,x,T, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, x,T,x,x, x,x,x,T, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,T,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,T,T, T,T,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, T,x,T,T, T,T,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,T,x, x,x,x,x, x,x,x,x, x,T,x,T, x,T,x,T, T,x,x,x, x,T,T,x, x,x,x,x, x,x,T,T, T,x,T,T, T,T,x,x, T,T,T,T, x,x,x,x, T,T,x,T, T,x,T,T, x,T,T,T, T,T,T,T, x,x,T,x, T,T,T,T, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,T,T, x,x,T,x, x,x,x,x, x,x,x,x, x,T,x,T, T,x,x,x, x,T,T,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, T,x,T,T, T,T,x,T, x,T,x,x, x,x,x,T, T,T,T,x, x,T,x,x, T,x,x,x, x,T,x,x, x,T,x,T, T,x,x,x, x,T,T,T, T,x,x,x, x,T,T,T, x,x,T,x, T,x,x,x, T,x,x,x, x,x,x,x, x,T,T,T, T,x,T,T, x,T,x,T, T,T,T,x, x,x,T,x, x,x,T,x, x,T,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x}

#line 133 "D:\DotNetProjects\Prexonite-Hg\prx-assembla-hg\Tools\Parser.frame" //FRAME

	};
} // end Parser

[System.Diagnostics.DebuggerStepThrough()]
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
			case 96: s = "ENDKEYWORDS expected"; break;
			case 97: s = "LPopExpr expected"; break;
			case 98: s = "??? expected"; break;
			case 99: s = "invalid AsmStatementBlock"; break;
			case 100: s = "invalid AsmInstruction"; break;
			case 101: s = "invalid AsmInstruction"; break;
			case 102: s = "invalid AsmInstruction"; break;
			case 103: s = "invalid AsmInstruction"; break;
			case 104: s = "invalid AsmInstruction"; break;
			case 105: s = "invalid AsmId"; break;
			case 106: s = "invalid SignedReal"; break;
			case 107: s = "invalid Boolean"; break;
			case 108: s = "invalid Id"; break;
			case 109: s = "invalid Expr"; break;
			case 110: s = "invalid AssignExpr"; break;
			case 111: s = "invalid AssignExpr"; break;
			case 112: s = "invalid TypeExpr"; break;
			case 113: s = "invalid GetSetExtension"; break;
			case 114: s = "invalid Primary"; break;
			case 115: s = "invalid Constant"; break;
			case 116: s = "invalid ListLiteral"; break;
			case 117: s = "invalid HashLiteral"; break;
			case 118: s = "invalid LoopExpr"; break;
			case 119: s = "invalid LambdaExpression"; break;
			case 120: s = "invalid LambdaExpression"; break;
			case 121: s = "invalid LazyExpression"; break;
			case 122: s = "invalid GetInitiator"; break;
			case 123: s = "invalid GetInitiator"; break;
			case 124: s = "invalid Real"; break;
			case 125: s = "invalid WhileLoop"; break;
			case 126: s = "invalid WhileLoop"; break;
			case 127: s = "invalid ForLoop"; break;
			case 128: s = "invalid ForLoop"; break;
			case 129: s = "invalid Arguments"; break;
			case 130: s = "invalid Arguments"; break;
			case 131: s = "invalid Statement"; break;
			case 132: s = "invalid ExplicitTypeExpr"; break;
			case 133: s = "invalid PrexoniteTypeExpr"; break;
			case 134: s = "invalid ClrTypeExpr"; break;
			case 135: s = "invalid TypeExprElement"; break;
			case 136: s = "this symbol not expected in Prexonite"; break;
			case 137: s = "invalid GlobalVariableDefinition"; break;
			case 138: s = "invalid GlobalVariableDefinition"; break;
			case 139: s = "this symbol not expected in Declaration"; break;
			case 140: s = "invalid Declaration"; break;
			case 141: s = "invalid MetaAssignment"; break;
			case 142: s = "invalid MetaAssignment"; break;
			case 143: s = "this symbol not expected in BuildBlock"; break;
			case 144: s = "invalid FunctionDefinition"; break;
			case 145: s = "invalid FunctionDefinition"; break;
			case 146: s = "invalid FunctionDefinition"; break;
			case 147: s = "invalid FunctionDefinition"; break;
			case 148: s = "invalid GlobalId"; break;
			case 149: s = "invalid MetaExpr"; break;
			case 150: s = "invalid MetaExpr"; break;
			case 151: s = "invalid GlobalQualifiedId"; break;
			case 152: s = "invalid Version"; break;
			case 153: s = "invalid ExplicitLabel"; break;
			case 154: s = "invalid SimpleStatement"; break;
			case 155: s = "invalid StructureStatement"; break;
			case 156: s = "invalid GetSetComplex"; break;
			case 157: s = "invalid Return"; break;
			case 158: s = "invalid Condition"; break;
			case 159: s = "invalid TryCatchFinally"; break;
			case 160: s = "invalid TryCatchFinally"; break;
			case 161: s = "invalid Assignment"; break;
			case 162: s = "invalid Variable"; break;
			case 163: s = "invalid VariableDeclaration"; break;

#line 160 "D:\DotNetProjects\Prexonite-Hg\prx-assembla-hg\Tools\Parser.frame" //FRAME

			default: s = "error " + n; break;
		}
		if(s.EndsWith(" expected"))
            s = "after \"" + parentParser.t.ToString(false) + "\", " + s.Replace("expected","is expected") + " and not \"" + parentParser.la.ToString(false) + "\"";
		else if(s.StartsWith("this symbol "))
		    s = "\"" + parentParser.t.val + "\"" + s.Substring(12);
        var msg = Message.Error(s, parentParser.scanner.File, line, col, "E"+n);
		AddLast(msg);
        OnMessageReceived(msg);
	}
	//TODO: enable message classes for semantic errors
	internal void SemErr (int line, int col, string s) {
        var msg = Message.Error(s, parentParser.scanner.File, line, col);
		AddLast(msg);
        OnMessageReceived(msg);

	}
	
	internal void SemErr (string s) {
        var msg = Message.Error(s, parentParser.scanner.File, parentParser.la.line, parentParser.la.col);
		AddLast(msg);
        OnMessageReceived(msg);
	}
	
	internal void Warning (int line, int col, string s) {
        var msg = Message.Warning(s, parentParser.scanner.File, line, col);
		AddLast(msg);
        OnMessageReceived(msg);

	}

    internal void Info (int line, int col, string s) {
        var msg = Message.Info(s, parentParser.scanner.File, line, col);
		AddLast(msg);
        OnMessageReceived(msg);
	}
	
	internal void Warning(string s) {
        var msg = Message.Warning(s, parentParser.scanner.File, parentParser.la.line, parentParser.la.col);
		AddLast(msg);
        OnMessageReceived(msg);
	}

    internal void Info(string s) {
        var msg = Message.Info(s, parentParser.scanner.File, parentParser.la.line, parentParser.la.col);
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