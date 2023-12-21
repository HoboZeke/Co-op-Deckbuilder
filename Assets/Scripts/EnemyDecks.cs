using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDecks : MonoBehaviour
{
    public static EnemyDecks main;

    public Card[] beastsDeck;
    public Card[] seaCreatureDeck;
    public Card[] bossCards;
    public GameObject cardObjectPrefab;

    public float delayBetweenSpawns;

    public int playerSpawns, locationSpawns;

    Dictionary<int, int[]> playerSpawnList = new Dictionary<int, int[]>();
    List<CardObject> monsterRef = new List<CardObject>();

    private void Awake()
    {
        main = this;
    }

    public void StartBattle()
    {
        if (!MutliplayerController.active.IsMultiplayerGame())
        {
            SpawnRandomBeastsToPlayer(playerSpawns, Player.active);
            SpawnRandomBeastsToLocation(locationSpawns);
        }
        else 
        {
            MultiplayerStartBattle();
        }
    }

    void MultiplayerStartBattle()
    {
        playerSpawnList.Clear();

        for (int i = 0; i < MutliplayerController.active.playerCount; i++)
        {
            int[] array = new int[playerSpawns];
            for(int s = 0; s < array.Length; s++) {
                array[s] = Random.Range(0, beastsDeck.Length); 
            }
            
            playerSpawnList.Add(i, array);
        }
        int[] locationMonsters = new int[locationSpawns];
        for (int s = 0; s < locationMonsters.Length; s++)
        {
            locationMonsters[s] = Random.Range(0, beastsDeck.Length);
        }

        SpawnMultiplayerMonsters(playerSpawnList, locationMonsters);
        Client.active.PassMonsterSpawnsToServer(playerSpawnList, locationMonsters);
    }

    public void StartBossBattle()
    {
        if (!MutliplayerController.active.IsMultiplayerGame())
        {
            SpawnRandomBeastsToPlayer(playerSpawns, Player.active);
            SpawnBossToLocation(1);
        }
        else
        {
            MultiplayerStartBossBattle();
        }
    }

    void MultiplayerStartBossBattle()
    {
        playerSpawnList.Clear();

        for (int i = 0; i < MutliplayerController.active.playerCount; i++)
        {
            int[] array = new int[playerSpawns];
            for (int s = 0; s < array.Length; s++)
            {
                array[s] = Random.Range(0, beastsDeck.Length);
            }

            playerSpawnList.Add(i, array);
        }
        int[] locationMonsters = new int[1];
        for (int s = 0; s < locationMonsters.Length; s++)
        {
            locationMonsters[s] = -1;
        }

        SpawnMultiplayerMonsters(playerSpawnList, locationMonsters);
        Client.active.PassMonsterSpawnsToServer(playerSpawnList, locationMonsters);
    }

    public CardObject GetCardWithRef(int cardRef)
    {
        return monsterRef[cardRef];
    }

    public CardObject[] RefArray()
    {
        return monsterRef.ToArray();
    }

    public void SpawnMultiplayerMonsters(Dictionary<int, int[]> playerSpawns, int[] locationSpawns)
    {
        Debug.Log("Monster spawn dictionary length " + playerSpawns.Count);
        for (int i = 0; i < MutliplayerController.active.playerCount; i++)
        {
            Debug.Log("Checking for Player " + i);
            if (playerSpawns[i][0] != -1)
            {
                Debug.Log("Spawning Monsters for player " + i);
                SpawnBeastListForPlayer(MutliplayerController.active.GetPlayerScript(i), playerSpawns[i]);
            }
        }
        SpawnBeastListForLocation(locationSpawns);
    }


    public void SpawnBeastListForPlayer(Player player, int[] array)
    {
        for(int i = 0; i < array.Length; i++)
        {
            int beast = array[i];
            SpawnBeastCardToPlayer(beast, player);
        }
    }

    public void MultiplayerSpawnBeastCardToLocationDuringGame(int i)
    {
        Client.active.TellServerIHaveSpawnedBeastToLocation(i);
        SpawnBeastCardToLocation(i);
    }

    public void SpawnBeastListForLocation(int[] array)
    {
        //If the first entry is negative it means that it was a boss spawning.
        if(array[0] < 0)
        {
            SpawnBossToLocation(Mathf.Abs(array[0]));
            return;
        }

        for(int i = 0; i < array.Length; i++)
        {
            int beast = array[i];
            SpawnBeastCardToLocation(beast);
        }
    }

    public void MultiplayerSpawnBeastCardToPlayerDuringGame(int i, Player player)
    {
        Client.active.TellServerIHaveSpawnedBeastForPlayer(i, MutliplayerController.active.GetPlayerNumber(player));
        SpawnBeastCardToPlayer(i, player);
    }

    public void SpawnBeastCardToPlayer(int i, Player player)
    {
        GameObject obj = Instantiate(cardObjectPrefab);
        CardObject script = obj.GetComponent<CardObject>();
        beastsDeck[i].JoinToObject(script);

        monsterRef.Add(script);
        script.referenceIndex = monsterRef.Count - 1;
        script.nameText.text += " (" + script.referenceIndex + ")";

        player.zones.monsterArea.AddCardToArea(script);
        script.zoneScript = player.zones;
    }

    public void SpawnSeaCreatureCardToPlayer(int i, Player player)
    {
        GameObject obj = Instantiate(cardObjectPrefab);
        CardObject script = obj.GetComponent<CardObject>();
        seaCreatureDeck[i].JoinToObject(script);

        monsterRef.Add(script);
        script.referenceIndex = monsterRef.Count - 1;
        script.nameText.text += " (" + script.referenceIndex + ")";

        player.zones.monsterArea.AddCardToArea(script);
        script.zoneScript = player.zones;
    }

    public void SpawnBeastCardToLocation(int i)
    {
        GameObject obj = Instantiate(cardObjectPrefab);
        CardObject script = obj.GetComponent<CardObject>();
        beastsDeck[i].JoinToObject(script);

        monsterRef.Add(script);
        script.referenceIndex = monsterRef.Count - 1;
        script.nameText.text += " (" + script.referenceIndex + ")";
        script.zoneScript = Zones.main;

        Zones.main.location.AddCardToArea(script);
    }

    public void SpawnRandomBeastsToPlayer(int amount, Player player)
    {
        for(int i = 0; i < amount; i++)
        {
            SpawnBeastCardToPlayer(Random.Range(0, beastsDeck.Length), player);
        }
    }

    public void SpawnRandomBeastsToLocation(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            SpawnBeastCardToLocation(Random.Range(0, beastsDeck.Length));
        }
    }

    public void SpawnBossToLocation(int boss)
    {
        GameObject obj = Instantiate(cardObjectPrefab);
        CardObject script = obj.GetComponent<CardObject>();
        bossCards[boss-1].JoinToObject(script);

        monsterRef.Add(script);
        script.referenceIndex = monsterRef.Count - 1;
        script.zoneScript = Zones.main;
        
        Zones.main.location.AddBossCardToArea(script);
    }

    void UpdateCardRefs()
    {
        for(int i = 0; i < monsterRef.Count; i++)
        {
            monsterRef[i].referenceIndex = i;
        }
    }

    public void RemoveMonster(CardObject card)
    {
        monsterRef.Remove(card);
        UpdateCardRefs();

        if(monsterRef.Count == 0) { GameController.main.AllMonstersDefeated(); }
    }
    
}
