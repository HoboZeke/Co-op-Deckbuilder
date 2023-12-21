using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Relics : MonoBehaviour
{
    public static Relics main;

    public Relic[] archive;

    private void Awake()
    {
        main = this;
    }

    public Relic RandomRelic()
    {
        return archive[UnityEngine.Random.Range(0, archive.Length)];
    }

    public Relic SpecificRelic(int index)
    {
        return archive[index];
    }
}

[System.Serializable]
public class Relic
{
    public string name;
    public int index;
    public Sprite sprite;
    public string description;
    public RelicEffect[] effects;
    public List<Player> currentlyOwnedByPLayerList = new List<Player>();

    public event Action<string> OnTrackerUpdate;

    private void Setup()
    {
        foreach (RelicEffect e in effects)
        {
            e.AttachEffectToRelic(this);
        }
    }

    public void ApplyRelic(Player toPlayer)
    {
        Setup();
        toPlayer.GainRelic(this);
        currentlyOwnedByPLayerList.Add(toPlayer);

        foreach(RelicEffect e in effects)
        {
            e.ApplyEffect(toPlayer);
        }

        if(toPlayer == Player.active)
        {
            UIController.main.UpdateUIWithNewRelic(this);
        }
    }

    public void UpdateRelicText(string text)
    {
        OnTrackerUpdate(text);
    }
}
