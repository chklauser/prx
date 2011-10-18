// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
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
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Prexonite;
using Prexonite.Compiler.Ast;

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
                Assert.IsTrue(placeholder.Index.HasValue,
                    "All placeholders should be assigned afterwards.");

            // ReSharper disable PossibleInvalidOperationException
            Assert.AreEqual(2, placeholders[0].Index.Value,
                "Placeholder at source position 0 is not mapped correctly");
            Assert.AreEqual(1, placeholders[1].Index.Value,
                "Placeholder at source position 1 is not mapped correctly");
            Assert.AreEqual(3, placeholders[2].Index.Value,
                "Placeholder at source position 2 is not mapped correctly");
            Assert.AreEqual(1, placeholders[3].Index.Value,
                "Placeholder at source position 3 is not mapped correctly");
            Assert.AreEqual(0, placeholders[4].Index.Value,
                "Placeholder at source position 4 is not mapped correctly");
            // ReSharper restore PossibleInvalidOperationException
        }

        [Test]
        public void DeterminePlaceholderIndicesEmptyTets()
        {
            AstPlaceholder.DeterminePlaceholderIndices(Enumerable.Empty<AstPlaceholder>());
        }

        [Test, ExpectedException(typeof (NullReferenceException))]
        public void DeterminePlaceholderIndicesRejectNullTets()
        {
            AstPlaceholder.DeterminePlaceholderIndices(Extensions.Singleton<AstPlaceholder>(null));
        }

        [Test]
        public void RemoveRedundant1()
        {
            const string file = "THE_FILE";
            const int line = 666;
            const int col = 555;
            //test case from MissingMapped
            var subject = new AstNull(file, line, col);
            var argv = new List<IAstExpression>
                {
                    subject,
                    _createPlaceholder(1),
                    _createPlaceholder(2)
                };
            var originalArgv = argv.ToList();

            _placeholderArgvProcessing(argv);

            Assert.AreEqual(3, argv.Count, "argc changed");
            for (var i = 0; i < argv.Count; i++)
                Assert.AreSame(originalArgv[i], argv[i]);
        }

        private static void _placeholderArgvProcessing(List<IAstExpression> argv)
        {
            Console.WriteLine("ARGV implicit:");
            foreach (var expr in argv)
                Console.WriteLine("\t{0}", expr);

            AstPlaceholder.DeterminePlaceholderIndices(argv.MapMaybe(x => x as AstPlaceholder));

            Console.WriteLine("ARGV explicit:");
            foreach (var expr in argv)
                Console.WriteLine("\t{0}", expr);

            AstPartiallyApplicable.RemoveRedundantPlaceholders(argv);

            Console.WriteLine("ARGV minimal:");
            foreach (var expr in argv)
                Console.WriteLine("\t{0}", expr);
        }

        [Test]
        public void RemoveRedundant2()
        {
            const string file = "THE_FILE";
            const int line = 666;
            const int col = 555;
            //test case from MissingMapped
            var subject = new AstNull(file, line, col);
            var argv = new List<IAstExpression>
                {
                    subject,
                    _createPlaceholder(2),
                    _createPlaceholder(),
                    _createPlaceholder(1)
                };
            var originalArgv = argv.GetRange(0, 2);

            _placeholderArgvProcessing(argv);

            Assert.AreEqual(originalArgv.Count, argv.Count, "argc not correct");
            for (var i = 0; i < originalArgv.Count; i++)
                Assert.AreSame(originalArgv[i], argv[i]);
        }
    }
}