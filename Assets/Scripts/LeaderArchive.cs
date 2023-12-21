using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderArchive : MonoBehaviour
{
    public static LeaderArchive main;

    public Card[] leaders;
    public GameObject cardObjectPrefab;

    private void Awake()
    {
        main = this;
    }

    public CardObject SpawnLeader(int i)
    {
        GameObject obj = Instantiate(cardObjectPrefab);
        CardObject card = obj.GetComponent<CardObject>();
        leaders[i].JoinToObject(card);
        return card;
    }

    public int[] IndicesOfLeaderLevelUps(int currentLeader)
    {
        List<int> indices = new List<int>();

        switch (currentLeader)
        {
            case 0:
                indices.Add(4);
                indices.Add(5);
                break;
            case 1:
                indices.Add(6);
                indices.Add(7);
                break;
            case 2:
                indices.Add(8);
                indices.Add(9);
                break;
            case 3:
                indices.Add(10);
                indices.Add(11);
                break;
            case 4:
                indices.Add(12);
                indices.Add(13);
                break;
            case 5:
                indices.Add(14);
                indices.Add(15);
                break;
            case 6:
                indices.Add(16);
                indices.Add(17);
                break;
            case 7:
                indices.Add(18);
                indices.Add(19);
                break;
            case 8:
                indices.Add(20);
                indices.Add(21);
                break;
            case 9:
                indices.Add(22);
                indices.Add(23);
                break;
            case 10:
                indices.Add(24);
                indices.Add(25);
                break;
            case 11:
                indices.Add(26);
                indices.Add(27);
                break;
            default:
                return null;
        }

        return indices.ToArray();
    }

    public CardObject[] SpawnLeaderLevelUps(int currentLeader)
    {
        List<CardObject> options = new List<CardObject>();
        int[] array = IndicesOfLeaderLevelUps(currentLeader);

        for(int i = 0; i < array.Length; i++)
        {
            CardObject obj = SpawnLeader(array[i]);
            options.Add(obj);
        }

        return options.ToArray();
    }
}
