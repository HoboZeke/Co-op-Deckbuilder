using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Impact : CardEffect
{
    public int strength;

    public override void EnterPlay(CardObject version)
    {
        //Make a new list so if cards die midway through it doesn't change the list.
        List<CardObject> targets = new List<CardObject>();
        targets.AddRange(GameController.main.ActiveZone().monsterArea.cardsInZone);

        for(int i = 0; i < targets.Count; i++)
        {
            targets[i].TakeProvisionalDamage(strength, false);
            EffectsManager.main.ConcurrentDamageToAnotherCardEffect(version, targets[i], strength, false);
        }
    }
}
