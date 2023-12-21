using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Zones : MonoBehaviour
{
    public static Zones main;

    public Player player;

    public enum Type { Deck, Hand, Play, Discard, Moving, Recruitment, RecruitmentDeck, Monster, Location, Leader, Ability, Equipped };

    public Deck deck;
    public Hand hand;
    public PlayArea play;
    public Discard discard;
    public LeaderZone leader;
    public Ability ability;
    public EquippedZone equip;

    public RecruitmentDeck recruit;

    public MonsterArea monsterArea;
    public LocationArea location;

    [Header("Visuals")]
    public Image boatImage;
    public MeshRenderer sailRenderer;
    public TextMeshPro nameText;

    private void Awake()
    {
        if(main == null) main = this;
    }

    public void SetupForGame()
    {
        SetupBoat();
        SetupName();
    }

    public void SweepPlayAreaIntoDiscard()
    {

        for(int i = play.cardsInPlay.Count -1; i >= 0; i--)
        {
            CardObject card = play.cardsInPlay[i];

            if (card.IsLeader())
            {
                card.transform.SetParent(leader.transform);
                leader.ReturnUsedLeaderCardToZone();
                card.OnLeavePlay();
                play.cardsInPlay.RemoveAt(i);
            }
            else
            {
                card.transform.SetParent(discard.transform);
                card.OnLeavePlay();
            }
        }
        discard.MoveMultipleCardsToDiscard(play.cardsInPlay);
        play.cardsInPlay.Clear();
    }

    public void ShuffleDiscardIntoDeck()
    {
        foreach (CardObject card in discard.cardsInDiscard)
        {
            card.transform.SetParent(deck.transform);
        }
        deck.ShuffleMultipleCardsIntoDeck(discard.cardsInDiscard);
        discard.cardsInDiscard.Clear();
    }

    public void ClearAllBackIntoDeck()
    {
        equip.ClearAllAttachedCardsBackIntoDeck(deck);

        if (discard.cardsInDiscard.Count > 0)
        {
            foreach (CardObject card in discard.cardsInDiscard)
            {
                card.transform.SetParent(deck.transform);
            }
            deck.ShuffleMultipleCardsIntoDeck(discard.cardsInDiscard);
            discard.cardsInDiscard.Clear();
        }
        if (hand.cardsInHand.Count > 0)
        {
            foreach (CardObject card in hand.cardsInHand)
            {
                card.transform.SetParent(deck.transform);
            }
            deck.ShuffleMultipleCardsIntoDeck(hand.cardsInHand);
            hand.cardsInHand.Clear();
        }
        if (play.cardsInPlay.Count > 0)
        {
            List<CardObject> nonLeaderCardsInPlay = new List<CardObject>();
            foreach (CardObject card in play.cardsInPlay)
            {
                if (card.IsLeader()) { leader.ReturnUsedLeaderCardToZone(); }
                else
                {
                    card.OnLeavePlay();
                    card.transform.SetParent(deck.transform);
                    nonLeaderCardsInPlay.Add(card);
                }
            }
            deck.ShuffleMultipleCardsIntoDeck(nonLeaderCardsInPlay);
            play.cardsInPlay.Clear();
        }

    }

    public void ClearZones()
    {
        deck.ClearZone();
        hand.ClearZone();
        play.ClearZone();
        discard.ClearZone();
        leader.leaderSpent = false;
    }

    public bool RecruitCard(CardObject card)
    {
        return recruit.RecruitCard(card, this);
    }

    #region Visuals
    void SetupBoat()
    {
        boatImage.sprite = HomeBase.main.ClanSprite((int)player.clan);
        sailRenderer.material = HomeBase.main.ClanMat((int)player.clan);
    }

    public void SetupName()
    {
        nameText.text = player.playerName;
    }
    #endregion
}
