using System;
using System.Runtime.CompilerServices;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public readonly struct EGIDMapper<T> where T : struct, IEntityStruct
    {
        internal readonly ITypeSafeDictionary<T> map;
        public uint Length => map.Count;
        public readonly ExclusiveGroupStruct groupID;

        public EGIDMapper(ExclusiveGroupStruct groupStructId, ITypeSafeDictionary<T> dic):this()
        {
            groupID = groupStructId;
            map = dic;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Entity(uint entityID)
        {
#if DEBUG && !PROFILER
                if (map.TryFindIndex(entityID, out var findIndex) == false)
                    throw new Exception("Entity not found in this group ".FastConcat(typeof(T).ToString()));
#else
                map.TryFindIndex(entityID, out var findIndex);
#endif
                return ref map.unsafeValues[(int) findIndex];
        }
        
        public bool TryGetEntity(uint entityID, out T value)
        {
            if (map.TryFindIndex(entityID, out var index))
            {
                value = map.GetDirectValue(index);
                return true;
            }

            value = default;
            return false;
        }
        
        public T[] GetArrayAndEntityIndex(uint entityID, out uint index)
        {
            if (map.TryFindIndex(entityID, out index))
            {
                return map.unsafeValues;
            }

            throw new ECSException("Entity not found");
        }
        
        public void Dispose()
        { }
    }
}

