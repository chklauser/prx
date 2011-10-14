// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using NUnit.Framework;
using Prexonite.Helper;

namespace PrexoniteTests.Tests
{
    [TestFixture]
    public class RandomAccessQueue
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Count()
        {
            var raq = new RandomAccessQueue<int>();

            Assert.AreEqual(0, raq.Count);

            var d = generateData();
            raq.Enqueue(d[0]);

            Assert.AreEqual(1, raq.Count);

            var cnt = 15;
            for (var i = cnt; i > 0; i--)
                raq.Enqueue(d[i]);

            Assert.AreEqual(cnt + 1, raq.Count);

            var cnt2 = 13;
            for (var i = 13; i > 0; i--)
                raq.Dequeue();

            Assert.AreEqual(cnt + 1 - cnt2, raq.Count);

            var cnt3 = 18;
            for (var i = cnt3; i > 0; i--)
                raq.Enqueue(d[i]);

            Assert.AreEqual(cnt + 1 - cnt2 + cnt3, raq.Count);
        }

        [Test]
        public void OrderSimple()
        {
            var raq = new RandomAccessQueue<int>();

            Assert.AreEqual(0, raq.Count);

            var d = generateData(13);
            int i;

            for (i = 0; i < d.Length; i++)
                raq.Enqueue(d[i]);

            i = 0;
            Assert.AreEqual(d.Length, raq.Count);
            while (raq.Count > 0)
            {
                Assert.AreEqual(d[i++], raq.Dequeue());
            }
            Assert.AreEqual(0, raq.Count);
        }

        [Test]
        public void Order()
        {
            var raq = new RandomAccessQueue<int>();

            Assert.AreEqual(0, raq.Count);

            var d = generateData(20);
            int i;

            for (i = 0; i < 15; i++)
                raq.Enqueue(d[i]);

            i = 0;
            while (raq.Count > 3)
                Assert.AreEqual(d[i++], raq.Dequeue());

            for (i = 0; i < 10; i++)
                raq.Enqueue(d[i]);

            i = 12;
            while (raq.Count > 10)
                Assert.AreEqual(d[i++], raq.Dequeue());
            i = 0;
            while (raq.Count > 0)
                Assert.AreEqual(d[i++], raq.Dequeue());
        }

        [Test]
        public void RandomAccess()
        {
            var raq = new RandomAccessQueue<int>();

            Assert.AreEqual(0, raq.Count);

            var d = generateData(20);
            int i;

            for (i = 0; i < 15; i++)
                raq.Enqueue(d[i]);

            for (i = 0; i < 15; i++)
                Assert.AreEqual(d[i], raq[i]);

            while (raq.Count > 3)
                raq.Dequeue();

            for (i = 0; i < 10; i++)
                raq.Enqueue(d[i]);

            for (i = 12; i < 15; i++)
                Assert.AreEqual(d[i], raq[i - 12]);

            while (raq.Count > 10)
                raq.Dequeue();

            for (i = 0; i < 10; i++)
                Assert.AreEqual(d[i], raq[i]);
        }

        [Test]
        public void SingleElement()
        {
            var raq = new RandomAccessQueue<int>();

            Assert.AreEqual(0, raq.Count);

            raq.Enqueue(1);
            Assert.AreEqual(1, raq.Count);
            Assert.AreEqual(1, raq[0]);
            Assert.AreEqual(1, raq.Dequeue());

            raq.Enqueue(2);
            Assert.AreEqual(1, raq.Count);
            Assert.AreEqual(2, raq[0]);
            Assert.AreEqual(2, raq.Dequeue());
        }

        [Test]
        public void Used()
        {
            var raq = new RandomAccessQueue<int>();

            var d = generateData(30);
            //Fill the queue
            foreach (var data in d)
            {
                raq.Enqueue(data);
            }

            //Empty it
            while (raq.Count > 0)
                raq.Dequeue();

            //And then test it's behaviour.
            Assert.AreEqual(0, raq.Count);

            d = generateData(20);
            int i;

            for (i = 0; i < 15; i++)
                raq.Enqueue(d[i]);

            for (i = 0; i < 15; i++)
                Assert.AreEqual(d[i], raq[i]);

            while (raq.Count > 3)
                raq.Dequeue();

            for (i = 0; i < 10; i++)
                raq.Enqueue(d[i]);

            for (i = 12; i < 15; i++)
                Assert.AreEqual(d[i], raq[i - 12]);

            while (raq.Count > 10)
                raq.Dequeue();

            for (i = 0; i < 10; i++)
                Assert.AreEqual(d[i], raq[i]);
        }

        private static int[] generateData()
        {
            return generateData(20);
        }

        private static int[] generateData(int k)
        {
            var d = new int[k];
            for (var i = 0; i < k; i++)
                d[i] = i + 1;
            return d;
        }
    }
}