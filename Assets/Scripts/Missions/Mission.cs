using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Netcode;

public class Mission : ScriptableObject, INetworkSerializable
{
    // treat this as abstract - not abstract only for the serialization :)
    public virtual bool IsMissionDone()
    {
        return false;
    }

    public virtual void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {}
}
