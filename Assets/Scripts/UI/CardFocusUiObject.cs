using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardFocusUiObject : MonoBehaviour
{
    [SerializeField] Image background, banner, cardArt, frame;
    [SerializeField] TextMeshProUGUI nameText, cardText, healthText, attackText, armourText;
    [SerializeField] GameObject healthOverlay, attackOverlay, armourOverlay;
    [SerializeField] GameObject topTab, topTabEmpty, bottomTab, bottomTabEmpty;
    [SerializeField] Transform keywordTooltipHolder;
    [SerializeField] GameObject keywordTooltip;
    CardObject currentVersion;

    public void SetupAsCard(CardObject version)
    {
        currentVersion = version;
        nameText.text = version.CardName();
        cardText.text = version.CardDescription();
        healthText.text = version.HealthValue().ToString();
        attackText.text = version.AttackValue().ToString();
        armourText.text = version.CardShield();

        foreach(Transform child in keywordTooltipHolder)
        {
            Destroy(child.gameObject);
        }
        keywordTooltipHolder.DetachChildren();

        Color[] ColourProfile = version.ColourProfile();
        banner.color = ColourProfile[0];
        frame.color = ColourProfile[1];
        cardArt.color = ColourProfile[2];
        background.color = ColourProfile[3];

        Material mat = version.TabMaterial();
        topTab.GetComponent<MeshRenderer>().material = mat;
        topTabEmpty.GetComponent<MeshRenderer>().material = mat;
        bottomTab.GetComponent<MeshRenderer>().material = mat;
        bottomTabEmpty.GetComponent<MeshRenderer>().material = mat;

        cardArt.sprite = version.cardArt.sprite;


        if (int.Parse(attackText.text) <= 0) 
        { 
            attackOverlay.SetActive(false); 
            topTab.SetActive(false);
            topTabEmpty.SetActive(true);
        }
        else 
        { 
            attackOverlay.SetActive(true);
            topTab.SetActive(true);
            topTabEmpty.SetActive(false);
        }

        if (int.Parse(healthText.text) <= 0) { healthOverlay.SetActive(false); }
        else { healthOverlay.SetActive(true); }
        if (int.Parse(armourText.text) <= 0) { armourOverlay.SetActive(false); }
        else { armourOverlay.SetActive(true); }

        if(int.Parse(healthText.text) > 0 || int.Parse(armourText.text) > 0)
        {
            bottomTab.SetActive(true);
            bottomTabEmpty.SetActive(false);
        }
        else
        {
            bottomTab.SetActive(false);
            bottomTabEmpty.SetActive(true);
        }

        StartCoroutine(ShowKeywordsAfterDelay(0.5f));
    }

    void PopulatedKeywords()
    {
        AbilityKeywords.Keyword[] keywords = currentVersion.GetKeywords();

        for(int i = 0; i < keywords.Length; i++)
        {
            GameObject tooltip = Instantiate(keywordTooltip);
            tooltip.transform.SetParent(keywordTooltipHolder);

            tooltip.GetComponent<KeywordTooltip>().Setup(keywords[i].ToString(), AbilityKeywords.KeywordDescription(keywords[i]));
        }
    }

    IEnumerator ShowKeywordsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        PopulatedKeywords();
    }
}
