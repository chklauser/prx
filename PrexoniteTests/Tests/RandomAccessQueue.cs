using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Prexonite.Helper;
using RAQ = Prexonite.Helper.RandomAccessQueue<int>;
using Q = System.Collections.Generic.Queue<int>;

namespace Prx.Tests
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
            RAQ raq = new RandomAccessQueue<int>();

            Assert.AreEqual(0, raq.Count);

            int[] d = generateData();
            raq.Enqueue(d[0]);

            Assert.AreEqual(1, raq.Count);

            int cnt = 15;
            for(int i = cnt; i > 0; i-- )
                raq.Enqueue(d[i]);

            Assert.AreEqual(cnt+1, raq.Count);

            int cnt2 = 13;
            for (int i = 13; i > 0; i--)
                raq.Dequeue();

            Assert.AreEqual(cnt+1-cnt2, raq.Count);

            int cnt3 = 18;
            for (int i = cnt3; i > 0; i--)
                raq.Enqueue(d[i]);

            Assert.AreEqual(cnt + 1 - cnt2 + cnt3, raq.Count);
        }

        [Test]
        public void OrderSimple()
        {
            RAQ raq = new RandomAccessQueue<int>();

            Assert.AreEqual(0, raq.Count);

            int[] d = generateData(13);
            int i;

            for (i = 0; i < d.Length; i++)
                raq.Enqueue(d[i]);

            i = 0;
            Assert.AreEqual(d.Length,raq.Count);
            while (raq.Count > 0)
            {
                Assert.AreEqual(d[i++], raq.Dequeue());
            }
            Assert.AreEqual(0, raq.Count );
        }

        [Test]
        public void Order()
        {
            RAQ raq = new RandomAccessQueue<int>();

            Assert.AreEqual(0, raq.Count);

            int[] d = generateData(20);
            int i;

            for (i = 0; i < 15; i++)
                raq.Enqueue(d[i]);

            i = 0;
            while (raq.Count > 3)
                Assert.AreEqual(d[i++], raq.Dequeue());

            for(i = 0; i < 10; i++)
                raq.Enqueue(d[i]);

            i = 12;
            while(raq.Count > 10)
                Assert.AreEqual(d[i++], raq.Dequeue());
            i = 0;
            while(raq.Count > 0)
                Assert.AreEqual(d[i++],raq.Dequeue());
        }

        [Test]
        public void RandomAccess()
        {
            RAQ raq = new RandomAccessQueue<int>();

            Assert.AreEqual(0, raq.Count);

            int[] d = generateData(20);
            int i;

            for (i = 0; i < 15; i++)
                raq.Enqueue(d[i]);

            for (i = 0; i < 15; i++ )
                Assert.AreEqual(d[i], raq[i]);

            while (raq.Count > 3)
                raq.Dequeue();

            for (i = 0; i < 10; i++)
                raq.Enqueue(d[i]);

            for (i = 12; i < 15; i++ )
                Assert.AreEqual(d[i], raq[i-12]);

            while (raq.Count > 10)
                raq.Dequeue();

            for (i = 0; i < 10; i++)
                Assert.AreEqual(d[i], raq[i]);
        }

        [Test]
        public void SingleElement()
        {
            RAQ raq = new RandomAccessQueue<int>();

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
            RAQ raq = new RandomAccessQueue<int>();

            int[] d = generateData(30);
            //Fill the queue
            foreach (int data in d)
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
            int[] d = new int[k];
            for (int i = 0; i < k; i++)
                d[i] = i + 1;
            return d;
        }

    }
}