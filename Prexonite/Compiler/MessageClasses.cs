namespace Prexonite.Compiler
{
    public static class MessageClasses
    {
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
        public const string OnlyErrorStandalone = "P.OnlyErrorStandalone";
        public const string MissingColonInDeclaration = "P.MissingColonInDeclaration";
        public const string ExceptionDuringCompilation = "P.ExceptionDuringCompilation";

        #endregion

        #region Compiler / Code Generator

        public const string ReferenceToMacro = "C.ReferenceToMacro";
        public const string InvalidReference = "C.InvalidReference";
        public const string YieldFromProtectedBlock = "C.YieldFromProtectedBlock";
        public const string ParameterNameReserved = "C.ParameterNameReserved";
        public const string InvalidModifyingAssignment = "C.InvalidModifyingAssignment";
        public const string PartialApplicationNotSupported = "C.PartialApplicationNotSupported";

        #endregion

        #region Macro System

        public const string NoSuchMacroCommand = "M.NoSuchMacroCommand";
        public const string NoSuchMacroFunction = "M.NoSuchMacroFunction";
        public const string MacroNotReentrant = "M.MacroNotReentrant";
        public const string NotAMacro = "M.NotAMacro";
        public const string BlockMergingUsesVariable = "M.BlockMergingUsesVariable";
        public const string PartialMacroMustReturnBoolean = "M.PartialMacroMustReturnBoolean";

        #endregion

    }
}
