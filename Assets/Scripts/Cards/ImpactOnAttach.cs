using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactOnAttach : CardEffect
{
    public int strength;

    public override void AttachToEffect(CardObject version, CardObject target)
    {
        //Make a new list so if cards die midway through it doesn't change the list.
        List<CardObject> targets = new List<CardObject>();
        targets.AddRange(GameController.main.ActiveZone().monsterArea.cardsInZone);
        foreach (CardObject card in targets)
        {
            card.TakeProvisionalDamage(strength, false);
            EffectsManager.main.ConcurrentDamageToAnotherCardEffect(version, card, strength, false);
        }
    }
}
