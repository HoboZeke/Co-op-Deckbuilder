using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KeywordTooltip : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI headerText;
    [SerializeField] TextMeshProUGUI descriptionText;

    public void Setup(string keyword, string description)
    {
        headerText.text = keyword;
        descriptionText.text = description;
    }
}

