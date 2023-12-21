using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LocationEventManager : MonoBehaviour
{
    public Image backingImage;
    private Vector2 backingImageMaxSize = new Vector2(1500f, 1000f);

    public GameObject[] eventHolders;


    Location.NodeEvent activeEvent;
    public float animationDuration;
    bool isFinished;

    public void LoadEvent(Location.NodeEvent nodeEvent, Transform triggeringFigure)
    {
        backingImage.rectTransform.anchoredPosition = Camera.main.WorldToScreenPoint(triggeringFigure.position);
        backingImage.rectTransform.sizeDelta = Vector2.zero;

        activeEvent = nodeEvent;
        StartCoroutine(MoveBackingImageIntoFocus());
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(1) && isFinished)
        {
            StopAllCoroutines();
            FinishEvent();
        }
    }

    void SetupEvent()
    {
        for(int i = 0; i < eventHolders.Length; i++)
        {
            if(i == (int)activeEvent) { eventHolders[i].SetActive(true); }
            else { eventHolders[i].SetActive(false); }
        }

        CameraController.main.StartEventCamera();

        switch (activeEvent)
        {
            case Location.NodeEvent.GainLoot:
                SetupLoot();
                break;
            case Location.NodeEvent.GainRecruit:
                SetupRecruit();
                break;
            case Location.NodeEvent.GainRelic:
                SetupRelic();
                break;
            case Location.NodeEvent.LoseACard:
                SetupLoseACard();
                break;
            case Location.NodeEvent.RepairShip:
                SetupRepair();
                break;
            case Location.NodeEvent.Hunt:
                SetupHunt();
                break;
            case Location.NodeEvent.PowerChallenge:
                SetupPowerChallenge();
                break;
            case Location.NodeEvent.CardSacrifice:
                SetupSacrifice();
                break;
            case Location.NodeEvent.Trade:
                SetupTrade();
                break;
            case Location.NodeEvent.LeaderLevelUp:
                SetupLeader();
                break;
        }
    }

    public void CleanUpForNextNodes()
    {
        friendsPresentToTrade.Clear();
    }

    void FinishEvent()
    {
        CameraController.main.EndEventCamera();

        StartCoroutine(MoveBackingImageOutOfFocus());
    }

    public void CardObjectClicked(CardObject obj)
    {
        switch (activeEvent)
        {
            case Location.NodeEvent.GainLoot:
                TreasureCardClicked(obj);
                break;
            case Location.NodeEvent.GainRecruit:
                RecruitCardClicked(obj);
                break;
            case Location.NodeEvent.LoseACard:
                LoseACardClicked(obj);
                break;
            case Location.NodeEvent.Hunt:
                HuntCardClicked(obj);
                break;
            case Location.NodeEvent.PowerChallenge:
                PowerChallengeRewardChosen(obj);
                break;
            case Location.NodeEvent.CardSacrifice:
                SacrificeCardClicked(obj);
                break;
            case Location.NodeEvent.Trade:
                TradeCardClicked(obj);
                break;
            case Location.NodeEvent.LeaderLevelUp:
                LeaderCardClicked(obj);
                break;
        }
    }

    #region Loot
    [Space(10f)]
    [Header("LootEvent")]
    public SimpleButton lootButton;
    public Transform[] possibleTreasureCardsPositions;
    bool initialLootSetupDone;
    List<GameObject> treasureCardObjects = new List<GameObject>();

    void SetupLoot()
    {
        treasureCardObjects.Clear();

        if (initialLootSetupDone) { return; }

        lootButton.OnButtonPressed += ChestPressed;
        initialLootSetupDone = true;
    }

    public void ChestPressed()
    {
        for (int i = 0; i < possibleTreasureCardsPositions.Length; i++)
        {
            GameObject card = CardArchive.main.SpawnRandomLooseTreasure();
            card.transform.SetParent(possibleTreasureCardsPositions[i].transform);
            card.transform.localPosition = Vector3.zero;
            card.transform.localEulerAngles = Vector3.zero;

            card.GetComponent<CardObject>().zoneScript = Zones.main;

            treasureCardObjects.Add(card);
        }
    }

    void TreasureCardClicked(CardObject obj)
    {
        if (MutliplayerController.active.IsMultiplayerGame())
        {
            Client.active.TellServerIHaveGainedACard(obj.referenceIndex);
        }

        treasureCardObjects.Remove(obj.gameObject);
        Zones.main.deck.AddCardToDeck(obj);
        Player.active.AddCardToPlayerDeckList(obj);

        LootCleanUp();
    }

    void LootCleanUp()
    {
        foreach (GameObject obj in treasureCardObjects)
        {
            Destroy(obj);
        }
        treasureCardObjects.Clear();

        StartCoroutine(FinishAfterTimer());
    }
    #endregion

    #region Recruit
    [Space(10f)]
    [Header("RecruitEvent")]
    public SimpleButton fireButton;
    public Transform[] possibleRecruitCardsPositions;
    bool initialRecruitSetupDone;
    List<GameObject> recruitCardObjects = new List<GameObject>();
    public TextMeshPro recruitEventEndText;

    void SetupRecruit()
    {
        recruitCardObjects.Clear();
        recruitEventEndText.gameObject.SetActive(false);

        if (initialRecruitSetupDone) { return; }

        fireButton.OnButtonPressed += FirePressed;
        initialRecruitSetupDone = true;
    }

    public void FirePressed()
    {
        for (int i = 0; i < possibleRecruitCardsPositions.Length; i++)
        {
            GameObject card = CardArchive.main.SpawnRandomLooseCard();
            card.transform.SetParent(possibleRecruitCardsPositions[i].transform);
            card.transform.localPosition = Vector3.zero;
            card.transform.localEulerAngles = Vector3.zero;

            card.GetComponent<CardObject>().zoneScript = Zones.main;

            recruitCardObjects.Add(card);
        }
    }

    void RecruitCardClicked(CardObject obj)
    {
        if (MutliplayerController.active.IsMultiplayerGame())
        {
            Client.active.TellServerIHaveGainedACard(obj.referenceIndex);
        }

        recruitEventEndText.text = obj.CardName() + " has been added to your deck.";
        recruitEventEndText.gameObject.SetActive(true);
        recruitCardObjects.Remove(obj.gameObject);
        Zones.main.deck.AddCardToDeck(obj);
        Player.active.AddCardToPlayerDeckList(obj);

        RecruitCleanUp();
    }

    void RecruitCleanUp()
    {
        foreach(GameObject obj in recruitCardObjects)
        {
            Destroy(obj);
        }
        recruitCardObjects.Clear();

        StartCoroutine(FinishAfterTimer());
    }
    #endregion

    #region Relic
    [Space(10f)]
    [Header("RelicEvent")]
    public SimpleButton tombButton;
    public Transform[] possibleRelicCards;
    bool initialRelicSetupDone;
    List<Relic> relicsToChooseFrom = new List<Relic>();
    bool relicsShown;
    public TextMeshPro relicEventEndText;

    void SetupRelic()
    {
        relicsToChooseFrom.Clear();
        relicEventEndText.gameObject.SetActive(false);

        foreach(Transform t in possibleRelicCards)
        {
            Relic r = Relics.main.RandomRelic();
            t.GetChild(0).GetChild(0).GetComponent<Image>().sprite = r.sprite;
            t.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = r.name;
            t.GetChild(0).GetChild(2).GetComponent<TextMeshProUGUI>().text = r.description;
            relicsToChooseFrom.Add(r);
            t.gameObject.SetActive(false);
        }

        relicsShown = false;

        if (initialRelicSetupDone) { return; }

        if (possibleRelicCards.Length > 0) { possibleRelicCards[0].GetComponent<SimpleButton>().OnButtonPressed += ChoseRelic1; }
        if (possibleRelicCards.Length > 1) { possibleRelicCards[1].GetComponent<SimpleButton>().OnButtonPressed += ChoseRelic2; }
        if (possibleRelicCards.Length > 2) { possibleRelicCards[2].GetComponent<SimpleButton>().OnButtonPressed += ChoseRelic3; }
        
        tombButton.OnButtonPressed += TombPressed;
        initialRelicSetupDone = true;
    }

    public void TombPressed()
    {
        if (!relicsShown && !isFinished)
        {
            foreach(Transform t in possibleRelicCards)
            {
                t.gameObject.SetActive(true);
            }
            relicsShown = true;
        }
    }

    void ChoseRelic1()
    {
        relicsToChooseFrom[0].ApplyRelic(Player.active);
        if (MutliplayerController.active.IsMultiplayerGame()) { Client.active.TellServerIHaveGainedARelic(relicsToChooseFrom[0].index); }
        relicEventEndText.text = "You have claimed the " + relicsToChooseFrom[0].name + " relic."; 
        RelicCleanUp();
    }

    void ChoseRelic2()
    {
        relicsToChooseFrom[1].ApplyRelic(Player.active);
        if (MutliplayerController.active.IsMultiplayerGame()) { Client.active.TellServerIHaveGainedARelic(relicsToChooseFrom[1].index); }
        relicEventEndText.text = "You have claimed the " + relicsToChooseFrom[1].name + " relic.";
        RelicCleanUp();
    }

    void ChoseRelic3()
    {
        relicsToChooseFrom[2].ApplyRelic(Player.active);
        if (MutliplayerController.active.IsMultiplayerGame()) { Client.active.TellServerIHaveGainedARelic(relicsToChooseFrom[2].index); }
        relicEventEndText.text = "You have claimed the " + relicsToChooseFrom[2].name + " relic.";
        RelicCleanUp();
    }

    void RelicCleanUp()
    {
        relicEventEndText.gameObject.SetActive(true);
        foreach (Transform t in possibleRelicCards)
        {
            t.gameObject.SetActive(false);
        }
        relicsShown = false;
        
        StartCoroutine(FinishAfterTimer());
    }
    #endregion

    #region LoseACard
    [Space(10f)]
    [Header("LoseACardEvent")]
    public SimpleButton settleZoneButton;
    public SimpleButton leftButton, rightButton;
    public Transform cardToLoseHolder;
    bool initialLoseACardSetupDone;
    List<CardObject> cardToLoseCardObjects = new List<CardObject>();
    public float distBetweenCards, cardMoveSpeed, loseACardAnimationSpeed;
    public Vector3 leftCardLimit, rightCardLimit;
    public Vector3 cardToLeavePos;
    bool choiceMade;
    public TextMeshPro loseCardEventEndText;

    void SetupLoseACard()
    {
        cardToLoseCardObjects.Clear();
        loseCardEventEndText.gameObject.SetActive(false);
        leftButton.gameObject.SetActive(false);
        rightButton.gameObject.SetActive(false);
        cardToLoseHolder.gameObject.SetActive(false);
        choiceMade = false;

        if (initialLoseACardSetupDone) { return; }

        settleZoneButton.OnButtonPressed += SettleZonePressed;
        leftButton.OnButtonPressed += MoveCardsRight;
        rightButton.OnButtonPressed += MoveCardsLeft;
        initialLoseACardSetupDone = true;
    }

    void MoveCardsLeft()
    {
        if (cardToLoseCardObjects[cardToLoseCardObjects.Count - 1].transform.localPosition.x <= rightCardLimit.x) { return; }

        foreach (CardObject card in cardToLoseCardObjects)
        {
            card.transform.localPosition = card.transform.localPosition - new Vector3(cardMoveSpeed * Time.deltaTime, 0f, 0f);
        }

        if (cardToLoseCardObjects[cardToLoseCardObjects.Count - 1].transform.localPosition.x <= rightCardLimit.x)
        {
            cardToLoseCardObjects[cardToLoseCardObjects.Count - 1].transform.localPosition = rightCardLimit;
            int count = 1;
            for(int i = cardToLoseCardObjects.Count - 2; i>=0; i--)
            {
                cardToLoseCardObjects[i].transform.localPosition = rightCardLimit - new Vector3(distBetweenCards * count, 0f, 0f);
                count++;
            }
        }
    }

    void MoveCardsRight()
    {
        if (cardToLoseCardObjects[0].transform.localPosition.x == leftCardLimit.x) { return; }

        foreach (CardObject card in cardToLoseCardObjects)
        {
            card.transform.localPosition = card.transform.localPosition + new Vector3(cardMoveSpeed * Time.deltaTime, 0f, 0f);
        }

        if (cardToLoseCardObjects[0].transform.localPosition.x >= leftCardLimit.x)
        {
            cardToLoseCardObjects[0].transform.localPosition = leftCardLimit;
            for (int i = 1; i < cardToLoseCardObjects.Count; i++)
            {
                cardToLoseCardObjects[i].transform.localPosition = leftCardLimit + new Vector3(distBetweenCards * i, 0f, 0f);
            }
        }
    }

    public void SettleZonePressed()
    {
        foreach(CardObject card in Zones.main.deck.cardsInDeck) { cardToLoseCardObjects.Add(card); }
        Zones.main.deck.cardsInDeck.Clear();

        for(int i = 0; i < cardToLoseCardObjects.Count; i++)
        {
            cardToLoseCardObjects[i].transform.SetParent(cardToLoseHolder);
            cardToLoseCardObjects[i].transform.localPosition = leftCardLimit + new Vector3(distBetweenCards * i, 0f, 0f);
            cardToLoseCardObjects[i].transform.localEulerAngles = Vector3.zero;

        }

        leftButton.gameObject.SetActive(true);
        rightButton.gameObject.SetActive(true);
        cardToLoseHolder.gameObject.SetActive(true);
    }

    void LoseACardClicked(CardObject obj)
    {
        if (choiceMade) { return; }
        choiceMade = true;
        StartCoroutine(MoveCardToLeaveLocation(obj.transform));
        loseCardEventEndText.text = obj.CardName() + " has left your deck";
    }

    void LoseACardCleanUp()
    {
        loseCardEventEndText.gameObject.SetActive(true);
        Debug.Log("Cleaning Up lose a Card");
        Zones.main.deck.InstantlyShuffleMultipleCardsIntoDeck(cardToLoseCardObjects);
        cardToLoseCardObjects.Clear();

        StartCoroutine(FinishAfterTimer());
    }

    IEnumerator MoveCardToLeaveLocation(Transform card)
    {
        float timeElapsed = 0f;
        Vector3 start = card.localPosition;

        while(timeElapsed < loseACardAnimationSpeed)
        {
            card.localPosition = Vector3.Lerp(start, cardToLeavePos, timeElapsed / loseACardAnimationSpeed);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        card.localPosition = cardToLeavePos;
        timeElapsed = 0f;
        Vector3 startScale = card.localScale;

        while (timeElapsed < loseACardAnimationSpeed)
        {
            card.localScale = Vector3.Lerp(startScale, Vector3.zero, timeElapsed / loseACardAnimationSpeed);
            card.localEulerAngles = Vector3.Lerp(Vector3.zero, new Vector3(0f, 180f, 0f), timeElapsed / loseACardAnimationSpeed);

            timeElapsed += Time.deltaTime;
            yield return null;
        }
        cardToLoseCardObjects.Remove(card.GetComponent<CardObject>());

        Player.active.RemoveCardFromPlayerDeckList(card.GetComponent<CardObject>());

        LoseACardCleanUp();
    }
    #endregion

    #region Hunt
    [Space(10f)]
    [Header("HuntEvent")]
    public SimpleButton beastTracksButton;
    public SimpleButton huntLeftButton, huntRightButton;
    public Transform cardToHuntHolder;
    bool initialHuntSetupDone;
    List<CardObject> cardToHuntCardObjects = new List<CardObject>();
    public float huntAnimationSpeed;
    public Vector3 cardToHuntPos;
    bool huntChoiceMade;
    public TextMeshPro huntEventEndText;

    void SetupHunt()
    {
        cardToHuntCardObjects.Clear();
        huntEventEndText.gameObject.SetActive(false);
        huntLeftButton.gameObject.SetActive(false);
        huntRightButton.gameObject.SetActive(false);
        cardToHuntHolder.gameObject.SetActive(false);
        huntChoiceMade = false;

        if (initialHuntSetupDone) { return; }

        beastTracksButton.OnButtonPressed += TrackZonePressed;
        huntLeftButton.OnButtonPressed += MoveHuntCardsRight;
        huntRightButton.OnButtonPressed += MoveHuntCardsLeft;
        initialHuntSetupDone = true;
    }

    void MoveHuntCardsLeft()
    {
        if (cardToHuntCardObjects[cardToHuntCardObjects.Count - 1].transform.localPosition.x <= rightCardLimit.x) { return; }

        foreach (CardObject card in cardToHuntCardObjects)
        {
            card.transform.localPosition = card.transform.localPosition - new Vector3(cardMoveSpeed * Time.deltaTime, 0f, 0f);
        }

        if (cardToHuntCardObjects[cardToHuntCardObjects.Count - 1].transform.localPosition.x <= rightCardLimit.x)
        {
            cardToHuntCardObjects[cardToHuntCardObjects.Count - 1].transform.localPosition = rightCardLimit;
            int count = 1;
            for (int i = cardToHuntCardObjects.Count - 2; i >= 0; i--)
            {
                cardToHuntCardObjects[i].transform.localPosition = rightCardLimit - new Vector3(distBetweenCards * count, 0f, 0f);
                count++;
            }
        }
    }

    void MoveHuntCardsRight()
    {
        if (cardToHuntCardObjects[0].transform.localPosition.x == leftCardLimit.x) { return; }

        foreach (CardObject card in cardToHuntCardObjects)
        {
            card.transform.localPosition = card.transform.localPosition + new Vector3(cardMoveSpeed * Time.deltaTime, 0f, 0f);
        }

        if (cardToHuntCardObjects[0].transform.localPosition.x >= leftCardLimit.x)
        {
            cardToHuntCardObjects[0].transform.localPosition = leftCardLimit;
            for (int i = 1; i < cardToHuntCardObjects.Count; i++)
            {
                cardToHuntCardObjects[i].transform.localPosition = leftCardLimit + new Vector3(distBetweenCards * i, 0f, 0f);
            }
        }
    }

    public void TrackZonePressed()
    {
        foreach (CardObject card in Zones.main.deck.cardsInDeck)
        {
            if (card.myCard.tag == Card.Tag.Person)
            {
                cardToHuntCardObjects.Add(card);
            }
        }
        foreach (CardObject card in cardToHuntCardObjects) { Zones.main.deck.cardsInDeck.Remove(card); }

        for (int i = 0; i < cardToHuntCardObjects.Count; i++)
        {
            cardToHuntCardObjects[i].transform.SetParent(cardToHuntHolder);
            cardToHuntCardObjects[i].transform.localPosition = leftCardLimit + new Vector3(distBetweenCards * i, 0f, 0f);
            cardToHuntCardObjects[i].transform.localEulerAngles = Vector3.zero;

        }

        huntLeftButton.gameObject.SetActive(true);
        huntRightButton.gameObject.SetActive(true);
        cardToHuntHolder.gameObject.SetActive(true);
    }

    void HuntCardClicked(CardObject obj)
    {
        if (huntChoiceMade) { return; }
        huntChoiceMade = true;
        huntEventEndText.text = obj.CardName();
        StartCoroutine(MoveCardToHuntLocation(obj.transform));
    }

    void HuntCleanUp()
    {
        huntEventEndText.gameObject.SetActive(true);
        Zones.main.deck.InstantlyShuffleMultipleCardsIntoDeck(cardToHuntCardObjects);
        cardToHuntCardObjects.Clear();

        StartCoroutine(FinishAfterTimer());
    }

    IEnumerator MoveCardToHuntLocation(Transform card)
    {
        float timeElapsed = 0f;
        Vector3 start = card.localPosition;

        while (timeElapsed < huntAnimationSpeed)
        {
            card.localPosition = Vector3.Lerp(start, cardToHuntPos, timeElapsed / huntAnimationSpeed);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        card.localPosition = cardToHuntPos;
        timeElapsed = 0f;
        Vector3 startScale = card.localScale;

        //20% chance to lose the card
        if (Random.Range(0f, 1f) < 0.2f)
        {
            while (timeElapsed < huntAnimationSpeed)
            {
                card.localScale = Vector3.Lerp(startScale, Vector3.zero, timeElapsed / huntAnimationSpeed);
                card.localEulerAngles = Vector3.Lerp(Vector3.zero, new Vector3(0f, 180f, 0f), timeElapsed / huntAnimationSpeed);

                timeElapsed += Time.deltaTime;
                yield return null;
            }
            cardToHuntCardObjects.Remove(card.GetComponent<CardObject>());

            Player.active.RemoveCardFromPlayerDeckList(card.GetComponent<CardObject>());
            Destroy(card.gameObject);
            huntEventEndText.text += " has been eaten by the beast and is no longer in your deck!";
        }
        else
        {
            card.GetComponent<CardObject>().AdjustAttackMod(2);
            huntEventEndText.text += " has succeeded their hunt and gained 2 strength from the experience!";
        }


        yield return new WaitForSeconds(huntAnimationSpeed);

        HuntCleanUp();
    }
    #endregion

    #region RepairShip
    [Space(10f)]
    [Header("RepairEvent")]
    public SimpleButton repairButton;
    public Transform[] possibleRepairCardsPositions;
    public int repairStrength;
    bool initialRepairSetupDone;
    List<GameObject> repairCardObjects = new List<GameObject>();
    public TextMeshPro repairEventEndText;

    void SetupRepair()
    {
        repairCardObjects.Clear();
        repairEventEndText.gameObject.SetActive(false);

        if (initialRepairSetupDone) { return; }

        repairButton.OnButtonPressed += RepairPressed;
        initialRepairSetupDone = true;
    }

    public void RepairPressed()
    {
        Player.active.GainHealth(repairStrength);
        if (MutliplayerController.active.IsMultiplayerGame()) { Client.active.TellServerIHaveGainedHealth(repairStrength); }
        repairEventEndText.text = "Your men used the materials to repair upto " + repairStrength + " points of damage to your ship.";
        RepairCleanUp();
    }

    void RepairCleanUp()
    {
        repairEventEndText.gameObject.SetActive(true);
        foreach (GameObject obj in recruitCardObjects)
        {
            Destroy(obj);
        }
        recruitCardObjects.Clear();

        StartCoroutine(FinishAfterTimer());
    }
    #endregion

    #region PowerChallenge
    [Space(10f)]
    [Header("PowerChallengeEvent")]
    public SimpleButton hitAndRunButton;
    public SimpleButton attackButton, challengeLeaderButton;
    public Transform challengeCardPos1, challengeCardPos2, challengeCardPos3;
    bool initialPowerChallengeSetupDone;
    List<CardObject> cardsForChallenge = new List<CardObject>();
    public float powerChallengeAnimationSpeed, timeBetweenPowerChallengeCards;
    public Vector3 powerChallengeCardSpawnPos;
    public TextMeshPro powerChallengeProgressText;
    public GameObject powerChallengeRewardObj, powerChallengeRelicReward;
    public Transform powerChallengeRewardHolderLeft, powerChallengeRewardHolderCentre, powerChallengeRewardHolderRight;
    List<GameObject> rewardObjects = new List<GameObject>();
    Relic powerChallengeRelic;
    int challengeThreshold;
    int currentPower;
    public TextMeshPro challengeEventEndText;

    void SetupPowerChallenge()
    {
        cardsForChallenge.Clear();
        challengeEventEndText.gameObject.SetActive(false);
        hitAndRunButton.gameObject.SetActive(true);
        attackButton.gameObject.SetActive(true);
        challengeLeaderButton.gameObject.SetActive(true);

        powerChallengeProgressText.gameObject.SetActive(false);
        powerChallengeRewardObj.gameObject.SetActive(false);
        powerChallengeRelicReward.SetActive(false);
        challengeThreshold = 0;
        currentPower = 0;

        if (initialPowerChallengeSetupDone) { return; }

        hitAndRunButton.OnButtonPressed += SetThreholdTo3;
        attackButton.OnButtonPressed += SetThreholdTo6;
        challengeLeaderButton.OnButtonPressed += SetThreholdTo9;
        powerChallengeRelicReward.GetComponent<SimpleButton>().OnButtonPressed += PowerChallengeRelicRewardClicked;
        initialPowerChallengeSetupDone = true;
    }

    void SetThreholdTo3()
    {
        challengeThreshold = 3;
        ThresholdChosen();
    }

    void SetThreholdTo6()
    {
        challengeThreshold = 6;
        ThresholdChosen();
    }

    void SetThreholdTo9()
    {
        challengeThreshold = 9;
        ThresholdChosen();
    }

    public void ThresholdChosen()
    {
        powerChallengeProgressText.gameObject.SetActive(true);
        UpdatePowerChallengeText();

        hitAndRunButton.gameObject.SetActive(false);
        attackButton.gameObject.SetActive(false);
        challengeLeaderButton.gameObject.SetActive(false);

        StartCoroutine(DrawThenFlip3Card());
    }

    void UpdatePowerChallengeText()
    {
        powerChallengeProgressText.text = "Your Power: " + currentPower + "\nTheir Power: " + challengeThreshold;
    }

    void CheckIfPowerChallengePassed()
    {
        if(currentPower >= challengeThreshold)
        {
            switch (challengeThreshold)
            {
                case 3:
                    SetUpTreasureReward();
                    break;
                case 6:
                    SetUpCardReward();
                    break;
                case 9:
                    SetUpRelicReward();
                    break;
            }
        }
        else
        {
            PowerThresholdCleanUp();
        }
    }

    void SetUpTreasureReward()
    {
        powerChallengeRewardObj.gameObject.SetActive(true);
        powerChallengeRelicReward.SetActive(false);

        GameObject a = CardArchive.main.SpawnRandomLooseTreasure();
        a.transform.SetParent(powerChallengeRewardHolderLeft);
        a.transform.localPosition = Vector3.zero;
        a.transform.localEulerAngles = Vector3.zero;
        a.GetComponent<CardObject>().zoneScript = Zones.main;
        rewardObjects.Add(a);

        GameObject b = CardArchive.main.SpawnRandomLooseTreasure();
        b.transform.SetParent(powerChallengeRewardHolderCentre);
        b.transform.localPosition = Vector3.zero;
        b.transform.localEulerAngles = Vector3.zero;
        b.GetComponent<CardObject>().zoneScript = Zones.main;
        rewardObjects.Add(b);

        GameObject c = CardArchive.main.SpawnRandomLooseTreasure();
        c.transform.SetParent(powerChallengeRewardHolderRight);
        c.transform.localPosition = Vector3.zero;
        c.transform.localEulerAngles = Vector3.zero;
        c.GetComponent<CardObject>().zoneScript = Zones.main;
        rewardObjects.Add(c);
    }

    void SetUpCardReward()
    {
        powerChallengeRewardObj.gameObject.SetActive(true);
        powerChallengeRelicReward.SetActive(false);

        GameObject a = CardArchive.main.SpawnRandomLooseCard();
        a.transform.SetParent(powerChallengeRewardHolderLeft);
        a.transform.localPosition = Vector3.zero;
        a.transform.localEulerAngles = Vector3.zero;
        a.GetComponent<CardObject>().zoneScript = Zones.main;
        rewardObjects.Add(a);

        GameObject b = CardArchive.main.SpawnRandomLooseCard();
        b.transform.SetParent(powerChallengeRewardHolderCentre);
        b.transform.localPosition = Vector3.zero;
        b.transform.localEulerAngles = Vector3.zero;
        b.GetComponent<CardObject>().zoneScript = Zones.main;
        rewardObjects.Add(b);

        GameObject c = CardArchive.main.SpawnRandomLooseCard();
        c.transform.SetParent(powerChallengeRewardHolderRight);
        c.transform.localPosition = Vector3.zero;
        c.transform.localEulerAngles = Vector3.zero;
        c.GetComponent<CardObject>().zoneScript = Zones.main;
        rewardObjects.Add(c);
    }

    void SetUpRelicReward()
    {
        powerChallengeRewardObj.gameObject.SetActive(true);
        powerChallengeRelicReward.SetActive(true);

        Relic r = Relics.main.RandomRelic();
        powerChallengeRelicReward.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = r.sprite;
        powerChallengeRelicReward.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = r.name;
        powerChallengeRelicReward.transform.GetChild(0).GetChild(2).GetComponent<TextMeshProUGUI>().text = r.description;
        powerChallengeRelic = r;

    }

    void PowerChallengeRelicRewardClicked()
    {
        powerChallengeRelic.ApplyRelic(Player.active);
        if (MutliplayerController.active.IsMultiplayerGame()) { Client.active.TellServerIHaveGainedARelic(powerChallengeRelic.index); }
        powerChallengeRelic = null;
        challengeEventEndText.text = "You succeeded the challenge and claimed the " + powerChallengeRelic.name + " relic.";
        PowerChallengeRewardComplete();
    }

    void PowerChallengeRewardChosen(CardObject card)
    {
        Player.active.AddCardToPlayerDeckList(card);
        rewardObjects.Remove(card.gameObject);
        card.transform.SetParent(Zones.main.deck.transform);
        Zones.main.deck.InstantlyShuffleCardIntoDeck(card);

        challengeEventEndText.text = "You succeeded the challenge and have added " + card.CardName() + " to your deck.";
        PowerChallengeRewardComplete();
    }

    void PowerChallengeRewardComplete()
    {
        PowerThresholdCleanUp();
    }

    void PowerThresholdCleanUp()
    {
        challengeEventEndText.gameObject.SetActive(true);
        Zones.main.deck.InstantlyShuffleMultipleCardsIntoDeck(cardsForChallenge);
        cardsForChallenge.Clear();

        foreach(GameObject obj in rewardObjects)
        {
            Destroy(obj);
        }
        rewardObjects.Clear();

        StartCoroutine(FinishAfterTimer());
    }

    IEnumerator DrawThenFlip3Card()
    {
        for(int i = 0; i < 3; i++)
        {
            CardObject cardFromDeck = Zones.main.deck.RandomCardFromDeck();
            cardFromDeck.transform.SetParent(eventHolders[(int)activeEvent].transform);
            cardFromDeck.transform.localPosition = powerChallengeCardSpawnPos;

            cardsForChallenge.Add(cardFromDeck);
        }

        cardsForChallenge[0].transform.SetParent(challengeCardPos1.transform);
        cardsForChallenge[0].transform.localEulerAngles = new Vector3(0f, -180f, 0f);
        cardsForChallenge[1].transform.SetParent(challengeCardPos2.transform);
        cardsForChallenge[1].transform.localEulerAngles = new Vector3(0f, -180f, 0f);
        cardsForChallenge[2].transform.SetParent(challengeCardPos3.transform);
        cardsForChallenge[2].transform.localEulerAngles = new Vector3(0f, -180f, 0f);

        StartCoroutine(MoveCardsToPowerZone(cardsForChallenge[0].transform, challengeCardPos1));
        yield return new WaitForSeconds(timeBetweenPowerChallengeCards);

        StartCoroutine(MoveCardsToPowerZone(cardsForChallenge[1].transform, challengeCardPos2));
        yield return new WaitForSeconds(timeBetweenPowerChallengeCards);

        StartCoroutine(MoveCardsToPowerZone(cardsForChallenge[2].transform, challengeCardPos3));
        yield return new WaitForSeconds(timeBetweenPowerChallengeCards);

        yield return new WaitForSeconds(powerChallengeAnimationSpeed);

        StartCoroutine(FlipCard(cardsForChallenge[0].transform));
        yield return new WaitForSeconds(timeBetweenPowerChallengeCards);

        StartCoroutine(FlipCard(cardsForChallenge[1].transform));
        yield return new WaitForSeconds(timeBetweenPowerChallengeCards);

        StartCoroutine(FlipCard(cardsForChallenge[2].transform));
        yield return new WaitForSeconds(timeBetweenPowerChallengeCards);
        
        yield return new WaitForSeconds(powerChallengeAnimationSpeed);

        CheckIfPowerChallengePassed();
    }

    IEnumerator FlipCard(Transform card)
    {
        float timeElapsed = 0f;
        Vector3 startRot = card.localEulerAngles;
        Vector3 endRot = Vector3.zero;

        while (timeElapsed < powerChallengeAnimationSpeed)
        {
            card.localEulerAngles = Vector3.Lerp(startRot, endRot, timeElapsed / powerChallengeAnimationSpeed);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        card.localEulerAngles = endRot;
        currentPower += card.GetComponent<CardObject>().AttackValue();
        UpdatePowerChallengeText();
    }

    IEnumerator MoveCardsToPowerZone(Transform card, Transform zone)
    {
        float timeElapsed = 0f;
        Vector3 start = card.localPosition;
        Vector3 end = zone.localPosition;

        while (timeElapsed < powerChallengeAnimationSpeed)
        {
            card.localPosition = Vector3.Lerp(start, end, timeElapsed / powerChallengeAnimationSpeed);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        card.localPosition = end;
    }
    #endregion

    #region CardSacrifice
    [Space(10f)]
    [Header("CardSacrificeEvent")]
    public SimpleButton leftCombatantButton;
    public SimpleButton rightCombatantButton, leftCardSelectButton, rightCardSelectButton;
    public Transform combatentsCardsToChooseFromHolder;
    bool initialCardSacrificeSetupDone;
    List<CardObject> cardToSendToFightCardObjects = new List<CardObject>();
    public float sacrificeAnimationSpeed;
    public Vector3 leftCardFightPos, rightCardFightPos;
    int sacrificeChoiceMode; //0 is no, 1 is chosing for left, 2 is chosing for right
    CardObject leftFighter, rightFighter;
    public TextMeshPro sacrificeEventEndText;

    void SetupSacrifice()
    {
        cardToSendToFightCardObjects.Clear();
        sacrificeEventEndText.gameObject.SetActive(false);
        leftCardSelectButton.gameObject.SetActive(false);
        rightCardSelectButton.gameObject.SetActive(false);
        combatentsCardsToChooseFromHolder.gameObject.SetActive(false);
        sacrificeChoiceMode = 0;

        if (initialCardSacrificeSetupDone) { return; }
        
        leftCardSelectButton.OnButtonPressed += MoveSacrificeCardsLeft;
        rightCardSelectButton.OnButtonPressed += MoveSacrificeCardsRight;
        leftCombatantButton.OnButtonPressed += LeftFighterZonePressed;
        rightCombatantButton.OnButtonPressed += RightFighterZonePressed;
        initialCardSacrificeSetupDone = true;
    }

    void MoveSacrificeCardsLeft()
    {
        if (cardToSendToFightCardObjects[cardToSendToFightCardObjects.Count - 1].transform.localPosition.x <= rightCardLimit.x) { return; }

        foreach (CardObject card in cardToSendToFightCardObjects)
        {
            card.transform.localPosition = card.transform.localPosition - new Vector3(cardMoveSpeed * Time.deltaTime, 0f, 0f);
        }

        if (cardToSendToFightCardObjects[cardToSendToFightCardObjects.Count - 1].transform.localPosition.x <= rightCardLimit.x)
        {
            cardToSendToFightCardObjects[cardToSendToFightCardObjects.Count - 1].transform.localPosition = rightCardLimit;
            int count = 1;
            for (int i = cardToSendToFightCardObjects.Count - 2; i >= 0; i--)
            {
                cardToSendToFightCardObjects[i].transform.localPosition = rightCardLimit - new Vector3(distBetweenCards * count, 0f, 0f);
                count++;
            }
        }
    }

    void MoveSacrificeCardsRight()
    {
        if (cardToSendToFightCardObjects[0].transform.localPosition.x == leftCardLimit.x) { return; }

        foreach (CardObject card in cardToSendToFightCardObjects)
        {
            card.transform.localPosition = card.transform.localPosition + new Vector3(cardMoveSpeed * Time.deltaTime, 0f, 0f);
        }

        if (cardToSendToFightCardObjects[0].transform.localPosition.x >= leftCardLimit.x)
        {
            cardToSendToFightCardObjects[0].transform.localPosition = leftCardLimit;
            for (int i = 1; i < cardToSendToFightCardObjects.Count; i++)
            {
                cardToSendToFightCardObjects[i].transform.localPosition = leftCardLimit + new Vector3(distBetweenCards * i, 0f, 0f);
            }
        }
    }

    public void LeftFighterZonePressed()
    {
        if (sacrificeChoiceMode == 0 && leftFighter == null)
        {
            foreach (CardObject card in Zones.main.deck.cardsInDeck)
            {
                if (card.myCard.tag == Card.Tag.Person)
                {
                    cardToSendToFightCardObjects.Add(card);
                }
            }
            foreach(CardObject card in cardToSendToFightCardObjects) { Zones.main.deck.cardsInDeck.Remove(card); }

            for (int i = 0; i < cardToSendToFightCardObjects.Count; i++)
            {
                cardToSendToFightCardObjects[i].transform.SetParent(combatentsCardsToChooseFromHolder);
                cardToSendToFightCardObjects[i].transform.localPosition = leftCardLimit + new Vector3(distBetweenCards * i, 0f, 0f);
                cardToSendToFightCardObjects[i].transform.localEulerAngles = Vector3.zero;

            }

            leftCardSelectButton.gameObject.SetActive(true);
            rightCardSelectButton.gameObject.SetActive(true);
            combatentsCardsToChooseFromHolder.gameObject.SetActive(true);

            sacrificeChoiceMode = 1;
        }
    }

    public void RightFighterZonePressed()
    {
        if (sacrificeChoiceMode == 0 && rightFighter == null)
        {
            foreach (CardObject card in Zones.main.deck.cardsInDeck)
            {
                if (card.myCard.tag == Card.Tag.Person)
                {
                    cardToSendToFightCardObjects.Add(card);
                }
            }
            foreach (CardObject card in cardToSendToFightCardObjects) { Zones.main.deck.cardsInDeck.Remove(card); }

            for (int i = 0; i < cardToSendToFightCardObjects.Count; i++)
            {
                cardToSendToFightCardObjects[i].transform.SetParent(combatentsCardsToChooseFromHolder);
                cardToSendToFightCardObjects[i].transform.localPosition = leftCardLimit + new Vector3(distBetweenCards * i, 0f, 0f);
                cardToSendToFightCardObjects[i].transform.localEulerAngles = Vector3.zero;

            }

            leftCardSelectButton.gameObject.SetActive(true);
            rightCardSelectButton.gameObject.SetActive(true);
            combatentsCardsToChooseFromHolder.gameObject.SetActive(true);

            sacrificeChoiceMode = 2;
        }
    }

    void SacrificeCardClicked(CardObject obj)
    {
        switch (sacrificeChoiceMode)
        {
            default:
                Debug.Log("ERROR chosing a card when not ready for it!!!!");
                break;
            case 1:
                if(obj == rightFighter) { return; }
                cardToSendToFightCardObjects.Remove(obj);
                leftFighter = obj;
                leftFighter.transform.SetParent(transform);
                StartCoroutine(MoveCardToFightLocation(obj.transform, leftCardFightPos));

                Zones.main.deck.InstantlyShuffleMultipleCardsIntoDeck(cardToSendToFightCardObjects);
                cardToSendToFightCardObjects.Clear();

                leftCardSelectButton.gameObject.SetActive(false);
                rightCardSelectButton.gameObject.SetActive(false);
                combatentsCardsToChooseFromHolder.gameObject.SetActive(false);
                sacrificeChoiceMode = 0;
                break;
            case 2:
                if (obj == leftFighter) { return; }
                cardToSendToFightCardObjects.Remove(obj);
                rightFighter = obj;
                rightFighter.transform.SetParent(transform);
                StartCoroutine(MoveCardToFightLocation(obj.transform, rightCardFightPos));

                Zones.main.deck.InstantlyShuffleMultipleCardsIntoDeck(cardToSendToFightCardObjects);
                cardToSendToFightCardObjects.Clear();

                leftCardSelectButton.gameObject.SetActive(false);
                rightCardSelectButton.gameObject.SetActive(false);
                combatentsCardsToChooseFromHolder.gameObject.SetActive(false);
                sacrificeChoiceMode = 0;
                break;

        }

        CheckFightersArePrepped();
    }

    void CheckFightersArePrepped()
    {
        if(leftFighter != null && rightFighter != null)
        {
            Transform winner = leftFighter.transform;
            if(leftFighter.AttackValue() < rightFighter.AttackValue()) { winner = rightFighter.transform; }
            Transform loser = rightFighter.transform;
            if (winner == rightFighter.transform) { loser = leftFighter.transform; }

            StartCoroutine(AnimateFight(winner, loser));
            if (MutliplayerController.active.IsMultiplayerGame()) { Client.active.TellServerMyCardAreFighting(leftFighter, rightFighter); }
        }
    }

    void SacrificeCleanUp()
    {
        sacrificeEventEndText.gameObject.SetActive(true);
        Zones.main.deck.InstantlyShuffleMultipleCardsIntoDeck(cardToSendToFightCardObjects);
        cardToSendToFightCardObjects.Clear();

        StartCoroutine(FinishAfterTimer());
    }

    IEnumerator AnimateFight(Transform winner, Transform loser)
    {
        yield return new WaitForSeconds(sacrificeAnimationSpeed);

        Vector3 midPoint = Vector3.Lerp(winner.localPosition, loser.localPosition, 0.5f);
        Vector3 leftPoint = new Vector3(midPoint.x -0.5f, midPoint.y, midPoint.z);
        Vector3 rightPoint = new Vector3(midPoint.x + 0.5f, midPoint.y, midPoint.z);

        Transform left = winner;
        if(loser.localPosition.x < winner.localPosition.x) { left = loser; }
        Transform right = loser;
        if(left == loser) { right = winner; }

        Vector3 leftStart = left.localPosition;
        Vector3 rightStart = right.localPosition;

        float timeElapsed = 0f;
        float firstStage = (sacrificeAnimationSpeed / 3f) * 2f;
        float secondStage = sacrificeAnimationSpeed * 2f;

        while(timeElapsed < firstStage)
        {
            left.localPosition = Vector3.Lerp(leftStart, leftPoint, timeElapsed / firstStage);
            right.localPosition = Vector3.Lerp(rightStart, rightPoint, timeElapsed / firstStage);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        Vector3 leftReboundPoint = Vector3.Lerp(leftStart, leftPoint, 0.2f);
        Vector3 rightReboundPoint = Vector3.Lerp(rightStart, rightPoint, 0.2f);

        while (timeElapsed < secondStage)
        {
            left.localPosition = Vector3.Lerp(leftPoint, leftReboundPoint, timeElapsed / secondStage);
            right.localPosition = Vector3.Lerp(rightPoint, rightReboundPoint, timeElapsed / secondStage);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        timeElapsed = 0f;
        Vector3 startScale = loser.localScale;
        Vector3 endScale = Vector3.zero;


        CardObject loserCard = loser.GetComponent<CardObject>();
        CardObject winnerCard = winner.GetComponent<CardObject>();

        winnerCard.extraEffects.AddRange(loserCard.myCard.cardEffects);
        winnerCard.cardText.text += loserCard.cardText.text;
        winnerCard.cardLootValues[0] += loserCard.cardLootValues[0];
        winnerCard.cardLootValues[1] += loserCard.cardLootValues[1];
        winnerCard.cardLootValues[2] += loserCard.cardLootValues[2];

        sacrificeEventEndText.text = winnerCard.CardName() + " won the contest and claimed the power of " + loserCard.CardName() + " for their own. \n" + loserCard.CardName() + " has been removed form your deck.";

        while (timeElapsed < sacrificeAnimationSpeed)
        {
            loser.localScale = Vector3.Lerp(startScale, endScale, timeElapsed / sacrificeAnimationSpeed);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        Zones.main.deck.InstantlyShuffleCardIntoDeck(winnerCard);

        Player.active.RemoveCardFromPlayerDeckList(loserCard, false);

        leftFighter = null;
        rightFighter = null;

        SacrificeCleanUp();
    }

    public void SimulateFightForMultiplayerUpdate(CardObject left, CardObject right, Player player)
    {
        CardObject winner = left;
        if(right.AttackValue() > left.AttackValue())
        {
            winner = right;
        }
        CardObject loser = right;
        if(winner == right) { loser = left; }


        winner.extraEffects.AddRange(loser.myCard.cardEffects);
        winner.cardText.text += loser.cardText.text;
        winner.cardLootValues[0] += loser.cardLootValues[0];
        winner.cardLootValues[1] += loser.cardLootValues[1];
        winner.cardLootValues[2] += loser.cardLootValues[2];

        player.RemoveCardFromPlayerDeckList(loser, false);
    }

    IEnumerator MoveCardToFightLocation(Transform card, Vector3 location)
    {
        float timeElapsed = 0f;
        Vector3 start = card.localPosition;

        while (timeElapsed < sacrificeAnimationSpeed)
        {
            card.localPosition = Vector3.Lerp(start, location, timeElapsed / sacrificeAnimationSpeed);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        card.localPosition = location;
    }
    #endregion

    #region Trade
    [Space(10f)]
    [Header("TradeEvent")]
    public SimpleButton tradeButton;
    public SimpleButton leftTradeButton, rightTradeButton;
    public GameObject[] playerClanFigures;
    public GameObject[] friendTradeReadyLight;
    public GameObject readyToTradeLight;
    public SimpleButton[] tradePartnerButtons;
    public SimpleButton npcTradePartnerButton;
    public Vector3 activePartnerLocation;
    public Transform cardToTradeHolder;
    bool initialTradeSetupDone;
    List<CardObject> cardToTradeCardObjects = new List<CardObject>();
    public float tradeAnimationSpeed;
    public CardObject cardToTrade, cardToRecieve;
    public Transform cardToGivePos, cardToRecievePos;
    int tradeStage = 0;
    [SerializeField] List<Player> friendsPresentToTrade = new List<Player>();
    int[] indexOfCardsOfferedByFriends = new int[4] { -1, - 1, -1, -1 };
    int chosenTradePartner;
    bool readyToTrade, committedToTrade;
    bool[] friendReadyStatus = new bool[4] { false, false, false, false };
    int[] friendTradePartner = new int[4] { -1, -1, -1, -1 };
    public TextMeshPro tradeEventEndText;

    void SetupTrade()
    {
        cardToTradeCardObjects.Clear();
        tradeEventEndText.gameObject.SetActive(false);
        leftTradeButton.gameObject.SetActive(false);
        rightTradeButton.gameObject.SetActive(false);
        cardToTradeHolder.gameObject.SetActive(false);

        for(int i = 0; i < playerClanFigures.Length; i++)
        {
            if(i == (int)Player.active.clan) { playerClanFigures[i].SetActive(true); }
            else{ playerClanFigures[i].SetActive(false); }
        }

        List<int> friendClans = new List<int>();
        foreach(Player p in friendsPresentToTrade) { friendClans.Add((int)p.clan); }
        for(int c = 0; c < tradePartnerButtons.Length; c++)
        {
            if (friendClans.Contains(c)) { tradePartnerButtons[c].gameObject.SetActive(true); }
            else { tradePartnerButtons[c].gameObject.SetActive(false); }

        }

        if (initialTradeSetupDone) { return; }

        tradeButton.OnButtonPressed += TradeButtonPressed;
        leftTradeButton.OnButtonPressed += MoveTradeCardsRight;
        rightTradeButton.OnButtonPressed += MoveTradeCardsLeft;

        npcTradePartnerButton.OnButtonPressed += ChooseNPCPartner;
        tradePartnerButtons[0].OnButtonPressed += ChoosePartner0;
        tradePartnerButtons[1].OnButtonPressed += ChoosePartner1;
        tradePartnerButtons[2].OnButtonPressed += ChoosePartner2;
        tradePartnerButtons[3].OnButtonPressed += ChoosePartner3;

        chosenTradePartner = -1;
        cardToRecieve = CardArchive.main.SpawnRandomLooseCard().GetComponent<CardObject>();
        cardToRecieve.transform.SetParent(cardToRecievePos);
        cardToRecieve.transform.localPosition = Vector3.zero;
        cardToRecieve.transform.localEulerAngles = new Vector3(0f, 180f, 0f);

        initialTradeSetupDone = true;
    }

    void MoveTradeCardsLeft()
    {
        if (cardToTradeCardObjects[cardToTradeCardObjects.Count - 1].transform.localPosition.x <= rightCardLimit.x) { return; }

        foreach (CardObject card in cardToTradeCardObjects)
        {
            card.transform.localPosition = card.transform.localPosition - new Vector3(cardMoveSpeed * Time.deltaTime, 0f, 0f);
        }

        if (cardToTradeCardObjects[cardToTradeCardObjects.Count - 1].transform.localPosition.x <= rightCardLimit.x)
        {
            cardToTradeCardObjects[cardToTradeCardObjects.Count - 1].transform.localPosition = rightCardLimit;
            int count = 1;
            for (int i = cardToTradeCardObjects.Count - 2; i >= 0; i--)
            {
                cardToTradeCardObjects[i].transform.localPosition = rightCardLimit - new Vector3(distBetweenCards * count, 0f, 0f);
                count++;
            }
        }
    }

    void MoveTradeCardsRight()
    {
        if (cardToTradeCardObjects[0].transform.localPosition.x == leftCardLimit.x) { return; }

        foreach (CardObject card in cardToTradeCardObjects)
        {
            card.transform.localPosition = card.transform.localPosition + new Vector3(cardMoveSpeed * Time.deltaTime, 0f, 0f);
        }

        if (cardToTradeCardObjects[0].transform.localPosition.x >= leftCardLimit.x)
        {
            cardToTradeCardObjects[0].transform.localPosition = leftCardLimit;
            for (int i = 1; i < cardToTradeCardObjects.Count; i++)
            {
                cardToTradeCardObjects[i].transform.localPosition = leftCardLimit + new Vector3(distBetweenCards * i, 0f, 0f);
            }
        }
    }

    public void TradeButtonPressed()
    {
        if(tradeStage == 1)
        {
            SetTradeStatus(true);
            //Zones.main.deck.InstantlyShuffleMultipleCardsIntoDeck(cardToTradeCardObjects);
            //cardToTradeCardObjects.Clear();
            //leftTradeButton.gameObject.SetActive(false);
            //rightTradeButton.gameObject.SetActive(false);
            tradeStage = 2;
            return;
        }
        else if(tradeStage == 2)
        {
            SetTradeStatus(false);
            PullBackOfferedCard();
            Zones.main.deck.InstantlyShuffleCardIntoDeck(cardToTrade);
            Zones.main.deck.InstantlyShuffleMultipleCardsIntoDeck(cardToTradeCardObjects);
            cardToTradeCardObjects.Clear();
            cardToTrade = null;
            tradeStage = 0;
        }

        BuildCardSelectionFromDeck();
    }

    void BuildCardSelectionFromDeck()
    {
        foreach (CardObject card in Zones.main.deck.cardsInDeck) { cardToTradeCardObjects.Add(card); }
        Zones.main.deck.cardsInDeck.Clear();

        for (int i = 0; i < cardToTradeCardObjects.Count; i++)
        {
            cardToTradeCardObjects[i].transform.SetParent(cardToTradeHolder);
            cardToTradeCardObjects[i].transform.localPosition = leftCardLimit + new Vector3(distBetweenCards * i, 0f, 0f);
            cardToTradeCardObjects[i].transform.localEulerAngles = Vector3.zero;

        }

        leftTradeButton.gameObject.SetActive(true);
        rightTradeButton.gameObject.SetActive(true);
        cardToTradeHolder.gameObject.SetActive(true);
    }

    public void FriendOfferingCard(int cardIndex, Player friend)
    {
        indexOfCardsOfferedByFriends[(int)friend.clan] = cardIndex;
        if(chosenTradePartner == (int)friend.clan)
        {
            if(cardToRecieve != null) {
                Destroy(cardToRecieve.gameObject);
                cardToRecieve = null;
            }
            if(cardIndex == -1) { return; }

            cardToRecieve = CardArchive.main.SpawnLooseCard(cardIndex).GetComponent<CardObject>();
            cardToRecieve.transform.SetParent(cardToRecievePos);
            cardToRecieve.transform.localPosition = Vector3.zero;
            cardToRecieve.transform.localEulerAngles = Vector3.zero;
        }
    }

    public void FriendWaitingForTrade(Player friend)
    {
        if (friendsPresentToTrade.Contains(friend)) { return; }
        else
        {
            friendsPresentToTrade.Add(friend);
            tradePartnerButtons[(int)friend.clan].gameObject.SetActive(true);
        }
    }

    public void FriendLeftTrade(Player friend)
    {
        if (!friendsPresentToTrade.Contains(friend)) { return; }
        else
        {
            friendsPresentToTrade.Remove(friend);
            friendReadyStatus[(int)friend.clan] = false;
            friendTradeReadyLight[(int)friend.clan].SetActive(false);
            friendTradePartner[(int)friend.clan] = -1;
            tradePartnerButtons[(int)friend.clan].gameObject.SetActive(false);
            indexOfCardsOfferedByFriends[(int)friend.clan] = -1;
            if(chosenTradePartner == (int)friend.clan && !committedToTrade)
            {
                ChooseTradePartner(-1);
            }
        }
    }

    void SetTradeStatus(bool status)
    {
        if(cardToTrade == null) { status = false; }
        readyToTrade = status;
        readyToTradeLight.SetActive(readyToTrade);
        if (MutliplayerController.active.IsMultiplayerGame()) { Client.active.TellServerMyTradeStatus(status, chosenTradePartner); }
        CheckIfBothReadytoTrade();
    }

    public void FriendTradeReadyStatus(Player friend, bool status, int tradePartner)
    {
        friendReadyStatus[(int)friend.clan] = status;
        friendTradeReadyLight[(int)friend.clan].SetActive(status);
        friendTradePartner[(int)friend.clan] = tradePartner;
        CheckIfBothReadytoTrade();
    }

    void CheckIfBothReadytoTrade()
    {
        if(!readyToTrade) { return; }

        if(chosenTradePartner == -1) { DoTrade(); }
        else if (friendReadyStatus[chosenTradePartner] && friendTradePartner[chosenTradePartner] == (int)Player.active.clan) { DoTrade(); }
    }

    void ChoosePartner0()
    {
        ChooseTradePartner(0);
    }
    void ChoosePartner1()
    {
        ChooseTradePartner(1);
    }
    void ChoosePartner2()
    {
        ChooseTradePartner(2);
    }
    void ChoosePartner3()
    {
        ChooseTradePartner(3);
    }
    void ChooseNPCPartner()
    {
        ChooseTradePartner(-1);
    }

    void ChooseTradePartner(int choice)
    {
        if(chosenTradePartner == choice) { return; }

        if (cardToRecieve != null) { Destroy(cardToRecieve.gameObject); }
        cardToRecieve = null;
        StartCoroutine(MoveChosenFigurineToActivePartnerLocation(TradePartner(choice), TradePartner(chosenTradePartner)));
        chosenTradePartner = choice;
        SetTradeStatus(false);
        if(cardToTrade != null) tradeStage = 1;

        if(choice == -1)
        {
            cardToRecieve = CardArchive.main.SpawnRandomLooseCard().GetComponent<CardObject>();
            cardToRecieve.transform.SetParent(cardToRecievePos);
            cardToRecieve.transform.localPosition = Vector3.zero;
            cardToRecieve.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
        }
        else
        {
            int cardIndex = indexOfCardsOfferedByFriends[choice];
            if(cardIndex == -1) { return; }
            cardToRecieve = CardArchive.main.SpawnLooseCard(cardIndex).GetComponent<CardObject>();
            cardToRecieve.transform.SetParent(cardToRecievePos);
            cardToRecieve.transform.localPosition = Vector3.zero;
            cardToRecieve.transform.localEulerAngles = Vector3.zero;
        }
    }

    Transform TradePartner(int index)
    {
        switch (index)
        {
            case -1:
                return npcTradePartnerButton.transform;
            default:
                return tradePartnerButtons[index].transform;
        }
    }

    Player TradePartnerScript()
    {
        if(chosenTradePartner == -1) { return null; }
        Player friend = friendsPresentToTrade[0];
        foreach (Player p in friendsPresentToTrade)
        {
            if ((int)p.clan == chosenTradePartner) { friend = p; }
        }
        return friend;
    }

    void TradeCardClicked(CardObject obj)
    {
        if (tradeStage > 0)
        {
            SetTradeStatus(false);
            PullBackOfferedCard();
            Zones.main.deck.InstantlyShuffleCardIntoDeck(cardToTrade);
            Zones.main.deck.InstantlyShuffleMultipleCardsIntoDeck(cardToTradeCardObjects);
            cardToTradeCardObjects.Clear();
            cardToTrade = null;
            tradeStage = 0;
            BuildCardSelectionFromDeck();
            return;
        }
        else
        {
            tradeStage = 1;
            StartCoroutine(MoveCardToTradeLocation(obj.transform));
            cardToTradeCardObjects.Remove(obj);

            cardToTrade = obj;
            if (MutliplayerController.active.IsMultiplayerGame())
            {
                Client.active.TellServerIHaveOfferedCardForTrade(CardArchive.main.CardIndex(cardToTrade.myCard));
            }

            //Move the card selection away.
            Zones.main.deck.InstantlyShuffleMultipleCardsIntoDeck(cardToTradeCardObjects);
            cardToTradeCardObjects.Clear();
            leftTradeButton.gameObject.SetActive(false);
            rightTradeButton.gameObject.SetActive(false);
        }
    }

    void PullBackOfferedCard()
    {
        if (MutliplayerController.active.IsMultiplayerGame())
        {
            Client.active.TellServerIHaveOfferedCardForTrade(-1);
        }
    }

    void DoTrade()
    {
        if(cardToTrade == null || cardToRecieve == null) { return; }
        
        StartCoroutine(AnimateTrade(cardToTrade, cardToRecieve));
    }

    void TradeCleanUp()
    {
        tradeEventEndText.gameObject.SetActive(true);
        if (MutliplayerController.active.IsMultiplayerGame()) { Client.active.TellServerILeftTradeEvent(); }
        Zones.main.deck.InstantlyShuffleMultipleCardsIntoDeck(cardToTradeCardObjects);
        cardToTradeCardObjects.Clear();
        committedToTrade = false;
        tradeStage = 0;

        for (int i = 0; i < friendReadyStatus.Length; i++) { friendReadyStatus[i] = false; }
        for (int i = 0; i < friendTradeReadyLight.Length; i++) { friendTradeReadyLight[i].SetActive(false); }
        for (int i = 0; i < friendTradePartner.Length; i++) { friendTradePartner[i] = -1; }
        for (int i = 0; i < indexOfCardsOfferedByFriends.Length; i++) { indexOfCardsOfferedByFriends[i] = -1; }

        friendsPresentToTrade.Clear();

        StartCoroutine(FinishAfterTimer());
    }

    IEnumerator MoveChosenFigurineToActivePartnerLocation(Transform newFig, Transform oldFig)
    {
        Debug.Log("Moving Trade Figure");
        float timeElapsed = 0f;
        Vector3 waitingPos = newFig.transform.localPosition;
        Vector3 activePos = activePartnerLocation;

        while(timeElapsed < tradeAnimationSpeed/2)
        {
            newFig.localPosition = Vector3.Lerp(waitingPos, activePos, timeElapsed / (tradeAnimationSpeed / 2));
            oldFig.localPosition = Vector3.Lerp(activePos, waitingPos, timeElapsed / (tradeAnimationSpeed / 2));

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        newFig.localPosition = activePos;
        oldFig.localPosition = waitingPos;
    }

    IEnumerator AnimateTrade(CardObject givenCard, CardObject recievedCard)
    {
        committedToTrade = true;
        float timeElapsed = 0f;
        recievedCard.transform.SetParent(cardToGivePos);
        Vector3 playerSide = givenCard.transform.localPosition;
        Vector3 partnerSide = recievedCard.transform.localPosition;
        Vector3 endRot = new Vector3(0f, 0f, 0f);
        Vector3 giveStartRot = givenCard.transform.localEulerAngles;
        Vector3 recieveStartRot = recievedCard.transform.localEulerAngles;

        tradeEventEndText.text = "Trade Complete \n You gave up " + givenCard.CardName() + "\n You recieved " + recievedCard.CardName();

        while(timeElapsed < tradeAnimationSpeed)
        {
            givenCard.transform.localPosition = Vector3.Lerp(playerSide, partnerSide, timeElapsed / tradeAnimationSpeed);
            recievedCard.transform.localPosition = Vector3.Lerp(partnerSide, playerSide, timeElapsed / tradeAnimationSpeed);

            givenCard.transform.localEulerAngles = Vector3.Lerp(giveStartRot, endRot, timeElapsed / tradeAnimationSpeed);
            recievedCard.transform.localEulerAngles = Vector3.Lerp(recieveStartRot, endRot, timeElapsed / tradeAnimationSpeed);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        givenCard.transform.localPosition = partnerSide;
        recievedCard.transform.localPosition = playerSide;

        givenCard.transform.localEulerAngles = endRot;
        recievedCard.transform.localEulerAngles = endRot;

        if (MutliplayerController.active.IsMultiplayerGame())
        {
            int oldCardIndex = givenCard.referenceIndex;
            int newCardSpawnIndex = CardArchive.main.CardIndex(recievedCard.myCard);
            Client.active.TellServerIReplaceACardInMyDeck(oldCardIndex, newCardSpawnIndex);
        }

        Player.active.ReplaceCardInPlayerDeckList(givenCard, recievedCard);
        Zones.main.deck.InstantlyShuffleCardIntoDeck(recievedCard);
        

        TradeCleanUp();
    }

    IEnumerator MoveCardToTradeLocation(Transform card)
    {
        float timeElapsed = 0f;
        card.SetParent(cardToGivePos);
        card.localEulerAngles = Vector3.zero;
        Vector3 start = card.localPosition;
        Vector3 end = Vector3.zero;

        while (timeElapsed < tradeAnimationSpeed)
        {
            card.localPosition = Vector3.Lerp(start, end, timeElapsed / tradeAnimationSpeed);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        card.localPosition = end;
    }
    #endregion

    #region LeaderLevelUp
    [Space(10f)]
    [Header("LeaderEvent")]
    public Transform currentLeaderHolder;
    public Transform leftLevelUpChoiceHolder, rightLevelUpChoiceHolder;
    int leaderLeftIndexChoice, leaderRightIndexChoice;

    void SetupLeader()
    {
        foreach(Transform child in currentLeaderHolder)
        {
            Destroy(child.gameObject);
        }
        currentLeaderHolder.DetachChildren();

        foreach (Transform child in leftLevelUpChoiceHolder)
        {
            Destroy(child.gameObject);
        }
        leftLevelUpChoiceHolder.DetachChildren();

        foreach (Transform child in rightLevelUpChoiceHolder)
        {
            Destroy(child.gameObject);
        }
        rightLevelUpChoiceHolder.DetachChildren();

        CardObject currentLeader = LeaderArchive.main.SpawnLeader(Player.active.leader);
        currentLeader.transform.SetParent(currentLeaderHolder);
        currentLeader.transform.localPosition = Vector3.zero;
        currentLeader.transform.localEulerAngles = Vector3.zero;

        int[] levelUpIndices = LeaderArchive.main.IndicesOfLeaderLevelUps(Player.active.leader);
        if(levelUpIndices == null) { LeaderCleanUp(); return; } //No available leavel ups so just get out of the event
        leaderLeftIndexChoice = levelUpIndices[0];
        leaderRightIndexChoice = levelUpIndices[1];

        CardObject[] levelUps = LeaderArchive.main.SpawnLeaderLevelUps(Player.active.leader);
        levelUps[0].transform.SetParent(leftLevelUpChoiceHolder);
        levelUps[0].transform.localPosition = Vector3.zero;
        levelUps[0].transform.localEulerAngles = Vector3.zero;
        levelUps[0].zoneScript = Zones.main;

        levelUps[1].transform.SetParent(rightLevelUpChoiceHolder);
        levelUps[1].transform.localPosition = Vector3.zero;
        levelUps[1].transform.localEulerAngles = Vector3.zero;
        levelUps[1].zoneScript = Zones.main;

    }

    void LeaderCardClicked(CardObject obj)
    {
        if(leftLevelUpChoiceHolder.GetChild(0).GetComponent<CardObject>() == obj)
        {
            Player.active.UpdateLeader(leaderLeftIndexChoice);
            if (MutliplayerController.active.IsMultiplayerGame()) { Client.active.TellServerIHaveANewLeader(leaderLeftIndexChoice); }
            LeaderCleanUp();
        }
        else if (rightLevelUpChoiceHolder.GetChild(0).GetComponent<CardObject>() == obj)
        {
            Player.active.UpdateLeader(leaderRightIndexChoice);
            if (MutliplayerController.active.IsMultiplayerGame()) { Client.active.TellServerIHaveANewLeader(leaderRightIndexChoice); }
            LeaderCleanUp();
        }
    }

    void LeaderCleanUp()
    {
        foreach (Transform child in currentLeaderHolder)
        {
            Destroy(child.gameObject);
        }
        currentLeaderHolder.DetachChildren();

        foreach (Transform child in leftLevelUpChoiceHolder)
        {
            Destroy(child.gameObject);
        }
        leftLevelUpChoiceHolder.DetachChildren();

        foreach (Transform child in rightLevelUpChoiceHolder)
        {
            Destroy(child.gameObject);
        }
        rightLevelUpChoiceHolder.DetachChildren();

        leaderLeftIndexChoice = 0;
        leaderRightIndexChoice = 0;

        FinishEvent();
    }

    #endregion

    IEnumerator MoveBackingImageIntoFocus()
    {
        float timeElapsed = 0f;
        backingImage.gameObject.SetActive(true);
        Vector2 startPos = backingImage.rectTransform.anchoredPosition;
        Vector2 startScale = Vector2.zero;
        Vector2 endPos = new Vector2(Screen.width / 2, Screen.height / 2);
        Vector2 endScale = backingImageMaxSize;

        while(timeElapsed < animationDuration)
        {
            backingImage.rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, timeElapsed / animationDuration);
            backingImage.rectTransform.sizeDelta = Vector2.Lerp(startScale, endScale, timeElapsed / animationDuration);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        backingImage.rectTransform.anchoredPosition = endPos;
        backingImage.rectTransform.sizeDelta = endScale;
        SetupEvent();
    }

    IEnumerator MoveBackingImageOutOfFocus()
    {
        float timeElapsed = 0f;
        Vector2 startPos = new Vector2(Screen.width / 2, Screen.height / 2);
        Vector2 startScale =  backingImageMaxSize;
        Vector2 endPos = new Vector2(Screen.width / 2, Screen.height / 2);
        Vector2 endScale = Vector2.zero;

        while (timeElapsed < animationDuration)
        {
            backingImage.rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, timeElapsed / animationDuration);
            backingImage.rectTransform.sizeDelta = Vector2.Lerp(startScale, endScale, timeElapsed / animationDuration);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        backingImage.rectTransform.anchoredPosition = endPos;
        backingImage.rectTransform.sizeDelta = endScale;
        backingImage.gameObject.SetActive(false);

        Location.active.ActivePlayerCompletedCurrentNode();
    }

    IEnumerator FinishAfterTimer()
    {
        isFinished = true;
        yield return new WaitForSeconds(5f);

        FinishEvent();
    }
}
