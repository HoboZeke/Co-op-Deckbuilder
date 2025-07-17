using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.EventSystems;

public class CardObject : MonoBehaviour
{
    public Card myCard;
    public int referenceIndex;
    public Zones zoneScript;

    [SerializeField] Image banner;
    [SerializeField] Image background;
    [SerializeField] Image frame;
    public Image cardArt;
    public Image attackOverlay;
    [SerializeField] Image healthOverlay;
    [SerializeField] Image shieldOverlay;
    [SerializeField] Image stunOverlay;
    public TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI cardText;
    public TextMeshProUGUI attackText;
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] TextMeshProUGUI shieldText;

    [SerializeField] GameObject topCornerTab, topCornerTabEmpty, bottomCornerTab, bottomCornerTabEmpty;

    [SerializeField] Image cardBackOverlay;
    [SerializeField] TextMeshProUGUI cardBackUnavailableDurationText;

    [SerializeField] Zones.Type zone;
    Zones.Type movingToZone;
    [SerializeField] Color[] typeColours;
    [SerializeField] Material[] typeMats;
    [SerializeField] Material concealedMat;
    [SerializeField] Color itemColour;
    [SerializeField] Color backgroundColour, leaderBackgroundColour, itemBackgroundColour, enemyBackgroundColour;

    [SerializeField] float animationDuration;

    [SerializeField] int attachments = 1;
    public List<CardObject> attachedCards = new List<CardObject>();
    [SerializeField] bool equippedCard;
    [HideInInspector] public bool attachmentTargetMode;

    [Header("Effects")]
    [SerializeField] Image effectOverlay;
    [SerializeField] Sprite[] effectSprites;
    [SerializeField] float effectAnimationDuration;

    [Header("Collider")]
    [SerializeField] BoxCollider myCollider;
    [SerializeField] Vector3 handColliderSize, baseColliderSize;

    Vector3 previousPosition;
    Vector3 previousRotation;
    bool isHighlighted;
    bool isAnimating;
    bool isLeader;
    int health;
    int preEffectDamage;
    int armour;

    [Header("Modifiers")]
    [SerializeField] int extraAttack;
    [SerializeField] List<CardEffect> extraEffects = new List<CardEffect>();
    public int[] cardLootValues; //Treasure, People, Weapons

    public bool isStunned;
    public bool isTapped;

    public void Setup(Card card)
    {
        myCard = card;
        cardArt.sprite = card.art;
        nameText.text = card.name;
        gameObject.name = card.name;

        ConstructCardDescription();
        SetupCornerTabs();

        if(card.tag == Card.Tag.Leader)
        {
            background.color = leaderBackgroundColour;
        }
        else if (card.tag == Card.Tag.Item)
        {
            background.color = itemBackgroundColour;
        }
        else if (card.tag == Card.Tag.Enemy)
        {
            background.color = enemyBackgroundColour;
        }

        banner.color = typeColours[(int)card.cardType];
        ChangeTabMat(typeMats[(int)card.cardType]);
        frame.color = banner.color;
        if (myCard.tag == Card.Tag.Item) { banner.color = itemColour; }
        cardLootValues = card.cardLoot;
    }

    void ChangeTabMat(Material newMat)
    {
        topCornerTab.GetComponent<MeshRenderer>().material = newMat;
        topCornerTabEmpty.GetComponent<MeshRenderer>().material = newMat;
        bottomCornerTab.GetComponent<MeshRenderer>().material = newMat;
        bottomCornerTabEmpty.GetComponent<MeshRenderer>().material = newMat;
    }

    void SetupCornerTabs()
    {
        if (myCard.attackValue > 0 || myCard.tag == Card.Tag.Person)
        {
            attackOverlay.gameObject.SetActive(true);
            attackText.text = AttackValue().ToString();
            topCornerTab.SetActive(true);
            topCornerTabEmpty.SetActive(false);
        }
        else
        {
            attackOverlay.gameObject.SetActive(false);
            topCornerTab.SetActive(false);
            topCornerTabEmpty.SetActive(true);
        }

        if (myCard.tag == Card.Tag.Enemy)
        {
            health = myCard.healthValue;

            healthOverlay.gameObject.SetActive(true);
            healthText.text = health.ToString();
            bottomCornerTab.SetActive(true);
            bottomCornerTabEmpty.SetActive(false);

            if (myCard.armourValue > 0)
            {
                armour = myCard.armourValue;

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
            bottomCornerTab.SetActive(false);
            bottomCornerTabEmpty.SetActive(true);
        }
    }

    void HideCornerTabs()
    {
        topCornerTab.SetActive(false);
        topCornerTabEmpty.SetActive(true);
        bottomCornerTab.SetActive(false);
        bottomCornerTabEmpty.SetActive(true);
    }

    void ShowCornerTabs()
    {
        if (myCard.attackValue > 0 || myCard.tag == Card.Tag.Person)
        {
            topCornerTab.SetActive(true);
            topCornerTabEmpty.SetActive(false);
        }
        else
        {
            topCornerTab.SetActive(false);
            topCornerTabEmpty.SetActive(true);
        }

        if (myCard.tag == Card.Tag.Enemy)
        {
            bottomCornerTab.SetActive(true);
            bottomCornerTabEmpty.SetActive(false);
        }
        else
        {
            bottomCornerTab.SetActive(false);
            bottomCornerTabEmpty.SetActive(true);
        }
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
        if (preEffectDamage > 0) { preEffectDamage -= amount; }
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

            if(myCard.gainCoinOnDefeat > 0)
            {
                GameController.main.AllPlayersGainCoins(myCard.gainCoinOnDefeat);
            }
        }
        else
        {
            OnTakeDamage();
        }
    }

    void OnTakeDamage()
    {
        foreach (CardEffect effect in CardEffects())
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

    public string CardDescription()
    {
        return cardText.text;
    }

    void ConstructCardDescription()
    {
        //Get all Descriptions
        List<string> effectDescriptions = new List<string>();
        effectDescriptions.AddRange(myCard.RawAbilityText());
        foreach(CardEffect effect in extraEffects) { effectDescriptions.Add(effect.DescriptionText()); }

        for(int i = effectDescriptions.Count - 1; i >= 0; i--)
        {
            string s = effectDescriptions[i];
            if (s == "" || s == " " || s == string.Empty)
            {
                effectDescriptions.Remove(s);
            }
        }


        //Split them into individual words
        List<string[]> splitDescriptions = new List<string[]>();
        for (int i = 0; i < effectDescriptions.Count; i++)
        {
            splitDescriptions.Add(effectDescriptions[i].Split(' '));
        }

        //Pull any words which have numbers before them and add them to a list
        List<string> wordsBeforeNumbers = new List<string>();
        List<int> wordValue = new List<int>();
        for(int i = 0; i < splitDescriptions.Count; i++)
        {
            for(int j = 1; j < splitDescriptions[i].Length; j++)
            {
                //If the word is a number put the word before on the list, if already on the list increase the number value associated with the word.
                if (int.TryParse(splitDescriptions[i][j], out int answer))
                {
                    string word = splitDescriptions[i][j-1];
                    if (wordsBeforeNumbers.Contains(word))
                    {
                        wordValue[wordsBeforeNumbers.IndexOf(word)] += answer;
                    }
                    else
                    {
                        wordsBeforeNumbers.Add(word);
                        wordValue.Add(answer);
                    }
                }
            }
        }

        //Build the final string.
        List<string> wordsAlreadyIncldued = new List<string>();
        string finalString = string.Empty;
        List<string> lines = new List<string>();
        for(int i = 0; i < splitDescriptions.Count; i++)
        {
            string line = "";

            for (int j = 0; j < splitDescriptions[i].Length; j++)
            {
                string word = splitDescriptions[i][j];

                if (wordsAlreadyIncldued.Contains(word))
                {
                    //Skip this word and the next one as already handled.
                    j++;
                }
                else
                {
                    if (AbilityKeywords.IsStringAKeyword(word)) { line += "<b>"; }

                    if (wordsBeforeNumbers.Contains(word))
                    {
                        //add the word and the cumulative value to the string then skip next word as will be old number.
                        line += word + " " + wordValue[wordsBeforeNumbers.IndexOf(word)] + " ";
                        wordsAlreadyIncldued.Add(word);
                        j++;
                    }
                    else
                    {
                        line += word + " ";
                    }

                    if (AbilityKeywords.IsStringAKeyword(word)) { line += "</b>"; }
                }
            }


            if (!string.Equals(line, "\n") && line != "")
            {
                lines.Add(line);
            }
        }

        for (int i = 0; i < lines.Count; i++)
        {
            if (i > 0) { finalString += "\n" + lines[i]; }
            else { finalString += lines[i]; }
        }

        Debug.Log("Final String: " + finalString);
        cardText.text = finalString.Trim();
    }

    public string CardShield()
    {
        return shieldText.text;
    }

    public int TurnsUnavailable()
    {
        return int.Parse(cardBackUnavailableDurationText.text);
    }

    public void ResetTurnsUnavailable()
    {
        cardBackUnavailableDurationText.text = 0.ToString();
        TickTurnForUnavailableCounter(1);
    }

    public void MarkCardAsUnavailableForTurns(int turns)
    {
        cardBackUnavailableDurationText.text = (TurnsUnavailable() + turns).ToString();
        cardBackOverlay.gameObject.SetActive(true);
    }

    public void TickTurnForUnavailableCounter(int tick)
    {
        int newCount = TurnsUnavailable() - tick;
        if (newCount < 0) { newCount = 0; }
        if (newCount == 0)
        {
            cardBackUnavailableDurationText.text = 0.ToString();
            cardBackOverlay.gameObject.SetActive(false);
        }
        else
        {
            cardBackUnavailableDurationText.text = newCount.ToString();
            cardBackOverlay.gameObject.SetActive(true);
        }
    }

    public Color[] ColourProfile()
    {
        List<Color> profile = new List<Color>();

        profile.Add(banner.color);
        profile.Add(frame.color);
        profile.Add(cardArt.color);
        profile.Add(background.color);

        return profile.ToArray();
    }

    public Material TabMaterial()
    {
        return topCornerTab.GetComponent<MeshRenderer>().material;
    }

    public void SetZone(Zones.Type newZone, Zones.Type targetZone)
    {
        zone = newZone;
        movingToZone = targetZone;
    }

    public void SetZone(Zones.Type newZone)
    {
        if(newZone == Zones.Type.Moving) { Debug.LogError("Card placed in moving zone but not told where they are moving too!"); }

        zone = newZone;
        if (!IsInVisibleZone())
        {
            ChangeTabMat(concealedMat);
            HideCornerTabs();
        }
        else
        {
            ChangeTabMat(typeMats[(int)myCard.cardType]);
            ShowCornerTabs();
        }
    }

    public Zones.Type Zone()
    {
        return zone;
    }

    public List<CardEffect> CardEffects()
    {
        List<CardEffect> effects = new List<CardEffect>();
        effects.AddRange(myCard.cardEffects);
        effects.AddRange(extraEffects);
        return effects;
    }

    public List<CardEffect> ExtraCardEffects()
    {
        return extraEffects;
    }

    public void AddEffectToCard(CardEffect newEffect)
    {
        extraEffects.Add(newEffect);
        ConstructCardDescription();
    }

    public void AddEffectToCard(CardEffect[] newEffectArray)
    {
        extraEffects.AddRange(newEffectArray);
        ConstructCardDescription();
    }

    public void RemoveEffectFromCard(CardEffect effect)
    {
        if (extraEffects.Contains(effect))
        {
            extraEffects.Remove(effect);
            ConstructCardDescription();
        }
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

    public bool IsInVisibleZone()
    {
        switch (zone)
        {
            case Zones.Type.Loose:
                return true;
            case Zones.Type.Hand: 
                return true;
            case Zones.Type.Location:
                return true;
            case Zones.Type.Play:
                return true;
            case Zones.Type.Ability:
                return true;
            case Zones.Type.Discard:
                return true;
            case Zones.Type.Equipped:
                return true;
            case Zones.Type.Monster:
                return true;
            case Zones.Type.Leader:
                return true;
            case Zones.Type.Recruitment:
                return true;
            default:
                bool extraEffectsAnswer = false;
                foreach (CardEffect cardEffect in myCard.cardEffects)
                {
                    if (cardEffect.IsVisibleExtraConditions(this)) { extraEffectsAnswer = true; break; }
                }
                return extraEffectsAnswer;
        }
    }

    public bool HasOpenAttachmentSlots()
    {
        return attachedCards.Count < attachments;
    }

    public bool Attach(CardObject equipment, bool forceAttach)
    {
        if (!HasOpenAttachmentSlots() && !forceAttach)
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
        AttachTo(target, false);
    }

    public void AttachTo(CardObject target, bool forceAttach)
    {
        if (target.Attach(this, forceAttach))
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
        if (zone == Zones.Type.Ability) { zoneScript.ability.TurnOffAbility(); return; }

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
        if (UIController.main.UIHasFocus()) { return; }

        //If this isn't the active players card return
        if (zoneScript != Zones.main && zone != Zones.Type.Monster && zone != Zones.Type.Recruitment) { return; }

        if(zone == Zones.Type.Hand && !isAnimating && !InputController.main.Busy())
        {
            if (!isHighlighted)
            {
                isHighlighted = true;
                previousPosition = zoneScript.hand.CardHandPos(this);
                previousRotation = transform.localEulerAngles;
                StopAllCoroutines();
                isAnimating = false;
                StartCoroutine(MoveToHighlight(new Vector3(transform.localPosition.x, transform.localPosition.y + 1f, transform.localPosition.z - 2f)));
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (IsInVisibleZone())
            {
                UIController.main.ShowZoomedCard(this);
            }
            else if(zone == Zones.Type.Deck)
            {
                zoneScript.deck.OpenDeckViewer();
            }
        }
    }

    private void OnMouseDown()
    {
        if (UIController.main.UIHasFocus()) { return; }

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
                isAnimating = false;
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
            Debug.Log("Trying to activate ability card.");
            if (zoneScript.ability.IsAvailable())
            {
                Debug.Log("Card available");
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

        Debug.Log(gameObject.name + " didn't trigger any response");
    }

    private void OnMouseExit()
    {
        if (zone == Zones.Type.Hand)
        {
            if (isHighlighted)
            {
                isHighlighted = false;
                StopAllCoroutines();
                isAnimating = false;
                StartCoroutine(MoveToHighlight(previousPosition));
            }
        }
    }

    private void LateUpdate()
    {
        foreach (CardEffect effect in CardEffects())
        {
            effect.VersionLateUpdate(this);
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

    public AbilityKeywords.Keyword[] GetKeywords()
    {
        List<AbilityKeywords.Keyword> keywords = new List<AbilityKeywords.Keyword>();

        keywords.AddRange(myCard.CardKeywords());

        foreach(CardEffect e in extraEffects)
        {
            keywords.AddRange(e.Keywords());
        }

        return keywords.ToArray();
    }
}
