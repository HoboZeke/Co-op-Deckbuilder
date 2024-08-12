using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthHolder : MonoBehaviour
{
    public GameObject heartPrefab;

    public int heartResizeThreshold;
    public float heartBaseGridSize;
    public GridLayoutGroup gridLayout;

    public float animationDuration;

    public void UpdateHealthHolder(int currentHealth)
    {
        if(currentHealth == transform.childCount) { return; }
        if(currentHealth < 0) { currentHealth = 0; }

        if(currentHealth > transform.childCount)
        {
            GainHearts(currentHealth - transform.childCount);
        }
        else
        {
            LoseHearts(transform.childCount - currentHealth);
        }
    }

    public void GainHearts(int amount)
    {
        for(int i = 0; i < amount; i++)
        {
            GameObject obj = Instantiate(heartPrefab);
            obj.transform.SetParent(transform);
            obj.transform.localEulerAngles = Vector3.zero;
        }

        CheckHeartGridSize();
    }

    void CheckHeartGridSize()
    {
        if (transform.childCount > heartResizeThreshold)
        {
            int multiplier = Mathf.FloorToInt(transform.childCount / heartResizeThreshold);
            gridLayout.cellSize = new Vector2(heartBaseGridSize / (2 * multiplier), heartBaseGridSize / (2 * multiplier));
        }
    }

    public void LoseHearts(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Transform lostHeart = transform.GetChild(transform.childCount - 1);
            lostHeart.SetParent(transform.parent);
            StartCoroutine(AnimateHeartLoss(lostHeart));
        }

        CheckHeartGridSize();        
    }

    IEnumerator AnimateHeartLoss(Transform heart)
    {
        float timeElapsed = 0f;
        Image heartImage = heart.GetComponent<Image>();
        Color startColour = heartImage.color;
        Color endColour = Color.black;
        Vector3 startScale = heart.localScale;
        Vector3 endScale = heart.localScale * 2;

        while(timeElapsed < animationDuration)
        {
            heartImage.color = Color.Lerp(startColour, endColour, timeElapsed / animationDuration);
            heart.localScale = Vector3.Lerp(startScale, endScale, timeElapsed / animationDuration);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(heart.gameObject);
    }
}
