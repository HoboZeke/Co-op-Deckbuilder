using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LocationNodeObject : MonoBehaviour
{
    public TextMeshPro nameText, healthText;
    public int currentHealth, maxHealth;
    public Transform figurineHolder;
    public GameObject[] figurinePrefabs;
    int index;
    Location.NodeType type;

    public GameObject eventNodePrefab;
    public Vector3[] eventLocations;
    List<GameObject> nodeEventObjs = new List<GameObject>();


    public void Setup(string name, int health, Location.NodeType nodeType, int ind)
    {
        index = ind;
        nameText.text = name;
        currentHealth = health;
        maxHealth = health;
        type = nodeType;
        UpdateHealthText();
        SpawnFigurine();
    }

    public void UpdateHealthText()
    {
        if(type == Location.NodeType.Destroyed) { healthText.text = "Destroyed!"; return; }
        healthText.text = currentHealth + "hp/" + maxHealth + "hp";
    }

    public bool TakeDamage(int amount)
    {
        currentHealth -= amount;

        if(currentHealth <= 0)
        {
            DestroyNode();
            return true;
        }
        else
        {
            UpdateHealthText();
            return false;
        }
    }

    void DestroyNode()
    {
        Location.active.ConvertNodeToDestroyedNode(this);
    }

    void SpawnFigurine()
    {
        if(figurineHolder.childCount > 0)
        {
            foreach(Transform child in figurineHolder)
            {
                Destroy(child.gameObject);
            }
        }

        GameObject fig = Instantiate(figurinePrefabs[(int)type]);
        fig.transform.SetParent(figurineHolder);
        fig.transform.localPosition = Vector3.zero;
        fig.transform.localEulerAngles = Vector3.zero;
        fig.transform.localScale = Vector3.one;
    }

    public void PlayerChoseEventNode0()
    {
        if (Location.active.CanReachNode(index))
        {
            PlayerChoseEventNode(0);
        }
    }

    public void PlayerChoseEventNode1()
    {
        if (Location.active.CanReachNode(index))
        {
            PlayerChoseEventNode(1);
        }
    }

    public void PlayerChoseEventNode2()
    {
        if (Location.active.CanReachNode(index))
        {
            PlayerChoseEventNode(2);
        }
    }

    public void PlayerChoseEventNode(int nodeIndex)
    {
        Vector3 loc = Vector3.zero;
        nodeEventObjs[nodeIndex].transform.SetParent(transform.parent);
        loc = nodeEventObjs[nodeIndex].transform.localPosition + Vector3.down;
        nodeEventObjs[nodeIndex].transform.SetParent(transform);

        Location.NodeEvent nodeEvent = Location.active.EventsForNode(type)[nodeIndex];

        Location.active.MovePlayerFigureToLocation(MutliplayerController.active.myPlayerNumber, loc, nodeEvent, true);
    }

    public void ExpandNode()
    {
        nodeEventObjs.Clear();
        Location.NodeEvent[] nodeEvents = Location.active.EventsForNode(type);

        for(int i = 0; i < nodeEvents.Length; i++)
        {
            GameObject obj = Instantiate(eventNodePrefab);
            obj.transform.SetParent(transform);
            obj.transform.localPosition = eventLocations[i];
            nodeEventObjs.Add(obj);

            SimpleButton button = obj.GetComponent<SimpleButton>();
            if (i == 0)
            {
                if (button != null) button.OnButtonPressed += PlayerChoseEventNode0;
            }
            else if (i == 1)
            {
                if (button != null) button.OnButtonPressed += PlayerChoseEventNode1;
            }
            else if (i == 2)
            {
                if (button != null) button.OnButtonPressed += PlayerChoseEventNode2;
            }

            obj.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = Location.active.EventSprite(nodeEvents[i]);
        }
    }
}
