using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class GameController : MonoBehaviour
{
    public static GameController main;
    public UIController ui;
    public Lobby lobby;
    public CutsceneController cutsceneController;
    bool playedCutscene;
    bool bossRound;
    int playersReadyForSync;
    bool newTurnSetupComplete;

    public enum Phase { Play, Recruit, Discard, Waiting, Explore }
    public Phase phase;

    public GameObject discardInfoText;
    public GameObject waitingForOtherPlayersInfoText;
    public TextMeshProUGUI buttonText;
    int playerTurn;
    int roundCount;

    [SerializeField] BattleProfile[] battleProfiles;
    BattleProfile currentBattleProfile;

    public BoardState currentBoardState;

    [Header("References")]
    public RecruitmentDeck recruitmentDeck;
    public Canvas menuCanvas;
    public Waves mainWaves;

    public bool waitTurnForSync;

    [Header("Player Count Balances")]
    public int[] baseRecruitsForPlayerCount;

    private void Awake()
    {
        main = this;
    }

    public void MoveFromLobbyToGame()
    {
        cutsceneController.FadeOutOfLobby();
    }

    public void StartSinglePlayerGame()
    {
        lobby.SinglePlayerSetup();
        lobby.gameObject.SetActive(true);
        CameraController.main.MoveCameraToLobby();
    }

    public int GetBaseRecruitsForPlayerCount()
    {
        return baseRecruitsForPlayerCount[MutliplayerController.active.playerCount-1];
    }

    public void LoadIntroCutscene(int profileId)
    {
        if (!MutliplayerController.active.IAmInSomeoneElsesGame())
        {
            Location.active.StartBattle(2);
        }
        cutsceneController.PlayIntroCutsceneIntoNewBattle(profileId);
        mainWaves.RunDeform();

        playedCutscene = true;
    }

    public void LoadRoundIntroCutscene()
    {
        if (!MutliplayerController.active.IAmInSomeoneElsesGame())
        {
            Location.active.StartBattle(3);
        }
        cutsceneController.PlayIntroCutsceneIntoNewRound();

        playedCutscene = true;
    }

    public void LoadBossRoundIntroCutscene()
    {
        Location.active.SetupBossBattle();
        cutsceneController.PlayIntroCutsceneIntoBossRound();

        playedCutscene = true;
    }

    public void AllPlayersGainCoins(int amount)
    {
        foreach(Player p in MutliplayerController.active.AllPlayers())
        {
            p.GainCoins(amount);
        }
    }

    public void StartBattle(int profileId)
    {
        HomeBase.main.CloseAllHomeBaseMenus();
        if (!playedCutscene) { LoadIntroCutscene(profileId); return; }

        foreach(Zones z in MutliplayerController.active.AllPlayerZones())
        {
            z.SetupForGame();
        }

        Zones.main.ClearZones();
        CameraController.main.ResetCamera();
        currentBattleProfile = battleProfiles[profileId];

        StatTracker.local.NewBattle();
        ui.ToggleEndTurnButton(true);
        ui.ToggleCameraUIControls(true);

        Player.active.StartBattle();
        UIController.main.SetupClanDoneUI();

        if (!MutliplayerController.active.IAmInSomeoneElsesGame())
        {
            EnemyDecks.main.StartBattle(currentBattleProfile);
            recruitmentDeck.StartBattle();
        }

        if (MutliplayerController.active.IsMultiplayerGame())
        {
            Fog.main.MoveFogClearingToActivePlayersTurn(playerTurn);
        }

        menuCanvas.gameObject.SetActive(false);
        ActiveZone().ability.TurnOnAbility();
        phase = Phase.Play;
        playedCutscene = false;
    }

    public void StartNewRound()
    {
        roundCount++;
        if(roundCount > 2) { StartBossRound(); return; }

        if (!playedCutscene) { LoadRoundIntroCutscene(); return; }

        Zones.main.ClearAllBackIntoDeck();
        CameraController.main.ResetCamera();
        StatTracker.local.NewRound();
        ui.ToggleEndTurnButton(true);
        ui.ToggleCameraUIControls(true);

        Player.active.StartRound();
        foreach (Player p in MutliplayerController.active.AllPlayers()) { if (p != Player.active) p.GainCoins(3); p.NewTurn(); }

        if (!MutliplayerController.active.IAmInSomeoneElsesGame())
        {
            EnemyDecks.main.StartBattle(currentBattleProfile);
            recruitmentDeck.StartRound();
        }

        if (MutliplayerController.active.IsMultiplayerGame())
        {
            Fog.main.MoveFogClearingToActivePlayersTurn(playerTurn);
        }

        menuCanvas.gameObject.SetActive(false);
        ActiveZone().ability.TurnOnAbility();
        phase = Phase.Play;
        playedCutscene = false;
    }

    public void StartBossRound()
    {
        if (!playedCutscene) { LoadBossRoundIntroCutscene(); return; }

        Zones.main.ClearAllBackIntoDeck();
        CameraController.main.ResetCamera();
        ui.ToggleEndTurnButton(true);
        ui.ToggleCameraUIControls(true);

        Player.active.StartRound();
        foreach (Player p in MutliplayerController.active.AllPlayers()) { if (p != Player.active) p.GainCoins(3); }

        if (!MutliplayerController.active.IAmInSomeoneElsesGame())
        {
            EnemyDecks.main.StartBossBattle();
            recruitmentDeck.StartBossBattle();
        }

        if (MutliplayerController.active.IsMultiplayerGame())
        {
            Fog.main.MoveFogClearingToActivePlayersTurn(playerTurn);
        }

        menuCanvas.gameObject.SetActive(false);
        ActiveZone().ability.TurnOnAbility();
        phase = Phase.Play;
        playedCutscene = false;
        bossRound = true;
    }

    public Zones ActiveZone()
    {
        if (MutliplayerController.active.IsMultiplayerGame())
        {
            return MutliplayerController.active.GetPlayerScript(playerTurn).zones;
        }
        else
        {
            return Zones.main;
        }
    }

    public Player ActivePlayer()
    {
        if (MutliplayerController.active.IsMultiplayerGame())
        {
            return MutliplayerController.active.GetPlayerScript(playerTurn);
        }
        else
        {
            return Player.active;
        }
    }

    public void EndTurnButtonPressed()
    {
        if (InputController.main.Busy()) {
            return;
        }
        PlayerEndPlayPhase();
    }

    public void PlayerEndPlayPhase()
    {
        if (MutliplayerController.active.IsMultiplayerGame())
        {
            if(MutliplayerController.active.myPlayerNumber == playerTurn)
            {
                if (phase == Phase.Play && MutliplayerController.active.LocalPlayer().coins > 0)
                {
                    EnterRecruitPhase();
                    return;
                }
                else if(phase == Phase.Recruit || (phase == Phase.Play && MutliplayerController.active.LocalPlayer().coins < 1))
                {
                    EnterDiscardPhase();
                    return;
                }
                else
                {
                    discardInfoText.gameObject.SetActive(false);
                }
            }

            waitingForOtherPlayersInfoText.SetActive(true);
            phase = Phase.Waiting;
            Client.active.TellServerIHaveFinishedMyTurn();
        }

        else if(phase == Phase.Recruit)
        {
            EnterDiscardPhase();
            return;
        }
        else if(phase == Phase.Discard)
        {
            EndTurn();
            return;
        }
        else
        {
            EnterRecruitPhase();
            return;
        }
    }

    public void EnterRecruitPhase()
    {
        phase = Phase.Recruit;
        Zones.main.recruit.ShowTray();
        buttonText.text = "Done Recruiting";
    }

    public void EnterDiscardPhase()
    {
        Zones.main.recruit.HideTray();
        phase = Phase.Discard;
        discardInfoText.gameObject.SetActive(true);
        buttonText.text = "Done Discarding";
    }

    void MoveActiveZone()
    {
        playerTurn++;
        if(playerTurn >= MutliplayerController.active.playerCount)
        {
            playerTurn = 0;
        }

        Fog.main.MoveFogClearingToActivePlayersTurn(playerTurn);
        CameraController.main.MoveCameraToActiveZone(playerTurn);
    }

    public void AllMonstersDefeated()
    {
        foreach (Zones z in MutliplayerController.active.AllPlayerZones()) { z.ClearAllBackIntoDeck(); }

        if (bossRound)
        {
            int[] loot = MutliplayerController.active.LocalPlayer().LootCount();
            cutsceneController.GameWin(loot);
            MutliplayerController.active.LocalPlayer().storedTreasure = loot[0];
            MutliplayerController.active.LocalPlayer().storedPopulation = loot[1];
            MutliplayerController.active.LocalPlayer().storedWeapons = loot[2];
            return;
        }
        phase = Phase.Explore;
        waitingForOtherPlayersInfoText.SetActive(false);
        if (MutliplayerController.active.IAmHost()) { Client.active.TellServerWeHaveMovedToExploreMode(); }
        Location.active.EnterExploreMode();
    }

    public void EndTurn()
    {
        newTurnSetupComplete = false;
        discardInfoText.gameObject.SetActive(false);
        waitingForOtherPlayersInfoText.SetActive(false);
        buttonText.text = "End Turn";

        ActiveZone().play.EndTurn();
        ActiveZone().monsterArea.EndTurn();
        Zones.main.location.EndTurn();

        foreach (Zones z in MutliplayerController.active.AllPlayerZones()) { z.SweepPlayAreaIntoDiscard(); }

        int toDraw = ActiveZone().player.cardsDrawnAtStartOfTurn - ActiveZone().hand.CardsInHand();
        if (toDraw > 0) { 
            ActiveZone().deck.DrawCards(toDraw);
            if(ActiveZone() != Zones.main) ActiveZone().player.cardsToDrawBeforeReadyForSync = toDraw; //If I am not the one drawing the cards tell the player script to wait until they have recieved the full draw instruction from the owner over the server.
        }
        ActiveZone().leader.RefreshLeader();
        ActiveZone().ability.TurnOffAbility();

        UIController.main.ToggleAllClanTurnDoneUI(false);

        if (MutliplayerController.active.IsMultiplayerGame())
        {
            foreach (Player p in MutliplayerController.active.AllPlayers())
            {
                Debug.Log("Telling " + p.name + " that it's a new turn");
                p.NewTurn(true);
            }
        }
        else
        {
            Player.active.NewTurn();
        }

        phase = Phase.Play;
        MoveActiveZone();
        ActiveZone().ability.TurnOnAbility();       
    }

    public void PlayerReadyForNewTurnSync()
    {
        playersReadyForSync++;
        Debug.Log("Player informed it's ready for sync, players ready to sync: " + playersReadyForSync + "/" + MutliplayerController.active.playerCount);
        if (playersReadyForSync == MutliplayerController.active.playerCount)
        {
            Debug.Log("All players ready for new turn Sync");
            newTurnSetupComplete = true;
            SyncBoardStates();
            playersReadyForSync = 0;
        }
    }

    public void SyncBoardStates()
    {
        if (waitTurnForSync) { StartCoroutine(WaitATurnThenSync()); return; }
        //SYNC BOARD STATES!
        if (MutliplayerController.active.IsMultiplayerGame() && !MutliplayerController.active.IAmInSomeoneElsesGame())
        {
            Debug.Log("Syncing states");
            currentBoardState = new BoardState(Zones.main.recruit, EnemyDecks.main, MutliplayerController.active.OrderedPlayerArray());
            Client.active.SendMyBoardStateToServer(currentBoardState.Stringify());
        }
    }

    public string CurrentBoardState()
    {
        currentBoardState = new BoardState(Zones.main.recruit, EnemyDecks.main, MutliplayerController.active.OrderedPlayerArray());
        return currentBoardState.Stringify();
    }

    int desyncAttemptsCounter;
    private void LateUpdate()
    {
        if (UIController.main.IsDesyncOverlayActive())
        {
            desyncAttemptsCounter++;
            UIController.main.SetDesyncOverlayText(desyncAttemptsCounter.ToString());
            Client.active.RequestServersBoardState();
        }
        else
        {
            desyncAttemptsCounter = 0;
        }
    }

    IEnumerator WaitATurnThenSync()
    {
        yield return null;
        waitTurnForSync = false;
        SyncBoardStates();
    }

    List<string> boardStates = new List<string>();
    List<int> boardStateOwners = new List<int>();

    public void VerifyBoardStatesWhenReady(string boardState, int fromPlayer)
    {
        boardStates.Add(boardState);
        boardStateOwners.Add(fromPlayer);
        if (!newTurnSetupComplete && boardStates.Count == 1) { StartCoroutine(HoldBoardStatesTilReadyToVerify()); }
        else if (newTurnSetupComplete) { VerifyBoardState(boardState, fromPlayer); boardStates.Remove(boardState); }
    }

    IEnumerator HoldBoardStatesTilReadyToVerify()
    {
        Debug.Log("Holding Board States to Verify");
        bool done = false;

        while (!done)
        {
            if (!newTurnSetupComplete) { Debug.Log("Waiting a frame for more board states to come in."); yield return null; }
            else
            {
                Debug.Log("All board states recieved");
                for(int i = 0; i < boardStates.Count; i++) { VerifyBoardState(boardStates[i], boardStateOwners[i]); }
                boardStates.Clear();
                boardStateOwners.Clear();
                done = true;
            }
        }
    }

    public void VerifyBoardState(string stringedServerBoardState, int fromPlayer)
    {
        currentBoardState = new BoardState(Zones.main.recruit, EnemyDecks.main, MutliplayerController.active.OrderedPlayerArray());
        string result = currentBoardState.CompareStringifiedBoardState(stringedServerBoardState);
        Client.active.debug.DebugEvent(result);
        Client.active.SendMessageToOtherPlayers("Player " + fromPlayer + " BOARD SYNC RESULT: " + result);
        if (result != "BOARD STATES MATCH") { UIController.main.ToggleDesyncOverlay(true); DebugUI.main.DesyncEventInfo("Player " + fromPlayer + " BOARD SYNC RESULT: " + result); }
        else { UIController.main.ToggleDesyncOverlay(false);}
    }

    public void LoseGame(Player deadPlayer)
    {
        cutsceneController.GameOver(deadPlayer);
    }
}

[Serializable]
public class BoardState
{
    CardObject[] recruitmentDeckList;
    CardObject recruitmentCardInSlot1, recruitmentCardInSlot2, recruitmentCardInSlot3;
    CardObject[] monsterRefList;

    int playerCount;

    CardObject[] player0DeckList;
    int player0currentHealth;
    Relic[] player0Relics;
    CardObject player0Leader;

    CardObject[] player1DeckList;
    int player1currentHealth;
    Relic[] player1Relics;
    CardObject player1Leader;

    CardObject[] player2DeckList;
    int player2currentHealth;
    Relic[] player2Relics;
    CardObject player2Leader;

    CardObject[] player3DeckList;
    int player3currentHealth;
    Relic[] player3Relics;
    CardObject player3Leader;

    CardObject[] player4DeckList;
    int player4currentHealth;
    Relic[] player4Relics;
    CardObject player4Leader;

    public BoardState(RecruitmentDeck recruit, EnemyDecks monster, Player[] players)
    {
        recruitmentDeckList = recruit.CurrentRecruitmentDeckList();
        recruitmentCardInSlot1 = recruit.GetCardInSlot(1);
        recruitmentCardInSlot2 = recruit.GetCardInSlot(2);
        recruitmentCardInSlot3 = recruit.GetCardInSlot(3);

        monsterRefList = monster.RefArray();

        playerCount = players.Length;

        player0DeckList = players[0].CopyOfPlayerRefList();
        player0currentHealth = players[0].health;
        player0Relics = players[0].relics.ToArray();
        player0Leader = players[0].zones.leader.leaderCard;

        if (players.Length > 1)
        {
            player1DeckList = players[1].CopyOfPlayerRefList();
            player1currentHealth = players[1].health;
            player1Relics = players[1].relics.ToArray();
            player1Leader = players[1].zones.leader.leaderCard;

            if (players.Length > 2)
            {
                player2DeckList = players[2].CopyOfPlayerRefList();
                player2currentHealth = players[2].health;
                player2Relics = players[2].relics.ToArray();
                player2Leader = players[2].zones.leader.leaderCard;

                if (players.Length > 3)
                {
                    player3DeckList = players[3].CopyOfPlayerRefList();
                    player3currentHealth = players[3].health;
                    player3Relics = players[3].relics.ToArray();
                    player3Leader = players[3].zones.leader.leaderCard;

                    if (players.Length > 4)
                    {
                        player4DeckList = players[4].CopyOfPlayerRefList();
                        player4currentHealth = players[4].health;
                        player4Relics = players[4].relics.ToArray();
                        player4Leader = players[4].zones.leader.leaderCard;
                    }
                }
            }
        }
    }

    public string Stringify()
    {
        string s = "";
        //Recruitment
        for(int i = 0; i < recruitmentDeckList.Length; i++)
        {
            s += recruitmentDeckList[i].CardName() + "(" + recruitmentDeckList[i].referenceIndex + "),";
        }

        s += "|";
        //Recruitment Slots
        s += recruitmentCardInSlot1.CardName() + "(" + recruitmentCardInSlot1.referenceIndex + "),";
        s += recruitmentCardInSlot2.CardName() + "(" + recruitmentCardInSlot2.referenceIndex + "),";
        s += recruitmentCardInSlot3.CardName() + "(" + recruitmentCardInSlot3.referenceIndex + "),";

        s += "|";
        //Monster
        for (int i = 0; i < monsterRefList.Length; i++)
        {
            s += monsterRefList[i].CardName() + "(" + monsterRefList[i].referenceIndex + ")/" + monsterRefList[i].ZoneAsString() + ",";
        }

        s += "|";
        //Player Count
        s += playerCount.ToString();

        s += "|";

        //Player 0
        //Player 0 Deck
        for (int i = 0; i < player0DeckList.Length; i++)
        {
            s += player0DeckList[i].CardName() + "(" + player0DeckList[i].referenceIndex + ")" + "/" + player0DeckList[i].ZoneAsString() + ",";
        }

        s += "|";
        //Player 0 health
        s += player0currentHealth.ToString();

        s += "|";
        //Player 0 Relics
        for (int i = 0; i < player0Relics.Length; i++)
        {
            s += player0Relics[i].name + "(" + player0Relics[i].index + "),";
        }

        s += "|";
        //Player 0 Leader
        s += player0Leader.CardName() + "(" + player0Leader.referenceIndex + ")";

        s += "|";

        if(playerCount > 1)
        {
            //Player 1
            //Player 1 Deck
            for (int i = 0; i < player1DeckList.Length; i++)
            {
                s += player1DeckList[i].CardName() + "(" + player1DeckList[i].referenceIndex + ")" + "/" + player1DeckList[i].ZoneAsString() + ",";
            }

            s += "|";
            //Player 1 health
            s += player1currentHealth.ToString();

            s += "|";
            //Player 1 Relics
            for (int i = 0; i < player1Relics.Length; i++)
            {
                s += player1Relics[i].name + "(" + player1Relics[i].index + "),";
            }

            s += "|";
            //Player 1 Leader
            s += player1Leader.CardName() + "(" + player1Leader.referenceIndex + ")";

            s += "|";

            if (playerCount > 2)
            {
                //Player 2
                //Player 2 Deck
                for (int i = 0; i < player2DeckList.Length; i++)
                {
                    s += player2DeckList[i].CardName() + "(" + player2DeckList[i].referenceIndex + ")" + "/" + player2DeckList[i].ZoneAsString() + ",";
                }

                s += "|";
                //Player 2 health
                s += player2currentHealth.ToString();

                s += "|";
                //Player 2 Relics
                for (int i = 0; i < player2Relics.Length; i++)
                {
                    s += player2Relics[i].name + "(" + player2Relics[i].index + "),";
                }

                s += "|";
                //Player 2 Leader
                s += player2Leader.CardName() + "(" + player2Leader.referenceIndex + ")";

                s += "|";

                if (playerCount > 3)
                {
                    //Player 3
                    //Player 3 Deck
                    for (int i = 0; i < player3DeckList.Length; i++)
                    {
                        s += player3DeckList[i].CardName() + "(" + player3DeckList[i].referenceIndex + ")" + "/" + player3DeckList[i].ZoneAsString() + ",";
                    }

                    s += "|";
                    //Player 3 health
                    s += player3currentHealth.ToString();

                    s += "|";
                    //Player 3 Relics
                    for (int i = 0; i < player3Relics.Length; i++)
                    {
                        s += player3Relics[i].name + "(" + player3Relics[i].index + "),";
                    }

                    s += "|";
                    //Player 3 Leader
                    s += player3Leader.CardName() + "(" + player3Leader.referenceIndex + ")";

                    s += "|";

                    if (playerCount > 4)
                    {
                        //Player 4
                        //Player 4 Deck
                        for (int i = 0; i < player4DeckList.Length; i++)
                        {
                            s += player4DeckList[i].CardName() + "(" + player4DeckList[i].referenceIndex + ")" + "/" + player4DeckList[i].ZoneAsString() + ",";
                        }

                        s += "|";
                        //Player 4 health
                        s += player4currentHealth.ToString();

                        s += "|";
                        //Player 4 Relics
                        for (int i = 0; i < player4Relics.Length; i++)
                        {
                            s += player4Relics[i].name + "(" + player4Relics[i].index + "),";
                        }

                        s += "|";
                        //Player 4 Leader
                        s += player4Leader.CardName() + "(" + player4Leader.referenceIndex + ")";

                        s += "|";
                    }
                }
            }
        }

        return s;
    }

    public string CompareStringifiedBoardState(string comparisionString)
    {
        string result = "";

        string myString = Stringify();

        string[] comparisonSections = comparisionString.Split("|");
        string[] mySections = myString.Split("|");

        if(comparisonSections[0] != mySections[0])
        {
            result += "DESYNC! Recruitment deck mismatched ";
            result += "[" + comparisonSections[0] + "] != {" + mySections[0] + "}   ";
        }

        if (comparisonSections[1] != mySections[1])
        {
            result += "DESYNC! Recruitment slots mismatched ";
            result += "[" + comparisonSections[1] + "] != {" + mySections[1] + "}   ";
        }

        if (comparisonSections[2] != mySections[2])
        {
            result += "DESYNC! Monsters mismatched ";
            result += "[" + comparisonSections[2] + "] != {" + mySections[2] + "}   ";
        }

        if (comparisonSections[3] != mySections[3])
        {
            result += "DESYNC! PlayerCount mismatched ";
            result += "[" + comparisonSections[3] + "] != {" + mySections[3] + "}   ";
        }

        if (comparisonSections[4] != mySections[4])
        {
            result += "DESYNC! Player 0 deck mismatched ";
            result += "[" + comparisonSections[4] + "] != {" + mySections[4] + "}   ";
        }

        if(comparisonSections[5] != mySections[5])
        {
            result += "DESYNC! Player 0 health mismatched ";
            result += "[" + comparisonSections[5] + "] != {" + mySections[5] + "}   ";
        }

        if (comparisonSections[6] != mySections[6])
        {
            result += "DESYNC! Player 0 Relics mismatched ";
            result += "[" + comparisonSections[6] + "] != {" + mySections[6] + "}   ";
        }

        if (comparisonSections[7] != mySections[7])
        {
            result += "DESYNC! Player 0 leader mismatched ";
            result += "[" + comparisonSections[7] + "] != {" + mySections[7] + "}   ";
        }

        if(int.Parse(comparisonSections[3]) > 1 && int.Parse(mySections[3]) > 1)
        {
            if (comparisonSections[8] != mySections[8])
            {
                result += "DESYNC! Player 1 deck mismatched ";
                result += "[" + comparisonSections[8] + "] != {" + mySections[8] + "}   ";
            }

            if (comparisonSections[9] != mySections[9])
            {
                result += "DESYNC! Player 1 health mismatched ";
                result += "[" + comparisonSections[9] + "] != {" + mySections[9] + "}   ";
            }

            if (comparisonSections[10] != mySections[10])
            {
                result += "DESYNC! Player 1 Relics mismatched ";
                result += "[" + comparisonSections[10] + "] != {" + mySections[10] + "}   ";
            }

            if (comparisonSections[11] != mySections[11])
            {
                result += "DESYNC! Player 1 leader mismatched ";
                result += "[" + comparisonSections[11] + "] != {" + mySections[11] + "}   ";
            }

            if (int.Parse(comparisonSections[3]) > 2 && int.Parse(mySections[3]) > 2)
            {
                if (comparisonSections[12] != mySections[12])
                {
                    result += "DESYNC! Player 2 deck mismatched ";
                    result += "[" + comparisonSections[12] + "] != {" + mySections[12] + "}   ";
                }

                if (comparisonSections[13] != mySections[13])
                {
                    result += "DESYNC! Player 2 health mismatched ";
                    result += "[" + comparisonSections[13] + "] != {" + mySections[13] + "}   ";
                }

                if (comparisonSections[14] != mySections[14])
                {
                    result += "DESYNC! Player 2 Relics mismatched ";
                    result += "[" + comparisonSections[14] + "] != {" + mySections[14] + "}   ";
                }

                if (comparisonSections[15] != mySections[15])
                {
                    result += "DESYNC! Player 2 leader mismatched ";
                    result += "[" + comparisonSections[15] + "] != {" + mySections[15] + "}   ";
                }

                if (int.Parse(comparisonSections[3]) > 3 && int.Parse(mySections[3]) > 3)
                {
                    if (comparisonSections[16] != mySections[16])
                    {
                        result += "DESYNC! Player 3 deck mismatched ";
                        result += "[" + comparisonSections[16] + "] != {" + mySections[16] + "}   ";
                    }

                    if (comparisonSections[17] != mySections[17])
                    {
                        result += "DESYNC! Player 3 health mismatched ";
                        result += "[" + comparisonSections[17] + "] != {" + mySections[17] + "}   ";
                    }

                    if (comparisonSections[18] != mySections[18])
                    {
                        result += "DESYNC! Player 3 Relics mismatched ";
                        result += "[" + comparisonSections[18] + "] != {" + mySections[18] + "}   ";
                    }

                    if (comparisonSections[19] != mySections[19])
                    {
                        result += "DESYNC! Player 3 leader mismatched ";
                        result += "[" + comparisonSections[19] + "] != {" + mySections[19] + "}   ";
                    }

                    if (int.Parse(comparisonSections[3]) > 4 && int.Parse(mySections[3]) > 4)
                    {
                        if (comparisonSections[20] != mySections[20])
                        {
                            result += "DESYNC! Player 4 deck mismatched ";
                            result += "[" + comparisonSections[20] + "] != {" + mySections[20] + "}   ";
                        }

                        if (comparisonSections[21] != mySections[21])
                        {
                            result += "DESYNC! Player 4 health mismatched ";
                            result += "[" + comparisonSections[21] + "] != {" + mySections[21] + "}   ";
                        }

                        if (comparisonSections[22] != mySections[22])
                        {
                            result += "DESYNC! Player 4 Relics mismatched ";
                            result += "[" + comparisonSections[22] + "] != {" + mySections[22] + "}   ";
                        }

                        if (comparisonSections[23] != mySections[23])
                        {
                            result += "DESYNC! Player 4 leader mismatched ";
                            result += "[" + comparisonSections[23] + "] != {" + mySections[23] + "}   ";
                        }
                    }
                }
            }
        }

        if (result == "") { result = "BOARD STATES MATCH"; }

        return result;
    }
}

[Serializable]
public class BattleProfile
{
    public string name;
    public EnemyDecks.EnemyDeck deck;
    public int[] reservedEnemyID;
}
