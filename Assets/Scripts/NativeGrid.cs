using Unity.Collections;
using Unity.Mathematics;

namespace OFogo
{
    public struct NativeGrid<T> where T : unmanaged
    {
        NativeArray<T> nativeHashingGrid;
        int2 size;

        public int2 Size => size;
        public bool IsCreated => nativeHashingGrid.IsCreated;

        public NativeGrid(int2 size, Allocator allocator)
        {
            this.size = size;
            nativeHashingGrid = new NativeArray<T>(size.x * size.y, allocator);
        }

        public int ToIndex(int2 pos) => pos.y * size.x + pos.x;

        public T this[int2 pos]
        {
            get { return nativeHashingGrid[ToIndex(pos)]; }
            set { nativeHashingGrid[ToIndex(pos)] = value; }
        }
        public T this[int x, int y]
        {
            get { return nativeHashingGrid[ToIndex(new int2(x, y))]; }
            set { nativeHashingGrid[ToIndex(new int2(x, y))] = value; }
        }

        public void Dispose()
        {
            nativeHashingGrid.Dispose();
        }
        public bool InBound(int2 pos)
        {
            return pos.x >= 0 && pos.y >= 0 && pos.x < size.x && pos.y < size.y;
        }
    }
}