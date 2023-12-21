using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkPlayer : NetworkBehaviour
{
    public string playerName;
    public int playerID;

    public override void OnNetworkSpawn()
    {
        Client.active.OnConnectionToNetwork(IsOwner, this);
    }

    public void SetName(string newName)
    {
        playerName = newName;
        name = "NetworkPlayer " + newName;
    }
}
