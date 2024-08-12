using System.Collections;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using TMPro;

public class Relay : MonoBehaviour
{
    public string joinCode;
    public TextMeshProUGUI joinText;
    public TextMeshProUGUI inputCodeText;

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () => {
            OnSignIn();
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }
    
    void OnSignIn()
    {
        Debug.Log("RELAY Signed into Relay, id " + AuthenticationService.Instance.PlayerId);
        joinText.text = "Signed in, looking for join code.";
    }

    public void SetupRelay()
    {
        CreateRelay();
    }

    private async void CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);

            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            joinText.text = "Join Code:\n" + joinCode;

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            //NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData);

            NetworkManager.Singleton.StartHost();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            joinText.text = "Something went wrong, couldn't create join code";
        }
    }

    public void SetJoinCode(string code)
    {
        joinCode = code;
        joinText.text = "Join Code:\n" + joinCode;
    }

    public void JoinRelayWithCode()
    {
        JoinRelay(inputCodeText.text);
    }

    private async void JoinRelay(string code)
    {
        try
        {
            //For some reason TMP adds a blank character at the end of the string, Relay codes are always 6 characters so just grab the first six characters
            code = code.Substring(0, 6);

            Debug.Log("Joining Relay with " + code + " of length " + code.Length);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(code);


            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            //NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
                );

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            joinText.text = "Something went wrong, couldn't join with join code";
        }
    }
}
