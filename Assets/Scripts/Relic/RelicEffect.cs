using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class RelicEffect : MonoBehaviour
{
    public int strength;
    public List<Relic> attachedRelics = new List<Relic>();

    public virtual void AttachEffectToRelic(Relic relic)
    {
        attachedRelics.Add(relic);
    }

    public virtual void ApplyEffect(Player toPlayer)
    {

    }

    public virtual void RemoveEffectFromRelic(Relic relic)
    {
        attachedRelics.Remove(relic);
    }
}
