using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Discard : MonoBehaviour
{
    public Zones zone;
    public List<CardObject> cardsInDiscard = new List<CardObject>();
    public float cardWidth;

    public float animationDuration;

    public void StackDiscard()
    {
        if (cardsInDiscard.Count < 1) { return; }

        float highestCardZ = cardWidth * cardsInDiscard.Count;

        for (int i = 0; i < cardsInDiscard.Count; i++)
        {
            cardsInDiscard[i].transform.localPosition = new Vector3(0f, 0f, (highestCardZ - (i * cardWidth)) * -1f);
            cardsInDiscard[i].transform.localEulerAngles = new Vector3(0f, 0f, 0f);
        }
    }

    public void MoveMultipleCardsToDiscard(List<CardObject> cards)
    {
        foreach(CardObject card in cards)
        {
            MoveCardToDiscard(card);
        }
    }

    public void MoveCardToDiscard(CardObject card)
    {
        card.SetZone(Zones.Type.Moving, Zones.Type.Discard);
        card.transform.SetParent(transform);
        cardsInDiscard.Add(card);
        StartCoroutine(AnimateMoveCardToDiscard(card));
    }

    Vector3 TopOfDiscard()
    {
        return new Vector3(0f, 0f, (cardWidth * cardsInDiscard.Count) * -1f);
    }

    public void AddCardToDiscard(CardObject card)
    {
        card.transform.SetParent(transform);
        cardsInDiscard.Add(card);
        card.zone = Zones.Type.Discard;

        StackDiscard();
    }

    public void AddMultipleCardsToDiscard(List<CardObject> cards)
    {
        foreach (CardObject card in cards)
        {
            card.transform.SetParent(transform);
            cardsInDiscard.Add(card);
            card.zone = Zones.Type.Discard;
        }

        StackDiscard();
    }

    public void ClearZone()
    {
        foreach (CardObject card in cardsInDiscard)
        {
            Destroy(card.gameObject);
        }
        cardsInDiscard.Clear();
    }

    IEnumerator AnimateMoveCardToDiscard(CardObject card)
    {
        float timeElapsed = 0f;
        Vector3 start = card.transform.localPosition;
        Vector3 startRot = card.transform.localEulerAngles;

        Vector3 end = TopOfDiscard();
        Vector3 endRot = Vector3.zero;

        while(timeElapsed < animationDuration)
        {
            card.transform.localPosition = Vector3.Lerp(start, end, timeElapsed / animationDuration);
            card.transform.localEulerAngles = Vector3.Lerp(startRot, endRot, timeElapsed / animationDuration);

            timeElapsed += Time.deltaTime;
            yield return null;
        }


        card.transform.localPosition = end;
        card.transform.localEulerAngles = endRot;
        card.zone = Zones.Type.Discard;

    }
}
