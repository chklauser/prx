//SOURCE ARRAY
/*PTypeExpression.atg:24*/using Prexonite;
using Prexonite.Types;
using FatalError = Prexonite.Compiler.FatalCompilerException;//END SOURCE ARRAY


#line 27 "D:\DotNetProjects\Prexonite\Tools\Parser.frame" //FRAME

using System;


#line default //END FRAME -->namespace

namespace Prexonite.Internal {


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
	public const int _tilde = 2;
	public const int _true = 3;
	public const int _false = 4;
	public const int _integer = 5;
	public const int _real = 6;
	public const int _string = 7;
	public enum Terminals
	{
		@EOF = 0,
		@id = 1,
		@tilde = 2,
		@true = 3,
		@false = 4,
		@integer = 5,
		@real = 6,
		@string = 7,
	}
	const int maxT = 11;

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

	void PTypeExpression() {
		Type(/*PTypeExpression.atg:101*/out _lastType);
	}

	void Type(/*PTypeExpression.atg:142*/out PType value) {
		/*PTypeExpression.atg:142*/string id; 
		System.Collections.Generic.List<PValue> args = new System.Collections.Generic.List<PValue>();
		
		if (la.kind == _tilde) {
			Get();
		}
		Expect(_id);
		/*PTypeExpression.atg:147*/id = t.val; 
		if(!_sctx.ParentEngine.PTypeRegistry.Contains(id))
		{
		    SemErr("Cannot find PType " + id + " referenced in PTypeExpression.");
		}                                    
		
		if (la.kind == 8) {
			Get();
			/*PTypeExpression.atg:153*/PValue obj; 
			if (StartOf(1)) {
				Expr(/*PTypeExpression.atg:154*/out obj);
				/*PTypeExpression.atg:154*/args.Add(obj); 
				while (WeakSeparator(9,1,2) ) {
					Expr(/*PTypeExpression.atg:156*/out obj);
					/*PTypeExpression.atg:156*/args.Add(obj); 
				}
			}
			Expect(10);
		}
		/*PTypeExpression.atg:161*/if(id == null) //Error recovery.
		{
		    SemErr("Error in type expression.");
		    value = PType.Object[typeof(object)];
		}
		else //Normal case
		    value = _sctx.ConstructPType(id, args.ToArray());
		
	}

	void Expr(/*PTypeExpression.atg:104*/out PValue value) {
		/*PTypeExpression.atg:104*/PType vt; value = null; 
		if (la.kind == _true || la.kind == _false) {
			Boolean(/*PTypeExpression.atg:106*/out value);
		} else if (la.kind == _integer) {
			Integer(/*PTypeExpression.atg:107*/out value);
		} else if (la.kind == _real) {
			Real(/*PTypeExpression.atg:108*/out value);
		} else if (la.kind == _string) {
			String(/*PTypeExpression.atg:109*/out value);
		} else if (la.kind == _id || la.kind == _tilde) {
			Type(/*PTypeExpression.atg:110*/out vt);
			/*PTypeExpression.atg:110*/value = PType.Object.CreatePValue(vt); 
		} else SynErr(12);
	}

	void Boolean(/*PTypeExpression.atg:113*/out PValue value) {
		/*PTypeExpression.atg:113*/value = true;  
		if (la.kind == _true) {
			Get();
		} else if (la.kind == _false) {
			Get();
			/*PTypeExpression.atg:116*/value = false; 
		} else SynErr(13);
	}

	void Integer(/*PTypeExpression.atg:119*/out PValue value) {
		/*PTypeExpression.atg:119*/int i; 
		Expect(_integer);
		/*PTypeExpression.atg:122*/if(!Prexonite.Compiler.Parser.TryParseInteger(t.val, out i))
		   SemErr("Cannot recognize integer " + t.val);
		value = i;
		
	}

	void Real(/*PTypeExpression.atg:128*/out PValue value) {
		/*PTypeExpression.atg:128*/double d; 
		Expect(_real);
		/*PTypeExpression.atg:130*/if(!Prexonite.Compiler.Parser.TryParseReal(t.val, out d))
		   SemErr("Cannot recognize real " + t.val);
		value = d;
		
	}

	void String(/*PTypeExpression.atg:137*/out PValue value) {
		Expect(_string);
		/*PTypeExpression.atg:138*/value = StringPType.Unescape(t.val.Substring(1,t.val.Length-2));
		
	}


#line 121 "D:\DotNetProjects\Prexonite\Tools\Parser.frame" //FRAME


	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();

#line default //END FRAME -->parseRoot

		PTypeExpression();

#line 127 "D:\DotNetProjects\Prexonite\Tools\Parser.frame" //FRAME

    Expect(0);
	}
	
	bool[,] set = {

#line default //END FRAME -->initialization

		{T,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,T, T,T,T,T, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,T,x, x}

#line 132 "D:\DotNetProjects\Prexonite\Tools\Parser.frame" //FRAME

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
			case 2: s = "tilde expected"; break;
			case 3: s = "true expected"; break;
			case 4: s = "false expected"; break;
			case 5: s = "integer expected"; break;
			case 6: s = "real expected"; break;
			case 7: s = "string expected"; break;
			case 8: s = "\"(\" expected"; break;
			case 9: s = "\",\" expected"; break;
			case 10: s = "\")\" expected"; break;
			case 11: s = "??? expected"; break;
			case 12: s = "invalid Expr"; break;
			case 13: s = "invalid Boolean"; break;

#line 228 "D:\DotNetProjects\Prexonite\Tools\Parser.frame" //FRAME

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