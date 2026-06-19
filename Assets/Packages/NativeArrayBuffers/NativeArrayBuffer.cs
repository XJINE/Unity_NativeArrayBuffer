using System;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace NativeArrayBuffers
{
    public class NativeArrayBuffer<T> : IDisposable where T : struct
    {
        private readonly NativeArray<T>[] _buffer;
        private readonly bool[]           _available;

        public int       BufferCount { get;              }
        public int       Length      { get;              }
        public Allocator Allocator   { get;              }
        public bool      IsDisposed  { get; private set; }

        public int AvailableCount => _available.Count(isAvailable => isAvailable);

        public NativeArrayBuffer(int bufferCount, int length, Allocator allocator = Allocator.Persistent)
        {
            _buffer    = new NativeArray<T>[bufferCount];
            _available = new bool[bufferCount];

            BufferCount = bufferCount;
            Length      = length;
            Allocator   = allocator;

            for (var i = 0; i < bufferCount; i++)
            {
                _buffer   [i] = new NativeArray<T>(length, allocator);
                _available[i] = true;
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
                if (_parent == null)
                {
                    return;
                }

                _parent.Release(_index);
                _parent = null;
                _index  = -1;
            }
        }

        public bool TryRent(out BufferHandle handle)
        {
            if (IsDisposed)
            {
                handle = default;
                return false;
            }

            for (var i = 0; i < _available.Length; i++)
            {
                if (!_available[i])
                {
                    continue;
                }

                _available[i] = false;
                handle = new BufferHandle(this, i, _buffer[i]);
                return true;
            }

            handle = default;
            return false;
        }

        private void Release(int bufferIndex)
        {
            if (IsDisposed
            || bufferIndex < 0
            || _available.Length <= bufferIndex
            || _available[bufferIndex])
            {
                return;
            }

            _available[bufferIndex] = true;
        }

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            var leakedCount = 0;

            for (var i = 0; i < _buffer.Length; i++)
            {
                if (!_available[i])
                {
                    leakedCount++;
                }

                if (_buffer[i].IsCreated)
                {
                    _buffer[i].Dispose();
                }

                _available[i] = false;
            }

            if (0 < leakedCount)
            {
                Debug.LogWarning($"{nameof(NativeArrayBuffer<T>)} was disposed while {leakedCount} buffer(s) were still rented.");
            }

            IsDisposed = true;
        }
    }
}