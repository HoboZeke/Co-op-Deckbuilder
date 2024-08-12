using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public static UIController main;

    public GameObject relicUIObj;
    public Transform relicHolder;
    List<RelicUIObject> relicUIObjects = new List<RelicUIObject>();

    [SerializeField] GameObject cameraControlButtons;
    public GameObject endTurnButton;

    public GameObject[] clanDoneIndicators;

    [SerializeField] private CardFocusUiObject cardFocusPopup;
    [SerializeField] private GameObject desyncOverlay;
    [SerializeField] private TextMeshProUGUI desyncTextOverlay;
    [SerializeField] GameObject[] hideInDeckViewer;
    [SerializeField] GameObject[] showInDeckViewer;
    bool cardFocussedOnInThisFrame;
    bool stopGameControl;

    private void Awake()
    {
        main = this;
    }

    private void LateUpdate()
    {
        if (cardFocussedOnInThisFrame) { cardFocussedOnInThisFrame = false; }
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

    public void ToggleCameraUIControls(bool toggle)
    {
        cameraControlButtons.SetActive(toggle);
        if(toggle) { CameraController.main.CheckForValidCameraMoves(); }
    }

    public void ShowOnlyValidCameraControls(List<int> validControls)
    {
        for (int i = 0; i < cameraControlButtons.transform.childCount; i++)
        {
            cameraControlButtons.transform.GetChild(i).gameObject.SetActive(validControls.Contains(i));
        }
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

    public void HideZoomedCard()
    {
        cardFocusPopup.gameObject.SetActive(false);
    }

    public void ShowZoomedCard(CardObject card)
    {
        cardFocusPopup.gameObject.SetActive(true);
        cardFocusPopup.SetupAsCard(card);
        cardFocussedOnInThisFrame = true;
    }

    public bool IsZoomedCardShown()
    {
        if (cardFocussedOnInThisFrame) { return false; }
        return cardFocusPopup.gameObject.activeInHierarchy;
    }

    public void ToggleDesyncOverlay(bool show)
    {
        if(show && desyncOverlay.activeInHierarchy) { SetDesyncOverlayText(""); }
        desyncOverlay.gameObject.SetActive(show);
    }

    public void SetDesyncOverlayText(string text)
    {
        desyncTextOverlay.text = "De-Synced " + text;
    }

    public bool IsDesyncOverlayActive()
    {
        return desyncOverlay.gameObject.activeInHierarchy;
    }

    public void ToggleDeckViewerUI(bool on)
    {
        foreach(GameObject go in hideInDeckViewer)
        {
            go.SetActive(!on);
        }
        foreach (GameObject go in showInDeckViewer)
        {
            go.SetActive(on);
        }
        stopGameControl = on;
    }

    public bool UIHasFocus()
    {
        return stopGameControl;
    }
}
