using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkUI : MonoBehaviour
{
    public Relay relay;

    public void StartAsHost()
    {
        relay.SetupRelay();
    }

    public void StartAsClient()
    {
        relay.JoinRelayWithCode();
    }
}
