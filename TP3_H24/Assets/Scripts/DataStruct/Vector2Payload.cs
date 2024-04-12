using Unity.Netcode;
using UnityEngine;

namespace DataStruct
{
    public class Vector2Payload : INetworkSerializable
    {

        public Vector2 Vector;
        public int Tick;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Tick);
            serializer.SerializeValue(ref Vector);
        }
    }
}