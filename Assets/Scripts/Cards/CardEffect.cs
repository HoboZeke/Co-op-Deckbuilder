using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardEffect : MonoBehaviour
{
    public enum EffectTag { None, Taunt, Attack, Stun, Recruit };
    public EffectTag effectTag;

    public enum EffectTargetTriggerType { None, OnPlay, OnActivate };
    public EffectTargetTriggerType targetTriggerType;

    public virtual void EnterPlay(CardObject version)
    {

    }

    public virtual void LeavePlay(CardObject version)
    {

    }

    public virtual void AttachToEffect(CardObject version, CardObject target)
    {

    }

    public virtual void DetachEffect(CardObject version, CardObject target)
    {

    }

    public virtual void EffectOnGainAttachment(CardObject version, CardObject target)
    {

    }

    public virtual void Effect(CardObject version)
    {

    }

    public virtual void EffectOnTarget(CardObject version, CardObject target)
    {

    }

    public virtual void EndOfTurn(CardObject version)
    {

    }

    public virtual void OnTakeDamage(CardObject version)
    {

    }
}
