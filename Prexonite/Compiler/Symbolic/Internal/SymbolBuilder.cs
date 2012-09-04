using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Prexonite.Modular;

namespace Prexonite.Compiler.Symbolic.Internal
{
    /// <summary>
    /// This class is intended to be used by the compiler alone.
    /// It facilitates building a symbol from the outside in, first recording ref and macro modifiers
    /// and only at the end, adding the actual core symbol.
    /// </summary>
    internal class SymbolBuilder : ICloneable
    {
        [Flags]
        public enum SymbolFlags
        {
            None = 0,
            Macro
        }

        public int DereferenceCount { get; set; }
        public SymbolFlags Flags { get; set; }
        public EntityRef Entity { get; set; }

        public List<Message> Messages
        {
            get { return _messages; }
        }

        public bool IsFlagSet(SymbolFlags flag)
        {
            return (Flags & flag) == flag;
        }

        public void Dereference()
        {
            DereferenceCount += 1;
        }

        public void ReferenceTo()
        {
            DereferenceCount -= 1;
        }

        private readonly List<Message> _messages = new List<Message>();

        public Symbol ToSymbol()
        {
            Symbol symbol;
            if(Entity == null)
            {
                if (!(IsTerminatedByError))
                    throw new PrexoniteException(
                        "Cannot construct symbol without an entity where the last message is not an error.");
                symbol = MessageSymbol.Create(Messages[Messages.Count - 1], null);
                return _wrapSymbol(symbol, usedLastMessage: true);
            }
            else
            {
                if (IsFlagSet(SymbolFlags.Macro))
                    symbol = ExpandSymbol.Create(Entity);
                else
                    symbol = CallSymbol.Create(Entity);
                return _wrapSymbol(symbol, usedLastMessage: false);
            }
        }

        public bool IsTerminatedByError
        {
            get { return Messages.Count - 1 >= 0 && Messages[Messages.Count - 1].Severity == MessageSeverity.Error; }
        }

        public Symbol WrapSymbol([CanBeNull]Symbol symbol)
        {
            if (symbol == null)
            {
                if (!(IsTerminatedByError))
                    throw new PrexoniteException(
                        "Cannot construct symbol without an entity where the last message is not an error.");
                symbol = MessageSymbol.Create(Messages[Messages.Count - 1], null);
                return _wrapSymbol(symbol, usedLastMessage: true);
            }
            else
            {
                return _wrapSymbol(symbol, usedLastMessage: false);
            }  
        }

        private Symbol _wrapSymbol([NotNull] Symbol symbol, bool usedLastMessage)
        {
            while (DereferenceCount > 0)
            {
                symbol = DereferenceSymbol.Create(symbol);
                DereferenceCount--;
            }

            while (DereferenceCount < 0)
            {
                symbol = ReferenceToSymbol.Create(symbol);
                DereferenceCount++;
            }

            for (var i = Messages.Count - (usedLastMessage ? 2 : 1); i >= 0; i--)
                symbol = MessageSymbol.Create(Messages[i], symbol);

            return symbol;
        }

        #region Implementation of ICloneable

        object ICloneable.Clone()
        {
            return Clone();
        }

        [NotNull,PublicAPI]
        public virtual SymbolBuilder Clone()
        {
            var c = new SymbolBuilder {DereferenceCount = DereferenceCount, Entity = Entity, Flags = Flags};
            c.Messages.AddRange(Messages);
            return c;
        }

        #endregion
    }
}
