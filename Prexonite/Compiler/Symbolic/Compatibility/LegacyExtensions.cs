using JetBrains.Annotations;
using Prexonite.Modular;
using Prexonite.Properties;

namespace Prexonite.Compiler.Symbolic.Compatibility;

[Obsolete("Use EntityRef API instead")]
public static class LegacyExtensions
{
    class SymbolEntryConversion : ISymbolHandler<object, SymbolEntry>
    {
        public SymbolEntry HandleReference(ReferenceSymbol self, object argument)
        {
            throw new SymbolConversionException(
                Resources.SymbolEntryConversion_BareReference,
                self
            );
        }

        public SymbolEntry HandleNamespace(NamespaceSymbol self, object argument)
        {
            throw new SymbolConversionException(Resources.SymbolEntryConversion_Namespace, self);
        }

        public SymbolEntry HandleNil(NilSymbol self, object argument)
        {
            throw new SymbolConversionException(Resources.SymbolEntryConversion_Nil, self);
        }

        public SymbolEntry HandleExpand(ExpandSymbol self, object argument)
        {
            SymbolEntry symEn;
            if (
                self.InnerSymbol.TryGetReferenceSymbol(out var refSym)
                && (symEn = refSym.Entity.ToSymbolEntry()).Interpretation
                    == SymbolInterpretations.MacroCommand
            )
            {
                return symEn;
            }
            else
            {
                throw new SymbolConversionException(
                    Resources.SymbolEntryConversion_ExpansionSymbolTooComplex,
                    self
                );
            }
        }

        public SymbolEntry HandleMessage(MessageSymbol self, object argument)
        {
            throw new SymbolConversionException(
                Resources.SymbolEntryConversion_MessageSymbol_cannot_be_converted_to_SymbolEntry,
                self
            );
        }

        public SymbolEntry HandleDereference(DereferenceSymbol self, object argument)
        {
            if (self.InnerSymbol.TryGetReferenceSymbol(out var refSym))
            {
                var sym = refSym.Entity.ToSymbolEntry();
                if (sym.Interpretation != SymbolInterpretations.MacroCommand)
                    return sym;
            }
            else
            {
                // double deref is for ref locals and ref globals
                if (self.InnerSymbol.TryGetDereferenceSymbol(out var innerDerefSym))
                {
                    var baseEntry = innerDerefSym.ToSymbolEntry();
                    switch (baseEntry.Interpretation)
                    {
                        case SymbolInterpretations.GlobalObjectVariable:
                            return baseEntry.With(SymbolInterpretations.GlobalReferenceVariable);
                        case SymbolInterpretations.LocalObjectVariable:
                            return baseEntry.With(SymbolInterpretations.LocalReferenceVariable);
                    }
                }
            }
            throw new SymbolConversionException(
                Resources.SymbolEntryConversion_No_arbirtrary_dereference,
                self
            );
        }
    }

    static readonly SymbolEntryConversion _convertSymbol = new();

    extension(Symbol symbol)
    {
        public SymbolEntry ToSymbolEntry()
        {
            return symbol.HandleWith(_convertSymbol, null!);
        }
    }

    extension(SymbolEntry entry)
    {
        public Symbol ToSymbol()
        {
            var isDereferenced = false;
            EntityRef entity;
            switch (entry.Interpretation)
            {
                case SymbolInterpretations.Function:
                    entity = EntityRef.Function.Create(entry.InternalId!, entry.Module!);
                    break;
                case SymbolInterpretations.Command:
                    entity = EntityRef.Command.Create(entry.InternalId!);
                    break;
                case SymbolInterpretations.LocalObjectVariable:
                    entity = EntityRef.Variable.Local.Create(entry.InternalId!);
                    break;
                case SymbolInterpretations.LocalReferenceVariable:
                    entity = EntityRef.Variable.Local.Create(entry.InternalId!);
                    isDereferenced = true;
                    break;
                case SymbolInterpretations.GlobalObjectVariable:
                    entity = EntityRef.Variable.Global.Create(entry.InternalId!, entry.Module!);
                    break;
                case SymbolInterpretations.GlobalReferenceVariable:
                    entity = EntityRef.Variable.Global.Create(entry.InternalId!, entry.Module!);
                    isDereferenced = true;
                    break;
                case SymbolInterpretations.MacroCommand:
                    entity = EntityRef.MacroCommand.Create(entry.InternalId!);
                    break;
                default:
                    var interpretation = Enum.GetName(
                        typeof(SymbolInterpretations),
                        entry.Interpretation
                    );
                    throw new ArgumentOutOfRangeException(
                        nameof(entry),
                        interpretation,
                        $"Cannot convert symbol entry {entry} to a symbol."
                    );
            }

            if (isDereferenced)
                return Symbol.CreateDereference(
                    Symbol.CreateCall(entity, NoSourcePosition.Instance)
                );
            else
                return Symbol.CreateCall(entity, NoSourcePosition.Instance);
        }
    }
}

public class SymbolConversionException : Exception
{
    public SymbolConversionException(Symbol symbol)
    {
        Symbol = symbol;
    }

    public SymbolConversionException(string message, Symbol symbol)
        : base(message)
    {
        Symbol = symbol;
    }

    public SymbolConversionException(string message, Symbol symbol, Exception inner)
        : base(message, inner)
    {
        Symbol = symbol;
    }

    [PublicAPI]
    public Symbol Symbol { get; }
}
