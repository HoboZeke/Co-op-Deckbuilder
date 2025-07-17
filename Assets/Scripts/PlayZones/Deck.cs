using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    public Zones zone;
    public List<CardObject> cardsInDeck = new List<CardObject>();
    public float cardWidth;

    public float animationDuration;

    bool deckBusy;

    [Header("DeckViewer")]
    [SerializeField] Vector3 deckViewerLeftMostPos;
    [SerializeField] Vector3 deckViewerRightMostPos;
    [SerializeField] Vector3 spaceBetweenCardsInDeckViewer;
    [SerializeField] float deckViewerMoveSpeed;
    Vector3[] originalDeckPositions;
    Vector3 trueDeckViewerRightMostPos;

    public void DrawCards(int amount)
    {
        for(int i = 0; i < amount; i++)
        {
            DrawCard();
        }
    }

    public void DrawCard()
    {
        if(zone != Zones.main) { return; }

        if (deckBusy)
        {
            StartCoroutine(DrawCardsAfterDelay());
            return;
        }

        if (cardsInDeck.Count == 0)
        {
            if (zone.discard.cardsInDiscard.Count > 0)
            {
                zone.ShuffleDiscardIntoDeck();
                StartCoroutine(DrawCardsAfterDelay());
                return;
            }
            else
            {
                Debug.Log("Trying to draw a card but none in deck or discard!");
            }

        }

        zone.hand.MoveCardToHand(cardsInDeck[0]);
        cardsInDeck[0].SetZone(Zones.Type.Moving, Zones.Type.Hand);

        if (MutliplayerController.active.IsMultiplayerGame())
        {
            Client.active.DrewCard(cardsInDeck[0].referenceIndex);
        }


        cardsInDeck.RemoveAt(0);
    }

    public void DrawSpecificCard(int cardRef)
    {
        if (cardsInDeck.Count == 0)
        {
            if (zone.discard.cardsInDiscard.Count > 0)
            {
                zone.ShuffleDiscardIntoDeck();
                StartCoroutine(DrawSpecificCardAfterDelay(cardRef));
                return;
            }
            else
            {
                Debug.Log("Trying to draw a card but none in deck or discard!");
            }

        }

        CardObject card = null;
        foreach(CardObject cardObject in cardsInDeck)
        {
            if(cardObject.referenceIndex == cardRef) { card = cardObject; break; }
        }
        
        if(card == null) { Debug.Log("Trying to draw a card ref " + cardRef + " but no card with that ref found in the deck"); return; }

        zone.hand.MoveCardToHand(card);
        card.SetZone(Zones.Type.Moving, Zones.Type.Hand);
        cardsInDeck.Remove(card);
        if(zone.player.cardsToDrawBeforeReadyForSync > 0) { zone.player.cardsToDrawBeforeReadyForSync--; }
    }

    public void StackDeck()
    {
        if(cardsInDeck.Count < 1) { return; }

        float highestCardZ = cardWidth * cardsInDeck.Count;

        for(int i = 0; i < cardsInDeck.Count; i++)
        {
            cardsInDeck[i].transform.localPosition = new Vector3(0f, 0f, (highestCardZ - (i * cardWidth))*-1f);
            cardsInDeck[i].transform.localEulerAngles = new Vector3(0f, 180f, Random.Range(-7.5f, 7.5f));
        }
    }

    public void ClearZone()
    {
        foreach(CardObject card in cardsInDeck)
        {
            Destroy(card.gameObject);
        }
        cardsInDeck.Clear();
    }

    Vector3 TopOfDeck()
    {
        return new Vector3(0f, 0f, (cardWidth * cardsInDeck.Count) * -1f);
    }

    public CardObject RandomCardFromDeck()
    {
        CardObject cardToSend = cardsInDeck[Random.Range(0, cardsInDeck.Count)];
        cardsInDeck.Remove(cardToSend);
        return cardToSend;
    }

    public void ShuffleDeck()
    {
        CardObject[] newPositions = new CardObject[cardsInDeck.Count];
        List<int> availablePositions = new List<int>();

        for(int i = 0; i < cardsInDeck.Count; i++) { availablePositions.Add(i); }

        foreach(CardObject card in cardsInDeck)
        {
            int rand = Random.Range(0, availablePositions.Count);
            int index = availablePositions[rand];
            newPositions[index] = card;
            availablePositions.RemoveAt(rand);
        }

        cardsInDeck = new List<CardObject>(newPositions);
        StackDeck();
    }

    public void AddCardToDeck(CardObject card)
    {
        AddCardToDeck(card, false);
    }

    public void AddCardToDeck(CardObject card, bool shuffle)
    {
        card.transform.SetParent(transform);
        cardsInDeck.Add(card);
        card.SetZone(Zones.Type.Deck);

        if (shuffle) { ShuffleDeck(); }
        else { StackDeck(); }
    }

    public void BuildFromDeckList(int[] decklist)
    {
        Debug.Log("Building Deck From List!");

        for (int i = 0; i < decklist.Length; i++)
        {
            CardArchive.main.SpawnCard(decklist[i], zone.player);
        }

        //Set the starting cards loot values to 0 to make the freebies not worth anything.
        foreach (CardObject card in cardsInDeck)
        {
            card.cardLootValues[0] = 0;
            card.cardLootValues[1] = 0;
            card.cardLootValues[2] = 0;
        }

        ShuffleDeck();
    }

    public void AddMultipleCardsToDeck(List<CardObject> cards)
    {
        AddMultipleCardsToDeck(cards, false);
    }

    public void AddMultipleCardsToDeck(List<CardObject> cards, bool shuffle)
    {
        foreach(CardObject card in cards)
        {
            card.transform.SetParent(transform);
            cardsInDeck.Add(card);
            card.SetZone(Zones.Type.Deck);
        }
        
        if (shuffle) { ShuffleDeck(); }
        else { StackDeck(); }
    }

    public void ShuffleMultipleCardsIntoDeck(List<CardObject> cards)
    {
        Debug.Log("Shuffling mutliple cards into deck");
        foreach (CardObject card in cards)
        {
            card.transform.SetParent(transform);
        }

        StartCoroutine(MoveMultipleCardsToDeckThenShuffle(cards));
    }

    public void ShuffleCardIntoDeck(CardObject card)
    {
        List<CardObject> cards = new List<CardObject>() { card };

        ShuffleMultipleCardsIntoDeck(cards);
    }

    public void InstantlyShuffleMultipleCardsIntoDeck(List<CardObject> cards)
    {
        foreach (CardObject card in cards)
        {
            card.transform.SetParent(transform);
            card.SetZone(Zones.Type.Deck);
            cardsInDeck.Add(card);
            card.transform.localPosition = TopOfDeck();
            card.transform.localEulerAngles = new Vector3(0f, 180f, Random.Range(-7.5f, 7.5f));
        }

        ShuffleDeck();
    }

    public void InstantlyShuffleCardIntoDeck(CardObject card)
    {
        List<CardObject> cards = new List<CardObject>() { card };

        InstantlyShuffleMultipleCardsIntoDeck(cards);
    }

    IEnumerator MoveMultipleCardsToDeckThenShuffle(List<CardObject> cards)
    {
        deckBusy = true;
        Debug.Log("Cards moving " + cards.Count);
        foreach(CardObject card in cards)
        {
            card.SetZone(Zones.Type.Moving, Zones.Type.Deck);
            cardsInDeck.Add(card);
            StartCoroutine(MoveCardToTopOfDeck(card));
        }

        yield return new WaitForSeconds(animationDuration);
        yield return null;
        ShuffleDeck();
        deckBusy = false;
    }

    IEnumerator MoveCardToTopOfDeck(CardObject card)
    {
        float timeElapsed = 0f;
        Vector3 start = card.transform.localPosition;
        Quaternion startRot = Quaternion.Euler(card.transform.localEulerAngles);

        Vector3 end = TopOfDeck();
        Quaternion endRot = Quaternion.Euler(new Vector3(0f, 180f, Random.Range(-7.5f, 7.5f)));

        while (timeElapsed < animationDuration)
        {
            card.transform.localPosition = Vector3.Lerp(start, end, timeElapsed / animationDuration);
            card.transform.localRotation = Quaternion.Lerp(startRot, endRot, timeElapsed / animationDuration);

            timeElapsed += Time.deltaTime;
            yield return null;
        }


        card.transform.localPosition = end;
        card.transform.localRotation = endRot;
        card.SetZone(Zones.Type.Deck);
    }

    IEnumerator DrawCardsAfterDelay()
    {
        yield return new WaitForSeconds(animationDuration);
        DrawCard();
    }

    IEnumerator DrawSpecificCardAfterDelay(int cardRef)
    {
        yield return new WaitForSeconds(animationDuration);
        DrawSpecificCard(cardRef);
    }

    #region DeckViewer

    bool deckViewerOpen = false;
    bool moveRight = false;
    bool moveLeft = false;

    public void OpenDeckViewer()
    {
        if(deckViewerOpen) { return; }

        UIController.main.ToggleDeckViewerUI(true);
        originalDeckPositions = new Vector3[cardsInDeck.Count];
        for(int i = 0; i < cardsInDeck.Count; i++)
        {
            originalDeckPositions[i] = cardsInDeck[i].transform.localPosition;
            StartCoroutine(MoveCardToPos(cardsInDeck[i].transform, DeckViewerBasePos(i)));
        }

        if(DeckViewerBasePos(cardsInDeck.Count-1).x < deckViewerRightMostPos.x)
        {
            trueDeckViewerRightMostPos = DeckViewerBasePos(cardsInDeck.Count - 1);
        }
        else
        {
            float rightMostZ = spaceBetweenCardsInDeckViewer.z * (cardsInDeck.Count-1);
            trueDeckViewerRightMostPos = deckViewerRightMostPos + new Vector3(0f, 0f, rightMostZ);
        }
        deckViewerOpen = true;
    }

    public void CloseDeckViewer()
    {
        if(!deckViewerOpen) { return; }

        UIController.main.ToggleDeckViewerUI(false); 
        for (int i = 0; i < cardsInDeck.Count; i++)
        {
            StartCoroutine(MoveCardToPos(cardsInDeck[i].transform, originalDeckPositions[i]));
        }

        deckViewerOpen = false;
    }

    Vector3 DeckViewerBasePos(int cardCount)
    {
        return deckViewerLeftMostPos + (spaceBetweenCardsInDeckViewer * cardCount);
    }

    private void Update()
    {
        if (moveRight)
        {
            MoveDeckViewRight();
        }
        else if (moveLeft)
        {
            MoveDeckViewLeft();
        }
    }

    public void ToggleMoveRight(bool toggle) { moveRight = toggle; }
    public void ToggleMoveLeft (bool toggle) { moveLeft = toggle; }

    public void MoveDeckViewRight()
    {
        Transform rightMostCard = cardsInDeck[cardsInDeck.Count - 1].transform;
        rightMostCard.localPosition += Vector3.left * deckViewerMoveSpeed * Time.deltaTime;

        if (rightMostCard.localPosition.x < trueDeckViewerRightMostPos.x) { rightMostCard.localPosition = trueDeckViewerRightMostPos; }
        for(int i = cardsInDeck.Count - 2; i >= 0; i--)
        {
            cardsInDeck[i].transform.localPosition = rightMostCard.localPosition - (spaceBetweenCardsInDeckViewer * (cardsInDeck.Count-1 - i));
        }
    }

    public void MoveDeckViewLeft()
    {
        Transform leftMostCard = cardsInDeck[0].transform;
        leftMostCard.localPosition += Vector3.right * deckViewerMoveSpeed * Time.deltaTime;

        if (leftMostCard.localPosition.x > deckViewerLeftMostPos.x) { leftMostCard.localPosition = deckViewerLeftMostPos; }
        for (int i = 1; i < cardsInDeck.Count; i++)
        {
            cardsInDeck[i].transform.localPosition = leftMostCard.localPosition + (spaceBetweenCardsInDeckViewer * i);
        }
    }

    IEnumerator MoveCardToPos(Transform card, Vector3 pos)
    {
        float timeElapsed = 0f;
        Vector3 start = card.transform.localPosition;

        while (timeElapsed < animationDuration)
        {
            card.localPosition = Vector3.Lerp(start, pos, timeElapsed/animationDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        card.localPosition = pos;
    }
    #endregion
}
