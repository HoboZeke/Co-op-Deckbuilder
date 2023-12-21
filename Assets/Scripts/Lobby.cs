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
    }

    public void NewPlayerJoined(int numberOfPlayers)
    {
        UpdateNumberOfPlayerPanels(numberOfPlayers);
        playerCount = numberOfPlayers;
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
    }

    public void SubmitName(int player)
    {
        playerNames[player].text = inputFields[player].text;
        Player.active.playerName = inputFields[player].text;
        playerSettingsList[player].name = inputFields[player].text;
        client.UpdateServerWithPlayerName(inputFields[player].text);
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
            return;
        }

        playerClanImage[player].sprite = clanSprites[clan];
        playerClanImage[player].color = new Color(playerClanImage[player].color.r, playerClanImage[player].color.g, playerClanImage[player].color.b, 1f);

        playerSettingsList[player].clan = clan;
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
