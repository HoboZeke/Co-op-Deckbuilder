using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecruitOnPlay : CardEffect
{
    public override void EnterPlay(CardObject version)
    {
        version.zoneScript.player.GainRecruits(1);
    }
}
