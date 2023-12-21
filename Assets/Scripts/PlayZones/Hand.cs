using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public Zones zone;
    public List<CardObject> cardsInHand = new List<CardObject>();
    public Vector3 leftMostPoint, midPoint, rightMostPoint;
    public Vector3 leftRot, rightRot;
    public Transform focusPoint;

    public float moveDuration;

    List<CardObject> drawQueue = new List<CardObject>();

    public void MoveCardToHand(CardObject card)
    {
        drawQueue.Add(card);
        card.transform.SetParent(transform);

        if (drawQueue.Count == 1)
        {
            cardsInHand.Add(card);
            StartCoroutine(AnimateMoveCardToHand(drawQueue[0]));
        }
    }

    public void ShiftHand(CardObject excluding)
    {
        for(int i = 0; i < cardsInHand.Count; i++)
        {
            if(cardsInHand[i] != excluding || !drawQueue.Contains(cardsInHand[i]))
            {
                cardsInHand[i].transform.localPosition = PositionInHand(i, cardsInHand.Count);
                cardsInHand[i].FaceTowards(focusPoint);
                cardsInHand[i].transform.localEulerAngles = new Vector3(cardsInHand[i].transform.localEulerAngles.x, cardsInHand[i].transform.localEulerAngles.y, RotationInHand(i, cardsInHand.Count).z);
                cardsInHand[i].ShiftPosition();
            }
        }
    }

    public void DiscardCard(CardObject card)
    {
        if (!cardsInHand.Contains(card))
        {
            Debug.Log("Trying to discard a card from the hand that doesn't exist! Card = " + card.name);
            return;
        }

        card.SetZone(Zones.Type.Moving, Zones.Type.Discard);
        card.transform.SetParent(zone.discard.transform);
        card.OnLeavePlay();
        zone.discard.MoveCardToDiscard(card);
        cardsInHand.Remove(card);
        ShiftHand(null);
    }

    public int CardsInHand()
    {
        return cardsInHand.Count;
    }

    public void ClearZone()
    {
        foreach (CardObject card in cardsInHand)
        {
            Destroy(card.gameObject);
        }
        cardsInHand.Clear();
    }

    public Vector3 CardHandPos(CardObject card)
    {
        if (cardsInHand.Contains(card))
        {
            return PositionInHand(cardsInHand.IndexOf(card), cardsInHand.Count);
        }
        else
        {
            return card.transform.localPosition;
        }
    }

    Vector3 PositionInHand(int index, int handCount)
    {
        //Set the offset manuall, then modify it by increments of 10;
        float offset = 0.1f;
        int mod = handCount / 10;
        offset = offset / (mod + 1);

        //Find out the position of the current card and last card
        float endOffset = offset * (handCount - 1);
        offset = offset * index;

        //Use the position of the last card to find out how much free space is left, then use this to centre the cards.
        float diff = (1f - endOffset) / 2;
        
        float t = offset + diff;

        Vector3 left = Vector3.Lerp(leftMostPoint, midPoint, t);
        Vector3 right = Vector3.Lerp(midPoint, rightMostPoint, t);
        Vector3 pos = Vector3.Lerp(left, right, t);
        

        return pos;
    }

    Vector3 RotationInHand(int index, int handCount)
    {
        //Set the offset manuall, then modify it by increments of 10;
        float offset = 0.1f;
        int mod = handCount / 10;
        offset = offset / (mod + 1);

        //Find out the position of the current card and last card
        float endOffset = offset * (handCount - 1);
        offset = offset * index;

        //Use the position of the last card to find out how much free space is left, then use this to centre the cards.
        float diff = (1f - endOffset) / 2;

        float t = offset + diff;

        return Vector3.Lerp(leftRot, rightRot, t);
    }

    IEnumerator AnimateMoveCardToHand(CardObject card)
    {
        ShiftHand(card);
        if (zone.player == Player.active) { AudioManager.main.CardDrawnAudioEvent(); }

        float timeElapsed = 0f;
        Vector3 startPos = card.transform.localPosition;
        Vector3 endPos = PositionInHand(cardsInHand.Count - 1, cardsInHand.Count);

        Vector3 startRot = card.transform.localEulerAngles;
        Vector3 endRot = Vector3.zero;

        while(timeElapsed < moveDuration)
        {
            card.transform.localPosition = Vector3.Lerp(startPos, endPos, timeElapsed / moveDuration);
            card.transform.localEulerAngles = Vector3.Lerp(startRot, endRot, timeElapsed / moveDuration);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        card.transform.localPosition = endPos;
        card.transform.localEulerAngles = endRot;

        card.zone = Zones.Type.Hand;
        card.FaceTowards(focusPoint);
        card.transform.localEulerAngles = new Vector3(card.transform.localEulerAngles.x, card.transform.localEulerAngles.y, RotationInHand(cardsInHand.Count-1, cardsInHand.Count).z);

        card.OnEnterHand();
        drawQueue.Remove(card);
        if(drawQueue.Count != 0)
        {
            cardsInHand.Add(drawQueue[0]);
            StartCoroutine(AnimateMoveCardToHand(drawQueue[0]));
        }
    }
}
