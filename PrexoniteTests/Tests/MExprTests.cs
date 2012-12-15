using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Prexonite.Compiler;
using Prexonite.Compiler.Internal;
using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;

namespace PrexoniteTests.Tests
{
    [TestFixture]
    public class MExprTests
    {
        private ISourcePosition _position;
        private ISourcePosition _otherPosition;
        private SymbolStore _symbols;
        private IDictionary<Symbol, String> _existing;
            
        [SetUp]
        public void SetUp()
        {
            _position = new SourcePosition("MExprTests.cs",20,62);
            _symbols = SymbolStore.Create();
            _existing = new Dictionary<Symbol, string>();
            _otherPosition = new SourcePosition("PrexoniteTests.csproj",33,77);
        }

        [Test]
        public void Nil()
        {
            var nil = Symbol.CreateNil(_position);

            var mnil = nil.HandleWith(SymbolMExprSerializer.Instance, _existing);
            var nil2 = SymbolMExprParser.Parse(_symbols, mnil);

            Assert.That(nil2,Is.InstanceOf<NilSymbol>());
            Assert.That(nil2.Position,Is.EqualTo(_position));
            Assert.That(nil2,Is.EqualTo(nil));
        }

        [Test]
        public void DereferenceNil()
        {
            var s1 = Symbol.CreateDereference(Symbol.CreateNil(_position));

            var mnil = s1.HandleWith(SymbolMExprSerializer.Instance, _existing);
            var s2 = SymbolMExprParser.Parse(_symbols, mnil);

            Assert.That(s2, Is.InstanceOf<DereferenceSymbol>());
            Assert.That(s2.Position, Is.EqualTo(_position));
            Assert.That(s2,Is.EqualTo(s1));
        }

        [Test]
        public void ExpandNil()
        {
            var s1 = Symbol.CreateExpand(Symbol.CreateNil(_position));

            var mnil = s1.HandleWith(SymbolMExprSerializer.Instance, _existing);
            var s2 = SymbolMExprParser.Parse(_symbols, mnil);

            Assert.That(s2, Is.InstanceOf<ExpandSymbol>());
            Assert.That(s2.Position, Is.EqualTo(_position));
            Assert.That(s2, Is.EqualTo(s1));
        }

        [Test]
        public void ErrorSymbolNil()
        {
            const string mc = "T.ZOMG";
            var m = Message.Error("OMG! Everything failed!", _position, mc);
            var s1 = Symbol.CreateMessage(m,Symbol.CreateNil(_position));

            var mnil = s1.HandleWith(SymbolMExprSerializer.Instance, _existing);
            dynamic s2 = SymbolMExprParser.Parse(_symbols, mnil);

            Assert.That(s2, Is.InstanceOf<MessageSymbol>());
            Assert.That(s2.Position, Is.EqualTo(_position));
            Assert.That(s2.Message, Is.EqualTo(m));
            Assert.That(s2, Is.EqualTo(s1));
        }

        [Test]
        public void InfoSymbolExpandDereferenceNil()
        {
            const string mc = "T.ZOMG";
            var m = Message.Info("Everything is quite ok.", _position, mc);
            var s1 = Symbol.CreateMessage(m, Symbol.CreateExpand(Symbol.CreateDereference(Symbol.CreateNil(_position))));

            var mnil = s1.HandleWith(SymbolMExprSerializer.Instance, _existing);
            dynamic s2 = SymbolMExprParser.Parse(_symbols, mnil);

            Assert.That(s2, Is.InstanceOf<MessageSymbol>());
            Assert.That(s2.Position, Is.EqualTo(_position));
            Assert.That(s2.Message, Is.EqualTo(m));
            Assert.That(s2, Is.EqualTo(s1));
        }

        [Test]
        public void DirectAlias()
        {
            const string alias = "x";
            var me = new MExpr.MList(_position,SymbolMExprSerializer.CrossReferenceHead, new[] {new MExpr.MAtom(_position, alias)});
            var s0 = Symbol.CreateNil(_position);
            _symbols.Declare(alias,s0);

            dynamic s2 = SymbolMExprParser.Parse(_symbols, me);

            Assert.That(s2,Is.InstanceOf<NilSymbol>());
            Assert.That(s2,Is.SameAs(s0));
        }


        [Test]
        public void ExpandedAlias()
        {
            const string alias = "x";
            var me = new MExpr.MList(_position, SymbolMExprSerializer.ExpandHead,_createCrossReference(alias));
            var s0 = Symbol.CreateNil(_otherPosition);
            _symbols.Declare(alias, s0);

            dynamic s2 = SymbolMExprParser.Parse(_symbols, me);

            Assert.That(s2, Is.InstanceOf<ExpandSymbol>());
            Assert.That(s2.Position, Is.EqualTo(_position));
            Assert.That(s2.InnerSymbol, Is.SameAs(s0));
        }

        [Test]
        public void UseAlias()
        {
            var s0 = Symbol.CreateNil(_otherPosition);
            var s1 = Symbol.CreateDereference(s0, _position);

            _existing.Add(s0,"x");
            _symbols.Declare("x",s0);

            var m = s1.HandleWith(SymbolMExprSerializer.Instance, _existing);
            dynamic s2 = SymbolMExprParser.Parse(_symbols, m);

            Assert.That(s2,Is.InstanceOf<DereferenceSymbol>());
            Assert.That(s2,Is.EqualTo(s1));
            Assert.That(s2.InnerSymbol,Is.SameAs(s0));
        }

        [Test]
        public void CommandReference()
        {
            var s1 = Symbol.CreateReference(EntityRef.Command.Create("c"), _position);

            var m = s1.HandleWith(SymbolMExprSerializer.Instance, _existing);
            dynamic s2 = SymbolMExprParser.Parse(_symbols, m);

            Assert.That(s2,Is.EqualTo(s1));
        }

        [Test]
        public void MacroCommandReference()
        {
            var s1 = Symbol.CreateReference(EntityRef.MacroCommand.Create("c"), _position);

            var m = s1.HandleWith(SymbolMExprSerializer.Instance, _existing);
            dynamic s2 = SymbolMExprParser.Parse(_symbols, m);

            Assert.That(s2, Is.EqualTo(s1));
        }

        [Test]
        public void LocalVariableReference()
        {
            var s1 = Symbol.CreateReference(EntityRef.Variable.Local.Create("c"), _position);

            var m = s1.HandleWith(SymbolMExprSerializer.Instance, _existing);
            dynamic s2 = SymbolMExprParser.Parse(_symbols, m);

            Assert.That(s2, Is.EqualTo(s1));
        }

        [Test]
        public void GlobalVariableReference()
        {
            var mn = new ModuleName("m", new Version(1, 2, 3, 4));
            var s1 = Symbol.CreateReference(EntityRef.Variable.Global.Create("c",mn), _position);

            var m = s1.HandleWith(SymbolMExprSerializer.Instance, _existing);
            dynamic s2 = SymbolMExprParser.Parse(_symbols, m);

            Assert.That(s2, Is.EqualTo(s1));
        }

        [Test]
        public void FunctionReference()
        {
            var mn = new ModuleName("m", new Version(1, 2, 3, 4));
            var s1 = Symbol.CreateReference(EntityRef.Function.Create("c",mn), _position);

            var m = s1.HandleWith(SymbolMExprSerializer.Instance, _existing);
            dynamic s2 = SymbolMExprParser.Parse(_symbols, m);

            Assert.That(s2, Is.EqualTo(s1));
        }

        [Test]
        public void ExpandWarningOnAliasOfDereferencedFunction()
        {
            var mn = new ModuleName("m", new Version(0, 0, 1, 2));
            const string mc = "T.ZOMG";
            var s0 = Symbol.CreateCall(EntityRef.Function.Create("f\\impl", mn), _position);
            var msg = Message.Create(MessageSeverity.Warning, "You shouldn't do this.", _otherPosition, mc);
            var s1 = Symbol.CreateExpand(Symbol.CreateMessage(msg, s0, _position));

            _symbols.Declare("f",s0);
            _existing.Add(s0,"f");

            var m = s1.HandleWith(SymbolMExprSerializer.Instance, _existing);
            dynamic s2 = SymbolMExprParser.Parse(_symbols, m);

            Assert.That(s2,Is.InstanceOf<ExpandSymbol>());
            Assert.That(s2.InnerSymbol,Is.InstanceOf<MessageSymbol>());
            Assert.That(s2,Is.EqualTo(s1));
            Assert.That(s2.InnerSymbol.InnerSymbol,Is.SameAs(s0));
        }

        private MExpr.MList _createCrossReference(string alias)
        {
            return new MExpr.MList(_position, SymbolMExprSerializer.CrossReferenceHead,
                                   new[] {new MExpr.MAtom(_position, alias)});
        }
    }
}
