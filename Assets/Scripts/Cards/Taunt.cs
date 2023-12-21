using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Taunt : CardEffect
{
    public override void EffectOnTarget(CardObject version, CardObject target)
    {
        if (target.myCard.IsImmune(effectTag)) { return; }

        target.MoveToMonsterZone(version.zoneScript.monsterArea);
        target.zoneScript = version.zoneScript;
    }
}
