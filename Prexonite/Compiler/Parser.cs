//SOURCE ARRAY
/*Header.atg:29*/using System.IO;
using Prexonite;
using System.Collections.Generic;
using System.Linq;
using FatalError = Prexonite.Compiler.FatalCompilerException;
using StringBuilder = System.Text.StringBuilder;
using Prexonite.Compiler.Ast;
using Prexonite.Types;//END SOURCE ARRAY


#line 27 "D:\DotNetProjects\Prexonite\Tools\Parser.frame" //FRAME

using System;


#line default //END FRAME -->namespace

namespace Prexonite.Compiler {


#line 30 "D:\DotNetProjects\Prexonite\Tools\Parser.frame" //FRAME


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
	public const int _macro = 90;
	public const int _lazy = 91;
	public const int _let = 92;
	public const int _ENDKEYWORDS = 93;
	public const int _LPopExpr = 94;
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
		@macro = 90,
		@lazy = 91,
		@let = 92,
		@ENDKEYWORDS = 93,
		@LPopExpr = 94,
	}
	const int maxT = 95;

#line 43 "D:\DotNetProjects\Prexonite\Tools\Parser.frame" //FRAME

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

#line 55 "D:\DotNetProjects\Prexonite\Tools\Parser.frame" //FRAME


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


#line 82 "D:\DotNetProjects\Prexonite\Tools\Parser.frame" //FRAME

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
		} else SynErr(96);
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
			target.Symbols.Add(id, new SymbolEntry(kind, id));
			
			while (la.kind == _comma) {
				Get();
				AsmId(/*Parser.Assembler.atg:60*/out id);
				/*Parser.Assembler.atg:62*/target.Function.Variables.Add(id);
				target.Symbols.Add(id, new SymbolEntry(kind, id));
				
			}
		} else if (/*Parser.Assembler.atg:68*/isInNullGroup()) {
			AsmId(/*Parser.Assembler.atg:68*/out insbase);
			if (la.kind == _dot) {
				Get();
				AsmId(/*Parser.Assembler.atg:69*/out detail);
			}
			/*Parser.Assembler.atg:70*/code = getOpCode(insbase, detail);
			addInstruction(block, new Instruction(code));
			
		} else if (/*Parser.Assembler.atg:76*/isAsmInstruction("label",null) ) {
			AsmId(/*Parser.Assembler.atg:76*/out insbase);
			AsmId(/*Parser.Assembler.atg:79*/out id);
			/*Parser.Assembler.atg:80*/addLabel(block, id); 
		} else if (/*Parser.Assembler.atg:83*/isAsmInstruction("nop", null)) {
			AsmId(/*Parser.Assembler.atg:83*/out insbase);
			/*Parser.Assembler.atg:83*/Instruction ins = new Instruction(OpCode.nop); 
			if (la.kind == _plus) {
				Get();
				AsmId(/*Parser.Assembler.atg:84*/out id);
				/*Parser.Assembler.atg:84*/ins.Id = id; 
			}
			/*Parser.Assembler.atg:86*/addInstruction(block, ins); 
		} else if (/*Parser.Assembler.atg:90*/isAsmInstruction("rot", null)) {
			AsmId(/*Parser.Assembler.atg:90*/out insbase);
			Expect(_dot);
			Integer(/*Parser.Assembler.atg:91*/out rotations);
			Expect(_comma);
			Integer(/*Parser.Assembler.atg:92*/out values);
			/*Parser.Assembler.atg:94*/addInstruction(block, Instruction.CreateRotate(rotations, values)); 
		} else if (/*Parser.Assembler.atg:98*/isAsmInstruction("indloci", null)) {
			if (la.kind == _at) {
				Get();
				/*Parser.Assembler.atg:98*/justEffect = true; 
			}
			AsmId(/*Parser.Assembler.atg:100*/out insbase);
			Expect(_dot);
			Integer(/*Parser.Assembler.atg:101*/out arguments);
			Integer(/*Parser.Assembler.atg:102*/out index);
			/*Parser.Assembler.atg:104*/addInstruction(block, Instruction.CreateIndLocI(index, arguments, justEffect)); 
		} else if (/*Parser.Assembler.atg:107*/isAsmInstruction("swap", null)) {
			AsmId(/*Parser.Assembler.atg:107*/out insbase);
			/*Parser.Assembler.atg:108*/addInstruction(block, Instruction.CreateExchange()); 
		} else if (/*Parser.Assembler.atg:113*/isAsmInstruction("ldc", "real")) {
			AsmId(/*Parser.Assembler.atg:113*/out insbase);
			Expect(_dot);
			AsmId(/*Parser.Assembler.atg:115*/out detail);
			SignedReal(/*Parser.Assembler.atg:116*/out dblArg);
			/*Parser.Assembler.atg:117*/addInstruction(block, Instruction.CreateConstant(dblArg)); 
		} else if (/*Parser.Assembler.atg:122*/isAsmInstruction("ldc", "bool")) {
			AsmId(/*Parser.Assembler.atg:122*/out insbase);
			Expect(_dot);
			AsmId(/*Parser.Assembler.atg:124*/out detail);
			Boolean(/*Parser.Assembler.atg:125*/out bolArg);
			/*Parser.Assembler.atg:126*/addInstruction(block, Instruction.CreateConstant(bolArg)); 
		} else if (/*Parser.Assembler.atg:131*/isInIntegerGroup()) {
			AsmId(/*Parser.Assembler.atg:131*/out insbase);
			if (la.kind == _dot) {
				Get();
				AsmId(/*Parser.Assembler.atg:132*/out detail);
			}
			SignedInteger(/*Parser.Assembler.atg:133*/out arguments);
			/*Parser.Assembler.atg:134*/code = getOpCode(insbase, detail);
			addInstruction(block, new Instruction(code, arguments));
			
		} else if (/*Parser.Assembler.atg:140*/isInJumpGroup()) {
			AsmId(/*Parser.Assembler.atg:140*/out insbase);
			if (la.kind == _dot) {
				Get();
				AsmId(/*Parser.Assembler.atg:141*/out detail);
			}
			/*Parser.Assembler.atg:142*/Instruction ins = null;
			code = getOpCode(insbase, detail);
			
			if (StartOf(2)) {
				AsmId(/*Parser.Assembler.atg:146*/out id);
				/*Parser.Assembler.atg:148*/ins = new Instruction(code, -1, id);
				
			} else if (la.kind == _integer) {
				Integer(/*Parser.Assembler.atg:150*/out arguments);
				/*Parser.Assembler.atg:150*/ins = new Instruction(code, arguments); 
			} else SynErr(97);
			/*Parser.Assembler.atg:151*/addInstruction(block, ins); 
		} else if (/*Parser.Assembler.atg:156*/isInIdGroup()) {
			AsmId(/*Parser.Assembler.atg:156*/out insbase);
			if (la.kind == _dot) {
				Get();
				AsmId(/*Parser.Assembler.atg:157*/out detail);
			}
			AsmId(/*Parser.Assembler.atg:158*/out id);
			/*Parser.Assembler.atg:159*/code = getOpCode(insbase, detail);
			addInstruction(block, new Instruction(code, id));
			
		} else if (/*Parser.Assembler.atg:166*/isInIdArgGroup()) {
			if (la.kind == _at) {
				Get();
				/*Parser.Assembler.atg:166*/justEffect = true; 
			}
			AsmId(/*Parser.Assembler.atg:168*/out insbase);
			if (la.kind == _dot) {
				Get();
				Integer(/*Parser.Assembler.atg:169*/out arguments);
			} else if (StartOf(2)) {
				/*Parser.Assembler.atg:170*/arguments = 0; 
			} else SynErr(98);
			AsmId(/*Parser.Assembler.atg:172*/out id);
			/*Parser.Assembler.atg:173*/code = getOpCode(insbase, detail);
			addInstruction(block, new Instruction(code, arguments, id, justEffect));
			
		} else if (/*Parser.Assembler.atg:179*/isInArgGroup()) {
			if (la.kind == _at) {
				Get();
				/*Parser.Assembler.atg:179*/justEffect = true; 
			}
			AsmId(/*Parser.Assembler.atg:181*/out insbase);
			if (la.kind == _dot) {
				Get();
				Integer(/*Parser.Assembler.atg:182*/out arguments);
			} else if (StartOf(3)) {
				/*Parser.Assembler.atg:183*/arguments = 0; 
			} else SynErr(99);
			/*Parser.Assembler.atg:185*/code = getOpCode(insbase, detail);
			addInstruction(block, new Instruction(code, arguments, null, justEffect));
			
		} else if (/*Parser.Assembler.atg:191*/isInQualidArgGroup()) {
			if (la.kind == _at) {
				Get();
				/*Parser.Assembler.atg:191*/justEffect = true; 
			}
			AsmId(/*Parser.Assembler.atg:193*/out insbase);
			if (la.kind == _dot) {
				Get();
				Integer(/*Parser.Assembler.atg:194*/out arguments);
			} else if (StartOf(2)) {
				/*Parser.Assembler.atg:195*/arguments = 0; 
			} else SynErr(100);
			AsmQualid(/*Parser.Assembler.atg:197*/out id);
			/*Parser.Assembler.atg:198*/code = getOpCode(insbase, detail);
			addInstruction(block, new Instruction(code, arguments, id, justEffect));
			
		} else if (StartOf(2)) {
			AsmId(/*Parser.Assembler.atg:203*/out insbase);
			/*Parser.Assembler.atg:203*/SemErr("Invalid assembler instruction \"" + insbase + "\" (" + t + ")."); 
		} else SynErr(101);
	}

	void AsmId(/*Parser.Assembler.atg:207*/out string id) {
		/*Parser.Assembler.atg:207*/id = "\\NoId\\"; 
		if (la.kind == _string) {
			String(/*Parser.Assembler.atg:209*/out id);
		} else if (StartOf(4)) {
			Id(/*Parser.Assembler.atg:210*/out id);
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
			/*Parser.Assembler.atg:241*/id = cache(t.val); 
		} else SynErr(102);
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
		} else SynErr(103);
		/*Parser.Helper.atg:83*/value = modifier * value; 
	}

	void Boolean(/*Parser.Helper.atg:40*/out bool value) {
		/*Parser.Helper.atg:40*/value = true; 
		if (la.kind == _true) {
			Get();
		} else if (la.kind == _false) {
			Get();
			/*Parser.Helper.atg:43*/value = false; 
		} else SynErr(104);
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

	void AsmQualid(/*Parser.Assembler.atg:245*/out string qualid) {
		
		AsmId(/*Parser.Assembler.atg:247*/out qualid);
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
		} else SynErr(105);
	}

	void Expr(/*Parser.Expression.atg:28*/out IAstExpression expr) {
		/*Parser.Expression.atg:28*/AstGetSet complex = null; 
		KeyValuePairExpr(/*Parser.Expression.atg:30*/out expr);
		while (la.kind == _appendright) {
			Get();
			GetCall(/*Parser.Expression.atg:33*/out complex);
			/*Parser.Expression.atg:33*/complex.Arguments.RightAppend(expr); 
			complex.Arguments.ReleaseRightAppend();
			if(complex is AstGetSetSymbol && ((AstGetSetSymbol)complex).IsVariable)
			    complex.Call = PCall.Set;
			expr = complex;										    
			
		}
	}

	void KeyValuePairExpr(/*Parser.Expression.atg:43*/out IAstExpression expr) {
		AtomicExpr(/*Parser.Expression.atg:44*/out expr);
		if (la.kind == _colon) {
			Get();
			/*Parser.Expression.atg:45*/IAstExpression value; 
			Expr(/*Parser.Expression.atg:46*/out value);
			/*Parser.Expression.atg:46*/expr = new AstKeyValuePair(this, expr, value); 
		}
	}

	void GetCall(/*Parser.Statement.atg:457*/out AstGetSet complex) {
		/*Parser.Statement.atg:457*/AstGetSet getMember = null; bool isDeclaration; 
		GetInitiator(/*Parser.Statement.atg:459*/out complex, out isDeclaration);
		while (la.kind == _dot || la.kind == _lbrack) {
			GetSetExtension(/*Parser.Statement.atg:460*/complex, out getMember);
		}
		/*Parser.Statement.atg:462*/if(getMember != null) 
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

	void AtomicExpr(/*Parser.Expression.atg:50*/out IAstExpression expr) {
		/*Parser.Expression.atg:50*/AstConditionalExpression cexpr = null; expr = null; 
		if (StartOf(7)) {
			OrExpr(/*Parser.Expression.atg:52*/out expr);
			while (la.kind == _question) {
				Get();
				/*Parser.Expression.atg:54*/cexpr = new AstConditionalExpression(this, expr); 
				AtomicExpr(/*Parser.Expression.atg:55*/out cexpr.IfExpression);
				Expect(_colon);
				AtomicExpr(/*Parser.Expression.atg:57*/out cexpr.ElseExpression);
				/*Parser.Expression.atg:57*/expr = cexpr; 
			}
		} else if (la.kind == _if || la.kind == _unless) {
			/*Parser.Expression.atg:59*/bool isNegated = false; 
			if (la.kind == _if) {
				Get();
			} else {
				Get();
				/*Parser.Expression.atg:61*/isNegated = true; 
			}
			Expect(_lpar);
			OrExpr(/*Parser.Expression.atg:63*/out expr);
			Expect(_rpar);
			/*Parser.Expression.atg:63*/cexpr = new AstConditionalExpression(this, expr, isNegated); 
			AtomicExpr(/*Parser.Expression.atg:64*/out cexpr.IfExpression);
			Expect(_else);
			AtomicExpr(/*Parser.Expression.atg:66*/out cexpr.ElseExpression);
			/*Parser.Expression.atg:66*/expr = cexpr; 
		} else SynErr(106);
	}

	void OrExpr(/*Parser.Expression.atg:70*/out IAstExpression expr) {
		/*Parser.Expression.atg:70*/IAstExpression lhs, rhs; 
		AndExpr(/*Parser.Expression.atg:72*/out lhs);
		/*Parser.Expression.atg:72*/expr = lhs; 
		if (la.kind == _or) {
			Get();
			OrExpr(/*Parser.Expression.atg:73*/out rhs);
			/*Parser.Expression.atg:73*/expr = new AstLogicalOr(this, lhs, rhs); 
		}
	}

	void AndExpr(/*Parser.Expression.atg:79*/out IAstExpression expr) {
		/*Parser.Expression.atg:79*/IAstExpression lhs, rhs; 
		BitOrExpr(/*Parser.Expression.atg:81*/out lhs);
		/*Parser.Expression.atg:81*/expr = lhs; 
		if (la.kind == _and) {
			Get();
			AndExpr(/*Parser.Expression.atg:82*/out rhs);
			/*Parser.Expression.atg:82*/expr = new AstLogicalAnd(this, lhs, rhs); 
		}
	}

	void BitOrExpr(/*Parser.Expression.atg:87*/out IAstExpression expr) {
		/*Parser.Expression.atg:87*/IAstExpression lhs, rhs; 
		BitXorExpr(/*Parser.Expression.atg:89*/out lhs);
		/*Parser.Expression.atg:89*/expr = lhs; 
		while (la.kind == _bitOr) {
			Get();
			BitXorExpr(/*Parser.Expression.atg:90*/out rhs);
			/*Parser.Expression.atg:90*/expr = new AstBinaryOperator(this, expr, BinaryOperator.BitwiseOr, rhs); 
		}
	}

	void BitXorExpr(/*Parser.Expression.atg:95*/out IAstExpression expr) {
		/*Parser.Expression.atg:95*/IAstExpression lhs, rhs; 
		BitAndExpr(/*Parser.Expression.atg:97*/out lhs);
		/*Parser.Expression.atg:97*/expr = lhs; 
		while (la.kind == _xor) {
			Get();
			BitAndExpr(/*Parser.Expression.atg:98*/out rhs);
			/*Parser.Expression.atg:99*/expr = new AstBinaryOperator(this, expr, BinaryOperator.ExclusiveOr, rhs); 
		}
	}

	void BitAndExpr(/*Parser.Expression.atg:104*/out IAstExpression expr) {
		/*Parser.Expression.atg:104*/IAstExpression lhs, rhs; 
		NotExpr(/*Parser.Expression.atg:106*/out lhs);
		/*Parser.Expression.atg:106*/expr = lhs; 
		while (la.kind == _bitAnd) {
			Get();
			NotExpr(/*Parser.Expression.atg:107*/out rhs);
			/*Parser.Expression.atg:108*/expr = new AstBinaryOperator(this, expr, BinaryOperator.BitwiseAnd, rhs); 
		}
	}

	void NotExpr(/*Parser.Expression.atg:113*/out IAstExpression expr) {
		/*Parser.Expression.atg:113*/IAstExpression lhs; bool isNot = false; 
		if (la.kind == _not) {
			Get();
			/*Parser.Expression.atg:115*/isNot = true; 
		}
		EqlExpr(/*Parser.Expression.atg:117*/out lhs);
		/*Parser.Expression.atg:117*/expr = isNot ? new AstUnaryOperator(this, UnaryOperator.LogicalNot, lhs) : lhs; 
	}

	void EqlExpr(/*Parser.Expression.atg:121*/out IAstExpression expr) {
		/*Parser.Expression.atg:121*/IAstExpression lhs, rhs; BinaryOperator op = BinaryOperator.None; 
		RelExpr(/*Parser.Expression.atg:123*/out lhs);
		/*Parser.Expression.atg:123*/expr = lhs; 
		while (la.kind == _eq || la.kind == _ne) {
			if (la.kind == _eq) {
				Get();
				/*Parser.Expression.atg:124*/op = BinaryOperator.Equality; 
			} else {
				Get();
				/*Parser.Expression.atg:125*/op = BinaryOperator.Inequality; 
			}
			RelExpr(/*Parser.Expression.atg:126*/out rhs);
			/*Parser.Expression.atg:126*/expr = new AstBinaryOperator(this, expr, op, rhs); 
		}
	}

	void RelExpr(/*Parser.Expression.atg:131*/out IAstExpression expr) {
		/*Parser.Expression.atg:131*/IAstExpression lhs, rhs; BinaryOperator op = BinaryOperator.None;  
		CoalExpr(/*Parser.Expression.atg:133*/out lhs);
		/*Parser.Expression.atg:133*/expr = lhs; 
		while (StartOf(8)) {
			if (la.kind == _lt) {
				Get();
				/*Parser.Expression.atg:134*/op = BinaryOperator.LessThan;              
			} else if (la.kind == _le) {
				Get();
				/*Parser.Expression.atg:135*/op = BinaryOperator.LessThanOrEqual;       
			} else if (la.kind == _gt) {
				Get();
				/*Parser.Expression.atg:136*/op = BinaryOperator.GreaterThan;           
			} else {
				Get();
				/*Parser.Expression.atg:137*/op = BinaryOperator.GreaterThanOrEqual;    
			}
			CoalExpr(/*Parser.Expression.atg:138*/out rhs);
			/*Parser.Expression.atg:138*/expr = new AstBinaryOperator(this, expr, op, rhs); 
		}
	}

	void CoalExpr(/*Parser.Expression.atg:143*/out IAstExpression expr) {
		/*Parser.Expression.atg:143*/IAstExpression lhs, rhs; AstCoalescence coal = new AstCoalescence(this); 
		AddExpr(/*Parser.Expression.atg:145*/out lhs);
		/*Parser.Expression.atg:145*/expr = lhs; coal.Expressions.Add(lhs); 
		while (la.kind == _coalescence) {
			Get();
			AddExpr(/*Parser.Expression.atg:148*/out rhs);
			/*Parser.Expression.atg:148*/expr = coal; coal.Expressions.Add(rhs); 
		}
	}

	void AddExpr(/*Parser.Expression.atg:153*/out IAstExpression expr) {
		/*Parser.Expression.atg:153*/IAstExpression lhs,rhs; BinaryOperator op = BinaryOperator.None; 
		MulExpr(/*Parser.Expression.atg:155*/out lhs);
		/*Parser.Expression.atg:155*/expr = lhs; 
		while (la.kind == _minus || la.kind == _plus) {
			if (la.kind == _plus) {
				Get();
				/*Parser.Expression.atg:156*/op = BinaryOperator.Addition;      
			} else {
				Get();
				/*Parser.Expression.atg:157*/op = BinaryOperator.Subtraction;   
			}
			MulExpr(/*Parser.Expression.atg:158*/out rhs);
			/*Parser.Expression.atg:158*/expr = new AstBinaryOperator(this, expr, op, rhs); 
		}
	}

	void MulExpr(/*Parser.Expression.atg:163*/out IAstExpression expr) {
		/*Parser.Expression.atg:163*/IAstExpression lhs, rhs; BinaryOperator op = BinaryOperator.None; 
		PowExpr(/*Parser.Expression.atg:165*/out lhs);
		/*Parser.Expression.atg:165*/expr = lhs; 
		while (la.kind == _div || la.kind == _times || la.kind == _mod) {
			if (la.kind == _times) {
				Get();
				/*Parser.Expression.atg:166*/op = BinaryOperator.Multiply;      
			} else if (la.kind == _div) {
				Get();
				/*Parser.Expression.atg:167*/op = BinaryOperator.Division;        
			} else {
				Get();
				/*Parser.Expression.atg:168*/op = BinaryOperator.Modulus;       
			}
			PowExpr(/*Parser.Expression.atg:169*/out rhs);
			/*Parser.Expression.atg:169*/expr = new AstBinaryOperator(this, expr, op, rhs); 
		}
	}

	void PowExpr(/*Parser.Expression.atg:174*/out IAstExpression expr) {
		/*Parser.Expression.atg:174*/IAstExpression lhs, rhs; 
		AssignExpr(/*Parser.Expression.atg:176*/out lhs);
		/*Parser.Expression.atg:176*/expr = lhs; 
		while (la.kind == _pow) {
			Get();
			AssignExpr(/*Parser.Expression.atg:177*/out rhs);
			/*Parser.Expression.atg:177*/expr = new AstBinaryOperator(this, expr, BinaryOperator.Power, rhs); 
		}
	}

	void AssignExpr(/*Parser.Expression.atg:181*/out IAstExpression expr) {
		/*Parser.Expression.atg:181*/AstGetSet assignment; BinaryOperator setModifier = BinaryOperator.None;
		IAstType T;
		
		PostfixUnaryExpr(/*Parser.Expression.atg:185*/out expr);
		if (/*Parser.Expression.atg:187*/isAssignmentOperator()) {
			/*Parser.Expression.atg:187*/assignment = expr as AstGetSet;
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
					/*Parser.Expression.atg:198*/setModifier = BinaryOperator.None; 
					break;
				}
				case _plus: {
					Get();
					Expect(_assign);
					/*Parser.Expression.atg:199*/setModifier = BinaryOperator.Addition; 
					break;
				}
				case _minus: {
					Get();
					Expect(_assign);
					/*Parser.Expression.atg:200*/setModifier = BinaryOperator.Subtraction; 
					break;
				}
				case _times: {
					Get();
					Expect(_assign);
					/*Parser.Expression.atg:201*/setModifier = BinaryOperator.Multiply; 
					break;
				}
				case _div: {
					Get();
					Expect(_assign);
					/*Parser.Expression.atg:202*/setModifier = BinaryOperator.Division; 
					break;
				}
				case _bitAnd: {
					Get();
					Expect(_assign);
					/*Parser.Expression.atg:203*/setModifier = BinaryOperator.BitwiseAnd; 
					break;
				}
				case _bitOr: {
					Get();
					Expect(_assign);
					/*Parser.Expression.atg:204*/setModifier = BinaryOperator.BitwiseOr; 
					break;
				}
				case _coalescence: {
					Get();
					Expect(_assign);
					/*Parser.Expression.atg:205*/setModifier = BinaryOperator.Coalescence; 
					break;
				}
				}
				Expr(/*Parser.Expression.atg:206*/out expr);
			} else if (la.kind == _tilde) {
				Get();
				Expect(_assign);
				/*Parser.Expression.atg:208*/setModifier = BinaryOperator.Cast; 
				TypeExpr(/*Parser.Expression.atg:209*/out T);
				/*Parser.Expression.atg:209*/expr = T; 
			} else SynErr(107);
			/*Parser.Expression.atg:211*/assignment.Arguments.Add(expr); 
			if(setModifier == BinaryOperator.None)
			    expr = assignment;
			else
			    expr = new AstModifyingAssignment(this,setModifier, assignment);
			
		} else if (StartOf(10)) {
		} else SynErr(108);
	}

	void PostfixUnaryExpr(/*Parser.Expression.atg:221*/out IAstExpression expr) {
		/*Parser.Expression.atg:221*/IAstType type = null; AstGetSet extension; bool isInverted = false; 
		PrefixUnaryExpr(/*Parser.Expression.atg:223*/out expr);
		while (StartOf(11)) {
			if (la.kind == _tilde) {
				Get();
				TypeExpr(/*Parser.Expression.atg:224*/out type);
				/*Parser.Expression.atg:224*/expr = new AstTypecast(this, expr, type); 
			} else if (la.kind == _is) {
				Get();
				if (la.kind == _not) {
					Get();
					/*Parser.Expression.atg:225*/isInverted = true; 
				}
				TypeExpr(/*Parser.Expression.atg:226*/out type);
				/*Parser.Expression.atg:226*/expr = new AstTypecheck(this, expr, type);
				if(isInverted)
					expr = new AstUnaryOperator(this, UnaryOperator.LogicalNot, expr);
				
			} else if (la.kind == _inc) {
				Get();
				/*Parser.Expression.atg:230*/expr = new AstUnaryOperator(this, UnaryOperator.PostIncrement, expr); 
			} else if (la.kind == _dec) {
				Get();
				/*Parser.Expression.atg:231*/expr = new AstUnaryOperator(this, UnaryOperator.PostDecrement, expr); 
			} else {
				GetSetExtension(/*Parser.Expression.atg:232*/expr, out extension);
				/*Parser.Expression.atg:233*/expr = extension; 
			}
		}
	}

	void TypeExpr(/*Parser.Expression.atg:450*/out IAstType type) {
		/*Parser.Expression.atg:450*/type = null; 
		if (StartOf(12)) {
			PrexoniteTypeExpr(/*Parser.Expression.atg:452*/out type);
		} else if (la.kind == _ns || la.kind == _doublecolon) {
			ClrTypeExpr(/*Parser.Expression.atg:453*/out type);
		} else SynErr(109);
	}

	void PrefixUnaryExpr(/*Parser.Expression.atg:238*/out IAstExpression expr) {
		/*Parser.Expression.atg:238*/UnaryOperator op = UnaryOperator.None; 
		while (StartOf(13)) {
			if (la.kind == _plus) {
				Get();
			} else if (la.kind == _minus) {
				Get();
				/*Parser.Expression.atg:241*/op = UnaryOperator.UnaryNegation; 
			} else if (la.kind == _inc) {
				Get();
				/*Parser.Expression.atg:242*/op = UnaryOperator.PreIncrement; 
			} else {
				Get();
				/*Parser.Expression.atg:243*/op = UnaryOperator.PreDecrement; 
			}
		}
		Primary(/*Parser.Expression.atg:245*/out expr);
		/*Parser.Expression.atg:245*/if(op != UnaryOperator.None) expr = new AstUnaryOperator(this, op, expr); 
	}

	void GetSetExtension(/*Parser.Statement.atg:115*/IAstExpression subject, out AstGetSet extension) {
		/*Parser.Statement.atg:115*/extension = null; string id;
		if(subject == null)
		{
			SemErr("Member access not preceded by a proper expression.");
			subject = new AstConstant(this,null);
		}
		                             
		if (/*Parser.Statement.atg:125*/isIndirectCall() ) {
			Expect(_dot);
			/*Parser.Statement.atg:125*/extension = new AstIndirectCall(this, PCall.Get, subject); 
			Arguments(/*Parser.Statement.atg:126*/extension.Arguments);
		} else if (la.kind == _dot) {
			Get();
			Id(/*Parser.Statement.atg:128*/out id);
			/*Parser.Statement.atg:128*/extension = new AstGetSetMemberAccess(this, PCall.Get, subject, id); 
			Arguments(/*Parser.Statement.atg:129*/extension.Arguments);
		} else if (la.kind == _lbrack) {
			/*Parser.Statement.atg:131*/IAstExpression expr; 
			extension = new AstGetSetMemberAccess(this, PCall.Get, subject, ""); 
			
			Get();
			if (StartOf(14)) {
				Expr(/*Parser.Statement.atg:135*/out expr);
				/*Parser.Statement.atg:135*/extension.Arguments.Add(expr); 
				while (WeakSeparator(_comma,14,15) ) {
					Expr(/*Parser.Statement.atg:136*/out expr);
					/*Parser.Statement.atg:136*/extension.Arguments.Add(expr); 
				}
			}
			Expect(_rbrack);
		} else SynErr(110);
	}

	void Primary(/*Parser.Expression.atg:249*/out IAstExpression expr) {
		/*Parser.Expression.atg:249*/expr = null;
		AstGetSet complex = null; bool declared; 
		if (la.kind == _asm) {
			/*Parser.Expression.atg:252*/_pushLexerState(Lexer.Asm); 
			/*Parser.Expression.atg:252*/AstBlockExpression blockExpr = new AstBlockExpression(this); 
			Get();
			Expect(_lpar);
			while (StartOf(1)) {
				AsmInstruction(/*Parser.Expression.atg:253*/blockExpr);
			}
			Expect(_rpar);
			/*Parser.Expression.atg:254*/_popLexerState(); 
			/*Parser.Expression.atg:254*/expr = blockExpr; 
		} else if (StartOf(16)) {
			Constant(/*Parser.Expression.atg:255*/out expr);
		} else if (la.kind == _coroutine) {
			CoroutineCreation(/*Parser.Expression.atg:256*/out expr);
		} else if (la.kind == _new) {
			ObjectCreation(/*Parser.Expression.atg:257*/out expr);
		} else if (la.kind == _lbrack) {
			ListLiteral(/*Parser.Expression.atg:258*/out expr);
		} else if (la.kind == _lbrace) {
			HashLiteral(/*Parser.Expression.atg:259*/out expr);
		} else if (StartOf(17)) {
			LoopExpr(/*Parser.Expression.atg:260*/out expr);
		} else if (la.kind == _throw) {
			/*Parser.Expression.atg:261*/AstThrow th; 
			ThrowExpression(/*Parser.Expression.atg:262*/out th);
			/*Parser.Expression.atg:262*/expr = th; 
		} else if (/*Parser.Expression.atg:264*/isLambdaExpression()) {
			LambdaExpression(/*Parser.Expression.atg:264*/out expr);
		} else if (la.kind == _lazy) {
			LazyExpression(/*Parser.Expression.atg:265*/out expr);
		} else if (StartOf(18)) {
			if (la.kind == _lpar) {
				Get();
				Expr(/*Parser.Expression.atg:267*/out expr);
				Expect(_rpar);
			} else {
				GetInitiator(/*Parser.Expression.atg:268*/out complex, out declared);
				/*Parser.Expression.atg:269*/expr = complex; 
			}
		} else if (la.kind == _LPopExpr) {
			Get();
			Expect(_lpar);
			Expr(/*Parser.Expression.atg:271*/out expr);
			/*Parser.Expression.atg:276*/_popLexerState(); _inject(_plus); 
			Expect(_rpar);
		} else SynErr(111);
	}

	void Constant(/*Parser.Expression.atg:281*/out IAstExpression expr) {
		/*Parser.Expression.atg:281*/expr = null; int vi; double vr; bool vb; string vs; 
		if (la.kind == _integer) {
			Integer(/*Parser.Expression.atg:283*/out vi);
			/*Parser.Expression.atg:283*/expr = new AstConstant(this, vi); 
		} else if (la.kind == _real) {
			Real(/*Parser.Expression.atg:284*/out vr);
			/*Parser.Expression.atg:284*/expr = new AstConstant(this, vr); 
		} else if (la.kind == _true || la.kind == _false) {
			Boolean(/*Parser.Expression.atg:285*/out vb);
			/*Parser.Expression.atg:285*/expr = new AstConstant(this, vb); 
		} else if (la.kind == _string) {
			String(/*Parser.Expression.atg:286*/out vs);
			/*Parser.Expression.atg:286*/expr = new AstConstant(this, vs); 
		} else if (la.kind == _null) {
			Null();
			/*Parser.Expression.atg:287*/expr = new AstConstant(this, null); 
		} else SynErr(112);
	}

	void CoroutineCreation(/*Parser.Expression.atg:339*/out IAstExpression expr) {
		/*Parser.Expression.atg:340*/AstCreateCoroutine cor = new AstCreateCoroutine(this); 
		IAstExpression iexpr;
		expr = cor;
		
		Expect(_coroutine);
		Expr(/*Parser.Expression.atg:345*/out iexpr);
		/*Parser.Expression.atg:345*/cor.Expression = iexpr; 
		if (la.kind == _for) {
			Get();
			Arguments(/*Parser.Expression.atg:346*/cor.Arguments);
		}
	}

	void ObjectCreation(/*Parser.Expression.atg:332*/out IAstExpression expr) {
		/*Parser.Expression.atg:332*/IAstType type; expr = null; 
		Expect(_new);
		TypeExpr(/*Parser.Expression.atg:334*/out type);
		/*Parser.Expression.atg:334*/AstObjectCreation creation = new AstObjectCreation(this, type); 
		Arguments(/*Parser.Expression.atg:335*/creation.Arguments);
		/*Parser.Expression.atg:335*/expr = creation; 
	}

	void ListLiteral(/*Parser.Expression.atg:291*/out IAstExpression expr) {
		/*Parser.Expression.atg:291*/IAstExpression iexpr = null; 
		AstListLiteral lst = new AstListLiteral(this);
		expr = lst;
		
		Expect(_lbrack);
		if (StartOf(14)) {
			Expr(/*Parser.Expression.atg:297*/out iexpr);
			/*Parser.Expression.atg:297*/lst.Elements.Add(iexpr); 
			while (la.kind == _comma) {
				Get();
				Expr(/*Parser.Expression.atg:299*/out iexpr);
				/*Parser.Expression.atg:299*/lst.Elements.Add(iexpr); 
			}
		}
		Expect(_rbrack);
	}

	void HashLiteral(/*Parser.Expression.atg:306*/out IAstExpression expr) {
		/*Parser.Expression.atg:306*/IAstExpression iexpr = null; 
		AstHashLiteral hash = new AstHashLiteral(this);
		expr = hash;
		
		Expect(_lbrace);
		if (StartOf(14)) {
			Expr(/*Parser.Expression.atg:312*/out iexpr);
			/*Parser.Expression.atg:312*/hash.Elements.Add(iexpr); 
			while (la.kind == _comma) {
				Get();
				Expr(/*Parser.Expression.atg:314*/out iexpr);
				/*Parser.Expression.atg:314*/hash.Elements.Add(iexpr); 
			}
		}
		Expect(_rbrace);
	}

	void LoopExpr(/*Parser.Expression.atg:321*/out IAstExpression expr) {
		/*Parser.Expression.atg:321*/AstBlock dummyBlock = new AstBlock(this);
		
		if (la.kind == _do || la.kind == _while || la.kind == _until) {
			WhileLoop(/*Parser.Expression.atg:324*/dummyBlock);
		} else if (la.kind == _for) {
			ForLoop(/*Parser.Expression.atg:325*/dummyBlock);
		} else if (la.kind == _foreach) {
			ForeachLoop(/*Parser.Expression.atg:326*/dummyBlock);
		} else SynErr(113);
		/*Parser.Expression.atg:327*/expr = new AstLoopExpression(this, (AstLoop) dummyBlock.Statements[0]); 
	}

	void ThrowExpression(/*Parser.Expression.atg:438*/out AstThrow th) {
		/*Parser.Expression.atg:438*/th = new AstThrow(this); 
		Expect(_throw);
		Expr(/*Parser.Expression.atg:441*/out th.Expression);
	}

	void LambdaExpression(/*Parser.Expression.atg:350*/out IAstExpression expr) {
		/*Parser.Expression.atg:350*/expr = null;
		PFunction func = new PFunction(TargetApplication, generateLocalId());                                             
		func.Meta[Application.ImportKey] = target.Function.Meta[Application.ImportKey];
		func.Meta[PFunction.ParentFunctionKey] = target.Function.Id;
		TargetApplication.Functions.Add(func);
		Loader.CreateFunctionTarget(func, new AstBlock(this));
		CompilerTarget ft = FunctionTargets[func];
		ft.ParentTarget = target;
		
		if (StartOf(19)) {
			FormalArg(/*Parser.Expression.atg:360*/ft);
		} else if (la.kind == _lpar) {
			Get();
			if (StartOf(19)) {
				FormalArg(/*Parser.Expression.atg:362*/ft);
				while (la.kind == _comma) {
					Get();
					FormalArg(/*Parser.Expression.atg:364*/ft);
				}
			}
			Expect(_rpar);
		} else SynErr(114);
		/*Parser.Expression.atg:370*/CompilerTarget oldTarget = target;
		target = ft;
		
		Expect(_implementation);
		if (la.kind == _lbrace) {
			Get();
			while (StartOf(20)) {
				Statement(/*Parser.Expression.atg:375*/ft.Ast);
			}
			Expect(_rbrace);
		} else if (StartOf(14)) {
			/*Parser.Expression.atg:377*/AstReturn ret = new AstReturn(this, ReturnVariant.Exit); 
			Expr(/*Parser.Expression.atg:378*/out ret.Expression);
			/*Parser.Expression.atg:378*/ft.Ast.Add(ret); 
		} else SynErr(115);
		/*Parser.Expression.atg:381*/target = oldTarget;
		if(errors.count == 0)
		{
		    //Emit code for top-level block
		    Ast[func].EmitCode(FunctionTargets[func],true);
		    FunctionTargets[func].FinishTarget();
		}
		
		expr = new AstCreateClosure(this, func.Id);                                         
		
	}

	void LazyExpression(/*Parser.Expression.atg:394*/out IAstExpression expr) {
		/*Parser.Expression.atg:394*/expr = null;
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
				Statement(/*Parser.Expression.atg:410*/ft.Ast);
			}
			Expect(_rbrace);
		} else if (StartOf(14)) {
			/*Parser.Expression.atg:412*/AstReturn ret = new AstReturn(this, ReturnVariant.Exit); 
			Expr(/*Parser.Expression.atg:413*/out ret.Expression);
			/*Parser.Expression.atg:413*/ft.Ast.Add(ret); 
		} else SynErr(116);
		/*Parser.Expression.atg:417*/var cap = ft.ToCaptureByValue(let_bindings(ft));
		
		//Restore parent target
		target = oldTarget;
		
		//Finish nested function
		if(errors.count == 0)
		{
		    Ast[func].EmitCode(FunctionTargets[func],true);
		    FunctionTargets[func].FinishTarget();
		}
		
		//Construct expr (appears in the place of lazy expression)
		var clo = new AstCreateClosure(this, func.Id);
		var thunk = new AstGetSetSymbol(this, Engine.ThunkAlias, SymbolInterpretations.Command);
		thunk.Arguments.Add(clo);
		thunk.Arguments.AddRange(cap(this)); //Add captured values
		expr = thunk;
		
	}

	void GetInitiator(/*Parser.Statement.atg:143*/out AstGetSet complex, out bool isDeclaration) {
		/*Parser.Statement.atg:143*/complex = null; 
		AstGetSetSymbol symbol = null;
		AstGetSetStatic staticCall = null;
		AstGetSet member = null;
		IAstExpression expr;
		List<IAstExpression> args = new List<IAstExpression>();
		isDeclaration = false;                                            
		string id;
		
		if (StartOf(21)) {
			if (/*Parser.Statement.atg:155*/isLikeFunction() || isUnknownId() ) {
				Function(/*Parser.Statement.atg:155*/out complex);
			} else if (StartOf(22)) {
				Variable(/*Parser.Statement.atg:156*/out complex, out isDeclaration);
			} else if (la.kind == _ns || la.kind == _tilde || la.kind == _doublecolon) {
				StaticCall(/*Parser.Statement.atg:157*/out staticCall);
			} else {
				Get();
				Expr(/*Parser.Statement.atg:158*/out expr);
				/*Parser.Statement.atg:158*/args.Add(expr); 
				while (la.kind == _comma) {
					Get();
					Expr(/*Parser.Statement.atg:159*/out expr);
					/*Parser.Statement.atg:159*/args.Add(expr); 
				}
				Expect(_rpar);
				if (la.kind == _dot || la.kind == _lbrack) {
					GetSetExtension(/*Parser.Statement.atg:162*/expr, out member);
					/*Parser.Statement.atg:163*/if(args.Count > 1)
					SemErr("A member access cannot have multiple subjects. (Did you mean '>>'?)");
					
				} else if (la.kind == _appendright) {
					Get();
					GetCall(/*Parser.Statement.atg:167*/out complex);
					/*Parser.Statement.atg:167*/complex.Arguments.RightAppend(args);
					complex.Arguments.ReleaseRightAppend();
					if(complex is AstGetSetSymbol && ((AstGetSetSymbol)complex).IsVariable)
					       complex.Call = PCall.Set;
					member = complex;
					
				} else SynErr(117);
			}
			/*Parser.Statement.atg:175*/complex = 
			(AstGetSet)symbol ?? 
			(AstGetSet)staticCall ?? 
			(AstGetSet)member ??
			complex; 
			
		} else if (/*Parser.Statement.atg:183*/isDeDereference() ) {
			Expect(_pointer);
			Expect(_pointer);
			Id(/*Parser.Statement.atg:183*/out id);
			/*Parser.Statement.atg:183*/SymbolEntry s = target.Symbols[id];
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
			Id(/*Parser.Statement.atg:203*/out id);
			/*Parser.Statement.atg:203*/SymbolEntry s = target.Symbols[id];
			if(s == null)
			{   
			    SemErr("The symbol " + id + " is not defined"); 
			    s = new SymbolEntry(SymbolInterpretations.LocalObjectVariable, id);
			}
			else if(InterpretationIsVariable(s.Interpretation))
			{
			    if(isOuterVariable(s.Id))
			        target.RequireOuterVariable(s.Id);
			}
			complex = new AstGetSetReference(this, s.Id, s.Interpretation);
			
		} else SynErr(118);
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

	void WhileLoop(/*Parser.Statement.atg:395*/AstBlock block) {
		/*Parser.Statement.atg:395*/AstWhileLoop loop = null;
		bool isPositive = true; 
		
		if (la.kind == _while || la.kind == _until) {
			if (la.kind == _while) {
				Get();
			} else {
				Get();
				/*Parser.Statement.atg:399*/isPositive = false; 
			}
			/*Parser.Statement.atg:400*/loop = new AstWhileLoop(this, true, isPositive); 
			Expect(_lpar);
			Expr(/*Parser.Statement.atg:401*/out loop.Condition);
			Expect(_rpar);
			/*Parser.Statement.atg:402*/target.BeginBlock(loop.Block); 
			StatementBlock(/*Parser.Statement.atg:403*/loop.Block);
		} else if (la.kind == _do) {
			Get();
			/*Parser.Statement.atg:405*/AstLoopBlock loopBody = new AstLoopBlock(this, null, "while"); 
			target.BeginBlock(loopBody);
			
			StatementBlock(/*Parser.Statement.atg:408*/loopBody);
			if (la.kind == _while) {
				Get();
			} else if (la.kind == _until) {
				Get();
				/*Parser.Statement.atg:409*/isPositive = false; 
			} else SynErr(119);
			/*Parser.Statement.atg:410*/loop = new AstWhileLoop(this, false, isPositive); 
			loop.Block = loopBody;
			
			Expect(_lpar);
			Expr(/*Parser.Statement.atg:413*/out loop.Condition);
			Expect(_rpar);
		} else SynErr(120);
		/*Parser.Statement.atg:414*/target.EndBlock(); block.Add(loop); 
	}

	void ForLoop(/*Parser.Statement.atg:417*/AstBlock block) {
		/*Parser.Statement.atg:417*/AstForLoop loop;
		
		Expect(_for);
		/*Parser.Statement.atg:420*/loop = new AstForLoop(this); target.BeginBlock(loop.Block); 
		Expect(_lpar);
		StatementBlock(/*Parser.Statement.atg:421*/loop.Initialize);
		if (la.kind == _do) {
			Get();
			StatementBlock(/*Parser.Statement.atg:423*/loop.NextIteration);
			/*Parser.Statement.atg:424*/loop.IsPrecondition = false; 
			if (la.kind == _while) {
				Get();
			} else if (la.kind == _until) {
				Get();
				/*Parser.Statement.atg:426*/loop.IsPositive = false; 
			} else SynErr(121);
			Expr(/*Parser.Statement.atg:428*/out loop.Condition);
		} else if (StartOf(14)) {
			if (la.kind == _while || la.kind == _until) {
				if (la.kind == _while) {
					Get();
				} else {
					Get();
					/*Parser.Statement.atg:430*/loop.IsPositive = false; 
				}
			}
			Expr(/*Parser.Statement.atg:432*/out loop.Condition);
			Expect(_semicolon);
			SimpleStatement(/*Parser.Statement.atg:434*/loop.NextIteration);
		} else SynErr(122);
		Expect(_rpar);
		StatementBlock(/*Parser.Statement.atg:437*/loop.Block);
		/*Parser.Statement.atg:437*/target.EndBlock(); block.Add(loop); 
	}

	void ForeachLoop(/*Parser.Statement.atg:441*/AstBlock block) {
		Expect(_foreach);
		/*Parser.Statement.atg:442*/AstForeachLoop loop = new AstForeachLoop(this);
		target.BeginBlock(loop.Block);
		
		Expect(_lpar);
		GetCall(/*Parser.Statement.atg:446*/out loop.Element);
		Expect(_in);
		Expr(/*Parser.Statement.atg:448*/out loop.List);
		Expect(_rpar);
		StatementBlock(/*Parser.Statement.atg:450*/loop.Block);
		/*Parser.Statement.atg:451*/target.EndBlock();
		block.Add(loop); 
		
	}

	void Arguments(/*Parser.Statement.atg:636*/ArgumentsProxy args) {
		/*Parser.Statement.atg:637*/IAstExpression expr;
		                      
		if (la.kind == _lpar) {
			Get();
			if (StartOf(14)) {
				Expr(/*Parser.Statement.atg:642*/out expr);
				/*Parser.Statement.atg:642*/args.Add(expr); 
				while (WeakSeparator(_comma,14,23) ) {
					Expr(/*Parser.Statement.atg:644*/out expr);
					/*Parser.Statement.atg:644*/args.Add(expr); 
				}
			}
			Expect(_rpar);
		}
		/*Parser.Statement.atg:649*/args.RemeberRightAppendPosition(); 
		if (la.kind == _appendleft) {
			Get();
			if (/*Parser.Statement.atg:654*/la.kind == _lpar && (!isLambdaExpression())) {
				Expect(_lpar);
				if (StartOf(14)) {
					Expr(/*Parser.Statement.atg:655*/out expr);
					/*Parser.Statement.atg:655*/args.Add(expr); 
					while (la.kind == _comma) {
						Get();
						Expr(/*Parser.Statement.atg:657*/out expr);
						/*Parser.Statement.atg:658*/args.Add(expr); 
					}
				}
				Expect(_rpar);
			} else if (StartOf(14)) {
				Expr(/*Parser.Statement.atg:662*/out expr);
				/*Parser.Statement.atg:662*/args.Add(expr); 
			} else SynErr(123);
		}
	}

	void FormalArg(/*Parser.GlobalScope.atg:566*/CompilerTarget ft) {
		/*Parser.GlobalScope.atg:566*/string id; SymbolInterpretations kind = SymbolInterpretations.LocalObjectVariable; 
		if (la.kind == _var || la.kind == _ref) {
			if (la.kind == _var) {
				Get();
			} else {
				Get();
				/*Parser.GlobalScope.atg:568*/kind = SymbolInterpretations.LocalReferenceVariable; 
			}
		}
		Id(/*Parser.GlobalScope.atg:570*/out id);
		/*Parser.GlobalScope.atg:573*/ft.Function.Parameters.Add(id); 
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
		} else SynErr(124);
		while (la.kind == _and) {
			Get();
			Statement(/*Parser.Statement.atg:38*/block);
		}
	}

	void ExplicitTypeExpr(/*Parser.Expression.atg:444*/out IAstType type) {
		/*Parser.Expression.atg:444*/type = null; 
		if (la.kind == _tilde) {
			Get();
			PrexoniteTypeExpr(/*Parser.Expression.atg:446*/out type);
		} else if (la.kind == _ns || la.kind == _doublecolon) {
			ClrTypeExpr(/*Parser.Expression.atg:447*/out type);
		} else SynErr(125);
	}

	void PrexoniteTypeExpr(/*Parser.Expression.atg:472*/out IAstType type) {
		/*Parser.Expression.atg:472*/string id = null; type = null; 
		if (StartOf(4)) {
			Id(/*Parser.Expression.atg:474*/out id);
		} else if (la.kind == _null) {
			Get();
			/*Parser.Expression.atg:474*/id = NullPType.Literal; 
		} else SynErr(126);
		/*Parser.Expression.atg:476*/AstDynamicTypeExpression dType = new AstDynamicTypeExpression(this, id); 
		if (la.kind == _lt) {
			Get();
			if (StartOf(27)) {
				TypeExprElement(/*Parser.Expression.atg:478*/dType.Arguments);
				while (la.kind == _comma) {
					Get();
					TypeExprElement(/*Parser.Expression.atg:479*/dType.Arguments);
				}
			}
			Expect(_gt);
		}
		/*Parser.Expression.atg:483*/type = dType; 
	}

	void ClrTypeExpr(/*Parser.Expression.atg:457*/out IAstType type) {
		/*Parser.Expression.atg:457*/string id; 
		/*Parser.Expression.atg:459*/StringBuilder typeId = new StringBuilder(); 
		if (la.kind == _doublecolon) {
			Get();
		} else if (la.kind == _ns) {
			Ns(/*Parser.Expression.atg:461*/out id);
			/*Parser.Expression.atg:461*/typeId.Append(id); typeId.Append('.'); 
		} else SynErr(127);
		while (la.kind == _ns) {
			Ns(/*Parser.Expression.atg:463*/out id);
			/*Parser.Expression.atg:463*/typeId.Append(id); typeId.Append('.'); 
		}
		Id(/*Parser.Expression.atg:465*/out id);
		/*Parser.Expression.atg:465*/typeId.Append(id);
		type = new AstConstantTypeExpression(this, 
		    "Object(\"" + StringPType.Escape(typeId.ToString()) + "\")");
		
	}

	void Ns(/*Parser.Helper.atg:35*/out string ns) {
		/*Parser.Helper.atg:35*/ns = "\\NoId\\"; 
		Expect(_ns);
		/*Parser.Helper.atg:37*/ns = cache(t.val); 
	}

	void TypeExprElement(/*Parser.Expression.atg:487*/List<IAstExpression> args ) {
		/*Parser.Expression.atg:487*/IAstExpression expr; IAstType type; 
		if (StartOf(16)) {
			Constant(/*Parser.Expression.atg:489*/out expr);
			/*Parser.Expression.atg:489*/args.Add(expr); 
		} else if (la.kind == _ns || la.kind == _tilde || la.kind == _doublecolon) {
			ExplicitTypeExpr(/*Parser.Expression.atg:490*/out type);
			/*Parser.Expression.atg:490*/args.Add(type); 
		} else if (la.kind == _lpar) {
			Get();
			Expr(/*Parser.Expression.atg:491*/out expr);
			Expect(_rpar);
			/*Parser.Expression.atg:491*/args.Add(expr); 
		} else SynErr(128);
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
				while (!(la.kind == _EOF || la.kind == _semicolon)) {SynErr(129); Get();}
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
		/*Parser.GlobalScope.atg:93*/string id = null; 
		List<string> aliases = new List<string>();
		PVariable vari; 
		SymbolInterpretations type = SymbolInterpretations.GlobalObjectVariable; 
		
		if (la.kind == _var) {
			Get();
		} else if (la.kind == _ref) {
			Get();
			/*Parser.GlobalScope.atg:101*/type = SymbolInterpretations.GlobalReferenceVariable; 
		} else SynErr(130);
		if (la.kind == _id || la.kind == _anyId) {
			GlobalId(/*Parser.GlobalScope.atg:104*/out id);
			/*Parser.GlobalScope.atg:104*/aliases.Add(id); 
			if (la.kind == _as) {
				GlobalVariableAliasList(/*Parser.GlobalScope.atg:105*/aliases);
			}
		} else if (la.kind == _as) {
			GlobalVariableAliasList(/*Parser.GlobalScope.atg:106*/aliases);
			/*Parser.GlobalScope.atg:107*/id = Engine.GenerateName("v"); 
		} else SynErr(131);
		/*Parser.GlobalScope.atg:110*/foreach(var alias in aliases)
		   Symbols[alias] = new SymbolEntry(type, id);
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
				MetaAssignment(/*Parser.GlobalScope.atg:121*/vari);
				while (!(la.kind == _EOF || la.kind == _semicolon)) {SynErr(132); Get();}
				Expect(_semicolon);
			}
			Expect(_rbrack);
		}
		if (la.kind == _assign) {
			/*Parser.GlobalScope.atg:125*/_pushLexerState(Lexer.Local); 
			Get();
			/*Parser.GlobalScope.atg:126*/CompilerTarget lastTarget = target;
			  target=FunctionTargets[Application.InitializationId];
			  IAstExpression expr;
			
			Expr(/*Parser.GlobalScope.atg:130*/out expr);
			/*Parser.GlobalScope.atg:131*/_popLexerState();
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
		/*Parser.GlobalScope.atg:157*/SymbolInterpretations type = SymbolInterpretations.Undefined; 
		while (!(la.kind == _EOF || la.kind == _declare)) {SynErr(133); Get();}
		Expect(_declare);
		if (StartOf(32)) {
			if (la.kind == _var) {
				Get();
				/*Parser.GlobalScope.atg:161*/type = SymbolInterpretations.GlobalObjectVariable; 
			} else if (la.kind == _ref) {
				Get();
				/*Parser.GlobalScope.atg:162*/type = SymbolInterpretations.GlobalReferenceVariable; 
			} else if (la.kind == _function) {
				Get();
				/*Parser.GlobalScope.atg:163*/type = SymbolInterpretations.Function; 
			} else {
				Get();
				/*Parser.GlobalScope.atg:164*/type = SymbolInterpretations.Command; 
			}
		}
		DeclarationInstance(/*Parser.GlobalScope.atg:166*/type);
		while (WeakSeparator(_comma,4,33) ) {
			DeclarationInstance(/*Parser.GlobalScope.atg:167*/type);
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
			} else SynErr(134);
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
			    entry = entry.AddToList(subEntry.List);
			}
			else
			{
			   entry = subEntry;
			}
			
		} else SynErr(135);
		/*Parser.GlobalScope.atg:66*/if(entry == null || key == null) 
		                        SemErr("Meta assignment did not generate an entry.");
		                   else 
		                        target.Meta[key] = entry; 
		                
	}

	void GlobalCode() {
		/*Parser.GlobalScope.atg:248*/PFunction func = TargetApplication._InitializationFunction;
		CompilerTarget ft = FunctionTargets[func];
		if(ft == null)
		    throw new PrexoniteException("Internal compilation error: InitializeFunction got lost.");
		
		/*Parser.GlobalScope.atg:255*/target = ft; 
		                             _pushLexerState(Lexer.Local);
		                         
		Expect(_lbrace);
		while (StartOf(20)) {
			Statement(/*Parser.GlobalScope.atg:259*/target.Ast);
		}
		Expect(_rbrace);
		/*Parser.GlobalScope.atg:262*/if(errors.count == 0)
		{
			TargetApplication._RequireInitialization();
			Loader._EmitPartialInitializationCode();
		}
		//Symbols defined in this block are not available to further global code blocks
		target.Symbols.Clear();
		target = null;
		_popLexerState();
		
	}

	void BuildBlock() {
		while (!(la.kind == _EOF || la.kind == _build)) {SynErr(136); Get();}
		Expect(_build);
		/*Parser.GlobalScope.atg:205*/PFunction func = new PFunction(TargetApplication);
		  CompilerTarget lastTarget = target; 
		  target = Loader.CreateFunctionTarget(func, new AstBlock(this));
		  Loader.DeclareBuildBlockCommands(target);
		  _pushLexerState(Lexer.Local);                                
		
		if (la.kind == _does) {
			Get();
		}
		StatementBlock(/*Parser.GlobalScope.atg:213*/target.Ast);
		/*Parser.GlobalScope.atg:216*/_popLexerState();
		  if(errors.count > 0)
		  {
		      SemErr("Cannot execute build block. Errors detected");
		      return;
		  }
		  
		  //Emit code for top-level build block
		  target.Ast.EmitCode(target, true);
		  target.Function.Meta["File"] = scanner.File;
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

	void FunctionDefinition(/*Parser.GlobalScope.atg:285*/out PFunction func) {
		/*Parser.GlobalScope.atg:286*/func = null; 
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
		
		if (la.kind == _lazy) {
			Get();
			if (la.kind == _function) {
				Get();
			}
			/*Parser.GlobalScope.atg:301*/isLazy = true; 
		} else if (la.kind == _function) {
			Get();
		} else if (la.kind == _coroutine) {
			Get();
			/*Parser.GlobalScope.atg:303*/isCoroutine = true; 
		} else if (la.kind == _macro) {
			Get();
			if (la.kind == _function) {
				Get();
			}
			/*Parser.GlobalScope.atg:304*/isMacro = true; 
		} else SynErr(137);
		if (StartOf(4)) {
			Id(/*Parser.GlobalScope.atg:306*/out id);
			/*Parser.GlobalScope.atg:306*/funcAliases.Add(id); 
			if (la.kind == _as) {
				FunctionAliasList(/*Parser.GlobalScope.atg:307*/funcAliases);
			}
		} else if (la.kind == _as) {
			FunctionAliasList(/*Parser.GlobalScope.atg:308*/funcAliases);
		} else SynErr(138);
		/*Parser.GlobalScope.atg:310*/funcId = id ?? Engine.GenerateName("f");
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
		                  SmartDeclareLocal(alias, localId, SymbolInterpretations.LocalReferenceVariable);
		          
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
			                                
		if (StartOf(35)) {
			if (la.kind == _lpar) {
				Get();
				if (StartOf(19)) {
					FormalArg(/*Parser.GlobalScope.atg:396*/ft);
					while (StartOf(36)) {
						if (la.kind == _comma) {
							Get();
						}
						FormalArg(/*Parser.GlobalScope.atg:398*/ft);
					}
				}
				Expect(_rpar);
			} else {
				FormalArg(/*Parser.GlobalScope.atg:402*/ft);
				while (StartOf(36)) {
					if (la.kind == _comma) {
						Get();
					}
					FormalArg(/*Parser.GlobalScope.atg:404*/ft);
				}
			}
		}
		/*Parser.GlobalScope.atg:407*/if(isNested && isLazy)
		   ft = cst;
		  
		  if(target == null && 
		      (!object.ReferenceEquals(func, TargetApplication._InitializationFunction)) &&
		      (!isNested))
		  {
		          //Add the name to the symboltable
		          foreach(var alias in funcAliases)	                                                
		              Symbols[alias] = new SymbolEntry(SymbolInterpretations.Function, func.Id);
		          
		          //Store the original (logical id, mentioned in the source code)
		          if((!string.IsNullOrEmpty(id)))
		              func.Meta[PFunction.LogicalIdKey] = id ?? funcId;
		  }
		  
		  //Target the derived (coroutine/lazy) body instead of the stub
		     if(isCoroutine || isLazy)
		         func = derBody;
		
		if (la.kind == _lbrack) {
			/*Parser.GlobalScope.atg:427*/_pushLexerState(Lexer.YYINITIAL); 
			Get();
			while (StartOf(31)) {
				MetaAssignment(/*Parser.GlobalScope.atg:429*/func);
				while (!(la.kind == _EOF || la.kind == _semicolon)) {SynErr(139); Get();}
				Expect(_semicolon);
			}
			/*Parser.GlobalScope.atg:431*/_popLexerState(); 
			Expect(_rbrack);
		}
		/*Parser.GlobalScope.atg:437*/if(isLazy && !isNested)
		{
		    foreach(var kvp in ft.LocalSymbols)
		    {
		        var paramId = kvp.Key;
		        var s = kvp.Value;
		        //Lazy functions cannot have ref parameters
		        if(s.Interpretation != SymbolInterpretations.LocalObjectVariable)
		            SemErr("Lazy functions can only have value parameters (ref is not allowed)");
		        ct.Function.Parameters.Add(s.Id);
		        ct.Symbols.Add(paramId, s);
		    }
		}
		
		    CompilerTarget lastTarget = target;
		    target = FunctionTargets[func]; 
		    _pushLexerState(Lexer.Local);
		    if(isMacro)
		        target.SetupAsMacro();
		
		if (StartOf(37)) {
			if (la.kind == _does) {
				Get();
			}
			StatementBlock(/*Parser.GlobalScope.atg:458*/target.Ast);
		} else if (/*Parser.GlobalScope.atg:460*/isFollowedByStatementBlock()) {
			Expect(_implementation);
			StatementBlock(/*Parser.GlobalScope.atg:461*/target.Ast);
		} else if (la.kind == _assign || la.kind == _implementation) {
			if (la.kind == _assign) {
				Get();
			} else {
				Get();
			}
			/*Parser.GlobalScope.atg:462*/AstReturn ret = new AstReturn(this, ReturnVariant.Exit); 
			Expr(/*Parser.GlobalScope.atg:463*/out ret.Expression);
			/*Parser.GlobalScope.atg:463*/target.Ast.Add(ret); 
			Expect(_semicolon);
		} else SynErr(140);
		/*Parser.GlobalScope.atg:465*/_popLexerState();
		target = lastTarget; 
		//Compile AST
		if(errors.count == 0)
		{
		    if(Engine.StringsAreEqual(func.Id, @"\init"))
		    {
		        TargetApplication._RequireInitialization();
		        Loader._EmitPartialInitializationCode();
		        //Initialize function gets finished at the end of Loader.Load
		    }
		    else
		    {
		        //Apply compiler hooks for all kinds of functions (lazy/coroutine/macro)
		FunctionTargets[func].ExecuteCompilerHooks();
		//Emit code for top-level block
		                                    Ast[func].EmitCode(FunctionTargets[func], true);
		                                    FunctionTargets[func].FinishTarget();
		                                }                                       
		                                
		if(isCoroutine)
		{
			//Stub has to be returned into the physical slot mentioned in the source code
			func = derStub;
			//Generate code for the stub
			AstCreateCoroutine crcor = new AstCreateCoroutine(this);                                            
			crcor.Expression = new AstCreateClosure(this,derBody.Id);
			AstReturn retst = new AstReturn(this, ReturnVariant.Exit);
			retst.Expression = crcor;
			cst.Ast.Add(retst);
			//Emit code for top-level block
			cst.Ast.EmitCode(cst,true);
			cst.FinishTarget();
		}
		else if(isLazy)
		{
		    derStub.Meta[PFunction.LazyKey] = true;
		    derStub.Meta["strict"] = true;
		
		    //Stub has to be returned into the physical slot mentioned in the source code
		    func = derStub;
		    
		    //Generate code for the stub
		    IAstExpression retVal;										    
		       
		       if(isNested)
		       {
		           //Nested lazy functions need a stub to capture their environment by value (handled by NestedFunction)
		           
		           //Generate stub code
		           retVal = new AstCreateClosure(this, ct.Function.Id);
		           
		           //Inject asthunk-conversion code into body
		           var inject = derStub.Parameters.Select(par => 
		           {
		               var getParam =
		                   new AstGetSetSymbol(this, PCall.Get, par, SymbolInterpretations.LocalObjectVariable);
		               var asThunkCall = 
		                new AstGetSetSymbol(this, PCall.Get, Engine.AsThunkAlias, SymbolInterpretations.Command);
		            asThunkCall.Arguments.Add(getParam);
		            var setParam =
		                new AstGetSetSymbol(this, PCall.Set, par, SymbolInterpretations.LocalObjectVariable);
		            setParam.Arguments.Add(asThunkCall);
		            return (AstNode) setParam;
		           });
		           ct.Ast.InsertRange(0,inject);
		       }
		       else
		       {										            
		           //Global lazy functions don't technically need a stub. Might be removed later on
		           var call = new AstGetSetSymbol(this, ct.Function.Id, SymbolInterpretations.Function);
		           
		           //Generate code for arguments (each wrapped in a `asThunk` command call)
		        foreach(var par in derStub.Parameters)
		        {
		            var getParam = 
		                new AstGetSetSymbol(this, PCall.Get, par, SymbolInterpretations.LocalObjectVariable);
		            var asThunkCall = 
		                new AstGetSetSymbol(this, PCall.Get, Engine.AsThunkAlias, SymbolInterpretations.Command);
		            asThunkCall.Arguments.Add(getParam);
		            call.Arguments.Add(asThunkCall);
		        }
		        
		        retVal = call;
		       }								    
		    
		    
		    //Assemble return statement
		    var ret = new AstReturn(this, ReturnVariant.Exit);
		    ret.Expression = retVal;
		    
		    cst.Ast.Add(ret);
		    
		    //Emit code for stub
		    cst.Ast.EmitCode(cst,true);
		    cst.FinishTarget();
		}                                        
		                             }
		                         
	}

	void GlobalId(/*Parser.GlobalScope.atg:578*/out string id) {
		/*Parser.GlobalScope.atg:578*/id = "...no freaking id..."; 
		if (la.kind == _id) {
			Get();
			/*Parser.GlobalScope.atg:580*/id = cache(t.val); 
		} else if (la.kind == _anyId) {
			Get();
			/*Parser.GlobalScope.atg:581*/id = cache(t.val.Substring(1)); 
		} else SynErr(141);
	}

	void MetaExpr(/*Parser.GlobalScope.atg:74*/out MetaEntry entry) {
		/*Parser.GlobalScope.atg:74*/bool sw; int i; double r; entry = null; string str; 
		switch (la.kind) {
		case _true: case _false: {
			Boolean(/*Parser.GlobalScope.atg:76*/out sw);
			/*Parser.GlobalScope.atg:76*/entry = sw; 
			break;
		}
		case _integer: {
			Integer(/*Parser.GlobalScope.atg:77*/out i);
			/*Parser.GlobalScope.atg:77*/entry = i.ToString(); 
			break;
		}
		case _real: {
			Real(/*Parser.GlobalScope.atg:78*/out r);
			/*Parser.GlobalScope.atg:78*/entry = r.ToString(); 
			break;
		}
		case _string: {
			String(/*Parser.GlobalScope.atg:79*/out str);
			/*Parser.GlobalScope.atg:79*/entry = str; 
			break;
		}
		case _id: case _anyId: case _ns: {
			GlobalQualifiedId(/*Parser.GlobalScope.atg:80*/out str);
			/*Parser.GlobalScope.atg:80*/entry = str; 
			break;
		}
		case _lbrace: {
			Get();
			/*Parser.GlobalScope.atg:81*/List<MetaEntry> lst = new List<MetaEntry>(); MetaEntry subEntry; 
			if (StartOf(34)) {
				MetaExpr(/*Parser.GlobalScope.atg:82*/out subEntry);
				/*Parser.GlobalScope.atg:82*/lst.Add(subEntry); 
				while (WeakSeparator(_comma,34,38) ) {
					MetaExpr(/*Parser.GlobalScope.atg:84*/out subEntry);
					/*Parser.GlobalScope.atg:84*/lst.Add(subEntry); 
				}
			}
			Expect(_rbrace);
			/*Parser.GlobalScope.atg:87*/entry = (MetaEntry) lst.ToArray(); 
			break;
		}
		default: SynErr(142); break;
		}
	}

	void GlobalQualifiedId(/*Parser.GlobalScope.atg:584*/out string id) {
		/*Parser.GlobalScope.atg:584*/id = "\\NoId\\"; 
		if (la.kind == _id || la.kind == _anyId) {
			GlobalId(/*Parser.GlobalScope.atg:586*/out id);
		} else if (la.kind == _ns) {
			Ns(/*Parser.GlobalScope.atg:587*/out id);
			/*Parser.GlobalScope.atg:587*/StringBuilder buffer = new StringBuilder(id); buffer.Append('.'); 
			while (la.kind == _ns) {
				Ns(/*Parser.GlobalScope.atg:588*/out id);
				/*Parser.GlobalScope.atg:588*/buffer.Append(id); buffer.Append('.'); 
			}
			GlobalId(/*Parser.GlobalScope.atg:590*/out id);
			/*Parser.GlobalScope.atg:590*/buffer.Append(id); 
			/*Parser.GlobalScope.atg:591*/id = buffer.ToString(); 
		} else SynErr(143);
	}

	void GlobalVariableAliasList(/*Parser.GlobalScope.atg:146*/IList<string> aliases ) {
		/*Parser.GlobalScope.atg:146*/string id = "\\NoId_In_GlobalVariableAliasList_\\"; 
		Expect(_as);
		GlobalId(/*Parser.GlobalScope.atg:148*/out id);
		/*Parser.GlobalScope.atg:148*/aliases.Add(id); 
		while (la.kind == _comma) {
			Get();
			GlobalId(/*Parser.GlobalScope.atg:150*/out id);
			/*Parser.GlobalScope.atg:150*/aliases.Add(id); 
		}
	}

	void DeclarationInstance(/*Parser.GlobalScope.atg:171*/SymbolInterpretations type) {
		/*Parser.GlobalScope.atg:171*/string id; string aId; 
		Id(/*Parser.GlobalScope.atg:173*/out id);
		/*Parser.GlobalScope.atg:173*/aId = id; 
		if (la.kind == _as) {
			Get();
			Id(/*Parser.GlobalScope.atg:174*/out aId);
		}
		/*Parser.GlobalScope.atg:175*/SymbolEntry inferredType;
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

	void FunctionAliasList(/*Parser.GlobalScope.atg:277*/IList<string> aliases ) {
		/*Parser.GlobalScope.atg:277*/String id; 
		Expect(_as);
		Id(/*Parser.GlobalScope.atg:279*/out id);
		/*Parser.GlobalScope.atg:279*/aliases.Add(id); 
		if (la.kind == _comma) {
			Get();
			Id(/*Parser.GlobalScope.atg:280*/out id);
			/*Parser.GlobalScope.atg:280*/aliases.Add(id); 
		}
	}

	void ExplicitLabel(/*Parser.Statement.atg:332*/AstBlock block) {
		/*Parser.Statement.atg:332*/string id = "--\\NotAnId\\--"; 
		if (StartOf(4)) {
			Id(/*Parser.Statement.atg:334*/out id);
			Expect(_colon);
		} else if (la.kind == _lid) {
			Get();
			/*Parser.Statement.atg:335*/id = cache(t.val.Substring(0,t.val.Length-1)); 
		} else SynErr(144);
		/*Parser.Statement.atg:336*/block.Statements.Add(new AstExplicitLabel(this, id)); 
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
		} else if (StartOf(39)) {
			Return(/*Parser.Statement.atg:49*/block);
		} else if (la.kind == _throw) {
			Throw(/*Parser.Statement.atg:50*/block);
		} else if (la.kind == _let) {
			LetBindingStmt(/*Parser.Statement.atg:51*/block);
		} else SynErr(145);
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
		default: SynErr(146); break;
		}
	}

	void ExplicitGoTo(/*Parser.Statement.atg:339*/AstBlock block) {
		/*Parser.Statement.atg:339*/string id; 
		Expect(_goto);
		Id(/*Parser.Statement.atg:342*/out id);
		/*Parser.Statement.atg:342*/block.Statements.Add(new AstExplicitGoTo(this, id)); 
	}

	void VariableDeclarationStatement() {
		/*Parser.Statement.atg:274*/AstGetSetSymbol variable; 
		VariableDeclaration(/*Parser.Statement.atg:275*/out variable);
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
			/*Parser.Statement.atg:88*/block.Add(new AstUnaryOperator(this, UnaryOperator.PostIncrement, complex)); 
		} else if (la.kind == _dec) {
			Get();
			/*Parser.Statement.atg:89*/block.Add(new AstUnaryOperator(this, UnaryOperator.PostDecrement, complex)); 
		} else if (StartOf(40)) {
			Assignment(/*Parser.Statement.atg:90*/complex, out node);
			/*Parser.Statement.atg:90*/symbol = node as AstGetSetSymbol;
			if(symbol != null && InterpretationIsVariable(symbol.Interpretation) && isDeclaration)
			    symbol.Interpretation = InterpretAsObjectVariable(symbol.Interpretation);
			block.Add(node);
			
		} else if (la.kind == _appendright) {
			AppendRightTermination(/*Parser.Statement.atg:95*/ref complex);
			while (la.kind == _appendright) {
				AppendRightTermination(/*Parser.Statement.atg:96*/ref complex);
			}
			/*Parser.Statement.atg:98*/block.Add(complex);  
		} else SynErr(147);
	}

	void Return(/*Parser.Statement.atg:477*/AstBlock block) {
		/*Parser.Statement.atg:477*/AstReturn ret = null; 
		AstExplicitGoTo jump = null; 
		IAstExpression expr = null; 
		AstLoopBlock bl = target.CurrentLoopBlock;
		
		if (la.kind == _return || la.kind == _yield) {
			if (la.kind == _return) {
				Get();
				/*Parser.Statement.atg:485*/ret = new AstReturn(this, ReturnVariant.Exit); 
			} else {
				Get();
				/*Parser.Statement.atg:486*/ret = new AstReturn(this, ReturnVariant.Continue); 
			}
			if (StartOf(41)) {
				if (StartOf(14)) {
					Expr(/*Parser.Statement.atg:488*/out expr);
					/*Parser.Statement.atg:488*/ret.Expression = expr; 
				} else {
					Get();
					/*Parser.Statement.atg:489*/ret.ReturnVariant = ReturnVariant.Set; 
					Expr(/*Parser.Statement.atg:490*/out expr);
					/*Parser.Statement.atg:490*/ret.Expression = expr; 
					/*Parser.Statement.atg:491*/SemErr("Return value assignment is no longer supported. You must use local variables instead."); 
				}
			}
		} else if (la.kind == _break) {
			Get();
			/*Parser.Statement.atg:493*/if(bl == null)
			   ret = new AstReturn(this, ReturnVariant.Break); 
			else
			    jump = new AstExplicitGoTo(this, bl.BreakLabel);
			
		} else if (la.kind == _continue) {
			Get();
			/*Parser.Statement.atg:498*/if(bl == null)
			   ret = new AstReturn(this, ReturnVariant.Continue); 
			else
			    jump = new AstExplicitGoTo(this, bl.ContinueLabel);
			
		} else SynErr(148);
		/*Parser.Statement.atg:503*/block.Add((AstNode)ret ?? (AstNode)jump); 
	}

	void Throw(/*Parser.Statement.atg:618*/AstBlock block) {
		/*Parser.Statement.atg:618*/AstThrow th; 
		ThrowExpression(/*Parser.Statement.atg:620*/out th);
		/*Parser.Statement.atg:621*/block.Add(th); 
	}

	void LetBindingStmt(/*Parser.Statement.atg:541*/AstBlock block) {
		Expect(_let);
		LetBinder(/*Parser.Statement.atg:542*/block);
		while (la.kind == _comma) {
			Get();
			LetBinder(/*Parser.Statement.atg:542*/block);
		}
	}

	void Condition(/*Parser.Statement.atg:374*/AstBlock block) {
		/*Parser.Statement.atg:374*/IAstExpression expr = null; bool isNegative = false; 
		if (la.kind == _if) {
			Get();
			/*Parser.Statement.atg:376*/isNegative = false; 
		} else if (la.kind == _unless) {
			Get();
			/*Parser.Statement.atg:377*/isNegative = true; 
		} else SynErr(149);
		Expect(_lpar);
		Expr(/*Parser.Statement.atg:380*/out expr);
		Expect(_rpar);
		/*Parser.Statement.atg:380*/AstCondition cond = new AstCondition(this, expr, isNegative);
		target.BeginBlock(cond.IfBlock);
		
		StatementBlock(/*Parser.Statement.atg:384*/cond.IfBlock);
		/*Parser.Statement.atg:385*/target.EndBlock(); 
		if (la.kind == _else) {
			Get();
			/*Parser.Statement.atg:388*/target.BeginBlock(cond.ElseBlock); 
			StatementBlock(/*Parser.Statement.atg:389*/cond.ElseBlock);
			/*Parser.Statement.atg:390*/target.EndBlock(); 
		}
		/*Parser.Statement.atg:391*/block.Add(cond); 
	}

	void NestedFunction(/*Parser.Statement.atg:507*/AstBlock block) {
		/*Parser.Statement.atg:507*/PFunction func; 
		FunctionDefinition(/*Parser.Statement.atg:509*/out func);
		/*Parser.Statement.atg:511*/string logicalId = func.Meta[PFunction.LogicalIdKey];
		func.Meta[PFunction.ParentFunctionKey] = target.Function.Id;
		string physicalId = func.Id;
		
		CompilerTarget ft = FunctionTargets[func];
		AstGetSetSymbol setVar = new AstGetSetSymbol(this, PCall.Set, logicalId, SymbolInterpretations.LocalObjectVariable);
		if(func.Meta[PFunction.LazyKey].Switch)
		{
		    //Capture environment by value                                        
		    var ps = ft.ToCaptureByValue(let_bindings(ft));
		    ft._DetermineSharedNames(); //Need to re-determine shared names since
		                                // ToCaptureByValue does not automatically modify shared names
		    var clos = new AstCreateClosure(this, func.Id);
		    var callStub = new AstIndirectCall(this, clos);
		    callStub.Arguments.AddRange(ps(this));
		    setVar.Arguments.Add(callStub);
		}
		else if(ft.OuterVariables.Count > 0)
		{                                        
		    setVar.Arguments.Add( new AstCreateClosure(this, physicalId) );                                        
		}
		else
		{
		    setVar.Arguments.Add( new AstGetSetReference(this, physicalId, SymbolInterpretations.Function) );
		}
		block.Add(setVar);
		
	}

	void TryCatchFinally(/*Parser.Statement.atg:568*/AstBlock block) {
		/*Parser.Statement.atg:568*/AstTryCatchFinally a = new AstTryCatchFinally(this); 
		Expect(_try);
		/*Parser.Statement.atg:570*/target.BeginBlock(a.TryBlock); 
		Expect(_lbrace);
		while (StartOf(20)) {
			Statement(/*Parser.Statement.atg:572*/a.TryBlock);
		}
		Expect(_rbrace);
		/*Parser.Statement.atg:574*/target.EndBlock(); 
		if (la.kind == _catch || la.kind == _finally) {
			if (la.kind == _catch) {
				Get();
				/*Parser.Statement.atg:575*/target.BeginBlock(a.CatchBlock); 
				if (la.kind == _lpar) {
					Get();
					GetCall(/*Parser.Statement.atg:577*/out a.ExceptionVar);
					Expect(_rpar);
				} else if (la.kind == _lbrace) {
					/*Parser.Statement.atg:579*/SemErr(la,"catch-clauses that don't store the exception are illegal."); 
				} else SynErr(150);
				Expect(_lbrace);
				while (StartOf(20)) {
					Statement(/*Parser.Statement.atg:582*/a.CatchBlock);
				}
				Expect(_rbrace);
				/*Parser.Statement.atg:584*/target.EndBlock(); 
				if (la.kind == _finally) {
					Get();
					/*Parser.Statement.atg:587*/target.BeginBlock(a.FinallyBlock); 
					Expect(_lbrace);
					while (StartOf(20)) {
						Statement(/*Parser.Statement.atg:589*/a.FinallyBlock);
					}
					Expect(_rbrace);
					/*Parser.Statement.atg:591*/target.EndBlock(); 
				}
			} else {
				Get();
				/*Parser.Statement.atg:594*/target.BeginBlock(a.FinallyBlock); 
				Expect(_lbrace);
				while (StartOf(20)) {
					Statement(/*Parser.Statement.atg:596*/a.FinallyBlock);
				}
				Expect(_rbrace);
				/*Parser.Statement.atg:598*/target.EndBlock(); 
				if (la.kind == _catch) {
					/*Parser.Statement.atg:600*/target.BeginBlock(a.CatchBlock); 
					Get();
					if (la.kind == _lpar) {
						Get();
						GetCall(/*Parser.Statement.atg:603*/out a.ExceptionVar);
						Expect(_rpar);
					} else if (la.kind == _lbrace) {
						/*Parser.Statement.atg:605*/SemErr(la,"catch-clauses that don't store the exception are illegal."); 
					} else SynErr(151);
					Expect(_lbrace);
					while (StartOf(20)) {
						Statement(/*Parser.Statement.atg:608*/a.CatchBlock);
					}
					Expect(_rbrace);
					/*Parser.Statement.atg:611*/target.EndBlock(); 
				}
			}
		}
		/*Parser.Statement.atg:614*/block.Add(a); 
	}

	void Using(/*Parser.Statement.atg:625*/AstBlock block) {
		/*Parser.Statement.atg:625*/AstUsing use = new AstUsing(this); 
		Expect(_uusing);
		Expect(_lpar);
		Expr(/*Parser.Statement.atg:627*/out use.Expression);
		Expect(_rpar);
		/*Parser.Statement.atg:628*/target.BeginBlock(use.Block); 
		StatementBlock(/*Parser.Statement.atg:629*/use.Block);
		/*Parser.Statement.atg:630*/target.EndBlock();
		block.Add(use); 
		
	}

	void Assignment(/*Parser.Statement.atg:346*/AstGetSet lvalue, out AstNode node) {
		/*Parser.Statement.atg:346*/IAstExpression expr = null;
		BinaryOperator setModifier = BinaryOperator.None;
		IAstType T;
		node = lvalue;
		
		if (StartOf(9)) {
			switch (la.kind) {
			case _assign: {
				Get();
				/*Parser.Statement.atg:353*/setModifier = BinaryOperator.None; 
				break;
			}
			case _plus: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:354*/setModifier = BinaryOperator.Addition; 
				break;
			}
			case _minus: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:355*/setModifier = BinaryOperator.Subtraction; 
				break;
			}
			case _times: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:356*/setModifier = BinaryOperator.Multiply; 
				break;
			}
			case _div: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:357*/setModifier = BinaryOperator.Division; 
				break;
			}
			case _bitAnd: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:358*/setModifier = BinaryOperator.BitwiseAnd; 
				break;
			}
			case _bitOr: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:359*/setModifier = BinaryOperator.BitwiseOr; 
				break;
			}
			case _coalescence: {
				Get();
				Expect(_assign);
				/*Parser.Statement.atg:360*/setModifier = BinaryOperator.Coalescence; 
				break;
			}
			}
			Expr(/*Parser.Statement.atg:361*/out expr);
		} else if (la.kind == _tilde) {
			Get();
			Expect(_assign);
			/*Parser.Statement.atg:363*/setModifier = BinaryOperator.Cast; 
			TypeExpr(/*Parser.Statement.atg:364*/out T);
			/*Parser.Statement.atg:364*/expr = T; 
		} else SynErr(152);
		/*Parser.Statement.atg:366*/lvalue.Arguments.Add(expr);
		lvalue.Call = PCall.Set; 
		if(setModifier != BinaryOperator.None)
		    node = new AstModifyingAssignment(this,setModifier,lvalue);
		
	}

	void AppendRightTermination(/*Parser.Statement.atg:103*/ref AstGetSet complex) {
		/*Parser.Statement.atg:103*/AstGetSet actualComplex; 
		Expect(_appendright);
		GetCall(/*Parser.Statement.atg:106*/out actualComplex);
		/*Parser.Statement.atg:106*/actualComplex.Arguments.RightAppend(complex);
		actualComplex.Arguments.ReleaseRightAppend();
		if(actualComplex is AstGetSetSymbol && ((AstGetSetSymbol)actualComplex).IsVariable)
		       actualComplex.Call = PCall.Set;
		   complex = actualComplex;
		
	}

	void Function(/*Parser.Statement.atg:281*/out AstGetSet function) {
		/*Parser.Statement.atg:281*/function = null; string id; 
		Id(/*Parser.Statement.atg:283*/out id);
		/*Parser.Statement.atg:283*/if(!target.Symbols.ContainsKey(id))
		{
		    function = new AstUnresolved(this, id);
		}
		else
		{
		    if(isOuterVariable(id))
		        target.RequireOuterVariable(id);
		    SymbolEntry sym = target.Symbols[id];
		    if(isKnownMacro(sym)) 
		    {
		        function = new AstMacroInvocation(this, sym.Id);
		    } 
		    else
		    {
		        function = new AstGetSetSymbol(this, sym.Id, sym.Interpretation);
		    }
		}
		
		Arguments(/*Parser.Statement.atg:302*/function.Arguments);
	}

	void Variable(/*Parser.Statement.atg:245*/out AstGetSet complex, out bool isDeclared) {
		/*Parser.Statement.atg:245*/string id; isDeclared = false; complex = null; 
		if (la.kind == _var || la.kind == _ref || la.kind == _static) {
			/*Parser.Statement.atg:246*/AstGetSetSymbol variable; 
			VariableDeclaration(/*Parser.Statement.atg:247*/out variable);
			/*Parser.Statement.atg:247*/isDeclared = true; complex = variable; 
		} else if (StartOf(4)) {
			Id(/*Parser.Statement.atg:248*/out id);
			/*Parser.Statement.atg:249*/if(target.Symbols.ContainsKey(id))
			{
			    SymbolEntry varSym = target.Symbols[id];
			    if(InterpretationIsVariable(varSym.Interpretation))
			    {
			        if(isOuterVariable(id))
			            target.RequireOuterVariable(id);                                                    
			    }
			    else
			    {
			        SemErr(t.line, t.col, "Variable name expected but was " + 
			            Enum.GetName(typeof(SymbolInterpretations),varSym.Interpretation));
			    }
			    complex = new AstGetSetSymbol(this, varSym.Id, varSym.Interpretation);;
			}
			else
			{
			    //Unknown symbols are treated as functions. See production Function for details.
			    SemErr(t.line, t.col, "Internal compiler error. Did not catch unknown identifier.");
			    complex = new AstGetSetSymbol(this, "Not a Variable Id", SymbolInterpretations.LocalObjectVariable);
			}
			
		} else SynErr(153);
	}

	void StaticCall(/*Parser.Statement.atg:306*/out AstGetSetStatic staticCall) {
		/*Parser.Statement.atg:306*/IAstType typeExpr;
		string memberId;
		staticCall = null;
		
		ExplicitTypeExpr(/*Parser.Statement.atg:311*/out typeExpr);
		Expect(_dot);
		Id(/*Parser.Statement.atg:312*/out memberId);
		/*Parser.Statement.atg:312*/staticCall = new AstGetSetStatic(this, PCall.Get, typeExpr, memberId); 
		Arguments(/*Parser.Statement.atg:313*/staticCall.Arguments);
	}

	void VariableDeclaration(/*Parser.Statement.atg:219*/out AstGetSetSymbol variable) {
		/*Parser.Statement.atg:219*/variable = null; string staticId = null; 
		/*Parser.Statement.atg:220*/string id = null; SymbolInterpretations kind = SymbolInterpretations.Undefined; 
		if (la.kind == _var || la.kind == _ref) {
			if (la.kind == _var) {
				Get();
				/*Parser.Statement.atg:221*/kind = SymbolInterpretations.LocalObjectVariable; 
			} else {
				Get();
				/*Parser.Statement.atg:222*/kind = SymbolInterpretations.LocalReferenceVariable; 
			}
			Id(/*Parser.Statement.atg:225*/out id);
			/*Parser.Statement.atg:226*/SmartDeclareLocal(id, kind);
			staticId = id; 
			
		} else if (la.kind == _static) {
			Get();
			/*Parser.Statement.atg:229*/kind = SymbolInterpretations.GlobalObjectVariable; 
			if (la.kind == _var || la.kind == _ref) {
				if (la.kind == _var) {
					Get();
				} else {
					Get();
					/*Parser.Statement.atg:231*/kind = SymbolInterpretations.GlobalReferenceVariable; 
				}
			}
			Id(/*Parser.Statement.atg:233*/out id);
			/*Parser.Statement.atg:233*/staticId = target.Function.Id + "\\static\\" + id;
			target.Declare(kind, id, staticId);
			if(!target.Loader.Options.TargetApplication.Variables.ContainsKey(staticId))
			    target.Loader.Options.TargetApplication.Variables.Add(staticId, new PVariable(staticId));
			
		} else SynErr(154);
		/*Parser.Statement.atg:238*/variable = InterpretationIsObjectVariable(kind) ?
		new AstGetSetSymbol(this, PCall.Get, staticId, kind)
		:
			new AstGetSetReference(this, PCall.Get, staticId, InterpretAsObjectVariable(kind)); 
	}

	void LetBinder(/*Parser.Statement.atg:546*/AstBlock block) {
		/*Parser.Statement.atg:546*/string id = null;
		IAstExpression thunk;
		
		Id(/*Parser.Statement.atg:550*/out id);
		/*Parser.Statement.atg:551*/SmartDeclareLocal(id, SymbolInterpretations.LocalObjectVariable);
		mark_as_let(target.Function, id);
		if(la.kind == _assign)
		    _inject(_lazy,"lazy"); 
		
		if (la.kind == _assign) {
			Get();
			LazyExpression(/*Parser.Statement.atg:557*/out thunk);
			/*Parser.Statement.atg:560*/var assign = new AstGetSetSymbol(this, PCall.Set, id, SymbolInterpretations.LocalObjectVariable);
			assign.Arguments.Add(thunk);
			block.Add(assign);
			
		}
	}


#line 121 "D:\DotNetProjects\Prexonite\Tools\Parser.frame" //FRAME


	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();

#line default //END FRAME -->parseRoot

		Prexonite();

#line 127 "D:\DotNetProjects\Prexonite\Tools\Parser.frame" //FRAME

    Expect(0);
	}
	
	bool[,] set = {

#line default //END FRAME -->initialization

		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,T, T,x,x,x, T,T,T,T, T,T,x,T, x,x,T,T, T,T,T,T, T,x,T,T, T,x,T,x, T,T,T,T, T,x,x,x, x,T,T,T, T,T,x,x, T,x,x,x, x,x,x,x, x},
		{x,T,T,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,T, T,T,x,T, x,x,T,T, T,T,T,T, T,x,T,T, T,x,T,x, T,T,T,T, T,x,x,x, x,T,T,T, T,T,x,x, T,x,x,x, x,x,x,x, x},
		{x,T,T,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,T,x, x,T,x,x, x,x,x,x, T,x,x,T, T,x,x,x, T,T,T,T, T,T,x,T, x,x,T,T, T,T,T,T, T,x,T,T, T,x,T,x, T,T,T,T, T,x,x,x, x,T,T,T, T,T,x,x, T,x,x,x, x,x,x,x, x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,T, x,x,x,T, x,x,x,T, T,T,x,T, T,x,T,T, T,x,T,x, T,T,T,T, T,x,x,x, x,T,T,T, T,T,x,x, T,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, T,T,T,T, x,x,x,T, x,x,x,x, x,T,T,T, T,x,x,T, x,x,T,x, x,x,x,T, x,x,x,T, x,x,T,x, x,x,x,T, T,T,T,x, x,x,x,T, T,T,x,x, T,x,T,x, x,x,T,x, x,x,x,x, x,x,x,T, T,x,x,x, T,T,x,T, x,T,T,T, T,x,x,x, T,x,x,T, x,x,T,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, T,T,x,x, T,x,x,x, x,x,x,x, x,x,x,T, x,T,T,x, x,x,x,x, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, T,T,T,T, T,T,T,T, T,T,x,T, x,T,T,T, T,T,T,T, T,T,T,T, T,T,T,x, T,T,x,x, x,x,T,x, x,x,x,x, T,T,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,T,T, T,x,x,x, x,x,x,T, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,T, x,T,x,x, x,T,x,T, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,x,T,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,T,x,x, x,x,x,T, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, T,T,T,T, x,x,x,T, x,x,x,x, x,T,T,T, T,x,x,T, x,x,T,x, x,x,x,T, x,x,x,T, x,x,T,x, x,x,x,T, T,T,T,x, x,x,x,T, T,T,x,x, T,x,T,x, x,x,T,x, x,x,x,x, x,x,x,T, T,T,T,x, T,T,x,T, x,T,T,T, T,x,x,x, T,x,x,T, x,x,T,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,T, x,x,x,T, x,x,T,x, x,x,x,T, T,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,x,T,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, T,x,x,x, x,x,x,x, x,x,x,T, x,T,x,T, x,x,T,x, x,x,x,T, T,x,x,x, x,x,x,x, T,T,T,x, T,T,T,T, x,x,T,T, T,T,x,x, x,x,T,T, x,T,T,x, x,T,x,T, x,T,T,T, T,T,x,x, T,T,T,T, T,x,x,x, x},
		{x,T,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,T, x,x,x,T, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,x,T,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,x,T,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,T, x,T,x,T, x,x,T,x, x,x,x,T, T,x,x,x, x,x,x,x, T,T,x,x, x,T,T,T, x,x,T,T, T,T,x,x, x,x,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, T,x,x,x, x},
		{x,T,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,T, x,x,x,T, x,x,T,x, x,x,x,T, T,x,x,x, x,x,x,x, T,T,x,x, x,T,T,T, x,x,T,T, T,T,x,x, x,x,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, T,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,T,x,T, x,T,T,T, T,T,x,x, x,T,T,T, x,x,x,x, x},
		{x,x,x,x, T,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,T, x,x,x,T, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,T,x,x, x,x,T,x, x,T,T,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,T,x,x, x,x,x,x, x,T,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,T,x,x, x,x,x,x, x,T,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, x,x,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, T,T,T,T, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,x,x, x,x,x,x, T,T,x,x, x,x,T,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, T,x,x,x, x,x,x,x, x,x,x,T, x,T,x,T, x,x,T,x, x,x,x,T, T,x,x,x, x,x,x,x, T,T,T,x, T,T,T,T, x,x,T,T, T,T,x,x, x,x,T,T, x,T,T,x, x,T,x,T, T,T,T,T, T,T,x,x, T,T,T,T, T,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,T, T,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, T,T,x,x, T,x,x,x, x,x,x,x, x,x,x,T, x,T,T,x, x,x,x,T, T,x,x,x, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,x, T,T,T,T, x,T,x,T, x,x,x,x, x,T,T,T, T,x,x,T, x,x,T,x, x,x,x,T, x,x,x,T, x,x,T,x, x,x,x,T, T,T,T,x, x,x,x,T, T,T,x,x, T,x,T,x, x,x,T,x, x,x,x,x, x,x,x,T, T,T,T,x, T,T,x,T, x,T,T,T, T,x,x,x, T,x,x,T, x,x,T,x, x}

#line 132 "D:\DotNetProjects\Prexonite\Tools\Parser.frame" //FRAME

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
			case 90: s = "macro expected"; break;
			case 91: s = "lazy expected"; break;
			case 92: s = "let expected"; break;
			case 93: s = "ENDKEYWORDS expected"; break;
			case 94: s = "LPopExpr expected"; break;
			case 95: s = "??? expected"; break;
			case 96: s = "invalid AsmStatementBlock"; break;
			case 97: s = "invalid AsmInstruction"; break;
			case 98: s = "invalid AsmInstruction"; break;
			case 99: s = "invalid AsmInstruction"; break;
			case 100: s = "invalid AsmInstruction"; break;
			case 101: s = "invalid AsmInstruction"; break;
			case 102: s = "invalid AsmId"; break;
			case 103: s = "invalid SignedReal"; break;
			case 104: s = "invalid Boolean"; break;
			case 105: s = "invalid Id"; break;
			case 106: s = "invalid AtomicExpr"; break;
			case 107: s = "invalid AssignExpr"; break;
			case 108: s = "invalid AssignExpr"; break;
			case 109: s = "invalid TypeExpr"; break;
			case 110: s = "invalid GetSetExtension"; break;
			case 111: s = "invalid Primary"; break;
			case 112: s = "invalid Constant"; break;
			case 113: s = "invalid LoopExpr"; break;
			case 114: s = "invalid LambdaExpression"; break;
			case 115: s = "invalid LambdaExpression"; break;
			case 116: s = "invalid LazyExpression"; break;
			case 117: s = "invalid GetInitiator"; break;
			case 118: s = "invalid GetInitiator"; break;
			case 119: s = "invalid WhileLoop"; break;
			case 120: s = "invalid WhileLoop"; break;
			case 121: s = "invalid ForLoop"; break;
			case 122: s = "invalid ForLoop"; break;
			case 123: s = "invalid Arguments"; break;
			case 124: s = "invalid Statement"; break;
			case 125: s = "invalid ExplicitTypeExpr"; break;
			case 126: s = "invalid PrexoniteTypeExpr"; break;
			case 127: s = "invalid ClrTypeExpr"; break;
			case 128: s = "invalid TypeExprElement"; break;
			case 129: s = "this symbol not expected in Prexonite"; break;
			case 130: s = "invalid GlobalVariableDefinition"; break;
			case 131: s = "invalid GlobalVariableDefinition"; break;
			case 132: s = "this symbol not expected in GlobalVariableDefinition"; break;
			case 133: s = "this symbol not expected in Declaration"; break;
			case 134: s = "invalid MetaAssignment"; break;
			case 135: s = "invalid MetaAssignment"; break;
			case 136: s = "this symbol not expected in BuildBlock"; break;
			case 137: s = "invalid FunctionDefinition"; break;
			case 138: s = "invalid FunctionDefinition"; break;
			case 139: s = "this symbol not expected in FunctionDefinition"; break;
			case 140: s = "invalid FunctionDefinition"; break;
			case 141: s = "invalid GlobalId"; break;
			case 142: s = "invalid MetaExpr"; break;
			case 143: s = "invalid GlobalQualifiedId"; break;
			case 144: s = "invalid ExplicitLabel"; break;
			case 145: s = "invalid SimpleStatement"; break;
			case 146: s = "invalid StructureStatement"; break;
			case 147: s = "invalid GetSetComplex"; break;
			case 148: s = "invalid Return"; break;
			case 149: s = "invalid Condition"; break;
			case 150: s = "invalid TryCatchFinally"; break;
			case 151: s = "invalid TryCatchFinally"; break;
			case 152: s = "invalid Assignment"; break;
			case 153: s = "invalid Variable"; break;
			case 154: s = "invalid VariableDeclaration"; break;

#line 146 "D:\DotNetProjects\Prexonite\Tools\Parser.frame" //FRAME

			default: s = "error " + n; break;
		}
		if(s.EndsWith(" expected"))
            s = "after \"" + parentParser.t.ToString(false) + "\", " + s.Replace("expected","is expected") + " and not \"" + parentParser.la.ToString(false) + "\"";
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