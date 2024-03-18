using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public struct NativeCircularBuffer<T> : IDisposable where T : unmanaged
{
    UnsafeList<T> array;

    int headIndex;
    int tailIndex;
    int maxLength;
    bool isFilled;
    public int Length => isFilled ? maxLength : tailIndex + 1;

    public unsafe NativeCircularBuffer(int length, Allocator allocator)
    {
        array = new UnsafeList<T>(length, allocator);
        array.Length = length;

        headIndex = 0;
        tailIndex = 0;
        isFilled = false;
        maxLength = length;
    }

    public void Add(T element)
    {
        array[tailIndex] = element;

        if (isFilled)
        {
            headIndex = (headIndex + 1) % maxLength;
            tailIndex = (tailIndex + 1) % maxLength;
        }
        else
        {
            tailIndex++;
            if (tailIndex == maxLength)
            {
                tailIndex = 0;
                headIndex = 1;
                isFilled = true;
            }
        }
    }

    public void Clear()
    {
        headIndex = 0;
        tailIndex = 0;
        isFilled = false;
    }

    private int ConvertIndex(int i)
    {
        int index = isFilled ? (int)Mathf.Repeat(headIndex + i - 1, maxLength) : i;
        return index;
    }

    //todo add out of buffer error?
    public T this[int i]
    {
        get { return array[ConvertIndex(i)]; }
        set { array[ConvertIndex(i)] = value; }
    }

    public void Dispose()
    {
        array.Dispose();
    }
}