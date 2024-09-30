using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class Lobby : MonoBehaviour
{
    public Client client;

    public GameObject[] playerPanels;
    public TextMeshProUGUI[] playerNames;
    public TMP_InputField[] inputFields;
    public GameObject[] submitButtons;
    public Image[] readyIcons;
    public Sprite[] readySprites;
    public Image[] playerClanImage;
    public Sprite[] clanSprites;
    public TextMeshProUGUI[] playerLeaderText;

    public TextMeshProUGUI startGameButtonText;
    public GameObject[] menuButtonsToHide;
    public GameObject clanOptions, leaderOptions;

    [Header("Lobby Objects")]
    [SerializeField] GameObject[] lobbyLongBoats;
    [SerializeField] GameObject[] lobbyLongBoatsLights;
    [SerializeField] Material[] lobbyLongBoatDefaultMats;
    [SerializeField] Material[] lobbyLongBoatTransparentMats;
    [SerializeField] GameObject localPlayerCube, player1Cube, player2Cube, player3Cube;
    [SerializeField] Image localPlayerSailImage, player1SailImage, player2SailImage, player3SailImage;
    [SerializeField] Material[] clanMats;
    [SerializeField] Material[] transparentClanMats;
    [SerializeField] Sprite unchosenClanSprite;
    [SerializeField] Material unchosenClanMat;

    public GameObject menuCanvas, uICanvas;
    int playerId;
    bool isHost;
    int playerCount;
    List<PlayerLobbySettings> playerSettingsList = new List<PlayerLobbySettings>();

    public void Setup(ulong myPlayerId, int numberOfPlayers)
    {
        playerId = (int)myPlayerId;
        playerCount = numberOfPlayers;
        UpdateNumberOfPlayerPanels(numberOfPlayers);
        UpdateStartButton();

        foreach(GameObject obj in menuButtonsToHide) { obj.SetActive(false); }

        clanOptions.SetActive(true);
        leaderOptions.SetActive(true);
        leaderOptions.GetComponent<LeaderSelection>().Setup();

        localPlayerCube.layer = 9;
        localPlayerSailImage.gameObject.layer = 9;
        localPlayerSailImage.transform.parent.gameObject.layer = 9;
        player1Cube.layer = 9;
        player1SailImage.gameObject.layer = 9;
        player1SailImage.transform.parent.gameObject.layer = 9;
        player2Cube.layer = 9;
        player2SailImage.gameObject.layer = 9;
        player2SailImage.transform.parent.gameObject.layer = 9;
        player3Cube.layer = 9;
        player3SailImage.gameObject.layer = 9;
        player3SailImage.transform.parent.gameObject.layer = 9;

        if (PlayerPrefs.HasKey("PlayersLastUsedName")) { SubmitName(PlayerPrefs.GetString("PlayersLastUsedName"), playerId); }
    }
    public void SinglePlayerSetup()
    {
        playerId = 0;
        playerCount = 1;
        UpdateNumberOfPlayerPanels(1);
        UpdateStartButton();

        foreach (GameObject obj in menuButtonsToHide) { obj.SetActive(false); }

        clanOptions.SetActive(true);
        leaderOptions.SetActive(true);
        leaderOptions.GetComponent<LeaderSelection>().Setup();

        localPlayerCube.layer = 9;
        localPlayerSailImage.gameObject.layer = 9;
        localPlayerSailImage.transform.parent.gameObject.layer = 9;
        player1Cube.layer = 9;
        player1SailImage.gameObject.layer = 9;
        player1SailImage.transform.parent.gameObject.layer = 9;
        player2Cube.layer = 9;
        player2SailImage.gameObject.layer = 9;
        player2SailImage.transform.parent.gameObject.layer = 9;
        player3Cube.layer = 9;
        player3SailImage.gameObject.layer = 9;
        player3SailImage.transform.parent.gameObject.layer = 9;

    }


    public void NewPlayerJoined(int numberOfPlayers)
    {
        UpdateNumberOfPlayerPanels(numberOfPlayers);
        playerCount = numberOfPlayers;
        client.UpdateServerWithPlayerName(Player.active.playerName);
    }

    void UpdateStartButton()
    {
        isHost = NetworkManager.Singleton.IsHost;

        if (!isHost) { startGameButtonText.text = "Ready?"; }
    }

    void UpdateNumberOfPlayerPanels(int numberOfPLayers)
    {
        foreach(GameObject obj in playerPanels) { obj.SetActive(false); }

        for(int i = 0; i < numberOfPLayers; i++)
        {
            playerPanels[i].SetActive(true);
            if(i == playerId)
            {
                inputFields[i].gameObject.SetActive(true);
                submitButtons[i].SetActive(true);
            }
            else
            {
                inputFields[i].gameObject.SetActive(false);
                submitButtons[i].SetActive(false);
            }

            if(i == 0) { readyIcons[i].sprite = readySprites[0]; }
            else { readyIcons[i].sprite = readySprites[1]; }

            if(playerSettingsList.Count - 1 < i) { playerSettingsList.Add(new PlayerLobbySettings(i, "Player " + playerSettingsList.Count)); }
        }

        for(int i = 0; i < lobbyLongBoats.Length; i++)
        {
            Transform t = lobbyLongBoats[i].transform;
            lobbyLongBoatsLights[i].SetActive(i < numberOfPLayers);
            if (i < numberOfPLayers)
            {
                t.GetChild(0).GetComponent<MeshRenderer>().material = lobbyLongBoatDefaultMats[0];
                t.GetChild(1).GetComponent<MeshRenderer>().material = lobbyLongBoatDefaultMats[1];
                t.GetChild(2).GetComponent<MeshRenderer>().material = lobbyLongBoatDefaultMats[2];
                t.GetChild(3).GetComponent<MeshRenderer>().material = lobbyLongBoatDefaultMats[3];
            }
            else
            {
                t.GetChild(0).GetComponent<MeshRenderer>().material = lobbyLongBoatTransparentMats[0];
                t.GetChild(1).GetComponent<MeshRenderer>().material = lobbyLongBoatTransparentMats[1];
                t.GetChild(2).GetComponent<MeshRenderer>().material = lobbyLongBoatTransparentMats[2];
                t.GetChild(3).GetComponent<MeshRenderer>().material = lobbyLongBoatTransparentMats[3];
            }
        }
    }

    public void SubmitName(int player)
    {
        string name = inputFields[player].text;
        playerNames[player].text = name;
        Player.active.playerName = name;
        playerSettingsList[player].name = name;
        PlayerPrefs.SetString("PlayersLastUsedName", name);
        client.UpdateServerWithPlayerName(name);
    }

    void SubmitName(string name, int player)
    {
        playerNames[player].text = name;
        Player.active.playerName = name;
        playerSettingsList[player].name = name;
        client.UpdateServerWithPlayerName(name);
    }

    public void UpdatePlayerName(int player, string name)
    {
        playerNames[player].text = name;
        playerSettingsList[player].name = name;
    }

    public void UpdatePlayerLeader(int leader)
    {
        UpdatePlayerLeader(playerId, leader);
    }

    public void UpdatePlayerLeader(int player, int leader)
    {
        if (leader < 0)
        {
            playerLeaderText[player].text = "No Leader Selected";
        }
        else
        {
            playerLeaderText[player].text = LeaderArchive.main.leaders[leader].name;
        }

        playerSettingsList[player].leader = leader;
    }

    public void UpdatePlayerClan(int clan)
    {
        UpdatePlayerClan(playerId, clan);
    }

    public void UpdatePlayerClan(int player, int clan)
    {
        Debug.Log("Updating clan selection for player " + player + " to clan number " + clan);
        if(clan < 0) {
            playerClanImage[player].color = new Color(playerClanImage[player].color.r, playerClanImage[player].color.g, playerClanImage[player].color.b, 0f);
            playerSettingsList[player].clan = clan;
            if(player < playerId) { player += 1; }
            ResetPlayerBoatImagesTodefault(player);
            return;
        }

        playerClanImage[player].sprite = clanSprites[clan];
        playerClanImage[player].color = new Color(playerClanImage[player].color.r, playerClanImage[player].color.g, playerClanImage[player].color.b, 1f);

        playerSettingsList[player].clan = clan;

        if(player == playerId)
        {
            localPlayerCube.GetComponent<MeshRenderer>().material = clanMats[clan];
            localPlayerSailImage.sprite = clanSprites[clan];
        }
        else if(player < playerId)
        {
            UpdatePlayerBoatWithClanChoice(player + 1, clan);
        }
        else
        {
            UpdatePlayerBoatWithClanChoice(player, clan);
        }
    }

    public void UpdatePotentialClanChoice(int clan)
    {
        localPlayerCube.GetComponent<MeshRenderer>().material = transparentClanMats[clan];
        localPlayerSailImage.sprite = clanSprites[clan];
    }

    void UpdatePlayerBoatWithClanChoice(int playerslot, int clan)
    {
        switch (playerslot)
        {
            case 0:
                localPlayerCube.GetComponent<MeshRenderer>().material = clanMats[clan];
                localPlayerSailImage.sprite = clanSprites[clan];
                break;
            case 1:
                player1Cube.GetComponent<MeshRenderer>().material = clanMats[clan];
                player1SailImage.sprite = clanSprites[clan];
                break;
            case 2:
                player2Cube.GetComponent<MeshRenderer>().material = clanMats[clan];
                player2SailImage.sprite = clanSprites[clan];
                break;
            case 3:
                player3Cube.GetComponent<MeshRenderer>().material = clanMats[clan];
                player3SailImage.sprite = clanSprites[clan];
                break;
        }
    }

    void ResetPlayerBoatImagesTodefault(int playerslot)
    {
        switch (playerslot)
        {
            case 0:
                localPlayerCube.GetComponent<MeshRenderer>().material = unchosenClanMat;
                localPlayerSailImage.sprite = unchosenClanSprite;
                break;
            case 1:
                player1Cube.GetComponent<MeshRenderer>().material = unchosenClanMat;
                player1SailImage.sprite = unchosenClanSprite;
                break;
            case 2:
                player2Cube.GetComponent<MeshRenderer>().material = unchosenClanMat;
                player2SailImage.sprite = unchosenClanSprite;
                break;
            case 3:
                player3Cube.GetComponent<MeshRenderer>().material = unchosenClanMat;
                player3SailImage.sprite = unchosenClanSprite;
                break;
        }
    }

    public void UpdatePlayerReady(int player, bool ready)
    {
        Sprite s = readySprites[1];
        if (ready) { s = readySprites[2]; }

        readyIcons[player].sprite = s;
    }

    public void StartButton()
    {
        if (isHost)
        {
            client.TellServerToStartTheGame(LobbySettings());
        }
        else
        {
            if(startGameButtonText.text == "Ready?")
            {
                //Ready up
                client.TellServerMyReadyStatus(true);
                startGameButtonText.text = "Unready";
                UpdatePlayerReady(playerId, true);
            }
            else 
            {
                //Unready
                client.TellServerMyReadyStatus(false);
                startGameButtonText.text = "Ready?";
                UpdatePlayerReady(playerId, false);
            }
        }
    }

    public PlayerLobbySettings[] LobbySettings()
    {
        foreach(PlayerLobbySettings p in playerSettingsList)
        {
            if(p.leader < 0) { p.leader = LeaderSelection.main.RandomUnchosenLeader(); }
            if(p.clan < 0) { p.clan = ClanSelection.main.RandomUnchosenClan(); }
        }

        return playerSettingsList.ToArray();
    }
}

[System.Serializable]
public class PlayerLobbySettings
{
    public int playerId;
    public int leader;
    public int clan;
    public string name;

    public PlayerLobbySettings(int iD, string chosenName)
    {
        playerId = iD;
        leader = -1;
        clan = -1;
        name = chosenName;
    }
}
