using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldOnGainAttachment : CardEffect
{
    public override void EffectOnGainAttachment(CardObject version, CardObject target)
    {
        GameController.main.ActivePlayer().GainShield(strength);
    }
}
