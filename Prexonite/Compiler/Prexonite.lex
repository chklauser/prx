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
#nullable disable
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable InvalidXmlDocComment
// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CheckNamespace
// ReSharper disable RedundantUsingDirective
// ReSharper disable RedundantArrayCreationExpression
// ReSharper disable RedundantAssignment
// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable RedundantNameQualifier
// ReSharper disable RedundantJumpStatement
// ReSharper disable PossibleNullReferenceException

//Prexonite Scanner file.

using System;
using System.Text;
using System.IO;
using Prexonite;
using Prexonite.Types;
using Prexonite.Commands;
using Prexonite.Compiler;
using Prexonite.Compiler.Ast;

partial
%%

%class Lexer
%function Scan
%type Token
%line
%column
%char
%unicode
%ignorecase
%implements Prexonite.Internal.IScanner

%eofval{
    return tok(Parser._EOF);
%eofval}

Digit               = [0-9]
HexDigit	        = [0-9ABCDEFabcdef]
Integer             = {Digit} ("'" | {Digit})*
Exponent            = e("+"|"-")?{Integer}
//Identifier          = {Letter} ( {Letter} | {Digit} )*
Identifier          = ([:jletter:] | [\\]) ([:jletterdigit:] | [\\] | "'")*
//For reference, a line break would be defined like this, but apart
// from line comments, Prexonite Script does not care about line breaks
//LineBreak           = \r\n|\r|\n|\u2028|\u2029|\u000B|\u000C|\u0085
NotLineBreak        = [^\r\n\u2028\u2029\u000B\u000C\u0085]
WhiteSpace          = [ \t\r\n\u2028\u2029\u000B\u000C\u0085]
RegularStringChar   = [^$\"\\\r\n\u2028\u2029\u000B\u000C\u0085]
RegularVerbatimStringChar = [^$\"]
Noise               = "/*" ~"*/" | "//" {NotLineBreak}* | {WhiteSpace}+

// String: a constant string with C-style escape sequences
// SmartString: a string with C-style escape sequences and $-interpolation (needs support from Parser)
// VerbatimString: a constant string where only "" escapes to ".
// SmartVerbatimString: a string with $-interpolation where only "" escapes to " (needs support from Parser)
// Local: code body (function body, RHS of global variable, etc.)
// LocalShell: like code body, but accepts flag literals (-q, --long, --option=EXPR)
// Asm: assembler code (resembles global scope)
// Transfer: Body of a symbol transfer expression (namespace declaration), mostly like global scope
// YYINITIAL: the global scope
%state String, SmartString, VerbatimString, SmartVerbatimString, VerbatimBlock, Local, LocalShell, Asm, Transfer

%%

//Only global code
<YYINITIAL> {
    "to"        { return tok(Parser._to); }
    "#!" {NotLineBreak}* { return tok(Parser._interpreterline); }
}

<YYINITIAL,Local,LocalShell> {
     "does" { return tok(Parser._does); }
}

//Not local code
<YYINITIAL,Asm, Transfer> {
    "\""          { buffer.Length = 0; PushState(String); }
    "@\""         { buffer.Length = 0; PushState(VerbatimString); }
}

//Only local code
<Local,LocalShell> {
    "\""        { buffer.Length = 0; PushState(SmartString); }
    "@\""       { buffer.Length = 0; PushState(SmartVerbatimString); }

}

// Everywhere in code except in symbol transfer specifications
// this is a hack to get around an ambiguity of (*)
<YYINITIAL,Local,LocalShell,Asm> {
     "(*)" { return tok(Parser._id,OperatorNames.Prexonite.Multiplication); }
}

<Transfer> {
     "(*)" { return tok(Parser._timessym); }
}

// When flag literals are enabled, GNU-style flags (-f, -vf, --long, --opt=EXPR) are recognized.
// This mode is of course not compatible with Prexonite code unaware of shell extensions
<LocalShell> {
    "-" "-"? [:jletterdigit:] ([:jletterdigit:]|"-")* "="? {
                                                                String flag = yytext();
                                                                if(flag.EndsWith("=")) {
                                                                    return multiple(
                                                                        tok(Parser._string, flag),
                                                                        tok(Parser._plus, "+")
                                                                    );
                                                                }
                                                                else {
                                                                    return tok(Parser._string, flag);
                                                                }
                                                            }
}

//Everywhere in code
 <YYINITIAL,Local,LocalShell,Asm,Transfer> {

     {Noise}    { /* Comment/Whitespace: ignore */ }

     {Integer} "." {Integer}  {Exponent} { return tok(Parser._real, yytext()); } //definite real
     {Integer} "." {Integer}             { return tok(Parser._realLike, yytext()); } //could also be version literal
     {Integer} "." {Integer} "." {Integer} ("." {Integer})? { return tok(Parser._version, yytext()); }

     {Integer}                  |
     0x{HexDigit}+              { return tok(Parser._integer, yytext()); }

     "true" { return tok(Parser._true); }
     "false" { return tok(Parser._false); }
     "var"  { return tok(Parser._var); }
     "ref"  { return tok(Parser._ref); }

     "$"	{Identifier} "::" { string ns = yytext();
								return tok(Parser._ns, ns.Substring(1,ns.Length-3)); }

	 {Identifier} "::" { string ns = yytext();
                         return tok(Parser._ns, ns.Substring(0, ns.Length-2)); }

     //any identifier

     "$"    {Identifier} { return tok(Parser._id, yytext().Substring(1)); }
     "$\""               { buffer.Length = 0; PushState(String); return tok(Parser._anyId); }
     // Support $@ to mean "var args". Emulated $@ from bash.
     "$@"                { return multiple(tok(Parser._var), tok(Parser._id, PFunction.ArgumentListId)); }

     {Identifier} { return tok(checkKeyword(yytext()), yytext()); }


     "{"    { return tok(Parser._lbrace); }
     "["    { return tok(Parser._lbrack); }
     "(+)" { return tok(Parser._id,OperatorNames.Prexonite.Addition); }
     "(-)" { return tok(Parser._id,OperatorNames.Prexonite.Subtraction); }
     // NOTE: the multiplication operator has a different meaning in state Transfer vs. other states 
     "(/)" { return tok(Parser._id,OperatorNames.Prexonite.Division); }
     "(" [mM][oO][dD] ")" { return tok(Parser._id,OperatorNames.Prexonite.Modulus); }
     "(^)" { return tok(Parser._id,OperatorNames.Prexonite.Power); }
     "(&)" { return tok(Parser._id,OperatorNames.Prexonite.BitwiseAnd); }
     "(|)" { return tok(Parser._id,OperatorNames.Prexonite.BitwiseOr); }
     "(" [xX][oO][rR] ")" { return tok(Parser._id,OperatorNames.Prexonite.ExclusiveOr); }
     "(==)" { return tok(Parser._id,OperatorNames.Prexonite.Equality); }
     "(!=)" { return tok(Parser._id,OperatorNames.Prexonite.Inequality); }
     "(>)" { return tok(Parser._id,OperatorNames.Prexonite.GreaterThan); }
     "(>=)" { return tok(Parser._id,OperatorNames.Prexonite.GreaterThanOrEqual); }
     "(<)" { return tok(Parser._id,OperatorNames.Prexonite.LessThan); }
     "(<=)" { return tok(Parser._id,OperatorNames.Prexonite.LessThanOrEqual); }
     "(-.)" { return tok(Parser._id,OperatorNames.Prexonite.UnaryNegation); }
     "(++)" { return tok(Parser._id,OperatorNames.Prexonite.Increment); }
     "(--)" { return tok(Parser._id,OperatorNames.Prexonite.Decrement); }
     "(<|.)" { return tok(Parser._id,OperatorNames.Prexonite.UnaryDeltaLeftPre); }
     "(.<|)" { return tok(Parser._id,OperatorNames.Prexonite.UnaryDeltaLeftPost); }
     "(|>.)" { return tok(Parser._id,OperatorNames.Prexonite.UnaryDeltaRightPre); }
     "(.|>)" { return tok(Parser._id,OperatorNames.Prexonite.UnaryDeltaRightPost); }
     "(<|)" { return tok(Parser._id,OperatorNames.Prexonite.BinaryDeltaLeft); }
     "(|>)" { return tok(Parser._id,OperatorNames.Prexonite.BinaryDeltaRight); }
     "("    { return tok(Parser._lpar); }
     ")"    { return tok(Parser._rpar); }
     "]"    { return tok(Parser._rbrack); }
     "}"    { return tok(Parser._rbrace); }
     "+"    { return tok(Parser._plus); }
     "-"    { return tok(Parser._minus); }
     "*"    { return tok(Parser._times); }
     "/"    { return tok(Parser._div); }
     "^"    { return tok(Parser._pow); }
     "="    { return tok(Parser._assign); }
     "<|"   { return tok(Parser._deltaleft); }
     "|>"   { return tok(Parser._deltaright); }
    "&&"    { return tok(Parser._and); }
    "||"    { return tok(Parser._or); }
    "|"     { return tok(Parser._bitOr); }
    "&"     { return tok(Parser._bitAnd); }
    "=="    { return tok(Parser._eq); }
    "!="    { return tok(Parser._ne); }
    ">"     { return tok(Parser._gt); }
    ">="    { return tok(Parser._ge); }
    "<="    { return tok(Parser._le); }
    "<"     { return tok(Parser._lt); }
    "++"    { return tok(Parser._inc); }
    "--"    { return tok(Parser._dec); }
    "~"     { return tok(Parser._tilde); }
    "::"    { return tok(Parser._doublecolon); }
    "??"	{ return tok(Parser._coalescence); }
    "?"     { return tok(Parser._question); }
    "->"    { return tok(Parser._pointer); }
    "=>"    { return tok(Parser._implementation); }
    ":"     { return tok(Parser._colon); }
    ";"     { return tok(Parser._semicolon); }
    ","     { return tok(Parser._comma); }
    "."     { return tok(Parser._dot); }
    "@"     { return tok(Parser._at); }
    ">>"	{ return tok(Parser._appendright); }
    "<<"	{ return tok(Parser._appendleft); }
    "!"     { return tok(Parser._not); }

    .|\n    { throw new PrexoniteException(System.String.Format("Invalid character \"{0}\" detected on line {1} in {2}.", yytext(), yyline, File)); }
}

<String> {
    "\""        { PopState();
                  ret(tok(Parser._string, buffer.ToString()));
                  buffer.Length = 0;
                }
    {RegularStringChar}+ { buffer.Append(yytext()); }
    "\\\\"      { buffer.Append("\\"); }
    "\\\""      { buffer.Append("\""); }
    "\\"&       { /* nothing to do  */ }
    "\\"0       { buffer.Append("\0"); }
    "\\"a       { buffer.Append("\a"); }
    "\\"b       { buffer.Append("\b"); }
    "\\"f       { buffer.Append("\f"); }
    "\\"n       { buffer.Append("\n"); }
    "\\"r       { buffer.Append("\r"); }
    "\\"v       { buffer.Append("\v"); }
    "\\"t       { buffer.Append("\t"); }
    "\\"x {HexDigit} {HexDigit}? {HexDigit}? {HexDigit}? |
    "\\"u {HexDigit} {HexDigit}  {HexDigit}  {HexDigit}  |
    "\\"U {HexDigit} {HexDigit}  {HexDigit}  {HexDigit}  {HexDigit}  {HexDigit}  {HexDigit}  {HexDigit} { buffer.Append(_unescapeChar(yytext())); }
    //No need to escape $, but possible.
    "$" | "\\$"         { buffer.Append("$"); }
}

<SmartString> {
    "\""        { PopState();
                  ret(tok(Parser._string, buffer.ToString()));
                  buffer.Length = 0;
                }
    {RegularStringChar}+ { buffer.Append(yytext()); }
    "\\\\"      { buffer.Append("\\"); }
    "\\\""      { buffer.Append("\""); }
    "\\"&       { /* nothing to do  */ }
    "\\"0       { buffer.Append("\0"); }
    "\\"a       { buffer.Append("\a"); }
    "\\"b       { buffer.Append("\b"); }
    "\\"f       { buffer.Append("\f"); }
    "\\"n       { buffer.Append("\n"); }
    "\\"r       { buffer.Append("\r"); }
    "\\"v       { buffer.Append("\v"); }
    "\\"t       { buffer.Append("\t"); }
    "\\"x {HexDigit} {HexDigit}? {HexDigit}? {HexDigit}? |
    "\\"u {HexDigit} {HexDigit}  {HexDigit}  {HexDigit}  |
    "\\"U {HexDigit} {HexDigit}  {HexDigit}  {HexDigit}  {HexDigit}  {HexDigit}  {HexDigit}  {HexDigit} { buffer.Append(_unescapeChar(yytext())); }
    "\\$"       { buffer.Append("$"); }
    "$" {Identifier} &?
                    {   string clipped;
                        string id = _pruneSmartStringIdentifier(yytext(), out clipped);
                        string fragment = buffer.ToString();
                        buffer.Length = 0;
                        return multiple(
                            tok(Parser._string, fragment),
                            tok(Parser._plus),
                            tok(Parser._id, id),
                            clipped != null ? tok(Parser._plus) : null,
                            clipped != null ? tok(Parser._string, clipped) : null,
                            tok(Parser._plus)
                        );
                     }
    "$("             {  string fragment = buffer.ToString();
                        buffer.Length = 0;
                        PushState(_surroundingLocalState);
                        return multiple(
                            tok(Parser._string, fragment),
                            tok(Parser._plus),
                            tok(Parser._LPopExpr),
                            tok(Parser._lpar)
                            //2nd plus is injected by the parser
                        );
                     }
    .|\n        { throw new PrexoniteException("Invalid smart string character '" + yytext() + "' (ASCII " + ((int)yytext()[0]) + ") in input on line " + yyline + "."); }
}

<VerbatimString> {
    "\""        { PopState();
                  ret(tok(Parser._string, buffer.ToString()));
                  buffer.Length = 0;
                }
    {RegularVerbatimStringChar} { buffer.Append(yytext()); }
    "\"\""      { buffer.Append("\""); }
    "$"         { buffer.Append("$"); }

}

<SmartVerbatimString> {
    "\""        { PopState();
                  ret(tok(Parser._string, buffer.ToString()));
                  buffer.Length = 0;
                }
    {RegularVerbatimStringChar} { buffer.Append(yytext()); }
    "\"\""      { buffer.Append("\""); }
    "$" {Identifier} &?
                    {   string clipped;
                        string id = _pruneSmartStringIdentifier(yytext(), out clipped);
                        string fragment = buffer.ToString();
                        buffer.Length = 0;
                        return multiple(
                            tok(Parser._string, fragment),
                            tok(Parser._plus),
                            tok(Parser._id, id),
                            clipped != null ? tok(Parser._plus) : null,
                            clipped != null ? tok(Parser._string, clipped) : null,
                            tok(Parser._plus)
                        );
                     }
    "$("             {  string fragment = buffer.ToString();
                        buffer.Length = 0;
                        PushState(_surroundingLocalState);
                        return multiple(
                            tok(Parser._string, fragment),
                            tok(Parser._plus),
                            tok(Parser._LPopExpr),
                            tok(Parser._lpar)
                            //2nd plus is injected by the parser
                        );
                     }

}

//Error symbol
.|\n            { throw new PrexoniteException(System.String.Format("Invalid character \"{0}\" detected on line {1} in {2}.", yytext(), yyline, File)); }
