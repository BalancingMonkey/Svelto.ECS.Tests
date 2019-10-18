using Svelto.DataStructures;

namespace Svelto.ECS
{
    public interface ISerializationData
    {
        uint dataPos { get; set; }
        FasterList<byte> data { get; }

        void ResetToReuse();
        void BeginNextEntityStruct();
    }
}