using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Prexonite.Compiler.Ast;
using Prexonite;

namespace PrexoniteTests.Tests
{
    [TestFixture]
    public class AstTests
    {
        private static AstPlaceholder _createPlaceholder(int? index = null)
        {
            return new AstPlaceholder("-file", -1, -2) {Index = index};
        }

        [Test]
        public void DeterminePlaceholderIndicesNormalTets()
        {
            //(?,?2,?,?2,?1)
            var placeholders = new List<AstPlaceholder>
            {
                _createPlaceholder(),
                _createPlaceholder(1),
                _createPlaceholder(),
                _createPlaceholder(1),
                _createPlaceholder(0)
            };

            var copy = placeholders.ToList();
            Assert.AreNotSame(copy, placeholders);

            AstPlaceholder.DeterminePlaceholderIndices(placeholders);

            //First assert that the list itself has not been altered
            Assert.AreEqual(placeholders.Count, copy.Count);
            for (var i = 0; i < placeholders.Count; i++)
                Assert.AreSame(copy[i], placeholders[i], "List itself must not be altered.");

            //Expexted mapping (0-based):
            //  ?2, ?1, ?3, ?1, ?0
            foreach (var placeholder in placeholders)
                Assert.IsTrue(placeholder.Index.HasValue,"All placeholders should be assigned afterwards.");

            // ReSharper disable PossibleInvalidOperationException
            Assert.AreEqual(2, placeholders[0].Index.Value, "Placeholder at source position 0 is not mapped correctly");
            Assert.AreEqual(1, placeholders[1].Index.Value, "Placeholder at source position 1 is not mapped correctly");
            Assert.AreEqual(3, placeholders[2].Index.Value, "Placeholder at source position 2 is not mapped correctly");
            Assert.AreEqual(1, placeholders[3].Index.Value, "Placeholder at source position 3 is not mapped correctly");
            Assert.AreEqual(0, placeholders[4].Index.Value, "Placeholder at source position 4 is not mapped correctly");
            // ReSharper restore PossibleInvalidOperationException
        }

        [Test]
        public void DeterminePlaceholderIndicesEmptyTets()
        {
            AstPlaceholder.DeterminePlaceholderIndices(Enumerable.Empty<AstPlaceholder>());
        }

        [Test,ExpectedException(typeof(NullReferenceException))]
        public void DeterminePlaceholderIndicesRejectNullTets()
        {
            AstPlaceholder.DeterminePlaceholderIndices(Extensions.Singleton<AstPlaceholder>(null));
        }

    }
}
