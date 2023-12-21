using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquippedZone : MonoBehaviour
{
    public List<CardObject> cardsInZone = new List<CardObject>();
    public Vector3 equippedCardPlacementOffset;

    public float animationDuration;

    public void ClearAllAttachedCardsBackIntoDeck(Deck deck)
    {
        foreach(CardObject card in cardsInZone)
        {
            card.DetachCard();
        }
        deck.ShuffleMultipleCardsIntoDeck(cardsInZone);
        cardsInZone.Clear();
    }

    public void AnimateCardBeingEquipped(CardObject equipment, CardObject target)
    {
        StartCoroutine(MoveCardToEquippedPosition(equipment, target));
    }

    IEnumerator MoveCardToEquippedPosition(CardObject equipment, CardObject target)
    {
        float timeElapsed = 0f;
        Transform t = equipment.transform;
        Vector3 endPos = equippedCardPlacementOffset * (target.attachedCards.Count);
        Vector3 startPos = t.localPosition;
        Vector3 endRot = Vector3.zero;
        Vector3 startRot = t.localEulerAngles;

        while(timeElapsed < animationDuration)
        {
            t.localPosition = Vector3.Lerp(startPos, endPos, timeElapsed / animationDuration);
            t.localEulerAngles = Vector3.Lerp(startRot, endRot, timeElapsed / animationDuration);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        t.localPosition = endPos;
        t.localEulerAngles = endRot;

    }
}
