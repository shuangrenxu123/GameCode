using System;
using System.Collections;
using System.Collections.Generic;

namespace Utilities.Collections
{
    public class Deque<T> : IEnumerable<T>
    {
        private static readonly T[] emptyBuffer = new T[0];

        private T[] buffer = emptyBuffer;

        private int front;

        private int rear;

        public int count { get; private set; }

        public T this[int index]
        {
            get
            {
                return buffer[CalcIndex(index)];
            }
            set
            {
                buffer[CalcIndex(index)] = value;
            }
        }

        private int CalcIndex(int index)
        {
            if (index >= count || index < 0)
            {
                throw new IndexOutOfRangeException("Accessing index " + index + " on deque with " + count + " elements.");
            }

            return (index + front) % buffer.Length;
        }

        public void Reserve(int capacity)
        {
            if (buffer.Length < capacity)
            {
                T[] array = new T[Math.Max(buffer.Length * 2, capacity)];
                for (int i = 0; i < count; i++)
                {
                    array[i] = this[i];
                }

                buffer = array;
                front = 0;
                rear = count;
            }
        }

        public void AddBack(T x)
        {
            Reserve(count + 1);
            buffer[rear] = x;
            rear = (rear + 1) % buffer.Length;
            count++;
        }

        public void AddFront(T x)
        {
            Reserve(count + 1);
            front--;
            if (front < 0)
            {
                front += buffer.Length;
            }

            buffer[front] = x;
            count++;
        }

        public T RemoveBack()
        {
            if (count <= 0)
            {
                throw new IndexOutOfRangeException();
            }

            rear--;
            if (rear < 0)
            {
                rear += buffer.Length;
            }

            T result = buffer[rear];
            buffer[rear] = default(T);
            count--;
            return result;
        }

        public T RemoveFront()
        {
            if (count <= 0)
            {
                throw new IndexOutOfRangeException();
            }

            T result = buffer[front];
            buffer[front] = default(T);
            front = (front + 1) % buffer.Length;
            count--;
            return result;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
