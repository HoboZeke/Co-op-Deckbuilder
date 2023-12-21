using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatTracker : MonoBehaviour
{
    public static StatTracker local;

    List<PlayerStats> playerStats = new List<PlayerStats>();

    public event Action OnCardPlayed;

    private void Awake()
    {
        local = this;
    }

    public void AddPlayerToStatTracker(Player player)
    {
        PlayerStats statHolder = new PlayerStats();
        statHolder.player = player;
        playerStats.Add(statHolder);
    }

    public void CardPlayed(CardObject card, Player player)
    {
        PlayerStats stats = StatsHolderForPlayer(player);

        stats.cardPlaysThisRound++;
        stats.cardPlaysThisBattle++;
        stats.playedCardTags.Add(card.myCard.tag);
        if(OnCardPlayed != null) OnCardPlayed();
    }

    public void NewRound()
    {
        foreach (PlayerStats stats in playerStats)
        {
            stats.cardPlaysThisRound = 0;
            stats.playedCardTags.Clear();
        }
    }

    public void NewBattle()
    {
        foreach (PlayerStats stats in playerStats)
        {
            stats.cardPlaysThisBattle = 0;
        }
        NewRound();
    }

    public int GetCardsPlayedThisRound(Player player)
    {
        return StatsHolderForPlayer(player).cardPlaysThisRound;
    }

    public int GetCardsPlayedThisBattle(Player player)
    {
        return StatsHolderForPlayer(player).cardPlaysThisBattle;
    }

    PlayerStats StatsHolderForPlayer(Player player)
    {
        foreach (PlayerStats playerStats in playerStats)
        {
            if(playerStats.player == player) return playerStats;
        }

        return null;
    }
}

[System.Serializable]
public class PlayerStats
{
    public Player player;

    public int cardPlaysThisRound;
    public int cardPlaysThisBattle;
    public List<Card.Tag> playedCardTags = new List<Card.Tag>();
}