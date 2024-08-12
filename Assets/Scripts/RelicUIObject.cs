using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RelicUIObject : MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI textMesh;
    public TextMeshProUGUI effectTrackerText;
    public Relic relic;
    public int count;

    public void Setup(Relic r)
    {
        relic = r;
        image.sprite = relic.sprite;
        textMesh.text = "";
        count = 1;
        r.OnTrackerUpdate += UpdateTrackerUI;
    }

    public void IncreaseCount()
    {
        count++;
        textMesh.text = count.ToString();
    }

    public void UpdateTrackerUI(string text)
    {
        effectTrackerText.text = text;
    }
}
