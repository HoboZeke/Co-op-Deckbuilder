using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackEqualToCardsInPlay : CardEffect
{
    Dictionary<CardObject, int> attackBonusGiven = new Dictionary<CardObject, int>();

    public override void EnterPlay(CardObject version)
    {
        version.AdjustAttackMod(version.zoneScript.play.cardsInPlay.Count, false);
        attackBonusGiven.Add(version, version.zoneScript.play.cardsInPlay.Count);
        if (version.AttackValue() > 0)
        {
            version.attackOverlay.gameObject.SetActive(true);
            version.attackText.text = version.AttackValue().ToString();
        }
        else
        {
            version.attackOverlay.gameObject.SetActive(false);
        }
    }

    public override void LeavePlay(CardObject version)
    {
        version.AdjustAttackMod(-attackBonusGiven[version], false);
        attackBonusGiven.Remove(version);
        if (version.AttackValue() > 0)
        {
            version.attackOverlay.gameObject.SetActive(true);
            version.attackText.text = version.AttackValue().ToString();
        }
        else
        {
            version.attackOverlay.gameObject.SetActive(false);
        }
    }
}
