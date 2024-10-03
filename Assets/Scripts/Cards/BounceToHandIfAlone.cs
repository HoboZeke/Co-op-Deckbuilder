using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceToHandIfAlone : CardEffect
{
    public override void EndOfTurn(CardObject version)
    {
        CardObject[] cardsInPLay = version.zoneScript.play.CardsInPLay();

        bool AmIAlone = true;

        for (int i = 0; i < cardsInPLay.Length; i++)
        {
            if (cardsInPLay[i] != version)
            {
                if (cardsInPLay[i].myCard.tag == Card.Tag.Person)
                {
                    AmIAlone = false;
                    break;
                }
            }
        }

        if (AmIAlone)
        {
            version.zoneScript.play.cardsInPlay.Remove(version);
            version.zoneScript.hand.MoveCardToHand(version);

            if (version.attachedCards.Count > 0)
            {
                //Make a temp list so that they can be removed from the main list during the foreach loop
                List<CardObject> attachCards = new List<CardObject>(version.attachedCards);
                foreach (CardObject attachment in attachCards)
                {
                    attachment.AttachedCardHasLeftPlay(version);
                }
            }
        }
    }
}
