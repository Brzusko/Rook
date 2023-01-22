using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using IT.Collections;
using UnityEngine;


namespace IT.Tests
{
    public class RingBufferTestRunner
    {
        [Test]
        public void CreateRingBufferTest()
        {
            RingBuffer<int> ringBuffer = new RingBuffer<int>(10);
            Assert.IsTrue(ringBuffer.Count == 0);
        }

        [Test]
        public void WriteTest()
        {
            RingBuffer<int> ringBuffer = new RingBuffer<int>(3);
            ringBuffer.Write(1);
            ringBuffer.Write(1);
            ringBuffer.Write(1);
            Assert.IsTrue(ringBuffer.Count == 3);
            Assert.Throws<OverflowException>(() => ringBuffer.Write(1));
            { int value = ringBuffer.Read(); }
            Assert.IsTrue(ringBuffer.Count == 2);
            { int value = ringBuffer.Read(); }
            { int value = ringBuffer.Read(); }
            Assert.IsTrue(ringBuffer.Count == 0);
            Debug.Log($"Tail: {ringBuffer.Tail.ToString()}, Head: {ringBuffer.Tail.ToString()}");
            ringBuffer.Write(1);
            ringBuffer.Write(1);
            Debug.Log($"Tail: {ringBuffer.Tail.ToString()}, Head: {ringBuffer.Tail.ToString()}");
        }
    }
}
