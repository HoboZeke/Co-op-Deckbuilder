using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ClanSelection : MonoBehaviour
{
    public static ClanSelection main;

    public Lobby lobby;

    public Transform clanOptionsWheel;
    public Transform[] clanFigurines;
    public float figurineSpinSpeed;
    public float animationDuration;

    public enum Clan { bear, cat, whale, bird};
    public Clan selectedClan;

    public List<Clan> lockedClans;

    public TextMeshProUGUI chooseClanButtonText;
    public GameObject arrowButton1, arrowButton2;
    bool selectionLockedIn;
    bool isAnimating;

    private void Awake()
    {
        main = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateButtonText();
    }

    // Update is called once per frame
    void Update()
    {
        if (isActiveAndEnabled)
        {
            foreach(Transform t in clanFigurines)
            {
                t.localEulerAngles = new Vector3(t.localEulerAngles.x, t.localEulerAngles.y - (figurineSpinSpeed * Time.deltaTime), t.localEulerAngles.z);
            }
        }
    }

    public void MainButtonPressed()
    {
        if (selectionLockedIn)
        {
            UnlockSelection();
        }
        else
        {
            LockInClan();
        }
    }

    public void UnlockSelection()
    {
        lockedClans.Remove(selectedClan);
        Client.active.TellServerIHaveUnlockedAClan(selectedClan);
        lobby.UpdatePlayerClan(-1);

        UpdateButtonText();
        selectionLockedIn = false;

        arrowButton1.SetActive(true);
        arrowButton2.SetActive(true);
    }

    public void LockClan(int player, Clan clan)
    {
        lockedClans.Add(clan);
        lobby.UpdatePlayerClan(player, (int)clan);
        UpdateButtonText();
    }

    public void UnlockClan(int player, Clan clan)
    {
        lockedClans.Remove(clan);
        lobby.UpdatePlayerClan(player, -1);
        UpdateButtonText();
    }

    public void LockInClan()
    {
        if (lockedClans.Contains(selectedClan)) { return; }

        lockedClans.Add(selectedClan);
        Client.active.TellServerIHaveLockedInAClan(selectedClan);
        chooseClanButtonText.text = "Unlock Clan choice?";
        selectionLockedIn = true;
        lobby.UpdatePlayerClan((int)selectedClan);

        Player.active.clan = selectedClan;

        arrowButton1.SetActive(false);
        arrowButton2.SetActive(false);
    }

    public int RandomUnchosenClan()
    {
        List<int> availableLeaders = new List<int>();
        int leaderCount = clanFigurines.Length;

        for (int i = 0; i < leaderCount; i++)
        {
            if (!lockedClans.Contains((Clan)i))
            {
                availableLeaders.Add(i);
            }
        }

        int chosenClan = availableLeaders[Random.Range(0, availableLeaders.Count)];
        lockedClans.Add((Clan)chosenClan);
        return chosenClan;
    }

    public void MoveSelectionToTheRight()
    {
        if (isAnimating) { return; }

        switch (selectedClan)
        {
            case Clan.bear:
                selectedClan = Clan.bird;
                break;
            case Clan.bird:
                selectedClan = Clan.whale;
                break;
            case Clan.whale:
                selectedClan = Clan.cat;
                break;
            case Clan.cat:
                selectedClan = Clan.bear;
                break;
        }

        StartCoroutine(SpinWheel(false));
    }

    public void MoveSelectionToTheLeft()
    {
        if (isAnimating) { return; }

        switch (selectedClan)
        {
            case Clan.bear:
                selectedClan = Clan.cat;
                break;
            case Clan.cat:
                selectedClan = Clan.whale;
                break;
            case Clan.whale:
                selectedClan = Clan.bird;
                break;
            case Clan.bird:
                selectedClan = Clan.bear;
                break;
        }

        StartCoroutine(SpinWheel(true));
    }

    void UpdateButtonText()
    {
        if (lockedClans.Contains(selectedClan))
        {
            chooseClanButtonText.text = "Locked by another Player";
            return;
        }

        switch (selectedClan)
        {
            case Clan.bear:
                chooseClanButtonText.text = "Choose Bear clan?";
                break;
            case Clan.bird:
                chooseClanButtonText.text = "Choose Bird clan?";
                break;
            case Clan.whale:
                chooseClanButtonText.text = "Choose Whale clan?";
                break;
            case Clan.cat:
                chooseClanButtonText.text = "Choose Cat clan?";
                break;
        }
    }

    IEnumerator SpinWheel(bool clockwise)
    {
        isAnimating = true;
        float endRotationY = 90f;
        if (!clockwise) { endRotationY *= -1f; }

        endRotationY = clanOptionsWheel.localEulerAngles.y + endRotationY;
        float timeElapsed = 0f;

        Vector3 start = clanOptionsWheel.localEulerAngles;
        Vector3 end = new Vector3(0f, endRotationY, 0f);

        while(timeElapsed < animationDuration)
        {
            clanOptionsWheel.localEulerAngles = Vector3.Lerp(start, end, timeElapsed / animationDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        clanOptionsWheel.localEulerAngles = end;
        isAnimating = false;
        UpdateButtonText();
    }
}
