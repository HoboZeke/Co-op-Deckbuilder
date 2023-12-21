using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : MonoBehaviour
{
    public CardObject abilityCard;
    public bool abilityUsed;

    public float animationDuration;

    private void Start()
    {
        TurnOffAbility();
    }

    public void TurnOffAbility()
    {
        abilityUsed = true;
        StartCoroutine(DisableAbility());
    }

    public void TurnOnAbility()
    {
        abilityUsed = false;
        StartCoroutine(EnableAbility());
    }

    public bool IsAvailable()
    {
        return !abilityUsed;
    }

    IEnumerator DisableAbility()
    {
        float timeElapsed = 0f;
        Vector3 startRot = abilityCard.transform.localEulerAngles;
        Vector3 endRot = new Vector3(0f, 180f, 0f);

        while(timeElapsed < animationDuration)
        {
            abilityCard.transform.localEulerAngles = Vector3.Lerp(startRot, endRot, timeElapsed / animationDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        abilityCard.transform.localEulerAngles = endRot;
    }

    IEnumerator EnableAbility()
    {
        float timeElapsed = 0f;
        Vector3 startRot = abilityCard.transform.localEulerAngles;
        Vector3 endRot = new Vector3(0f, 0f, 0f);

        while (timeElapsed < animationDuration)
        {
            abilityCard.transform.localEulerAngles = Vector3.Lerp(startRot, endRot, timeElapsed / animationDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        abilityCard.transform.localEulerAngles = endRot;
    }
}
