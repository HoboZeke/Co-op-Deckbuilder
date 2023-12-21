using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddEffectOnAttach : CardEffect
{
    public CardEffect effectToAdd;

    public override void AttachToEffect(CardObject version, CardObject target)
    {
        target.extraEffects.Add(effectToAdd);
    }

    public override void DetachEffect(CardObject version, CardObject target)
    {
        target.extraEffects.Remove(effectToAdd);
    }
}
