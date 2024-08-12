using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fog : MonoBehaviour
{
    public static Fog main;
    
    public Vector3[] fogGapRotations;
    public Transform fogSpriteMask;

    public Transform[] fogSections;
    public float fogShowZ, fogHideZ;

    int showFogIndex;
    public float animationDuration;

    private void Awake()
    {
        main = this;
    }

    public void MoveFogClearingToActivePlayersTurn(int activePLayer)
    {
        int index = MutliplayerController.active.PlayerPosition(activePLayer);
        //Debug.Log("FOG moving to player " + activePLayer + " at playmat index " + index);
        //
        //StartCoroutine(MoveMaskToNewPosition(fogGapRotations[index]));

        StartCoroutine(ShowFogSection(fogSections[showFogIndex]));
        StartCoroutine(HideFogSection(fogSections[index]));
        showFogIndex = index;
    }

    IEnumerator MoveMaskToNewPosition(Vector3 endRot)
    {
        Vector3 startRot = fogSpriteMask.localEulerAngles;

        float timeElapsed = 0f;

        while(timeElapsed < animationDuration)
        {
            float t = timeElapsed / animationDuration;
            fogSpriteMask.localEulerAngles = Vector3.Lerp(startRot, endRot, t);

            timeElapsed += Time.deltaTime;
            yield return null;
        }
        fogSpriteMask.localEulerAngles = endRot;
    }

    IEnumerator HideFogSection(Transform section)
    {
        Vector3 endPos = new Vector3(section.localPosition.x, section.localPosition.y, fogHideZ);
        Vector3 startPos = new Vector3(section.localPosition.x, section.localPosition.y, fogShowZ);
        float timeElapsed = 0f;

        while (timeElapsed < animationDuration)
        {
            float t = timeElapsed / animationDuration;
            section.localPosition = Vector3.Lerp(startPos, endPos, t);

            timeElapsed += Time.deltaTime;
            yield return null;
        }
        fogSpriteMask.localPosition = endPos;
    }

    IEnumerator ShowFogSection(Transform section)
    {
        Vector3 endPos = new Vector3(section.localPosition.x, section.localPosition.y, fogShowZ);
        Vector3 startPos = new Vector3(section.localPosition.x, section.localPosition.y, fogHideZ);
        float timeElapsed = 0f;

        while (timeElapsed < animationDuration)
        {
            float t = timeElapsed / animationDuration;
            section.localPosition = Vector3.Lerp(startPos, endPos, t);

            timeElapsed += Time.deltaTime;
            yield return null;
        }
        fogSpriteMask.localPosition = endPos;
    }
}
