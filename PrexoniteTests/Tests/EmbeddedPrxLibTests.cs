using NUnit.Framework;
using Prexonite.Compiler.Build;

namespace PrexoniteTests.Tests
{
    /// <summary>
    /// These tests just verify that the prxlib/* embedded resources are present.
    /// </summary>
    [TestFixture]
    public class EmbeddedPrxLibTests
    {
        [Test]
        public void LegacySymbols()
        {
            _checkEmbeddedResource("prxlib.legacy_symbols.pxs");
        }

        
        [Test]
        public void PrxCore()
        {
            _checkEmbeddedResource("prxlib.prx.core.pxs");
        }
        
        [Test]
        public void Sh()
        {
            _checkEmbeddedResource("prxlib.sh.pxs");
        }
        
        [Test]
        public void PrxPrim()
        {
            _checkEmbeddedResource("prxlib.prx.prim.pxs");
        }
        
        [Test]
        public void Sys()
        {
            _checkEmbeddedResource("prxlib.sys.pxs");
        }

        private static void _checkEmbeddedResource(string name)
        {
            Assert.True(Source.FromEmbeddedResource(name).TryOpen(out var reader), $"Cannot open {name}");
            var contents = reader.ReadToEnd();
            Assert.That(contents, Is.Not.Null);
            Assert.That(contents.Length, Is.GreaterThan(100));
        }
    }
}