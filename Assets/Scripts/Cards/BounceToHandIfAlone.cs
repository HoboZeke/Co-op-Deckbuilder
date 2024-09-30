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
        }
    }
}
