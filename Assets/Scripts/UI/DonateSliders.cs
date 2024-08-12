using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DonateSliders : MonoBehaviour
{
    public int sliderType;
    public TextMeshProUGUI textMesh;
    public int value;
    public Image plusImage;
    public Image minusImage;

    public void PlusButtonPressed()
    {
        value++;
        if(value > MutliplayerController.active.LocalPlayer().StoredLoot(sliderType))
        {
            value = MutliplayerController.active.LocalPlayer().StoredLoot(sliderType);
        }

        textMesh.text = value.ToString();
        CheckDonateThresholds();
    }

    public void MinusButtonPressed()
    {
        value--;
        if (value < 0)
        {
            value = 0;
        }

        textMesh.text = value.ToString();
        CheckDonateThresholds();
    }

    void CheckDonateThresholds()
    {
        ToggleMinusImage(value - 1 >= 0);
        TogglePlusImage(value + 1 <= MutliplayerController.active.LocalPlayer().StoredLoot(sliderType));
    }

    void TogglePlusImage(bool on)
    {
        if (on)
        {
            plusImage.color = Color.white;
        }
        else
        {
            plusImage.color = new Color(0.4f, 0.4f, 0.4f, 0.4f);
        }
    }

    void ToggleMinusImage(bool on)
    {
        if (on)
        {
            minusImage.color = Color.red;
        }
        else
        {
            minusImage.color = new Color(0.4f, 0.4f, 0.4f, 0.4f);
        }
    }
}
