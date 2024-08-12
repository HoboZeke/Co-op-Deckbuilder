using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawOnPlay : CardEffect
{
    public override void EnterPlay(CardObject version)
    {
        for (int i = 0; i < strength; i++)
        {
            version.zoneScript.deck.DrawCard();
        }
    }
}
