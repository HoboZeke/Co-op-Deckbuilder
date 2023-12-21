using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MutliplayerController : MonoBehaviour
{
    public static MutliplayerController active;

    bool isMultiplayerGame;

    public int myPlayerNumber;
    public int playerCount;

    public enum PlayerType { Client, Server, Host };
    public PlayerType activePlayerType;

    [Header("Extra Player Setup")]
    public GameObject additionalPlayerPrefab;
    public Vector3[] playerPositions;
    public Vector3[] playerCameraPositions;
    public Vector3[] playerPlayCameraPositions;
    public Vector3[] playerRotations;

    public Dictionary<int, PlayerProfile> playerProfileDictionary = new Dictionary<int, PlayerProfile>();

    private void Awake()
    {
        active = this;
    }

    public bool IsMultiplayerGame()
    {
        return isMultiplayerGame;
    }

    public bool IAmInSomeoneElsesGame()
    {
        if (!isMultiplayerGame) { return false; }
        else
        {
            return !NetworkManager.Singleton.IsHost;
        }
    }

    public bool IAmHost()
    {
        return !IAmInSomeoneElsesGame();
    }

    public void SetupMultiplayerGame(int myPlayerID, int numOfPlayers, PlayerLobbySettings[] lobbySettings)
    {
        myPlayerNumber = myPlayerID;
        playerCount = numOfPlayers;
        isMultiplayerGame = true;

        for(int i = 0; i < playerCount; i++)
        {
            if(i != myPlayerNumber)
            {
                SpawnPlayer(i, lobbySettings[i]);
            }
            else
            {
                playerProfileDictionary.Add(i, new PlayerProfile(Player.active, Zones.main, i, lobbySettings[i]));
            }
        }
    }

    public int PlayerPosition(int playerIndex)
    {
        if(playerIndex == myPlayerNumber) { return 0; }
        else
        {
            return playerProfileDictionary[playerIndex].playmatPos;
        }
    }

    public Player GetPlayerScript(int playerIndex)
    {
        if(playerIndex == myPlayerNumber) { return Player.active; }
        else
        {
            return playerProfileDictionary[playerIndex].playerScript;
        }
    }

    public Player LocalPlayer()
    {
        return Player.active;
    }

    public int GetPlayerNumber(Player script)
    {

        foreach (KeyValuePair<int, PlayerProfile> profile in playerProfileDictionary)
        {
            if(profile.Value.playerScript == script) { return profile.Key; }
        }

        return -1;
    }

    public int LocalPlayerNumber()
    {
        return GetPlayerNumber(LocalPlayer());
    }

    public List<Player> AllPlayers()
    {
        List<Player> playerList = new List<Player>();
        foreach(KeyValuePair<int, PlayerProfile> profile in playerProfileDictionary)
        {
            playerList.Add(profile.Value.playerScript);
        }
        return playerList;
    }

    public Player[] OrderedPlayerArray()
    {
        List<Player> playerList = new List<Player>();
        for (int i = 0; i < playerProfileDictionary.Count; i++)
        {
            playerList.Add(playerProfileDictionary[i].playerScript);
        }
        return playerList.ToArray();
    }

    public List<Zones> AllPlayerZones()
    {
        List<Zones> playerList = new List<Zones>();
        foreach (KeyValuePair<int, PlayerProfile> profile in playerProfileDictionary)
        {
            playerList.Add(profile.Value.zoneScript);
        }
        return playerList;
    }

    public void SpawnPlayer(int playerNumber, PlayerLobbySettings settings)
    {
        Debug.Log("Spawning in a Player (Number"+playerNumber+")");
        GameObject obj = Instantiate(additionalPlayerPrefab);
        obj.name = "Player " + playerNumber;

        playerProfileDictionary.Add(playerNumber, new PlayerProfile(obj.GetComponent<Player>(), obj.GetComponent<Zones>(), playerNumber, settings));

        if (playerNumber != myPlayerNumber)
        {
            if (playerNumber == 0)
            {
                CameraController.main.AddCameraPositions(AbstractPlayerPos(myPlayerNumber), playerCameraPositions[myPlayerNumber], playerRotations[myPlayerNumber], playerNumber);
                CameraController.main.AddCameraPositions(AbstractPlayerPlayPos(myPlayerNumber), playerPlayCameraPositions[myPlayerNumber], playerRotations[myPlayerNumber], playerNumber);
            }
            else
            {
                CameraController.main.AddCameraPositions(AbstractPlayerPos(playerNumber), playerCameraPositions[playerNumber], playerRotations[playerNumber], playerNumber);
                CameraController.main.AddCameraPositions(AbstractPlayerPlayPos(playerNumber), playerPlayCameraPositions[playerNumber], playerRotations[playerNumber], playerNumber);
            }
        }

        if (playerNumber == 0)
        {
            playerProfileDictionary[playerNumber].playmatPos = myPlayerNumber;
            playerNumber = myPlayerNumber;
        }
        obj.transform.position = playerPositions[playerNumber];
        obj.transform.localEulerAngles = playerRotations[playerNumber];

        obj.GetComponent<Player>().playerName = settings.name;

    }

    Vector2 AbstractPlayerPos(int player)
    {
        switch (player)
        {
            default:
                return Vector2.zero;
            case 1:
                return new Vector2(1f, 0f);
            case 2:
                return new Vector2(-1f, 0f);
            case 3:
                return new Vector2(2f, 0f);
            case 4:
                return new Vector2(-2f, 0f);
        }
    }

    Vector2 AbstractPlayerPlayPos(int player)
    {
        switch (player)
        {
            default:
                return new Vector2(0f, 1f);
            case 1:
                return new Vector2(1f, 1f);
            case 2:
                return new Vector2(-1f, 1f);
            case 3:
                return new Vector2(2f, 1f);
            case 4:
                return new Vector2(-2f, 1f);
        }
    }

}

public class PlayerProfile
{
    public Player playerScript;
    public Zones zoneScript;
    public int playmatPos;
    public PlayerLobbySettings lobbyChoices;

    public PlayerProfile(Player player, Zones zone, int position, PlayerLobbySettings lobbySettings)
    {
        playerScript = player;
        zoneScript = zone;
        playmatPos = position;
        lobbyChoices = lobbySettings;

        player.SetupAsPlayerProxy(zoneScript);
        player.UpdateFromLobbyChoices(lobbyChoices);
    }
}
