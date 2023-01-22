using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Collections
{
    public class RingBuffer<T>
    {
        private T[] _container;
        private int _head;
        private int _tail;
        private int _capacity;
        private int _count = 0;

        public int Count => _count;
        public int Head => _head;
        public int Tail => _tail;
        
        public RingBuffer(int capacity)
        {
            _container = new T[capacity];
            _capacity = capacity;
            _head = 0;
            _tail = 0;
        }

        public void Write(T dataToWrite)
        {
            int tempHead = (_head + 1) % _capacity;

            if (tempHead == _tail)
            {
                throw new OverflowException("Ring buffer is full");
            }

            _count++;
            
            _container[_head] = dataToWrite;
            _head = tempHead;
        }
        
        public T Read()
        {
            int tempTail = (_tail + 1) % _capacity;

            if (tempTail == _head)
            {
                throw new OverflowException("There is no data to read");
            }

            _count--;
            T value = _container[_tail];
            _tail = tempTail;
            return value;
        }
        
        public T Peek()
        {
            if (_head == _tail)
            {
                throw new OverflowException("There is no data to read");
            }

            return _container[_head];
        }
    }
}
