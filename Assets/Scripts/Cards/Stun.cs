using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stun : CardEffect
{
    public override void EffectOnTarget(CardObject version, CardObject target)
    {
        if (target.myCard.IsImmune(effectTag)) { return; }
        target.Stun(true);
    }
}
