using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Server : NetworkBehaviour
{
    public Client client;
    public Relay relay;

    public List<ulong> otherClientIds = new List<ulong>();

    List<ulong> playersEndedTurn = new List<ulong>();
    List<ulong> playersExploredNode = new List<ulong>();

    //Server RPCs, these should be called form the client script to let the Server know something.

    [ServerRpc(RequireOwnership = false)]
    public void SendMessageToServerRpc(ulong playerId, string message)
    {
        MessageFromServerClientRpc(playerId, message, SendToAllBut(playerId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendClanChoiceToServerRpc(ulong playerId, int chosenClan)
    {
        ClanChoiceFromServerClientRpc(playerId, chosenClan, SendToAllBut(playerId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void UnlockClanChoiceToServerRpc(ulong playerId, int chosenClan)
    {
        ReleaseClanChoiceFromServerClientRpc(playerId, chosenClan, SendToAllBut(playerId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendLeaderChoiceToServerRpc(ulong playerId, int leader)
    {
        LeaderChoiceFromServerClientRpc(playerId, leader, SendToAllBut(playerId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void UnlockLeaderChoiceToServerRpc(ulong playerId, int leader)
    {
        ReleaseLeaderChoiceFromServerClientRpc(playerId, leader, SendToAllBut(playerId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void GetJoinCodeServerRpc(ulong playedId)
    {
        SendJoinCodeToClientRpc(relay.joinCode, SendToSpecific(playedId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendCardUpdateToServerRpc(ulong playerId, string message)
    {
        CardUpdateFromServerClientRpc(playerId, message, SendToAllBut(playerId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendNameUpdateToServerRpc(ulong playerId, string message)
    {
        SendNameUpdateToClientRpc(playerId, message, SendToAllBut(playerId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReadyUpPlayerServerRpc(ulong playerId, bool ready)
    {
        PlayerReadyClientRpc(playerId, ready, SendToAllBut(playerId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartTheGameServerRpc(ulong playerId, string lobbySettings)
    {
        StartYourGameClientRpc(playerId, lobbySettings, SendToAll());
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartTheBattleServerRpc(ulong playerId)
    {
        StartYourBattleClientRpc(playerId, SendToAll());
    }

    [ServerRpc(RequireOwnership = false)]
    public void HostHasSpawnedMonstersServerRpc(ulong playerId, int[] player0MobSpawns, int[] player1MobSpawns, int[] player2MobSpawns, int[] player3MobSpawns, int[] player4MobSpawns, int[] locationMobs)
    {
        HereAreYourMonsterSpawnsClientRpc(playerId, player0MobSpawns, player1MobSpawns, player2MobSpawns, player3MobSpawns, player4MobSpawns, locationMobs, SendToAllBut(playerId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void HostHasSpawnedNodessServerRpc(ulong playerId, int[] nodes)
    {
        HereAreYourNodesClientRpc(playerId, nodes, SendToAllBut(playerId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void HostHasStackedRecruitmentDeckServerRpc(ulong playerId, int[] deckStack)
    {
        HereIsYourRecruitmentDeckClientRpc(playerId, deckStack, SendToAllBut(playerId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerBuiltDeckServerRpc(ulong playerId, int[] decklist)
    {
        PlayerHasBuiltDeckClientRpc(playerId, decklist, SendToAllBut(playerId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerDrewACardServerRpc(ulong playerId, int cardRef)
    {
        PlayerHasDrawnACardClientRpc(playerId, cardRef, SendToAllBut(playerId));
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void PlayerHasEndedTheirTurnServerRpc(ulong playerId)
    {
        PlayerHasEndedTurnClientRpc(playerId, SendToAll());
        CheckIfAllPlayersHaveEndedTurn(playerId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerHasFinishedTheirNodeServerRpc(ulong playerId)
    {
        CheckIfAllPlayersHaveFinishedTheirNode(playerId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerHasGainedACardServerRpc(ulong playerId, int card)
    {
        PlayerHasGainedACardClientRpc(playerId, card, SendToAllBut(playerId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerHasLostACardServerRpc(ulong playerId, int card)
    {
        PlayerHasLostACardClientRpc(playerId, card, SendToAllBut(playerId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerHasNewLeaderServerRpc(ulong playerId, int leader)
    {
        PlayerHasNewLeaderClientRpc(playerId, leader, SendToAllBut(playerId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerHasAdjustedACardAttackServerRpc(ulong playerId, int card, int amount)
    {
        PlayerHasAdjustedACardAttackClientRpc(playerId, card, amount, SendToAllBut(playerId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerHasGainedHealthServerRpc(ulong playerId, int amount)
    {
        PlayerHasGainedHealthClientRpc(playerId, amount, SendToAllBut(playerId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerHasGainedARelicServerRpc(ulong playerId, int relicId)
    {
        PlayerHasGainedARelicClientRpc(playerId, relicId, SendToAllBut(playerId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerJoinedTradeEventServerRpc(ulong playerId)
    {
        PlayerJoinedTradeEventClientRpc(playerId, SendToAllBut(playerId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerLeftTradeEventServerRpc(ulong playerId)
    {
        PlayerLeftTradeEventClientRpc(playerId, SendToAllBut(playerId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerOfferedCardTradeEventServerRpc(ulong playerId, int cardIndex)
    {
        PlayerOfferedCardTradeEventClientRpc(playerId, cardIndex, SendToAllBut(playerId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerStatusUpdateTradeEventServerRpc(ulong playerId, bool status, int tradePartner)
    {
        PlayerStatusUpdateTradeEventClientRpc(playerId, status, tradePartner, SendToAllBut(playerId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerReplacedACardInTheirDeckServerRpc(ulong playerId, int oldCard, int newCardSpawnIndex)
    {
        PlayerReplacedACardInTheirDeckClientRpc(playerId, oldCard, newCardSpawnIndex, SendToAllBut(playerId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerCardsAreFightingServerRpc(ulong playerId, int leftCardRef, int rightCardRef)
    {
        PlayerCardsAreFightingClientRpc(playerId, leftCardRef, rightCardRef, SendToAllBut(playerId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerHasChosenTheirNodeServerRpc(ulong playerId, Vector3 loc, int nodeEvent)
    {
        PlayerHasChosenANodeClientRpc(playerId, loc, nodeEvent, SendToAllBut(playerId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void HosHasSpawnedASeaCreatureForPlayerServerRpc(ulong playerId, int seaCreature, int player)
    {
        SpawnSeaCreatureForPlayerClientRpc(seaCreature, player, SendToAllBut(playerId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void HostHasSpawnedABeastForPlayerServerRpc(ulong playerId, int beast, int player)
    {
        SpawnBeastForPlayerClientRpc(beast, player, SendToAllBut(playerId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void HostHasSpawnedABeastToLocationServerRpc(ulong playerId, int beast)
    {
        SpawnBeastForLocationClientRpc(beast, SendToAllBut(playerId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void EveryonDrawsACardServerRpc(ulong playerId)
    {
        AllClientsDrawACardClientRpc(playerId, SendToAll());
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendBoardStateToOtherPlayersServerRpc(ulong playerId, string boardState)
    {
        SyncBoardStatesClientRpc(playerId, boardState, SendToAllBut(playerId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerHasDonatedFundsToLongHallServerRpc(ulong playerId, int treasure, int pop, int weapons)
    {
        TellOtherPlayersAboutPlayerLongHallDonationClientRpc(playerId, treasure, pop, weapons, SendToAllBut(playerId));
    }

    //Client RPCs, messages the server tells the clients, these should only be called by the Server.

    [ClientRpc]
    void MessageFromServerClientRpc(ulong fromPlayer, string message, ClientRpcParams sendToClients)
    {
        client.MessageFromOtherPlayer(fromPlayer, message);
    }

    [ClientRpc]
    void ClanChoiceFromServerClientRpc(ulong fromPlayer, int clan, ClientRpcParams sendToClients)
    {
        client.RecieveOtherPlayersClanChoice(fromPlayer, clan);
    }

    [ClientRpc]
    void ReleaseClanChoiceFromServerClientRpc(ulong fromPlayer, int clan, ClientRpcParams sendToClients)
    {
        client.RecieveOtherPlayerUnlockingClan(fromPlayer, clan);
    }

    [ClientRpc]
    void LeaderChoiceFromServerClientRpc(ulong fromPlayer, int leader, ClientRpcParams sendToClients)
    {
        client.RecieveOtherPlayersLeaderChoice(fromPlayer, leader);
    }

    [ClientRpc]
    void ReleaseLeaderChoiceFromServerClientRpc(ulong fromPlayer, int leader, ClientRpcParams sendToClients)
    {
        client.RecieveOtherPlayerUnlockingLeader(fromPlayer, leader);
    }

    [ClientRpc]
    void SendJoinCodeToClientRpc(string message, ClientRpcParams sendToClients)
    {
        client.RecieveJoinCode(message);
    }

    [ClientRpc]
    void CardUpdateFromServerClientRpc(ulong fromPlayer, string message, ClientRpcParams sendToClients)
    {
        client.CardUpdateFromOtherPlayer(fromPlayer, message);
    }

    [ClientRpc]
    void SendNameUpdateToClientRpc(ulong fromPlayer, string message, ClientRpcParams sendToClients)
    {
        client.RecieveNameUpdateFromServer(fromPlayer, message);
    }

    [ClientRpc]
    void PlayerReadyClientRpc(ulong fromPlayer, bool ready, ClientRpcParams sendToClients)
    {
        client.RecieveReadyStatus(fromPlayer, ready);
    }

    [ClientRpc]
    void StartYourGameClientRpc(ulong fromPlayer, string lobbySettings, ClientRpcParams sendToClients)
    {
        client.StartMultiplayerGame(lobbySettings);
    }

    [ClientRpc]
    void StartYourBattleClientRpc(ulong fromPlayer, ClientRpcParams sendToClients)
    {
        client.StartMultiplayerBattle();
    }

    [ClientRpc]
    void HereAreYourMonsterSpawnsClientRpc(ulong fromPlayer, int[] player0MobSpawns, int[] player1MobSpawns, int[] player2MobSpawns, int[] player3MobSpawns, int[] player4MobSpawns, int[] locationMobs, ClientRpcParams sendToClients)
    {
        client.RecieveMonsterSpawnsFromServer(player0MobSpawns, player1MobSpawns, player2MobSpawns, player3MobSpawns, player4MobSpawns, locationMobs);
    }

    [ClientRpc]
    void HereAreYourNodesClientRpc(ulong fromPlayer, int[] nodes, ClientRpcParams sendToClients)
    {
        client.RecieveNodeSelectionFromServer(nodes);
    }

    [ClientRpc]
    void HereIsYourRecruitmentDeckClientRpc(ulong fromPlayer, int[] deckStack, ClientRpcParams sendToClients)
    {
        client.RecieveRecruitmentDeckStackFromServer(deckStack);
    }

    [ClientRpc]
    void PlayerHasBuiltDeckClientRpc(ulong fromPlayer, int[] decklist, ClientRpcParams sendToClients)
    {
        client.BuildOtherPlayersDeck(fromPlayer, decklist);
    }

    [ClientRpc]
    void PlayerHasDrawnACardClientRpc(ulong fromPlayer, int cardRef, ClientRpcParams sendToClients)
    {
        client.AnotherPlayerDrewCard(fromPlayer, cardRef);
    }

    [ClientRpc]
    void PlayerHasChosenANodeClientRpc(ulong fromPlayer, Vector3 loc, int nodeEvent, ClientRpcParams sendToClients)
    {
        client.RecievePlayerNodeChoice(fromPlayer, loc, nodeEvent);
    }

    [ClientRpc]
    void PlayerHasGainedACardClientRpc(ulong fromPlayer, int card, ClientRpcParams sendToClients)
    {
        client.AnotherPlayerHasGainedACard(fromPlayer, card);
    }

    [ClientRpc]
    void PlayerHasNewLeaderClientRpc(ulong fromPlayer, int leader, ClientRpcParams sendToClients)
    {
        client.AnotherPlayerLeaderUpdate(fromPlayer, leader);
    }

    [ClientRpc]
    void PlayerHasAdjustedACardAttackClientRpc(ulong fromPlayer,int card, int amount, ClientRpcParams sendToClients)
    {
        client.AnotherPlayerAdjustCardAttack(fromPlayer, card, amount);
    }

    [ClientRpc]
    void PlayerHasGainedHealthClientRpc(ulong fromPlayer, int amount, ClientRpcParams sendToClients)
    {
        client.AnotherPlayerHasGainedHealth(fromPlayer, amount);
    }

    [ClientRpc]
    void PlayerHasGainedARelicClientRpc(ulong fromPlayer, int relicId, ClientRpcParams sendToClients)
    {
        client.AnotherPlayerHasGainedARelic(fromPlayer, relicId);
    }

    [ClientRpc]
    public void PlayerJoinedTradeEventClientRpc(ulong playerId, ClientRpcParams sendToClients)
    {
        client.AnotherPlayerIsInTradeEvent(playerId);
    }

    [ClientRpc]
    public void PlayerLeftTradeEventClientRpc(ulong playerId, ClientRpcParams sendToClients)
    {
        client.AnotherPlayerLeftTradeEvent(playerId);
    }

    [ClientRpc]
    public void PlayerOfferedCardTradeEventClientRpc(ulong playerId, int cardIndex, ClientRpcParams sendToClients)
    {
        client.AnotherPlayerIsOfferingCardforTrade(playerId, cardIndex);
    }

    [ClientRpc]
    public void PlayerStatusUpdateTradeEventClientRpc(ulong playerId, bool status, int tradePartner, ClientRpcParams sendToClients)
    {
        client.AnotherPlayersTradeStatusUpdate(playerId, status, tradePartner);
    }

    [ClientRpc]
    public void PlayerReplacedACardInTheirDeckClientRpc(ulong playerId, int oldCard, int newCardSpawnIndex, ClientRpcParams sendToClients)
    {
        client.AnotherPlayerReplaceACardInTheirDeck(playerId, oldCard, newCardSpawnIndex);
    }

    [ClientRpc]
    void PlayerCardsAreFightingClientRpc(ulong fromPlayer, int leftCard, int rightCard, ClientRpcParams sendToClients)
    {
        client.AnotherPLayersCardsAreFighting(fromPlayer, leftCard, rightCard);
    }

    [ClientRpc]
    void PlayerHasLostACardClientRpc(ulong fromPlayer, int card, ClientRpcParams sendToClients)
    {
        client.AnotherPlayerHasLostCard(fromPlayer, card);
    }

    [ClientRpc]
    void PlayerHasEndedTurnClientRpc(ulong fromPlayer, ClientRpcParams sendToClients)
    {
        client.TogglePlayerEndTurn(fromPlayer);
    }

    [ClientRpc]
    void AllPlayersHaveEndedTurnClientRpc(ClientRpcParams sendToClients)
    {
        client.AllPlayersHaveEndedTheirTurns();
    }

    [ClientRpc]
    void AllPlayersHaveFinishedTheirNodesClientRpc(ClientRpcParams sendToClients)
    {
        client.AllPlayersHaveFinishedTheirNodes();
    }

    [ClientRpc]
    void SpawnSeaCreatureForPlayerClientRpc(int seaCreature, int player, ClientRpcParams sendToClients)
    {
        client.RecieveSpawnSeaCreatureForPlayer(seaCreature, player);
    }

    [ClientRpc]
    void SpawnBeastForPlayerClientRpc(int beast, int player, ClientRpcParams sendToClients)
    {
        client.RecieveSpawnBeastForPlayer(beast, player);
    }

    [ClientRpc]
    void SpawnBeastForLocationClientRpc(int beast, ClientRpcParams sendToClients)
    {
        client.RecieveSpawnBeastToLocation(beast);
    }

    [ClientRpc]
    void AllClientsDrawACardClientRpc(ulong fromPlayer, ClientRpcParams sendToClients)
    {
        client.AllPlayersDrawACard();
    }

    [ClientRpc]
    void SyncBoardStatesClientRpc(ulong fromPlayer, string boardState, ClientRpcParams sendToClients)
    {
        client.RecieveBoardStateFromServer(boardState, fromPlayer);
    }

    [ClientRpc]
    void TellOtherPlayersAboutPlayerLongHallDonationClientRpc(ulong fromPlayer, int treasure, int pop, int weapons, ClientRpcParams sendToClients)
    {
        client.OtherPlayerDonatedFundsToLongHall((int)fromPlayer, treasure, pop, weapons);
    }

    #region Logic

    void CheckIfAllPlayersHaveEndedTurn(ulong playerId)
    {
        playersEndedTurn.Add(playerId);
        List<ulong> clientIds = new List<ulong>(NetworkManager.ConnectedClientsIds);

        bool answer = true;
        foreach(ulong id in clientIds)
        {
            if (!playersEndedTurn.Contains(id)) { answer = false; }
        }

        if (answer) {
            AllPlayersHaveEndedTurnClientRpc(SendToAll());
            playersEndedTurn.Clear();
        }
    }

    public void GameHasMovedToExploreMode()
    {
        playersEndedTurn.Clear();
    }

    void CheckIfAllPlayersHaveFinishedTheirNode(ulong playerId)
    {
        playersExploredNode.Add(playerId);
        List<ulong> clientIds = new List<ulong>(NetworkManager.ConnectedClientsIds);

        bool answer = true;
        foreach (ulong id in clientIds)
        {
            if (!playersExploredNode.Contains(id)) { answer = false; }
        }

        if (answer)
        {
            AllPlayersHaveFinishedTheirNodesClientRpc(SendToAll());
            playersExploredNode.Clear();
        }
    }

    #endregion

    #region ClientRpcParams

    ClientRpcParams SendToSpecific(ulong id)
    {
        List<ulong> clientIds = new List<ulong>() { id };

        ClientRpcParams clientList = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = clientIds } };

        return clientList;
    }

    ClientRpcParams SendToAllBut(ulong id)
    {
        List<ulong> clientIds = new List<ulong>(NetworkManager.ConnectedClientsIds);
        clientIds.Remove(id);

        ClientRpcParams clientList = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = clientIds } };

        return clientList;
    }

    ClientRpcParams SendToAll()
    {
        List<ulong> clientIds = new List<ulong>(NetworkManager.ConnectedClientsIds);

        ClientRpcParams clientList = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = clientIds } };

        return clientList;
    }

#endregion
}
