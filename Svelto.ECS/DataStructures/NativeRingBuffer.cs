using System;
using System.Runtime.CompilerServices;
using Svelto.Common;

namespace Svelto.ECS.DataStructures
{
    /// <summary>
    /// Burst friendly RingBuffer on steroid:
    /// it can: Enqueue/Dequeue, it wraps if there is enough space after dequeuing
    /// It resizes if there isn't enough space left.
    /// It's a "bag", you can queue and dequeue any T. Just be sure that you dequeue what you queue! No check on type
    /// is done.
    /// You can reserve a position in the queue to update it later.
    /// The datastructure is a struct and it's "copyable" 
    /// </summary>
    public struct NativeRingBuffer : IDisposable
    {
#if ENABLE_BURST_AOT        
        [global::Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        unsafe UnsafeBlob* _queue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEmpty()
        {
            unsafe
            {
                if (_queue == null || _queue->ptr == null)
                    return true;
            }

            return count == 0;
        }

        public uint count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                unsafe
                {
#if DEBUG && !PROFILE_SVELTO
                    if (_queue == null)
                        throw new Exception("SimpleNativeArray: null-access");
#endif
                    
                    return _queue->size;
                }
            }
        }

        public uint capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                unsafe
                {
#if DEBUG && !PROFILE_SVELTO
                    if (_queue == null)
                        throw new Exception("SimpleNativeArray: null-access");
#endif

                    return _queue->capacity;
                }
            }
        }

        public NativeRingBuffer(Common.Allocator allocator)
        {
            unsafe 
            {
                var sizeOf = MemoryUtilities.SizeOf<UnsafeBlob>();
                UnsafeBlob* listData = (UnsafeBlob*) MemoryUtilities.Alloc((uint) sizeOf
                                                             , (uint) MemoryUtilities.AlignOf<UnsafeBlob>(), allocator);

                //clear to nullify the pointers
                MemoryUtilities.MemClear((IntPtr) listData, (uint) sizeOf);
                listData->allocator = allocator;
#if DEBUG && !PROFILE_SVELTO                
                listData->id = 0xDEADBEEF;
#endif
                _queue = listData;
            }
        }
        
        public NativeRingBuffer(uint bufferID, Common.Allocator allocator)
        {
            unsafe 
            {
                var sizeOf = MemoryUtilities.SizeOf<UnsafeBlob>();
                UnsafeBlob* listData = (UnsafeBlob*) MemoryUtilities.Alloc((uint) sizeOf
                                                                         , (uint) MemoryUtilities.AlignOf<UnsafeBlob>(), allocator);

                //clear to nullify the pointers
                MemoryUtilities.MemClear((IntPtr) listData, (uint) sizeOf);
                listData->allocator = allocator;
#if DEBUG && !PROFILE_SVELTO                
                listData->id = bufferID;
#endif
                _queue = listData;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Dispose()
        {
            if (_queue != null)
            {
                _queue->Dispose();
                _queue = null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T ReserveEnqueue<T>(out UnsafeArrayIndex index) where T:unmanaged
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_queue == null)
                    throw new Exception("SimpleNativeArray: null-access");
#endif
                int sizeOf = MemoryUtilities.SizeOf<T>();
                if (_queue->space - sizeOf < 0)
                    _queue->Realloc((uint) MemoryUtilities.AlignOf<int>(), (uint) ((_queue->capacity + sizeOf) * 1.5f));
                
                return ref _queue->Reserve<T>(out index);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue<T>(in T item) where T : struct
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_queue == null)
                    throw new Exception("SimpleNativeArray: null-access");
#endif
                int sizeOf = MemoryUtilities.SizeOf<T>();
                if (_queue->space - sizeOf < 0)
                    _queue->Realloc((uint) MemoryUtilities.AlignOf<int>(), (uint) ((_queue->capacity + sizeOf) * 1.5f));

                _queue->Write(item);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_queue == null)
                    throw new Exception("SimpleNativeArray: null-access");
#endif
                _queue->Clear();
            }
        }

        public T Dequeue<T>() where T : struct
        {
            unsafe 
            {
                return _queue->Read<T>();
            }
        }

        public ref T AccessReserved<T>(UnsafeArrayIndex reserverIndex) where T : unmanaged
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_queue == null)
                    throw new Exception("SimpleNativeArray: null-access");
#endif
                return ref _queue->AccessReserved<T>(reserverIndex);
            }
        }
    }
}
