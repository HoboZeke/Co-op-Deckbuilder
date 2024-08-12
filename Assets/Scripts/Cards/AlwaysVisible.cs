using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlwaysVisible : CardEffect
{
    [SerializeField] Vector3 visibleRotEuler;

    public override void VersionLateUpdate(CardObject version)
    {
        if (version.Zone() == Zones.Type.Deck)
        {
            version.transform.localEulerAngles = new Vector3(version.transform.localEulerAngles.x, visibleRotEuler.y, version.transform.localEulerAngles.z);
        }
    }

    public override bool IsVisibleExtraConditions(CardObject version)
    {
        return true;
    }
}
