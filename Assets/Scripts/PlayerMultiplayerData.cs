using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public struct PlayerMultiplayerData : IEquatable<PlayerMultiplayerData>, INetworkSerializable
{
    public ulong clientID;
    public bool isServer;
    public int modelIndex;
    public FixedString64Bytes playerID;
    public FixedString64Bytes playerName;

    public bool Equals(PlayerMultiplayerData other)
    {
        return this.clientID == other.clientID && this.modelIndex == other.modelIndex && playerID == other.playerID
            && playerName == other.playerName && isServer == other.isServer;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientID);
        serializer.SerializeValue(ref modelIndex);
        serializer.SerializeValue(ref playerID);
        serializer.SerializeValue(ref isServer);
        serializer.SerializeValue(ref playerName);
    }
}
