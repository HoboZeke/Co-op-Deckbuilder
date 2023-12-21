using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Card
{
    public string name;
    public string abilityText;
    public Sprite art;
    public int attackValue;
    public int healthValue;
    public int armourValue;

    public enum Tag { Enemy, Person, Item, Leader, Ability };
    public Tag tag;

    public enum CardType { Physical, Magical, Monster, Ability, Treasure };
    public CardType cardType;

    public bool needTargetForActivate;
    public bool needTargetToPlay;

    public CardType[] validCardTypes;
    public Tag[] validTargetTags;
    public Zones.Type[] validZones;
    public bool canTargetInactive, mustTargetMyCards;

    public CardEffect.EffectTag[] immunities;

    public CardEffect[] cardEffects;
    public int[] cardLoot; //Trasure, People, Weapons

    public void JoinToObject(CardObject obj)
    {
        obj.Setup(this);
    }

    public void OnClickedInHand(CardObject version)
    {
        Debug.Log(version.name + " Card click in hand");
        if(tag == Tag.Person)
        {
            if (!version.zoneScript.player.CanPlayPerson()) { Debug.Log("Tried to play " +version.name + " but playerScript said no"); return; }
            else { version.zoneScript.player.UseRecruits(1); }
        }

        version.SetZone(Zones.Type.Moving, Zones.Type.Play);
        version.OnLeaveHand();
        version.zoneScript.hand.cardsInHand.Remove(version);
        version.zoneScript.hand.ShiftHand(null);

        version.zoneScript.play.MoveCardToPlayArea(version);
    }

    public void OnClickedInLeaderZone(CardObject version)
    {
        Debug.Log(version.name + " Card click in leader zone");
        if (version.zoneScript.leader.leaderSpent) { return; }

        if (!version.zoneScript.player.CanPlayPerson()) { Debug.Log("Tried to play " + version.name + " but playerScript said no"); return; }
        else { version.zoneScript.player.UseRecruits(1); }

        version.SetZone(Zones.Type.Moving, Zones.Type.Play);
        version.OnLeaveHand();

        version.zoneScript.play.MoveCardToPlayArea(version);

    }

    public void OnClickedInPlay(CardObject version)
    {
        if (!version.isTapped)
        {
            version.Tap();
            Activate(version);
            if (MutliplayerController.active.IsMultiplayerGame())
            {
                if (version.zoneScript == Zones.main) //If zone is equl to Zones.main then it is one of the local players cards.
                {
                    Client.active.SendCardUpdate(version.referenceIndex, NetworkCardUpdate.StateType.TappedInPlay);
                }
            }
        }
    }

    public bool ValidateTarget(GameObject target, CardObject version)
    {
        if (target.CompareTag("Card"))
        {
            CardObject script = target.GetComponent<CardObject>();

            if (!script.IsAlive() && script.myCard.tag == Card.Tag.Enemy) { Debug.Log("WRONG TARGET Target card is dead, might only be in play to allow an effect to conclude"); return false; }

            //Check the card is in the right Zone
            if (mustTargetMyCards)
            {
                if (script.zoneScript != Zones.main) { Debug.Log("WRONG TARGET Target card isn't one of mine");  return false; }
            }
            else
            {
                if (script.zoneScript != GameController.main.ActiveZone() && !canTargetInactive && script.zone != Zones.Type.Location ) { Debug.Log("WRONG TARGET Target card isn't in the active zone");  return false; }
            }

            //Check the card is of the right card type
            if(validCardTypes.Length > 0)
            {
                bool hasTheRightType = false;
                foreach(CardType type in validCardTypes)
                {
                    if(script.myCard.cardType == type) { hasTheRightType = true; }
                }
                if (!hasTheRightType) { Debug.Log("WRONG TARGET target card doesn't have the right CardType"); return false; }
            }

            //Check the target tags.
            if (!version.attachmentTargetMode)
            {
                //Check if the target is immune to all effects
                List<CardEffect.EffectTag> tags = new List<CardEffect.EffectTag>();
                foreach(CardEffect e in version.CardEffects())
                {
                    if(e.targetTriggerType == CardEffect.EffectTargetTriggerType.OnActivate) { tags.Add(e.effectTag); }
                }

                bool isImmuneToAllEffects = true;
                foreach(CardEffect.EffectTag t in tags)
                {
                    if (!script.myCard.IsImmune(t)) { isImmuneToAllEffects = false; }
                }

                if (isImmuneToAllEffects) { Debug.Log("WRONG TARGET is immune to all the effects"); return false; }

                //Now Check each tag
                foreach (Tag t in validTargetTags)
                {
                    if (t == script.myCard.tag)
                    {
                        foreach (Zones.Type type in validZones)
                        {
                            if (type == script.zone)
                            {
                                version.Tap();
                                ActivateWithTarget(version, script);
                                return true;
                            }
                            //If they have targetted a location monster and they have no monsters left in front of them. (Only if monsterzone is usually a valid target
                            else if (type == Zones.Type.Monster && GameController.main.ActiveZone().monsterArea.IsClear())
                            {
                                if (script.zone == Zones.Type.Location)
                                {
                                    version.Tap();
                                    ActivateWithTarget(version, script);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (!script.HasOpenAttachmentSlots()) { Debug.Log("WRONG TARGET Target card can't take any more attachments"); return false; }
                
                //Check if the target is immune to all effects
                List<CardEffect.EffectTag> tags = new List<CardEffect.EffectTag>();
                foreach (CardEffect e in version.CardEffects())
                {
                    if (e.targetTriggerType == CardEffect.EffectTargetTriggerType.OnPlay) { tags.Add(e.effectTag); }
                }

                bool isImmuneToAllEffects = true;
                foreach (CardEffect.EffectTag t in tags)
                {
                    if (!script.myCard.IsImmune(t)) { isImmuneToAllEffects = false; }
                }

                if (isImmuneToAllEffects) { Debug.Log("WRONG TARGET is immune to all the effects"); return false; }

                //Now Check Tags
                foreach (Tag t in validTargetTags)
                {
                    if (t == script.myCard.tag)
                    {
                        foreach (Zones.Type type in validZones)
                        {
                            if (type == script.zone)
                            {
                                AttachToTarget(version, script);
                                return true;
                            }
                            //If they have targetted a location monster and they have no monsters left in front of them. (Only if monsterzone is usually a valid target)
                            else if (type == Zones.Type.Monster && GameController.main.ActiveZone().monsterArea.IsClear())
                            {
                                if (script.zone == Zones.Type.Location)
                                {
                                    AttachToTarget(version, script);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
        }

        Debug.Log("WRONG TARGET Target card not picked up by if statements. Probably Wrong Tags");
        return false;
    }

    public bool IsImmune(CardEffect.EffectTag tag)
    {
        Debug.Log("Testing Immunity of effect " + tag.ToString() + " against card " + name);
        bool answer = false;
        foreach(CardEffect.EffectTag t in immunities)
        {
            if (tag == t && t != CardEffect.EffectTag.None) { answer = true; }
        }
        Debug.Log("Immunity found to be: " + answer.ToString());
        return answer;
    }

    public void OnAttachTo(CardObject version, CardObject target)
    {
        if (cardEffects != null)
        {
            foreach (CardEffect effect in cardEffects)
            {
                effect.AttachToEffect(version, target);
            }
        }
        if (version.extraEffects != null)
        {
            foreach (CardEffect effect in version.extraEffects)
            {
                effect.AttachToEffect(version, target);
            }
        }
    }

    public void OnDetachFrom(CardObject version, CardObject target)
    {
        if (cardEffects != null)
        {
            foreach (CardEffect effect in cardEffects)
            {
                effect.DetachEffect(version, target);
            }
        }
        if (version.extraEffects != null)
        {
            foreach (CardEffect effect in version.extraEffects)
            {
                effect.DetachEffect(version, target);
            }
        }
    }

    public void OnGainAttachment(CardObject version, CardObject target)
    {
        if (cardEffects != null)
        {
            foreach (CardEffect effect in cardEffects)
            {
                effect.EffectOnGainAttachment(version, target);
            }
        }
        if (version.extraEffects != null)
        {
            foreach (CardEffect effect in version.extraEffects)
            {
                effect.EffectOnGainAttachment(version, target);
            }
        }
    }

    public void OnEnterPlay(CardObject version)
    {
        if (cardEffects != null)
        {
            foreach (CardEffect effect in cardEffects)
            {
                effect.EnterPlay(version);
            }
        }
        if (version.extraEffects != null)
        {
            foreach (CardEffect effect in version.extraEffects)
            {
                effect.EnterPlay(version);
            }
        }


        if (MutliplayerController.active.IsMultiplayerGame())
        {
            if (version.zoneScript == Zones.main) //If zone is equal to Zones.main then it is one of the local players cards.
            {
                Client.active.SendCardUpdate(version.referenceIndex, NetworkCardUpdate.StateType.HandToPlay);
            }
        }
    }

    public void Activate(CardObject version)
    {
        if (cardEffects != null)
        {
            foreach (CardEffect effect in cardEffects)
            {
                effect.Effect(version);
            }
        }
        if (version.extraEffects != null)
        {
            foreach (CardEffect effect in version.extraEffects)
            {
                effect.Effect(version);
            }
        }
        AudioManager.main.ChooseAudioEventForCardActivated(version);

        if (MutliplayerController.active.IsMultiplayerGame())
        {
            if (version.zoneScript == Zones.main) //If zone is equl to Zones.main then it is one of the local players cards.
            {
                Client.active.SendCardUpdate(version.referenceIndex, NetworkCardUpdate.StateType.Activate);
            }
        }
    }

    public void ActivateWithTarget(CardObject version, CardObject target)
    {
        if (cardEffects != null)
        {
            foreach (CardEffect effect in cardEffects)
            {
                effect.EffectOnTarget(version, target);
            }
        }
        if (version.extraEffects != null)
        {
            foreach (CardEffect effect in version.extraEffects)
            {
                effect.EffectOnTarget(version, target);
            }
        }

        if (version.zone == Zones.Type.Ability) { version.zoneScript.ability.TurnOffAbility(); }


        if (MutliplayerController.active.IsMultiplayerGame())
        { 
            if (version.zoneScript == Zones.main) //If zone is equl to Zones.main then it is one of the local players cards.
            {
                Client.active.SendCardUpdate(version.referenceIndex, NetworkCardUpdate.StateType.ActivatedWithTarget, target.zone.ToString() + "|" + target.referenceIndex);
            }
        }
    }

    public void AttachToTarget(CardObject version, CardObject target)
    {
        version.AttachTo(target);

        if (MutliplayerController.active.IsMultiplayerGame())
        {
            if (version.zoneScript == Zones.main) //If zone is equl to Zones.main then it is one of the local players cards.
            {
                Client.active.SendCardUpdate(version.referenceIndex, NetworkCardUpdate.StateType.AttachedToTarget, target.zone.ToString() + "|" + target.referenceIndex);
            }
        }
    }

    public void EndOfTurn(CardObject version)
    {
        if (version.isStunned)
        {
            version.Stun(false);
            return;
        }

        if (tag == Tag.Enemy)
        {
            if (version.zone == Zones.Type.Monster)
            {
                GameController.main.ActivePlayer().LoseHealth(version.AttackValue());
                EffectsManager.main.ConcurrentDamageToPlayerBoatEffect(version, version.zoneScript.player, version.AttackValue(), false);
            }
            else if(version.zone == Zones.Type.Location)
            {
                Location.active.DoDamgeToNode(version.HealthValue());
            }
        }

        if (cardEffects != null)
        {
            foreach (CardEffect effect in cardEffects)
            {
                effect.EndOfTurn(version);
            }
        }
        if(version.extraEffects != null)
        {
            foreach (CardEffect effect in version.extraEffects)
            {
                effect.EndOfTurn(version);
            }
        }
    }
}
