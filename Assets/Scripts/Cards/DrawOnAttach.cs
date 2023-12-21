using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawOnAttach : CardEffect
{
    public override void AttachToEffect(CardObject version, CardObject target)
    {
        version.zoneScript.deck.DrawCard();
    }
}
