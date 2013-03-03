// Prexonite
// 
// Copyright (c) 2013, Christian Klauser
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
namespace Prexonite.Compiler
{
    public static class MessageClasses
    {
        public const string SelfAssembly = "SA.Error";
        public const string NoSymbolEntryEquivalentToSymbol = "ES.NoSymbolEntryEquivalentToSymbol";
        public const string InvalidSymbolInterpretation = "ES.InvalidSymbolInterpretation";
        public const string SymbolConflict = "ES.SymbolConflict";
        public const string SymbolNotResolved = "ES.SymbolNotResolved";

        #region Parser

        public const string CannotCreateReference = "P.CannotCreateReference";
        public const string CannotUseExpressionAsConstructor = "P.CannotUseExpressionAsConstructor";
        public const string UnexpectedModuleName = "P.UnexpectedModuleName";
        public const string SymbolAliasMissing = "P.SymbolAliasMissing";
        public const string EntityNameMissing = "P.EntityNameMissing";
        public const string MissingColonInDeclaration = "P.MissingColonInDeclaration";
        public const string ExceptionDuringCompilation = "P.ExceptionDuringCompilation";
        public const string ErrorsInBuildBlock = "P.ErrorsInBuildBlock";
        public const string ObjectCreationSyntax = "P.ObjectCreationSyntax";
        public const string UnknownAssemblyOperator = "P.UnknownAssemblyOperator";
        public const string UnknownEntityRefType = "P.UnknownEntityRefType";
        public const string CannotParseMExpr = "P.CannotParseMExpr";
        public const string DuplicateComma = "P.DuplicateComma";
        public const string ParserInternal = "P.ParserInternal";
        public const string LValueExpected = "P.LValueExpected";
        public const string LegacyMacro = "P.LegacyMacro";
        public const string InnerMacrosIllegal = "P.InnerMacrosIllegal";
        public const string IllegalInitializationFunction = "P.IllegalInitializationFunction";
        public const string ThisReserved = "P.ThisReserved";
        public const string DuplicateVar = "P.DuplicateVar";
        public const string GlobalUnbindNotSupported = "P.GlobalUnbindNotSupported";

        #endregion

        #region Compiler / Code Generator

        public const string ReferenceToMacro = "C.ReferenceToMacro";
        public const string InvalidReference = "C.InvalidReference";
        public const string YieldFromProtectedBlock = "C.YieldFromProtectedBlock";
        public const string ParameterNameReserved = "C.ParameterNameReserved";
        public const string InvalidModifyingAssignment = "C.InvalidModifyingAssignment";
        public const string PartialApplicationNotSupported = "C.PartialApplicationNotSupported";
        public const string OnlyLastOperandPartialInLazy = "C.OnlyLastOperandPartialInLazy";
        public const string ForeachElementTooComplicated = "C.ForeachElementTooComplicated";
        public const string TypeExpressionExpected = "C.TypeExpressionExpected";

        #endregion

        #region Macro System

        public const string NoSuchMacroCommand = "M.NoSuchMacroCommand";
        public const string NoSuchMacroFunction = "M.NoSuchMacroFunction";
        public const string MacroNotReentrant = "M.MacroNotReentrant";
        public const string NotAMacro = "M.NotAMacro";
        public const string BlockMergingUsesVariable = "M.BlockMergingUsesVariable";
        public const string PartialMacroMustReturnBoolean = "M.PartialMacroMustReturnBoolean";
        public const string MacroReferenceForCallMacroMissing = "M.MacroReferenceForCallMacroMissing";
        public const string CallMacroCalledFromNonMacro = "M.CallMacroCalledFromNonMacro";
        public const string SpecifyPlaceholderIndexExplicitly = "M.SpecifyPlaceholderIndexExplicitly";
        public const string CallMacroUsage = "M.CallMacroUsage";
        public const string CallMacroNotOnPlaceholder = "M.CallMacroNotOnPlaceholder";
        public const string CallStarUsage = "M.CallStarUsage";
        public const string CallStarPassThrough = "M.CallStarPassThrough";
        public const string MacroContextOutsideOfMacro = "M.MacroContextOutsideOfMacro";
        public const string PackUsage = "M.PackUsage";
        public const string UnpackUsage = "M.UnpackUsage";
        public const string ReferenceUsage = "M.ReferenceUsage";
        public const string ApiMisuse = "M.ApiMisuse";
        public const string EntityRefTo = "M.EntityRefTo";

        #endregion

        #region Built-in Commands

        public const string SubUsage = "D.SubUsage";
        public const string SubAsExpressionInLoop = "D.SubAsExpressionInLoop";

        #endregion


    }
}
