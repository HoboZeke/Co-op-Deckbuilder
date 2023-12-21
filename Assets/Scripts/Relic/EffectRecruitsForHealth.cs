using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectRecruitsForHealth : RelicEffect
{
    public override void ApplyEffect(Player toPlayer)
    {
        toPlayer.recruits += strength;
        toPlayer.ChangeMaxHealth(-strength * 2);
    }
}
