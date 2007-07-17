//SOURCE ARRAY
/*Header.atg:29*/using System.IO;
using Prexonite;
using System.Collections.Generic;
using FatalError = Prexonite.Compiler.FatalCompilerException;
using StringBuilder = System.Text.StringBuilder;
using Prexonite.Compiler.Ast;
using Prexonite.Types;//END SOURCE ARRAY


#line 27 "F:\DotNetProjects\Prexonite\Tools\Parser.frame" //FRAME

using System;


#line default //END FRAME -->namespace

namespace Prexonite.Compiler {


#line 30 "F:\DotNetProjects\Prexonite\Tools\Parser.frame" //FRAME


using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

internal interface IScanner
{
    Token Scan();
    Token Peek();
    void ResetPeek();
    string File { get; }
}

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
	public const int _var = 41;
	public const int _ref = 42;
	public const int _true = 43;
	public const int _false = 44;
	public const int _BEGINKEYWORDS = 45;
	public const int _mod = 46;
	public const int _is = 47;
	public const int _as = 48;
	public const int _not = 49;
	public const int _enabled = 50;
	public const int _disabled = 51;
	public const int _function = 52;
	public const int _command = 53;
	public const int _asm = 54;
	public const int _declare = 55;
	public const int _build = 56;
	public const int _return = 57;
	public const int _in = 58;
	public const int _to = 59;
	public const int _add = 60;
	public const int _continue = 61;
	public const int _break = 62;
	public const int _yield = 63;
	public const int _or = 64;
	public const int _and = 65;
	public const int _xor = 66;
	public const int _label = 67;
	public const int _goto = 68;
	public const int _static = 69;
	public const int _null = 70;
	public const int _if = 71;
	public const int _unless = 72;
	public const int _else = 73;
	public const int _new = 74;
	public const int _coroutine = 75;
	public const int _from = 76;
	public const int _do = 77;
	public const int _does = 78;
	public const int _while = 79;
	public const int _until = 80;
	public const int _for = 81;
	public const int _foreach = 82;
	public const int _try = 83;
	public const int _catch = 84;
	public const int _finally = 85;
	public const int _throw = 86;
	public const int _uusing = 87;
	public const int _ENDKEYWORDS = 88;
	public const int _LPopExpr = 89;
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
		@var = 41,
		@ref = 42,
		@true = 43,
		@false = 44,
		@BEGINKEYWORDS = 45,
		@mod = 46,
		@is = 47,
		@as = 48,
		@not = 49,
		@enabled = 50,
		@disabled = 51,
		@function = 52,
		@command = 53,
		@asm = 54,
		@declare = 55,
		@build = 56,
		@return = 57,
		@in = 58,
		@to = 59,
		@add = 60,
		@continue = 61,
		@break = 62,
		@yield = 63,
		@or = 64,
		@and = 65,
		@xor = 66,
		@label = 67,
		@goto = 68,
		@static = 69,
		@null = 70,
		@if = 71,
		@unless = 72,
		@else = 73,
		@new = 74,
		@coroutine = 75,
		@from = 76,
		@do = 77,
		@does = 78,
		@while = 79,
		@until = 80,
		@for = 81,
		@foreach = 82,
		@try = 83,
		@catch = 84,
		@finally = 85,
		@throw = 86,
		@uusing = 87,
		@ENDKEYWORDS = 88,
		@LPopExpr = 89,
	}
	const int maxT = 90;

#line 43 "F:\DotNetProjects\Prexonite\Tools\Parser.frame" //FRAME

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

#line 55 "F:\DotNetProjects\Prexonite\Tools\Parser.frame" //FRAME


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


#line 82 "F:\DotNetProjects\Prexonite\Tools\Parser.frame" //FRAME

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
		} else SynErr(91);
	}

	void AsmInstruction(/*Parser.Assembler.atg:37*/AstBlock block) {
		/*Parser.Assembler.atg:37*/int arguments = 0;
		string id = null;
		int SecArg = 0;
		double dblArg = 0.0;
		string insbase = null; string detail = null;
		bool bolArg = false;
		OpCode code;
		bool justEffect = false;
		
		if (la.kind == _var || la.kind == _ref) {
			/*Parser.Assembler.atg:48*/SymbolInterpretations kind = SymbolInterpretations.LocalObjectVariable; 
			if (la.kind == _var) {
				Get();
			} else {
				Get();
				/*Parser.Assembler.atg:49*/kind = SymbolInterpretations.LocalReferenceVariable; 
			}
			AsmId(/*Parser.Assembler.atg:51*/out id);
			/*Parser.Assembler.atg:54*/target.Function.Variables.Add(id);
			target.Symbols.Add(id, new SymbolEntry(kind, id));
			
			while (la.kind == _comma) {
				Get();
				AsmId(/*Parser.Assembler.atg:58*/out id);
				/*Parser.Assembler.atg:60*/target.Function.Variables.Add(id);
				target.Symbols.Add(id, new SymbolEntry(kind, id));
				
			}
		} else if (/*Parser.Assembler.atg:66*/isInNullGroup()) {
			AsmId(/*Parser.Assembler.atg:66*/out insbase);
			if (la.kind == _dot) {
				Get();
				AsmId(/*Parser.Assembler.atg:67*/out detail);
			}
			/*Parser.Assembler.atg:68*/code = getOpCode(insbase, detail);
			addInstruction(block, new Instruction(code));
			
		} else if (/*Parser.Assembler.atg:74*/isAsmInstruction("label",null) ) {
			AsmId(/*Parser.Assembler.atg:74*/out insbase);
			AsmId(/*Parser.Assembler.atg:77*/out id);
			/*Parser.Assembler.atg:78*/addLabel(block, id); 
		} else if (/*Parser.Assembler.atg:81*/isAsmInstruction("nop", null)) {
			AsmId(/*Parser.Assembler.atg:81*/out insbase);
			/*Parser.Assembler.atg:81*/Instruction ins = new Instruction(OpCode.nop); 
			if (la.kind == _plus) {
				Get();
				AsmId(/*Parser.Assembler.atg:82*/out id);
				/*Parser.Assembler.atg:82*/ins.Id = id; 
			}
			/*Parser.Assembler.atg:84*/addInstruction(block, ins); 
		} else if (/*Parser.Assembler.atg:88*/isAsmInstruction("rot", null)) {
			AsmId(/*Parser.Assembler.atg:88*/out insbase);
			Expect(_dot);
			Integer(/*Parser.Assembler.atg:89*/out arguments);
			Expect(_comma);
			Integer(/*Parser.Assembler.atg:90*/out SecArg);
			/*Parser.Assembler.atg:92*/addInstruction(block, Instruction.CreateRotate(arguments, SecArg)); 
		} else if (/*Parser.Assembler.atg:97*/isAsmInstruction("swap", null)) {
			AsmId(/*Parser.Assembler.atg:97*/out insbase);
			/*Parser.Assembler.atg:98*/addInstruction(block, Instruction.CreateExchange()); 
		} else if (/*Parser.Assembler.atg:103*/isAsmInstruction("ldc", "real")) {
			AsmId(/*Parser.Assembler.atg:103*/out insbase);
			Expect(_dot);
			AsmId(/*Parser.Assembler.atg:105*/out detail);
			SignedReal(/*Parser.Assembler.atg:106*/out dblArg);
			/*Parser.Assembler.atg:107*/addInstruction(block, Instruction.CreateConstant(dblArg)); 
		} else if (/*Parser.Assembler.atg:112*/isAsmInstruction("ldc", "bool")) {
			AsmId(/*Parser.Assembler.atg:112*/out insbase);
			Expect(_dot);
			AsmId(/*Parser.Assembler.atg:114*/out detail);
			Boolean(/*Parser.Assembler.atg:115*/out bolArg);
			/*Parser.Assembler.atg:116*/addInstruction(block, Instruction.CreateConstant(bolArg)); 
		} else if (/*Parser.Assembler.atg:121*/isInIntegerGroup()) {
			AsmId(/*Parser.Assembler.atg:121*/out insbase);
			if (la.kind == _dot) {
				Get();
				AsmId(/*Parser.Assembler.atg:122*/out detail);
			}
			SignedInteger(/*Parser.Assembler.atg:123*/out arguments);
			/*Parser.Assembler.atg:124*/code = getOpCode(insbase, detail);
			addInstruction(block, new Instruction(code, arguments));
			
		} else if (/*Parser.Assembler.atg:130*/isInJumpGroup()) {
			AsmId(/*Parser.Assembler.atg:130*/out insbase);
			if (la.kind == _dot) {
				Get();
				AsmId(/*Parser.Assembler.atg:131*/out detail);
			}
			/*Parser.Assembler.atg:132*/Instruction ins = null;
			code = getOpCode(insbase, detail);
			
			if (StartOf(2)) {
				AsmId(/*Parser.Assembler.atg:136*/out id);
				/*Parser.Assembler.atg:138*/ins = new Instruction(code, -1, id);
				
			} else if (la.kind == _integer) {
				Integer(/*Parser.Assembler.atg:140*/out arguments);
				/*Parser.Assembler.atg:140*/ins = new Instruction(code, arguments); 
			} else SynErr(92);
			/*Parser.Assembler.atg:141*/addInstruction(block, ins); 
		} else if (/*Parser.Assembler.atg:146*/isInIdGroup()) {
			AsmId(/*Parser.Assembler.atg:146*/out insbase);
			if (la.kind == _dot) {
				Get();
				AsmId(/*Parser.Assembler.atg:147*/out detail);
			}
			AsmId(/*Parser.Assembler.atg:148*/out id);
			/*Parser.Assembler.atg:149*/code = getOpCode(insbase, detail);
			addInstruction(block, new Instruction(code, id));
			
		} else if (/*Parser.Assembler.atg:156*/isInIdArgGroup()) {
			if (la.kind == _at) {
				Get();
				/*Parser.Assembler.atg:156*/justEffect = true; 
			}
			AsmId(/*Parser.Assembler.atg:158*/out insbase);
			if (la.kind == _dot) {
				Get();
				Integer(/*Parser.Assembler.atg:159*/out arguments);
			} else if (StartOf(2)) {
				/*Parser.Assembler.atg:160*/arguments = 0; 
			} else SynErr(93);
			AsmId(/*Parser.Assembler.atg:162*/out id);
			/*Parser.Assembler.atg:163*/code = getOpCode(insbase, detail);
			addInstruction(block, new Instruction(code, arguments, id, justEffect));
			
		} else if (/*Parser.Assembler.atg:169*/isInArgGroup()) {
			if (la.kind == _at) {
				Get();
				/*Parser.Assembler.atg:169*/justEffect = true; 
			}
			AsmId(/*Parser.Assembler.atg:171*/out insbase);
			if (la.kind == _dot) {
				Get();
				Integer(/*Parser.Assembler.atg:172*/out arguments);
			} else if (StartOf(3)) {
				/*Parser.Assembler.atg:173*/arguments = 0; 
			} else SynErr(94);
			/*Parser.Assembler.atg:175*/code = getOpCode(insbase, detail);
			addInstruction(block, new Instruction(code, arguments, null, justEffect));
			
		} else if (/*Parser.Assembler.atg:181*/isInQualidArgGroup()) {
			if (la.kind == _at) {
				Get();
				/*Parser.Assembler.atg:181*/justEffect = true; 
			}
			AsmId(/*Parser.Assembler.atg:183*/out insbase);
			if (la.kind == _dot) {
				Get();
				Integer(/*Parser.Assembler.atg:184*/out arguments);
			} else if (StartOf(2)) {
				/*Parser.Assembler.atg:185*/arguments = 0; 
			} else SynErr(95);
			AsmQualid(/*Parser.Assembler.atg:187*/out id);
			/*Parser.Assembler.atg:188*/code = getOpCode(insbase, detail);
			addInstruction(block, new Instruction(code, arguments, id, justEffect));
			
		} else if (StartOf(2)) {
			AsmId(/*Parser.Assembler.atg:193*/out insbase);
			/*Parser.Assembler.atg:193*/SemErr("Invalid assembler instruction \"" + insbase + "\" (" + t + ")."); 
		} else SynErr(96);
	}

	void AsmId(/*Parser.Assembler.atg:197*/out string id) {
		/*Parser.Assembler.atg:197*/id = "\\NoId\\"; 
		if (la.kind == _string) {
			String(/*Parser.Assembler.atg:199*/out id);
		} else if (StartOf(4)) {
			Id(/*Parser.Assembler.atg:200*/out id);
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
			}
			/*Parser.Assembler.atg:229*/id = cache(t.val); 
		} else SynErr(97);
	}

	void Integer(/*Parser.Helper.atg:47*/out int value) {
		Expect(_integer);
		/*Parser.Helper.atg:48*/if(!Int32.TryParse(t.val, out value))
		   SemErr("Cannot recognize integer " + t.val + ".");
		
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
		} else SynErr(98);
		/*Parser.Helper.atg:83*/value = modifier * value; 
	}

	void Boolean(/*Parser.Helper.atg:40*/out bool value) {
		/*Parser.Helper.atg:40*/value = true; 
		if (la.kind == _true) {
			Get();
		} else if (la.kind == _false) {
			Get();
			/*Parser.Helper.atg:43*/value = false; 
		} else SynErr(99);
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

	void AsmQualid(/*Parser.Assembler.atg:233*/out string qualid) {
		
		AsmId(/*Parser.Assembler.atg:235*/out qualid);
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
		} else SynErr(100);
	}

	void Expr(/*Parser.Expression.atg:26*/out IAstExpression expr) {
		AtomicExpr(/*Parser.Expression.atg:27*/out expr);
		if (la.kind == _colon) {
			Get();
			/*Parser.Expression.atg:28*/IAstExpression value; 
			Expr(/*Parser.Expression.atg:29*/out value);
			/*Parser.Expression.atg:29*/expr = new AstKeyValuePair(this, expr, value); 
		}
	}

	void AtomicExpr(/*Parser.Expression.atg:33*/out IAstExpression expr) {
		/*Parser.Expression.atg:33*/AstConditionalExpression cexpr = null; expr = null; 
		if (StartOf(7)) {
			OrExpr(/*Parser.Expression.atg:35*/out expr);
			while (la.kind == _question) {
				Get();
				/*Parser.Expression.atg:37*/cexpr = new AstConditionalExpression(this, expr); 
				AtomicExpr(/*Parser.Expression.atg:38*/out cexpr.IfExpression);
				Expect(_colon);
				AtomicExpr(/*Parser.Expression.atg:40*/out cexpr.ElseExpression);
				/*Parser.Expression.atg:40*/expr = cexpr; 
			}
		} else if (la.kind == _if || la.kind == _unless) {
			/*Parser.Expression.atg:42*/bool isNegated = false; 
			if (la.kind == _if) {
				Get();
			} else {
				Get();
				/*Parser.Expression.atg:44*/isNegated = true; 
			}
			Expect(_lpar);
			OrExpr(/*Parser.Expression.atg:46*/out expr);
			Expect(_rpar);
			/*Parser.Expression.atg:46*/cexpr = new AstConditionalExpression(this, expr, isNegated); 
			AtomicExpr(/*Parser.Expression.atg:47*/out cexpr.IfExpression);
			Expect(_else);
			AtomicExpr(/*Parser.Expression.atg:49*/out cexpr.ElseExpression);
			/*Parser.Expression.atg:49*/expr = cexpr; 
		} else SynErr(101);
	}

	void OrExpr(/*Parser.Expression.atg:53*/out IAstExpression expr) {
		/*Parser.Expression.atg:53*/IAstExpression lhs, rhs; 
		AndExpr(/*Parser.Expression.atg:55*/out lhs);
		/*Parser.Expression.atg:55*/expr = lhs; 
		if (la.kind == _or) {
			Get();
			OrExpr(/*Parser.Expression.atg:56*/out rhs);
			/*Parser.Expression.atg:56*/expr = new AstLogicalOr(this, lhs, rhs); 
		}
	}

	void AndExpr(/*Parser.Expression.atg:62*/out IAstExpression expr) {
		/*Parser.Expression.atg:62*/IAstExpression lhs, rhs; 
		BitOrExpr(/*Parser.Expression.atg:64*/out lhs);
		/*Parser.Expression.atg:64*/expr = lhs; 
		if (la.kind == _and) {
			Get();
			AndExpr(/*Parser.Expression.atg:65*/out rhs);
			/*Parser.Expression.atg:65*/expr = new AstLogicalAnd(this, lhs, rhs); 
		}
	}

	void BitOrExpr(/*Parser.Expression.atg:70*/out IAstExpression expr) {
		/*Parser.Expression.atg:70*/IAstExpression lhs, rhs; 
		BitXorExpr(/*Parser.Expression.atg:72*/out lhs);
		/*Parser.Expression.atg:72*/expr = lhs; 
		while (la.kind == _bitAnd) {
			Get();
			BitXorExpr(/*Parser.Expression.atg:73*/out rhs);
			/*Parser.Expression.atg:73*/expr = new AstBinaryOperator(this, expr, BinaryOperator.BitwiseOr, rhs); 
		}
	}

	void BitXorExpr(/*Parser.Expression.atg:78*/out IAstExpression expr) {
		/*Parser.Expression.atg:78*/IAstExpression lhs, rhs; 
		BitAndExpr(/*Parser.Expression.atg:80*/out lhs);
		/*Parser.Expression.atg:80*/expr = lhs; 
		while (la.kind == _xor) {
			Get();
			BitAndExpr(/*Parser.Expression.atg:81*/out rhs);
			/*Parser.Expression.atg:82*/expr = new AstBinaryOperator(this, expr, BinaryOperator.ExclusiveOr, rhs); 
		}
	}

	void BitAndExpr(/*Parser.Expression.atg:87*/out IAstExpression expr) {
		/*Parser.Expression.atg:87*/IAstExpression lhs, rhs; 
		NotExpr(/*Parser.Expression.atg:89*/out lhs);
		/*Parser.Expression.atg:89*/expr = lhs; 
		while (la.kind == _bitAnd) {
			Get();
			NotExpr(/*Parser.Expression.atg:90*/out rhs);
			/*Parser.Expression.atg:91*/expr = new AstBinaryOperator(this, expr, BinaryOperator.BitwiseAnd, rhs); 
		}
	}

	void NotExpr(/*Parser.Expression.atg:96*/out IAstExpression expr) {
		/*Parser.Expression.atg:96*/IAstExpression lhs; bool isNot = false; 
		if (la.kind == _not) {
			Get();
			/*Parser.Expression.atg:98*/isNot = true; 
		}
		EqlExpr(/*Parser.Expression.atg:100*/out lhs);
		/*Parser.Expression.atg:100*/expr = isNot ? new AstUnaryOperator(this, UnaryOperator.LogicalNot, lhs) : lhs; 
	}

	void EqlExpr(/*Parser.Expression.atg:104*/out IAstExpression expr) {
		/*Parser.Expression.atg:104*/IAstExpression lhs, rhs; BinaryOperator op = BinaryOperator.None; 
		RelExpr(/*Parser.Expression.atg:106*/out lhs);
		/*Parser.Expression.atg:106*/expr = lhs; 
		while (la.kind == _eq || la.kind == _ne) {
			if (la.kind == _eq) {
				Get();
				/*Parser.Expression.atg:107*/op = BinaryOperator.Equality; 
			} else {
				Get();
				/*Parser.Expression.atg:108*/op = BinaryOperator.Inequality; 
			}
			RelExpr(/*Parser.Expression.atg:109*/out rhs);
			/*Parser.Expression.atg:109*/expr = new AstBinaryOperator(this, expr, op, rhs); 
		}
	}

	void RelExpr(/*Parser.Expression.atg:114*/out IAstExpression expr) {
		/*Parser.Expression.atg:114*/IAstExpression lhs, rhs; BinaryOperator op = BinaryOperator.None;  
		CoalExpr(/*Parser.Expression.atg:116*/out lhs);
		/*Parser.Expression.atg:116*/expr = lhs; 
		while (StartOf(8)) {
			if (la.kind == _lt) {
				Get();
				/*Parser.Expression.atg:117*/op = BinaryOperator.LessThan;              
			} else if (la.kind == _le) {
				Get();
				/*Parser.Expression.atg:118*/op = BinaryOperator.LessThanOrEqual;       
			} else if (la.kind == _gt) {
				Get();
				/*Parser.Expression.atg:119*/op = BinaryOperator.GreaterThan;           
			} else {
				Get();
				/*Parser.Expression.atg:120*/op = BinaryOperator.GreaterThanOrEqual;    
			}
			CoalExpr(/*Parser.Expression.atg:121*/out rhs);
			/*Parser.Expression.atg:121*/expr = new AstBinaryOperator(this, expr, op, rhs); 
		}
	}

	void CoalExpr(/*Parser.Expression.atg:126*/out IAstExpression expr) {
		/*Parser.Expression.atg:126*/IAstExpression lhs, rhs; AstCoalescence coal = new AstCoalescence(this); 
		AddExpr(/*Parser.Expression.atg:128*/out lhs);
		/*Parser.Expression.atg:128*/expr = lhs; coal.Expressions.Add(lhs); 
		while (la.kind == _coalescence) {
			Get();
			AddExpr(/*Parser.Expression.atg:131*/out rhs);
			/*Parser.Expression.atg:131*/expr = coal; coal.Expressions.Add(rhs); 
		}
	}

	void AddExpr(/*Parser.Expression.atg:136*/out IAstExpression expr) {
		/*Parser.Expression.atg:136*/IAstExpression lhs,rhs; BinaryOperator op = BinaryOperator.None; 
		MulExpr(/*Parser.Expression.atg:138*/out lhs);
		/*Parser.Expression.atg:138*/expr = lhs; 
		while (la.kind == _minus || la.kind == _plus) {
			if (la.kind == _plus) {
				Get();
				/*Parser.Expression.atg:139*/op = BinaryOperator.Addition;      
			} else {
				Get();
				/*Parser.Expression.atg:140*/op = BinaryOperator.Subtraction;   
			}
			MulExpr(/*Parser.Expression.atg:141*/out rhs);
			/*Parser.Expression.atg:141*/expr = new AstBinaryOperator(this, expr, op, rhs); 
		}
	}

	void MulExpr(/*Parser.Expression.atg:146*/out IAstExpression expr) {
		/*Parser.Expression.atg:146*/IAstExpression lhs, rhs; BinaryOperator op = BinaryOperator.None; 
		PowExpr(/*Parser.Expression.atg:148*/out lhs);
		/*Parser.Expression.atg:148*/expr = lhs; 
		while (la.kind == _div || la.kind == _times || la.kind == _mod) {
			if (la.kind == _times) {
				Get();
				/*Parser.Expression.atg:149*/op = BinaryOperator.Multiply;      
			} else if (la.kind == _div) {
				Get();
				/*Parser.Expression.atg:150*/op = BinaryOperator.Division;        
			} else {
				Get();
				/*Parser.Expression.atg:151*/op = BinaryOperator.Modulus;       
			}
			PowExpr(/*Parser.Expression.atg:152*/out rhs);
			/*Parser.Expression.atg:152*/expr = new AstBinaryOperator(this, expr, op, rhs); 
		}
	}

	void PowExpr(/*Parser.Expression.atg:157*/out IAstExpression expr) {
		/*Parser.Expression.atg:157*/IAstExpression lhs, rhs; 
		PostfixUnaryExpr(/*Parser.Expression.atg:159*/out lhs);
		/*Parser.Expression.atg:159*/expr = lhs; 
		while (la.kind == _pow) {
			Get();
			PostfixUnaryExpr(/*Parser.Expression.atg:160*/out rhs);
			/*Parser.Expression.atg:160*/expr = new AstBinaryOperator(this, expr, BinaryOperator.Power, rhs); 
		}
	}

	void PostfixUnaryExpr(/*Parser.Expression.atg:165*/out IAstExpression expr) {
		/*Parser.Expression.atg:165*/IAstType type = null; AstGetSet extension; 
		PrefixUnaryExpr(/*Parser.Expression.atg:167*/out expr);
		while (StartOf(9)) {
			if (la.kind == _tilde) {
				Get();
				TypeExpr(/*Parser.Expression.atg:168*/out type);
				/*Parser.Expression.atg:168*/expr = new AstTypecast(this, expr, type); 
			} else if (la.kind == _is) {
				Get();
				TypeExpr(/*Parser.Expression.atg:169*/out type);
				/*Parser.Expression.atg:169*/expr = new AstTypecheck(this, expr, type); 
			} else if (la.kind == _inc) {
				Get();
				/*Parser.Expression.atg:170*/expr = new AstUnaryOperator(this, UnaryOperator.PostIncrement, expr); 
			} else if (la.kind == _dec) {
				Get();
				/*Parser.Expression.atg:171*/expr = new AstUnaryOperator(this, UnaryOperator.PostDecrement, expr); 
			} else {
				GetSetExtension(/*Parser.Expression.atg:172*/expr, out extension);
				/*Parser.Expression.atg:173*/expr = extension; 
			}
		}
	}

	void PrefixUnaryExpr(/*Parser.Expression.atg:178*/out IAstExpression expr) {
		/*Parser.Expression.atg:178*/UnaryOperator op = UnaryOperator.None; 
		while (StartOf(10)) {
			if (la.kind == _plus) {
				Get();
			} else if (la.kind == _minus) {
				Get();
				/*Parser.Expression.atg:181*/op = UnaryOperator.UnaryNegation; 
			} else if (la.kind == _inc) {
				Get();
				/*Parser.Expression.atg:182*/op = UnaryOperator.PreIncrement; 
			} else {
				Get();
				/*Parser.Expression.atg:183*/op = UnaryOperator.PreDecrement; 
			}
		}
		Primary(/*Parser.Expression.atg:185*/out expr);
		/*Parser.Expression.atg:185*/if(op != UnaryOperator.None) expr = new AstUnaryOperator(this, op, expr); 
	}

	void TypeExpr(/*Parser.Expression.atg:336*/out IAstType type) {
		/*Parser.Expression.atg:336*/type = null; 
		if (StartOf(11)) {
			PrexoniteTypeExpr(/*Parser.Expression.atg:338*/out type);
		} else if (la.kind == _ns || la.kind == _doublecolon) {
			ClrTypeExpr(/*Parser.Expression.atg:339*/out type);
		} else SynErr(102);
	}

	void GetSetExtension(/*Parser.Statement.atg:106*/IAstExpression subject, out AstGetSet extension) {
		/*Parser.Statement.atg:106*/extension = null; string id; List<IAstExpression> args = null; 
		if(subject == null)
		{
			SemErr("Member access not preceded by a proper expression.");
			subject = new AstConstant(this,null);
		}
		                             
		if (/*Parser.Statement.atg:116*/isIndirectCall() ) {
			Expect(_dot);
			/*Parser.Statement.atg:116*/extension = new AstIndirectCall(this, PCall.Get, subject); 
			Arguments(/*Parser.Statement.atg:117*/out args);
			/*Parser.Statement.atg:117*/extension.Arguments.AddRange(args); 
		} else if (la.kind == _dot) {
			Get();
			Id(/*Parser.Statement.atg:119*/out id);
			/*Parser.Statement.atg:119*/extension = new AstGetSetMemberAccess(this, PCall.Get, subject, id); 
			if (la.kind == _lpar) {
				Arguments(/*Parser.Statement.atg:120*/out args);
				/*Parser.Statement.atg:121*/extension.Arguments.AddRange(args); 
			}
		} else if (la.kind == _lbrack) {
			/*Parser.Statement.atg:124*/IAstExpression expr; 
			extension = new AstGetSetMemberAccess(this, PCall.Get, subject, ""); 
			
			Get();
			if (StartOf(12)) {
				Expr(/*Parser.Statement.atg:128*/out expr);
				/*Parser.Statement.atg:128*/extension.Arguments.Add(expr); 
				while (WeakSeparator(_comma,12,13) ) {
					Expr(/*Parser.Statement.atg:129*/out expr);
					/*Parser.Statement.atg:129*/extension.Arguments.Add(expr); 
				}
			}
			Expect(_rbrack);
		} else SynErr(103);
	}

	void Primary(/*Parser.Expression.atg:189*/out IAstExpression expr) {
		/*Parser.Expression.atg:189*/expr = null;
		AstGetSet complex = null; bool declared; 
		if (la.kind == _asm) {
			/*Parser.Expression.atg:192*/_pushLexerState(Lexer.Asm); 
			/*Parser.Expression.atg:192*/AstBlockExpression blockExpr = new AstBlockExpression(this); 
			Get();
			Expect(_lpar);
			while (StartOf(1)) {
				AsmInstruction(/*Parser.Expression.atg:193*/blockExpr);
			}
			Expect(_rpar);
			/*Parser.Expression.atg:194*/_popLexerState(); 
			/*Parser.Expression.atg:194*/expr = blockExpr; 
		} else if (StartOf(14)) {
			Constant(/*Parser.Expression.atg:195*/out expr);
		} else if (la.kind == _coroutine) {
			CoroutineCreation(/*Parser.Expression.atg:196*/out expr);
		} else if (la.kind == _new) {
			ObjectCreation(/*Parser.Expression.atg:197*/out expr);
		} else if (la.kind == _lbrack) {
			ListLiteral(/*Parser.Expression.atg:198*/out expr);
		} else if (la.kind == _lbrace) {
			HashLiteral(/*Parser.Expression.atg:199*/out expr);
		} else if (StartOf(15)) {
			LoopExpr(/*Parser.Expression.atg:200*/out expr);
		} else if (/*Parser.Expression.atg:202*/isLambdaExpression()) {
			LambdaExpression(/*Parser.Expression.atg:202*/out expr);
		} else if (StartOf(16)) {
			if (la.kind == _lpar) {
				Get();
				Expr(/*Parser.Expression.atg:204*/out expr);
				Expect(_rpar);
			} else {
				GetInitiator(/*Parser.Expression.atg:205*/out complex, out declared);
				/*Parser.Expression.atg:206*/expr = complex; 
			}
		} else if (la.kind == _LPopExpr) {
			Get();
			Expect(_lpar);
			Expr(/*Parser.Expression.atg:208*/out expr);
			/*Parser.Expression.atg:213*/_popLexerState(); _inject(_plus); 
			Expect(_rpar);
		} else SynErr(104);
	}

	void Constant(/*Parser.Expression.atg:218*/out IAstExpression expr) {
		/*Parser.Expression.atg:218*/expr = null; int vi; double vr; bool vb; string vs; 
		if (la.kind == _integer) {
			Integer(/*Parser.Expression.atg:220*/out vi);
			/*Parser.Expression.atg:220*/expr = new AstConstant(this, vi); 
		} else if (la.kind == _real) {
			Real(/*Parser.Expression.atg:221*/out vr);
			/*Parser.Expression.atg:221*/expr = new AstConstant(this, vr); 
		} else if (la.kind == _true || la.kind == _false) {
			Boolean(/*Parser.Expression.atg:222*/out vb);
			/*Parser.Expression.atg:222*/expr = new AstConstant(this, vb); 
		} else if (la.kind == _string) {
			String(/*Parser.Expression.atg:223*/out vs);
			/*Parser.Expression.atg:223*/expr = new AstConstant(this, vs); 
		} else if (la.kind == _null) {
			Null();
			/*Parser.Expression.atg:224*/expr = new AstConstant(this, null); 
		} else SynErr(105);
	}

	void CoroutineCreation(/*Parser.Expression.atg:278*/out IAstExpression expr) {
		/*Parser.Expression.atg:279*/AstCreateCoroutine cor = new AstCreateCoroutine(this); 
		IAstExpression iexpr;
		expr = cor;
		
		Expect(_coroutine);
		Expr(/*Parser.Expression.atg:284*/out iexpr);
		/*Parser.Expression.atg:284*/cor.Expression = iexpr; 
		if (la.kind == _for) {
			Get();
			Arguments(/*Parser.Expression.atg:285*/out cor.Arguments);
		}
	}

	void ObjectCreation(/*Parser.Expression.atg:269*/out IAstExpression expr) {
		/*Parser.Expression.atg:269*/IAstType type; expr = null; 
		Expect(_new);
		TypeExpr(/*Parser.Expression.atg:271*/out type);
		/*Parser.Expression.atg:271*/AstObjectCreation creation = new AstObjectCreation(this, type);
		List<IAstExpression> args; 
		if (la.kind == _lpar) {
			Arguments(/*Parser.Expression.atg:273*/out args);
			/*Parser.Expression.atg:273*/creation.Arguments.AddRange(args); 
		}
		/*Parser.Expression.atg:274*/expr = creation; 
	}

	void ListLiteral(/*Parser.Expression.atg:228*/out IAstExpression expr) {
		/*Parser.Expression.atg:228*/IAstExpression iexpr = null; 
		AstListLiteral lst = new AstListLiteral(this);
		expr = lst;
		
		Expect(_lbrack);
		if (StartOf(12)) {
			Expr(/*Parser.Expression.atg:234*/out iexpr);
			/*Parser.Expression.atg:234*/lst.Elements.Add(iexpr); 
			while (la.kind == _comma) {
				Get();
				Expr(/*Parser.Expression.atg:236*/out iexpr);
				/*Parser.Expression.atg:236*/lst.Elements.Add(iexpr); 
			}
		}
		Expect(_rbrack);
	}

	void HashLiteral(/*Parser.Expression.atg:243*/out IAstExpression expr) {
		/*Parser.Expression.atg:243*/IAstExpression iexpr = null; 
		AstHashLiteral hash = new AstHashLiteral(this);
		expr = hash;
		
		Expect(_lbrace);
		if (StartOf(12)) {
			Expr(/*Parser.Expression.atg:249*/out iexpr);
			/*Parser.Expression.atg:249*/hash.Elements.Add(iexpr); 
			while (la.kind == _comma) {
				Get();
				Expr(/*Parser.Expression.atg:251*/out iexpr);
				/*Parser.Expression.atg:251*/hash.Elements.Add(iexpr); 
			}
		}
		Expect(_rbrace);
	}

	void LoopExpr(/*Parser.Expression.atg:258*/out IAstExpression expr) {
		/*Parser.Expression.atg:258*/AstBlock dummyBlock = new AstBlock(this);
		
		if (la.kind == _do || la.kind == _while || la.kind == _until) {
			WhileLoop(/*Parser.Expression.atg:261*/dummyBlock);
		} else if (la.kind == _for) {
			ForLoop(/*Parser.Expression.atg:262*/dummyBlock);
		} else if (la.kind == _foreach) {
			ForeachLoop(/*Parser.Expression.atg:263*/dummyBlock);
		} else SynErr(106);
		/*Parser.Expression.atg:264*/expr = new AstLoopExpression(this, (AstLoop) dummyBlock.Statements[0]); 
	}

	void LambdaExpression(/*Parser.Expression.atg:289*/out IAstExpression expr) {
		/*Parser.Expression.atg:289*/expr = null;
		PFunction func = new PFunction(TargetApplication, generateNestedFunctionId());                                             
		func.Meta[Application.ImportKey] = target.Function.Meta[Application.ImportKey];
		TargetApplication.Functions.Add(func);
		Loader.CreateFunctionTarget(func, new AstBlock(this));
		CompilerTarget ft = FunctionTargets[func];
		ft.ParentTarget = target;
		
		if (StartOf(17)) {
			FormalArg(/*Parser.Expression.atg:298*/ft);
		} else if (la.kind == _lpar) {
			Get();
			if (StartOf(17)) {
				FormalArg(/*Parser.Expression.atg:300*/ft);
				while (la.kind == _comma) {
					Get();
					FormalArg(/*Parser.Expression.atg:302*/ft);
				}
			}
			Expect(_rpar);
		} else SynErr(107);
		/*Parser.Expression.atg:308*/CompilerTarget oldTarget = target;
		target = ft;
		
		Expect(_implementation);
		if (la.kind == _lbrace) {
			Get();
			while (StartOf(18)) {
				Statement(/*Parser.Expression.atg:313*/ft.Ast);
			}
			Expect(_rbrace);
		} else if (StartOf(12)) {
			/*Parser.Expression.atg:315*/AstReturn ret = new AstReturn(this, ReturnVariant.Exit); 
			Expr(/*Parser.Expression.atg:316*/out ret.Expression);
			/*Parser.Expression.atg:316*/ft.Ast.Add(ret); 
		} else SynErr(108);
		/*Parser.Expression.atg:319*/target = oldTarget;
		if(errors.count == 0)
		{
		    Ast[func].EmitCode(FunctionTargets[func]);
		    FunctionTargets[func].FinishTarget();
		}
		
		expr = new AstCreateClosure(this, func.Id);                                         
		
	}

	void GetInitiator(/*Parser.Statement.atg:136*/out AstGetSet complex, out bool isDeclaration) {
		/*Parser.Statement.atg:136*/complex = null; 
		AstGetSetSymbol symbol = null;
		AstGetSetStatic staticCall = null;
		isDeclaration = false;
		string id;
		
		if (StartOf(19)) {
			if (/*Parser.Statement.atg:145*/isLikeFunction(la.val) ) {
				Function(/*Parser.Statement.atg:145*/out symbol);
			} else if (StartOf(20)) {
				Variable(/*Parser.Statement.atg:146*/out symbol, out isDeclaration);
			} else {
				StaticCall(/*Parser.Statement.atg:147*/out staticCall);
			}
			/*Parser.Statement.atg:149*/complex = (AstGetSet)symbol ?? (AstGetSet)staticCall; 
		} else if (/*Parser.Statement.atg:152*/isDeDereference() ) {
			Expect(_pointer);
			Expect(_pointer);
			Id(/*Parser.Statement.atg:152*/out id);
			/*Parser.Statement.atg:152*/SymbolEntry s = target.Symbols[id];
			SymbolInterpretations kind;
			if(s == null)
			{   
			    SemErr("The symbol " + id + " is not defined"); 
			    s = new SymbolEntry(SymbolInterpretations.LocalObjectVariable, id);
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
			complex = new AstGetSetReference(this, s.Id, kind);
			
		} else if (la.kind == _pointer) {
			Get();
			Id(/*Parser.Statement.atg:172*/out id);
			/*Parser.Statement.atg:172*/SymbolEntry s = target.Symbols[id];
			if(s == null)
			{   
			    SemErr("The symbol " + id + " is not defined"); 
			    s = new SymbolEntry(SymbolInterpretations.LocalObjectVariable, id);
			}
			else if(InterpretationIsVariable(s.Interpretation))
			{
			    if(isOuterVariable(id))
			        target.RequireOuterVariable(id);
			}
			complex = new AstGetSetReference(this, s.Id, s.Interpretation);
			
		} else SynErr(109);
	}

	void Real(/*Parser.Helper.atg:61*/out double value) {
		Expect(_real);
		/*Parser.Helper.atg:70*/string real = t.val;
		if(!double.TryParse(real, out value))
		    SemErr("Cannot recognize real " + real + ".");
		
	}

	void Null() {
		Expect(_null);
	}

	void WhileLoop(/*Parser.Statement.atg:344*/AstBlock block) {
		/*Parser.Statement.atg:344*/AstWhileLoop loop = null;
		bool isPositive = true; 
		
		if (la.kind == _while || la.kind == _until) {
			if (la.kind == _while) {
				Get();
			} else {
				Get();
				/*Parser.Statement.atg:348*/isPositive = false; 
			}
			/*Parser.Statement.atg:349*/loop = new AstWhileLoop(this, true, isPositive); 
			Expect(_lpar);
			Expr(/*Parser.Statement.atg:350*/out loop.Condition);
			Expect(_rpar);
			/*Parser.Statement.atg:351*/target.BeginBlock(loop.Labels); 
			StatementBlock(/*Parser.Statement.atg:352*/loop.Block);
		} else if (la.kind == _do) {
			Get();
			/*Parser.Statement.atg:353*/AstBlock loopBody = new AstBlock(this); 
			BlockLabels labels = AstWhileLoop.CreateBlockLabels();
			target.BeginBlock(labels);
			
			StatementBlock(/*Parser.Statement.atg:357*/loopBody);
			if (la.kind == _while) {
				Get();
			} else if (la.kind == _until) {
				Get();
				/*Parser.Statement.atg:358*/isPositive = false; 
			} else SynErr(110);
			/*Parser.Statement.atg:359*/loop = new AstWhileLoop(this, false, isPositive); 
			loop.Labels = labels;
			loop.Block = loopBody;
			
			Expect(_lpar);
			Expr(/*Parser.Statement.atg:363*/out loop.Condition);
			Expect(_rpar);
		} else SynErr(111);
		/*Parser.Statement.atg:364*/target.EndBlock(); block.Add(loop); 
	}

	void ForLoop(/*Parser.Statement.atg:367*/AstBlock block) {
		/*Parser.Statement.atg:367*/AstForLoop loop;
		
		Expect(_for);
		/*Parser.Statement.atg:370*/loop = new AstForLoop(this); target.BeginBlock(loop.Labels); 
		Expect(_lpar);
		StatementBlock(/*Parser.Statement.atg:371*/loop.Initialize);
		if (StartOf(12)) {
			if (la.kind == _while || la.kind == _until) {
				if (la.kind == _while) {
					Get();
				} else {
					Get();
					/*Parser.Statement.atg:374*/loop.IsPositive = false; 
				}
			}
			Expr(/*Parser.Statement.atg:376*/out loop.Condition);
			Expect(_semicolon);
			SimpleStatement(/*Parser.Statement.atg:378*/loop.NextIteration);
		} else if (la.kind == _do) {
			Get();
			StatementBlock(/*Parser.Statement.atg:379*/loop.NextIteration);
			/*Parser.Statement.atg:380*/loop.IsPrecondition = false; 
			if (la.kind == _while || la.kind == _until) {
				if (la.kind == _while) {
					Get();
				} else {
					Get();
					/*Parser.Statement.atg:382*/loop.IsPositive = false; 
				}
			}
			Expr(/*Parser.Statement.atg:384*/out loop.Condition);
		} else SynErr(112);
		Expect(_rpar);
		StatementBlock(/*Parser.Statement.atg:387*/loop.Block);
		/*Parser.Statement.atg:387*/target.EndBlock(); block.Add(loop); 
	}

	void ForeachLoop(/*Parser.Statement.atg:391*/AstBlock block) {
		Expect(_foreach);
		/*Parser.Statement.atg:392*/AstForeachLoop loop = new AstForeachLoop(this); 
		Expect(_lpar);
		GetCall(/*Parser.Statement.atg:394*/out loop.Element);
		Expect(_in);
		Expr(/*Parser.Statement.atg:396*/out loop.List);
		Expect(_rpar);
		/*Parser.Statement.atg:398*/target.BeginBlock(loop.Labels); 
		StatementBlock(/*Parser.Statement.atg:399*/loop.Block);
		/*Parser.Statement.atg:400*/target.EndBlock(); 
		/*Parser.Statement.atg:403*/block.Add(loop); 
	}

	void Arguments(/*Parser.Statement.atg:533*/out List<IAstExpression> args) {
		/*Parser.Statement.atg:533*/args = new List<IAstExpression>();
		IAstExpression expr;
		
		Expect(_lpar);
		if (StartOf(12)) {
			Expr(/*Parser.Statement.atg:538*/out expr);
			/*Parser.Statement.atg:538*/args.Add(expr); 
			while (WeakSeparator(_comma,12,21) ) {
				Expr(/*Parser.Statement.atg:540*/out expr);
				/*Parser.Statement.atg:540*/args.Add(expr); 
			}
		}
		Expect(_rpar);
	}

	void FormalArg(/*Parser.GlobalScope.atg:362*/CompilerTarget ft) {
		/*Parser.GlobalScope.atg:362*/string id; SymbolInterpretations kind = SymbolInterpretations.LocalObjectVariable; 
		if (la.kind == _var || la.kind == _ref) {
			if (la.kind == _var) {
				Get();
			} else {
				Get();
				/*Parser.GlobalScope.atg:364*/kind = SymbolInterpretations.LocalReferenceVariable; 
			}
		}
		Id(/*Parser.GlobalScope.atg:366*/out id);
		/*Parser.GlobalScope.atg:366*/ft.Function.Parameters.Add(id); 
		ft.Symbols.Add(id, new SymbolEntry(kind, id));
		
	}

	void Statement(/*Parser.Statement.atg:35*/AstBlock block) {
		if (/*Parser.Statement.atg:38*/isLabel() ) {
			ExplicitLabel(/*Parser.Statement.atg:38*/block);
		} else if (StartOf(22)) {
			if (StartOf(23)) {
				SimpleStatement(/*Parser.Statement.atg:39*/block);
			}
			Expect(_semicolon);
		} else if (StartOf(24)) {
			StructureStatement(/*Parser.Statement.atg:40*/block);
		} else SynErr(113);
		while (la.kind == _and) {
			Get();
			Statement(/*Parser.Statement.atg:42*/block);
		}
	}

	void ExplicitTypeExpr(/*Parser.Expression.atg:330*/out IAstType type) {
		/*Parser.Expression.atg:330*/type = null; 
		if (la.kind == _tilde) {
			Get();
			PrexoniteTypeExpr(/*Parser.Expression.atg:332*/out type);
		} else if (la.kind == _ns || la.kind == _doublecolon) {
			ClrTypeExpr(/*Parser.Expression.atg:333*/out type);
		} else SynErr(114);
	}

	void PrexoniteTypeExpr(/*Parser.Expression.atg:358*/out IAstType type) {
		/*Parser.Expression.atg:358*/string id = null; type = null; 
		if (StartOf(4)) {
			Id(/*Parser.Expression.atg:360*/out id);
		} else if (la.kind == _null) {
			Get();
			/*Parser.Expression.atg:360*/id = NullPType.Literal; 
		} else SynErr(115);
		/*Parser.Expression.atg:362*/AstDynamicTypeExpression dType = new AstDynamicTypeExpression(this, id); 
		if (la.kind == _lt) {
			Get();
			if (StartOf(25)) {
				TypeExprElement(/*Parser.Expression.atg:364*/dType.Arguments);
				while (la.kind == _comma) {
					Get();
					TypeExprElement(/*Parser.Expression.atg:365*/dType.Arguments);
				}
			}
			Expect(_gt);
		}
		/*Parser.Expression.atg:369*/type = dType; 
	}

	void ClrTypeExpr(/*Parser.Expression.atg:343*/out IAstType type) {
		/*Parser.Expression.atg:343*/string id; 
		/*Parser.Expression.atg:345*/StringBuilder typeId = new StringBuilder(); 
		if (la.kind == _doublecolon) {
			Get();
		} else if (la.kind == _ns) {
			Ns(/*Parser.Expression.atg:347*/out id);
			/*Parser.Expression.atg:347*/typeId.Append(id); typeId.Append('.'); 
		} else SynErr(116);
		while (la.kind == _ns) {
			Ns(/*Parser.Expression.atg:349*/out id);
			/*Parser.Expression.atg:349*/typeId.Append(id); typeId.Append('.'); 
		}
		Id(/*Parser.Expression.atg:351*/out id);
		/*Parser.Expression.atg:351*/typeId.Append(id);
		type = new AstConstantTypeExpression(this, 
		    "Object(\"" + StringPType.Escape(typeId.ToString()) + "\")");
		
	}

	void Ns(/*Parser.Helper.atg:35*/out string ns) {
		/*Parser.Helper.atg:35*/ns = "\\NoId\\"; 
		Expect(_ns);
		/*Parser.Helper.atg:37*/ns = cache(t.val); 
	}

	void TypeExprElement(/*Parser.Expression.atg:373*/List<IAstExpression> args ) {
		/*Parser.Expression.atg:373*/IAstExpression expr; IAstType type; 
		if (StartOf(14)) {
			Constant(/*Parser.Expression.atg:375*/out expr);
			/*Parser.Expression.atg:375*/args.Add(expr); 
		} else if (la.kind == _ns || la.kind == _tilde || la.kind == _doublecolon) {
			ExplicitTypeExpr(/*Parser.Expression.atg:376*/out type);
			/*Parser.Expression.atg:376*/args.Add(type); 
		} else if (la.kind == _lpar) {
			Get();
			Expr(/*Parser.Expression.atg:377*/out expr);
			Expect(_rpar);
			/*Parser.Expression.atg:377*/args.Add(expr); 
		} else SynErr(117);
	}

	void Prexonite() {
		/*Parser.GlobalScope.atg:26*/PFunction func; 
		while (StartOf(26)) {
			if (StartOf(27)) {
				if (StartOf(28)) {
					if (la.kind == _var || la.kind == _ref) {
						GlobalVariableDefinition();
					} else if (la.kind == _declare) {
						Declaration();
					} else {
						MetaAssignment(/*Parser.GlobalScope.atg:30*/TargetApplication);
					}
				}
				while (!(la.kind == _EOF || la.kind == _semicolon)) {SynErr(118); Get();}
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
		/*Parser.GlobalScope.atg:85*/string id; PVariable vari; SymbolInterpretations type = SymbolInterpretations.GlobalObjectVariable;; 
		while (!(la.kind == _EOF || la.kind == _var || la.kind == _ref)) {SynErr(119); Get();}
		if (la.kind == _var) {
			Get();
		} else if (la.kind == _ref) {
			Get();
			/*Parser.GlobalScope.atg:89*/type = SymbolInterpretations.GlobalReferenceVariable; 
		} else SynErr(120);
		GlobalId(/*Parser.GlobalScope.atg:92*/out id);
		/*Parser.GlobalScope.atg:93*/Symbols[id] = new SymbolEntry(type, id);
		if(TargetApplication.Variables.ContainsKey(id))
		    vari = TargetApplication.Variables[id];
		else
		{
		    vari = new PVariable(id);
		    TargetApplication.Variables[id] = vari;
		}
		
		if (la.kind == _lbrack) {
			Get();
			while (StartOf(29)) {
				MetaAssignment(/*Parser.GlobalScope.atg:103*/vari);
				while (!(la.kind == _EOF || la.kind == _semicolon)) {SynErr(121); Get();}
				Expect(_semicolon);
			}
			Expect(_rbrack);
		}
		if (la.kind == _assign) {
			/*Parser.GlobalScope.atg:107*/_pushLexerState(Lexer.Local); 
			Get();
			/*Parser.GlobalScope.atg:108*/CompilerTarget lastTarget = target;
			  target=FunctionTargets[Application.InitializationId];
			  IAstExpression expr;
			
			Expr(/*Parser.GlobalScope.atg:112*/out expr);
			/*Parser.GlobalScope.atg:113*/_popLexerState();
			AstGetSet complex = new AstGetSetSymbol(this, PCall.Set, id, InterpretAsObjectVariable(type));
			complex.Arguments.Add(expr);
			target.Ast.Add(complex);
			vari.Meta[Application.InitializationId] = TargetApplication._RegisterInitializationUpdate().ToString();
			target = lastTarget;
			
		}
	}

	void Declaration() {
		/*Parser.GlobalScope.atg:126*/SymbolInterpretations type = SymbolInterpretations.Undefined; 
		while (!(la.kind == _EOF || la.kind == _declare)) {SynErr(122); Get();}
		Expect(_declare);
		if (StartOf(30)) {
			if (la.kind == _var) {
				Get();
				/*Parser.GlobalScope.atg:130*/type = SymbolInterpretations.GlobalObjectVariable; 
			} else if (la.kind == _ref) {
				Get();
				/*Parser.GlobalScope.atg:131*/type = SymbolInterpretations.GlobalReferenceVariable; 
			} else if (la.kind == _function) {
				Get();
				/*Parser.GlobalScope.atg:132*/type = SymbolInterpretations.Function; 
			} else {
				Get();
				/*Parser.GlobalScope.atg:133*/type = SymbolInterpretations.Command; 
			}
		}
		DeclarationInstance(/*Parser.GlobalScope.atg:135*/type);
		while (WeakSeparator(_comma,4,31) ) {
			DeclarationInstance(/*Parser.GlobalScope.atg:136*/type);
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
		} else if (la.kind == _id || la.kind == _anyId) {
			GlobalId(/*Parser.GlobalScope.atg:48*/out key);
			if (la.kind == _enabled) {
				Get();
				/*Parser.GlobalScope.atg:49*/entry = true; 
			} else if (la.kind == _disabled) {
				Get();
				/*Parser.GlobalScope.atg:50*/entry = false; 
			} else if (StartOf(32)) {
				MetaExpr(/*Parser.GlobalScope.atg:51*/out entry);
			} else SynErr(123);
		} else if (la.kind == _add) {
			Get();
			/*Parser.GlobalScope.atg:53*/MetaEntry subEntry; 
			MetaExpr(/*Parser.GlobalScope.atg:54*/out subEntry);
			/*Parser.GlobalScope.atg:54*/if(!subEntry.IsList) subEntry = (MetaEntry) subEntry.List; 
			Expect(_to);
			GlobalId(/*Parser.GlobalScope.atg:56*/out key);
			/*Parser.GlobalScope.atg:56*/if(target.Meta.ContainsKey(key))
			{
			    entry = target.Meta[key];
			    entry.AddToList(subEntry.List);
			}else
			    entry = subEntry;
			
		} else SynErr(124);
		/*Parser.GlobalScope.atg:63*/if(entry == null || key == null) SemErr("Meta assignment did not generate an entry."); else target.Meta[key] = entry; 
	}

	void GlobalCode() {
		/*Parser.GlobalScope.atg:216*/PFunction func = TargetApplication._InitializationFunction;
		CompilerTarget ft = FunctionTargets[func];
		
		/*Parser.GlobalScope.atg:221*/target = ft; 
		                             _pushLexerState(Lexer.Local);
		                         
		Expect(_lbrace);
		while (StartOf(18)) {
			Statement(/*Parser.GlobalScope.atg:225*/target.Ast);
		}
		Expect(_rbrace);
		/*Parser.GlobalScope.atg:228*/if(errors.count == 0)
		TargetApplication._RequireInitialization();
		target = null;
		_popLexerState();
		
	}

	void BuildBlock() {
		while (!(la.kind == _EOF || la.kind == _build)) {SynErr(125); Get();}
		Expect(_build);
		/*Parser.GlobalScope.atg:174*/PFunction func = new PFunction(TargetApplication);
		  CompilerTarget lastTarget = target; 
		  target = Loader.CreateFunctionTarget(func, new AstBlock(this));
		  target.Declare(SymbolInterpretations.Command, "Add", Loader.BuildAddCommand);
		  target.Declare(SymbolInterpretations.Command, "Require", Loader.BuildRequireCommand);
		  target.Declare(SymbolInterpretations.Command, "Default", Loader.BuildDefaultCommand);
		  _pushLexerState(Lexer.Local);                                
		
		if (la.kind == _does) {
			Get();
		}
		StatementBlock(/*Parser.GlobalScope.atg:184*/target.Ast);
		/*Parser.GlobalScope.atg:187*/_popLexerState();
		  if(errors.count > 0)
		  {
		      SemErr("Cannot execute build block. Errors detected");
		      return;
		  }
		  target.Ast.EmitCode(target);
		  target.FinishTarget();	                                
		  target = lastTarget;
		  //Run the build block 
		  FunctionContext fctx = func.CreateFunctionContext(ParentEngine, new PValue[] {}, new PVariable[] {}, true);
		  
		  try
		  {
		      TargetApplication._SuppressInitialization = true;
		      Loader.BuildCommandsEnabled = true;
		      ParentEngine.Process(fctx);
		  }
		  finally
		  {
		      Loader.BuildCommandsEnabled = false;
		      TargetApplication._SuppressInitialization = false;
		  }
		
	}

	void FunctionDefinition(/*Parser.GlobalScope.atg:238*/out PFunction func) {
		/*Parser.GlobalScope.atg:239*/func = null; 
		string id;     
		string funcId; 
		func = null; 
		bool isNested = target != null; 
		bool isCoroutine = false;
		PFunction corBody = null;
		PFunction corStub = null;
		string corId = null;
		CompilerTarget ct = null;
		CompilerTarget cst = null;
		
		if (la.kind == _function) {
			Get();
		} else if (la.kind == _coroutine) {
			Get();
			/*Parser.GlobalScope.atg:253*/isCoroutine = true;
			
		} else SynErr(126);
		Id(/*Parser.GlobalScope.atg:256*/out id);
		/*Parser.GlobalScope.atg:256*/funcId = id;
		  if(Engine.StringsAreEqual(id, @"\init"))
		  {
		      func = TargetApplication._InitializationFunction;
		      if(isNested)
		          SemErr("Cannot define initialization code inside another function.");
		      if(isCoroutine)
		          SemErr("Cannot define initialization code as a coroutine.");
		  }
		  else
		  {
		      if(isNested)
		          funcId = generateNestedFunctionId(id);
		      func = new PFunction(TargetApplication, funcId);
		      
		      if(isNested)
		           func.Meta["LogicalId"] = id;
		      
		      Loader.CreateFunctionTarget(func, new AstBlock(this));
		      TargetApplication.Functions.Add(func);
		  }
		  CompilerTarget ft = FunctionTargets[func];
		  if(isCoroutine)
		  {
		      corStub = func;
		      
		      //Create coroutine body function
		      corId = generateNestedFunctionId(ft);                                            
		      corBody = new PFunction(TargetApplication, corId);
		      Loader.CreateFunctionTarget(corBody, new AstBlock(this));
		      TargetApplication.Functions.Add(corBody);
		      corBody.Meta["LogicalId"] = id;
		
		                                            //Get compiler target references
		                                            ct = FunctionTargets[corBody];
		                                            cst = ft;
		                                            ct.ParentTarget = cst;
		                                        }
		                                        if(target != null) //Link to parent in case of a nested function
		                                            ft.ParentTarget = target;	                                           
			                                    
			                                
		if (la.kind == _lpar) {
			Get();
			if (StartOf(17)) {
				FormalArg(/*Parser.GlobalScope.atg:299*/ft);
				while (la.kind == _comma) {
					Get();
					FormalArg(/*Parser.GlobalScope.atg:301*/ft);
				}
			}
			Expect(_rpar);
		}
		/*Parser.GlobalScope.atg:305*/if(target == null && 
		     (!object.ReferenceEquals(func, TargetApplication._InitializationFunction)))
		 {
		         //Add the name to the symboltable
		         Symbols[func.Id] = new SymbolEntry(SymbolInterpretations.Function, func.Id);
		 }
		 //Target the coroutine body instead of the stub
		    if(isCoroutine)
		        func = corBody;
		
		if (la.kind == _lbrack) {
			/*Parser.GlobalScope.atg:315*/_pushLexerState(Lexer.YYINITIAL); 
			Get();
			while (StartOf(29)) {
				MetaAssignment(/*Parser.GlobalScope.atg:317*/func);
				while (!(la.kind == _EOF || la.kind == _semicolon)) {SynErr(127); Get();}
				Expect(_semicolon);
			}
			/*Parser.GlobalScope.atg:319*/_popLexerState(); 
			Expect(_rbrack);
		}
		/*Parser.GlobalScope.atg:322*/CompilerTarget lastTarget = target;
		target = FunctionTargets[func]; 
		_pushLexerState(Lexer.Local);
		
		if (la.kind == _does) {
			Get();
			StatementBlock(/*Parser.GlobalScope.atg:327*/target.Ast);
		} else if (la.kind == _lbrace) {
			Get();
			while (StartOf(18)) {
				Statement(/*Parser.GlobalScope.atg:328*/target.Ast);
			}
			Expect(_rbrace);
		} else if (la.kind == _assign) {
			Get();
			/*Parser.GlobalScope.atg:329*/AstReturn ret = new AstReturn(this, ReturnVariant.Exit); 
			Expr(/*Parser.GlobalScope.atg:330*/out ret.Expression);
			/*Parser.GlobalScope.atg:330*/target.Ast.Add(ret); 
			Expect(_semicolon);
		} else SynErr(128);
		/*Parser.GlobalScope.atg:332*/_popLexerState();
		target = lastTarget; 
		//Compile AST
		if(errors.count == 0)
		    if(Engine.StringsAreEqual(func.Id, @"\init"))
		    {
		        TargetApplication._RequireInitialization();
		    }
		    else
		    {
		        Ast[func].EmitCode(FunctionTargets[func]);
		        FunctionTargets[func].FinishTarget();
		    }
		    
		if(isCoroutine)
		{
		    //Stub has to be returned
		    func = corStub;
		    //Generate code for the stub
		    AstCreateCoroutine crcor = new AstCreateCoroutine(this);                                            
		    crcor.Expression = new AstCreateClosure(this,corBody.Id);
		    AstReturn retst = new AstReturn(this, ReturnVariant.Exit);
		    retst.Expression = crcor;
		    cst.Ast.Add(retst);
		    cst.Ast.EmitCode(cst);
		    cst.FinishTarget();
		}
		
	}

	void GlobalId(/*Parser.GlobalScope.atg:371*/out string id) {
		/*Parser.GlobalScope.atg:371*/id = "...no freaking id..."; 
		if (la.kind == _id) {
			Get();
			/*Parser.GlobalScope.atg:373*/id = cache(t.val); 
		} else if (la.kind == _anyId) {
			Get();
			/*Parser.GlobalScope.atg:374*/id = cache(t.val.Substring(1)); 
		} else SynErr(129);
	}

	void MetaExpr(/*Parser.GlobalScope.atg:67*/out MetaEntry entry) {
		/*Parser.GlobalScope.atg:67*/bool sw; int i; double r; entry = null; string str; 
		switch (la.kind) {
		case _true: case _false: {
			Boolean(/*Parser.GlobalScope.atg:69*/out sw);
			/*Parser.GlobalScope.atg:69*/entry = sw; 
			break;
		}
		case _integer: {
			Integer(/*Parser.GlobalScope.atg:70*/out i);
			/*Parser.GlobalScope.atg:70*/entry = i.ToString(); 
			break;
		}
		case _real: {
			Real(/*Parser.GlobalScope.atg:71*/out r);
			/*Parser.GlobalScope.atg:71*/entry = r.ToString(); 
			break;
		}
		case _string: {
			String(/*Parser.GlobalScope.atg:72*/out str);
			/*Parser.GlobalScope.atg:72*/entry = str; 
			break;
		}
		case _id: case _anyId: case _ns: {
			GlobalQualifiedId(/*Parser.GlobalScope.atg:73*/out str);
			/*Parser.GlobalScope.atg:73*/entry = str; 
			break;
		}
		case _lbrace: {
			Get();
			/*Parser.GlobalScope.atg:74*/List<MetaEntry> lst = new List<MetaEntry>(); MetaEntry subEntry; 
			if (StartOf(32)) {
				MetaExpr(/*Parser.GlobalScope.atg:75*/out subEntry);
				/*Parser.GlobalScope.atg:75*/lst.Add(subEntry); 
				while (WeakSeparator(_comma,32,33) ) {
					MetaExpr(/*Parser.GlobalScope.atg:77*/out subEntry);
					/*Parser.GlobalScope.atg:77*/lst.Add(subEntry); 
				}
			}
			Expect(_rbrace);
			/*Parser.GlobalScope.atg:80*/entry = (MetaEntry) lst.ToArray(); 
			break;
		}
		default: SynErr(130); break;
		}
	}

	void GlobalQualifiedId(/*Parser.GlobalScope.atg:377*/out string id) {
		/*Parser.GlobalScope.atg:377*/id = "\\NoId\\"; 
		if (la.kind == _id || la.kind == _anyId) {
			GlobalId(/*Parser.GlobalScope.atg:379*/out id);
		} else if (la.kind == _ns) {
			Ns(/*Parser.GlobalScope.atg:380*/out id);
			/*Parser.GlobalScope.atg:380*/StringBuilder buffer = new StringBuilder(id); buffer.Append('.'); 
			while (la.kind == _ns) {
				Ns(/*Parser.GlobalScope.atg:381*/out id);
				/*Parser.GlobalScope.atg:381*/buffer.Append(id); buffer.Append('.'); 
			}
			GlobalId(/*Parser.GlobalScope.atg:383*/out id);
			/*Parser.GlobalScope.atg:383*/buffer.Append(id); 
			/*Parser.GlobalScope.atg:384*/id = buffer.ToString(); 
		} else SynErr(131);
	}

	void DeclarationInstance(/*Parser.GlobalScope.atg:140*/SymbolInterpretations type) {
		/*Parser.GlobalScope.atg:140*/string id; string aId; 
		Id(/*Parser.GlobalScope.atg:142*/out id);
		/*Parser.GlobalScope.atg:142*/aId = id; 
		if (la.kind == _as) {
			Get();
			Id(/*Parser.GlobalScope.atg:143*/out aId);
		}
		/*Parser.GlobalScope.atg:144*/SymbolEntry inferredType;
		if(target == null) //global symbol
		{
		    if(type == SymbolInterpretations.Undefined)
		        if(Symbols.TryGetValue(id, out inferredType))
		            type = inferredType.Interpretation;
		        else if(Symbols.TryGetValue(aId, out inferredType))
		            type = inferredType.Interpretation;
		        else
		            SemErr("Interpretation of symbol " + id + " as " + aId + " cannot be inferred.");
		    Symbols[aId] = new SymbolEntry(type, id);
		}
		else
		{
		    if(type == SymbolInterpretations.Undefined)
		        if(target.Symbols.TryGetValue(id, out inferredType))
		            type = inferredType.Interpretation;
		        else if(target.Symbols.TryGetValue(aId, out inferredType))
		            type = inferredType.Interpretation;
		        else
		            SemErr("Interpretation of symbol " + id + " as " + aId + " cannot be inferred.");
		    target.Symbols[aId] = new SymbolEntry(type, id);
		}
		
	}

	void StatementBlock(/*Parser.Statement.atg:26*/AstBlock block) {
		if (la.kind == _lbrace) {
			Get();
			while (StartOf(18)) {
				Statement(/*Parser.Statement.atg:28*/block);
			}
			Expect(_rbrace);
		} else if (StartOf(18)) {
			Statement(/*Parser.Statement.atg:31*/block);
		} else SynErr(132);
	}

	void ExplicitLabel(/*Parser.Statement.atg:295*/AstBlock block) {
		/*Parser.Statement.atg:295*/string id = "--\\NotAnId\\--"; 
		if (StartOf(4)) {
			Id(/*Parser.Statement.atg:297*/out id);
			Expect(_colon);
		} else if (la.kind == _lid) {
			Get();
			/*Parser.Statement.atg:298*/id = cache(t.val.Substring(0,t.val.Length-1)); 
		} else SynErr(133);
		/*Parser.Statement.atg:299*/block.Statements.Add(new AstExplicitLabel(this, id)); 
	}

	void SimpleStatement(/*Parser.Statement.atg:47*/AstBlock block) {
		if (la.kind == _goto) {
			ExplicitGoTo(/*Parser.Statement.atg:48*/block);
		} else if (la.kind == _declare) {
			Declaration();
		} else if (/*Parser.Statement.atg:51*/isVariableDeclaration() ) {
			VariableDeclarationStatement();
		} else if (StartOf(16)) {
			GetSetComplex(/*Parser.Statement.atg:52*/block);
		} else if (StartOf(34)) {
			Return(/*Parser.Statement.atg:53*/block);
		} else if (la.kind == _throw) {
			Throw(/*Parser.Statement.atg:54*/block);
		} else SynErr(134);
	}

	void StructureStatement(/*Parser.Statement.atg:58*/AstBlock block) {
		switch (la.kind) {
		case _asm: {
			/*Parser.Statement.atg:59*/_pushLexerState(Lexer.Asm); 
			Get();
			AsmStatementBlock(/*Parser.Statement.atg:60*/block);
			/*Parser.Statement.atg:61*/_popLexerState(); 
			break;
		}
		case _if: case _unless: {
			Condition(/*Parser.Statement.atg:62*/block);
			break;
		}
		case _do: case _while: case _until: {
			WhileLoop(/*Parser.Statement.atg:63*/block);
			break;
		}
		case _for: {
			ForLoop(/*Parser.Statement.atg:64*/block);
			break;
		}
		case _foreach: {
			ForeachLoop(/*Parser.Statement.atg:65*/block);
			break;
		}
		case _function: case _coroutine: {
			NestedFunction(/*Parser.Statement.atg:66*/block);
			break;
		}
		case _try: {
			TryCatchFinally(/*Parser.Statement.atg:67*/block);
			break;
		}
		case _uusing: {
			Using(/*Parser.Statement.atg:68*/block);
			break;
		}
		default: SynErr(135); break;
		}
	}

	void ExplicitGoTo(/*Parser.Statement.atg:302*/AstBlock block) {
		/*Parser.Statement.atg:302*/string id; 
		Expect(_goto);
		Id(/*Parser.Statement.atg:305*/out id);
		/*Parser.Statement.atg:305*/block.Statements.Add(new AstExplicitGoTo(this, id)); 
	}

	void VariableDeclarationStatement() {
		/*Parser.Statement.atg:237*/AstGetSetSymbol variable; 
		VariableDeclaration(/*Parser.Statement.atg:238*/out variable);
	}

	void GetSetComplex(/*Parser.Statement.atg:72*/AstBlock block) {
		/*Parser.Statement.atg:72*/AstGetSet complex = null; 
		AstGetSetSymbol symbol = null;
		bool isDeclaration = false;
		IAstExpression expr;
		
		if (StartOf(35)) {
			GetInitiator(/*Parser.Statement.atg:79*/out complex, out isDeclaration);
		} else if (la.kind == _lpar) {
			Get();
			Expr(/*Parser.Statement.atg:80*/out expr);
			Expect(_rpar);
			GetSetExtension(/*Parser.Statement.atg:81*/expr, out complex);
		} else SynErr(136);
		while (la.kind == _dot || la.kind == _lbrack) {
			GetSetExtension(/*Parser.Statement.atg:84*/complex, out complex);
		}
		if (la.kind == _rpar || la.kind == _semicolon) {
		} else if (la.kind == _inc) {
			Get();
			/*Parser.Statement.atg:88*/block.Add(new AstUnaryOperator(this, UnaryOperator.PostIncrement, complex));
			complex = null;
			
		} else if (la.kind == _dec) {
			Get();
			/*Parser.Statement.atg:91*/block.Add(new AstUnaryOperator(this, UnaryOperator.PostDecrement, complex));
			complex = null;
			
		} else if (StartOf(36)) {
			Assignment(/*Parser.Statement.atg:94*/complex);
			/*Parser.Statement.atg:94*/symbol = complex as AstGetSetSymbol;
			if(symbol != null && InterpretationIsVariable(symbol.Interpretation) && isDeclaration)
			    symbol.Interpretation = InterpretAsObjectVariable(symbol.Interpretation);
			
		} else SynErr(137);
		/*Parser.Statement.atg:100*/if(complex != null)
		   block.Add(complex);
		
	}

	void Return(/*Parser.Statement.atg:427*/AstBlock block) {
		/*Parser.Statement.atg:427*/AstReturn ret = null; 
		AstExplicitGoTo jump = null; 
		IAstExpression expr = null; 
		BlockLabels bl = target.CurrentBlock;
		
		if (la.kind == _return || la.kind == _yield) {
			if (la.kind == _return) {
				Get();
				/*Parser.Statement.atg:435*/ret = new AstReturn(this, ReturnVariant.Exit); 
			} else {
				Get();
				/*Parser.Statement.atg:436*/ret = new AstReturn(this, ReturnVariant.Continue); 
			}
			if (StartOf(37)) {
				if (StartOf(12)) {
					Expr(/*Parser.Statement.atg:438*/out expr);
					/*Parser.Statement.atg:438*/ret.Expression = expr; 
				} else {
					Get();
					/*Parser.Statement.atg:439*/ret.ReturnVariant = ReturnVariant.Set; 
					Expr(/*Parser.Statement.atg:440*/out expr);
					/*Parser.Statement.atg:440*/ret.Expression = expr; 
				}
			}
		} else if (la.kind == _break) {
			Get();
			/*Parser.Statement.atg:442*/if(bl == null)
			   ret = new AstReturn(this, ReturnVariant.Break); 
			else
			    jump = new AstExplicitGoTo(this, bl.BreakLabel);
			
		} else if (la.kind == _continue) {
			Get();
			/*Parser.Statement.atg:447*/if(bl == null)
			   ret = new AstReturn(this, ReturnVariant.Continue); 
			else
			    jump = new AstExplicitGoTo(this, bl.ContinueLabel);
			
		} else SynErr(138);
		/*Parser.Statement.atg:452*/block.Add((AstNode)ret ?? (AstNode)jump); 
	}

	void Throw(/*Parser.Statement.atg:517*/AstBlock block) {
		/*Parser.Statement.atg:517*/AstThrow th = new AstThrow(this); 
		Expect(_throw);
		Expr(/*Parser.Statement.atg:520*/out th.Expression);
		/*Parser.Statement.atg:521*/block.Add(th); 
	}

	void Condition(/*Parser.Statement.atg:326*/AstBlock block) {
		/*Parser.Statement.atg:326*/IAstExpression expr = null; bool isNegative = false; 
		if (la.kind == _if) {
			Get();
			/*Parser.Statement.atg:328*/isNegative = false; 
		} else if (la.kind == _unless) {
			Get();
			/*Parser.Statement.atg:329*/isNegative = true; 
		} else SynErr(139);
		Expect(_lpar);
		Expr(/*Parser.Statement.atg:332*/out expr);
		Expect(_rpar);
		/*Parser.Statement.atg:333*/AstCondition cond = new AstCondition(this, expr, isNegative); 
		StatementBlock(/*Parser.Statement.atg:335*/cond.IfBlock);
		if (la.kind == _else) {
			Get();
			StatementBlock(/*Parser.Statement.atg:338*/cond.ElseBlock);
		}
		/*Parser.Statement.atg:340*/block.Add(cond); 
	}

	void NestedFunction(/*Parser.Statement.atg:456*/AstBlock block) {
		/*Parser.Statement.atg:456*/PFunction func; 
		FunctionDefinition(/*Parser.Statement.atg:458*/out func);
		/*Parser.Statement.atg:460*/string logicalId = func.Meta["LogicalId"];
		string physicalId = func.Id;
		
		SmartDeclareLocal(logicalId, SymbolInterpretations.LocalReferenceVariable);
		AstGetSetSymbol setVar = new AstGetSetSymbol(this, PCall.Set, logicalId, SymbolInterpretations.LocalObjectVariable);
		setVar.Arguments.Add( new AstCreateClosure(this, physicalId) );
		block.Add(setVar);
		
	}

	void TryCatchFinally(/*Parser.Statement.atg:471*/AstBlock block) {
		/*Parser.Statement.atg:471*/AstTryCatchFinally a = new AstTryCatchFinally(this); 
		Expect(_try);
		Expect(_lbrace);
		while (StartOf(18)) {
			Statement(/*Parser.Statement.atg:475*/a.TryBlock);
		}
		Expect(_rbrace);
		if (la.kind == _catch || la.kind == _finally) {
			if (la.kind == _catch) {
				Get();
				if (la.kind == _lpar) {
					Get();
					GetCall(/*Parser.Statement.atg:480*/out a.ExceptionVar);
					Expect(_rpar);
				}
				Expect(_lbrace);
				while (StartOf(18)) {
					Statement(/*Parser.Statement.atg:484*/a.CatchBlock);
				}
				Expect(_rbrace);
				if (la.kind == _finally) {
					Get();
					Expect(_lbrace);
					while (StartOf(18)) {
						Statement(/*Parser.Statement.atg:491*/a.FinallyBlock);
					}
					Expect(_rbrace);
				}
			} else {
				Get();
				Expect(_lbrace);
				while (StartOf(18)) {
					Statement(/*Parser.Statement.atg:498*/a.FinallyBlock);
				}
				Expect(_rbrace);
				if (la.kind == _catch) {
					Get();
					if (la.kind == _lpar) {
						Get();
						GetCall(/*Parser.Statement.atg:504*/out a.ExceptionVar);
						Expect(_rpar);
					}
					Expect(_lbrace);
					while (StartOf(18)) {
						Statement(/*Parser.Statement.atg:508*/a.CatchBlock);
					}
					Expect(_rbrace);
				}
			}
		}
		/*Parser.Statement.atg:513*/block.Add(a); 
	}

	void Using(/*Parser.Statement.atg:525*/AstBlock block) {
		/*Parser.Statement.atg:525*/AstUsing use = new AstUsing(this); 
		Expect(_uusing);
		Expect(_lpar);
		GetCall(/*Parser.Statement.atg:527*/out use.Container);
		Expect(_assign);
		Expr(/*Parser.Statement.atg:527*/out use.Expression);
		Expect(_rpar);
		StatementBlock(/*Parser.Statement.atg:528*/use.Block);
		/*Parser.Statement.atg:529*/block.Add(use); 
	}

	void Assignment(/*Parser.Statement.atg:309*/AstGetSet lvalue) {
		/*Parser.Statement.atg:309*/IAstExpression expr; BinaryOperator setModifier = BinaryOperator.None; 
		switch (la.kind) {
		case _assign: {
			Get();
			/*Parser.Statement.atg:311*/setModifier = BinaryOperator.None; 
			break;
		}
		case _plus: {
			Get();
			Expect(_assign);
			/*Parser.Statement.atg:312*/setModifier = BinaryOperator.Addition; 
			break;
		}
		case _minus: {
			Get();
			Expect(_assign);
			/*Parser.Statement.atg:313*/setModifier = BinaryOperator.Subtraction; 
			break;
		}
		case _times: {
			Get();
			Expect(_assign);
			/*Parser.Statement.atg:314*/setModifier = BinaryOperator.Multiply; 
			break;
		}
		case _div: {
			Get();
			Expect(_assign);
			/*Parser.Statement.atg:315*/setModifier = BinaryOperator.Division; 
			break;
		}
		case _bitAnd: {
			Get();
			Expect(_assign);
			/*Parser.Statement.atg:316*/setModifier = BinaryOperator.BitwiseAnd; 
			break;
		}
		case _bitOr: {
			Get();
			Expect(_assign);
			/*Parser.Statement.atg:317*/setModifier = BinaryOperator.BitwiseOr; 
			break;
		}
		case _coalescence: {
			Get();
			Expect(_assign);
			/*Parser.Statement.atg:318*/setModifier = BinaryOperator.Coalescence; 
			break;
		}
		default: SynErr(140); break;
		}
		Expr(/*Parser.Statement.atg:319*/out expr);
		/*Parser.Statement.atg:319*/lvalue.Arguments.Add(expr);
		lvalue.Call = PCall.Set; 
		lvalue.SetModifier = setModifier;
		
	}

	void Function(/*Parser.Statement.atg:244*/out AstGetSetSymbol function) {
		/*Parser.Statement.atg:244*/function = null; string id; 
		Id(/*Parser.Statement.atg:246*/out id);
		/*Parser.Statement.atg:246*/if(!target.Symbols.ContainsKey(id))
		{
		    SemErr("There is no function-like symbol named " + id + ".");
		    function = new AstGetSetSymbol(this, id, SymbolInterpretations.Function);
		}
		else
		{
		    if(isOuterVariable(id))
		        target.RequireOuterVariable(id);
		    SymbolEntry sym = target.Symbols[id];
		    function = new AstGetSetSymbol(this, sym.Id, sym.Interpretation);
		}
		List<IAstExpression> args; 
		if (la.kind == _lpar) {
			Arguments(/*Parser.Statement.atg:259*/out args);
			/*Parser.Statement.atg:259*/function.Arguments.AddRange(args); 
		}
	}

	void Variable(/*Parser.Statement.atg:214*/out AstGetSetSymbol variable, out bool isDeclared) {
		/*Parser.Statement.atg:214*/variable = null; string id; isDeclared = false; 
		if (la.kind == _var || la.kind == _ref || la.kind == _static) {
			VariableDeclaration(/*Parser.Statement.atg:216*/out variable);
			/*Parser.Statement.atg:216*/isDeclared = true; 
		} else if (StartOf(4)) {
			Id(/*Parser.Statement.atg:217*/out id);
			/*Parser.Statement.atg:217*/if(target.Symbols.ContainsKey(id))
			{
			    SymbolEntry varSym = target.Symbols[id];
			    if(InterpretationIsVariable(varSym.Interpretation))
			    {
			        if(isOuterVariable(id))
			            target.RequireOuterVariable(id);
			        variable = new AstGetSetSymbol(this, varSym.Id, varSym.Interpretation);
			    }
			    else
			        SemErr(t.line, t.col, "Variable name expected");
			}
			else
			{
			    SemErr(t.line, t.col, "Unkown symbol \"" + id + "\". Variable name expected.");
			}
			
		} else SynErr(141);
	}

	void StaticCall(/*Parser.Statement.atg:264*/out AstGetSetStatic staticCall) {
		/*Parser.Statement.atg:264*/IAstType typeExpr;
		string memberId;
		staticCall = null;
		List<IAstExpression> args;
		
		ExplicitTypeExpr(/*Parser.Statement.atg:270*/out typeExpr);
		Expect(_dot);
		Id(/*Parser.Statement.atg:271*/out memberId);
		/*Parser.Statement.atg:271*/staticCall = new AstGetSetStatic(this, PCall.Get, typeExpr, memberId); 
		if (la.kind == _lpar) {
			Arguments(/*Parser.Statement.atg:272*/out args);
			/*Parser.Statement.atg:272*/staticCall.Arguments.AddRange(args); 
		}
	}

	void VariableDeclaration(/*Parser.Statement.atg:188*/out AstGetSetSymbol variable) {
		/*Parser.Statement.atg:188*/variable = null; string staticId = null; 
		/*Parser.Statement.atg:189*/string id = null; SymbolInterpretations kind = SymbolInterpretations.Undefined; 
		if (la.kind == _var || la.kind == _ref) {
			if (la.kind == _var) {
				Get();
				/*Parser.Statement.atg:190*/kind = SymbolInterpretations.LocalObjectVariable; 
			} else {
				Get();
				/*Parser.Statement.atg:191*/kind = SymbolInterpretations.LocalReferenceVariable; 
			}
			Id(/*Parser.Statement.atg:194*/out id);
			/*Parser.Statement.atg:195*/SmartDeclareLocal(id, kind);
			staticId = id; 
			
		} else if (la.kind == _static) {
			Get();
			/*Parser.Statement.atg:198*/kind = SymbolInterpretations.GlobalObjectVariable; 
			if (la.kind == _var || la.kind == _ref) {
				if (la.kind == _var) {
					Get();
				} else {
					Get();
					/*Parser.Statement.atg:200*/kind = SymbolInterpretations.GlobalReferenceVariable; 
				}
			}
			Id(/*Parser.Statement.atg:202*/out id);
			/*Parser.Statement.atg:202*/staticId = target.Function.Id + "\\static\\" + id;
			target.Declare(kind, id, staticId);
			if(!target.Loader.Options.TargetApplication.Variables.ContainsKey(staticId))
			    target.Loader.Options.TargetApplication.Variables.Add(staticId, new PVariable(staticId));
			
		} else SynErr(142);
		/*Parser.Statement.atg:207*/variable = InterpretationIsObjectVariable(kind) ?
		new AstGetSetSymbol(this, PCall.Get, staticId, kind)
		:
			new AstGetSetReference(this, PCall.Get, staticId, InterpretAsObjectVariable(kind)); 
	}

	void GetCall(/*Parser.Statement.atg:407*/out AstGetSet complex) {
		/*Parser.Statement.atg:407*/AstGetSet getMember = null; bool isDeclaration; 
		GetInitiator(/*Parser.Statement.atg:409*/out complex, out isDeclaration);
		while (la.kind == _dot || la.kind == _lbrack) {
			GetSetExtension(/*Parser.Statement.atg:410*/complex, out getMember);
		}
		/*Parser.Statement.atg:412*/if(getMember != null) 
		{
		    complex = getMember; 
		}
		else
		{
		    AstGetSetSymbol symbol = complex as AstGetSetSymbol;
		    if(symbol != null && InterpretationIsVariable(symbol.Interpretation) && isDeclaration)
		    {
		        symbol.Interpretation = InterpretAsObjectVariable(symbol.Interpretation);
		        complex = symbol;
		    }                                        
		} 
	}


#line 121 "F:\DotNetProjects\Prexonite\Tools\Parser.frame" //FRAME


	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();

#line default //END FRAME -->parseRoot

		Prexonite();

#line 127 "F:\DotNetProjects\Prexonite\Tools\Parser.frame" //FRAME

    Expect(0);
	}
	
	bool[,] set = {

#line default //END FRAME -->initialization

		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,x, x,x,T,T, T,T,T,T, x,T,x,x, T,T,T,T, T,T,T,x, T,T,T,x, T,x,T,T, T,T,T,x, x,x,x,T, T,T,T,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,T,T,T, x,T,x,x, T,T,T,T, T,T,T,x, T,T,T,x, T,x,T,T, T,T,T,x, x,x,x,T, T,T,T,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,T,x, x,T,x,x, x,x,x,x, T,T,T,x, x,x,T,T, T,T,T,T, x,T,x,x, T,T,T,T, T,T,T,x, T,T,T,x, T,x,T,T, T,T,T,x, x,x,x,T, T,T,T,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,T,x,x, x,T,x,x, x,T,T,T, x,T,T,x, T,T,T,x, T,x,T,T, T,T,T,x, x,x,x,T, T,T,T,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, T,T,T,T, x,x,x,T, x,x,x,x, x,T,T,T, T,x,x,T, x,x,T,x, x,x,x,T, x,x,x,T, x,x,T,x, x,T,T,T, T,x,x,x, x,T,T,T, x,x,T,x, T,x,x,x, T,x,x,x, x,x,x,x, x,T,T,x, x,x,T,T, x,T,x,T, T,T,T,x, x,x,x,x, x,T,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,T, x,T,x,x, x,T,x,T, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,T,x,x, x,x,x,T, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, T,T,T,T, x,x,x,T, x,x,x,x, x,T,T,T, T,x,x,T, x,x,T,x, x,x,x,T, x,x,x,T, x,x,T,x, x,T,T,T, T,x,x,x, x,T,T,T, x,x,T,x, T,x,x,x, T,x,x,x, x,x,x,x, x,T,T,T, T,x,T,T, x,T,x,T, T,T,T,x, x,x,x,x, x,T,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,T, T,T,T,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,T, x,x,x,T, x,x,T,x, x,T,T,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,T, x,T,x,T, x,x,T,x, x,T,T,x, x,x,x,x, x,x,T,T, T,x,T,T, T,T,x,x, T,T,T,T, x,x,x,x, T,T,x,T, T,x,x,T, x,T,x,T, T,T,T,T, x,x,T,T, x,x,x,x},
		{x,T,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,T, x,x,x,x, x,T,T,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,T, x,T,x,T, x,x,T,x, x,T,T,x, x,x,x,x, x,x,T,T, x,x,x,T, T,T,x,x, T,T,T,T, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x},
		{x,T,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,T, x,x,x,T, x,x,T,x, x,T,T,x, x,x,x,x, x,x,T,T, x,x,x,T, T,T,x,x, T,T,T,T, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,T, x,T,x,T, T,T,T,T, x,x,x,T, x,x,x,x},
		{x,x,x,x, T,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,T, x,x,x,T, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,T,T,x, x,x,x,T, x,x,x,x, T,x,x,T, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,T,T,x, x,x,x,T, x,x,x,x, x,x,x,T, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,T, x,x,x,x, x,x,x,T, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, T,T,T,T, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,T, x,x,T,x, x,T,T,x, x,x,x,x, x,x,T,T, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,x,x,x, x,x,x,x, T,T,x,x, T,x,x,x, x,x,x,x, x,x,x,T, x,T,T,x, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,x, T,T,T,T, x,T,x,T, x,x,x,x, x,T,T,T, T,x,x,T, x,x,T,x, x,x,x,T, x,x,x,T, x,x,T,x, x,T,T,T, T,x,x,x, x,T,T,T, x,x,T,x, T,x,x,x, T,x,x,x, x,x,x,x, x,T,T,T, T,x,T,T, x,T,x,T, T,T,T,x, x,x,x,x, x,T,x,x}

#line 132 "F:\DotNetProjects\Prexonite\Tools\Parser.frame" //FRAME

	};
} // end Parser

[NoDebug()]
internal class Errors {
	internal int count = 0;                                    // number of errors detected
	internal System.IO.TextWriter errorStream = Console.Out;   // error messages go to this stream
    internal string errMsgFormat = "-- line {0} col {1}: {2}"; // 0=line, 1=column, 2=text
    internal Parser parentParser;
  
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
			case 41: s = "var expected"; break;
			case 42: s = "ref expected"; break;
			case 43: s = "true expected"; break;
			case 44: s = "false expected"; break;
			case 45: s = "BEGINKEYWORDS expected"; break;
			case 46: s = "mod expected"; break;
			case 47: s = "is expected"; break;
			case 48: s = "as expected"; break;
			case 49: s = "not expected"; break;
			case 50: s = "enabled expected"; break;
			case 51: s = "disabled expected"; break;
			case 52: s = "function expected"; break;
			case 53: s = "command expected"; break;
			case 54: s = "asm expected"; break;
			case 55: s = "declare expected"; break;
			case 56: s = "build expected"; break;
			case 57: s = "return expected"; break;
			case 58: s = "in expected"; break;
			case 59: s = "to expected"; break;
			case 60: s = "add expected"; break;
			case 61: s = "continue expected"; break;
			case 62: s = "break expected"; break;
			case 63: s = "yield expected"; break;
			case 64: s = "or expected"; break;
			case 65: s = "and expected"; break;
			case 66: s = "xor expected"; break;
			case 67: s = "label expected"; break;
			case 68: s = "goto expected"; break;
			case 69: s = "static expected"; break;
			case 70: s = "null expected"; break;
			case 71: s = "if expected"; break;
			case 72: s = "unless expected"; break;
			case 73: s = "else expected"; break;
			case 74: s = "new expected"; break;
			case 75: s = "coroutine expected"; break;
			case 76: s = "from expected"; break;
			case 77: s = "do expected"; break;
			case 78: s = "does expected"; break;
			case 79: s = "while expected"; break;
			case 80: s = "until expected"; break;
			case 81: s = "for expected"; break;
			case 82: s = "foreach expected"; break;
			case 83: s = "try expected"; break;
			case 84: s = "catch expected"; break;
			case 85: s = "finally expected"; break;
			case 86: s = "throw expected"; break;
			case 87: s = "uusing expected"; break;
			case 88: s = "ENDKEYWORDS expected"; break;
			case 89: s = "LPopExpr expected"; break;
			case 90: s = "??? expected"; break;
			case 91: s = "invalid AsmStatementBlock"; break;
			case 92: s = "invalid AsmInstruction"; break;
			case 93: s = "invalid AsmInstruction"; break;
			case 94: s = "invalid AsmInstruction"; break;
			case 95: s = "invalid AsmInstruction"; break;
			case 96: s = "invalid AsmInstruction"; break;
			case 97: s = "invalid AsmId"; break;
			case 98: s = "invalid SignedReal"; break;
			case 99: s = "invalid Boolean"; break;
			case 100: s = "invalid Id"; break;
			case 101: s = "invalid AtomicExpr"; break;
			case 102: s = "invalid TypeExpr"; break;
			case 103: s = "invalid GetSetExtension"; break;
			case 104: s = "invalid Primary"; break;
			case 105: s = "invalid Constant"; break;
			case 106: s = "invalid LoopExpr"; break;
			case 107: s = "invalid LambdaExpression"; break;
			case 108: s = "invalid LambdaExpression"; break;
			case 109: s = "invalid GetInitiator"; break;
			case 110: s = "invalid WhileLoop"; break;
			case 111: s = "invalid WhileLoop"; break;
			case 112: s = "invalid ForLoop"; break;
			case 113: s = "invalid Statement"; break;
			case 114: s = "invalid ExplicitTypeExpr"; break;
			case 115: s = "invalid PrexoniteTypeExpr"; break;
			case 116: s = "invalid ClrTypeExpr"; break;
			case 117: s = "invalid TypeExprElement"; break;
			case 118: s = "this symbol not expected in Prexonite"; break;
			case 119: s = "this symbol not expected in GlobalVariableDefinition"; break;
			case 120: s = "invalid GlobalVariableDefinition"; break;
			case 121: s = "this symbol not expected in GlobalVariableDefinition"; break;
			case 122: s = "this symbol not expected in Declaration"; break;
			case 123: s = "invalid MetaAssignment"; break;
			case 124: s = "invalid MetaAssignment"; break;
			case 125: s = "this symbol not expected in BuildBlock"; break;
			case 126: s = "invalid FunctionDefinition"; break;
			case 127: s = "this symbol not expected in FunctionDefinition"; break;
			case 128: s = "invalid FunctionDefinition"; break;
			case 129: s = "invalid GlobalId"; break;
			case 130: s = "invalid MetaExpr"; break;
			case 131: s = "invalid GlobalQualifiedId"; break;
			case 132: s = "invalid StatementBlock"; break;
			case 133: s = "invalid ExplicitLabel"; break;
			case 134: s = "invalid SimpleStatement"; break;
			case 135: s = "invalid StructureStatement"; break;
			case 136: s = "invalid GetSetComplex"; break;
			case 137: s = "invalid GetSetComplex"; break;
			case 138: s = "invalid Return"; break;
			case 139: s = "invalid Condition"; break;
			case 140: s = "invalid Assignment"; break;
			case 141: s = "invalid Variable"; break;
			case 142: s = "invalid VariableDeclaration"; break;

#line 146 "F:\DotNetProjects\Prexonite\Tools\Parser.frame" //FRAME

			default: s = "error " + n; break;
		}
		if(s.EndsWith(" expected"))
		    s += " and not \"" + parentParser.t.ToString(false) + " " + parentParser.la.ToString(false) + "\"";
		else if(s.StartsWith("this symbol "))
		    s = "\"" + parentParser.t.val + "\"" + s.Substring(12);
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}

	internal void SemErr (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}
	
	internal void SemErr (string s) {
		errorStream.WriteLine(s);
		count++;
	}
	
	internal void Warning (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
	}
	
	internal void Warning(string s) {
		errorStream.WriteLine(s);
	}
} // Errors


#line default //END FRAME $$$

}