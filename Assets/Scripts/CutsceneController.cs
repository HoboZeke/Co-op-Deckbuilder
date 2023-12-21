using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CutsceneController : MonoBehaviour
{
    public UIController ui;

    public Camera cutsceneCamera;
    public Vector3[] introCutsceneCameraPos;
    public Quaternion[] introCutsceneCameraRot;
    public GameObject[] introCutsceneBoats;
    public float cameraMoveTime;

    [Header("GameOver")]
    public GameObject gameOverHolder;
    public Vector3 gameOverCameraPos, gameOverCameraRot;
    public GameObject gameOverText;
    public GameObject boatObj;
    public GameObject[] clanBoats;
    public Vector3[] boatPos;
    public Vector3[] boatRot;

    [Header("GameWin")]
    public GameObject gameWinHolder;
    public Vector3 gameWinCameraPos, gameWinCameraRot;
    public TextMeshPro gameWinText;
    public GameObject[] gameWinClanBoats;

    public void PlayIntroCutsceneIntoNewBattle()
    {
        gameObject.SetActive(true);

        foreach(GameObject boat in introCutsceneBoats)
        {
            boat.SetActive(false);
        }

        if (MutliplayerController.active.IsMultiplayerGame())
        {
            foreach(Player p in MutliplayerController.active.AllPlayers())
            {
                introCutsceneBoats[(int)p.clan].SetActive(true);
            }
        }
        else
        {
            introCutsceneBoats[(int)Player.active.clan].SetActive(true);
        }

        StartCoroutine(IntroCutscene());
    }

    IEnumerator IntroCutscene()
    {
        cutsceneCamera.gameObject.SetActive(true);
        cutsceneCamera.transform.localPosition = introCutsceneCameraPos[0];
        cutsceneCamera.transform.localRotation = introCutsceneCameraRot[0];
        float timeElapsed = 0f;

        for(int i = 1; i < introCutsceneCameraPos.Length; i++)
        {
            Debug.Log("Cutscene stage " + i);
            timeElapsed = 0f;

            while (timeElapsed < cameraMoveTime)
            {
                cutsceneCamera.transform.localPosition = Vector3.Lerp(introCutsceneCameraPos[i-1], introCutsceneCameraPos[i], timeElapsed/cameraMoveTime);
                cutsceneCamera.transform.localRotation = Quaternion.Lerp(introCutsceneCameraRot[i - 1], introCutsceneCameraRot[i], timeElapsed / cameraMoveTime);

                timeElapsed += Time.deltaTime;
                yield return null;
            }
        }

        cutsceneCamera.gameObject.SetActive(false);
        GameController.main.StartBattle();
        gameObject.SetActive(false);
    }

    public void PlayIntroCutsceneIntoNewRound()
    {
        gameObject.SetActive(true);

        foreach (GameObject boat in introCutsceneBoats)
        {
            boat.SetActive(false);
        }

        if (MutliplayerController.active.IsMultiplayerGame())
        {
            foreach (Player p in MutliplayerController.active.AllPlayers())
            {
                introCutsceneBoats[(int)p.clan].SetActive(true);
            }
        }
        else
        {
            introCutsceneBoats[(int)Player.active.clan].SetActive(true);
        }

        StartCoroutine(RoundIntroCutscene());
    }

    IEnumerator RoundIntroCutscene()
    {
        cutsceneCamera.gameObject.SetActive(true);
        cutsceneCamera.transform.localPosition = introCutsceneCameraPos[0];
        cutsceneCamera.transform.localRotation = introCutsceneCameraRot[0];
        float timeElapsed = 0f;

        for (int i = 1; i < introCutsceneCameraPos.Length; i++)
        {
            Debug.Log("Cutscene stage " + i);
            timeElapsed = 0f;

            while (timeElapsed < cameraMoveTime)
            {
                cutsceneCamera.transform.localPosition = Vector3.Lerp(introCutsceneCameraPos[i - 1], introCutsceneCameraPos[i], timeElapsed / cameraMoveTime);
                cutsceneCamera.transform.localRotation = Quaternion.Lerp(introCutsceneCameraRot[i - 1], introCutsceneCameraRot[i], timeElapsed / cameraMoveTime);

                timeElapsed += Time.deltaTime;
                yield return null;
            }
        }

        cutsceneCamera.gameObject.SetActive(false);
        GameController.main.StartNewRound();
        gameObject.SetActive(false);
    }

    public void PlayIntroCutsceneIntoBossRound()
    {
        gameObject.SetActive(true);

        foreach (GameObject boat in introCutsceneBoats)
        {
            boat.SetActive(false);
        }

        if (MutliplayerController.active.IsMultiplayerGame())
        {
            foreach (Player p in MutliplayerController.active.AllPlayers())
            {
                introCutsceneBoats[(int)p.clan].SetActive(true);
            }
        }
        else
        {
            introCutsceneBoats[(int)Player.active.clan].SetActive(true);
        }

        StartCoroutine(BossIntroCutscene());
    }

    IEnumerator BossIntroCutscene()
    {
        cutsceneCamera.gameObject.SetActive(true);
        cutsceneCamera.transform.localPosition = introCutsceneCameraPos[0];
        cutsceneCamera.transform.localRotation = introCutsceneCameraRot[0];
        float timeElapsed = 0f;

        for (int i = 1; i < introCutsceneCameraPos.Length; i++)
        {
            Debug.Log("Cutscene stage " + i);
            timeElapsed = 0f;

            while (timeElapsed < cameraMoveTime)
            {
                cutsceneCamera.transform.localPosition = Vector3.Lerp(introCutsceneCameraPos[i - 1], introCutsceneCameraPos[i], timeElapsed / cameraMoveTime);
                cutsceneCamera.transform.localRotation = Quaternion.Lerp(introCutsceneCameraRot[i - 1], introCutsceneCameraRot[i], timeElapsed / cameraMoveTime);

                timeElapsed += Time.deltaTime;
                yield return null;
            }
        }

        cutsceneCamera.gameObject.SetActive(false);
        GameController.main.StartBossRound();
        gameObject.SetActive(false);
    }

    public void GameOver(Player deadPlayer)
    {
        ui.gameObject.SetActive(false);
        gameObject.SetActive(true);
        cutsceneCamera.gameObject.SetActive(true);
        gameOverHolder.SetActive(true);
        cutsceneCamera.transform.localPosition = gameOverCameraPos;
        cutsceneCamera.transform.localEulerAngles = gameOverCameraRot;

        foreach(GameObject obj in clanBoats)
        {
            obj.SetActive(false);
        }
        clanBoats[(int)deadPlayer.clan].SetActive(true);
        boatObj = clanBoats[(int)deadPlayer.clan];

        boatObj.transform.localPosition = boatPos[0];
        boatObj.transform.localEulerAngles = boatRot[0];
        StartCoroutine(SinkBoat());
    }

    IEnumerator SinkBoat()
    {
        float timeElapsed = 0f;

        for (int i = 1; i < boatPos.Length; i++)
        {
            timeElapsed = 0f;
            while (timeElapsed < cameraMoveTime)
            {
                boatObj.transform.localPosition = Vector3.Lerp(boatPos[i - 1], boatPos[i], timeElapsed / cameraMoveTime);
                boatObj.transform.localEulerAngles = Vector3.Lerp(boatRot[i - 1], boatRot[i], timeElapsed / cameraMoveTime);

                timeElapsed += Time.deltaTime;
                yield return null;
            }
        }

        gameOverText.SetActive(true);
    }

    public void GameWin(int[] lootCount)
    {
        ui.gameObject.SetActive(false);
        gameObject.SetActive(true);
        cutsceneCamera.gameObject.SetActive(true);
        gameWinHolder.SetActive(true);
        cutsceneCamera.transform.localPosition = gameWinCameraPos;
        cutsceneCamera.transform.localEulerAngles = gameWinCameraRot;

        foreach (GameObject obj in clanBoats)
        {
            obj.SetActive(false);
        }

        foreach (Player p in MutliplayerController.active.AllPlayers())
        {
            gameWinClanBoats[(int)p.clan].SetActive(true);
        }

        gameWinText.text = "Game Won!\n";
        gameWinText.text += "You looted" + lootCount[0] + " Treasures\n";
        if(HomeBase.main.peopleStorageUnlocked) gameWinText.text += "You rescued" + lootCount[1] + " People\n";
        if(HomeBase.main.weaponStorageUnlocked) gameWinText.text += "You reclaimed" + lootCount[2] + " Weapons\n";
        gameWinText.text += "On your Voyage";
    }
}
