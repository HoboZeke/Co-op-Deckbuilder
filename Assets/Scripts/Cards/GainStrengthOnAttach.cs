using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GainStrengthOnAttach : CardEffect
{
    public int strength;

    public override void AttachToEffect(CardObject version, CardObject target)
    {
        target.AdjustAttackMod(strength, false);
    }

    public override void DetachEffect(CardObject version, CardObject target)
    {
        target.AdjustAttackMod(-strength, false);
    }
}
