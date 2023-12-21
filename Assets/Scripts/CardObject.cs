using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardObject : MonoBehaviour
{
    public Card myCard;
    public int referenceIndex;
    public Zones zoneScript;

    public Image banner;
    public Image frame;
    public Image cardArt;
    public Image attackOverlay;
    public Image healthOverlay;
    public Image shieldOverlay;
    public Image stunOverlay;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI cardText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI shieldText;

    public Zones.Type zone;
    Zones.Type movingToZone;
    public Color[] typeColours;
    public Color itemColour;

    public float animationDuration;

    public int attachments = 1;
    public List<CardObject> attachedCards = new List<CardObject>();
    public bool equippedCard;
    [HideInInspector] public bool attachmentTargetMode;

    [Header("Effects")]
    public Image effectOverlay;
    public Sprite[] effectSprites;
    public float effectAnimationDuration;

    [Header("Collider")]
    public BoxCollider myCollider;
    public Vector3 handColliderSize, baseColliderSize;

    Vector3 previousPosition;
    Vector3 previousRotation;
    bool isHighlighted;
    bool isAnimating;
    bool isLeader;
    int health;
    int preEffectDamage;
    int armour;

    [Header("Modifiers")]
    public int extraAttack;
    public List<CardEffect> extraEffects = new List<CardEffect>();
    public int[] cardLootValues; //Treasure, People, Weapons

    public bool isStunned;
    public bool isTapped;

    public void Setup(Card card)
    {
        myCard = card;
        cardArt.sprite = card.art;
        nameText.text = card.name;
        cardText.text = card.abilityText;
        gameObject.name = card.name;

        if(card.attackValue > 0 || card.tag == Card.Tag.Person)
        {
            attackOverlay.gameObject.SetActive(true);
            attackText.text = AttackValue().ToString();
        }
        else
        {
            attackOverlay.gameObject.SetActive(false);
        }

        if(card.tag == Card.Tag.Enemy)
        {
            health = card.healthValue;

            healthOverlay.gameObject.SetActive(true);
            healthText.text = health.ToString();

            if(card.armourValue > 0)
            {
                armour = card.armourValue;

                shieldOverlay.gameObject.SetActive(true);
                shieldText.text = armour.ToString();
            }
            else
            {
                shieldOverlay.gameObject.SetActive(false);
            }
        }
        else
        {
            healthOverlay.gameObject.SetActive(false);
            shieldOverlay.gameObject.SetActive(false);
        }

        banner.color = typeColours[(int)card.cardType];
        frame.color = banner.color;
        if (myCard.tag == Card.Tag.Item) { banner.color = itemColour; }
        cardLootValues = card.cardLoot;
    }

    public void SetAsLeaderCard()
    {
        isLeader = true;
    }

    public bool IsLeader()
    {
        return isLeader;
    }

    public bool IsAlive()
    {
        return preEffectDamage < health;
    }

    public void TakeProvisionalDamage(int amount, bool pierce)
    {
        if (pierce)
        {
            preEffectDamage += amount;
        }
        else
        {
            if(amount > armour) { preEffectDamage += amount - armour; }
        }
    }

    public void TakeDamage(int amount, bool pierce)
    {   
        //Clear up any preffect damage
        if(preEffectDamage > 0) { preEffectDamage -= amount; }
        if(preEffectDamage < 0) { preEffectDamage = 0; }


        if(health <= 0) { return; }
        if (!pierce)
        {
            amount = amount - armour;
            if (amount <= 0)
            {
                amount = 0;
                effectOverlay.sprite = effectSprites[1];
                StartCoroutine(PlayEffectAnimation());
            }
            else
            {
                effectOverlay.sprite = effectSprites[2];
                StartCoroutine(PlayEffectAnimation());
            }
        }

        health -= amount;
        healthText.text = health.ToString();

        if (pierce)
        {
            effectOverlay.sprite = effectSprites[2];
            StartCoroutine(PlayEffectAnimation());
        }
        else if (amount > 0)
        {
            effectOverlay.sprite = effectSprites[0];
            StartCoroutine(PlayEffectAnimation());
        }

        if(health <= 0) {
            if(zone == Zones.Type.Monster) { zoneScript.monsterArea.RemoveMonster(this); }
            else if(zone == Zones.Type.Location) { zoneScript.location.RemoveMonster(this); }
        }
        else
        {
            OnTakeDamage();
        }
    }

    void OnTakeDamage()
    {
        foreach(CardEffect effect in CardEffects())
        {
            effect.OnTakeDamage(this);
        }
    }

    public void Stun(bool status)
    {
        isStunned = status;
        stunOverlay.gameObject.SetActive(status);
    }

    public void MoveToMonsterZone(MonsterArea area)
    {
        if (zone == Zones.Type.Monster) { zoneScript.monsterArea.cardsInZone.Remove(this); }
        else if( zone == Zones.Type.Location) { zoneScript.location.cardsInZone.Remove(this); }
        area.MoveCardToMonsterArea(this);
    }

    public void AdjustAttackMod(int amount)
    {
        AdjustAttackMod(amount, true);
    }

    public void AdjustAttackMod(int amount, bool updateServer)
    {
        extraAttack += amount;
        attackText.text = AttackValue().ToString();
        if (MutliplayerController.active.IsMultiplayerGame())
        {
            if(zoneScript.player == Player.active && updateServer)
            {
                Client.active.TellServerIHaveAdjustedACardAttack(referenceIndex, amount);
            }
        }

        if (AttackValue() > 0)
        {
            attackOverlay.gameObject.SetActive(true);
            attackText.text = AttackValue().ToString();
        }
        else
        {
            attackOverlay.gameObject.SetActive(false);
        }
    }

    public int AttackValue()
    {
        return myCard.attackValue + extraAttack;
    }

    public int HealthValue()
    {
        return health;
    }

    public string CardName()
    {
        return myCard.name;
    }

    public void SetZone(Zones.Type newZone, Zones.Type targetZone)
    {
        zone = newZone;
        movingToZone = targetZone;
    }

    public List<CardEffect> CardEffects()
    {
        List<CardEffect> effects = new List<CardEffect>();
        effects.AddRange(myCard.cardEffects);
        effects.AddRange(extraEffects);
        return effects;
    }

    public List<CardEffect.EffectTag> OnPlayCardEffectTags()
    {
        List<CardEffect.EffectTag> tags = new List<CardEffect.EffectTag>();
        foreach(CardEffect e in CardEffects())
        {
            if (e.targetTriggerType == CardEffect.EffectTargetTriggerType.OnPlay)
            {
                if (!tags.Contains(e.effectTag))
                {
                    tags.Add(e.effectTag);
                }
            }
        }
        return tags;
    }

    public List<CardEffect.EffectTag> OnActivateCardEffectTags()
    {
        List<CardEffect.EffectTag> tags = new List<CardEffect.EffectTag>();
        foreach (CardEffect e in CardEffects())
        {
            if (e.targetTriggerType == CardEffect.EffectTargetTriggerType.OnActivate)
            {
                if (!tags.Contains(e.effectTag))
                {
                    tags.Add(e.effectTag);
                }
            }
        }
        return tags;
    }

    public void FaceTowards(Transform target)
    {
        transform.LookAt(target);
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x * -1f, transform.localEulerAngles.y + 180f, transform.localEulerAngles.z);
        if(zoneScript.player != Player.active && isLeader) { transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 0f); }
    }

    public void MoveCollider(bool inHand)
    {
        if (inHand)
        {
            myCollider.size = handColliderSize;
            myCollider.center = new Vector3(0f, -0.5f, 0f);
        }
        else
        {
            myCollider.size = baseColliderSize;
            myCollider.center = Vector3.zero;

        }
    }

    public bool HasOpenAttachmentSlots()
    {
        return attachedCards.Count < attachments;
    }

    public bool Attach(CardObject equipment)
    {
        if(!HasOpenAttachmentSlots())
        {
            Debug.Log("ERROR Trying to attach to a card which doesn't have any attachment slots left");
            return false;
        }

        attachedCards.Add(equipment);
        equipment.OnAttachTo(this);
        myCard.OnGainAttachment(this, equipment);
        return true;
    }

    public void AttachTo(CardObject target)
    {
        if (target.Attach(this))
        {
            Debug.Log("Successfully Attached");
            equippedCard = true;

            if (zone == Zones.Type.Hand)
            {
                zoneScript.hand.cardsInHand.Remove(this);
                zoneScript.hand.ShiftHand(null);
            }


            zone = Zones.Type.Equipped;
            transform.SetParent(target.transform);
            zoneScript.equip.cardsInZone.Add(this);
            zoneScript.equip.AnimateCardBeingEquipped(this, target);
        }
    }

    public void OnAttachTo(CardObject target)
    {
        myCard.OnAttachTo(this, target);
    }

    public void AttachedCardHasLeftPlay(CardObject attachedCard)
    {
        Detach(attachedCard);
        equippedCard = false;
        transform.SetParent(zoneScript.discard.transform);
        zoneScript.equip.cardsInZone.Remove(this);
        zoneScript.discard.MoveCardToDiscard(this);
    }

    public void DetachCard()
    {
        if(equippedCard) Detach(transform.parent.GetComponent<CardObject>());
    }

    void Detach(CardObject target)
    {
        if (target.attachedCards.Contains(this))
        {
            myCard.OnDetachFrom(this, target);
            target.attachedCards.Remove(this);
            equippedCard = false;
        }
    }

    public void Tap()
    {
        if(zone == Zones.Type.Ability) { zoneScript.ability.TurnOffAbility(); return; }

        isTapped = true;
        StartCoroutine(TapAnimation());
        Client.active.SendCardUpdate(referenceIndex, NetworkCardUpdate.StateType.TappedInPlay);
    }

    public void RemoteCallTap()
    {
        isTapped = true;
        StartCoroutine(TapAnimation());
    }

    public void ShiftPosition()
    {
        isHighlighted = false;
    }

    public void OnEnterHand()
    {
        MoveCollider(true);
    }

    public void OnEnterPlay()
    {
        AudioManager.main.ChooseAudioEventForCardPlayed(this);
        myCard.OnEnterPlay(this);
        StatTracker.local.CardPlayed(this, zoneScript.player);
    }

    public void OnLeaveHand()
    {
        MoveCollider(false);
        isHighlighted = false;
    }

    public void OnLeavePlay()
    {
        isTapped = false;

        foreach(CardEffect effect in CardEffects())
        {
            effect.LeavePlay(this);
        }

        if(attachedCards.Count > 0)
        {
            //Make a temp list so that they can be removed from the main list during the foreach loop
            List<CardObject> attachCards = new List<CardObject>(attachedCards);
            foreach(CardObject attachment in attachCards)
            {
                attachment.AttachedCardHasLeftPlay(this);
            }
        }
    }

    public void OnEndTurn()
    {
        myCard.EndOfTurn(this);
    }

    public void TargetFound(GameObject target)
    {
        if (myCard.ValidateTarget(target, this))
        {
            InputController.main.ExitTargetMode();
        }
    }

    public int[] CardLoot()
    {
        return cardLootValues;
    }

    private void OnMouseOver()
    {
        //If this isn't the active players card return
        if(zoneScript != Zones.main && zone != Zones.Type.Monster && zone != Zones.Type.Recruitment) { return; }

        if(zone == Zones.Type.Hand && !isAnimating && !InputController.main.Busy())
        {
            if (!isHighlighted)
            {
                isHighlighted = true;
                previousPosition = zoneScript.hand.CardHandPos(this);
                previousRotation = transform.localEulerAngles;
                StopAllCoroutines();
                StartCoroutine(MoveToHighlight(new Vector3(transform.localPosition.x, transform.localPosition.y + 1f, transform.localPosition.z - 2f)));
            }
        }
    }

    private void OnMouseDown()
    {
        //If this isn't the active players card return
        if (zoneScript != Zones.main && zone != Zones.Type.Monster && zone != Zones.Type.Location && zone != Zones.Type.Recruitment) { return; }
        if (GameController.main.phase == GameController.Phase.Waiting) { return; }

        Debug.Log("Clicked on card " + gameObject.name);

        //Explore Mode
        if(GameController.main.phase == GameController.Phase.Explore)
        {
            Location.active.CardObjectInEventPressed(this);
            return;
        }

        //Discard Phase choose this card to discard
        if(GameController.main.phase == GameController.Phase.Discard)
        {
            if(zone == Zones.Type.Hand)
            {
                isHighlighted = false;
                StopAllCoroutines();
                zoneScript.hand.DiscardCard(this);
                if (MutliplayerController.active.IsMultiplayerGame()) { Client.active.SendCardUpdate(referenceIndex, NetworkCardUpdate.StateType.HandToDiscard); }
            }
            return;
        }
        //Recruit phase if this is in recruitment try to recruit it.
        else if (GameController.main.phase == GameController.Phase.Recruit && zone == Zones.Type.Recruitment)
        {
            int recruitDeckCardRef = referenceIndex; //Store this value as the reference will get overriden by the player deck ref in the RecruitCard() function below.
            if (GameController.main.ActiveZone().RecruitCard(this))
            {
                if (MutliplayerController.active.IsMultiplayerGame()) { Client.active.SendCardUpdate(recruitDeckCardRef, NetworkCardUpdate.StateType.Recruit); Debug.Log("Sending Recruitment ID - " + recruitDeckCardRef); }
            }
            return;
        }

        //If game is waiting for a target try this one.
        if (InputController.main.TargetMode())
        {
            InputController.main.SubmitTarget(gameObject);
            return;
        }

        //Play card from hand
        if(zone == Zones.Type.Hand && isHighlighted && !isAnimating)
        {
            if (myCard.needTargetToPlay)
            {
                attachmentTargetMode = true;
                InputController.main.EnterTargetMode(gameObject);
            }
            else
            {
                myCard.OnClickedInHand(this);
            }
        }
        else if(zone == Zones.Type.Leader && isLeader)
        {
            myCard.OnClickedInLeaderZone(this);
        }
        else if(zone == Zones.Type.Play && !isAnimating && !isTapped)
        {
            if (myCard.needTargetForActivate)
            {
                InputController.main.EnterTargetMode(gameObject);
            }
            else
            {
                myCard.OnClickedInPlay(this);
            }
        }
        else if(zone == Zones.Type.Ability)
        {
            if (zoneScript.ability.IsAvailable())
            {
                if (myCard.needTargetForActivate)
                {
                    InputController.main.EnterTargetMode(gameObject);
                }
                else
                {
                    //Nothing yet
                }
            }
        }
    }

    private void OnMouseExit()
    {
        if (zone == Zones.Type.Hand)
        {
            if (isHighlighted)
            {
                isHighlighted = false;
                StopAllCoroutines();
                StartCoroutine(MoveToHighlight(previousPosition));
            }
        }
    }

    IEnumerator MoveToHighlight(Vector3 position)
    {
        Debug.Log("Moving to highlight");
        isAnimating = true;
        float timeElapsed = 0f;
        Vector3 start = transform.localPosition;

        while(timeElapsed < animationDuration)
        {
            transform.localPosition = Vector3.Lerp(start, position, timeElapsed / animationDuration);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = position;
        isAnimating = false;
    }

    IEnumerator TapAnimation()
    {
        isAnimating = true;
        float timeElapsed = 0f;
        Vector3 start = transform.localEulerAngles;
        Vector3 end = new Vector3(0f, 0f, -90f);

        while (timeElapsed < animationDuration)
        {
            transform.localEulerAngles = Vector3.Lerp(start, end, timeElapsed / animationDuration);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.localEulerAngles = end;
        isAnimating = false;
    }

    IEnumerator PlayEffectAnimation()
    {
        float timeElapsed = 0f;

        effectOverlay.gameObject.SetActive(true);
        effectOverlay.color = new Color(effectOverlay.color.r, effectOverlay.color.g, effectOverlay.color.b, 0f);

        while(timeElapsed < effectAnimationDuration)
        {
            float a = Mathf.Lerp(0f, 1f, timeElapsed / effectAnimationDuration);
            effectOverlay.color = new Color(effectOverlay.color.r, effectOverlay.color.g, effectOverlay.color.b, a);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        timeElapsed = 0f;

        while (timeElapsed < effectAnimationDuration)
        {
            float a = Mathf.Lerp(1f, 0f, timeElapsed / effectAnimationDuration);
            effectOverlay.color = new Color(effectOverlay.color.r, effectOverlay.color.g, effectOverlay.color.b, a);

            timeElapsed += Time.deltaTime;
            yield return null;
        }


        effectOverlay.color = new Color(effectOverlay.color.r, effectOverlay.color.g, effectOverlay.color.b, 0f);
        effectOverlay.gameObject.SetActive(false);
    }
    
    public string ZoneAsString()
    {
        if(zone == Zones.Type.Moving)
        {
            return movingToZone.ToString();
        }
        return zone.ToString();
    }
}
