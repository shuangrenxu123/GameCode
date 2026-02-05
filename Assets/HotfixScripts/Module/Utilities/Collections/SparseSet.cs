using System;
using System.Collections;
using System.Collections.Generic;

namespace Utilities.Collections
{
    public class SparseSet<T> : IEnumerable<T>
    {
        int[] sparse;
        T[] dense;
        int[] denseIds;

        ushort startIndex;

        public SparseSet(int sparesCount = 512, int cap = 8)
        {
            if (sparesCount <= 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(sparesCount), "sparesCount must be greater than 0");
            }
            if (cap <= 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(cap), "cap must be greater than 0");
            }
            sparse = new int[sparesCount];

            dense = new T[cap + 1];
            denseIds = new int[cap + 1];
            startIndex = 1;
        }

        public void Add(int id, T value)
        {
            if (id < 0 || id >= sparse.Length)
            {
                throw new System.ArgumentOutOfRangeException(nameof(id), "id is out of sparse range");
            }

            if (sparse[id] != 0)
            {
                dense[sparse[id]] = value;
                return;
            }

            sparse[id] = startIndex;
            if (dense.Length == startIndex)
            {
                Array.Resize(ref dense, startIndex << 1);
                Array.Resize(ref denseIds, startIndex << 1);
            }
            dense[startIndex++] = value;
            denseIds[startIndex - 1] = id;
        }

        public T Get(int id)
        {
            if (id < 0 || id >= sparse.Length)
            {
                throw new System.ArgumentOutOfRangeException(nameof(id), "id is out of sparse range");
            }
            if (sparse[id] == 0)
            {
                throw new System.InvalidOperationException("id does not exist, cannot get");
            }
            return dense[sparse[id]];
        }

        public void Remove(int id)
        {
            if (id < 0 || id >= sparse.Length)
            {
                throw new System.ArgumentOutOfRangeException(nameof(id), "id is out of sparse range");
            }
            if (sparse[id] == 0)
            {
                throw new System.InvalidOperationException("id does not exist, cannot remove");
            }

            var index = sparse[id];
            sparse[id] = 0;
            var lastIndex = startIndex - 1;
            if (index != lastIndex)
            {
                dense[index] = dense[lastIndex];
                var movedId = denseIds[lastIndex];
                denseIds[index] = movedId;
                sparse[movedId] = index;
            }
            startIndex--;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public struct Enumerator : IEnumerator<T>
        {
            readonly SparseSet<T> set;
            int index;
            T current;

            public Enumerator(SparseSet<T> set)
            {
                this.set = set;
                index = 0;
                current = default;
            }

            public T Current => current;

            object IEnumerator.Current => current;

            public bool MoveNext()
            {
                if (index + 1 >= set.startIndex)
                {
                    return false;
                }

                index++;
                current = set.dense[index];
                return true;
            }

            public void Reset()
            {
                index = 0;
                current = default;
            }

            public void Dispose()
            {
            }
        }
    }
}
