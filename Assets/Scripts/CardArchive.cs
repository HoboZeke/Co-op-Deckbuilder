using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardArchive : MonoBehaviour
{
    public static CardArchive main;

    public Card[] cards;
    public GameObject cardObjectPrefab;

    [SerializeField] List<int> treasureCardIndices = new List<int>();
    List<int> regularCardIndices = new List<int>();

    private void Awake()
    {
        main = this;
    }

    private void Start()
    {
        for(int i = 0; i < cards.Length; i++)
        {
            if (!treasureCardIndices.Contains(i))
            {
                regularCardIndices.Add(i);
            }
        }
    }

    public int[] RegularCardIndicies()
    {
        return regularCardIndices.ToArray();
    }

    public void SpawnCard(int i, Player player)
    {
        GameObject obj = Instantiate(cardObjectPrefab);
        CardObject card = obj.GetComponent<CardObject>();
        cards[i].JoinToObject(card);
        card.zoneScript = player.zones;

        player.zones.deck.AddCardToDeck(card);
        player.AddCardToPlayerDeckList(card);
    }

    public GameObject SpawnRandomLooseCard()
    {
        return SpawnLooseCard(regularCardIndices[Random.Range(0, regularCardIndices.Count)]);
    }

    public GameObject SpawnRandomLooseTreasure()
    {
        return SpawnLooseCard(treasureCardIndices[Random.Range(0, treasureCardIndices.Count)]);
    }

    public GameObject SpawnLooseCard(int i)
    {
        GameObject obj = Instantiate(cardObjectPrefab);
        CardObject card = obj.GetComponent<CardObject>();
        cards[i].JoinToObject(card);
        card.referenceIndex = i;

        return obj;
    }

    public int CardIndex(Card c)
    {
        for(int i = 0; i < cards.Length; i++)
        {
            if(c == cards[i]) { return i; }
        }

        return -1;
    }
}
