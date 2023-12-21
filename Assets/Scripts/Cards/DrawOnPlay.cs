using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawOnPlay : CardEffect
{
    public override void EnterPlay(CardObject version)
    {
        version.zoneScript.deck.DrawCard();
    }
}
