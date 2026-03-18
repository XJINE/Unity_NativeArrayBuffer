using System;
using System.Linq;
using Unity.Collections;

namespace NativeArrayBuffers 
{
    public class NativeArrayBuffer<T> : IDisposable where T : struct
    {
        private readonly NativeArray<T>[] _buffer;
        private readonly bool[]           _released;

        public int       BufferCount { get;              }
        public int       Length      { get;              }
        public Allocator Allocator   { get;              }
        public bool      IsDisposed  { get; private set; }

        public int ReleasedCount => _released.Count(isReleased => isReleased);

        public NativeArrayBuffer(int bufferCount, int length, Allocator allocator = Allocator.Persistent)
        {
            _buffer   = new NativeArray<T>[bufferCount];
            _released = new bool[bufferCount];

            BufferCount = bufferCount;
            Length      = length;
            Allocator   = allocator;

            for (var i = 0; i < bufferCount; i++)
            {
                _buffer  [i] = new NativeArray<T>(length, allocator);
                _released[i] = true;
            }
        }

        public struct BufferHandle : IDisposable
        {
            private NativeArrayBuffer<T> _parent;
            private int                  _index;

            public NativeArray<T> Array;

            internal BufferHandle(NativeArrayBuffer<T> parent, int index, NativeArray<T> array)
            {
                _parent = parent;
                _index  = index;
                Array   = array;
            }

            public void Dispose()
            {
                _parent.Release(_index);
                _parent = null; 
                _index = -1;
            }
        }

        public bool TryRent(out BufferHandle handle)
        {
            if (IsDisposed)
            {
                handle = default;
                return false;
            }

            for (var i = 0; i < _released.Length; i++)
            {
                if (!_released[i])
                {
                    continue;
                }

                _released[i] = false; 
                handle = new BufferHandle(this, i, _buffer[i]);
                return true;
            }

            handle = default;
            return false;
        }

        private void Release(int bufferIndex)
        {
            _released[bufferIndex] = true;
        }

        public void Dispose()
        {
            // CAUTION:
            // Dispose method does not dispose the buffers that are not released yet.
            // It is the caller's responsibility to release all buffers before calling Dispose.

            if (IsDisposed)
            {
                return;
            }

            for (var i = 0; i < _buffer?.Length; i++)
            {
                if (_buffer[i].IsCreated && _released[i])
                {
                    _buffer[i].Dispose();
                }
            }

            IsDisposed = ReleasedCount == _released.Length;
        }
    }
}