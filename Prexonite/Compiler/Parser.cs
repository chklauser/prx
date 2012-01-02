//SOURCE ARRAY
/*Header.atg:29*/using System.IO;
using Prexonite;
using System.Collections.Generic;
using System.Linq;
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
	public const int _integer = 5;
	public const int _real = 6;
	public const int _string = 7;
	public const int _bitAnd = 8;
	public const int _assign = 9;
	public const int _comma = 10;
	public const int _dec = 11;
	public const int _div = 12;
	public const int _dot = 13;
	public const int _eq = 14;
	public const int _gt = 15;
	public const int _ge = 16;
	public const int _inc = 17;
	public const int _lbrace = 18;
	public const int _lbrack = 19;
	public const int _lpar = 20;
	public const int _lt = 21;
	public const int _le = 22;
	public const int _minus = 23;
	public const int _ne = 24;
	public const int _bitOr = 25;
	public const int _plus = 26;
	public const int _pow = 27;
	public const int _rbrace = 28;
	public const int _rbrack = 29;
	public const int _rpar = 30;
	public const int _tilde = 31;
	public const int _times = 32;
	public const int _semicolon = 33;
	public const int _colon = 34;
	public const int _doublecolon = 35;
	public const int _coalescence = 36;
	public const int _question = 37;
	public const int _pointer = 38;
	public const int _implementation = 39;
	public const int _at = 40;
	public const int _appendleft = 41;
	public const int _appendright = 42;
	public const int _var = 43;
	public const int _ref = 44;
	public const int _true = 45;
	public const int _false = 46;
	public const int _BEGINKEYWORDS = 47;
	public const int _mod = 48;
	public const int _is = 49;
	public const int _as = 50;
	public const int _not = 51;
	public const int _enabled = 52;
	public const int _disabled = 53;
	public const int _function = 54;
	public const int _command = 55;
	public const int _asm = 56;
	public const int _declare = 57;
	public const int _build = 58;
	public const int _return = 59;
	public const int _in = 60;
	public const int _to = 61;
	public const int _add = 62;
	public const int _continue = 63;
	public const int _break = 64;
	public const int _yield = 65;
	public const int _or = 66;
	public const int _and = 67;
	public const int _xor = 68;
	public const int _label = 69;
	public const int _goto = 70;
	public const int _static = 71;
	public const int _null = 72;
	public const int _if = 73;
	public const int _unless = 74;
	public const int _else = 75;
	public const int _new = 76;
	public const int _coroutine = 77;
	public const int _from = 78;
	public const int _do = 79;
	public const int _does = 80;
	public const int _while = 81;
	public const int _until = 82;
	public const int _for = 83;
	public const int _foreach = 84;
	public const int _try = 85;
	public const int _catch = 86;
	public const int _finally = 87;
	public const int _throw = 88;
	public const int _then = 89;
	public const int _uusing = 90;
	public const int _macro = 91;
	public const int _lazy = 92;
	public const int _let = 93;
	public const int _ENDKEYWORDS = 94;
	public const int _LPopExpr = 95;
	public enum Terminals
	{
		@EOF = 0,
		@id = 1,
		@anyId = 2,
		@lid = 3,
		@ns = 4,
		@integer = 5,
		@real = 6,
		@string = 7,
		@bitAnd = 8,
		@assign = 9,
		@comma = 10,
		@dec = 11,
		@div = 12,
		@dot = 13,
		@eq = 14,
		@gt = 15,
		@ge = 16,
		@inc = 17,
		@lbrace = 18,
		@lbrack = 19,
		@lpar = 20,
		@lt = 21,
		@le = 22,
		@minus = 23,
		@ne = 24,
		@bitOr = 25,
		@plus = 26,
		@pow = 27,
		@rbrace = 28,
		@rbrack = 29,
		@rpar = 30,
		@tilde = 31,
		@times = 32,
		@semicolon = 33,
		@colon = 34,
		@doublecolon = 35,
		@coalescence = 36,
		@question = 37,
		@pointer = 38,
		@implementation = 39,
		@at = 40,
		@appendleft = 41,
		@appendright = 42,
		@var = 43,
		@ref = 44,
		@true = 45,
		@false = 46,
		@BEGINKEYWORDS = 47,
		@mod = 48,
		@is = 49,
		@as = 50,
		@not = 51,
		@enabled = 52,
		@disabled = 53,
		@function = 54,
		@command = 55,
		@asm = 56,
		@declare = 57,
		@build = 58,
		@return = 59,
		@in = 60,
		@to = 61,
		@add = 62,
		@continue = 63,
		@break = 64,
		@yield = 65,
		@or = 66,
		@and = 67,
		@xor = 68,
		@label = 69,
		@goto = 70,
		@static = 71,
		@null = 72,
		@if = 73,
		@unless = 74,
		@else = 75,
		@new = 76,
		@coroutine = 77,
		@from = 78,
		@do = 79,
		@does = 80,
		@while = 81,
		@until = 82,
		@for = 83,
		@foreach = 84,
		@try = 85,
		@catch = 86,
		@finally = 87,
		@throw = 88,
		@then = 89,
		@uusing = 90,
		@macro = 91,
		@lazy = 92,
		@let = 93,
		@ENDKEYWORDS = 94,
		@LPopExpr = 95,
	}
	const int maxT = 96;

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
		} else SynErr(97);
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
			target.DefineModuleLocal(kind, id);
			
			while (la.kind == _comma) {
				Get();
				AsmId(/*Parser.Assembler.atg:60*/out id);
				/*Parser.Assembler.atg:62*/target.Function.Variables.Add(id);
				target.DefineModuleLocal(kind, id);
				
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
			} else SynErr(98);
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
			} else SynErr(99);
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
			} else SynErr(100);
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
			} else SynErr(101);
			AsmQualid(/*Parser.Assembler.atg:202*/out id);
			/*Parser.Assembler.atg:203*/code = getOpCode(insbase, detail);
			addInstruction(block, new Instruction(code, arguments, id, justEffect));
			
		} else if (StartOf(2)) {
			AsmId(/*Parser.Assembler.atg:208*/out insbase);
			/*Parser.Assembler.atg:208*/SemErr("Invalid assembler instruction \"" + insbase + "\" (" + t + ")."); 
		} else SynErr(102);
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
		} else SynErr(103);
	}

	void Integer(/*Parser.Helper.atg:47*/out int value) {
		Expect(_integer);
		/*Parser.Helper.atg:48*/if(!TryParseInteger(t.val, out value))
		   SemErr(t, "Cannot recognize integer " + t.val);
		
	}

	void SignedReal(/*Parser.Helper.atg:76*/out double value) {
		/*Parser.Helper.atg:76*/value = 0.0; double modifier = 1.0; int ival; 
		if (la.kind == _minus || la.kind == _plus) {
			if (la.kind == _plus) {
				Get();
			} else {
				Get();
				/*Parser.Helper.atg:79*/modifier = -1.0; 
			}
		}
		if (la.kind == _real) {
			Real(/*Parser.Helper.atg:80*/out value);
		} else if (la.kind == _integer) {
			Integer(/*Parser.Helper.atg:81*/out ival);
			/*Parser.Helper.atg:81*/value = ival; 
		} else SynErr(104);
		/*Parser.Helper.atg:83*/value = modifier * value; 
	}

	void Boolean(/*Parser.Helper.atg:40*/out bool value) {
		/*Parser.Helper.atg:40*/value = true; 
		if (la.kind == _true) {
			Get();
		} else if (la.kind == _false) {
			Get();
			/*Parser.Helper.atg:43*/value = false; 
		} else SynErr(105);
	}

	void SignedInteger(/*Parser.Helper.atg:53*/out int value) {
		/*Parser.Helper.atg:53*/int modifier = 1; 
		if (la.kind == _minus || la.kind == _plus) {
			if (la.kind == _plus) {
				Get();
			} else {
				Get();
				/*Parser.Helper.atg:56*/modifier = -1; 
			}
		}
		Integer(/*Parser.Helper.atg:57*/out value);
		/*Parser.Helper.atg:57*/value = modifier * value; 
	}

	void AsmQualid(/*Parser.Assembler.atg:249*/out string qualid) {
		
		AsmId(/*Parser.Assembler.atg:251*/out qualid);
	}

	void String(/*Parser.Helper.atg:87*/out string value) {
		Expect(_string);
		/*Parser.Helper.atg:88*/value = cache(t.val); 
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
		} else SynErr(106);
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
		} else SynErr(107);
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

	void GetCall(/*Parser.Statement.atg:500*/out AstGetSet complex) {
		/*Parser.Statement.atg:500*/AstGetSet getMember = null; bool isDeclaration; 
		GetInitiator(/*Parser.Statement.atg:502*/out complex, out isDeclaration);
		while (la.kind == _dot || la.kind == _lbrack) {
			GetSetExtension(/*Parser.Statement.atg:503*/complex, out getMember);
		}
		/*Parser.Statement.atg:505*/if(getMember != null) 
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
			} else SynErr(108);
			/*Parser.Expression.atg:222*/assignment.Arguments.Add(expr); 
			if(setModifier == BinaryOperator.None)
			    expr = assignment;
			else
			    expr = AstModifyingAssignment.Create(this,setModifier, assignment);
			
		} else if (StartOf(10)) {
		} else SynErr(109);
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
		} else SynErr(110);
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

	void GetSetExtension(/*Parser.Statement.atg:120*/AstExpr subject, out AstGetSet extension) {
		/*Parser.Statement.atg:120*/extension = null; string id;
		if(subject == null)
		{
			SemErr("Member access not preceded by a proper expression.");
			subject = new AstConstant(this,null);
		}
		                             
		if (/*Parser.Statement.atg:130*/isIndirectCall() ) {
			Expect(_dot);
			/*Parser.Statement.atg:130*/extension = new AstIndirectCall(this, PCall.Get, subject); 
			Arguments(/*Parser.Statement.atg:131*/extension.Arguments);
		} else if (la.kind == _dot) {
			Get();
			Id(/*Parser.Statement.atg:133*/out id);
			/*Parser.Statement.atg:133*/extension = new AstGetSetMemberAccess(this, PCall.Get, subject, id); 
			Arguments(/*Parser.Statement.atg:134*/extension.Arguments);
		} else if (la.kind == _lbrack) {
			/*Parser.Statement.atg:136*/AstExpr expr; 
			extension = new AstGetSetMemberAccess(this, PCall.Get, subject, ""); 
			
			Get();
			if (StartOf(14)) {
				Expr(/*Parser.Statement.atg:140*/out expr);
				/*Parser.Statement.atg:140*/extension.Arguments.Add(expr); 
				while (WeakSeparator(_comma,14,15) ) {
					Expr(/*Parser.Statement.atg:141*/out expr);
					/*Parser.Statement.atg:141*/extension.Arguments.Add(expr); 
				}
			}
			Expect(_rbrack);
		} else SynErr(111);
	}

	void Primary(/*Parser.Expression.atg:266*/out AstExpr expr) {
		/*Parser.Expression.atg:266*/expr = null;
		AstGetSet complex = null; bool declared; 
		if (la.kind == _asm) {
			/*Parser.Expression.atg:269*/_pushLexerState(Lexer.Asm); 
			/*Parser.Expression.atg:269*/AstBlock blockExpr = new AstBlock(this); 
			Get();
			Expect(_lpar);
			while (StartOf(1)) {
				AsmInstruction(/*Parser.Expression.atg:270*/blockExpr);
			}
			Expect(_rpar);
			/*Parser.Expression.atg:271*/_popLexerState(); 
			/*Parser.Expression.atg:271*/expr = blockExpr; 
		} else if (StartOf(16)) {
			Constant(/*Parser.Expression.atg:272*/out expr);
		} else if (la.kind == _coroutine) {
			CoroutineCreation(/*Parser.Expression.atg:273*/out expr);
		} else if (la.kind == _lbrack) {
			ListLiteral(/*Parser.Expression.atg:274*/out expr);
		} else if (la.kind == _lbrace) {
			HashLiteral(/*Parser.Expression.atg:275*/out expr);
		} else if (StartOf(17)) {
			LoopExpr(/*Parser.Expression.atg:276*/out expr);
		} else if (la.kind == _throw) {
			/*Parser.Expression.atg:277*/AstThrow th; 
			ThrowExpression(/*Parser.Expression.atg:278*/out th);
			/*Parser.Expression.atg:278*/expr = th; 
		} else if (/*Parser.Expression.atg:280*/isLambdaExpression()) {
			LambdaExpression(/*Parser.Expression.atg:280*/out expr);
		} else if (la.kind == _lazy) {
			LazyExpression(/*Parser.Expression.atg:281*/out expr);
		} else if (la.kind == _lpar) {
			Get();
			Expr(/*Parser.Expression.atg:282*/out expr);
			Expect(_rpar);
		} else if (/*Parser.Expression.atg:283*/_isNotNewDecl()) {
			ObjectCreation(/*Parser.Expression.atg:283*/out expr);
		} else if (StartOf(18)) {
			GetInitiator(/*Parser.Expression.atg:284*/out complex, out declared);
			/*Parser.Expression.atg:285*/expr = complex; 
		} else if (la.kind == _LPopExpr) {
			Get();
			Expect(_lpar);
			Expr(/*Parser.Expression.atg:286*/out expr);
			/*Parser.Expression.atg:291*/_popLexerState(); _inject(_plus); 
			Expect(_rpar);
		} else SynErr(112);
	}

	void Constant(/*Parser.Expression.atg:296*/out AstExpr expr) {
		/*Parser.Expression.atg:296*/expr = null; int vi; double vr; bool vb; string vs; 
		if (la.kind == _integer) {
			Integer(/*Parser.Expression.atg:298*/out vi);
			/*Parser.Expression.atg:298*/expr = new AstConstant(this, vi); 
		} else if (la.kind == _real) {
			Real(/*Parser.Expression.atg:299*/out vr);
			/*Parser.Expression.atg:299*/expr = new AstConstant(this, vr); 
		} else if (la.kind == _true || la.kind == _false) {
			Boolean(/*Parser.Expression.atg:300*/out vb);
			/*Parser.Expression.atg:300*/expr = new AstConstant(this, vb); 
		} else if (la.kind == _string) {
			String(/*Parser.Expression.atg:301*/out vs);
			/*Parser.Expression.atg:301*/expr = new AstConstant(this, vs); 
		} else if (la.kind == _null) {
			Null();
			/*Parser.Expression.atg:302*/expr = new AstConstant(this, null); 
		} else SynErr(113);
	}

	void CoroutineCreation(/*Parser.Expression.atg:369*/out AstExpr expr) {
		/*Parser.Expression.atg:370*/AstCreateCoroutine cor = new AstCreateCoroutine(this); 
		AstExpr iexpr;
		expr = cor;
		
		Expect(_coroutine);
		Expr(/*Parser.Expression.atg:375*/out iexpr);
		/*Parser.Expression.atg:375*/cor.Expression = iexpr; 
		if (la.kind == _for) {
			Get();
			Arguments(/*Parser.Expression.atg:376*/cor.Arguments);
		}
	}

	void ListLiteral(/*Parser.Expression.atg:306*/out AstExpr expr) {
		/*Parser.Expression.atg:306*/AstExpr iexpr = null; 
		AstListLiteral lst = new AstListLiteral(this);
		expr = lst;
		bool missingExpr = false;
		
		Expect(_lbrack);
		if (StartOf(14)) {
			Expr(/*Parser.Expression.atg:313*/out iexpr);
			/*Parser.Expression.atg:313*/lst.Elements.Add(iexpr); 
			while (la.kind == _comma) {
				Get();
				/*Parser.Expression.atg:314*/if(missingExpr)
				   SemErr("Missing expression in list literal (two consecutive commas).");
				
				if (StartOf(14)) {
					Expr(/*Parser.Expression.atg:317*/out iexpr);
					/*Parser.Expression.atg:317*/lst.Elements.Add(iexpr); 
					missingExpr = false; 
					
				} else if (la.kind == _comma || la.kind == _rbrack) {
					/*Parser.Expression.atg:320*/missingExpr = true; 
				} else SynErr(114);
			}
		}
		Expect(_rbrack);
	}

	void HashLiteral(/*Parser.Expression.atg:328*/out AstExpr expr) {
		/*Parser.Expression.atg:328*/AstExpr iexpr = null; 
		AstHashLiteral hash = new AstHashLiteral(this);
		expr = hash;
		                                 bool missingExpr = false;
		
		Expect(_lbrace);
		if (StartOf(14)) {
			Expr(/*Parser.Expression.atg:335*/out iexpr);
			/*Parser.Expression.atg:335*/hash.Elements.Add(iexpr); 
			while (la.kind == _comma) {
				Get();
				/*Parser.Expression.atg:336*/if(missingExpr)
				           SemErr("Missing expression in list literal (two consecutive commas).");
				   
				if (StartOf(14)) {
					Expr(/*Parser.Expression.atg:339*/out iexpr);
					/*Parser.Expression.atg:339*/hash.Elements.Add(iexpr); 
					      missingExpr = false;
					  
				} else if (la.kind == _comma || la.kind == _rbrace) {
					/*Parser.Expression.atg:342*/missingExpr = true; 
				} else SynErr(115);
			}
		}
		Expect(_rbrace);
	}

	void LoopExpr(/*Parser.Expression.atg:350*/out AstExpr expr) {
		/*Parser.Expression.atg:350*/AstBlock dummyBlock = new AstBlock(this);
		
		if (la.kind == _do || la.kind == _while || la.kind == _until) {
			WhileLoop(/*Parser.Expression.atg:353*/dummyBlock);
		} else if (la.kind == _for) {
			ForLoop(/*Parser.Expression.atg:354*/dummyBlock);
		} else if (la.kind == _foreach) {
			ForeachLoop(/*Parser.Expression.atg:355*/dummyBlock);
		} else SynErr(116);
		/*Parser.Expression.atg:356*/expr = new AstLoopExpression(this, (AstLoop) dummyBlock.Statements[0]); 
	}

	void ThrowExpression(/*Parser.Expression.atg:481*/out AstThrow th) {
		/*Parser.Expression.atg:481*/th = new AstThrow(this); 
		Expect(_throw);
		Expr(/*Parser.Expression.atg:484*/out th.Expression);
	}

	void LambdaExpression(/*Parser.Expression.atg:380*/out AstExpr expr) {
		/*Parser.Expression.atg:380*/expr = null;
		PFunction func = new PFunction(TargetApplication, generateLocalId());                                             
		func.Meta[Application.ImportKey] = target.Function.Meta[Application.ImportKey];
		func.Meta[PFunction.ParentFunctionKey] = target.Function.Id;
		TargetApplication.Functions.Add(func);
		Loader.CreateFunctionTarget(func, new AstBlock(this));
		CompilerTarget ft = FunctionTargets[func];
		ft.ParentTarget = target;
		
		if (StartOf(19)) {
			FormalArg(/*Parser.Expression.atg:390*/ft);
		} else if (la.kind == _lpar) {
			Get();
			if (StartOf(19)) {
				FormalArg(/*Parser.Expression.atg:392*/ft);
				while (la.kind == _comma) {
					Get();
					FormalArg(/*Parser.Expression.atg:394*/ft);
				}
			}
			Expect(_rpar);
		} else SynErr(117);
		/*Parser.Expression.atg:400*/CompilerTarget oldTarget = target;
		target = ft;
		
		Expect(_implementation);
		if (la.kind == _lbrace) {
			Get();
			while (StartOf(20)) {
				Statement(/*Parser.Expression.atg:405*/ft.Ast);
			}
			Expect(_rbrace);
		} else if (StartOf(14)) {
			/*Parser.Expression.atg:407*/AstReturn ret = new AstReturn(this, ReturnVariant.Exit); 
			Expr(/*Parser.Expression.atg:408*/out ret.Expression);
			/*Parser.Expression.atg:408*/ft.Ast.Add(ret); 
		} else SynErr(118);
		/*Parser.Expression.atg:411*/target = oldTarget;
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

	void LazyExpression(/*Parser.Expression.atg:430*/out AstExpr expr) {
		/*Parser.Expression.atg:430*/expr = null;
		PFunction func = new PFunction(TargetApplication, generateLocalId());
		func.Meta[Application.ImportKey] = target.Function.Meta[Application.ImportKey];
		func.Meta[PFunction.ParentFunctionKey] = target.Function.Id;
		TargetApplication.Functions.Add(func);
		Loader.CreateFunctionTarget(func, new AstBlock(this));
		CompilerTarget ft = FunctionTargets[func];
		ft.ParentTarget = target;
		
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
		} else SynErr(119);
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

	void ObjectCreation(/*Parser.Expression.atg:361*/out AstExpr expr) {
		/*Parser.Expression.atg:361*/AstTypeExpr type; expr = null;
		ArgumentsProxy args; 
		Expect(_new);
		TypeExpr(/*Parser.Expression.atg:364*/out type);
		/*Parser.Expression.atg:364*/_fallbackObjectCreation(this, type, out expr, out args); 
		Arguments(/*Parser.Expression.atg:365*/args);
	}

	void GetInitiator(/*Parser.Statement.atg:148*/out AstGetSet complex, out bool isDeclaration) {
		/*Parser.Statement.atg:148*/complex = null; 
		AstGetSetSymbol symbol = null;
		AstGetSetStatic staticCall = null;
		AstGetSet member = null;
		AstExpr expr;
		List<AstExpr> args = new List<AstExpr>();
		isDeclaration = false;                                            
		string id;
		int placeholderIndex = -1;
		
		if (StartOf(21)) {
			if (/*Parser.Statement.atg:161*/isLikeFunction() || isUnknownId() ) {
				Function(/*Parser.Statement.atg:161*/out complex);
			} else if (StartOf(22)) {
				Variable(/*Parser.Statement.atg:162*/out complex, out isDeclaration);
			} else if (la.kind == _ns || la.kind == _tilde || la.kind == _doublecolon) {
				StaticCall(/*Parser.Statement.atg:163*/out staticCall);
			} else {
				Get();
				Expr(/*Parser.Statement.atg:164*/out expr);
				/*Parser.Statement.atg:164*/args.Add(expr); 
				while (la.kind == _comma) {
					Get();
					Expr(/*Parser.Statement.atg:165*/out expr);
					/*Parser.Statement.atg:165*/args.Add(expr); 
				}
				Expect(_rpar);
				if (la.kind == _dot || la.kind == _lbrack) {
					GetSetExtension(/*Parser.Statement.atg:168*/expr, out member);
					/*Parser.Statement.atg:169*/if(args.Count > 1)
					SemErr("A member access cannot have multiple subjects. (Did you mean '>>'?)");
					
				} else if (la.kind == _appendright) {
					Get();
					GetCall(/*Parser.Statement.atg:173*/out complex);
					/*Parser.Statement.atg:173*/complex.Arguments.RightAppend(args);
					complex.Arguments.ReleaseRightAppend();
					if(complex is AstGetSetSymbol && ((AstGetSetSymbol)complex).IsVariable)
					       complex.Call = PCall.Set;
					member = complex;
					
				} else SynErr(120);
			}
			/*Parser.Statement.atg:181*/complex = 
			(AstGetSet)symbol ?? 
			(AstGetSet)staticCall ?? 
			(AstGetSet)member ??
			complex; 
			
		} else if (/*Parser.Statement.atg:189*/isDeDereference() ) {
			Expect(_pointer);
			Expect(_pointer);
			Id(/*Parser.Statement.atg:189*/out id);
			/*Parser.Statement.atg:189*/SymbolEntry s = target.Symbols[id];
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
			Id(/*Parser.Statement.atg:209*/out id);
			/*Parser.Statement.atg:209*/SymbolEntry s = target.Symbols[id];
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
				Integer(/*Parser.Statement.atg:223*/out placeholderIndex);
			}
			/*Parser.Statement.atg:223*/complex = new AstPlaceholder(this, 0 <= placeholderIndex ? (int?)placeholderIndex : null); 
		} else SynErr(121);
	}

	void Real(/*Parser.Helper.atg:61*/out double value) {
		Expect(_real);
		/*Parser.Helper.atg:70*/string real = t.val;
		if(!TryParseReal(real, out value))
		    SemErr(t, "Cannot recognize real " + real);
		
	}

	void Null() {
		Expect(_null);
	}

	void WhileLoop(/*Parser.Statement.atg:440*/AstBlock block) {
		/*Parser.Statement.atg:440*/AstWhileLoop loop = new AstWhileLoop(this); 
		if (la.kind == _while || la.kind == _until) {
			if (la.kind == _while) {
				Get();
			} else {
				Get();
				/*Parser.Statement.atg:442*/loop.IsPositive = false; 
			}
			Expect(_lpar);
			Expr(/*Parser.Statement.atg:444*/out loop.Condition);
			Expect(_rpar);
			/*Parser.Statement.atg:445*/target.BeginBlock(loop.Block); //EndBlock is common for both loops
			
			StatementBlock(/*Parser.Statement.atg:447*/loop.Block);
		} else if (la.kind == _do) {
			Get();
			/*Parser.Statement.atg:449*/target.BeginBlock(loop.Block); 
			loop.IsPrecondition = false;
			
			StatementBlock(/*Parser.Statement.atg:452*/loop.Block);
			if (la.kind == _while) {
				Get();
			} else if (la.kind == _until) {
				Get();
				/*Parser.Statement.atg:453*/loop.IsPositive = false; 
			} else SynErr(122);
			Expect(_lpar);
			Expr(/*Parser.Statement.atg:455*/out loop.Condition);
			Expect(_rpar);
		} else SynErr(123);
		/*Parser.Statement.atg:456*/target.EndBlock(); block.Add(loop); 
	}

	void ForLoop(/*Parser.Statement.atg:459*/AstBlock block) {
		/*Parser.Statement.atg:459*/AstForLoop loop;
		
		Expect(_for);
		/*Parser.Statement.atg:462*/loop = new AstForLoop(this); target.BeginBlock(loop.Block); 
		Expect(_lpar);
		StatementBlock(/*Parser.Statement.atg:463*/loop.Initialize);
		if (la.kind == _do) {
			Get();
			StatementBlock(/*Parser.Statement.atg:465*/loop.NextIteration);
			/*Parser.Statement.atg:466*/loop.IsPrecondition = false; 
			if (la.kind == _while) {
				Get();
			} else if (la.kind == _until) {
				Get();
				/*Parser.Statement.atg:468*/loop.IsPositive = false; 
			} else SynErr(124);
			Expr(/*Parser.Statement.atg:470*/out loop.Condition);
		} else if (StartOf(14)) {
			if (la.kind == _while || la.kind == _until) {
				if (la.kind == _while) {
					Get();
				} else {
					Get();
					/*Parser.Statement.atg:472*/loop.IsPositive = false; 
				}
			}
			Expr(/*Parser.Statement.atg:474*/out loop.Condition);
			Expect(_semicolon);
			SimpleStatement(/*Parser.Statement.atg:476*/loop.NextIteration);
			if (la.kind == _semicolon) {
				Get();
			}
		} else SynErr(125);
		Expect(_rpar);
		StatementBlock(/*Parser.Statement.atg:480*/loop.Block);
		/*Parser.Statement.atg:480*/target.EndBlock(); block.Add(loop); 
	}

	void ForeachLoop(/*Parser.Statement.atg:484*/AstBlock block) {
		Expect(_foreach);
		/*Parser.Statement.atg:485*/AstForeachLoop loop = new AstForeachLoop(this);
		target.BeginBlock(loop.Block);
		
		Expect(_lpar);
		GetCall(/*Parser.Statement.atg:489*/out loop.Element);
		Expect(_in);
		Expr(/*Parser.Statement.atg:491*/out loop.List);
		Expect(_rpar);
		StatementBlock(/*Parser.Statement.atg:493*/loop.Block);
		/*Parser.Statement.atg:494*/target.EndBlock();
		block.Add(loop); 
		
	}

	void Arguments(/*Parser.Statement.atg:689*/ArgumentsProxy args) {
		/*Parser.Statement.atg:690*/AstExpr expr;
		                          bool missingArg = false;
		                      
		if (la.kind == _lpar) {
			Get();
			if (StartOf(14)) {
				Expr(/*Parser.Statement.atg:696*/out expr);
				/*Parser.Statement.atg:696*/args.Add(expr); 
				while (la.kind == _comma) {
					Get();
					/*Parser.Statement.atg:697*/if(missingArg)
					              SemErr("Missing argument expression (two consecutive commas)");
					      
					if (StartOf(14)) {
						Expr(/*Parser.Statement.atg:700*/out expr);
						/*Parser.Statement.atg:701*/args.Add(expr);
						missingArg = false;
						
					} else if (la.kind == _comma || la.kind == _rpar) {
						/*Parser.Statement.atg:704*/missingArg = true; 
					} else SynErr(126);
				}
			}
			Expect(_rpar);
		}
		/*Parser.Statement.atg:710*/args.RememberRightAppendPosition(); 
		if (la.kind == _appendleft) {
			Get();
			if (/*Parser.Statement.atg:715*/la.kind == _lpar && (!isLambdaExpression())) {
				Expect(_lpar);
				if (StartOf(14)) {
					Expr(/*Parser.Statement.atg:716*/out expr);
					/*Parser.Statement.atg:716*/args.Add(expr); 
					while (la.kind == _comma) {
						Get();
						Expr(/*Parser.Statement.atg:718*/out expr);
						/*Parser.Statement.atg:719*/args.Add(expr); 
					}
				}
				Expect(_rpar);
			} else if (StartOf(14)) {
				Expr(/*Parser.Statement.atg:723*/out expr);
				/*Parser.Statement.atg:723*/args.Add(expr); 
			} else SynErr(127);
		}
	}

	void FormalArg(/*Parser.GlobalScope.atg:647*/CompilerTarget ft) {
		/*Parser.GlobalScope.atg:647*/string id; SymbolInterpretations kind = SymbolInterpretations.LocalObjectVariable; 
		if (la.kind == _var || la.kind == _ref) {
			if (la.kind == _var) {
				Get();
			} else {
				Get();
				/*Parser.GlobalScope.atg:649*/kind = SymbolInterpretations.LocalReferenceVariable; 
			}
		}
		Id(/*Parser.GlobalScope.atg:651*/out id);
		/*Parser.GlobalScope.atg:654*/ft.Function.Parameters.Add(id); 
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
		} else SynErr(128);
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
		} else SynErr(129);
	}

	void PrexoniteTypeExpr(/*Parser.Expression.atg:515*/out AstTypeExpr type) {
		/*Parser.Expression.atg:515*/string id = null; type = null; 
		if (StartOf(4)) {
			Id(/*Parser.Expression.atg:517*/out id);
		} else if (la.kind == _null) {
			Get();
			/*Parser.Expression.atg:517*/id = NullPType.Literal; 
		} else SynErr(130);
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
			Ns(/*Parser.Expression.atg:504*/out id);
			/*Parser.Expression.atg:504*/typeId.Append(id); typeId.Append('.'); 
		} else SynErr(131);
		while (la.kind == _ns) {
			Ns(/*Parser.Expression.atg:506*/out id);
			/*Parser.Expression.atg:506*/typeId.Append(id); typeId.Append('.'); 
		}
		Id(/*Parser.Expression.atg:508*/out id);
		/*Parser.Expression.atg:508*/typeId.Append(id);
		type = new AstConstantTypeExpression(this, 
		    "Object(\"" + StringPType.Escape(typeId.ToString()) + "\")");
		
	}

	void Ns(/*Parser.Helper.atg:35*/out string ns) {
		/*Parser.Helper.atg:35*/ns = "\\NoId\\"; 
		Expect(_ns);
		/*Parser.Helper.atg:37*/ns = cache(t.val); 
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
		} else SynErr(132);
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
				while (!(la.kind == _EOF || la.kind == _semicolon)) {SynErr(133); Get();}
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
		} else SynErr(134);
		if (la.kind == _id || la.kind == _anyId) {
			GlobalId(/*Parser.GlobalScope.atg:119*/out id);
			/*Parser.GlobalScope.atg:119*/primaryAlias = id; 
			if (la.kind == _as) {
				GlobalVariableAliasList(/*Parser.GlobalScope.atg:120*/aliases);
			}
		} else if (la.kind == _as) {
			GlobalVariableAliasList(/*Parser.GlobalScope.atg:121*/aliases);
			/*Parser.GlobalScope.atg:122*/id = Engine.GenerateName("v"); 
		} else SynErr(135);
		/*Parser.GlobalScope.atg:125*/entry = new SymbolEntry(type,id, TargetModule.Name);
		foreach(var alias in aliases)
		    Symbols[alias] = entry;
		   if(!TargetModule.Variables.TryGetVariable(id, out vari))
		{
		    vari = Modular.VariableDeclaration.Create(id);
		       TargetModule.Variables.Add(vari);
		    TargetApplication.Variables[id] = new PVariable(vari);
		}
		
		if (la.kind == _lbrack) {
			Get();
			if (StartOf(30)) {
				MetaAssignment(/*Parser.GlobalScope.atg:136*/vari);
				while (la.kind == _semicolon) {
					Get();
					if (StartOf(30)) {
						MetaAssignment(/*Parser.GlobalScope.atg:138*/vari);
					}
				}
			}
			Expect(_rbrack);
		}
		/*Parser.GlobalScope.atg:142*/if(primaryAlias != null && !_suppressPrimarySymbol(vari))
		      Symbols[primaryAlias] = entry;
		
		if (la.kind == _assign) {
			/*Parser.GlobalScope.atg:145*/_pushLexerState(Lexer.Local); 
			Get();
			/*Parser.GlobalScope.atg:146*/CompilerTarget lastTarget = target;
			  target=FunctionTargets[Application.InitializationId];
			  AstExpr expr;
			
			Expr(/*Parser.GlobalScope.atg:150*/out expr);
			/*Parser.GlobalScope.atg:151*/_popLexerState();
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
		/*Parser.GlobalScope.atg:179*/SymbolInterpretations type = SymbolInterpretations.Undefined;
		ModuleName module = TargetModule.Name;
		
		while (!(la.kind == _EOF || la.kind == _declare)) {SynErr(136); Get();}
		Expect(_declare);
		if (StartOf(31)) {
			if (la.kind == _var) {
				Get();
				/*Parser.GlobalScope.atg:185*/type = SymbolInterpretations.GlobalObjectVariable; 
			} else if (la.kind == _ref) {
				Get();
				/*Parser.GlobalScope.atg:186*/type = SymbolInterpretations.GlobalReferenceVariable; 
			} else if (la.kind == _function) {
				Get();
				/*Parser.GlobalScope.atg:187*/type = SymbolInterpretations.Function; 
			} else {
				Get();
				/*Parser.GlobalScope.atg:188*/type = SymbolInterpretations.Command; 
			}
		}
		DeclarationInstance(/*Parser.GlobalScope.atg:190*/type,module);
		while (la.kind == _comma) {
			Get();
			if (StartOf(4)) {
				DeclarationInstance(/*Parser.GlobalScope.atg:191*/type,module);
			}
		}
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
			} else if (StartOf(32)) {
				MetaExpr(/*Parser.GlobalScope.atg:53*/out entry);
			} else if (la.kind == _rbrack || la.kind == _semicolon) {
				/*Parser.GlobalScope.atg:54*/entry = true; 
			} else SynErr(137);
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
			
		} else SynErr(138);
		/*Parser.GlobalScope.atg:69*/if(entry == null || key == null) 
		                        SemErr("Meta assignment did not generate an entry.");
		                   else 
		                        target.Meta[key] = entry; 
		                
	}

	void GlobalCode() {
		/*Parser.GlobalScope.atg:268*/PFunction func = TargetApplication._InitializationFunction;
		CompilerTarget ft = FunctionTargets[func];
		if(ft == null)
		    throw new PrexoniteException("Internal compilation error: InitializeFunction got lost.");
		
		/*Parser.GlobalScope.atg:275*/target = ft; 
		                             _pushLexerState(Lexer.Local);
		                         
		Expect(_lbrace);
		while (StartOf(20)) {
			Statement(/*Parser.GlobalScope.atg:279*/target.Ast);
		}
		Expect(_rbrace);
		/*Parser.GlobalScope.atg:282*/try {
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
		while (!(la.kind == _EOF || la.kind == _build)) {SynErr(139); Get();}
		Expect(_build);
		/*Parser.GlobalScope.atg:247*/PFunction func = new PFunction(TargetApplication);
		  CompilerTarget lastTarget = target; 
		  CompilerTarget buildBlockTarget = Loader.CreateFunctionTarget(func, new AstBlock(this));
		  target = buildBlockTarget;
		  Loader.DeclareBuildBlockCommands(target);
		  _pushLexerState(Lexer.Local);                                
		
		if (la.kind == _does) {
			Get();
		}
		StatementBlock(/*Parser.GlobalScope.atg:256*/target.Ast);
		/*Parser.GlobalScope.atg:259*/_popLexerState();                                    
		  target = lastTarget;
		  _compileAndExecuteBuildBlock(buildBlockTarget);
		
	}

	void FunctionDefinition(/*Parser.GlobalScope.atg:312*/out PFunction func) {
		/*Parser.GlobalScope.atg:313*/func = null; 
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
			/*Parser.GlobalScope.atg:332*/isLazy = true; 
		} else if (la.kind == _function) {
			Get();
		} else if (la.kind == _coroutine) {
			Get();
			/*Parser.GlobalScope.atg:334*/isCoroutine = true; 
		} else if (la.kind == _macro) {
			Get();
			if (la.kind == _function) {
				Get();
			}
			/*Parser.GlobalScope.atg:335*/isMacro = true; 
		} else SynErr(140);
		if (StartOf(4)) {
			Id(/*Parser.GlobalScope.atg:337*/out id);
			/*Parser.GlobalScope.atg:337*/primaryAlias = id; 
			if (la.kind == _as) {
				FunctionAliasList(/*Parser.GlobalScope.atg:338*/funcAliases);
			}
		} else if (la.kind == _as) {
			FunctionAliasList(/*Parser.GlobalScope.atg:339*/funcAliases);
		} else SynErr(141);
		/*Parser.GlobalScope.atg:341*/funcId = id ?? Engine.GenerateName("f");
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
		      
		      func = new PFunction(TargetApplication, funcId);
		      
		      if(isNested)
		      {
		           func.Meta[PFunction.LogicalIdKey] = localId;
		           if(isLazy)
		              mark_as_let(target.Function,localId);
		      }
		      
		      Loader.CreateFunctionTarget(func, new AstBlock(this));
		      
		      //Add function to application
		      if(TargetApplication.Functions.Contains(func.Id) && !TargetApplication.Meta.GetDefault(Application.AllowOverridingKey,true))
		SemErr(t,"Application " + TargetApplication.Id + " does not allow overriding of function " + func.Id + ".");
		                                TargetApplication.Functions.AddOverride(func);
		                            }
		                            CompilerTarget ft = FunctionTargets[func];
		                            
		                            //Generate derived stub
		                            if(isCoroutine || isLazy)
		                            {
		                                derStub = func;
		                                
		                                //Create derived body function
		                                derId = ft.GenerateLocalId();
		                                derBody = new PFunction(TargetApplication, derId);
		                                Loader.CreateFunctionTarget(derBody, new AstBlock(this));
		                                TargetApplication.Functions.Add(derBody);
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
		                                            ct.ParentTarget = cst;
		                                        }
		                                        
		                                        if(isNested) //Link to parent in case of a nested function
		                                        {
		                                            ft.ParentTarget = target;	                                           
		                                            if(isLazy)
		                                                ft = ct;
		                                        }	                                    
			                                
		if (StartOf(33)) {
			if (la.kind == _lpar) {
				Get();
				if (StartOf(19)) {
					FormalArg(/*Parser.GlobalScope.atg:428*/ft);
					while (la.kind == _comma) {
						Get();
						/*Parser.GlobalScope.atg:429*/if(missingArg)
						       {
						           SemErr("Missing formal argument (two consecutive commas).");
						       } 
						   
						if (StartOf(19)) {
							FormalArg(/*Parser.GlobalScope.atg:434*/ft);
							/*Parser.GlobalScope.atg:434*/missingArg = false; 
						} else if (la.kind == _comma || la.kind == _rpar) {
							/*Parser.GlobalScope.atg:435*/missingArg = true; 
						} else SynErr(142);
					}
				}
				Expect(_rpar);
			} else {
				FormalArg(/*Parser.GlobalScope.atg:440*/ft);
				while (StartOf(34)) {
					if (la.kind == _comma) {
						Get();
					}
					FormalArg(/*Parser.GlobalScope.atg:442*/ft);
				}
			}
		}
		/*Parser.GlobalScope.atg:445*/if(isNested && isLazy)
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
			/*Parser.GlobalScope.atg:471*/_pushLexerState(Lexer.YYINITIAL); 
			Get();
			if (StartOf(30)) {
				MetaAssignment(/*Parser.GlobalScope.atg:473*/func);
				while (la.kind == _semicolon) {
					Get();
					if (StartOf(30)) {
						MetaAssignment(/*Parser.GlobalScope.atg:475*/func);
					}
				}
			}
			/*Parser.GlobalScope.atg:478*/_popLexerState(); 
			Expect(_rbrack);
		}
		/*Parser.GlobalScope.atg:483*/if(primaryAlias != null && !_suppressPrimarySymbol(func))
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
		                                    
		if (StartOf(35)) {
			if (la.kind == _does) {
				Get();
			}
			StatementBlock(/*Parser.GlobalScope.atg:514*/target.Ast);
		} else if (/*Parser.GlobalScope.atg:516*/isFollowedByStatementBlock()) {
			Expect(_implementation);
			StatementBlock(/*Parser.GlobalScope.atg:517*/target.Ast);
		} else if (la.kind == _assign || la.kind == _implementation) {
			if (la.kind == _assign) {
				Get();
			} else {
				Get();
			}
			/*Parser.GlobalScope.atg:518*/AstReturn ret = new AstReturn(this, ReturnVariant.Exit); 
			Expr(/*Parser.GlobalScope.atg:519*/out ret.Expression);
			/*Parser.GlobalScope.atg:519*/target.Ast.Add(ret); 
			Expect(_semicolon);
		} else SynErr(143);
		/*Parser.GlobalScope.atg:521*/_popLexerState();
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

	void GlobalId(/*Parser.GlobalScope.atg:659*/out string id) {
		/*Parser.GlobalScope.atg:659*/id = "...no freaking id..."; 
		if (la.kind == _id) {
			Get();
			/*Parser.GlobalScope.atg:661*/id = cache(t.val); 
		} else if (la.kind == _anyId) {
			Get();
			/*Parser.GlobalScope.atg:662*/id = cache(t.val.Substring(1)); 
		} else SynErr(144);
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
		case _real: {
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
			
			if (StartOf(32)) {
				MetaExpr(/*Parser.GlobalScope.atg:88*/out subEntry);
				/*Parser.GlobalScope.atg:88*/lst.Add(subEntry); 
				while (la.kind == _comma) {
					Get();
					/*Parser.GlobalScope.atg:89*/if(lastWasEmpty)
					    SemErr("Missing meta expression in list (two consecutive commas).");
					
					if (StartOf(32)) {
						MetaExpr(/*Parser.GlobalScope.atg:92*/out subEntry);
						/*Parser.GlobalScope.atg:93*/lst.Add(subEntry); 
						lastWasEmpty = false;
						
					} else if (la.kind == _comma || la.kind == _rbrace) {
						/*Parser.GlobalScope.atg:96*/lastWasEmpty = true; 
					} else SynErr(145);
				}
			}
			Expect(_rbrace);
			/*Parser.GlobalScope.atg:100*/entry = (MetaEntry) lst.ToArray(); 
			break;
		}
		default: SynErr(146); break;
		}
	}

	void GlobalQualifiedId(/*Parser.GlobalScope.atg:665*/out string id) {
		/*Parser.GlobalScope.atg:665*/id = "\\NoId\\"; 
		if (la.kind == _id || la.kind == _anyId) {
			GlobalId(/*Parser.GlobalScope.atg:667*/out id);
		} else if (la.kind == _ns) {
			Ns(/*Parser.GlobalScope.atg:668*/out id);
			/*Parser.GlobalScope.atg:668*/StringBuilder buffer = new StringBuilder(id); buffer.Append('.'); 
			while (la.kind == _ns) {
				Ns(/*Parser.GlobalScope.atg:669*/out id);
				/*Parser.GlobalScope.atg:669*/buffer.Append(id); buffer.Append('.'); 
			}
			GlobalId(/*Parser.GlobalScope.atg:671*/out id);
			/*Parser.GlobalScope.atg:671*/buffer.Append(id); 
			/*Parser.GlobalScope.atg:672*/id = buffer.ToString(); 
		} else SynErr(147);
	}

	void GlobalVariableAliasList(/*Parser.GlobalScope.atg:167*/IList<string> aliases ) {
		/*Parser.GlobalScope.atg:167*/string id = "\\NoId_In_GlobalVariableAliasList_\\"; 
		Expect(_as);
		GlobalId(/*Parser.GlobalScope.atg:169*/out id);
		/*Parser.GlobalScope.atg:169*/aliases.Add(id); 
		while (la.kind == _comma) {
			Get();
			if (la.kind == _id || la.kind == _anyId) {
				GlobalId(/*Parser.GlobalScope.atg:171*/out id);
				/*Parser.GlobalScope.atg:171*/aliases.Add(id); 
			}
		}
	}

	void DeclarationInstance(/*Parser.GlobalScope.atg:195*/SymbolInterpretations type, ModuleName module) {
		/*Parser.GlobalScope.atg:195*/string id; string aId; 
		Id(/*Parser.GlobalScope.atg:197*/out id);
		/*Parser.GlobalScope.atg:197*/aId = id; 
		if (la.kind == _as) {
			Get();
			Id(/*Parser.GlobalScope.atg:198*/out aId);
		}
		/*Parser.GlobalScope.atg:199*/SymbolEntry inferredType;
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

	void StatementBlock(/*Parser.Statement.atg:26*/AstBlock block) {
		Statement(/*Parser.Statement.atg:27*/block);
	}

	void FunctionAliasList(/*Parser.GlobalScope.atg:302*/IList<string> aliases ) {
		/*Parser.GlobalScope.atg:302*/String id; 
		Expect(_as);
		Id(/*Parser.GlobalScope.atg:304*/out id);
		/*Parser.GlobalScope.atg:304*/aliases.Add(id); 
		while (la.kind == _comma) {
			Get();
			if (StartOf(4)) {
				Id(/*Parser.GlobalScope.atg:306*/out id);
				/*Parser.GlobalScope.atg:306*/aliases.Add(id); 
			}
		}
	}

	void ExplicitLabel(/*Parser.Statement.atg:375*/AstBlock block) {
		/*Parser.Statement.atg:375*/string id = "--\\NotAnId\\--"; 
		if (StartOf(4)) {
			Id(/*Parser.Statement.atg:377*/out id);
			Expect(_colon);
		} else if (la.kind == _lid) {
			Get();
			/*Parser.Statement.atg:378*/id = cache(t.val.Substring(0,t.val.Length-1)); 
		} else SynErr(148);
		/*Parser.Statement.atg:379*/block.Statements.Add(new AstExplicitLabel(this, id)); 
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
		} else if (StartOf(36)) {
			Return(/*Parser.Statement.atg:49*/block);
		} else if (la.kind == _throw) {
			Throw(/*Parser.Statement.atg:50*/block);
		} else if (la.kind == _let) {
			LetBindingStmt(/*Parser.Statement.atg:51*/block);
		} else SynErr(149);
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
		default: SynErr(150); break;
		}
	}

	void ExplicitGoTo(/*Parser.Statement.atg:382*/AstBlock block) {
		/*Parser.Statement.atg:382*/string id; 
		Expect(_goto);
		Id(/*Parser.Statement.atg:385*/out id);
		/*Parser.Statement.atg:385*/block.Statements.Add(new AstExplicitGoTo(this, id)); 
	}

	void VariableDeclarationStatement(/*Parser.Statement.atg:305*/AstBlock block) {
		/*Parser.Statement.atg:305*/AstGetSet variable;
		bool isNewDecl = false;
		
		if (la.kind == _new) {
			Get();
			/*Parser.Statement.atg:308*/isNewDecl = true; 
		}
		VariableDeclaration(/*Parser.Statement.atg:310*/out variable, isNewDecl);
		/*Parser.Statement.atg:311*/if(isNewDecl)
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
		} else if (StartOf(37)) {
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
		} else SynErr(151);
	}

	void Return(/*Parser.Statement.atg:522*/AstBlock block) {
		/*Parser.Statement.atg:522*/AstReturn ret = null; 
		AstExplicitGoTo jump = null; 
		AstExpr expr = null; 
		AstLoopBlock bl = target.CurrentLoopBlock;
		
		if (la.kind == _return || la.kind == _yield) {
			if (la.kind == _return) {
				Get();
				/*Parser.Statement.atg:530*/ret = new AstReturn(this, ReturnVariant.Exit); 
			} else {
				Get();
				/*Parser.Statement.atg:531*/ret = new AstReturn(this, ReturnVariant.Continue); 
			}
			if (StartOf(38)) {
				if (StartOf(14)) {
					Expr(/*Parser.Statement.atg:533*/out expr);
					/*Parser.Statement.atg:533*/ret.Expression = expr; 
				} else {
					Get();
					/*Parser.Statement.atg:534*/ret.ReturnVariant = ReturnVariant.Set; 
					Expr(/*Parser.Statement.atg:535*/out expr);
					/*Parser.Statement.atg:535*/ret.Expression = expr; 
					/*Parser.Statement.atg:536*/SemErr("Return value assignment is no longer supported. You must use local variables instead."); 
				}
			}
		} else if (la.kind == _break) {
			Get();
			/*Parser.Statement.atg:538*/if(bl == null)
			   ret = new AstReturn(this, ReturnVariant.Break); 
			else
			    jump = new AstExplicitGoTo(this, bl.BreakLabel);
			
		} else if (la.kind == _continue) {
			Get();
			/*Parser.Statement.atg:543*/if(bl == null)
			   ret = new AstReturn(this, ReturnVariant.Continue); 
			else
			    jump = new AstExplicitGoTo(this, bl.ContinueLabel);
			
		} else SynErr(152);
		/*Parser.Statement.atg:548*/block.Add((AstNode)ret ?? (AstNode)jump); 
	}

	void Throw(/*Parser.Statement.atg:671*/AstBlock block) {
		/*Parser.Statement.atg:671*/AstThrow th; 
		ThrowExpression(/*Parser.Statement.atg:673*/out th);
		/*Parser.Statement.atg:674*/block.Add(th); 
	}

	void LetBindingStmt(/*Parser.Statement.atg:590*/AstBlock block) {
		Expect(_let);
		LetBinder(/*Parser.Statement.atg:591*/block);
		while (la.kind == _comma) {
			Get();
			LetBinder(/*Parser.Statement.atg:591*/block);
		}
	}

	void Condition(/*Parser.Statement.atg:417*/AstBlock block) {
		/*Parser.Statement.atg:417*/AstExpr expr = null; bool isNegative = false; 
		if (la.kind == _if) {
			Get();
			/*Parser.Statement.atg:419*/isNegative = false; 
		} else if (la.kind == _unless) {
			Get();
			/*Parser.Statement.atg:420*/isNegative = true; 
		} else SynErr(153);
		Expect(_lpar);
		Expr(/*Parser.Statement.atg:423*/out expr);
		Expect(_rpar);
		/*Parser.Statement.atg:423*/if(expr == null)
		   expr = _createUnknownExpr();
		AstCondition cond = new AstCondition(this, expr, isNegative);
		target.BeginBlock(cond.IfBlock);
		
		StatementBlock(/*Parser.Statement.atg:429*/cond.IfBlock);
		/*Parser.Statement.atg:430*/target.EndBlock(); 
		if (la.kind == _else) {
			Get();
			/*Parser.Statement.atg:433*/target.BeginBlock(cond.ElseBlock); 
			StatementBlock(/*Parser.Statement.atg:434*/cond.ElseBlock);
			/*Parser.Statement.atg:435*/target.EndBlock(); 
		}
		/*Parser.Statement.atg:436*/block.Add(cond); 
	}

	void NestedFunction(/*Parser.Statement.atg:552*/AstBlock block) {
		/*Parser.Statement.atg:552*/PFunction func; 
		FunctionDefinition(/*Parser.Statement.atg:554*/out func);
		/*Parser.Statement.atg:556*/string logicalId = func.Meta[PFunction.LogicalIdKey];
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

	void TryCatchFinally(/*Parser.Statement.atg:618*/AstBlock block) {
		/*Parser.Statement.atg:618*/AstTryCatchFinally a = new AstTryCatchFinally(this);
		AstGetSet excVar = null;
		
		Expect(_try);
		/*Parser.Statement.atg:622*/target.BeginBlock(a.TryBlock); 
		Expect(_lbrace);
		while (StartOf(20)) {
			Statement(/*Parser.Statement.atg:624*/a.TryBlock);
		}
		Expect(_rbrace);
		/*Parser.Statement.atg:626*/target.EndBlock(); 
		if (la.kind == _catch || la.kind == _finally) {
			if (la.kind == _catch) {
				Get();
				/*Parser.Statement.atg:627*/target.BeginBlock(a.CatchBlock); 
				if (la.kind == _lpar) {
					Get();
					GetCall(/*Parser.Statement.atg:629*/out excVar);
					/*Parser.Statement.atg:629*/a.ExceptionVar = excVar; 
					Expect(_rpar);
				} else if (la.kind == _lbrace) {
					/*Parser.Statement.atg:631*/SemErr(la,"catch-clauses that don't store the exception are illegal."); 
				} else SynErr(154);
				Expect(_lbrace);
				while (StartOf(20)) {
					Statement(/*Parser.Statement.atg:634*/a.CatchBlock);
				}
				Expect(_rbrace);
				/*Parser.Statement.atg:636*/target.EndBlock(); 
				if (la.kind == _finally) {
					Get();
					/*Parser.Statement.atg:639*/target.BeginBlock(a.FinallyBlock); 
					Expect(_lbrace);
					while (StartOf(20)) {
						Statement(/*Parser.Statement.atg:641*/a.FinallyBlock);
					}
					Expect(_rbrace);
					/*Parser.Statement.atg:643*/target.EndBlock(); 
				}
			} else {
				Get();
				/*Parser.Statement.atg:646*/target.BeginBlock(a.FinallyBlock); 
				Expect(_lbrace);
				while (StartOf(20)) {
					Statement(/*Parser.Statement.atg:648*/a.FinallyBlock);
				}
				Expect(_rbrace);
				/*Parser.Statement.atg:650*/target.EndBlock(); 
				if (la.kind == _catch) {
					/*Parser.Statement.atg:652*/target.BeginBlock(a.CatchBlock); 
					Get();
					if (la.kind == _lpar) {
						Get();
						GetCall(/*Parser.Statement.atg:655*/out excVar);
						/*Parser.Statement.atg:656*/a.ExceptionVar = excVar; 
						Expect(_rpar);
					} else if (la.kind == _lbrace) {
						/*Parser.Statement.atg:658*/SemErr(la,"catch-clauses that don't store the exception are illegal."); 
					} else SynErr(155);
					Expect(_lbrace);
					while (StartOf(20)) {
						Statement(/*Parser.Statement.atg:661*/a.CatchBlock);
					}
					Expect(_rbrace);
					/*Parser.Statement.atg:664*/target.EndBlock(); 
				}
			}
		}
		/*Parser.Statement.atg:667*/block.Add(a); 
	}

	void Using(/*Parser.Statement.atg:678*/AstBlock block) {
		/*Parser.Statement.atg:678*/AstUsing use = new AstUsing(this); 
		Expect(_uusing);
		Expect(_lpar);
		Expr(/*Parser.Statement.atg:680*/out use.Expression);
		Expect(_rpar);
		/*Parser.Statement.atg:681*/target.BeginBlock(use.Block); 
		StatementBlock(/*Parser.Statement.atg:682*/use.Block);
		/*Parser.Statement.atg:683*/target.EndBlock();
		block.Add(use); 
		
	}

	void Assignment(/*Parser.Statement.atg:389*/AstGetSet lvalue, out AstNode node) {
		/*Parser.Statement.atg:389*/AstExpr expr = null;
		BinaryOperator setModifier = BinaryOperator.None;
		AstTypeExpr T;
		node = lvalue;
		
		if (StartOf(9)) {
			switch (la.kind) {
			case _assign: {
				Get();
				/*Parser.Statement.atg:396*/setModifier = BinaryOperator.None; 
				break;
			}
			case _plus: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:397*/setModifier = BinaryOperator.Addition; 
				break;
			}
			case _minus: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:398*/setModifier = BinaryOperator.Subtraction; 
				break;
			}
			case _times: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:399*/setModifier = BinaryOperator.Multiply; 
				break;
			}
			case _div: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:400*/setModifier = BinaryOperator.Division; 
				break;
			}
			case _bitAnd: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:401*/setModifier = BinaryOperator.BitwiseAnd; 
				break;
			}
			case _bitOr: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:402*/setModifier = BinaryOperator.BitwiseOr; 
				break;
			}
			case _coalescence: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:403*/setModifier = BinaryOperator.Coalescence; 
				break;
			}
			}
			Expr(/*Parser.Statement.atg:404*/out expr);
		} else if (la.kind == _tilde) {
			Get();
			Expect(_assign);
			/*Parser.Statement.atg:406*/setModifier = BinaryOperator.Cast; 
			TypeExpr(/*Parser.Statement.atg:407*/out T);
			/*Parser.Statement.atg:407*/expr = T; 
		} else SynErr(156);
		/*Parser.Statement.atg:409*/lvalue.Arguments.Add(expr);
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
		if(actualComplex is AstGetSetSymbol && ((AstGetSetSymbol)actualComplex).IsVariable)
		       actualComplex.Call = PCall.Set;
		   complex = actualComplex;
		
	}

	void Function(/*Parser.Statement.atg:324*/out AstGetSet function) {
		/*Parser.Statement.atg:324*/function = null; string id; 
		Id(/*Parser.Statement.atg:326*/out id);
		/*Parser.Statement.atg:326*/if(!target.Symbols.ContainsKey(id))
		{
		    function = new AstUnresolved(this, id);
		}
		else
		{
		    if(isOuterVariable(id))
		        target.RequireOuterVariable(id);
		    SymbolEntry sym = target.Symbols[id];
		    if(isKnownMacroFunction(sym) || sym.Interpretation == SymbolInterpretations.MacroCommand) 
		    {
		        function = new AstMacroInvocation(this, sym);
		    } 
		    else
		    {
		        function = new AstGetSetSymbol(this, sym);
		    }
		}
		
		Arguments(/*Parser.Statement.atg:345*/function.Arguments);
	}

	void Variable(/*Parser.Statement.atg:266*/out AstGetSet complex, out bool isDeclared) {
		/*Parser.Statement.atg:266*/string id; 
		isDeclared = false; 
		complex = null; 
		bool isNewDecl = false;
		
		if (StartOf(39)) {
			if (la.kind == _new) {
				Get();
				/*Parser.Statement.atg:272*/isNewDecl = true; 
			}
			VariableDeclaration(/*Parser.Statement.atg:274*/out complex, isNewDecl);
			/*Parser.Statement.atg:274*/isDeclared = true; 
		} else if (StartOf(4)) {
			Id(/*Parser.Statement.atg:275*/out id);
			/*Parser.Statement.atg:276*/SymbolEntry varSym;
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
			
		} else SynErr(157);
	}

	void StaticCall(/*Parser.Statement.atg:349*/out AstGetSetStatic staticCall) {
		/*Parser.Statement.atg:349*/AstTypeExpr typeExpr;
		string memberId;
		staticCall = null;
		
		ExplicitTypeExpr(/*Parser.Statement.atg:354*/out typeExpr);
		Expect(_dot);
		Id(/*Parser.Statement.atg:355*/out memberId);
		/*Parser.Statement.atg:355*/staticCall = new AstGetSetStatic(this, PCall.Get, typeExpr, memberId); 
		Arguments(/*Parser.Statement.atg:356*/staticCall.Arguments);
	}

	void VariableDeclaration(/*Parser.Statement.atg:227*/out AstGetSet variable, bool isNewDecl) {
		/*Parser.Statement.atg:227*/variable = null; 
		string staticId = null; 
		string id = null;
		bool isOverrideDecl = false;
		
		/*Parser.Statement.atg:232*/SymbolInterpretations kind = SymbolInterpretations.Undefined; 
		if (la.kind == _var || la.kind == _ref) {
			if (la.kind == _var) {
				Get();
				/*Parser.Statement.atg:233*/kind = SymbolInterpretations.LocalObjectVariable; 
			} else {
				Get();
				/*Parser.Statement.atg:234*/kind = SymbolInterpretations.LocalReferenceVariable; 
			}
			if (la.kind == _new) {
				Get();
				/*Parser.Statement.atg:236*/isOverrideDecl = true; 
			}
			Id(/*Parser.Statement.atg:238*/out id);
			/*Parser.Statement.atg:239*/SmartDeclareLocal(id, kind, isOverrideDecl);
			staticId = id; 
			
		} else if (la.kind == _static) {
			Get();
			/*Parser.Statement.atg:242*/kind = SymbolInterpretations.GlobalObjectVariable; 
			if (la.kind == _var || la.kind == _ref) {
				if (la.kind == _var) {
					Get();
				} else {
					Get();
					/*Parser.Statement.atg:244*/kind = SymbolInterpretations.GlobalReferenceVariable; 
				}
			}
			Id(/*Parser.Statement.atg:246*/out id);
			/*Parser.Statement.atg:246*/staticId = target.Function.Id + "\\static\\" + id;
			target.DeclareModuleLocal(kind, id, staticId);
			if(!target.Loader.Options.TargetApplication.Variables.ContainsKey(staticId))
			    target.Loader.Options.TargetApplication.Variables.Add(staticId, new PVariable(staticId));
			
		} else SynErr(158);
		/*Parser.Statement.atg:251*/variable = InterpretationIsObjectVariable(kind) ?
		new AstGetSetSymbol(this, PCall.Get, target.Symbols[id])
		:
			new AstGetSetReference(this, PCall.Get, target.Symbols[id].With(interpretation: target.Symbols[id].Interpretation.ToObjectVariable())); 
		                                     
		                                 if(isNewDecl)
		                                     variable = new AstGetSetNewDecl(this)
		                                     {
		                                         Expression = variable,
		                                         Id = staticId
		                                     };
		                             
	}

	void LetBinder(/*Parser.Statement.atg:595*/AstBlock block) {
		/*Parser.Statement.atg:595*/string id = null;
		AstExpr thunk;
		
		Id(/*Parser.Statement.atg:599*/out id);
		/*Parser.Statement.atg:600*/SmartDeclareLocal(id, SymbolInterpretations.LocalObjectVariable);
		mark_as_let(target.Function, id);
		if(la.kind == _assign)
		    _inject(_lazy,"lazy"); 
		
		if (la.kind == _assign) {
			Get();
			LazyExpression(/*Parser.Statement.atg:606*/out thunk);
			/*Parser.Statement.atg:609*/var assign = new AstGetSetSymbol(this, PCall.Set, 
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

		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,T, T,x,x,x, T,T,T,T, T,T,x,T, x,x,T,T, T,T,T,T, T,x,T,T, T,x,T,x, T,T,T,T, T,x,x,x, x,T,T,T, T,T,x,x, T,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,T, T,T,x,T, x,x,T,T, T,T,T,T, T,x,T,T, T,x,T,x, T,T,T,T, T,x,x,x, x,T,T,T, T,T,x,x, T,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,T,x, x,T,x,x, x,x,x,x, T,x,x,T, T,x,x,x, T,T,T,T, T,T,x,T, x,x,T,T, T,T,T,T, T,x,T,T, T,x,T,x, T,T,T,T, T,x,x,x, x,T,T,T, T,T,x,x, T,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,T, x,x,x,T, x,x,x,T, T,T,x,T, T,x,T,T, T,x,T,x, T,T,T,T, T,x,x,x, x,T,T,T, T,T,x,x, T,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, T,T,T,T, x,x,x,T, x,x,x,x, x,T,T,T, T,x,x,T, x,x,T,x, x,x,x,T, x,x,x,T, x,T,T,x, x,x,x,T, T,T,T,x, x,x,x,T, T,T,x,x, T,x,T,x, x,x,T,x, x,x,x,x, x,x,x,T, T,x,x,x, T,T,x,T, x,T,T,T, T,x,x,x, T,x,x,x, T,x,x,T, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, T,T,x,x, T,x,x,x, x,x,x,x, x,x,x,T, x,T,T,x, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, T,T,T,T, T,T,T,T, T,T,x,T, x,T,T,T, T,T,T,T, T,T,T,T, T,T,T,x, T,x,x,x, x,x,T,x, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,T,T, T,x,x,x, x,x,x,T, x,x,x,x, x,x,x,T, x,x,x,x, x,T,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,T, x,T,x,x, x,T,x,T, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,x,T,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,T,x,x, x,x,x,T, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, T,T,T,T, x,x,x,T, x,x,x,x, x,T,T,T, T,x,x,T, x,x,T,x, x,x,x,T, x,x,x,T, x,T,T,x, x,x,x,T, T,T,T,x, x,x,x,T, T,T,x,x, T,x,T,x, x,x,T,x, x,x,x,x, x,x,x,T, T,T,T,x, T,T,x,T, x,T,T,T, T,x,x,x, T,x,x,x, T,x,x,T, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,T, x,x,x,T, x,T,T,x, x,x,x,T, T,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,x,T,x, x,x,x,x, x,x,x,T, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, T,x,x,x, x,x,x,x, x,x,x,T, x,T,x,T, x,T,T,x, x,x,x,T, T,x,x,x, x,x,x,x, T,T,T,x, T,T,T,T, x,x,T,T, T,T,x,x, x,x,T,T, x,T,T,x, T,T,x,T, x,T,T,T, T,T,x,x, T,x,T,T, T,T,x,x, x,x},
		{x,T,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,T, x,x,x,T, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,x,T,x, x,x,x,x, x,x,x,T, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,x,T,x, x,x,x,x, x,x,x,T, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,T, x,T,x,T, x,T,T,x, x,x,x,T, T,x,x,x, x,x,x,x, T,T,x,x, x,T,T,T, x,x,T,T, T,T,x,x, x,x,T,T, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,T,x,x, x,x},
		{x,T,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,T, x,x,x,T, x,T,T,x, x,x,x,T, T,x,x,x, x,x,x,x, T,T,x,x, x,T,T,T, x,x,T,T, T,T,x,x, x,x,T,T, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,T,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,T,x,T, x,T,T,T, T,T,x,x, x,x,T,T, T,x,x,x, x,x},
		{x,x,x,x, T,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,T, x,x,x,T, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,T,x,T, x,x,T,x, x,T,T,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,T,x,T, x,x,x,x, x,T,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,T,x,T, x,x,x,x, x,T,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,T, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, T,T,T,T, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, T,x,x,x, x,x,x,x, x,x,x,T, x,T,x,T, x,T,T,x, x,x,x,T, T,x,x,x, x,x,x,x, T,T,T,x, T,T,T,T, x,x,T,T, T,T,x,x, x,x,T,T, x,T,T,x, T,T,x,T, T,T,T,T, T,T,x,x, T,x,T,T, T,T,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, T,T,x,x, T,x,x,x, x,x,x,x, x,x,x,T, x,T,T,x, x,x,x,T, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, T,T,T,T, x,T,x,T, x,x,x,x, x,T,T,T, T,x,x,T, x,x,T,x, x,x,x,T, x,x,x,T, x,T,T,x, x,x,x,T, T,T,T,x, x,x,x,T, T,T,x,x, T,x,T,x, x,x,T,x, x,x,x,x, x,x,x,T, T,T,T,x, T,T,x,T, x,T,T,T, T,x,x,x, T,x,x,x, T,x,x,T, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x}

#line 133 "D:\DotNetProjects\Prexonite-Hg\prx-assembla-hg\Tools\Parser.frame" //FRAME

	};
} // end Parser

public enum ParseMessageSeverity
{
    Error,
    Warning,
    Info
}

public partial class ParseMessage : Prexonite.Compiler.ISourcePosition {
    private const string errMsgFormat = "-- ({3}) line {0} col {1}: {2}"; // 0=line, 1=column, 2=text, 3=file
    private readonly string _message;
    public string Message { get { return _message; } }
    private readonly string _file;
    private readonly int _line;
    private readonly int _column;
    public string File { get { return _file; } }
    public int Line { get { return _line; } }
    public int Column { get { return _column; } }
    private readonly ParseMessageSeverity _severity;
    public ParseMessageSeverity Severity { get { return _severity; } }

    public ParseMessage(ParseMessageSeverity severity, string message, string file, int line, int column) 
    {
        if(message == null)
            throw new ArgumentNullException();
        _message = message;
        _file = file;
        _line = line;
        _column = column;
        _severity = severity;
    }

    public static ParseMessage Error(string message, string file, int line, int column) 
    {
        return new ParseMessage(ParseMessageSeverity.Error, message, file, line, column);
    }

    public static ParseMessage Warning(string message, string file, int line, int column) 
    {
        return new ParseMessage(ParseMessageSeverity.Warning, message, file, line, column);
    }

    public static ParseMessage Info(string message, string file, int line, int column) 
    {
        return new ParseMessage(ParseMessageSeverity.Info, message, file, line, column);
    }

    public ParseMessage(ParseMessageSeverity severity, string message, Prexonite.Compiler.ISourcePosition position)
        : this(severity, message, position.File, position.Line, position.Column)
    {
    }

    public override string ToString() 
    {
        return String.Format(errMsgFormat,Line, Column, Message, File);
    }
}

internal class ParseMessageEventArgs : EventArgs
{
    private readonly ParseMessage _message;
    public ParseMessage Message { get { return _message; } }
    public ParseMessageEventArgs(ParseMessage message)
    {
        if(message == null)
            throw new ArgumentNullException("message");
        _message = message;
    }
}

[System.Diagnostics.DebuggerStepThrough()]
internal class Errors : System.Collections.Generic.LinkedList<ParseMessage> {
    internal Parser parentParser;
  
    internal event EventHandler<ParseMessageEventArgs> MessageReceived;
    protected void OnMessageReceived(ParseMessage message)
    {
        var handler = MessageReceived;
        if(handler != null)
            handler(this, new ParseMessageEventArgs(message));
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
			case 5: s = "integer expected"; break;
			case 6: s = "real expected"; break;
			case 7: s = "string expected"; break;
			case 8: s = "bitAnd expected"; break;
			case 9: s = "assign expected"; break;
			case 10: s = "comma expected"; break;
			case 11: s = "dec expected"; break;
			case 12: s = "div expected"; break;
			case 13: s = "dot expected"; break;
			case 14: s = "eq expected"; break;
			case 15: s = "gt expected"; break;
			case 16: s = "ge expected"; break;
			case 17: s = "inc expected"; break;
			case 18: s = "lbrace expected"; break;
			case 19: s = "lbrack expected"; break;
			case 20: s = "lpar expected"; break;
			case 21: s = "lt expected"; break;
			case 22: s = "le expected"; break;
			case 23: s = "minus expected"; break;
			case 24: s = "ne expected"; break;
			case 25: s = "bitOr expected"; break;
			case 26: s = "plus expected"; break;
			case 27: s = "pow expected"; break;
			case 28: s = "rbrace expected"; break;
			case 29: s = "rbrack expected"; break;
			case 30: s = "rpar expected"; break;
			case 31: s = "tilde expected"; break;
			case 32: s = "times expected"; break;
			case 33: s = "semicolon expected"; break;
			case 34: s = "colon expected"; break;
			case 35: s = "doublecolon expected"; break;
			case 36: s = "coalescence expected"; break;
			case 37: s = "question expected"; break;
			case 38: s = "pointer expected"; break;
			case 39: s = "implementation expected"; break;
			case 40: s = "at expected"; break;
			case 41: s = "appendleft expected"; break;
			case 42: s = "appendright expected"; break;
			case 43: s = "var expected"; break;
			case 44: s = "ref expected"; break;
			case 45: s = "true expected"; break;
			case 46: s = "false expected"; break;
			case 47: s = "BEGINKEYWORDS expected"; break;
			case 48: s = "mod expected"; break;
			case 49: s = "is expected"; break;
			case 50: s = "as expected"; break;
			case 51: s = "not expected"; break;
			case 52: s = "enabled expected"; break;
			case 53: s = "disabled expected"; break;
			case 54: s = "function expected"; break;
			case 55: s = "command expected"; break;
			case 56: s = "asm expected"; break;
			case 57: s = "declare expected"; break;
			case 58: s = "build expected"; break;
			case 59: s = "return expected"; break;
			case 60: s = "in expected"; break;
			case 61: s = "to expected"; break;
			case 62: s = "add expected"; break;
			case 63: s = "continue expected"; break;
			case 64: s = "break expected"; break;
			case 65: s = "yield expected"; break;
			case 66: s = "or expected"; break;
			case 67: s = "and expected"; break;
			case 68: s = "xor expected"; break;
			case 69: s = "label expected"; break;
			case 70: s = "goto expected"; break;
			case 71: s = "static expected"; break;
			case 72: s = "null expected"; break;
			case 73: s = "if expected"; break;
			case 74: s = "unless expected"; break;
			case 75: s = "else expected"; break;
			case 76: s = "new expected"; break;
			case 77: s = "coroutine expected"; break;
			case 78: s = "from expected"; break;
			case 79: s = "do expected"; break;
			case 80: s = "does expected"; break;
			case 81: s = "while expected"; break;
			case 82: s = "until expected"; break;
			case 83: s = "for expected"; break;
			case 84: s = "foreach expected"; break;
			case 85: s = "try expected"; break;
			case 86: s = "catch expected"; break;
			case 87: s = "finally expected"; break;
			case 88: s = "throw expected"; break;
			case 89: s = "then expected"; break;
			case 90: s = "uusing expected"; break;
			case 91: s = "macro expected"; break;
			case 92: s = "lazy expected"; break;
			case 93: s = "let expected"; break;
			case 94: s = "ENDKEYWORDS expected"; break;
			case 95: s = "LPopExpr expected"; break;
			case 96: s = "??? expected"; break;
			case 97: s = "invalid AsmStatementBlock"; break;
			case 98: s = "invalid AsmInstruction"; break;
			case 99: s = "invalid AsmInstruction"; break;
			case 100: s = "invalid AsmInstruction"; break;
			case 101: s = "invalid AsmInstruction"; break;
			case 102: s = "invalid AsmInstruction"; break;
			case 103: s = "invalid AsmId"; break;
			case 104: s = "invalid SignedReal"; break;
			case 105: s = "invalid Boolean"; break;
			case 106: s = "invalid Id"; break;
			case 107: s = "invalid Expr"; break;
			case 108: s = "invalid AssignExpr"; break;
			case 109: s = "invalid AssignExpr"; break;
			case 110: s = "invalid TypeExpr"; break;
			case 111: s = "invalid GetSetExtension"; break;
			case 112: s = "invalid Primary"; break;
			case 113: s = "invalid Constant"; break;
			case 114: s = "invalid ListLiteral"; break;
			case 115: s = "invalid HashLiteral"; break;
			case 116: s = "invalid LoopExpr"; break;
			case 117: s = "invalid LambdaExpression"; break;
			case 118: s = "invalid LambdaExpression"; break;
			case 119: s = "invalid LazyExpression"; break;
			case 120: s = "invalid GetInitiator"; break;
			case 121: s = "invalid GetInitiator"; break;
			case 122: s = "invalid WhileLoop"; break;
			case 123: s = "invalid WhileLoop"; break;
			case 124: s = "invalid ForLoop"; break;
			case 125: s = "invalid ForLoop"; break;
			case 126: s = "invalid Arguments"; break;
			case 127: s = "invalid Arguments"; break;
			case 128: s = "invalid Statement"; break;
			case 129: s = "invalid ExplicitTypeExpr"; break;
			case 130: s = "invalid PrexoniteTypeExpr"; break;
			case 131: s = "invalid ClrTypeExpr"; break;
			case 132: s = "invalid TypeExprElement"; break;
			case 133: s = "this symbol not expected in Prexonite"; break;
			case 134: s = "invalid GlobalVariableDefinition"; break;
			case 135: s = "invalid GlobalVariableDefinition"; break;
			case 136: s = "this symbol not expected in Declaration"; break;
			case 137: s = "invalid MetaAssignment"; break;
			case 138: s = "invalid MetaAssignment"; break;
			case 139: s = "this symbol not expected in BuildBlock"; break;
			case 140: s = "invalid FunctionDefinition"; break;
			case 141: s = "invalid FunctionDefinition"; break;
			case 142: s = "invalid FunctionDefinition"; break;
			case 143: s = "invalid FunctionDefinition"; break;
			case 144: s = "invalid GlobalId"; break;
			case 145: s = "invalid MetaExpr"; break;
			case 146: s = "invalid MetaExpr"; break;
			case 147: s = "invalid GlobalQualifiedId"; break;
			case 148: s = "invalid ExplicitLabel"; break;
			case 149: s = "invalid SimpleStatement"; break;
			case 150: s = "invalid StructureStatement"; break;
			case 151: s = "invalid GetSetComplex"; break;
			case 152: s = "invalid Return"; break;
			case 153: s = "invalid Condition"; break;
			case 154: s = "invalid TryCatchFinally"; break;
			case 155: s = "invalid TryCatchFinally"; break;
			case 156: s = "invalid Assignment"; break;
			case 157: s = "invalid Variable"; break;
			case 158: s = "invalid VariableDeclaration"; break;

#line 229 "D:\DotNetProjects\Prexonite-Hg\prx-assembla-hg\Tools\Parser.frame" //FRAME

			default: s = "error " + n; break;
		}
		if(s.EndsWith(" expected"))
            s = "after \"" + parentParser.t.ToString(false) + "\", " + s.Replace("expected","is expected") + " and not \"" + parentParser.la.ToString(false) + "\"";
		else if(s.StartsWith("this symbol "))
		    s = "\"" + parentParser.t.val + "\"" + s.Substring(12);
        var msg = ParseMessage.Error(s, parentParser.scanner.File, line, col);
		AddLast(msg);
        OnMessageReceived(msg);
	}

	internal void SemErr (int line, int col, string s) {
        var msg = ParseMessage.Error(s, parentParser.scanner.File, line, col);
		AddLast(msg);
        OnMessageReceived(msg);

	}
	
	internal void SemErr (string s) {
        var msg = ParseMessage.Error(s, parentParser.scanner.File, parentParser.la.line, parentParser.la.col);
		AddLast(msg);
        OnMessageReceived(msg);
	}
	
	internal void Warning (int line, int col, string s) {
        var msg = ParseMessage.Warning(s, parentParser.scanner.File, line, col);
		AddLast(msg);
        OnMessageReceived(msg);

	}

    internal void Info (int line, int col, string s) {
        var msg = ParseMessage.Info(s, parentParser.scanner.File, line, col);
		AddLast(msg);
        OnMessageReceived(msg);
	}
	
	internal void Warning(string s) {
        var msg = ParseMessage.Warning(s, parentParser.scanner.File, parentParser.la.line, parentParser.la.col);
		AddLast(msg);
        OnMessageReceived(msg);
	}

    internal void Info(string s) {
        var msg = ParseMessage.Info(s, parentParser.scanner.File, parentParser.la.line, parentParser.la.col);
		AddLast(msg);
        OnMessageReceived(msg);
	}

    public int GetErrorCount() 
    {
        return System.Linq.Enumerable.Count(System.Linq.Enumerable.Where(this, pm => pm.Severity == ParseMessageSeverity.Error));
    }

    public int GetWarningCount() 
    {
         return System.Linq.Enumerable.Count(System.Linq.Enumerable.Where(this, pm => pm.Severity == ParseMessageSeverity.Warning));
    }
} // Errors


#line default //END FRAME $$$

}