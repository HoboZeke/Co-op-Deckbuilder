using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairOnGainAttachment : CardEffect
{
    public int strength;

    public override void EffectOnGainAttachment(CardObject version, CardObject target)
    {
        GameController.main.ActivePlayer().GainHealth(strength);
    }
}
