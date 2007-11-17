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
	public const int _uusing = 89;
	public const int _ENDKEYWORDS = 90;
	public const int _LPopExpr = 91;
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
		@uusing = 89,
		@ENDKEYWORDS = 90,
		@LPopExpr = 91,
	}
	const int maxT = 92;

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
		} else SynErr(93);
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
		int values = 0;
		int rotations = 0;
		int index = 0;
		
		if (la.kind == _var || la.kind == _ref) {
			/*Parser.Assembler.atg:51*/SymbolInterpretations kind = SymbolInterpretations.LocalObjectVariable; 
			if (la.kind == _var) {
				Get();
			} else {
				Get();
				/*Parser.Assembler.atg:52*/kind = SymbolInterpretations.LocalReferenceVariable; 
			}
			AsmId(/*Parser.Assembler.atg:54*/out id);
			/*Parser.Assembler.atg:57*/target.Function.Variables.Add(id);
			target.Symbols.Add(id, new SymbolEntry(kind, id));
			
			while (la.kind == _comma) {
				Get();
				AsmId(/*Parser.Assembler.atg:61*/out id);
				/*Parser.Assembler.atg:63*/target.Function.Variables.Add(id);
				target.Symbols.Add(id, new SymbolEntry(kind, id));
				
			}
		} else if (/*Parser.Assembler.atg:69*/isInNullGroup()) {
			AsmId(/*Parser.Assembler.atg:69*/out insbase);
			if (la.kind == _dot) {
				Get();
				AsmId(/*Parser.Assembler.atg:70*/out detail);
			}
			/*Parser.Assembler.atg:71*/code = getOpCode(insbase, detail);
			addInstruction(block, new Instruction(code));
			
		} else if (/*Parser.Assembler.atg:77*/isAsmInstruction("label",null) ) {
			AsmId(/*Parser.Assembler.atg:77*/out insbase);
			AsmId(/*Parser.Assembler.atg:80*/out id);
			/*Parser.Assembler.atg:81*/addLabel(block, id); 
		} else if (/*Parser.Assembler.atg:84*/isAsmInstruction("nop", null)) {
			AsmId(/*Parser.Assembler.atg:84*/out insbase);
			/*Parser.Assembler.atg:84*/Instruction ins = new Instruction(OpCode.nop); 
			if (la.kind == _plus) {
				Get();
				AsmId(/*Parser.Assembler.atg:85*/out id);
				/*Parser.Assembler.atg:85*/ins.Id = id; 
			}
			/*Parser.Assembler.atg:87*/addInstruction(block, ins); 
		} else if (/*Parser.Assembler.atg:91*/isAsmInstruction("rot", null)) {
			AsmId(/*Parser.Assembler.atg:91*/out insbase);
			Expect(_dot);
			Integer(/*Parser.Assembler.atg:92*/out rotations);
			Expect(_comma);
			Integer(/*Parser.Assembler.atg:93*/out values);
			/*Parser.Assembler.atg:95*/addInstruction(block, Instruction.CreateRotate(rotations, values)); 
		} else if (/*Parser.Assembler.atg:99*/isAsmInstruction("indloci", null)) {
			AsmId(/*Parser.Assembler.atg:99*/out insbase);
			Expect(_dot);
			Integer(/*Parser.Assembler.atg:100*/out arguments);
			Integer(/*Parser.Assembler.atg:101*/out index);
			/*Parser.Assembler.atg:103*/addInstruction(block, Instruction.CreateIndLocI(index, arguments)); 
		} else if (/*Parser.Assembler.atg:106*/isAsmInstruction("swap", null)) {
			AsmId(/*Parser.Assembler.atg:106*/out insbase);
			/*Parser.Assembler.atg:107*/addInstruction(block, Instruction.CreateExchange()); 
		} else if (/*Parser.Assembler.atg:112*/isAsmInstruction("ldc", "real")) {
			AsmId(/*Parser.Assembler.atg:112*/out insbase);
			Expect(_dot);
			AsmId(/*Parser.Assembler.atg:114*/out detail);
			SignedReal(/*Parser.Assembler.atg:115*/out dblArg);
			/*Parser.Assembler.atg:116*/addInstruction(block, Instruction.CreateConstant(dblArg)); 
		} else if (/*Parser.Assembler.atg:121*/isAsmInstruction("ldc", "bool")) {
			AsmId(/*Parser.Assembler.atg:121*/out insbase);
			Expect(_dot);
			AsmId(/*Parser.Assembler.atg:123*/out detail);
			Boolean(/*Parser.Assembler.atg:124*/out bolArg);
			/*Parser.Assembler.atg:125*/addInstruction(block, Instruction.CreateConstant(bolArg)); 
		} else if (/*Parser.Assembler.atg:130*/isInIntegerGroup()) {
			AsmId(/*Parser.Assembler.atg:130*/out insbase);
			if (la.kind == _dot) {
				Get();
				AsmId(/*Parser.Assembler.atg:131*/out detail);
			}
			SignedInteger(/*Parser.Assembler.atg:132*/out arguments);
			/*Parser.Assembler.atg:133*/code = getOpCode(insbase, detail);
			addInstruction(block, new Instruction(code, arguments));
			
		} else if (/*Parser.Assembler.atg:139*/isInJumpGroup()) {
			AsmId(/*Parser.Assembler.atg:139*/out insbase);
			if (la.kind == _dot) {
				Get();
				AsmId(/*Parser.Assembler.atg:140*/out detail);
			}
			/*Parser.Assembler.atg:141*/Instruction ins = null;
			code = getOpCode(insbase, detail);
			
			if (StartOf(2)) {
				AsmId(/*Parser.Assembler.atg:145*/out id);
				/*Parser.Assembler.atg:147*/ins = new Instruction(code, -1, id);
				
			} else if (la.kind == _integer) {
				Integer(/*Parser.Assembler.atg:149*/out arguments);
				/*Parser.Assembler.atg:149*/ins = new Instruction(code, arguments); 
			} else SynErr(94);
			/*Parser.Assembler.atg:150*/addInstruction(block, ins); 
		} else if (/*Parser.Assembler.atg:155*/isInIdGroup()) {
			AsmId(/*Parser.Assembler.atg:155*/out insbase);
			if (la.kind == _dot) {
				Get();
				AsmId(/*Parser.Assembler.atg:156*/out detail);
			}
			AsmId(/*Parser.Assembler.atg:157*/out id);
			/*Parser.Assembler.atg:158*/code = getOpCode(insbase, detail);
			addInstruction(block, new Instruction(code, id));
			
		} else if (/*Parser.Assembler.atg:165*/isInIdArgGroup()) {
			if (la.kind == _at) {
				Get();
				/*Parser.Assembler.atg:165*/justEffect = true; 
			}
			AsmId(/*Parser.Assembler.atg:167*/out insbase);
			if (la.kind == _dot) {
				Get();
				Integer(/*Parser.Assembler.atg:168*/out arguments);
			} else if (StartOf(2)) {
				/*Parser.Assembler.atg:169*/arguments = 0; 
			} else SynErr(95);
			AsmId(/*Parser.Assembler.atg:171*/out id);
			/*Parser.Assembler.atg:172*/code = getOpCode(insbase, detail);
			addInstruction(block, new Instruction(code, arguments, id, justEffect));
			
		} else if (/*Parser.Assembler.atg:178*/isInArgGroup()) {
			if (la.kind == _at) {
				Get();
				/*Parser.Assembler.atg:178*/justEffect = true; 
			}
			AsmId(/*Parser.Assembler.atg:180*/out insbase);
			if (la.kind == _dot) {
				Get();
				Integer(/*Parser.Assembler.atg:181*/out arguments);
			} else if (StartOf(3)) {
				/*Parser.Assembler.atg:182*/arguments = 0; 
			} else SynErr(96);
			/*Parser.Assembler.atg:184*/code = getOpCode(insbase, detail);
			addInstruction(block, new Instruction(code, arguments, null, justEffect));
			
		} else if (/*Parser.Assembler.atg:190*/isInQualidArgGroup()) {
			if (la.kind == _at) {
				Get();
				/*Parser.Assembler.atg:190*/justEffect = true; 
			}
			AsmId(/*Parser.Assembler.atg:192*/out insbase);
			if (la.kind == _dot) {
				Get();
				Integer(/*Parser.Assembler.atg:193*/out arguments);
			} else if (StartOf(2)) {
				/*Parser.Assembler.atg:194*/arguments = 0; 
			} else SynErr(97);
			AsmQualid(/*Parser.Assembler.atg:196*/out id);
			/*Parser.Assembler.atg:197*/code = getOpCode(insbase, detail);
			addInstruction(block, new Instruction(code, arguments, id, justEffect));
			
		} else if (StartOf(2)) {
			AsmId(/*Parser.Assembler.atg:202*/out insbase);
			/*Parser.Assembler.atg:202*/SemErr("Invalid assembler instruction \"" + insbase + "\" (" + t + ")."); 
		} else SynErr(98);
	}

	void AsmId(/*Parser.Assembler.atg:206*/out string id) {
		/*Parser.Assembler.atg:206*/id = "\\NoId\\"; 
		if (la.kind == _string) {
			String(/*Parser.Assembler.atg:208*/out id);
		} else if (StartOf(4)) {
			Id(/*Parser.Assembler.atg:209*/out id);
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
			/*Parser.Assembler.atg:240*/id = cache(t.val); 
		} else SynErr(99);
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
		} else SynErr(100);
		/*Parser.Helper.atg:83*/value = modifier * value; 
	}

	void Boolean(/*Parser.Helper.atg:40*/out bool value) {
		/*Parser.Helper.atg:40*/value = true; 
		if (la.kind == _true) {
			Get();
		} else if (la.kind == _false) {
			Get();
			/*Parser.Helper.atg:43*/value = false; 
		} else SynErr(101);
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

	void AsmQualid(/*Parser.Assembler.atg:244*/out string qualid) {
		
		AsmId(/*Parser.Assembler.atg:246*/out qualid);
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
		} else SynErr(102);
	}

	void Expr(/*Parser.Expression.atg:28*/out IAstExpression expr) {
		/*Parser.Expression.atg:28*/AstGetSet complex = null; 
		KeyValuePairExpr(/*Parser.Expression.atg:30*/out expr);
		while (la.kind == _appendright) {
			Get();
			GetCall(/*Parser.Expression.atg:33*/out complex);
			/*Parser.Expression.atg:33*/complex.Arguments.RightAppend(expr); 
			complex.Arguments.ReleaseRightAppend();
			expr = complex;
			
		}
	}

	void KeyValuePairExpr(/*Parser.Expression.atg:41*/out IAstExpression expr) {
		AtomicExpr(/*Parser.Expression.atg:42*/out expr);
		if (la.kind == _colon) {
			Get();
			/*Parser.Expression.atg:43*/IAstExpression value; 
			Expr(/*Parser.Expression.atg:44*/out value);
			/*Parser.Expression.atg:44*/expr = new AstKeyValuePair(this, expr, value); 
		}
	}

	void GetCall(/*Parser.Statement.atg:427*/out AstGetSet complex) {
		/*Parser.Statement.atg:427*/AstGetSet getMember = null; bool isDeclaration; 
		GetInitiator(/*Parser.Statement.atg:429*/out complex, out isDeclaration);
		while (la.kind == _dot || la.kind == _lbrack) {
			GetSetExtension(/*Parser.Statement.atg:430*/complex, out getMember);
		}
		/*Parser.Statement.atg:432*/if(getMember != null) 
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

	void AtomicExpr(/*Parser.Expression.atg:48*/out IAstExpression expr) {
		/*Parser.Expression.atg:48*/AstConditionalExpression cexpr = null; expr = null; 
		if (StartOf(7)) {
			OrExpr(/*Parser.Expression.atg:50*/out expr);
			while (la.kind == _question) {
				Get();
				/*Parser.Expression.atg:52*/cexpr = new AstConditionalExpression(this, expr); 
				AtomicExpr(/*Parser.Expression.atg:53*/out cexpr.IfExpression);
				Expect(_colon);
				AtomicExpr(/*Parser.Expression.atg:55*/out cexpr.ElseExpression);
				/*Parser.Expression.atg:55*/expr = cexpr; 
			}
		} else if (la.kind == _if || la.kind == _unless) {
			/*Parser.Expression.atg:57*/bool isNegated = false; 
			if (la.kind == _if) {
				Get();
			} else {
				Get();
				/*Parser.Expression.atg:59*/isNegated = true; 
			}
			Expect(_lpar);
			OrExpr(/*Parser.Expression.atg:61*/out expr);
			Expect(_rpar);
			/*Parser.Expression.atg:61*/cexpr = new AstConditionalExpression(this, expr, isNegated); 
			AtomicExpr(/*Parser.Expression.atg:62*/out cexpr.IfExpression);
			Expect(_else);
			AtomicExpr(/*Parser.Expression.atg:64*/out cexpr.ElseExpression);
			/*Parser.Expression.atg:64*/expr = cexpr; 
		} else SynErr(103);
	}

	void OrExpr(/*Parser.Expression.atg:68*/out IAstExpression expr) {
		/*Parser.Expression.atg:68*/IAstExpression lhs, rhs; 
		AndExpr(/*Parser.Expression.atg:70*/out lhs);
		/*Parser.Expression.atg:70*/expr = lhs; 
		if (la.kind == _or) {
			Get();
			OrExpr(/*Parser.Expression.atg:71*/out rhs);
			/*Parser.Expression.atg:71*/expr = new AstLogicalOr(this, lhs, rhs); 
		}
	}

	void AndExpr(/*Parser.Expression.atg:77*/out IAstExpression expr) {
		/*Parser.Expression.atg:77*/IAstExpression lhs, rhs; 
		BitOrExpr(/*Parser.Expression.atg:79*/out lhs);
		/*Parser.Expression.atg:79*/expr = lhs; 
		if (la.kind == _and) {
			Get();
			AndExpr(/*Parser.Expression.atg:80*/out rhs);
			/*Parser.Expression.atg:80*/expr = new AstLogicalAnd(this, lhs, rhs); 
		}
	}

	void BitOrExpr(/*Parser.Expression.atg:85*/out IAstExpression expr) {
		/*Parser.Expression.atg:85*/IAstExpression lhs, rhs; 
		BitXorExpr(/*Parser.Expression.atg:87*/out lhs);
		/*Parser.Expression.atg:87*/expr = lhs; 
		while (la.kind == _bitAnd) {
			Get();
			BitXorExpr(/*Parser.Expression.atg:88*/out rhs);
			/*Parser.Expression.atg:88*/expr = new AstBinaryOperator(this, expr, BinaryOperator.BitwiseOr, rhs); 
		}
	}

	void BitXorExpr(/*Parser.Expression.atg:93*/out IAstExpression expr) {
		/*Parser.Expression.atg:93*/IAstExpression lhs, rhs; 
		BitAndExpr(/*Parser.Expression.atg:95*/out lhs);
		/*Parser.Expression.atg:95*/expr = lhs; 
		while (la.kind == _xor) {
			Get();
			BitAndExpr(/*Parser.Expression.atg:96*/out rhs);
			/*Parser.Expression.atg:97*/expr = new AstBinaryOperator(this, expr, BinaryOperator.ExclusiveOr, rhs); 
		}
	}

	void BitAndExpr(/*Parser.Expression.atg:102*/out IAstExpression expr) {
		/*Parser.Expression.atg:102*/IAstExpression lhs, rhs; 
		NotExpr(/*Parser.Expression.atg:104*/out lhs);
		/*Parser.Expression.atg:104*/expr = lhs; 
		while (la.kind == _bitAnd) {
			Get();
			NotExpr(/*Parser.Expression.atg:105*/out rhs);
			/*Parser.Expression.atg:106*/expr = new AstBinaryOperator(this, expr, BinaryOperator.BitwiseAnd, rhs); 
		}
	}

	void NotExpr(/*Parser.Expression.atg:111*/out IAstExpression expr) {
		/*Parser.Expression.atg:111*/IAstExpression lhs; bool isNot = false; 
		if (la.kind == _not) {
			Get();
			/*Parser.Expression.atg:113*/isNot = true; 
		}
		EqlExpr(/*Parser.Expression.atg:115*/out lhs);
		/*Parser.Expression.atg:115*/expr = isNot ? new AstUnaryOperator(this, UnaryOperator.LogicalNot, lhs) : lhs; 
	}

	void EqlExpr(/*Parser.Expression.atg:119*/out IAstExpression expr) {
		/*Parser.Expression.atg:119*/IAstExpression lhs, rhs; BinaryOperator op = BinaryOperator.None; 
		RelExpr(/*Parser.Expression.atg:121*/out lhs);
		/*Parser.Expression.atg:121*/expr = lhs; 
		while (la.kind == _eq || la.kind == _ne) {
			if (la.kind == _eq) {
				Get();
				/*Parser.Expression.atg:122*/op = BinaryOperator.Equality; 
			} else {
				Get();
				/*Parser.Expression.atg:123*/op = BinaryOperator.Inequality; 
			}
			RelExpr(/*Parser.Expression.atg:124*/out rhs);
			/*Parser.Expression.atg:124*/expr = new AstBinaryOperator(this, expr, op, rhs); 
		}
	}

	void RelExpr(/*Parser.Expression.atg:129*/out IAstExpression expr) {
		/*Parser.Expression.atg:129*/IAstExpression lhs, rhs; BinaryOperator op = BinaryOperator.None;  
		CoalExpr(/*Parser.Expression.atg:131*/out lhs);
		/*Parser.Expression.atg:131*/expr = lhs; 
		while (StartOf(8)) {
			if (la.kind == _lt) {
				Get();
				/*Parser.Expression.atg:132*/op = BinaryOperator.LessThan;              
			} else if (la.kind == _le) {
				Get();
				/*Parser.Expression.atg:133*/op = BinaryOperator.LessThanOrEqual;       
			} else if (la.kind == _gt) {
				Get();
				/*Parser.Expression.atg:134*/op = BinaryOperator.GreaterThan;           
			} else {
				Get();
				/*Parser.Expression.atg:135*/op = BinaryOperator.GreaterThanOrEqual;    
			}
			CoalExpr(/*Parser.Expression.atg:136*/out rhs);
			/*Parser.Expression.atg:136*/expr = new AstBinaryOperator(this, expr, op, rhs); 
		}
	}

	void CoalExpr(/*Parser.Expression.atg:141*/out IAstExpression expr) {
		/*Parser.Expression.atg:141*/IAstExpression lhs, rhs; AstCoalescence coal = new AstCoalescence(this); 
		AddExpr(/*Parser.Expression.atg:143*/out lhs);
		/*Parser.Expression.atg:143*/expr = lhs; coal.Expressions.Add(lhs); 
		while (la.kind == _coalescence) {
			Get();
			AddExpr(/*Parser.Expression.atg:146*/out rhs);
			/*Parser.Expression.atg:146*/expr = coal; coal.Expressions.Add(rhs); 
		}
	}

	void AddExpr(/*Parser.Expression.atg:151*/out IAstExpression expr) {
		/*Parser.Expression.atg:151*/IAstExpression lhs,rhs; BinaryOperator op = BinaryOperator.None; 
		MulExpr(/*Parser.Expression.atg:153*/out lhs);
		/*Parser.Expression.atg:153*/expr = lhs; 
		while (la.kind == _minus || la.kind == _plus) {
			if (la.kind == _plus) {
				Get();
				/*Parser.Expression.atg:154*/op = BinaryOperator.Addition;      
			} else {
				Get();
				/*Parser.Expression.atg:155*/op = BinaryOperator.Subtraction;   
			}
			MulExpr(/*Parser.Expression.atg:156*/out rhs);
			/*Parser.Expression.atg:156*/expr = new AstBinaryOperator(this, expr, op, rhs); 
		}
	}

	void MulExpr(/*Parser.Expression.atg:161*/out IAstExpression expr) {
		/*Parser.Expression.atg:161*/IAstExpression lhs, rhs; BinaryOperator op = BinaryOperator.None; 
		PowExpr(/*Parser.Expression.atg:163*/out lhs);
		/*Parser.Expression.atg:163*/expr = lhs; 
		while (la.kind == _div || la.kind == _times || la.kind == _mod) {
			if (la.kind == _times) {
				Get();
				/*Parser.Expression.atg:164*/op = BinaryOperator.Multiply;      
			} else if (la.kind == _div) {
				Get();
				/*Parser.Expression.atg:165*/op = BinaryOperator.Division;        
			} else {
				Get();
				/*Parser.Expression.atg:166*/op = BinaryOperator.Modulus;       
			}
			PowExpr(/*Parser.Expression.atg:167*/out rhs);
			/*Parser.Expression.atg:167*/expr = new AstBinaryOperator(this, expr, op, rhs); 
		}
	}

	void PowExpr(/*Parser.Expression.atg:172*/out IAstExpression expr) {
		/*Parser.Expression.atg:172*/IAstExpression lhs, rhs; 
		AssignExpr(/*Parser.Expression.atg:174*/out lhs);
		/*Parser.Expression.atg:174*/expr = lhs; 
		while (la.kind == _pow) {
			Get();
			AssignExpr(/*Parser.Expression.atg:175*/out rhs);
			/*Parser.Expression.atg:175*/expr = new AstBinaryOperator(this, expr, BinaryOperator.Power, rhs); 
		}
	}

	void AssignExpr(/*Parser.Expression.atg:179*/out IAstExpression expr) {
		/*Parser.Expression.atg:179*/AstGetSet assignment; BinaryOperator setModifier = BinaryOperator.None;
		IAstType T;
		
		PostfixUnaryExpr(/*Parser.Expression.atg:183*/out expr);
		if (/*Parser.Expression.atg:185*/isAssignmentOperator()) {
			/*Parser.Expression.atg:185*/assignment = expr as AstGetSet;
			if(assignment == null) 
			{
			    SemErr(string.Format("Cannot assign to a {0}",
			        expr.GetType().Name));
			    assignment = new AstGetSetSymbol(this, PCall.Get, "SEMANTIC_ERROR",
			        SymbolInterpretations.LocalObjectVariable); //to prevent null references
			}
			assignment.Call = PCall.Set;
			
			if (StartOf(9)) {
				switch (la.kind) {
				case _assign: {
					Get();
					/*Parser.Expression.atg:196*/setModifier = BinaryOperator.None; 
					break;
				}
				case _plus: {
					Get();
					Expect(_assign);
					/*Parser.Expression.atg:197*/setModifier = BinaryOperator.Addition; 
					break;
				}
				case _minus: {
					Get();
					Expect(_assign);
					/*Parser.Expression.atg:198*/setModifier = BinaryOperator.Subtraction; 
					break;
				}
				case _times: {
					Get();
					Expect(_assign);
					/*Parser.Expression.atg:199*/setModifier = BinaryOperator.Multiply; 
					break;
				}
				case _div: {
					Get();
					Expect(_assign);
					/*Parser.Expression.atg:200*/setModifier = BinaryOperator.Division; 
					break;
				}
				case _bitAnd: {
					Get();
					Expect(_assign);
					/*Parser.Expression.atg:201*/setModifier = BinaryOperator.BitwiseAnd; 
					break;
				}
				case _bitOr: {
					Get();
					Expect(_assign);
					/*Parser.Expression.atg:202*/setModifier = BinaryOperator.BitwiseOr; 
					break;
				}
				case _coalescence: {
					Get();
					Expect(_assign);
					/*Parser.Expression.atg:203*/setModifier = BinaryOperator.Coalescence; 
					break;
				}
				}
				Expr(/*Parser.Expression.atg:204*/out expr);
			} else if (la.kind == _tilde) {
				Get();
				Expect(_assign);
				/*Parser.Expression.atg:206*/setModifier = BinaryOperator.Cast; 
				TypeExpr(/*Parser.Expression.atg:207*/out T);
				/*Parser.Expression.atg:207*/expr = T; 
			} else SynErr(104);
			/*Parser.Expression.atg:209*/assignment.Arguments.Add(expr); 
			assignment.SetModifier = setModifier;
			   expr = assignment;
			
		} else if (StartOf(10)) {
		} else SynErr(105);
	}

	void PostfixUnaryExpr(/*Parser.Expression.atg:217*/out IAstExpression expr) {
		/*Parser.Expression.atg:217*/IAstType type = null; AstGetSet extension; 
		PrefixUnaryExpr(/*Parser.Expression.atg:219*/out expr);
		while (StartOf(11)) {
			if (la.kind == _tilde) {
				Get();
				TypeExpr(/*Parser.Expression.atg:220*/out type);
				/*Parser.Expression.atg:220*/expr = new AstTypecast(this, expr, type); 
			} else if (la.kind == _is) {
				Get();
				TypeExpr(/*Parser.Expression.atg:221*/out type);
				/*Parser.Expression.atg:221*/expr = new AstTypecheck(this, expr, type); 
			} else if (la.kind == _inc) {
				Get();
				/*Parser.Expression.atg:222*/expr = new AstUnaryOperator(this, UnaryOperator.PostIncrement, expr); 
			} else if (la.kind == _dec) {
				Get();
				/*Parser.Expression.atg:223*/expr = new AstUnaryOperator(this, UnaryOperator.PostDecrement, expr); 
			} else {
				GetSetExtension(/*Parser.Expression.atg:224*/expr, out extension);
				/*Parser.Expression.atg:225*/expr = extension; 
			}
		}
	}

	void TypeExpr(/*Parser.Expression.atg:396*/out IAstType type) {
		/*Parser.Expression.atg:396*/type = null; 
		if (StartOf(12)) {
			PrexoniteTypeExpr(/*Parser.Expression.atg:398*/out type);
		} else if (la.kind == _ns || la.kind == _doublecolon) {
			ClrTypeExpr(/*Parser.Expression.atg:399*/out type);
		} else SynErr(106);
	}

	void PrefixUnaryExpr(/*Parser.Expression.atg:230*/out IAstExpression expr) {
		/*Parser.Expression.atg:230*/UnaryOperator op = UnaryOperator.None; 
		while (StartOf(13)) {
			if (la.kind == _plus) {
				Get();
			} else if (la.kind == _minus) {
				Get();
				/*Parser.Expression.atg:233*/op = UnaryOperator.UnaryNegation; 
			} else if (la.kind == _inc) {
				Get();
				/*Parser.Expression.atg:234*/op = UnaryOperator.PreIncrement; 
			} else {
				Get();
				/*Parser.Expression.atg:235*/op = UnaryOperator.PreDecrement; 
			}
		}
		Primary(/*Parser.Expression.atg:237*/out expr);
		/*Parser.Expression.atg:237*/if(op != UnaryOperator.None) expr = new AstUnaryOperator(this, op, expr); 
	}

	void GetSetExtension(/*Parser.Statement.atg:104*/IAstExpression subject, out AstGetSet extension) {
		/*Parser.Statement.atg:104*/extension = null; string id;
		if(subject == null)
		{
			SemErr("Member access not preceded by a proper expression.");
			subject = new AstConstant(this,null);
		}
		                             
		if (/*Parser.Statement.atg:114*/isIndirectCall() ) {
			Expect(_dot);
			/*Parser.Statement.atg:114*/extension = new AstIndirectCall(this, PCall.Get, subject); 
			Arguments(/*Parser.Statement.atg:115*/extension.Arguments);
		} else if (la.kind == _dot) {
			Get();
			Id(/*Parser.Statement.atg:117*/out id);
			/*Parser.Statement.atg:117*/extension = new AstGetSetMemberAccess(this, PCall.Get, subject, id); 
			Arguments(/*Parser.Statement.atg:118*/extension.Arguments);
		} else if (la.kind == _lbrack) {
			/*Parser.Statement.atg:120*/IAstExpression expr; 
			extension = new AstGetSetMemberAccess(this, PCall.Get, subject, ""); 
			
			Get();
			if (StartOf(14)) {
				Expr(/*Parser.Statement.atg:124*/out expr);
				/*Parser.Statement.atg:124*/extension.Arguments.Add(expr); 
				while (WeakSeparator(_comma,14,15) ) {
					Expr(/*Parser.Statement.atg:125*/out expr);
					/*Parser.Statement.atg:125*/extension.Arguments.Add(expr); 
				}
			}
			Expect(_rbrack);
		} else SynErr(107);
	}

	void Primary(/*Parser.Expression.atg:241*/out IAstExpression expr) {
		/*Parser.Expression.atg:241*/expr = null;
		AstGetSet complex = null; bool declared; 
		if (la.kind == _asm) {
			/*Parser.Expression.atg:244*/_pushLexerState(Lexer.Asm); 
			/*Parser.Expression.atg:244*/AstBlockExpression blockExpr = new AstBlockExpression(this); 
			Get();
			Expect(_lpar);
			while (StartOf(1)) {
				AsmInstruction(/*Parser.Expression.atg:245*/blockExpr);
			}
			Expect(_rpar);
			/*Parser.Expression.atg:246*/_popLexerState(); 
			/*Parser.Expression.atg:246*/expr = blockExpr; 
		} else if (StartOf(16)) {
			Constant(/*Parser.Expression.atg:247*/out expr);
		} else if (la.kind == _coroutine) {
			CoroutineCreation(/*Parser.Expression.atg:248*/out expr);
		} else if (la.kind == _new) {
			ObjectCreation(/*Parser.Expression.atg:249*/out expr);
		} else if (la.kind == _lbrack) {
			ListLiteral(/*Parser.Expression.atg:250*/out expr);
		} else if (la.kind == _lbrace) {
			HashLiteral(/*Parser.Expression.atg:251*/out expr);
		} else if (StartOf(17)) {
			LoopExpr(/*Parser.Expression.atg:252*/out expr);
		} else if (la.kind == _throw) {
			/*Parser.Expression.atg:253*/AstThrow th; 
			ThrowExpression(/*Parser.Expression.atg:254*/out th);
			/*Parser.Expression.atg:254*/expr = th; 
		} else if (/*Parser.Expression.atg:256*/isLambdaExpression()) {
			LambdaExpression(/*Parser.Expression.atg:256*/out expr);
		} else if (StartOf(18)) {
			if (la.kind == _lpar) {
				Get();
				Expr(/*Parser.Expression.atg:258*/out expr);
				Expect(_rpar);
			} else {
				GetInitiator(/*Parser.Expression.atg:259*/out complex, out declared);
				/*Parser.Expression.atg:260*/expr = complex; 
			}
		} else if (la.kind == _LPopExpr) {
			Get();
			Expect(_lpar);
			Expr(/*Parser.Expression.atg:262*/out expr);
			/*Parser.Expression.atg:267*/_popLexerState(); _inject(_plus); 
			Expect(_rpar);
		} else SynErr(108);
	}

	void Constant(/*Parser.Expression.atg:272*/out IAstExpression expr) {
		/*Parser.Expression.atg:272*/expr = null; int vi; double vr; bool vb; string vs; 
		if (la.kind == _integer) {
			Integer(/*Parser.Expression.atg:274*/out vi);
			/*Parser.Expression.atg:274*/expr = new AstConstant(this, vi); 
		} else if (la.kind == _real) {
			Real(/*Parser.Expression.atg:275*/out vr);
			/*Parser.Expression.atg:275*/expr = new AstConstant(this, vr); 
		} else if (la.kind == _true || la.kind == _false) {
			Boolean(/*Parser.Expression.atg:276*/out vb);
			/*Parser.Expression.atg:276*/expr = new AstConstant(this, vb); 
		} else if (la.kind == _string) {
			String(/*Parser.Expression.atg:277*/out vs);
			/*Parser.Expression.atg:277*/expr = new AstConstant(this, vs); 
		} else if (la.kind == _null) {
			Null();
			/*Parser.Expression.atg:278*/expr = new AstConstant(this, null); 
		} else SynErr(109);
	}

	void CoroutineCreation(/*Parser.Expression.atg:331*/out IAstExpression expr) {
		/*Parser.Expression.atg:332*/AstCreateCoroutine cor = new AstCreateCoroutine(this); 
		IAstExpression iexpr;
		expr = cor;
		
		Expect(_coroutine);
		Expr(/*Parser.Expression.atg:337*/out iexpr);
		/*Parser.Expression.atg:337*/cor.Expression = iexpr; 
		if (la.kind == _for) {
			Get();
			Arguments(/*Parser.Expression.atg:338*/cor.Arguments);
		}
	}

	void ObjectCreation(/*Parser.Expression.atg:323*/out IAstExpression expr) {
		/*Parser.Expression.atg:323*/IAstType type; expr = null; 
		Expect(_new);
		TypeExpr(/*Parser.Expression.atg:325*/out type);
		/*Parser.Expression.atg:325*/AstObjectCreation creation = new AstObjectCreation(this, type); 
		if (la.kind == _lpar || la.kind == _appendleft) {
			Arguments(/*Parser.Expression.atg:326*/creation.Arguments);
		}
		/*Parser.Expression.atg:327*/expr = creation; 
	}

	void ListLiteral(/*Parser.Expression.atg:282*/out IAstExpression expr) {
		/*Parser.Expression.atg:282*/IAstExpression iexpr = null; 
		AstListLiteral lst = new AstListLiteral(this);
		expr = lst;
		
		Expect(_lbrack);
		if (StartOf(14)) {
			Expr(/*Parser.Expression.atg:288*/out iexpr);
			/*Parser.Expression.atg:288*/lst.Elements.Add(iexpr); 
			while (la.kind == _comma) {
				Get();
				Expr(/*Parser.Expression.atg:290*/out iexpr);
				/*Parser.Expression.atg:290*/lst.Elements.Add(iexpr); 
			}
		}
		Expect(_rbrack);
	}

	void HashLiteral(/*Parser.Expression.atg:297*/out IAstExpression expr) {
		/*Parser.Expression.atg:297*/IAstExpression iexpr = null; 
		AstHashLiteral hash = new AstHashLiteral(this);
		expr = hash;
		
		Expect(_lbrace);
		if (StartOf(14)) {
			Expr(/*Parser.Expression.atg:303*/out iexpr);
			/*Parser.Expression.atg:303*/hash.Elements.Add(iexpr); 
			while (la.kind == _comma) {
				Get();
				Expr(/*Parser.Expression.atg:305*/out iexpr);
				/*Parser.Expression.atg:305*/hash.Elements.Add(iexpr); 
			}
		}
		Expect(_rbrace);
	}

	void LoopExpr(/*Parser.Expression.atg:312*/out IAstExpression expr) {
		/*Parser.Expression.atg:312*/AstBlock dummyBlock = new AstBlock(this);
		
		if (la.kind == _do || la.kind == _while || la.kind == _until) {
			WhileLoop(/*Parser.Expression.atg:315*/dummyBlock);
		} else if (la.kind == _for) {
			ForLoop(/*Parser.Expression.atg:316*/dummyBlock);
		} else if (la.kind == _foreach) {
			ForeachLoop(/*Parser.Expression.atg:317*/dummyBlock);
		} else SynErr(110);
		/*Parser.Expression.atg:318*/expr = new AstLoopExpression(this, (AstLoop) dummyBlock.Statements[0]); 
	}

	void ThrowExpression(/*Parser.Expression.atg:384*/out AstThrow th) {
		/*Parser.Expression.atg:384*/th = new AstThrow(this); 
		Expect(_throw);
		Expr(/*Parser.Expression.atg:387*/out th.Expression);
	}

	void LambdaExpression(/*Parser.Expression.atg:342*/out IAstExpression expr) {
		/*Parser.Expression.atg:342*/expr = null;
		PFunction func = new PFunction(TargetApplication, generateNestedFunctionId());                                             
		func.Meta[Application.ImportKey] = target.Function.Meta[Application.ImportKey];
		func.Meta["ParentFunction"] = target.Function.Id;
		TargetApplication.Functions.Add(func);
		Loader.CreateFunctionTarget(func, new AstBlock(this));
		CompilerTarget ft = FunctionTargets[func];
		ft.ParentTarget = target;
		
		if (StartOf(19)) {
			FormalArg(/*Parser.Expression.atg:352*/ft);
		} else if (la.kind == _lpar) {
			Get();
			if (StartOf(19)) {
				FormalArg(/*Parser.Expression.atg:354*/ft);
				while (la.kind == _comma) {
					Get();
					FormalArg(/*Parser.Expression.atg:356*/ft);
				}
			}
			Expect(_rpar);
		} else SynErr(111);
		/*Parser.Expression.atg:362*/CompilerTarget oldTarget = target;
		target = ft;
		
		Expect(_implementation);
		if (la.kind == _lbrace) {
			Get();
			while (StartOf(20)) {
				Statement(/*Parser.Expression.atg:367*/ft.Ast);
			}
			Expect(_rbrace);
		} else if (StartOf(14)) {
			/*Parser.Expression.atg:369*/AstReturn ret = new AstReturn(this, ReturnVariant.Exit); 
			Expr(/*Parser.Expression.atg:370*/out ret.Expression);
			/*Parser.Expression.atg:370*/ft.Ast.Add(ret); 
		} else SynErr(112);
		/*Parser.Expression.atg:373*/target = oldTarget;
		if(errors.count == 0)
		{
		    Ast[func].EmitCode(FunctionTargets[func]);
		    FunctionTargets[func].FinishTarget();
		}
		
		expr = new AstCreateClosure(this, func.Id);                                         
		
	}

	void GetInitiator(/*Parser.Statement.atg:132*/out AstGetSet complex, out bool isDeclaration) {
		/*Parser.Statement.atg:132*/complex = null; 
		AstGetSetSymbol symbol = null;
		AstGetSetStatic staticCall = null;
		AstGetSet member = null;
		IAstExpression expr = null;
		List<IAstExpression> args = new List<IAstExpression>();
		isDeclaration = false;                                            
		string id;
		
		if (StartOf(21)) {
			if (/*Parser.Statement.atg:144*/isLikeFunction(la.val) ) {
				Function(/*Parser.Statement.atg:144*/out symbol);
			} else if (StartOf(22)) {
				Variable(/*Parser.Statement.atg:145*/out symbol, out isDeclaration);
			} else if (la.kind == _ns || la.kind == _tilde || la.kind == _doublecolon) {
				StaticCall(/*Parser.Statement.atg:146*/out staticCall);
			} else {
				Get();
				Expr(/*Parser.Statement.atg:147*/out expr);
				/*Parser.Statement.atg:147*/args.Add(expr); 
				while (la.kind == _comma) {
					Get();
					Expr(/*Parser.Statement.atg:148*/out expr);
					/*Parser.Statement.atg:148*/args.Add(expr); 
				}
				Expect(_rpar);
				if (la.kind == _dot || la.kind == _lbrack) {
					GetSetExtension(/*Parser.Statement.atg:151*/expr, out member);
					/*Parser.Statement.atg:152*/if(args.Count > 1)
					SemErr("A member access cannot have multiple subjects. (Did you mean '>>'?)");
					
				} else if (la.kind == _appendright) {
					Get();
					GetCall(/*Parser.Statement.atg:156*/out complex);
					/*Parser.Statement.atg:156*/complex.Arguments.RightAppend(args);
					complex.Arguments.ReleaseRightAppend();
					member = complex;
					
				} else SynErr(113);
			}
			/*Parser.Statement.atg:162*/complex = 
			(AstGetSet)symbol ?? 
			(AstGetSet)staticCall ?? 
			(AstGetSet)member; 
			
		} else if (/*Parser.Statement.atg:169*/isDeDereference() ) {
			Expect(_pointer);
			Expect(_pointer);
			Id(/*Parser.Statement.atg:169*/out id);
			/*Parser.Statement.atg:169*/SymbolEntry s = target.Symbols[id];
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
			Id(/*Parser.Statement.atg:189*/out id);
			/*Parser.Statement.atg:189*/SymbolEntry s = target.Symbols[id];
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
			
		} else SynErr(114);
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

	void WhileLoop(/*Parser.Statement.atg:364*/AstBlock block) {
		/*Parser.Statement.atg:364*/AstWhileLoop loop = null;
		bool isPositive = true; 
		
		if (la.kind == _while || la.kind == _until) {
			if (la.kind == _while) {
				Get();
			} else {
				Get();
				/*Parser.Statement.atg:368*/isPositive = false; 
			}
			/*Parser.Statement.atg:369*/loop = new AstWhileLoop(this, true, isPositive); 
			Expect(_lpar);
			Expr(/*Parser.Statement.atg:370*/out loop.Condition);
			Expect(_rpar);
			/*Parser.Statement.atg:371*/target.BeginBlock(loop.Labels); 
			StatementBlock(/*Parser.Statement.atg:372*/loop.Block);
		} else if (la.kind == _do) {
			Get();
			/*Parser.Statement.atg:373*/AstBlock loopBody = new AstBlock(this); 
			BlockLabels labels = AstWhileLoop.CreateBlockLabels();
			target.BeginBlock(labels);
			
			StatementBlock(/*Parser.Statement.atg:377*/loopBody);
			if (la.kind == _while) {
				Get();
			} else if (la.kind == _until) {
				Get();
				/*Parser.Statement.atg:378*/isPositive = false; 
			} else SynErr(115);
			/*Parser.Statement.atg:379*/loop = new AstWhileLoop(this, false, isPositive); 
			loop.Labels = labels;
			loop.Block = loopBody;
			
			Expect(_lpar);
			Expr(/*Parser.Statement.atg:383*/out loop.Condition);
			Expect(_rpar);
		} else SynErr(116);
		/*Parser.Statement.atg:384*/target.EndBlock(); block.Add(loop); 
	}

	void ForLoop(/*Parser.Statement.atg:387*/AstBlock block) {
		/*Parser.Statement.atg:387*/AstForLoop loop;
		
		Expect(_for);
		/*Parser.Statement.atg:390*/loop = new AstForLoop(this); target.BeginBlock(loop.Labels); 
		Expect(_lpar);
		StatementBlock(/*Parser.Statement.atg:391*/loop.Initialize);
		if (la.kind == _do) {
			Get();
			StatementBlock(/*Parser.Statement.atg:393*/loop.NextIteration);
			/*Parser.Statement.atg:394*/loop.IsPrecondition = false; 
			if (la.kind == _while || la.kind == _until) {
				if (la.kind == _while) {
					Get();
				} else {
					Get();
					/*Parser.Statement.atg:396*/loop.IsPositive = false; 
				}
			}
			Expr(/*Parser.Statement.atg:398*/out loop.Condition);
		} else if (StartOf(14)) {
			if (la.kind == _while || la.kind == _until) {
				if (la.kind == _while) {
					Get();
				} else {
					Get();
					/*Parser.Statement.atg:400*/loop.IsPositive = false; 
				}
			}
			Expr(/*Parser.Statement.atg:402*/out loop.Condition);
			Expect(_semicolon);
			SimpleStatement(/*Parser.Statement.atg:404*/loop.NextIteration);
		} else SynErr(117);
		Expect(_rpar);
		StatementBlock(/*Parser.Statement.atg:407*/loop.Block);
		/*Parser.Statement.atg:407*/target.EndBlock(); block.Add(loop); 
	}

	void ForeachLoop(/*Parser.Statement.atg:411*/AstBlock block) {
		Expect(_foreach);
		/*Parser.Statement.atg:412*/AstForeachLoop loop = new AstForeachLoop(this); 
		Expect(_lpar);
		GetCall(/*Parser.Statement.atg:414*/out loop.Element);
		Expect(_in);
		Expr(/*Parser.Statement.atg:416*/out loop.List);
		Expect(_rpar);
		/*Parser.Statement.atg:418*/target.BeginBlock(loop.Labels); 
		StatementBlock(/*Parser.Statement.atg:419*/loop.Block);
		/*Parser.Statement.atg:420*/target.EndBlock(); 
		/*Parser.Statement.atg:423*/block.Add(loop); 
	}

	void Arguments(/*Parser.Statement.atg:552*/ArgumentsProxy args) {
		/*Parser.Statement.atg:553*/IAstExpression expr;
		                      
		if (la.kind == _lpar) {
			Get();
			if (StartOf(14)) {
				Expr(/*Parser.Statement.atg:558*/out expr);
				/*Parser.Statement.atg:558*/args.Add(expr); 
				while (WeakSeparator(_comma,14,23) ) {
					Expr(/*Parser.Statement.atg:560*/out expr);
					/*Parser.Statement.atg:560*/args.Add(expr); 
				}
			}
			Expect(_rpar);
		}
		/*Parser.Statement.atg:565*/args.RemeberRightAppendPosition(); 
		if (la.kind == _appendleft) {
			Get();
			if (la.kind == _lpar) {
				Get();
				if (StartOf(14)) {
					Expr(/*Parser.Statement.atg:570*/out expr);
					/*Parser.Statement.atg:570*/args.Add(expr); 
					while (la.kind == _comma) {
						Get();
						Expr(/*Parser.Statement.atg:572*/out expr);
						/*Parser.Statement.atg:573*/args.Add(expr); 
					}
				}
				Expect(_rpar);
			} else if (StartOf(14)) {
				Expr(/*Parser.Statement.atg:577*/out expr);
				/*Parser.Statement.atg:577*/args.Add(expr); 
			} else SynErr(118);
		}
	}

	void FormalArg(/*Parser.GlobalScope.atg:379*/CompilerTarget ft) {
		/*Parser.GlobalScope.atg:379*/string id; SymbolInterpretations kind = SymbolInterpretations.LocalObjectVariable; 
		if (la.kind == _var || la.kind == _ref) {
			if (la.kind == _var) {
				Get();
			} else {
				Get();
				/*Parser.GlobalScope.atg:381*/kind = SymbolInterpretations.LocalReferenceVariable; 
			}
		}
		Id(/*Parser.GlobalScope.atg:383*/out id);
		/*Parser.GlobalScope.atg:383*/ft.Function.Parameters.Add(id); 
		ft.Symbols.Add(id, new SymbolEntry(kind, id));
		
	}

	void Statement(/*Parser.Statement.atg:31*/AstBlock block) {
		if (/*Parser.Statement.atg:34*/isLabel() ) {
			ExplicitLabel(/*Parser.Statement.atg:34*/block);
		} else if (StartOf(24)) {
			if (StartOf(25)) {
				SimpleStatement(/*Parser.Statement.atg:35*/block);
			}
			Expect(_semicolon);
		} else if (StartOf(26)) {
			StructureStatement(/*Parser.Statement.atg:36*/block);
		} else SynErr(119);
		while (la.kind == _and) {
			Get();
			Statement(/*Parser.Statement.atg:38*/block);
		}
	}

	void ExplicitTypeExpr(/*Parser.Expression.atg:390*/out IAstType type) {
		/*Parser.Expression.atg:390*/type = null; 
		if (la.kind == _tilde) {
			Get();
			PrexoniteTypeExpr(/*Parser.Expression.atg:392*/out type);
		} else if (la.kind == _ns || la.kind == _doublecolon) {
			ClrTypeExpr(/*Parser.Expression.atg:393*/out type);
		} else SynErr(120);
	}

	void PrexoniteTypeExpr(/*Parser.Expression.atg:418*/out IAstType type) {
		/*Parser.Expression.atg:418*/string id = null; type = null; 
		if (StartOf(4)) {
			Id(/*Parser.Expression.atg:420*/out id);
		} else if (la.kind == _null) {
			Get();
			/*Parser.Expression.atg:420*/id = NullPType.Literal; 
		} else SynErr(121);
		/*Parser.Expression.atg:422*/AstDynamicTypeExpression dType = new AstDynamicTypeExpression(this, id); 
		if (la.kind == _lt) {
			Get();
			if (StartOf(27)) {
				TypeExprElement(/*Parser.Expression.atg:424*/dType.Arguments);
				while (la.kind == _comma) {
					Get();
					TypeExprElement(/*Parser.Expression.atg:425*/dType.Arguments);
				}
			}
			Expect(_gt);
		}
		/*Parser.Expression.atg:429*/type = dType; 
	}

	void ClrTypeExpr(/*Parser.Expression.atg:403*/out IAstType type) {
		/*Parser.Expression.atg:403*/string id; 
		/*Parser.Expression.atg:405*/StringBuilder typeId = new StringBuilder(); 
		if (la.kind == _doublecolon) {
			Get();
		} else if (la.kind == _ns) {
			Ns(/*Parser.Expression.atg:407*/out id);
			/*Parser.Expression.atg:407*/typeId.Append(id); typeId.Append('.'); 
		} else SynErr(122);
		while (la.kind == _ns) {
			Ns(/*Parser.Expression.atg:409*/out id);
			/*Parser.Expression.atg:409*/typeId.Append(id); typeId.Append('.'); 
		}
		Id(/*Parser.Expression.atg:411*/out id);
		/*Parser.Expression.atg:411*/typeId.Append(id);
		type = new AstConstantTypeExpression(this, 
		    "Object(\"" + StringPType.Escape(typeId.ToString()) + "\")");
		
	}

	void Ns(/*Parser.Helper.atg:35*/out string ns) {
		/*Parser.Helper.atg:35*/ns = "\\NoId\\"; 
		Expect(_ns);
		/*Parser.Helper.atg:37*/ns = cache(t.val); 
	}

	void TypeExprElement(/*Parser.Expression.atg:433*/List<IAstExpression> args ) {
		/*Parser.Expression.atg:433*/IAstExpression expr; IAstType type; 
		if (StartOf(16)) {
			Constant(/*Parser.Expression.atg:435*/out expr);
			/*Parser.Expression.atg:435*/args.Add(expr); 
		} else if (la.kind == _ns || la.kind == _tilde || la.kind == _doublecolon) {
			ExplicitTypeExpr(/*Parser.Expression.atg:436*/out type);
			/*Parser.Expression.atg:436*/args.Add(type); 
		} else if (la.kind == _lpar) {
			Get();
			Expr(/*Parser.Expression.atg:437*/out expr);
			Expect(_rpar);
			/*Parser.Expression.atg:437*/args.Add(expr); 
		} else SynErr(123);
	}

	void Prexonite() {
		/*Parser.GlobalScope.atg:26*/PFunction func; 
		while (StartOf(28)) {
			if (StartOf(29)) {
				if (StartOf(30)) {
					if (la.kind == _var || la.kind == _ref) {
						GlobalVariableDefinition();
					} else if (la.kind == _declare) {
						Declaration();
					} else {
						MetaAssignment(/*Parser.GlobalScope.atg:30*/TargetApplication);
					}
				}
				while (!(la.kind == _EOF || la.kind == _semicolon)) {SynErr(124); Get();}
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
		while (!(la.kind == _EOF || la.kind == _var || la.kind == _ref)) {SynErr(125); Get();}
		if (la.kind == _var) {
			Get();
		} else if (la.kind == _ref) {
			Get();
			/*Parser.GlobalScope.atg:89*/type = SymbolInterpretations.GlobalReferenceVariable; 
		} else SynErr(126);
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
			while (StartOf(31)) {
				MetaAssignment(/*Parser.GlobalScope.atg:103*/vari);
				while (!(la.kind == _EOF || la.kind == _semicolon)) {SynErr(127); Get();}
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
			if(errors.count == 0)
			{
				AstGetSet complex = new AstGetSetSymbol(this, PCall.Set, id, InterpretAsObjectVariable(type));
				complex.Arguments.Add(expr);
				target.Ast.Add(complex);
				vari.Meta[Application.InitializationId] = TargetApplication._RegisterInitializationUpdate().ToString();
				Loader._EmitPartialInitializationCode();
			                  }
			                  target = lastTarget;
			              
		}
	}

	void Declaration() {
		/*Parser.GlobalScope.atg:130*/SymbolInterpretations type = SymbolInterpretations.Undefined; 
		while (!(la.kind == _EOF || la.kind == _declare)) {SynErr(128); Get();}
		Expect(_declare);
		if (StartOf(32)) {
			if (la.kind == _var) {
				Get();
				/*Parser.GlobalScope.atg:134*/type = SymbolInterpretations.GlobalObjectVariable; 
			} else if (la.kind == _ref) {
				Get();
				/*Parser.GlobalScope.atg:135*/type = SymbolInterpretations.GlobalReferenceVariable; 
			} else if (la.kind == _function) {
				Get();
				/*Parser.GlobalScope.atg:136*/type = SymbolInterpretations.Function; 
			} else {
				Get();
				/*Parser.GlobalScope.atg:137*/type = SymbolInterpretations.Command; 
			}
		}
		DeclarationInstance(/*Parser.GlobalScope.atg:139*/type);
		while (WeakSeparator(_comma,4,33) ) {
			DeclarationInstance(/*Parser.GlobalScope.atg:140*/type);
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
			} else if (StartOf(34)) {
				MetaExpr(/*Parser.GlobalScope.atg:51*/out entry);
			} else SynErr(129);
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
			
		} else SynErr(130);
		/*Parser.GlobalScope.atg:63*/if(entry == null || key == null) SemErr("Meta assignment did not generate an entry."); else target.Meta[key] = entry; 
	}

	void GlobalCode() {
		/*Parser.GlobalScope.atg:223*/PFunction func = TargetApplication._InitializationFunction;
		CompilerTarget ft = FunctionTargets[func];
		
		/*Parser.GlobalScope.atg:228*/target = ft; 
		                             _pushLexerState(Lexer.Local);
		                         
		Expect(_lbrace);
		while (StartOf(20)) {
			Statement(/*Parser.GlobalScope.atg:232*/target.Ast);
		}
		Expect(_rbrace);
		/*Parser.GlobalScope.atg:235*/if(errors.count == 0)
		{
			TargetApplication._RequireInitialization();
			Loader._EmitPartialInitializationCode();
		}
		target = null;
		_popLexerState();
		
	}

	void BuildBlock() {
		while (!(la.kind == _EOF || la.kind == _build)) {SynErr(131); Get();}
		Expect(_build);
		/*Parser.GlobalScope.atg:178*/PFunction func = new PFunction(TargetApplication);
		  CompilerTarget lastTarget = target; 
		  target = Loader.CreateFunctionTarget(func, new AstBlock(this));
		  target.Declare(SymbolInterpretations.Command, "Add", Loader.BuildAddCommand);
		  target.Declare(SymbolInterpretations.Command, "Require", Loader.BuildRequireCommand);
		  target.Declare(SymbolInterpretations.Command, "Default", Loader.BuildDefaultCommand);
		  target.Declare(SymbolInterpretations.Command, "Hook", Loader.BuildHookCommand);
		  target.Declare(SymbolInterpretations.Command, "GetLoader", Loader.BuildGetLoaderCommand);
		  _pushLexerState(Lexer.Local);                                
		
		if (la.kind == _does) {
			Get();
		}
		StatementBlock(/*Parser.GlobalScope.atg:190*/target.Ast);
		/*Parser.GlobalScope.atg:193*/_popLexerState();
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

	void FunctionDefinition(/*Parser.GlobalScope.atg:248*/out PFunction func) {
		/*Parser.GlobalScope.atg:249*/func = null; 
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
			/*Parser.GlobalScope.atg:263*/isCoroutine = true;
			
		} else SynErr(132);
		Id(/*Parser.GlobalScope.atg:266*/out id);
		/*Parser.GlobalScope.atg:266*/funcId = id;
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
		      {
		          funcId = generateNestedFunctionId(id);
		          SmartDeclareLocal(id, SymbolInterpretations.LocalReferenceVariable);
		      }
		      
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
			if (StartOf(19)) {
				FormalArg(/*Parser.GlobalScope.atg:313*/ft);
				while (la.kind == _comma) {
					Get();
					FormalArg(/*Parser.GlobalScope.atg:315*/ft);
				}
			}
			Expect(_rpar);
		}
		/*Parser.GlobalScope.atg:319*/if(target == null && 
		     (!object.ReferenceEquals(func, TargetApplication._InitializationFunction)))
		 {
		         //Add the name to the symboltable
		         Symbols[func.Id] = new SymbolEntry(SymbolInterpretations.Function, func.Id);
		 }
		 //Target the coroutine body instead of the stub
		    if(isCoroutine)
		        func = corBody;
		
		if (la.kind == _lbrack) {
			/*Parser.GlobalScope.atg:329*/_pushLexerState(Lexer.YYINITIAL); 
			Get();
			while (StartOf(31)) {
				MetaAssignment(/*Parser.GlobalScope.atg:331*/func);
				while (!(la.kind == _EOF || la.kind == _semicolon)) {SynErr(133); Get();}
				Expect(_semicolon);
			}
			/*Parser.GlobalScope.atg:333*/_popLexerState(); 
			Expect(_rbrack);
		}
		/*Parser.GlobalScope.atg:336*/CompilerTarget lastTarget = target;
		target = FunctionTargets[func]; 
		_pushLexerState(Lexer.Local);
		
		if (la.kind == _does) {
			Get();
			StatementBlock(/*Parser.GlobalScope.atg:341*/target.Ast);
		} else if (la.kind == _lbrace) {
			Get();
			while (StartOf(20)) {
				Statement(/*Parser.GlobalScope.atg:342*/target.Ast);
			}
			Expect(_rbrace);
		} else if (la.kind == _assign) {
			Get();
			/*Parser.GlobalScope.atg:343*/AstReturn ret = new AstReturn(this, ReturnVariant.Exit); 
			Expr(/*Parser.GlobalScope.atg:344*/out ret.Expression);
			/*Parser.GlobalScope.atg:344*/target.Ast.Add(ret); 
			Expect(_semicolon);
		} else SynErr(134);
		/*Parser.GlobalScope.atg:346*/_popLexerState();
		target = lastTarget; 
		//Compile AST
		if(errors.count == 0)
		    if(Engine.StringsAreEqual(func.Id, @"\init"))
		    {
		        TargetApplication._RequireInitialization();
		        Loader._EmitPartialInitializationCode();
		        //Initialize function gets finished at the end of Loader.Load
		    }
		    else
		    {
		FunctionTargets[func].ExecuteCompilerHooks();
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

	void GlobalId(/*Parser.GlobalScope.atg:388*/out string id) {
		/*Parser.GlobalScope.atg:388*/id = "...no freaking id..."; 
		if (la.kind == _id) {
			Get();
			/*Parser.GlobalScope.atg:390*/id = cache(t.val); 
		} else if (la.kind == _anyId) {
			Get();
			/*Parser.GlobalScope.atg:391*/id = cache(t.val.Substring(1)); 
		} else SynErr(135);
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
			if (StartOf(34)) {
				MetaExpr(/*Parser.GlobalScope.atg:75*/out subEntry);
				/*Parser.GlobalScope.atg:75*/lst.Add(subEntry); 
				while (WeakSeparator(_comma,34,35) ) {
					MetaExpr(/*Parser.GlobalScope.atg:77*/out subEntry);
					/*Parser.GlobalScope.atg:77*/lst.Add(subEntry); 
				}
			}
			Expect(_rbrace);
			/*Parser.GlobalScope.atg:80*/entry = (MetaEntry) lst.ToArray(); 
			break;
		}
		default: SynErr(136); break;
		}
	}

	void GlobalQualifiedId(/*Parser.GlobalScope.atg:394*/out string id) {
		/*Parser.GlobalScope.atg:394*/id = "\\NoId\\"; 
		if (la.kind == _id || la.kind == _anyId) {
			GlobalId(/*Parser.GlobalScope.atg:396*/out id);
		} else if (la.kind == _ns) {
			Ns(/*Parser.GlobalScope.atg:397*/out id);
			/*Parser.GlobalScope.atg:397*/StringBuilder buffer = new StringBuilder(id); buffer.Append('.'); 
			while (la.kind == _ns) {
				Ns(/*Parser.GlobalScope.atg:398*/out id);
				/*Parser.GlobalScope.atg:398*/buffer.Append(id); buffer.Append('.'); 
			}
			GlobalId(/*Parser.GlobalScope.atg:400*/out id);
			/*Parser.GlobalScope.atg:400*/buffer.Append(id); 
			/*Parser.GlobalScope.atg:401*/id = buffer.ToString(); 
		} else SynErr(137);
	}

	void DeclarationInstance(/*Parser.GlobalScope.atg:144*/SymbolInterpretations type) {
		/*Parser.GlobalScope.atg:144*/string id; string aId; 
		Id(/*Parser.GlobalScope.atg:146*/out id);
		/*Parser.GlobalScope.atg:146*/aId = id; 
		if (la.kind == _as) {
			Get();
			Id(/*Parser.GlobalScope.atg:147*/out aId);
		}
		/*Parser.GlobalScope.atg:148*/SymbolEntry inferredType;
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
		Statement(/*Parser.Statement.atg:27*/block);
	}

	void ExplicitLabel(/*Parser.Statement.atg:306*/AstBlock block) {
		/*Parser.Statement.atg:306*/string id = "--\\NotAnId\\--"; 
		if (StartOf(4)) {
			Id(/*Parser.Statement.atg:308*/out id);
			Expect(_colon);
		} else if (la.kind == _lid) {
			Get();
			/*Parser.Statement.atg:309*/id = cache(t.val.Substring(0,t.val.Length-1)); 
		} else SynErr(138);
		/*Parser.Statement.atg:310*/block.Statements.Add(new AstExplicitLabel(this, id)); 
	}

	void SimpleStatement(/*Parser.Statement.atg:43*/AstBlock block) {
		if (la.kind == _goto) {
			ExplicitGoTo(/*Parser.Statement.atg:44*/block);
		} else if (la.kind == _declare) {
			Declaration();
		} else if (/*Parser.Statement.atg:47*/isVariableDeclaration() ) {
			VariableDeclarationStatement();
		} else if (StartOf(18)) {
			GetSetComplex(/*Parser.Statement.atg:48*/block);
		} else if (StartOf(36)) {
			Return(/*Parser.Statement.atg:49*/block);
		} else if (la.kind == _throw) {
			Throw(/*Parser.Statement.atg:50*/block);
		} else SynErr(139);
	}

	void StructureStatement(/*Parser.Statement.atg:54*/AstBlock block) {
		switch (la.kind) {
		case _asm: {
			/*Parser.Statement.atg:55*/_pushLexerState(Lexer.Asm); 
			Get();
			AsmStatementBlock(/*Parser.Statement.atg:56*/block);
			/*Parser.Statement.atg:57*/_popLexerState(); 
			break;
		}
		case _if: case _unless: {
			Condition(/*Parser.Statement.atg:58*/block);
			break;
		}
		case _do: case _while: case _until: {
			WhileLoop(/*Parser.Statement.atg:59*/block);
			break;
		}
		case _for: {
			ForLoop(/*Parser.Statement.atg:60*/block);
			break;
		}
		case _foreach: {
			ForeachLoop(/*Parser.Statement.atg:61*/block);
			break;
		}
		case _function: case _coroutine: {
			NestedFunction(/*Parser.Statement.atg:62*/block);
			break;
		}
		case _try: {
			TryCatchFinally(/*Parser.Statement.atg:63*/block);
			break;
		}
		case _uusing: {
			Using(/*Parser.Statement.atg:64*/block);
			break;
		}
		case _lbrace: {
			Get();
			while (StartOf(20)) {
				Statement(/*Parser.Statement.atg:67*/block);
			}
			Expect(_rbrace);
			break;
		}
		default: SynErr(140); break;
		}
	}

	void ExplicitGoTo(/*Parser.Statement.atg:313*/AstBlock block) {
		/*Parser.Statement.atg:313*/string id; 
		Expect(_goto);
		Id(/*Parser.Statement.atg:316*/out id);
		/*Parser.Statement.atg:316*/block.Statements.Add(new AstExplicitGoTo(this, id)); 
	}

	void VariableDeclarationStatement() {
		/*Parser.Statement.atg:254*/AstGetSetSymbol variable; 
		VariableDeclaration(/*Parser.Statement.atg:255*/out variable);
	}

	void GetSetComplex(/*Parser.Statement.atg:73*/AstBlock block) {
		/*Parser.Statement.atg:73*/AstGetSet complex = null; 
		AstGetSetSymbol symbol = null;
		bool isDeclaration = false;
		
		GetInitiator(/*Parser.Statement.atg:79*/out complex, out isDeclaration);
		while (la.kind == _dot || la.kind == _lbrack) {
			GetSetExtension(/*Parser.Statement.atg:82*/complex, out complex);
		}
		if (la.kind == _rpar || la.kind == _semicolon) {
		} else if (la.kind == _inc) {
			Get();
			/*Parser.Statement.atg:86*/block.Add(new AstUnaryOperator(this, UnaryOperator.PostIncrement, complex));
			complex = null;
			
		} else if (la.kind == _dec) {
			Get();
			/*Parser.Statement.atg:89*/block.Add(new AstUnaryOperator(this, UnaryOperator.PostDecrement, complex));
			complex = null;
			
		} else if (StartOf(37)) {
			Assignment(/*Parser.Statement.atg:92*/complex);
			/*Parser.Statement.atg:92*/symbol = complex as AstGetSetSymbol;
			if(symbol != null && InterpretationIsVariable(symbol.Interpretation) && isDeclaration)
			    symbol.Interpretation = InterpretAsObjectVariable(symbol.Interpretation);
			
		} else SynErr(141);
		/*Parser.Statement.atg:98*/if(complex != null)
		   block.Add(complex);
		
	}

	void Return(/*Parser.Statement.atg:447*/AstBlock block) {
		/*Parser.Statement.atg:447*/AstReturn ret = null; 
		AstExplicitGoTo jump = null; 
		IAstExpression expr = null; 
		BlockLabels bl = target.CurrentBlock;
		
		if (la.kind == _return || la.kind == _yield) {
			if (la.kind == _return) {
				Get();
				/*Parser.Statement.atg:455*/ret = new AstReturn(this, ReturnVariant.Exit); 
			} else {
				Get();
				/*Parser.Statement.atg:456*/ret = new AstReturn(this, ReturnVariant.Continue); 
			}
			if (StartOf(38)) {
				if (StartOf(14)) {
					Expr(/*Parser.Statement.atg:458*/out expr);
					/*Parser.Statement.atg:458*/ret.Expression = expr; 
				} else {
					Get();
					/*Parser.Statement.atg:459*/ret.ReturnVariant = ReturnVariant.Set; 
					Expr(/*Parser.Statement.atg:460*/out expr);
					/*Parser.Statement.atg:460*/ret.Expression = expr; 
				}
			}
		} else if (la.kind == _break) {
			Get();
			/*Parser.Statement.atg:462*/if(bl == null)
			   ret = new AstReturn(this, ReturnVariant.Break); 
			else
			    jump = new AstExplicitGoTo(this, bl.BreakLabel);
			
		} else if (la.kind == _continue) {
			Get();
			/*Parser.Statement.atg:467*/if(bl == null)
			   ret = new AstReturn(this, ReturnVariant.Continue); 
			else
			    jump = new AstExplicitGoTo(this, bl.ContinueLabel);
			
		} else SynErr(142);
		/*Parser.Statement.atg:472*/block.Add((AstNode)ret ?? (AstNode)jump); 
	}

	void Throw(/*Parser.Statement.atg:537*/AstBlock block) {
		/*Parser.Statement.atg:537*/AstThrow th; 
		ThrowExpression(/*Parser.Statement.atg:539*/out th);
		/*Parser.Statement.atg:540*/block.Add(th); 
	}

	void Condition(/*Parser.Statement.atg:346*/AstBlock block) {
		/*Parser.Statement.atg:346*/IAstExpression expr = null; bool isNegative = false; 
		if (la.kind == _if) {
			Get();
			/*Parser.Statement.atg:348*/isNegative = false; 
		} else if (la.kind == _unless) {
			Get();
			/*Parser.Statement.atg:349*/isNegative = true; 
		} else SynErr(143);
		Expect(_lpar);
		Expr(/*Parser.Statement.atg:352*/out expr);
		Expect(_rpar);
		/*Parser.Statement.atg:353*/AstCondition cond = new AstCondition(this, expr, isNegative); 
		StatementBlock(/*Parser.Statement.atg:355*/cond.IfBlock);
		if (la.kind == _else) {
			Get();
			StatementBlock(/*Parser.Statement.atg:358*/cond.ElseBlock);
		}
		/*Parser.Statement.atg:360*/block.Add(cond); 
	}

	void NestedFunction(/*Parser.Statement.atg:476*/AstBlock block) {
		/*Parser.Statement.atg:476*/PFunction func; 
		FunctionDefinition(/*Parser.Statement.atg:478*/out func);
		/*Parser.Statement.atg:480*/string logicalId = func.Meta["LogicalId"];
		func.Meta["ParentFunction"] = target.Function.Id;
		string physicalId = func.Id;
		
		AstGetSetSymbol setVar = new AstGetSetSymbol(this, PCall.Set, logicalId, SymbolInterpretations.LocalObjectVariable);
		setVar.Arguments.Add( new AstCreateClosure(this, physicalId) );
		block.Add(setVar);
		
	}

	void TryCatchFinally(/*Parser.Statement.atg:491*/AstBlock block) {
		/*Parser.Statement.atg:491*/AstTryCatchFinally a = new AstTryCatchFinally(this); 
		Expect(_try);
		Expect(_lbrace);
		while (StartOf(20)) {
			Statement(/*Parser.Statement.atg:495*/a.TryBlock);
		}
		Expect(_rbrace);
		if (la.kind == _catch || la.kind == _finally) {
			if (la.kind == _catch) {
				Get();
				if (la.kind == _lpar) {
					Get();
					GetCall(/*Parser.Statement.atg:500*/out a.ExceptionVar);
					Expect(_rpar);
				}
				Expect(_lbrace);
				while (StartOf(20)) {
					Statement(/*Parser.Statement.atg:504*/a.CatchBlock);
				}
				Expect(_rbrace);
				if (la.kind == _finally) {
					Get();
					Expect(_lbrace);
					while (StartOf(20)) {
						Statement(/*Parser.Statement.atg:511*/a.FinallyBlock);
					}
					Expect(_rbrace);
				}
			} else {
				Get();
				Expect(_lbrace);
				while (StartOf(20)) {
					Statement(/*Parser.Statement.atg:518*/a.FinallyBlock);
				}
				Expect(_rbrace);
				if (la.kind == _catch) {
					Get();
					if (la.kind == _lpar) {
						Get();
						GetCall(/*Parser.Statement.atg:524*/out a.ExceptionVar);
						Expect(_rpar);
					}
					Expect(_lbrace);
					while (StartOf(20)) {
						Statement(/*Parser.Statement.atg:528*/a.CatchBlock);
					}
					Expect(_rbrace);
				}
			}
		}
		/*Parser.Statement.atg:533*/block.Add(a); 
	}

	void Using(/*Parser.Statement.atg:544*/AstBlock block) {
		/*Parser.Statement.atg:544*/AstUsing use = new AstUsing(this); 
		Expect(_uusing);
		Expect(_lpar);
		GetCall(/*Parser.Statement.atg:546*/out use.Container);
		Expect(_assign);
		Expr(/*Parser.Statement.atg:546*/out use.Expression);
		Expect(_rpar);
		StatementBlock(/*Parser.Statement.atg:547*/use.Block);
		/*Parser.Statement.atg:548*/block.Add(use); 
	}

	void Assignment(/*Parser.Statement.atg:320*/AstGetSet lvalue) {
		/*Parser.Statement.atg:320*/IAstExpression expr = null;
		BinaryOperator setModifier = BinaryOperator.None;
		IAstType T;
		
		if (StartOf(9)) {
			switch (la.kind) {
			case _assign: {
				Get();
				/*Parser.Statement.atg:326*/setModifier = BinaryOperator.None; 
				break;
			}
			case _plus: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:327*/setModifier = BinaryOperator.Addition; 
				break;
			}
			case _minus: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:328*/setModifier = BinaryOperator.Subtraction; 
				break;
			}
			case _times: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:329*/setModifier = BinaryOperator.Multiply; 
				break;
			}
			case _div: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:330*/setModifier = BinaryOperator.Division; 
				break;
			}
			case _bitAnd: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:331*/setModifier = BinaryOperator.BitwiseAnd; 
				break;
			}
			case _bitOr: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:332*/setModifier = BinaryOperator.BitwiseOr; 
				break;
			}
			case _coalescence: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:333*/setModifier = BinaryOperator.Coalescence; 
				break;
			}
			}
			Expr(/*Parser.Statement.atg:334*/out expr);
		} else if (la.kind == _tilde) {
			Get();
			Expect(_assign);
			/*Parser.Statement.atg:336*/setModifier = BinaryOperator.Cast; 
			TypeExpr(/*Parser.Statement.atg:337*/out T);
			/*Parser.Statement.atg:337*/expr = T; 
		} else SynErr(144);
		/*Parser.Statement.atg:339*/lvalue.Arguments.Add(expr);
		lvalue.Call = PCall.Set; 
		lvalue.SetModifier = setModifier;
		
	}

	void Function(/*Parser.Statement.atg:261*/out AstGetSetSymbol function) {
		/*Parser.Statement.atg:261*/function = null; string id; 
		Id(/*Parser.Statement.atg:263*/out id);
		/*Parser.Statement.atg:263*/if(!target.Symbols.ContainsKey(id))
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
		
		Arguments(/*Parser.Statement.atg:276*/function.Arguments);
	}

	void Variable(/*Parser.Statement.atg:231*/out AstGetSetSymbol variable, out bool isDeclared) {
		/*Parser.Statement.atg:231*/variable = null; string id; isDeclared = false; 
		if (la.kind == _var || la.kind == _ref || la.kind == _static) {
			VariableDeclaration(/*Parser.Statement.atg:233*/out variable);
			/*Parser.Statement.atg:233*/isDeclared = true; 
		} else if (StartOf(4)) {
			Id(/*Parser.Statement.atg:234*/out id);
			/*Parser.Statement.atg:234*/if(target.Symbols.ContainsKey(id))
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
			
		} else SynErr(145);
	}

	void StaticCall(/*Parser.Statement.atg:280*/out AstGetSetStatic staticCall) {
		/*Parser.Statement.atg:280*/IAstType typeExpr;
		string memberId;
		staticCall = null;
		
		ExplicitTypeExpr(/*Parser.Statement.atg:285*/out typeExpr);
		Expect(_dot);
		Id(/*Parser.Statement.atg:286*/out memberId);
		/*Parser.Statement.atg:286*/staticCall = new AstGetSetStatic(this, PCall.Get, typeExpr, memberId); 
		Arguments(/*Parser.Statement.atg:287*/staticCall.Arguments);
	}

	void VariableDeclaration(/*Parser.Statement.atg:205*/out AstGetSetSymbol variable) {
		/*Parser.Statement.atg:205*/variable = null; string staticId = null; 
		/*Parser.Statement.atg:206*/string id = null; SymbolInterpretations kind = SymbolInterpretations.Undefined; 
		if (la.kind == _var || la.kind == _ref) {
			if (la.kind == _var) {
				Get();
				/*Parser.Statement.atg:207*/kind = SymbolInterpretations.LocalObjectVariable; 
			} else {
				Get();
				/*Parser.Statement.atg:208*/kind = SymbolInterpretations.LocalReferenceVariable; 
			}
			Id(/*Parser.Statement.atg:211*/out id);
			/*Parser.Statement.atg:212*/SmartDeclareLocal(id, kind);
			staticId = id; 
			
		} else if (la.kind == _static) {
			Get();
			/*Parser.Statement.atg:215*/kind = SymbolInterpretations.GlobalObjectVariable; 
			if (la.kind == _var || la.kind == _ref) {
				if (la.kind == _var) {
					Get();
				} else {
					Get();
					/*Parser.Statement.atg:217*/kind = SymbolInterpretations.GlobalReferenceVariable; 
				}
			}
			Id(/*Parser.Statement.atg:219*/out id);
			/*Parser.Statement.atg:219*/staticId = target.Function.Id + "\\static\\" + id;
			target.Declare(kind, id, staticId);
			if(!target.Loader.Options.TargetApplication.Variables.ContainsKey(staticId))
			    target.Loader.Options.TargetApplication.Variables.Add(staticId, new PVariable(staticId));
			
		} else SynErr(146);
		/*Parser.Statement.atg:224*/variable = InterpretationIsObjectVariable(kind) ?
		new AstGetSetSymbol(this, PCall.Get, staticId, kind)
		:
			new AstGetSetReference(this, PCall.Get, staticId, InterpretAsObjectVariable(kind)); 
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

		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,T, T,x,x,x, T,T,T,T, T,T,x,T, x,x,T,T, T,T,T,T, T,x,T,T, T,x,T,x, T,T,T,T, T,x,x,x, x,T,T,T, T,T,x,x, T,x,x,x, x,x},
		{x,T,T,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,T, T,T,x,T, x,x,T,T, T,T,T,T, T,x,T,T, T,x,T,x, T,T,T,T, T,x,x,x, x,T,T,T, T,T,x,x, T,x,x,x, x,x},
		{x,T,T,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,T,x, x,T,x,x, x,x,x,x, T,x,x,T, T,x,x,x, T,T,T,T, T,T,x,T, x,x,T,T, T,T,T,T, T,x,T,T, T,x,T,x, T,T,T,T, T,x,x,x, x,T,T,T, T,T,x,x, T,x,x,x, x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,T, x,x,x,T, x,x,x,T, T,T,x,T, T,x,T,T, T,x,T,x, T,T,T,T, T,x,x,x, x,T,T,T, T,T,x,x, T,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, T,T,T,T, x,x,x,T, x,x,x,x, x,T,T,T, T,x,x,T, x,x,T,x, x,x,x,T, x,x,x,T, x,x,T,x, x,x,x,T, T,T,T,x, x,x,x,T, T,T,x,x, T,x,T,x, x,x,T,x, x,x,x,x, x,x,x,T, T,x,x,x, T,T,x,T, x,T,T,T, T,x,x,x, T,x,x,T, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, T,T,x,x, T,x,x,x, x,x,x,x, x,x,x,T, x,T,T,x, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, T,T,T,T, T,T,T,T, T,T,x,T, x,T,T,T, T,T,T,T, T,T,T,T, T,T,T,x, T,T,x,x, x,x,T,x, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,T,T, T,x,x,x, x,x,x,T, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,T, x,T,x,x, x,T,x,T, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,x,T,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,T,x,x, x,x,x,T, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, T,T,T,T, x,x,x,T, x,x,x,x, x,T,T,T, T,x,x,T, x,x,T,x, x,x,x,T, x,x,x,T, x,x,T,x, x,x,x,T, T,T,T,x, x,x,x,T, T,T,x,x, T,x,T,x, x,x,T,x, x,x,x,x, x,x,x,T, T,T,T,x, T,T,x,T, x,T,T,T, T,x,x,x, T,x,x,T, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,T,T,T, T,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,T, x,x,x,T, x,x,T,x, x,x,x,T, T,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,x,T,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, T,x,x,x, x,x,x,x, x,x,x,T, x,T,x,T, x,x,T,x, x,x,x,T, T,x,x,x, x,x,x,x, T,T,T,x, T,T,T,T, x,x,T,T, T,T,x,x, x,x,T,T, x,T,T,x, x,T,x,T, x,T,T,T, T,T,x,x, T,T,x,x, x,x},
		{x,T,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,T, x,x,x,T, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,x,T,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,x,T,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,T, x,T,x,T, x,x,T,x, x,x,x,T, T,x,x,x, x,x,x,x, T,T,x,x, x,T,T,T, x,x,T,T, T,T,x,x, x,x,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x},
		{x,T,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,T, x,x,x,T, x,x,T,x, x,x,x,T, T,x,x,x, x,x,x,x, T,T,x,x, x,T,T,T, x,x,T,T, T,T,x,x, x,x,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,T,x,T, x,T,T,T, T,T,x,x, x,T,x,x, x,x},
		{x,x,x,x, T,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,T, x,x,x,T, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,T,x,x, x,x,T,x, x,T,T,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,T,x,x, x,x,x,x, x,T,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,T,x,x, x,x,x,x, x,T,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, T,T,T,T, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,x,x,x, x,x,x,x, T,T,x,x, T,x,x,x, x,x,x,x, x,x,x,T, x,T,T,x, x,x,x,T, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x},
		{x,T,T,x, T,T,T,T, x,T,x,T, x,x,x,x, x,T,T,T, T,x,x,T, x,x,T,x, x,x,x,T, x,x,x,T, x,x,T,x, x,x,x,T, T,T,T,x, x,x,x,T, T,T,x,x, T,x,T,x, x,x,T,x, x,x,x,x, x,x,x,T, T,T,T,x, T,T,x,T, x,T,T,T, T,x,x,x, T,x,x,T, x,x}

#line 132 "F:\DotNetProjects\Prexonite\Tools\Parser.frame" //FRAME

	};
} // end Parser

[NoDebug()]
internal class Errors {
	internal int count = 0;                                    // number of errors detected
	internal System.IO.TextWriter errorStream = Console.Out;   // error messages go to this stream
    internal string errMsgFormat = "-- ({3}) line {0} col {1}: {2}"; // 0=line, 1=column, 2=text, 3=file
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
			case 89: s = "uusing expected"; break;
			case 90: s = "ENDKEYWORDS expected"; break;
			case 91: s = "LPopExpr expected"; break;
			case 92: s = "??? expected"; break;
			case 93: s = "invalid AsmStatementBlock"; break;
			case 94: s = "invalid AsmInstruction"; break;
			case 95: s = "invalid AsmInstruction"; break;
			case 96: s = "invalid AsmInstruction"; break;
			case 97: s = "invalid AsmInstruction"; break;
			case 98: s = "invalid AsmInstruction"; break;
			case 99: s = "invalid AsmId"; break;
			case 100: s = "invalid SignedReal"; break;
			case 101: s = "invalid Boolean"; break;
			case 102: s = "invalid Id"; break;
			case 103: s = "invalid AtomicExpr"; break;
			case 104: s = "invalid AssignExpr"; break;
			case 105: s = "invalid AssignExpr"; break;
			case 106: s = "invalid TypeExpr"; break;
			case 107: s = "invalid GetSetExtension"; break;
			case 108: s = "invalid Primary"; break;
			case 109: s = "invalid Constant"; break;
			case 110: s = "invalid LoopExpr"; break;
			case 111: s = "invalid LambdaExpression"; break;
			case 112: s = "invalid LambdaExpression"; break;
			case 113: s = "invalid GetInitiator"; break;
			case 114: s = "invalid GetInitiator"; break;
			case 115: s = "invalid WhileLoop"; break;
			case 116: s = "invalid WhileLoop"; break;
			case 117: s = "invalid ForLoop"; break;
			case 118: s = "invalid Arguments"; break;
			case 119: s = "invalid Statement"; break;
			case 120: s = "invalid ExplicitTypeExpr"; break;
			case 121: s = "invalid PrexoniteTypeExpr"; break;
			case 122: s = "invalid ClrTypeExpr"; break;
			case 123: s = "invalid TypeExprElement"; break;
			case 124: s = "this symbol not expected in Prexonite"; break;
			case 125: s = "this symbol not expected in GlobalVariableDefinition"; break;
			case 126: s = "invalid GlobalVariableDefinition"; break;
			case 127: s = "this symbol not expected in GlobalVariableDefinition"; break;
			case 128: s = "this symbol not expected in Declaration"; break;
			case 129: s = "invalid MetaAssignment"; break;
			case 130: s = "invalid MetaAssignment"; break;
			case 131: s = "this symbol not expected in BuildBlock"; break;
			case 132: s = "invalid FunctionDefinition"; break;
			case 133: s = "this symbol not expected in FunctionDefinition"; break;
			case 134: s = "invalid FunctionDefinition"; break;
			case 135: s = "invalid GlobalId"; break;
			case 136: s = "invalid MetaExpr"; break;
			case 137: s = "invalid GlobalQualifiedId"; break;
			case 138: s = "invalid ExplicitLabel"; break;
			case 139: s = "invalid SimpleStatement"; break;
			case 140: s = "invalid StructureStatement"; break;
			case 141: s = "invalid GetSetComplex"; break;
			case 142: s = "invalid Return"; break;
			case 143: s = "invalid Condition"; break;
			case 144: s = "invalid Assignment"; break;
			case 145: s = "invalid Variable"; break;
			case 146: s = "invalid VariableDeclaration"; break;

#line 146 "F:\DotNetProjects\Prexonite\Tools\Parser.frame" //FRAME

			default: s = "error " + n; break;
		}
		if(s.EndsWith(" expected"))
		    s += " and not \"" + parentParser.t.ToString(false) + " " + parentParser.la.ToString(false) + "\"";
		else if(s.StartsWith("this symbol "))
		    s = "\"" + parentParser.t.val + "\"" + s.Substring(12);
		errorStream.WriteLine(errMsgFormat, line, col, s, parentParser.scanner.File);
		count++;
	}

	internal void SemErr (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s, parentParser.scanner.File);
		count++;
	}
	
	internal void SemErr (string s) {
		errorStream.WriteLine(s);
		count++;
	}
	
	internal void Warning (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s, parentParser.scanner.File);
	}
	
	internal void Warning(string s) {
		errorStream.WriteLine(s);
	}
} // Errors


#line default //END FRAME $$$

}