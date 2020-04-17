using System;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Allocator = Svelto.Common.Allocator;

namespace Svelto.ECS.DataStructures.Unity
{
    /// <summary>
    /// A collection of <see cref="NativeRingBuffer"/> intended to allow one buffer per thread.
    /// from: https://github.com/jeffvella/UnityEcsEvents/blob/develop/Runtime/MultiAppendBuffer.cs
    /// </summary>
    unsafe struct AtomicRingBuffers:IDisposable
    {
        public const int DefaultThreadIndex = -1;
        public const int MinThreadIndex = DefaultThreadIndex;

#if UNITY_ECS        
        [global::Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        NativeRingBuffer* _data;
        public readonly Allocator Allocator;
        readonly uint _threadsCount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsInvalidThreadIndex(int index) => index < MinThreadIndex || index > _threadsCount;

        public AtomicRingBuffers(Common.Allocator allocator, uint threadsCount)
        {
            Allocator = allocator;
            _threadsCount = threadsCount;

            var bufferSize = MemoryUtilities.SizeOf<NativeRingBuffer>();
            var bufferCount = _threadsCount;
            var allocationSize = bufferSize * bufferCount;

            var ptr = (byte*)MemoryUtilities.Alloc((uint) allocationSize, (uint) MemoryUtilities.AlignOf<int>(), allocator);
            MemoryUtilities.MemClear((IntPtr) ptr, (uint) allocationSize);

            for (int i = 0; i < bufferCount; i++)
            {
                var bufferPtr = (NativeRingBuffer*)(ptr + bufferSize * i);
                var buffer = new NativeRingBuffer((uint) i, allocator);
                MemoryUtilities.CopyStructureToPtr(ref buffer, (IntPtr) bufferPtr);
            }

            _data = (NativeRingBuffer*)ptr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref NativeRingBuffer GetBuffer(int index)
        {
            return ref MemoryUtilities.ArrayElementAsRef<NativeRingBuffer>((IntPtr) _data, index);
        }

        public uint count => _threadsCount;

        public void Dispose()
        {
            for (int i = 0; i < _threadsCount; i++)
            {
                GetBuffer(i).Dispose();
            }
            MemoryUtilities.Free((IntPtr) _data, Allocator);
        }

        public void Clear()
        {
            for (int i = 0; i < _threadsCount; i++)
            {
                GetBuffer(i).Clear();
            }
        }
    }
}
