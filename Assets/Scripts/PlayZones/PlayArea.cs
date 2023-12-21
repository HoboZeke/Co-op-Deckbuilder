using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayArea : MonoBehaviour
{
    public List<CardObject> cardsInPlay = new List<CardObject>();
    public Vector3 leftMostPoint, rightMostPoint;

    public float moveDuration;
    

    public void MoveCardToPlayArea(CardObject card)
    {
        card.transform.SetParent(transform);
        cardsInPlay.Add(card);
        StartCoroutine(AnimatedMoveCardToPlayArea(card));
        if (card.IsLeader() && card.zoneScript.player != Player.active) { card.zoneScript.player.UseRecruits(1); }
    }

    public void ShiftPositions(CardObject excluding)
    {
        for (int i = 0; i < cardsInPlay.Count; i++)
        {
            if (cardsInPlay[i] != excluding)
            {
                cardsInPlay[i].transform.localPosition = PositionInPlay(i, cardsInPlay.Count);
                if(!cardsInPlay[i].isTapped) cardsInPlay[i].transform.localEulerAngles = Vector3.zero;
                
            }
        }
    }

    public void EndTurn()
    {
        foreach(CardObject card in cardsInPlay)
        {
            card.OnEndTurn();
        }
    }

    public void ClearZone()
    {
        foreach (CardObject card in cardsInPlay)
        {
            Destroy(card.gameObject);
        }
        cardsInPlay.Clear();
    }

    Vector3 PositionInPlay(int index, int cardCount)
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

    IEnumerator AnimatedMoveCardToPlayArea(CardObject card)
    {
        ShiftPositions(card);
        float timeElapsed = 0f;
        Vector3 startPos = card.transform.localPosition;
        Vector3 endPos = PositionInPlay(cardsInPlay.Count - 1, cardsInPlay.Count);

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

        card.zone = Zones.Type.Play;
        card.transform.localEulerAngles = Vector3.zero;
        card.OnEnterPlay();
    }
}
