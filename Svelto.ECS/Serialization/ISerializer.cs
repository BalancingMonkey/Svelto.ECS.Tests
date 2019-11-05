using System;

namespace Svelto.ECS.Serialization
{
    public interface ISerializer<T>
        where T : unmanaged, IEntityStruct
    {
        bool Serialize(in T value, ISerializationData serializationData);
        bool Deserialize(ref T value, ISerializationData serializationData);

        uint size { get; }
    }

    public static class SerializerExt
    {
        public static bool SerializeSafe<T>(this ISerializer<T> serializer, in T value, ISerializationData serializationData)
            where T : unmanaged, IEntityStruct
        {
            uint posBefore = serializationData.dataPos;
            bool res = serializer.Serialize(value, serializationData);
#if DEBUG
            // size == 0 is a special case when we don't know the size in advance
            if (serializer.size != 0 && serializationData.dataPos != posBefore + serializer.size)
            {
                throw new IndexOutOfRangeException(
                    $"Size mismatch when serializing {typeof(T).FullName} using {serializer.GetType().FullName}, " +
                    $"expected offset {posBefore + serializer.size}, got {serializationData.dataPos}");
            }
#endif
            return res;
        }

        public static bool DeserializeSafe<T>(this ISerializer<T> serializer, ref T value, ISerializationData serializationData)
            where T : unmanaged, IEntityStruct
        {
            uint posBefore = serializationData.dataPos;
            bool res = serializer.Deserialize(ref value, serializationData);
#if DEBUG
            if (serializer.size != 0 && serializationData.dataPos != posBefore + serializer.size)
            {
                throw new IndexOutOfRangeException(
                    $"Size mismatch when deserializing {typeof(T).FullName} using {serializer.GetType().FullName}, " +
                    $"expected offset {posBefore + serializer.size}, got {serializationData.dataPos}");
            }
#endif
            return res;
        }
    }
}
