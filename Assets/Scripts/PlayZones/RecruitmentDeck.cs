using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecruitmentDeck : MonoBehaviour
{
    public int cardIndexStartPoint;

    public List<Card> recruitmentCardList = new List<Card>();
    List<CardObject> cardReferenceList = new List<CardObject>();

    public Vector3 hidePos, shownPos;
    bool trayOut;

    public Vector3 deckPos, deckRot;
    public List<CardObject> cardsInDeck = new List<CardObject>();
    public float cardWidth;

    public Vector3 cardPos1, cardPos2, cardPos3;
    public Vector3 cardOnShowRot;
    CardObject cardInSlot1, cardInSlot2, cardInSlot3;

    public GameObject coinPrefab;
    public Vector3 coinSpawnPoint;
    public float coinSpawnDist;
    List<GameObject> coinObjects = new List<GameObject>();

    public float animationDuration;

    public void StartBattle()
    {
        BuildCardList();
        BuildDeck();
        ShuffleDeck();

        if (MutliplayerController.active.IsMultiplayerGame())
        {
            Client.active.PassRecruitmentDeckStackToServer(GetDeckListStack());
        }

        DrawCardToSlot(1);
        DrawCardToSlot(2);
        DrawCardToSlot(3);
    }

    public void StartRound()
    {

    }

    public void StartBossBattle()
    {

    }

    public void StartBattleFromHostInfo(int[] deckStack)
    {
        BuildCardList();
        BuildDeckFromList(new List<int>(deckStack));

        DrawCardToSlot(1);
        DrawCardToSlot(2);
        DrawCardToSlot(3);
    }

    void BuildCardList()
    {
        int[] array = CardArchive.main.RegularCardIndicies();

        for(int i = cardIndexStartPoint; i < array.Length; i++)
        {
            recruitmentCardList.Add(CardArchive.main.cards[array[i]]);
        }
    }

    public void BuildDeck()
    {
        cardReferenceList = new List<CardObject>();

        for(int i = 0; i < recruitmentCardList.Count; i++)
        {
            Card card = recruitmentCardList[i];

            GameObject obj = Instantiate(CardArchive.main.cardObjectPrefab);
            CardObject script = obj.GetComponent<CardObject>();
            card.JoinToObject(script);

            cardsInDeck.Add(script);

            script.referenceIndex = i;
            cardReferenceList.Add(script);

            script.zone = Zones.Type.RecruitmentDeck;
            script.zoneScript = Zones.main;
            obj.transform.SetParent(transform);
        }

        StackDeck();
    }

    public void BuildDeckFromList(List<int> list)
    {
        cardReferenceList = new List<CardObject>();
        foreach(int i in list) { cardReferenceList.Add(null); }

        for(int i = 0; i < list.Count; i++)
        {
            Card card = recruitmentCardList[list[i]];

            GameObject obj = Instantiate(CardArchive.main.cardObjectPrefab);
            CardObject script = obj.GetComponent<CardObject>();
            card.JoinToObject(script);

            cardsInDeck.Add(script);

            script.referenceIndex = list[i];
            cardReferenceList[list[i]] = script;

            script.zone = Zones.Type.RecruitmentDeck;
            obj.transform.SetParent(transform);
        }

        StackDeck();
    }

    public CardObject[] CurrentRecruitmentDeckList()
    {
        return cardReferenceList.ToArray();
    }

    public CardObject GetCardByRef(int cardRef)
    {
        return cardReferenceList[cardRef];
    }

    public CardObject GetCardInSlot(int slot)
    {
        switch (slot)
        {
            default:
                return cardInSlot1;
            case 2:
                return cardInSlot2;
            case 3:
                return cardInSlot3;
        }
    }

    public void StackDeck()
    {
        if (cardsInDeck.Count < 1) { return; }

        float highestCardY = cardWidth * cardsInDeck.Count;

        for (int i = 0; i < cardsInDeck.Count; i++)
        {
            cardsInDeck[i].transform.localPosition = new Vector3(deckPos.x, deckPos.y - ((highestCardY - (i * cardWidth)) * -1f), deckPos.z);
            cardsInDeck[i].transform.localEulerAngles = deckRot;
        }
    }

    public void ShuffleDeck()
    {
        CardObject[] newPositions = new CardObject[cardsInDeck.Count];
        List<int> availablePositions = new List<int>();

        for (int i = 0; i < cardsInDeck.Count; i++) { availablePositions.Add(i); }

        foreach (CardObject card in cardsInDeck)
        {
            int rand = Random.Range(0, availablePositions.Count);
            int index = availablePositions[rand];
            newPositions[index] = card;
            availablePositions.RemoveAt(rand);
        }

        cardsInDeck = new List<CardObject>(newPositions);
        StackDeck();
    }

    public int[] GetDeckListStack()
    {
        int[] array = new int[cardsInDeck.Count];

        for(int i = 0; i < array.Length; i++)
        {
            array[i] = cardsInDeck[i].referenceIndex;
        }

        return array;
    }

    public void DrawCardToSlot(int slot)
    {
        if(cardsInDeck.Count == 0) { return; }

        CardObject card = cardsInDeck[0];

        if(slot == 1)
        {
            StartCoroutine(MoveCardToPosition(card, cardPos1));
            cardInSlot1 = card;
            cardsInDeck.Remove(card);
            card.SetZone(Zones.Type.Moving, Zones.Type.Recruitment);
        }
        else if (slot == 2)
        {
            StartCoroutine(MoveCardToPosition(card, cardPos2));
            cardInSlot2 = card;
            cardsInDeck.Remove(card);
            card.SetZone(Zones.Type.Moving, Zones.Type.Recruitment);
        }
        else if (slot == 3)
        {
            StartCoroutine(MoveCardToPosition(card, cardPos3));
            cardInSlot3 = card;
            cardsInDeck.Remove(card);
            card.SetZone(Zones.Type.Moving, Zones.Type.Recruitment);
        }
    }

    public void DrawSpecificCardToSlot(int specificCard, int slot)
    {
        if (cardsInDeck.Count == 0) { return; }

        CardObject card = cardsInDeck[specificCard];

        if (slot == 1)
        {
            StartCoroutine(MoveCardToPosition(card, cardPos1));
            cardInSlot1 = card;
            cardsInDeck.Remove(card);
            card.SetZone(Zones.Type.Moving, Zones.Type.Recruitment);
        }
        else if (slot == 2)
        {
            StartCoroutine(MoveCardToPosition(card, cardPos2));
            cardInSlot2 = card;
            cardsInDeck.Remove(card);
            card.SetZone(Zones.Type.Moving, Zones.Type.Recruitment);
        }
        else if (slot == 3)
        {
            StartCoroutine(MoveCardToPosition(card, cardPos3));
            cardInSlot3 = card;
            cardsInDeck.Remove(card);
            card.SetZone(Zones.Type.Moving, Zones.Type.Recruitment);
        }
    }

    public bool RecruitCard(CardObject card, Zones playerZone)
    {
        if (!playerZone.player.CanAffordRecruit()) { Debug.Log("Player can't afford recruit"); return false; }

        cardsInDeck.Remove(card);
        recruitmentCardList.Remove(card.myCard);
        RemoveCardFromRefList(card);

        if(cardInSlot1 == card)
        {
            DrawCardToSlot(1);
        }
        else if(cardInSlot2 == card)
        {
            DrawCardToSlot(2);
        }
        else if (cardInSlot3 == card)
        {
            DrawCardToSlot(3);
        }

        playerZone.player.AddCardToPlayerDeckList(card);
        card.transform.SetParent(playerZone.discard.transform);
        GameController.main.ActiveZone().discard.MoveCardToDiscard(card);
        return true;
    }

    void RemoveCardFromRefList(CardObject card)
    {
        cardReferenceList.Remove(card);

        for(int i = 0; i < cardReferenceList.Count; i++)
        {
            cardReferenceList[i].referenceIndex = i;
        }
    }

    public void ShowTray()
    {
        if (!trayOut)
        {
            CameraController.main.MoveToRecruit();
            trayOut = true;
        }
    }

    public void HideTray()
    {
        if (trayOut)
        {
            CameraController.main.MoveFromRecruit();
            trayOut = false;
        }
    }

    public void SpawnCoins()
    {
        if(coinObjects.Count > 0)
        {
            foreach(GameObject obj in coinObjects)
            {
                Destroy(obj);
            }
            coinObjects.Clear();
        }

        for(int i = 0; i < Player.active.coins; i++)
        {
            GameObject c = Instantiate(coinPrefab);
            c.transform.SetParent(transform);
            c.transform.localPosition = new Vector3(coinSpawnPoint.x, coinSpawnPoint.y + (coinSpawnDist * i), coinSpawnPoint.z);
            coinObjects.Add(c);
        }
    }

    public void LoseACoin()
    {
        if(coinObjects.Count > 0)
        {
            Destroy(coinObjects[0]);
            coinObjects.RemoveAt(0);
        }
    }
    
    IEnumerator MoveCardToPosition(CardObject card, Vector3 position)
    {
        float timeElapsed = 0f;
        Vector3 startPos = card.transform.localPosition;
        Vector3 startRot = card.transform.localEulerAngles;
        Vector3 endrot = cardOnShowRot;

        while(timeElapsed < animationDuration)
        {
            card.transform.localPosition = Vector3.Lerp(startPos, position, timeElapsed / animationDuration);
            card.transform.localEulerAngles = Vector3.Lerp(startRot, endrot, timeElapsed / animationDuration);

            timeElapsed += Time.deltaTime;
            yield return null;
        }
        card.transform.localPosition = position;
        card.transform.localEulerAngles = endrot;
        card.zone = Zones.Type.Recruitment;
    }

    IEnumerator MoveRecruitmentTray(Vector3 start, Vector3 end)
    {
        float timeElapsed = 0f;

        while (timeElapsed < animationDuration)
        {
            transform.localPosition = Vector3.Lerp(start, end, timeElapsed / animationDuration);

            timeElapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = end;
    }
}
