using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;

namespace OFogo
{
    public struct NativeGrid<T> where T : unmanaged
    {
        NativeArray<T> nativeGrid;
        int2 size;

        public int2 Size => size;
        public int TotalLength => size.x * size.y;
        public bool IsCreated => nativeGrid.IsCreated;

        public NativeGrid(int2 size, Allocator allocator)
        {
            this.size = size;
            nativeGrid = new NativeArray<T>(size.x * size.y, allocator);
        }

        public int PosToIndex(int2 pos) => pos.y * size.x + pos.x;
        public int2 IndexToPos(int index) => new int2(index % size.x, index / size.x);

        public T this[int2 pos]
        {
            get { return nativeGrid[PosToIndex(pos)]; }
            set { nativeGrid[PosToIndex(pos)] = value; }
        }
        public T this[int x, int y]
        {
            get { return nativeGrid[PosToIndex(new int2(x, y))]; }
            set { nativeGrid[PosToIndex(new int2(x, y))] = value; }
        }

        public T this[int i]
        {
            get { return nativeGrid[i]; }
            set { nativeGrid[i] = value; }
        }


        public void Dispose()
        {
            if(nativeGrid.IsCreated)
                nativeGrid.Dispose();
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
    }
}