using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : CardEffect
{
    public bool pierce;

    public override void EffectOnTarget(CardObject version, CardObject target)
    {
        target.TakeProvisionalDamage(version.AttackValue(), pierce);
        EffectsManager.main.DamageToAnotherCardEffect(version, target, version.AttackValue(), pierce);
    }
}
