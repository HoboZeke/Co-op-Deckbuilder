using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugUI : MonoBehaviour
{
    public TextMeshProUGUI debugEventText;
    public TextMeshProUGUI debugText;
    public GameObject scrollView;

    public string[] playerColours;
    public string numberColour;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        debugText.text = "Recruitment Deck Ref List \n";
        CardObject[] array = Zones.main.recruit.CurrentRecruitmentDeckList();

        for(int i = 0; i < array.Length; i++)
        {
            debugText.text += i + ": " + array[i].nameText.text + " [" + array[i].referenceIndex + "]\n";
        }

        if (Input.GetKeyDown(KeyCode.F8))
        {
            debugText.gameObject.SetActive(!debugText.gameObject.activeInHierarchy);
        }
        if (Input.GetKeyDown(KeyCode.F9))
        {
            scrollView.SetActive(!scrollView.activeInHierarchy);
        }
    }

    public void DebugEvent(string s)
    {
        debugEventText.text += "\n";
        debugEventText.text += "[" + Mathf.Round(Time.time * 100f) / 100f + "] ";
        Debug.Log(s);
        debugEventText.text += FormattedString(s);
    }

    string FormattedString(string s)
    {
        string formattedString = "";
        string[] words = s.Split(' ');
        List<string> formattedWords = new List<string>();

        for(int i = 0; i < words.Length; i++)
        {
            if(words[i] == "Player")
            {
                formattedWords.Add(playerColours[int.Parse(words[i + 1])]);
                formattedWords.Add(words[i]);
                formattedWords.Add(words[i + 1]);
                formattedWords.Add("</color>");
                i++;
            }
            else if (words[i].StartsWith('('))
            {
                formattedWords.Add(numberColour);
                formattedWords.Add(words[i]);
                formattedWords.Add("</color>");
            }
            else
            {
                formattedWords.Add(words[i]);
            }
        }

        formattedString = string.Join(' ', formattedWords.ToArray());
        return formattedString;
    }
}
