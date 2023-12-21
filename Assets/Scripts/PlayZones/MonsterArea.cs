using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterArea : MonoBehaviour
{
    public List<CardObject> cardsInZone = new List<CardObject>();
    public Vector3 spawnPoint;
    public Vector3 leftMostPoint, rightMostPoint;

    public float moveDuration;

    List<CardObject> spawnQueue = new List<CardObject>();

    public void AddCardToArea(CardObject card)
    {
        card.transform.SetParent(transform);
        card.transform.localPosition = spawnPoint;
        card.transform.localEulerAngles = new Vector3(0f, 180f, 0f);

        card.zone = Zones.Type.Deck;
        spawnQueue.Add(card);

        if (spawnQueue.Count == 1)
        {
            MoveCardToMonsterArea(card);
        }
    }

    public bool IsClear()
    {
        return cardsInZone.Count == 0;
    }

    public void MoveCardToMonsterArea(CardObject card)
    {
        card.transform.SetParent(transform);
        cardsInZone.Add(card);
        StartCoroutine(AnimateMoveCardToArea(card));
    }

    public void ShiftPositions(CardObject excluding)
    {
        for (int i = 0; i < cardsInZone.Count; i++)
        {
            if (cardsInZone[i] != excluding)
            {
                cardsInZone[i].transform.localPosition = PositionInArea(i, cardsInZone.Count);
                cardsInZone[i].transform.localEulerAngles = Vector3.zero;
            }
        }
    }

    public void RemoveMonster(CardObject card)
    {
        cardsInZone.Remove(card);
        EnemyDecks.main.RemoveMonster(card);
        Destroy(card.gameObject);
    }

    public void EndTurn()
    {
        foreach (CardObject card in cardsInZone)
        {
            card.OnEndTurn();
        }
    }

    Vector3 PositionInArea(int index, int cardCount)
    {
        //Set the offset manuall, then modify it by increments of 10;
        float offset = 0.2f;
        int mod = cardCount / 10;
        offset = offset / (mod + 1);

        //Find out the position of the current card and last card
        float endOffset = offset * (cardCount - 1);
        offset = offset * index;

        //Use the position of the last card to find out how much free space is left, then use this to centre the cards.
        float diff = (1f - endOffset) / 2;

        float t = offset + diff;

        Vector3 pos = Vector3.Lerp(leftMostPoint, rightMostPoint, t);
        return pos;
    }

    IEnumerator AnimateMoveCardToArea(CardObject card)
    {
        ShiftPositions(card);
        float timeElapsed = 0f;
        Vector3 startPos = card.transform.localPosition;
        Vector3 endPos = PositionInArea(cardsInZone.Count - 1, cardsInZone.Count);

        Vector3 startRot = card.transform.localEulerAngles;
        Vector3 endRot = Vector3.zero;

        while (timeElapsed < moveDuration)
        {
            card.transform.localPosition = Vector3.Lerp(startPos, endPos, timeElapsed / moveDuration);
            card.transform.localEulerAngles = Vector3.Lerp(startRot, endRot, timeElapsed / moveDuration);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        card.transform.localPosition = endPos;
        card.transform.localEulerAngles = endRot;

        card.zone = Zones.Type.Monster;
        card.transform.localEulerAngles = Vector3.zero;

        spawnQueue.Remove(card);
        if(spawnQueue.Count > 0) { MoveCardToMonsterArea(spawnQueue[0]); }
    }
}
