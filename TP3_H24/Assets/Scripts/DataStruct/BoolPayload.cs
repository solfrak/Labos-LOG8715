using Unity.Netcode;

namespace DataStruct
{
    public class BoolPayload : INetworkSerializable
    {
        public bool Value;
        public int Tick;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Value);
            serializer.SerializeValue(ref Tick);
        }
    }
}