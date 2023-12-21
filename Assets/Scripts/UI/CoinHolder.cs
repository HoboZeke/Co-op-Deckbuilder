using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinHolder : MonoBehaviour
{
    public GameObject coinPrefab;

    public int coinResizeThreshold;
    public float coinBaseGridSize;
    public GridLayoutGroup gridLayout;

    public float animationDuration;

    public void UpdateCoinHolder(int currentWealth)
    {
        if (currentWealth == transform.childCount) { return; }

        if (currentWealth > transform.childCount)
        {
            GainCoins(currentWealth - transform.childCount);
        }
        else
        {
            LoseCoins(transform.childCount - currentWealth);
        }
    }

    public void GainCoins(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject obj = Instantiate(coinPrefab);
            obj.transform.SetParent(transform);
            obj.transform.localEulerAngles = Vector3.zero;
        }

        CheckCoinGridSize();
    }

    void CheckCoinGridSize()
    {
        if (transform.childCount > coinResizeThreshold)
        {
            int multiplier = Mathf.FloorToInt(transform.childCount / coinResizeThreshold);
            gridLayout.cellSize = new Vector2(coinBaseGridSize / (2 * multiplier), coinBaseGridSize / (2 * multiplier));
        }
    }

    public void LoseCoins(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Transform lostShield = transform.GetChild(transform.childCount - 1);
            lostShield.SetParent(transform.parent);
            StartCoroutine(AnimateCoinLoss(lostShield));
        }

        CheckCoinGridSize();
    }

    IEnumerator AnimateCoinLoss(Transform coin)
    {
        float timeElapsed = 0f;
        Image coinImage = coin.GetComponent<Image>();
        Color startColour = coinImage.color;
        Color endColour = Color.white;
        Vector3 startScale = coin.localScale;
        Vector3 endScale = Vector3.zero;

        while (timeElapsed < animationDuration)
        {
            coinImage.color = Color.Lerp(startColour, endColour, timeElapsed / animationDuration);
            coin.localScale = Vector3.Lerp(startScale, endScale, timeElapsed / animationDuration);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(coin.gameObject);
    }
}
