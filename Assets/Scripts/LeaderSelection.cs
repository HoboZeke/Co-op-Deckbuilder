using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LeaderSelection : MonoBehaviour
{
    public static LeaderSelection main;

    public Lobby lobby;

    public Transform leaderSelectionWheel;
    public Transform[] leaderProfiles;

    public float animationDuration;

    public List<int> lockedLeaders;

    public TextMeshProUGUI chooseLeaderButtoneText;
    public GameObject arrowButton1, arrowButton2;

    public int selectedLeader;
    bool selectionLockedIn;
    bool isAnimating;

    private void Awake()
    {
        main = this;
    }

    public void Setup()
    {
        for(int i = 0; i < leaderProfiles.Length; i++)
        {
            LeaderArchive.main.leaders[i].JoinToObject(leaderProfiles[i].GetChild(0).GetComponent<CardObject>());

            for(int c = 0; c < Player.active.startingDeck.Length; c++)
            {
                CardArchive.main.cards[Player.active.startingDeck[c]].JoinToObject(leaderProfiles[i].GetChild(c + 1).GetComponent<CardObject>());
            }

            if(i > 0) { leaderProfiles[i].localScale = new Vector3(1f, 0f, 1f); }
        }

        UpdateButtonText();
    }

    public void MainButtonPressed()
    {
        if (selectionLockedIn)
        {
            UnlockSelection();
        }
        else
        {
            LockInLeader();
        }
    }

    public void LockLeader(int player, int leader)
    {
        lockedLeaders.Add(leader);
        lobby.UpdatePlayerLeader(player, leader);
        UpdateButtonText();
    }

    public void UnlockLeader(int player, int leader)
    {
        lockedLeaders.Remove(leader);
        lobby.UpdatePlayerLeader(player, leader);
        UpdateButtonText();
    }

    public void UnlockSelection()
    {
        lockedLeaders.Remove(selectedLeader);
        Client.active.TellServerIHaveUnlockedALeader(selectedLeader);
        lobby.UpdatePlayerLeader(selectedLeader);

        UpdateButtonText();
        selectionLockedIn = false;
        
        Player.active.leader = -1;

        arrowButton1.SetActive(true);
        arrowButton2.SetActive(true);
    }

    public void LockInLeader()
    {
        if (lockedLeaders.Contains(selectedLeader)) { return; }

        lockedLeaders.Add(selectedLeader);
        Client.active.TellServerIHaveLockedInALeader(selectedLeader);
        chooseLeaderButtoneText.text = "Unlock Leader choice?";
        selectionLockedIn = true;
        lobby.UpdatePlayerLeader(selectedLeader);

        Player.active.leader = selectedLeader;

        arrowButton1.SetActive(false);
        arrowButton2.SetActive(false);
    }

    public int RandomUnchosenLeader()
    {
        List<int> availableLeaders = new List<int>();
        int leaderCount = leaderProfiles.Length;

        for(int i = 0; i < leaderCount; i++)
        {
            if (!lockedLeaders.Contains(i))
            {
                availableLeaders.Add(i);
            }
        }

        int chosenLeader = availableLeaders[Random.Range(0, availableLeaders.Count)];
        lockedLeaders.Add(chosenLeader);
        return chosenLeader;
    }

    public void MoveSelectionToTheRight()
    {
        if (isAnimating) { return; }

        selectedLeader--;
        if (selectedLeader < 0) { selectedLeader = leaderProfiles.Length - 1; }

        StartCoroutine(SpinWheel(false));
    }

    public void MoveSelectionToTheLeft()
    {
        if (isAnimating) { return; }

        selectedLeader++;
        if (selectedLeader >= leaderProfiles.Length) { selectedLeader = 0; }

        StartCoroutine(SpinWheel(true));
    }

    void UpdateButtonText()
    {
        if (lockedLeaders.Contains(selectedLeader))
        {
            chooseLeaderButtoneText.text = "Locked by another Player";
            return;
        }

        chooseLeaderButtoneText.text = "Choose leader " + LeaderArchive.main.leaders[selectedLeader].name + "?";
    }

    IEnumerator SpinWheel(bool clockwise)
    {
        isAnimating = true;
        float endRotationY = 90f;
        if (!clockwise) { endRotationY *= -1f; }

        Transform activeLeader = leaderProfiles[selectedLeader];
        Vector3[] startScales = new Vector3[leaderProfiles.Length];
        for(int i = 0; i < leaderProfiles.Length; i++)
        {
            startScales[i] = leaderProfiles[i].localScale;
        }

        endRotationY = leaderSelectionWheel.localEulerAngles.y + endRotationY;
        float timeElapsed = 0f;

        Vector3 start = leaderSelectionWheel.localEulerAngles;
        Vector3 end = new Vector3(0f, endRotationY, 0f);

        while (timeElapsed < animationDuration)
        {
            float t = timeElapsed / animationDuration;
            leaderSelectionWheel.localEulerAngles = Vector3.Lerp(start, end, t);

            activeLeader.localScale = Vector3.Lerp(startScales[selectedLeader], Vector3.one, t);
            for (int i = 0; i < leaderProfiles.Length; i++)
            {
                if(leaderProfiles[i] != activeLeader) { leaderProfiles[i].localScale = Vector3.Lerp(startScales[i], new Vector3(1f, 0f, 1f), t); }
            }
            
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        leaderSelectionWheel.localEulerAngles = end;
        foreach(Transform t in leaderProfiles) { t.localScale = new Vector3(1f, 0f, 1f); }
        activeLeader.localScale = Vector3.one;

        UpdateButtonText();
        isAnimating = false;
    }
}
