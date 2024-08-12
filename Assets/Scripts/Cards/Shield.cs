using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : CardEffect
{
    public override void EnterPlay(CardObject version)
    {
        GameController.main.ActivePlayer().GainShield(strength);
    }
}
