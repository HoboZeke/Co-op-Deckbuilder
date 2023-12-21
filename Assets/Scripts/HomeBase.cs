using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HomeBase : MonoBehaviour
{
    public static HomeBase main;

    public int availableTreasure, availablePop, availableWeapons;
    public SimpleButton longHallButton, trasureRoomButton, player0DockButton, player1DockButton, player2DockButton, player3DockButton, player4DockButton, compassButton;
    public GameObject longHall, treasureRoom, p0Dock, p1Dock, p2Dock, p3Dock, p4Dock, compass;
    bool treasureRoomUnlocked;

    public Material[] clanMaterialColours;
    public Sprite[] clanSprites;

    public Image dock0FlagImage, dock0BoatImage, dock1FlagImage, dock1BoatImage, dock2FlagImage, dock2BoatImage, dock3FlagImage, dock3BoatImage, dock4FlagImage, dock4BoatImage;
    public GameObject dock0FlagBG, dock0BoatSailBG, dock1FlagBG, dock1BoatSailBG, dock2FlagBG, dock2BoatSailBG, dock3FlagBG, dock3BoatSailBG, dock4FlagBG, dock4BoatSailBG;

    [Space(10)]
    [Header("Unlocks")]
    public bool peopleStorageUnlocked;
    public bool weaponStorageUnlocked;

    [Space(10)]
    [Header("UI")]
    public GameObject optionsCanavs;
    public RectTransform optionsBackground;
    private Vector2 backingImageMaxSize;
    public TextMeshProUGUI treasuresText;
    public float animationDuration;

    [Space(10)]
    [Header("DockMenu")]
    public GameObject dockMenu;
    public CardObject leaderCard;
    public CardObject[] deckCards;

    [Space(10)]
    [Header("ExpeditionMap")]
    public GameObject expeditionMap;

    [Space(10)]
    [Header("LongHallMenu")]
    public GameObject longHallMenu;
    public DonateSliders treasureDonateSlider, popDonateSlider, weaponDonateSlider;
    public TextMeshProUGUI collectiveResourceText;

    private void Awake()
    {
        main = this;
        backingImageMaxSize = optionsBackground.sizeDelta;
    }

    private void Start()
    {
        longHallButton.OnButtonPressed += OnPressedLongHall;
        trasureRoomButton.OnButtonPressed += OnPressedTreasureRoom;
        compassButton.OnButtonPressed += OnPressedCompass;
        player0DockButton.OnButtonPressed += OnPressedP0Dock;
        player1DockButton.OnButtonPressed += OnPressedP1Dock;
        player2DockButton.OnButtonPressed += OnPressedP2Dock;
        player3DockButton.OnButtonPressed += OnPressedP3Dock;
        player4DockButton.OnButtonPressed += OnPressedP4Dock;

        UpdateShownBuildings();
    }

    public void MoveToHomeBase()
    {
        CameraController.main.MoveCameraToHomeBase();
        UpdateShownBuildings();
    }

    void UpdateBaseUI()
    {
        treasuresText.text = "Treasures: " + MutliplayerController.active.LocalPlayer().storedTreasure;
    }

    void UpdateShownBuildings()
    {
        int playerCount = MutliplayerController.active.playerCount;

        switch (playerCount)
        {
            case 1:
                p0Dock.SetActive(true);
                p1Dock.SetActive(false);
                p2Dock.SetActive(false);
                p3Dock.SetActive(false);
                p4Dock.SetActive(false);
                break;
            case 2:
                p0Dock.SetActive(true);
                p1Dock.SetActive(true);
                p2Dock.SetActive(false);
                p3Dock.SetActive(false);
                p4Dock.SetActive(false);
                break;
            case 3:
                p0Dock.SetActive(true);
                p1Dock.SetActive(true);
                p2Dock.SetActive(true);
                p3Dock.SetActive(false);
                p4Dock.SetActive(false);
                break;
            case 4:
                p0Dock.SetActive(true);
                p1Dock.SetActive(true);
                p2Dock.SetActive(true);
                p3Dock.SetActive(true);
                p4Dock.SetActive(false);
                break;
            case 5:
                p0Dock.SetActive(true);
                p1Dock.SetActive(true);
                p2Dock.SetActive(true);
                p3Dock.SetActive(true);
                p4Dock.SetActive(true);
                break;
        }

        if(treasureRoom != null)
        {
            treasureRoom.SetActive(treasureRoomUnlocked);
        }

        UpdateClanMarkings(playerCount);
    }

    void UpdateClanMarkings(int playerCount)
    {
        for(int i = 0; i < playerCount; i++)
        {
            int clan = (int)MutliplayerController.active.GetPlayerScript(i).clan;

            switch (i)
            {
                case 0:
                    dock0BoatImage.sprite = clanSprites[clan];
                    dock0FlagImage.sprite = clanSprites[clan];
                    dock0BoatSailBG.GetComponent<Renderer>().material = clanMaterialColours[clan];
                    dock0FlagBG.GetComponent<Renderer>().material = clanMaterialColours[clan];
                    break;
                case 1:
                    dock1BoatImage.sprite = clanSprites[clan];
                    dock1FlagImage.sprite = clanSprites[clan];
                    dock1BoatSailBG.GetComponent<Renderer>().material = clanMaterialColours[clan];
                    dock1FlagBG.GetComponent<Renderer>().material = clanMaterialColours[clan];
                    break;
                case 2:
                    dock2BoatImage.sprite = clanSprites[clan];
                    dock2FlagImage.sprite = clanSprites[clan];
                    dock2BoatSailBG.GetComponent<Renderer>().material = clanMaterialColours[clan];
                    dock2FlagBG.GetComponent<Renderer>().material = clanMaterialColours[clan];
                    break;
                case 3:
                    dock3BoatImage.sprite = clanSprites[clan];
                    dock3FlagImage.sprite = clanSprites[clan];
                    dock3BoatSailBG.GetComponent<Renderer>().material = clanMaterialColours[clan];
                    dock3FlagBG.GetComponent<Renderer>().material = clanMaterialColours[clan];
                    break;
                case 4:
                    dock4BoatImage.sprite = clanSprites[clan];
                    dock4FlagImage.sprite = clanSprites[clan];
                    dock4BoatSailBG.GetComponent<Renderer>().material = clanMaterialColours[clan];
                    dock4FlagBG.GetComponent<Renderer>().material = clanMaterialColours[clan];
                    break;
            }
        }
    }

    public Sprite ClanSprite(int clan)
    {
        return clanSprites[clan];
    }

    public Material ClanMat(int clan)
    {
        return clanMaterialColours[clan];
    }

    #region Events

    void OnPressedCompass()
    {
        optionsBackground.anchoredPosition = Camera.main.WorldToScreenPoint(compass.transform.position);
        OpenExploreMenu();
    }

    void OnPressedLongHall()
    {
        optionsBackground.anchoredPosition = Camera.main.WorldToScreenPoint(longHall.transform.position);
        OpenLongHallMenu();
    }

    void OnPressedTreasureRoom()
    {

    }

    void OnPressedP0Dock()
    {
        if(MutliplayerController.active.LocalPlayerNumber() == 0) {
            optionsBackground.anchoredPosition = Camera.main.WorldToScreenPoint(p0Dock.transform.position);
            OpenDockMenu();
        }
    }

    void OnPressedP1Dock()
    {
        if (MutliplayerController.active.LocalPlayerNumber() == 1)
        {
            optionsBackground.anchoredPosition = Camera.main.WorldToScreenPoint(p1Dock.transform.position);
            OpenDockMenu();
        }
    }

    void OnPressedP2Dock()
    {
        if (MutliplayerController.active.LocalPlayerNumber() == 2)
        {
            optionsBackground.anchoredPosition = Camera.main.WorldToScreenPoint(p2Dock.transform.position);
            OpenDockMenu();
        }
    }

    void OnPressedP3Dock()
    {
        if (MutliplayerController.active.LocalPlayerNumber() == 3)
        {
            optionsBackground.anchoredPosition = Camera.main.WorldToScreenPoint(p3Dock.transform.position);
            OpenDockMenu();
        }
    }

    void OnPressedP4Dock()
    {
        if (MutliplayerController.active.LocalPlayerNumber() == 4)
        {
            optionsBackground.anchoredPosition = Camera.main.WorldToScreenPoint(p4Dock.transform.position);
            OpenDockMenu();
        }
    }
    #endregion

    public void CloseAllHomeBaseMenus()
    {
        CloseDockMenu();
        CloseExploreMenu();
        CloseLongHallMenu();
    }

    void OpenDockMenu()
    {
        Debug.Log("Opening dock menu");
        SetupDeckList();
        dockMenu.SetActive(true);
        StartCoroutine(MoveOptionsBackgroundIntoFocus());
    }

    public void CloseDockMenu()
    {
        dockMenu.SetActive(false);
        optionsBackground.gameObject.SetActive(false);
    }

    void SetupDeckList()
    {
        LeaderArchive.main.leaders[Player.active.leader].JoinToObject(leaderCard);
        for(int i = 0; i < deckCards.Length; i++)
        {
            CardArchive.main.cards[Player.active.startingDeck[i]].JoinToObject(deckCards[i]);
        }
    }

    void OpenExploreMenu()
    {
        Debug.Log("Opening explore menu");
        expeditionMap.SetActive(true);
        StartCoroutine(MoveOptionsBackgroundIntoFocus());
        AudioManager.main.OpenAndCloseMapAudioEvent();
    }

    public void BattleLocationChosen()
    {
        if (MutliplayerController.active.IAmHost())
        {
            Client.active.TellServerToStartTheBattle();
        }
    }

    public void CloseExploreMenu()
    {
        expeditionMap.SetActive(false);
        optionsBackground.gameObject.SetActive(false);
        AudioManager.main.OpenAndCloseMapAudioEvent();
    }

    void OpenLongHallMenu()
    {
        Debug.Log("Opening long hall menu");
        longHallMenu.SetActive(true);
        UpdateCollectiveLootText();
        StartCoroutine(MoveOptionsBackgroundIntoFocus());
    }

    public void Donate()
    {
        int treasureDonated = 0, popDonated = 0, weaponsDonated = 0;

        if(treasureDonateSlider.value > 0)
        {
            MutliplayerController.active.LocalPlayer().storedTreasure -= treasureDonateSlider.value;
            availableTreasure += treasureDonateSlider.value;
            treasureDonated = treasureDonateSlider.value;
            treasureDonateSlider.value = 0;
            treasureDonateSlider.textMesh.text = treasureDonateSlider.value.ToString();
        }
        if (popDonateSlider.value > 0)
        {
            MutliplayerController.active.LocalPlayer().storedPopulation -= popDonateSlider.value;
            availablePop += popDonateSlider.value;
            popDonated = popDonateSlider.value;
            popDonateSlider.value = 0;
            popDonateSlider.textMesh.text = popDonateSlider.value.ToString();
        }
        if (weaponDonateSlider.value > 0)
        {
            MutliplayerController.active.LocalPlayer().storedWeapons -= weaponDonateSlider.value;
            availableWeapons += weaponDonateSlider.value;
            weaponsDonated = weaponDonateSlider.value;
            weaponDonateSlider.value = 0;
            weaponDonateSlider.textMesh.text = treasureDonateSlider.value.ToString();
        }

        if (MutliplayerController.active.IsMultiplayerGame()) { Client.active.TellServerIHaveDonatedFundsToLongHall(treasureDonated, popDonated, weaponsDonated); }
        UpdateCollectiveLootText();
    }

    public void OtherPlayerDonated(int treasure, int pop, int weapons)
    {
        availableTreasure += treasure;
        availablePop += pop;
        availableWeapons += weapons;
        UpdateCollectiveLootText();
    }

    void UpdateCollectiveLootText()
    {
        collectiveResourceText.text = "The Long Hall currently holds ";
        collectiveResourceText.text += availableTreasure + " Treasure ";
        if (peopleStorageUnlocked) { collectiveResourceText.text += availablePop + " People "; }
        if (weaponStorageUnlocked) { collectiveResourceText.text += "and " + availableWeapons + " Weapons"; }
    }

    public void CloseLongHallMenu()
    {
        longHallMenu.SetActive(false);
        optionsBackground.gameObject.SetActive(false);
    }

    IEnumerator MoveOptionsBackgroundIntoFocus()
    {
        float timeElapsed = 0f;
        optionsBackground.gameObject.SetActive(true);
        Vector2 startPos = optionsBackground.anchoredPosition;
        Vector2 startScale = Vector2.zero;
        Vector2 endPos = new Vector2(Screen.width / 2, Screen.height / 2);
        Vector2 endScale = backingImageMaxSize;

        while (timeElapsed < animationDuration)
        {
            optionsBackground.anchoredPosition = Vector2.Lerp(startPos, endPos, timeElapsed / animationDuration);
            optionsBackground.sizeDelta = Vector2.Lerp(startScale, endScale, timeElapsed / animationDuration);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        optionsBackground.anchoredPosition = endPos;
        optionsBackground.sizeDelta = endScale;
    }
}
