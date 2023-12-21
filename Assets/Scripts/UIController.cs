using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public static UIController main;

    public GameObject relicUIObj;
    public Transform relicHolder;
    List<RelicUIObject> relicUIObjects = new List<RelicUIObject>();

    public GameObject endTurnButton;

    public GameObject[] clanDoneIndicators;

    private void Awake()
    {
        main = this;
    }

    public void UpdateUIWithNewRelic(Relic r)
    {
        for(int i = 0; i < relicUIObjects.Count; i++)
        {
            if(relicUIObjects[i].relic == r)
            {
                relicUIObjects[i].IncreaseCount();
                return;
            }
        }

        GameObject obj = Instantiate(relicUIObj);
        obj.transform.SetParent(relicHolder);
        obj.GetComponent<RelicUIObject>().Setup(r);
        relicUIObjects.Add(obj.GetComponent<RelicUIObject>());
    }

    public void ToggleEndTurnButton(bool toggle)
    {
        endTurnButton.SetActive(toggle);
    }

    public void SetupClanDoneUI()
    {
        foreach(GameObject obj in clanDoneIndicators)
        {
            obj.SetActive(false);
        }

        if(MutliplayerController.active.playerCount <= 1) { return; }

        foreach(Player p in MutliplayerController.active.AllPlayers())
        {
            int c = (int)p.clan;
            clanDoneIndicators[c].SetActive(true);
            clanDoneIndicators[c].transform.GetChild(1).gameObject.SetActive(false);
            clanDoneIndicators[c].transform.GetChild(2).gameObject.SetActive(true);
        }
    }

    public void ToggleClanTurnDoneUI(int clan, bool turnDone)
    {
        clanDoneIndicators[clan].transform.GetChild(1).gameObject.SetActive(turnDone);
        clanDoneIndicators[clan].transform.GetChild(2).gameObject.SetActive(!turnDone);
    }

    public void ToggleAllClanTurnDoneUI(bool turnDone)
    {
        foreach (Player p in MutliplayerController.active.AllPlayers())
        {
            int c = (int)p.clan;
            clanDoneIndicators[c].transform.GetChild(1).gameObject.SetActive(turnDone);
            clanDoneIndicators[c].transform.GetChild(2).gameObject.SetActive(!turnDone);
        }
    }
}
