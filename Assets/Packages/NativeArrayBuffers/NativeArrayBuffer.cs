using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace NativeArrayBuffers {
public class NativeArrayBuffer<T> : IDisposable where T : struct
{
    private readonly NativeArray<T>[] _buffer;
    private readonly bool          [] _released;

    public NativeArray<T> this[int index] => _buffer[index];

    public int       BufferCount { get;              }
    public int       Length      { get;              }
    public Allocator Allocator   { get;              }
    public bool      IsDisposed  { get; private set; }

    public int ReleasedCount => _released.Count(isReleased => isReleased);

    public NativeArrayBuffer(int bufferCount, int length, Allocator allocator = Allocator.Persistent)
    {
        _buffer   = new NativeArray<T>[bufferCount];
        _released = new bool          [bufferCount];

        BufferCount = bufferCount;
        Length      = length;
        Allocator   = allocator;

        for (var i = 0; i < bufferCount; i++)
        {
            _buffer  [i] = new NativeArray<T>(length, allocator);
            _released[i] = true;
        }
    }

    private bool GetReleasedIndex(out int bufferIndex)
    {
        if (IsDisposed)
        {
            bufferIndex = -1;
            return false;
        }

        for (var i = 0; i < _released.Length; i++)
        {
            if (!_released[i])
            {
                continue;
            }

            _released[i] = false;
            bufferIndex  = i;

            return true;
        }

        bufferIndex = -1;

        return false;
    }

    public ref NativeArray<T> GetRef(out int bufferIndex)
    {
        // NOTE:
        // It cannot use ternary operator when the return type is ref.

        if (GetReleasedIndex(out bufferIndex))
        {
            return ref _buffer[bufferIndex];
        }
        else
        {
            return ref Unsafe.NullRef<NativeArray<T>>();
        }
    }

    public NativeArray<T> Get(out int bufferIndex)
    {
        return GetReleasedIndex(out bufferIndex) ? _buffer[bufferIndex] : Unsafe.NullRef<NativeArray<T>>();
    }

    public void Release(int bufferIndex)
    {
        if (bufferIndex < 0 || _buffer.Length <= bufferIndex)
        {
            throw new ArgumentOutOfRangeException(nameof(bufferIndex));
        }

        if (_released[bufferIndex])
        {
            throw new InvalidOperationException($"Buffer index { bufferIndex } is already released.");
        }

        _released[bufferIndex] = true;
    }

    public void Dispose()
    {
        for (var i = 0; i < _buffer?.Length; i++)
        {
            if (_buffer[i].IsCreated && _released[i])
            {
                _buffer[i].Dispose();
            }
        }

        IsDisposed = true;
    }
}}