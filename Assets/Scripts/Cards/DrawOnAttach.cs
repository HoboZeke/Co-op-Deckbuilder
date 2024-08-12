using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawOnAttach : CardEffect
{
    public override void AttachToEffect(CardObject version, CardObject target)
    {
        for (int i = 0; i < strength; i++) { }
        version.zoneScript.deck.DrawCard();
    }
}
