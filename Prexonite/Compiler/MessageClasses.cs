namespace Prexonite.Compiler;

public static class MessageClasses
{
    public const string SelfAssembly = "SA.Error";
    public const string NoSymbolEntryEquivalentToSymbol = "ES.NoSymbolEntryEquivalentToSymbol";
    public const string InvalidSymbolInterpretation = "ES.InvalidSymbolInterpretation";
    public const string SymbolConflict = "ES.SymbolConflict";
    public const string SymbolNotResolved = "ES.SymbolNotResolved";
    public const string UnqualifiedImport = "ES.UnqualifiedImport";

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
    public const string NamespaceExcepted = "P.NamespaceExpected";
    public const string CannotExtendMergedNamespace = "P.CannotExtendMergedNamespace";
    public const string ImportExpected = "P.ImportExpected";
    public const string UnexpectedWildcard = "P.UnexpectedWildcard";
    public const string QualifiedIdPartsAfterWildcard = "P.QualifiedIdPartsAfterWildcard";
    public const string NonTopLevelNamespaceImport = "P.NonTopLevelNamespaceImport";
    public const string UnexpectedDoubleColonInNamespaceName =
        "P.UnexpectedDoubleColonInNamespaceName";
    public const string IncompleteBinaryOperation = "P.IncompleteBinaryOperation";
    public const string ShiftedSymbolRequired = "P.ShiftedSymbolRequired";
    public const string TypeArgumentsNotSupported = "P.TypeArgumentsNotSupported";
    public const string SkeletonNamespaceRequiresSemicolon = "P.SkeletonNamespaceRequiresSemicolon";
    public const string CannotFindPType = "P.CannotFindPType";
    public const string ErrorInTypeExpr = "P.ErrorInTypeExpr";
    public const string InvalidConstant = "P.InvalidConstant";
    public const string MemberAccessWithoutSubject = "P.MemberAccessWithoutSubject";
    public const string LoopExprNotSupported = "P.LoopExprNotSupported";
    public const string TupleNotSupported = "P.TupleNotSupported";
    public const string FunctionOverridingNotAllowed = "P.FunctionOverridingNotAllowed";
    public const string RefInLazyFunction = "P.RefInLazyFunction";
    public const string ReturnValueAssignment = "P.ReturnValueAssignment";
    public const string CatchWithoutException = "P.CatchWithoutException";

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
    public const string ExpectedEntityFoundNamespace = "C.NamespaceInEntityPosition";
    public const string ArgumentSpliceNotSupported = "C.ArgumentSpliceNotSupported";
    public const string IncompleteBuiltinConversion = "C.IncompleteBuiltinConversion";
    public const string IncompleteBuiltinTypeCheck = "C.IncompleteBuiltinTypeCheck";
    public const string IncompleteBuiltinStaticCall = "C.IncompleteBuiltinStaticCall";
    public const string IncompleteBuiltinObjectCreation = "C.IncompleteBuiltinObjectCreation";
    public const string RepeatedFileTransclusion = "C.RepeatedFileTransclusion";

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
