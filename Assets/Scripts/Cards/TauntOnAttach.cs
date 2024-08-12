using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TauntOnAttach : CardEffect
{
    CardObject versionStore;

    public override void AttachToEffect(CardObject version, CardObject target)
    {
        if (target.zoneScript.player == Player.active)
        {
            InputController.main.EnterTargetMode(gameObject);
            Debug.Log("Asked Input Controller to go to target mode for items' taunt effect");
        }
        versionStore = version;
    }

    public void TargetFound(GameObject target)
    {
        CardObject t = target.GetComponent<CardObject>();

        if(t.myCard.tag == Card.Tag.Enemy)
        {
            if(t.Zone() == Zones.Type.Monster || t.Zone() == Zones.Type.Location)
            {
                if(!t.myCard.IsImmune(effectTag) && t.IsAlive())
                {
                    t.MoveToMonsterZone(t.zoneScript.monsterArea);
                    t.zoneScript = versionStore.zoneScript;
                    versionStore = null;
                    InputController.main.ExitTargetMode();
                }
            }
        }
    }
}
