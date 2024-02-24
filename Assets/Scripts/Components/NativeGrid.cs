using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;

namespace OFogo
{
    public struct NativeGrid<T> where T : unmanaged
    {
        NativeArray<T> nativeHashingGrid;
        int2 size;

        public int2 Size => size;
        public int TotalLength => size.x * size.y;
        public bool IsCreated => nativeHashingGrid.IsCreated;

        public NativeGrid(int2 size, Allocator allocator)
        {
            this.size = size;
            nativeHashingGrid = new NativeArray<T>(size.x * size.y, allocator);
        }

        public int PosToIndex(int2 pos) => pos.y * size.x + pos.x;
        public int2 IndexToPos(int index) => new int2(index % size.x, index / size.x);

        public T this[int2 pos]
        {
            get { return nativeHashingGrid[PosToIndex(pos)]; }
            set { nativeHashingGrid[PosToIndex(pos)] = value; }
        }
        public T this[int x, int y]
        {
            get { return nativeHashingGrid[PosToIndex(new int2(x, y))]; }
            set { nativeHashingGrid[PosToIndex(new int2(x, y))] = value; }
        }

        public T this[int i]
        {
            get { return nativeHashingGrid[i]; }
            set { nativeHashingGrid[i] = value; }
        }


        public void Dispose()
        {
            nativeHashingGrid.Dispose();
        }
        public bool InBound(int2 pos)
        {
            return pos.x >= 0 && pos.y >= 0 && pos.x < size.x && pos.y < size.y;
        }

        public IEnumerable<int2> GetIterator(bool xFirst = true)
        {
            int2 sizeAdjusted = xFirst ? size : size.yx;
            for (int x = 0; x < sizeAdjusted.x; x++)
            {
                for (int y = 0; y < sizeAdjusted.y; y++)
                {
                    yield return xFirst ? new int2(x, y) : new int2(y, x);
                }
            }       
        }

        //public Int2Enumerator GetEnumerator() => new Int2Enumerator(this);
        //public IEnumerator<int2> GetEnumerator() => GetEnumerator();
        //IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        //public struct Int2Enumerator : IEnumerator<int2>
        //{
        //    public Int2Enumerator(NativeGrid<T> test)
        //    {

        //    }

        //    public int2 Current => throw new System.NotImplementedException();

        //    object IEnumerator.Current => throw new System.NotImplementedException();

        //    public void Dispose()
        //    {
        //        throw new System.NotImplementedException();
        //    }

        //    public bool MoveNext()
        //    {
        //        throw new System.NotImplementedException();
        //    }

        //    public void Reset()
        //    {
        //        throw new System.NotImplementedException();
        //    }
        //}
    }
}