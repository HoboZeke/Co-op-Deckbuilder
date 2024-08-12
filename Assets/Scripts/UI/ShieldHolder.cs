using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShieldHolder : MonoBehaviour
{
    public GameObject shieldPrefab;

    public int shieldResizeThreshold;
    public float shieldBaseGridSize;
    public GridLayoutGroup gridLayout;

    public float animationDuration;

    public void UpdateShieldHolder(int currentHealth)
    {
        if (currentHealth == transform.childCount) { return; }

        if (currentHealth > transform.childCount)
        {
            GainShields(currentHealth - transform.childCount);
        }
        else
        {
            LoseShields(transform.childCount - currentHealth);
        }
    }

    public void GainShields(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject obj = Instantiate(shieldPrefab);
            obj.transform.SetParent(transform);
            obj.transform.localEulerAngles = Vector3.zero;
        }

        CheckShieldGridSize();
    }

    void CheckShieldGridSize()
    {
        if (transform.childCount > shieldResizeThreshold)
        {
            int multiplier = Mathf.FloorToInt(transform.childCount / shieldResizeThreshold);
            gridLayout.cellSize = new Vector2(shieldBaseGridSize / (2 * multiplier), shieldBaseGridSize / (2 * multiplier));
        }
    }

    public void LoseShields(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Transform lostShield = transform.GetChild(transform.childCount - 1);
            lostShield.SetParent(transform.parent);
            StartCoroutine(AnimateShieldLoss(lostShield));
        }

        CheckShieldGridSize();
    }

    IEnumerator AnimateShieldLoss(Transform shield)
    {
        float timeElapsed = 0f;
        Image shieldImage = shield.GetComponent<Image>();
        Color startColour = shieldImage.color;
        Color endColour = Color.white;
        Vector3 startScale = shield.localScale;
        Vector3 endScale = shield.localScale * 2;

        while (timeElapsed < animationDuration)
        {
            shieldImage.color = Color.Lerp(startColour, endColour, timeElapsed / animationDuration);
            shield.localScale = Vector3.Lerp(startScale, endScale, timeElapsed / animationDuration);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(shield.gameObject);
    }
}
