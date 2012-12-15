using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Prexonite.Modular;
using Prexonite.Properties;

namespace Prexonite.Compiler.Symbolic.Internal
{
    /// <summary>
    /// This class is intended to be used by the compiler alone.
    /// It facilitates building a symbol from the outside in, first recording ref and macro modifiers
    /// and only at the end, adding the actual core symbol.
    /// </summary>
    internal class SymbolBuilder : ICloneable
    {
        public EntityRef Entity { get; set; }

        private Symbol _prefix = Symbol.CreateNil(NoSourcePosition.Instance);
        private int _dereferenceCount = 1;
        private readonly Queue<Message> _messages = new Queue<Message>();

        public SymbolBuilder Dereference()
        {
            _dereferenceCount += 1;
            return this;
        }

        public SymbolBuilder ReferenceTo()
        {
            _dereferenceCount -= 1;
            return this;
        }

        public SymbolBuilder AddMessage([NotNull]Message message)
        {
            if (message == null)
                throw new ArgumentNullException("message");
            _messages.Enqueue(message);
            return this;
        }

        public SymbolBuilder Expand()
        {
            _materializePrefix();
            _prefix = Symbol.CreateExpand(_prefix);
            return this;
        }

        private void _materializePrefix()
        {
            while (_dereferenceCount > 0)
            {
                _prefix = Symbol.CreateDereference(_prefix);
                _dereferenceCount--;
            }

            if(_dereferenceCount < 0)
            {
                _prefix =
                    Symbol.CreateMessage(
                        Message.Error(
                            Resources.SymbolBuilder_TooManyArrows, NoSourcePosition.Instance,
                            MessageClasses.CannotCreateReference), _prefix);
            }

            while(_messages.Count > 0)
                _prefix = Symbol.CreateMessage(_messages.Dequeue(), _prefix);
        }

        public Symbol ToSymbol()
        {
            Symbol symbol;
            if (Entity == null)
                symbol = null;
            else
                symbol = Symbol.CreateReference(Entity,NoSourcePosition.Instance);
            return WrapSymbol(symbol);
        }

        public Symbol WrapSymbol([CanBeNull]Symbol symbol)
        {
            _materializePrefix();
            if (symbol == null)
            {
                return _prefix;
            }
            else
            {
                return _prefix.HandleWith(ReplaceCoreNilHandler.Instance, symbol);
            }  
        }

        #region Implementation of ICloneable

        object ICloneable.Clone()
        {
            return Clone();
        }

        [NotNull,PublicAPI]
        public virtual SymbolBuilder Clone()
        {
            var c = new SymbolBuilder {_dereferenceCount = _dereferenceCount, Entity = Entity, _prefix = _prefix};
            foreach (var message in _messages)
                c._messages.Enqueue(message);
            return c;
        }

        #endregion
    }
}
