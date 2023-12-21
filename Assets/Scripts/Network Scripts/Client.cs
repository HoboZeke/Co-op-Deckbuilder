using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class Client : NetworkBehaviour
{
    public static Client active;

    public DebugUI debug;

    public Server server;
    public Relay relay;
    public Lobby lobby;

    public ulong playerNumber;
    
    public List<NetworkPlayer> playerInfoScripts = new List<NetworkPlayer>();

    private void Awake()
    {
        active = this;
    }

    public void OnConnectionToNetwork(bool isMine, NetworkPlayer script)
    {
        playerInfoScripts.Add(script);

        if (isMine)
        {
            SetPlayerNumber(script);
            GoToLobby();
            server.GetJoinCodeServerRpc(playerNumber);
        }
        else
        {
            Debug.LogFormat("SOMEONE ELSE HAS JOINED!!!!");
            script.playerID = playerInfoScripts.Count - 1;
            lobby.NewPlayerJoined(playerInfoScripts.Count);
        }

        for(int i = 0; i < playerInfoScripts.Count; i++)
        {
            playerInfoScripts[i].playerID = i;
        }
    }

    public void SetPlayerNumber(NetworkPlayer script)
    {
        Debug.Log("CONNECTED!!!!");
        playerNumber = NetworkManager.Singleton.LocalClientId;
        script.playerID = (int)playerNumber;
        lobby.Setup(playerNumber, playerInfoScripts.Count);
    }

    #region Lobby
    public void GoToLobby()
    {
        lobby.Setup(playerNumber, playerInfoScripts.Count);
        lobby.gameObject.SetActive(true);
        CameraController.main.MoveCameraToLobby();
    }

    public void RecieveJoinCode(string code)
    {
        relay.SetJoinCode(code);
    }

    public void UpdateServerWithPlayerName(string name)
    {
        server.SendNameUpdateToServerRpc(playerNumber, name);
        playerInfoScripts[(int)playerNumber].SetName(name);
    }

    public void RecieveNameUpdateFromServer(ulong playerId, string name)
    {
        playerInfoScripts[(int)playerId].SetName(name);
        lobby.UpdatePlayerName((int)playerId, name);
    }

    public void TellServerIHaveLockedInAClan(ClanSelection.Clan clan)
    {
        server.SendClanChoiceToServerRpc(playerNumber, (int)clan);
    }

    public void RecieveOtherPlayersClanChoice(ulong player, int clan)
    {
        Debug.Log("LOBBY MESSAGE: Player " + player + " has chosen clanNo. " + clan);
        ClanSelection.main.LockClan((int)player, (ClanSelection.Clan)clan);
    }

    public void TellServerIHaveUnlockedAClan(ClanSelection.Clan clan)
    {
        server.UnlockClanChoiceToServerRpc(playerNumber, (int)clan);
    }

    public void RecieveOtherPlayerUnlockingClan(ulong player, int clan)
    {
        Debug.Log("LOBBY MESSAGE: Player " + player + " has given up clanNo. " + clan);
        ClanSelection.main.UnlockClan((int)player, (ClanSelection.Clan)clan);
    }

    public void TellServerIHaveLockedInALeader(int leader)
    {
        server.SendLeaderChoiceToServerRpc(playerNumber, leader);
    }

    public void RecieveOtherPlayersLeaderChoice(ulong player, int leader)
    {
        Debug.Log("LOBBY MESSAGE: Player " + player + " has chosen leaderNo. " + leader);
        LeaderSelection.main.LockLeader((int)player, leader);
    }

    public void TellServerIHaveUnlockedALeader(int leader)
    {
        server.UnlockLeaderChoiceToServerRpc(playerNumber, leader);
    }

    public void RecieveOtherPlayerUnlockingLeader(ulong player, int leader)
    {
        Debug.Log("LOBBY MESSAGE: Player " + player + " has given up leaderNo. " + leader);
        LeaderSelection.main.UnlockLeader((int)player, leader);
    }

    public void TellServerMyReadyStatus(bool ready)
    {
        server.ReadyUpPlayerServerRpc(playerNumber, ready);
    }

    public void RecieveReadyStatus(ulong playerId, bool ready)
    {
        lobby.UpdatePlayerReady((int)playerId, ready);
    }

    public void TellServerToStartTheGame(PlayerLobbySettings[] lobbySettings)
    {
        server.StartTheGameServerRpc(playerNumber, LobbySettingsToString(lobbySettings));
    }

    public void StartMultiplayerGame(string lobbySettings)
    {
        MutliplayerController.active.SetupMultiplayerGame((int)playerNumber, playerInfoScripts.Count, StringToLobbySettings(lobbySettings));
        lobby.menuCanvas.gameObject.SetActive(false);
        lobby.uICanvas.gameObject.SetActive(true);
        //CameraController.main.MoveCamera(Vector2.zero);]
        HomeBase.main.MoveToHomeBase();
    }

    public void TellServerToStartTheBattle()
    {
        server.StartTheBattleServerRpc(playerNumber);
    }

    public void StartMultiplayerBattle()
    {
        GameController.main.StartBattle();
        foreach (Player p in MutliplayerController.active.AllPlayers()) { p.GainCoins(3); }
    }

    string LobbySettingsToString(PlayerLobbySettings[] lobbySettings)
    {
        string s = "";

        for(int i = 0; i < lobbySettings.Length; i++)
        {
            s += lobbySettings[i].playerId + ",";
            s += lobbySettings[i].leader + ",";
            s += lobbySettings[i].clan + ",";
            s += lobbySettings[i].name + ",";
            s += "|";
        }

        return s;
    }

    PlayerLobbySettings[] StringToLobbySettings(string s)
    {
        List<PlayerLobbySettings> list = new List<PlayerLobbySettings>();

        Debug.Log("Lobby Settings: " + s);

        string[] sections = s.Split("|");

        for(int i = 0; i < sections.Length-1; i++)
        {
            Debug.Log("Section: " + sections[i]);
            string[] values = sections[i].Split(",");
            PlayerLobbySettings settings = new PlayerLobbySettings(int.Parse(values[0]), values[3]);
            settings.leader = int.Parse(values[1]);
            settings.clan = int.Parse(values[2]);

            list.Add(settings);
        }

        return list.ToArray();
    }

    #endregion

    public void PassMonsterSpawnsToServer(Dictionary<int, int[]> playerSpawnDict, int[] locationMobs)
    {
        int[] p0 = playerSpawnDict[0];
        int[] p1 = { -1 };
        int[] p2 = { -1 };
        int[] p3 = { -1 };
        int[] p4 = { -1 };

        if (MutliplayerController.active.playerCount > 1) { p1 = playerSpawnDict[1]; }
        if (MutliplayerController.active.playerCount > 2) { p2 = playerSpawnDict[2]; }
        if (MutliplayerController.active.playerCount > 3) { p3 = playerSpawnDict[3]; }
        if (MutliplayerController.active.playerCount > 4) { p4 = playerSpawnDict[4]; }

        server.HostHasSpawnedMonstersServerRpc(playerNumber, p0, p1, p2, p3, p4, locationMobs);
        debug.DebugEvent("Sending Monster Spawns");
    }

    public void RecieveMonsterSpawnsFromServer(int[] player0MobSpawns, int[] player1MobSpawns, int[] player2MobSpawns, int[] player3MobSpawns, int[] player4MobSpawns, int[] locationMobs)
    {
        Dictionary<int, int[]> playerSpawnDict = DictFromPlayerSpawnList(player0MobSpawns, player1MobSpawns, player2MobSpawns, player3MobSpawns, player4MobSpawns);
        EnemyDecks.main.SpawnMultiplayerMonsters(playerSpawnDict, locationMobs);
        debug.DebugEvent("Recieved Monster Spawns");
    }

    public void TellServerIHaveSpawnedSeaCreatureForPlayer(int seaCreature, int playerID)
    {
        server.HosHasSpawnedASeaCreatureForPlayerServerRpc(playerNumber, seaCreature, playerID);
    }

    public void RecieveSpawnSeaCreatureForPlayer(int seaCreature, int playerID)
    {
        Player p = MutliplayerController.active.GetPlayerScript(playerID);
        EnemyDecks.main.SpawnSeaCreatureCardToPlayer(seaCreature, p);
    }

    public void TellServerIHaveSpawnedBeastForPlayer(int beast, int playerID)
    {
        server.HostHasSpawnedABeastForPlayerServerRpc(playerNumber, beast, playerID);
    }

    public void RecieveSpawnBeastForPlayer(int beast, int playerID)
    {
        Player p = MutliplayerController.active.GetPlayerScript(playerID);
        EnemyDecks.main.SpawnBeastCardToPlayer(beast, p);
    }

    public void TellServerIHaveSpawnedBeastToLocation(int beast)
    {
        server.HostHasSpawnedABeastToLocationServerRpc(playerNumber, beast);
    }

    public void RecieveSpawnBeastToLocation(int beast)
    {
        EnemyDecks.main.SpawnBeastCardToLocation(beast);
    }

    public void PassNodeSelectionToServer(int[] nodes)
    {
        server.HostHasSpawnedNodessServerRpc(playerNumber, nodes);
    }

    public void RecieveNodeSelectionFromServer(int[] nodes)
    {
        Location.active.BuildNodes(nodes);
        debug.DebugEvent("Recieved Location Nodes");
    }

    public void PassRecruitmentDeckStackToServer(int[] deckStack)
    {
        server.HostHasStackedRecruitmentDeckServerRpc(playerNumber, deckStack);
        debug.DebugEvent("Sent RecruitmentDeck");
    }

    public void RecieveRecruitmentDeckStackFromServer(int[] deckStack)
    {
        GameController.main.recruitmentDeck.StartBattleFromHostInfo(deckStack);
        debug.DebugEvent("Recieved RecruitmentDeck");
    }

    public void TellServerIHaveBuiltMyDeckList(int[] deckList)
    {
        server.PlayerBuiltDeckServerRpc(playerNumber, deckList);
        debug.DebugEvent("Player " + playerNumber + " built decklist");
    }

    public void BuildOtherPlayersDeck(ulong playerID, int[] deckList)
    {
        MutliplayerController.active.playerProfileDictionary[(int)playerID].zoneScript.deck.BuildFromDeckList(deckList);
        MutliplayerController.active.playerProfileDictionary[(int)playerID].zoneScript.leader.Setup(MutliplayerController.active.playerProfileDictionary[(int)playerID].playerScript.leader);
        debug.DebugEvent("Building Player " + playerID + " deck from decklist");
    }

    public void DrewCard(int cardIndex)
    {
        server.PlayerDrewACardServerRpc(playerNumber, cardIndex);
        debug.DebugEvent("Player " + playerNumber + " drew a card (" + cardIndex + ")");
    }

    public void AnotherPlayerDrewCard(ulong playerID, int cardIndex)
    {
        MutliplayerController.active.playerProfileDictionary[(int)playerID].zoneScript.deck.DrawSpecificCard(cardIndex);
        debug.DebugEvent("Player " + playerID + " drew a card (" + cardIndex + ")");
    }

    public void TellServerIHaveFinishedMyTurn()
    {
        Debug.Log("Told server I am done");
        server.PlayerHasEndedTheirTurnServerRpc(playerNumber);
    }

    public void TogglePlayerEndTurn(ulong playerID)
    {
        int clan = (int)MutliplayerController.active.playerProfileDictionary[(int)playerID].playerScript.clan;
        UIController.main.ToggleClanTurnDoneUI(clan, true);
    }

    public void AllPlayersHaveEndedTheirTurns()
    {
        Debug.Log("Server confirmed all players are done");
        GameController.main.EndTurn();
    }

    public void SendCardUpdate(int cardIndex, NetworkCardUpdate.StateType state)
    {
        NetworkCardUpdate update = new NetworkCardUpdate(playerNumber, state, cardIndex);

        string infoDump = update.InfoDump();

        Debug.Log("NETWORK SENDING: " + infoDump);

        server.SendCardUpdateToServerRpc(playerNumber, infoDump);
        debug.DebugEvent("Player " + playerNumber + " sent cardUpdate (" + infoDump + ")");
    }

    public void SendCardUpdate(int cardIndex, NetworkCardUpdate.StateType state, string message)
    {
        NetworkCardUpdate update = new NetworkCardUpdate(playerNumber, state, cardIndex, message);

        string infoDump = update.InfoDump();

        Debug.Log("NETWORK SENDING: " + infoDump);

        server.SendCardUpdateToServerRpc(playerNumber, infoDump);
        debug.DebugEvent("Player " + playerNumber + " sent cardUpdate (" + infoDump + ")");
    }

    public void CardUpdateFromOtherPlayer(ulong fromPlayer, string infoDump)
    {
        Debug.Log("NETWORK RECIEVED: " + infoDump);
        NetworkCardUpdate update = new NetworkCardUpdate(infoDump);
        DecodeCardUpdate(update);
        debug.DebugEvent("Player " + fromPlayer + " sent cardUpdate (" + infoDump + ")");
    }

    public void TellServerWeHaveMovedToExploreMode()
    {
        server.GameHasMovedToExploreMode();
    }

    public void SendMessageToOtherPlayers(string message)
    {
        server.SendMessageToServerRpc(playerNumber, message);
    }

    public void MessageFromOtherPlayer(ulong fromPLayer, string message)
    {
        debug.DebugEvent("Message from Player " + fromPLayer + " : " + message);
    }

    #region ExploreUpdates
    public void TellServerIHaveChosenANode(Vector3 targetLoc, Location.NodeEvent nodeEvent)
    {
        server.PlayerHasChosenTheirNodeServerRpc(playerNumber, targetLoc, (int)nodeEvent);
        debug.DebugEvent("Player " + playerNumber + " chose node (" + nodeEvent.ToString() + ")");
    }

    public void RecievePlayerNodeChoice(ulong playerNum, Vector3 targetLoc, int nodeEvent)
    {
        int pNum = (int)playerNum;
        Location.NodeEvent nEvent = (Location.NodeEvent)nodeEvent;

        Location.active.MovePlayerFigureToLocation(pNum, targetLoc, nEvent, false);

        debug.DebugEvent("Player " + playerNum + " chose node (" + nEvent.ToString() + ")");
    }

    public void TellServerIHaveLostACard(int cardRef)
    {
        server.PlayerHasLostACardServerRpc(playerNumber, cardRef);
        debug.DebugEvent("Player " + playerNumber + " lost a card (" + cardRef + ")");
    }

    public void AnotherPlayerHasLostCard(ulong playerID, int cardRef)
    {
        MutliplayerController.active.playerProfileDictionary[(int)playerID].playerScript.RemoveCardByRefFromPlayerDeckList(cardRef);
        debug.DebugEvent("Player " + playerID + " lost a card (" + cardRef + ")");
    }

    public void TellServerIHaveGainedACard(int cardRef)
    {
        server.PlayerHasGainedACardServerRpc(playerNumber, cardRef);
        debug.DebugEvent("Player " + playerNumber + " gained a card (" + cardRef + ")");
    }

    public void AnotherPlayerHasGainedACard(ulong playerId, int cardRef)
    {
        CardArchive.main.SpawnCard(cardRef, MutliplayerController.active.playerProfileDictionary[(int)playerId].playerScript);
        debug.DebugEvent("Player " + playerId + " gained a card (" + cardRef + ")");
    }

    public void TellServerIHaveAdjustedACardAttack(int cardRef, int amountAdjusted)
    {
        server.PlayerHasAdjustedACardAttackServerRpc(playerNumber, cardRef, amountAdjusted);
        debug.DebugEvent("Player " + playerNumber + " adjusted a card attack (" + cardRef + ") by (" + amountAdjusted + ")");
    }

    public void AnotherPlayerAdjustCardAttack(ulong playerId, int cardRef, int amountAdjusted)
    {
        MutliplayerController.active.playerProfileDictionary[(int)playerId].playerScript.GetCardByRef(cardRef).AdjustAttackMod(amountAdjusted);

        debug.DebugEvent("Player " + playerId + " adjusted a card attack (" + cardRef + ") by (" + amountAdjusted + ")");
    }

    public void TellServerMyCardAreFighting(CardObject left, CardObject right)
    {
        server.PlayerCardsAreFightingServerRpc(playerNumber, left.referenceIndex, right.referenceIndex);
        debug.DebugEvent("Player " + playerNumber + " fought two cards (" + left.referenceIndex + ") by (" + right.referenceIndex + ")");
    }

    public void AnotherPLayersCardsAreFighting(ulong playerId, int leftCardRef, int rightCardRef)
    {
        Player player = MutliplayerController.active.playerProfileDictionary[(int)playerId].playerScript;
        CardObject left = player.GetCardByRef(leftCardRef);
        CardObject right = player.GetCardByRef(rightCardRef);

        Location.active.eventManager.SimulateFightForMultiplayerUpdate(left, right, player);
        debug.DebugEvent("Player " + playerId + " fought two cards (" + leftCardRef + ") by (" + rightCardRef + ")");
    }

    public void TellServerIHaveANewLeader(int newLeader)
    {
        server.PlayerHasNewLeaderServerRpc(playerNumber, newLeader);
        debug.DebugEvent("Player " + playerNumber + " choose leader (" + newLeader + ")");
    }

    public void AnotherPlayerLeaderUpdate(ulong playerId, int newLeader)
    {
        MutliplayerController.active.playerProfileDictionary[(int)playerId].playerScript.UpdateLeader(newLeader);
        debug.DebugEvent("Player " + playerId + " choose leader (" + newLeader + ")");
    }

    public void TellServerIHaveGainedHealth(int amount)
    {
        server.PlayerHasGainedHealthServerRpc(playerNumber, amount);
        debug.DebugEvent("Player " + playerNumber + " gained health (" + amount + ")");
    }

    public void AnotherPlayerHasGainedHealth(ulong playerId, int amount)
    {
        MutliplayerController.active.playerProfileDictionary[(int)playerId].playerScript.GainHealth(amount);
        debug.DebugEvent("Player " + playerId + " gained health (" + amount + ")");
    }

    public void TellServerIHaveGainedARelic(int relicId)
    {
        server.PlayerHasGainedARelicServerRpc(playerNumber, relicId);
        debug.DebugEvent("Player " + playerNumber + " gained relic (" + relicId + ")");
    }

    public void AnotherPlayerHasGainedARelic(ulong playerId, int relicId)
    {
        Player p = MutliplayerController.active.playerProfileDictionary[(int)playerId].playerScript;
        Relics.main.SpecificRelic(relicId).ApplyRelic(p);
        debug.DebugEvent("Player " + playerId + " gained relic (" + relicId + ")");
    }

    public void TellServerIAmInTradeEvent()
    {
        server.PlayerJoinedTradeEventServerRpc(playerNumber);
        debug.DebugEvent("Player " + playerNumber + " entered trade event");
    }

    public void AnotherPlayerIsInTradeEvent(ulong playerId)
    {
        Location.active.eventManager.FriendWaitingForTrade(MutliplayerController.active.playerProfileDictionary[(int)playerId].playerScript);
        debug.DebugEvent("Player " + playerId + " entered trade event");
    }

    public void TellServerILeftTradeEvent()
    {
        server.PlayerLeftTradeEventServerRpc(playerNumber);
        debug.DebugEvent("Player " + playerNumber + " left trade event");
    }

    public void AnotherPlayerLeftTradeEvent(ulong playerId)
    {
        Location.active.eventManager.FriendLeftTrade(MutliplayerController.active.playerProfileDictionary[(int)playerId].playerScript);
        debug.DebugEvent("Player " + playerId + " left trade event");
    }

    public void TellServerIHaveOfferedCardForTrade(int cardIndex)
    {
        server.PlayerOfferedCardTradeEventServerRpc(playerNumber, cardIndex);
        debug.DebugEvent("Player " + playerNumber + " is offering card (" + cardIndex + ") for trade");
    }

    public void AnotherPlayerIsOfferingCardforTrade(ulong playerId, int cardIndex)
    {
        Location.active.eventManager.FriendOfferingCard(cardIndex, MutliplayerController.active.playerProfileDictionary[(int)playerId].playerScript);
        debug.DebugEvent("Player " + playerId + " is offering card (" + cardIndex + ") for trade");
    }

    public void TellServerMyTradeStatus(bool status, int tradePartner)
    {
        server.PlayerStatusUpdateTradeEventServerRpc(playerNumber, status, tradePartner);
        debug.DebugEvent("Player " + playerNumber + " update trade status (" + status.ToString() + ")");
    }

    public void AnotherPlayersTradeStatusUpdate(ulong playerId, bool status, int tradePartner)
    {
        Location.active.eventManager.FriendTradeReadyStatus(MutliplayerController.active.playerProfileDictionary[(int)playerId].playerScript, status, tradePartner);
        debug.DebugEvent("Player " + playerId + " update trade status (" + status.ToString() + ")");
    }

    public void TellServerIReplaceACardInMyDeck(int oldCardIndex, int newCardSpawnIndex)
    {
        server.PlayerReplacedACardInTheirDeckServerRpc(playerNumber, oldCardIndex, newCardSpawnIndex);
        debug.DebugEvent("Player " + playerNumber + " has swapped card (" + oldCardIndex + ") for card (" + newCardSpawnIndex + ")");
    }

    public void AnotherPlayerReplaceACardInTheirDeck(ulong playerId, int oldCardIndex, int newCardSpawnIndex)
    {
        Player p = MutliplayerController.active.playerProfileDictionary[(int)playerId].playerScript;
        CardObject oldCard = p.GetCardByRef(oldCardIndex);
        CardObject newCard = CardArchive.main.SpawnLooseCard(newCardSpawnIndex).GetComponent<CardObject>();

        p.ReplaceCardInPlayerDeckList(oldCard, newCard);
        p.zones.deck.InstantlyShuffleCardIntoDeck(newCard);

        debug.DebugEvent("Player " + playerId + " has swapped card (" + oldCardIndex + ") for card (" + newCardSpawnIndex + ")");
    }

    public void TellServerIHaveCompletedMyNode()
    {
        server.PlayerHasFinishedTheirNodeServerRpc(playerNumber);
    }

    public void AllPlayersHaveFinishedTheirNodes()
    {
        Location.active.AllPlayersCompletedCurrentNode();
    }

    #endregion

    #region CardUpdates

    #region MetaUpdates

    public void TellServerIHaveDonatedFundsToLongHall(int treasure, int pop, int weapons)
    {
        server.PlayerHasDonatedFundsToLongHallServerRpc(playerNumber, treasure, pop, weapons);
    }

    public void OtherPlayerDonatedFundsToLongHall(int playerNumber, int treasure, int pop, int weapons)
    {
        HomeBase.main.OtherPlayerDonated(treasure, pop, weapons);
    }

    #endregion

    void DecodeCardUpdate(NetworkCardUpdate cardUpdate)
    {
        Debug.Log("Processing Network Update");
        switch (cardUpdate.state)
        {
            case NetworkCardUpdate.StateType.DeckToHand:
                DrewCard((int)cardUpdate.playerOwnerId, cardUpdate.cardDeckIndex);
                break;
            case NetworkCardUpdate.StateType.HandToPlay:
                CardFromHandToPlay((int)cardUpdate.playerOwnerId, cardUpdate.cardDeckIndex);
                break;
            case NetworkCardUpdate.StateType.TappedInPlay:
                TapCard((int)cardUpdate.playerOwnerId, cardUpdate.cardDeckIndex);
                break;
            case NetworkCardUpdate.StateType.PlayToDiscard:
                break;
            case NetworkCardUpdate.StateType.HandToDiscard:
                DiscardCard((int)cardUpdate.playerOwnerId, cardUpdate.cardDeckIndex);
                break;
            case NetworkCardUpdate.StateType.DiscardToDeck:
                break;
            case NetworkCardUpdate.StateType.CardUpdate:
                break;
            case NetworkCardUpdate.StateType.Recruit:
                RecruitCard((int)cardUpdate.playerOwnerId, cardUpdate.cardDeckIndex);
                break;
            case NetworkCardUpdate.StateType.Activate:
                ActivateCard((int)cardUpdate.playerOwnerId, cardUpdate.cardDeckIndex);
                break;
            case NetworkCardUpdate.StateType.ActivatedWithTarget:
                ActivateCardWithTarget((int)cardUpdate.playerOwnerId, cardUpdate.cardDeckIndex, cardUpdate.message);
                break;
            case NetworkCardUpdate.StateType.AttachedToTarget:
                AttachedCardToTarget((int)cardUpdate.playerOwnerId, cardUpdate.cardDeckIndex, cardUpdate.message);
                break;
        }
    }

    void DrewCard(int player, int cardRef)
    {
        Debug.Log("Processing Network DrawCard Update");
        Player p = MutliplayerController.active.GetPlayerScript(player);
        p.zones.deck.DrawSpecificCard(cardRef);
    }

    void DiscardCard(int player, int cardRef)
    {
        Debug.Log("Processing Network DiscardCard Update");
        Player p = MutliplayerController.active.GetPlayerScript(player);
        p.zones.hand.DiscardCard(p.GetCardByRef(cardRef));

    }

    void TapCard(int player, int cardRef)
    {
        Debug.Log("Processing Network TapCard Update");
        Player p = MutliplayerController.active.GetPlayerScript(player);
        p.GetCardByRef(cardRef).RemoteCallTap();
    }

    void RecruitCard(int player, int cardRef)
    {
        Debug.Log("Processing Network Recruit Update");
        Player p = MutliplayerController.active.GetPlayerScript(player);
        p.zones.RecruitCard(p.zones.recruit.GetCardByRef(cardRef));
    }

    void CardFromHandToPlay(int player, int cardRef)
    {
        Debug.Log("Processing Network PlayCard Update");
        Player p = MutliplayerController.active.GetPlayerScript(player);
        CardObject version = p.GetCardByRef(cardRef);
        version.myCard.OnClickedInHand(version);
    }

    void ActivateCard(int player, int cardRef)
    {
        Debug.Log("Processing Network ActivateCard Update");
        Player p = MutliplayerController.active.GetPlayerScript(player);
        CardObject version = p.GetCardByRef(cardRef);
        version.myCard.Activate(version);
    }

    void ActivateCardWithTarget(int player, int cardRef, string message)
    {
        Debug.Log("Processing Network ActivateCardWithTarget Update| P:" + player + " Card:" + cardRef + " Msg:" + message);
        Player p = MutliplayerController.active.GetPlayerScript(player);
        CardObject version = p.GetCardByRef(cardRef);

        string[] words = message.Split('|');
        int targetRef = int.Parse(words[1]);

        if(words[0] == "Monster" || words[0] == "Location")
        {
            Debug.Log("Targetting enemy ref: " + targetRef);
            version.myCard.ActivateWithTarget(version, EnemyDecks.main.GetCardWithRef(targetRef));
        }
    }

    void AttachedCardToTarget(int player, int cardRef, string message)
    {
        Debug.Log("Processing Network AttachedCardToTarget Update| P:" + player + " Card:" + cardRef + " Msg:" + message);
        Player p = MutliplayerController.active.GetPlayerScript(player);
        CardObject version = p.GetCardByRef(cardRef);

        string[] words = message.Split('|');
        int targetRef = int.Parse(words[1]);

        if (words[0] == "Monster" || words[0] == "Location")
        {
            Debug.Log("Targetting card ref: " + targetRef);
            version.myCard.AttachToTarget(version, EnemyDecks.main.GetCardWithRef(targetRef));
        }
        else if (words[0] == "Play")
        {
            Debug.Log("Targetting card ref: " + targetRef);
            version.myCard.AttachToTarget(version, p.GetCardByRef(targetRef));
        }
    }

    #endregion

    public void SendMyBoardStateToServer(string boardState)
    {
        server.SendBoardStateToOtherPlayersServerRpc(playerNumber, boardState);
    }

    public void RecieveBoardStateFromServer(string boardState, ulong fromPlayer)
    {
        debug.DebugEvent("Checking board states with one sent from Player " + (int)fromPlayer);
        GameController.main.VarifyBoardState(boardState);
    }

    public void TellServerEveryoneDrawsACard()
    {
        server.EveryonDrawsACardServerRpc(playerNumber);
    }

    public void AllPlayersDrawACard()
    {
        MutliplayerController.active.LocalPlayer().zones.deck.DrawCard();
    }

    Dictionary<int, int[]> DictFromPlayerSpawnList(int[] p0, int[] p1, int[] p2, int[] p3, int[] p4)
    {
        Dictionary<int, int[]> dict = new Dictionary<int, int[]>();

        dict.Add(0, p0);
        dict.Add(1, p1);
        dict.Add(2, p2);
        dict.Add(3, p3);
        dict.Add(4, p4);

        Debug.Log("Built dictionary from recieved spawns, length = " + dict.Count);
        foreach (KeyValuePair<int, int[]> pair in dict) { Debug.Log("Spawn dictionary entry " + pair.Key + " has " + pair.Value.Length + " spawns"); }

        return dict;
    }
}

[System.Serializable]
public class NetworkCardUpdate
{
    public ulong playerOwnerId;
    public enum StateType { DeckToHand, HandToPlay, TappedInPlay, PlayToDiscard, HandToDiscard, DiscardToDeck, CardUpdate, Recruit, Activate, ActivatedWithTarget, AttachedToTarget };
    public StateType state;
    public int cardDeckIndex;
    public string message;

    public NetworkCardUpdate(ulong iD, StateType updateType, int cardId, string optionalMessage)
    {
        playerOwnerId = iD;
        state = updateType;
        cardDeckIndex = cardId;
        message = optionalMessage;
    }

    public NetworkCardUpdate(ulong iD, StateType updateType, int cardId)
    {
        playerOwnerId = iD;
        state = updateType;
        cardDeckIndex = cardId;
        message = "";
    }

    public string InfoDump()
    {
        string pID = playerOwnerId.ToString();
        string stateString = state.ToString();
        string cardId = cardDeckIndex.ToString();
        string msg = message;

        string toSend = string.Join('|', new string[] { pID, stateString, cardId, msg });
        Debug.Log("NETWORK CLIENT Built Message: " + toSend);
        return toSend;
    }

    public NetworkCardUpdate(string infoDump)
    {
        string[] info = infoDump.Split('|');

        playerOwnerId = ulong.Parse(info[0]);
        state = (StateType)System.Enum.Parse( typeof(StateType), info[1]);
        cardDeckIndex = int.Parse(info[2]);
        if (info.Length > 3)
        {
            message = info[3];
            for (int i = 4; i < info.Length; i++)
            {
                message += "|" + info[i];
            }
        }


        Debug.Log("NETWORK UPDATE DECODED: Owner-" + playerOwnerId + " State-" + state.ToString() + " CardIndex-" + cardDeckIndex + " Msg-" + message);
    }
}
