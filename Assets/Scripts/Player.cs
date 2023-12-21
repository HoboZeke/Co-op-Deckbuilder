using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{
    public static Player active;
    public Zones zones;
    public string playerName;

    public int health;
    int maxHealth;
    public TextMeshPro healthText;
    public HealthHolder healthHolder;

    public int shield;
    public TextMeshPro shieldText;
    public ShieldHolder shieldHolder;

    public int recruits;
    public TextMeshPro recruitText;
    public RecruitHolder recruitHolder;
    int remainingRecruits;
    
    public int coins;
    public TextMeshPro coinsText;
    public CoinHolder coinHolder;

    public int[] startingDeck;
    List<CardObject> cardReferenceList = new List<CardObject>();

    public List<Relic> relics = new List<Relic>();

    public int leader;
    public ClanSelection.Clan clan;

    public int storedTreasure;
    public int storedPopulation;
    public int storedWeapons;

    private void Awake()
    {
        if (active == null)
        {
            active = this;
        }
    }

    public void SetupAsPlayerProxy(Zones zone)
    {
        Debug.Log("Setting up a Player Proxy");
        zones = zone;
        zones.player = this;
        zones.recruit = Zones.main.recruit;
        zones.location = Zones.main.location;
        NewTurn();
    }

    public void UpdateFromLobbyChoices(PlayerLobbySettings settings)
    {
        leader = settings.leader;
        clan = (ClanSelection.Clan)settings.clan;
        zones.leader.Setup(leader);
        playerName = settings.name;
    }
    
    void Start()
    {
        UpdateResourceSettings(Settings.main.resourceDisplay);
        maxHealth = health;
        UpdateHealthText();
        StatTracker.local.AddPlayerToStatTracker(this);
    }

    public void ReplaceCardInPlayerDeckList(CardObject oldCard, CardObject newCard)
    {
        Debug.Log("Replacing " + oldCard.name + " with " + newCard.name);

        for(int i = 0; i < cardReferenceList.Count; i++)
        {
            if(cardReferenceList[i] == oldCard)
            {
                cardReferenceList[i] = newCard;
                RemoveCardFromZoneList(oldCard);
                Destroy(oldCard.gameObject);
                newCard.zoneScript = zones;
                newCard.referenceIndex = i;
                return;
            }
        }

        Debug.Log("OOOOOOPS!!!! Trying to replace a card which doesn't exist!");
    }

    public void AddCardToPlayerDeckList(CardObject card)
    {
        cardReferenceList.Add(card);
        card.referenceIndex = cardReferenceList.Count - 1;
        card.zoneScript = zones;
    }

    public CardObject[] CopyOfPlayerRefList()
    {
        return cardReferenceList.ToArray();
    }

    public void RemoveCardFromPlayerDeckList(CardObject card)
    {
        RemoveCardFromPlayerDeckList(card, false);
    }

    public void RemoveCardFromPlayerDeckList(CardObject card, bool dontUpdateServer)
    {
        if (!cardReferenceList.Contains(card)) { Debug.Log("OOOOPS!!! Trying to remove a card from player reference list that doesn't exist."); return; }
        if (MutliplayerController.active.IsMultiplayerGame() && !dontUpdateServer)
        {
            if(Player.active == this) { Client.active.TellServerIHaveLostACard(GetCardRefForCard(card)); }
        }

        RemoveCardFromZoneList(card);
        cardReferenceList.Remove(card);

        for(int i = 0; i < cardReferenceList.Count; i++)
        {
            cardReferenceList[i].referenceIndex = i;
        }

        Destroy(card.gameObject);
    }

    public void RemoveCardByRefFromPlayerDeckList(int cardRef)
    {
        RemoveCardFromPlayerDeckList(GetCardByRef(cardRef));
    }

    void RemoveCardFromZoneList(CardObject card)
    {
        Debug.Log("Removing card current in zone " + card.zone.ToString());
        switch (card.zone)
        {
            case Zones.Type.Deck:
                zones.deck.cardsInDeck.Remove(card);
                break;
            case Zones.Type.Hand:
                zones.hand.cardsInHand.Remove(card);
                break;
            case Zones.Type.Play:
                zones.play.cardsInPlay.Remove(card);
                break;
            case Zones.Type.Discard:
                zones.discard.cardsInDiscard.Remove(card);
                break;
        }
    }

    public void UpdateLeader(int newLeaderRef)
    {
        leader = newLeaderRef;
        zones.leader.Setup(newLeaderRef);
        zones.leader.RefreshLeader();
    }

    public CardObject GetCardByRef(int cardRef)
    {
        if (cardRef == -1) return zones.leader.leaderCard;
        else if (cardRef == -2) return zones.ability.abilityCard;
        else if (cardRef >= cardReferenceList.Count)
        {
            Debug.Log("Trying to get ref " + cardRef + " but not that many cards in the ref list!"); return null;
        }
        return cardReferenceList[cardRef];
    }

    public int GetCardRefForCard(CardObject card)
    {
        if (card == zones.leader.leaderCard) return -1;
        else if (card == zones.ability.abilityCard) return -2;
        else
        {
            for(int i = 0; i < cardReferenceList.Count; i++)
            {
                if(cardReferenceList[i] == card) { return i; }
            }

            Debug.Log("ERROR requesting card ref but card not found in my ref list! Card Name: " + card.name);
            return -50;
        }
    }

    public void StartBattle()
    {
        zones.deck.BuildFromDeckList(startingDeck);
        Client.active.TellServerIHaveBuiltMyDeckList(startingDeck);

        zones.deck.DrawCards(5);
        NewTurn();

    }

    public void StartRound()
    {
        GainCoins(3);
        zones.deck.DrawCards(5);
        NewTurn();
    }

    public void NewTurn()
    {
        remainingRecruits = recruits;
        UpdateRecruitsText();

        ResetShield();
    }

    public void LoseHealth(int amount)
    {
        if(shield > 0)
        {
            int shieldValue = shield;
            shield -= amount;

            if(shield <= 0)
            {
                amount -= shieldValue;
                shield = 0;
                UpdateShieldText();
            }
            else
            {
                UpdateShieldText();
                return;
            }
            
        }

        health -= amount;
        UpdateHealthText();

        if(health <= 0)
        {
            GameController.main.LoseGame(this);
        }
    }

    public void GainHealth(int amount)
    {
        health += amount;
        if(health > maxHealth) { health = maxHealth;  }
        UpdateHealthText();
    }

    public void GainShield(int amount)
    {
        shield += amount;
        UpdateShieldText();
    }

    void ResetShield()
    {
        shield = 0;
        UpdateShieldText();
    }

    public void ChangeMaxHealth(int amount)
    {
        maxHealth += amount;
        if (health > maxHealth) { health = maxHealth; }
        UpdateHealthText();
    }

    public void UseRecruits(int amount)
    {
        remainingRecruits -= amount;
        UpdateRecruitsText();
    }

    public bool CanPlayPerson()
    {
        return remainingRecruits > 0;
    }

    public void GainRecruits(int amount)
    {
        remainingRecruits += amount;
        UpdateRecruitsText();
    }

    public bool CanAffordRecruit()
    {
        if(coins > 0)
        {
            coins--;
            zones.recruit.LoseACoin();
            UpdateCoinText();
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public void GainCoins(int amount)
    {
        coins += amount;
        UpdateCoinText();
    }

    public void GainRelic(Relic relic)
    {
        relics.Add(relic);
    }

    public int[] LootCount()
    {
        int treasureCount = 0;
        int popCount = 0;
        int weaponCount = 0;

        foreach(CardObject card in cardReferenceList)
        {
            int[] loot = card.cardLootValues;
            treasureCount += loot[0];
            popCount += loot[1];
            weaponCount += loot[2];
        }

        if (!HomeBase.main.peopleStorageUnlocked) { popCount = 0; }
        if (!HomeBase.main.weaponStorageUnlocked) { weaponCount = 0; }

        return new int[] { treasureCount, popCount, weaponCount };
    }

    public int StoredLoot(int type)
    {
        switch (type)
        {
            case 0:
                return storedTreasure;
            case 1:
                return storedPopulation;
            case 2:
                return storedWeapons;
        }
        return 0;
    }

    public void UpdateResourceSettings(Settings.ResourceDisplay setting)
    {
        switch (setting)
        {
            case Settings.ResourceDisplay.Icons:
                healthText.gameObject.SetActive(false);
                shieldText.gameObject.SetActive(false);
                recruitText.gameObject.SetActive(false);
                coinsText.gameObject.SetActive(false);
                break;
            case Settings.ResourceDisplay.Numbers:
                healthHolder.gameObject.SetActive(false);
                shieldHolder.gameObject.SetActive(false);
                recruitHolder.gameObject.SetActive(false);
                coinHolder.gameObject.SetActive(false);
                break;
        }
    }

    public void UpdateHealthText()
    {
        healthText.text = "Health: " + health;
        healthHolder.UpdateHealthHolder(health);
    }

    public void UpdateShieldText()
    {
        if (shield <= 0) { shieldText.text = ""; }
        else { shieldText.text = "Shield: " + shield; }
        shieldHolder.UpdateShieldHolder(shield);
    }

    public void UpdateRecruitsText()
    {
        recruitText.text = "Recruits: " + remainingRecruits;
        recruitHolder.UpdateRecruitHolder(remainingRecruits);
    }

    public void UpdateCoinText()
    {
        coinsText.text = "Coins: " + coins;
        coinHolder.UpdateCoinHolder(coins);
    }

    public Vector3 BoatModelLocation()
    {
        return zones.sailRenderer.transform.parent.position;
    }
}
