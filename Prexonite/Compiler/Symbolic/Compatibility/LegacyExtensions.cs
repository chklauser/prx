// Prexonite
// 
// Copyright (c) 2012, Christian Klauser
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

using System;
using System.Diagnostics;
using Prexonite.Modular;
using Prexonite.Properties;

namespace Prexonite.Compiler.Symbolic.Compatibility
{
    // TODO: Mark obsolote and remove
    public static class LegacyExtensions
    {
        private class SymbolEntryConversion : ISymbolHandler<object, SymbolEntry>
        {
            public SymbolEntry HandleCall(CallSymbol symbol, object argument)
            {
                return symbol.Entity.ToSymbolEntry();
            }

            public SymbolEntry HandleExpand(ExpandSymbol symbol, object argument)
            {
                return symbol.Entity.ToSymbolEntry();
            }

            public SymbolEntry HandleMessage(MessageSymbol symbol, object argument)
            {
                throw new SymbolConversionException(Resources.SymbolEntryConversion_MessageSymbol_cannot_be_converted_to_SymbolEntry, symbol);
            }

            public SymbolEntry HandleDereference(DereferenceSymbol symbol, object argument)
            {
                CallSymbol callSymbol;
                EntityRef.Variable variable;
                if(symbol.Symbol.TryGetCallSymbol(out callSymbol) && callSymbol.Entity.TryGetVariable(out variable))
                {
                    var baseEntry = variable.ToSymbolEntry();
                    switch (baseEntry.Interpretation)
                    {
                        case SymbolInterpretations.GlobalObjectVariable:
                            return baseEntry.With(SymbolInterpretations.GlobalReferenceVariable);
                        case SymbolInterpretations.LocalObjectVariable:
                            return baseEntry.With(SymbolInterpretations.LocalReferenceVariable);
                    }
                }
                throw new SymbolConversionException(
                    Resources.
                        SymbolEntryConversion_No_arbirtrary_dereference,
                    symbol);
            }

            public SymbolEntry HandleReferenceTo(ReferenceToSymbol symbol, object argument)
            {
                CallSymbol callSymbol;
                EntityRef.Variable variable;
                if (symbol.Symbol.TryGetCallSymbol(out callSymbol) && callSymbol.Entity.TryGetVariable(out variable))
                {
                    var baseEntry = variable.ToSymbolEntry();
                    switch (baseEntry.Interpretation)
                    {
                        case SymbolInterpretations.GlobalReferenceVariable:
                            return baseEntry.With(SymbolInterpretations.GlobalObjectVariable);
                        case SymbolInterpretations.LocalReferenceVariable:
                            return baseEntry.With(SymbolInterpretations.GlobalReferenceVariable);
                    }
                }
                throw new SymbolConversionException(
                    Resources.
                        SymbolEntryConversion_No_arbirtrary_dereference,
                    symbol);
            }

            public SymbolEntry HandleMacroInstance(MacroInstanceSymbol symbol, object argument)
            {
                throw new SymbolConversionException(Resources.SymbolEntryConversion_MacroInstance_not_supported, symbol);
            }
        }
        private static readonly SymbolEntryConversion _convertSymbol = new SymbolEntryConversion();


        public static SymbolEntry ToSymbolEntry(this Symbol symbol)
        {
            return symbol.HandleWith(_convertSymbol, null);
        }

        public static Symbol ToSymbol(this SymbolEntry entry)
        {
            var isDereferenced = false;
            EntityRef entity; 
            switch (entry.Interpretation)
            {
                case SymbolInterpretations.Function:
                    entity = EntityRef.Function.Create(entry.InternalId, entry.Module);
                    break;
                case SymbolInterpretations.Command:
                    entity = EntityRef.Command.Create(entry.InternalId);
                    break;
                case SymbolInterpretations.LocalObjectVariable:
                    entity = EntityRef.Variable.Local.Create(entry.InternalId);
                    break;
                case SymbolInterpretations.LocalReferenceVariable:
                    entity = EntityRef.Variable.Local.Create(entry.InternalId);
                    isDereferenced = true;
                    break;
                case SymbolInterpretations.GlobalObjectVariable:
                    entity = EntityRef.Variable.Global.Create(entry.InternalId, entry.Module);
                    break;
                case SymbolInterpretations.GlobalReferenceVariable:
                    entity = EntityRef.Variable.Global.Create(entry.InternalId, entry.Module);
                    isDereferenced = true;
                    break;
                case SymbolInterpretations.MacroCommand:
                    entity = EntityRef.MacroCommand.Create(entry.InternalId);
                    break;
                default:
                    var interpretation = Enum.GetName(typeof (SymbolInterpretations), entry.Interpretation);
                    throw new ArgumentOutOfRangeException("entry", interpretation,
                                                          string.Format("Cannot convert symbol entry {0} to a symbol.",
                                                                        entry));
            }

            if (isDereferenced)
                return DereferenceSymbol.Create(CallSymbol.Create(entity));
            else
                return CallSymbol.Create(entity);
        }
    }

    public class SymbolConversionException : Exception
    {
        private readonly Symbol _symbol;

        public SymbolConversionException(Symbol symbol)
        {
            _symbol = symbol;
        }

        public SymbolConversionException(string message, Symbol symbol)
            : base(message)
        {
            _symbol = symbol;
        }

        public SymbolConversionException(string message, Symbol symbol, Exception inner)
            : base(message, inner)
        {
            _symbol = symbol;
        }

        public Symbol Symbol1
        {
            [DebuggerStepThrough]
            get { return _symbol; }
        }
    }
}