using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreedyForItems : CardEffect
{
    List<CardObject> items = new List<CardObject>();

    public override void EnterPlay(CardObject version)
    {
        items.Clear();
        items = version.zoneScript.equip.AllEquippedCards();

        foreach (CardObject item in items)
        {
            item.DetachCard();
            version.zoneScript.equip.AnimateCardBeingEquipped(item, version);
            item.AttachTo(version, true);
        }
    }
}
