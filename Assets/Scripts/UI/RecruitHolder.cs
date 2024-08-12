using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecruitHolder : MonoBehaviour
{
    public GameObject recuitPrefab;

    public int recruitResizeThreshold;
    public float recruitBaseGridSize;
    public GridLayoutGroup gridLayout;
    public Color spentColour, freshColour;
    bool spent;

    public float animationDuration;

    public void UpdateRecruitHolder(int currentRecruits)
    {
        if (currentRecruits == transform.childCount && !spent) { return; }
        if (currentRecruits == 0 && spent) { return; }

        if (currentRecruits > transform.childCount)
        {
            GainRecruits(currentRecruits - transform.childCount);
        }
        else if (currentRecruits == transform.childCount && spent)
        {
            GainRecruits(1);
        }
        else
        {
            LoseRecruits(transform.childCount - currentRecruits);
        }
    }

    public void GainRecruits(int amount)
    {
        if (amount <= 0) { return; }

        if(spent)
        {
            transform.GetChild(0).GetComponent<Image>().color = freshColour;
            spent = false;
            amount--;
        }

        for (int i = 0; i < amount; i++)
        {
            GameObject obj = Instantiate(recuitPrefab);
            obj.transform.SetParent(transform);
            obj.transform.localEulerAngles = Vector3.zero;
        }

        CheckRecruitGridSize();
    }

    void CheckRecruitGridSize()
    {
        if (transform.childCount > recruitResizeThreshold)
        {
            int multiplier = Mathf.FloorToInt(transform.childCount / recruitResizeThreshold);
            gridLayout.cellSize = new Vector2(recruitBaseGridSize / (2 * multiplier), recruitBaseGridSize / (2 * multiplier));
        }
    }

    public void LoseRecruits(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Transform lostRecruit = transform.GetChild(transform.childCount - 1);
            if (lostRecruit != transform.GetChild(0))
            {
                lostRecruit.SetParent(transform.parent);
                StartCoroutine(AnimateRecruitLoss(lostRecruit));
            }
            else
            {
                lostRecruit.GetComponent<Image>().color = spentColour;
                spent = true;
            }
        }

        CheckRecruitGridSize();
    }

    IEnumerator AnimateRecruitLoss(Transform recruit)
    {
        float timeElapsed = 0f;
        Image recruitImage = recruit.GetComponent<Image>();
        Color startColour = recruitImage.color;
        Color endColour = Color.white;
        Vector3 startScale = recruit.localScale;
        Vector3 endScale = recruit.localScale * 2;

        while (timeElapsed < animationDuration)
        {
            recruitImage.color = Color.Lerp(startColour, endColour, timeElapsed / animationDuration);
            recruit.localScale = Vector3.Lerp(startScale, endScale, timeElapsed / animationDuration);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(recruit.gameObject);
    }
}
