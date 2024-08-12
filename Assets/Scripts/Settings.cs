using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour
{
    public static Settings main;

    public enum ResourceDisplay { Icons, Numbers }
    public ResourceDisplay resourceDisplay;
    Resolution windowedResolution;

    private void Awake()
    {
        main = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        windowedResolution = Screen.currentResolution;
        if(PlayerPrefs.HasKey("ResourceDisplay"))
        {
            resourceDisplay = (ResourceDisplay)PlayerPrefs.GetInt("RescourceDisplay");
        }
        else
        {
            PlayerPrefs.SetInt("ResourceDisplay", (int)resourceDisplay);
        }
    }

    public void ChangeResourceDisplay(ResourceDisplay to)
    {
        resourceDisplay = to;
        PlayerPrefs.SetInt("ResourceDisplay", (int)resourceDisplay);
        Player.active.UpdateResourceSettings(resourceDisplay);
        if (MutliplayerController.active.IsMultiplayerGame())
        {
            foreach(Player p in MutliplayerController.active.AllPlayers())
            {
                p.UpdateResourceSettings(resourceDisplay);
            }
        }
    }

    public void ToggleFullScreen(bool toggle)
    {
        Screen.fullScreen = toggle;
        if (toggle)
        {
            windowedResolution = Screen.currentResolution;
            Screen.SetResolution(Screen.resolutions[Screen.resolutions.Length-1].width, Screen.resolutions[Screen.resolutions.Length - 1].height, true);
        }
        else
        {
            Screen.SetResolution(windowedResolution.width, windowedResolution.height, false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
