using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecruitOnAttach : CardEffect
{
    public override void AttachToEffect(CardObject version, CardObject target)
    {
        version.zoneScript.player.GainRecruits(1);
    }
}
