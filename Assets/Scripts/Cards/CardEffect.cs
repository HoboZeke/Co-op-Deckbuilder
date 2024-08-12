using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardEffect : MonoBehaviour
{
    public enum EffectTag { None, Taunt, Attack, Stun, Recruit };
    public EffectTag effectTag;

    public enum EffectTargetTriggerType { None, OnPlay, OnActivate };
    public EffectTargetTriggerType targetTriggerType;

    [SerializeField] string abilityString;
    [SerializeField] AbilityKeywords.Keyword[] keywords;
    public int strength;

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

    public virtual void VersionLateUpdate(CardObject version)
    {

    }

    public virtual bool IsVisibleExtraConditions(CardObject version)
    {
        return false;
    }

    public virtual AbilityKeywords.Keyword[] Keywords()
    {
        return keywords;
    }

    public virtual string DescriptionText()
    {
        string s = abilityString;
        string[] splits = s.Split(' ');

        for(int i = 0; i < splits.Length; i++)
        {
            switch(splits[i])
            {
                case "[str]":
                    splits[i] = strength.ToString();
                    break;
            }
        }

        s = string.Join(" ", splits);

        return s;
    }
}
