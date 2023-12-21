using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsManager : MonoBehaviour
{
    public static EffectsManager main;

    public float animationDuration;
    public GameObject attackEffectPrefab;
    public float zCurvePeak;

    int effectsRunning;
    public bool isAnimating;
    public List<VisualEffect> effectsQueue = new List<VisualEffect>();
    public enum EffectType { DamageToCard, DamageToPlayer, ShieldPlayer }

    bool progressQueueAtEndOfFrame;

    private void Awake()
    {
        main = this;
    }

    private void LateUpdate()
    {
        if (progressQueueAtEndOfFrame) { ProgressEffectQueue(); progressQueueAtEndOfFrame = false; }
    }

    public void ConcurrentDamageToAnotherCardEffect(CardObject source, CardObject target, int damageValue, bool pierce)
    {
        effectsQueue.Add(new VisualEffect(EffectType.DamageToCard, source, target, damageValue, pierce, true));
        if (!isAnimating) { progressQueueAtEndOfFrame = true; }
    }

    public void DamageToAnotherCardEffect(CardObject source, CardObject target, int damageValue, bool pierce)
    {
        effectsQueue.Add(new VisualEffect(EffectType.DamageToCard, source, target, damageValue, pierce));
        if (!isAnimating) { ProgressEffectQueue(); }
    }

    public void DamageToPlayerBoatEffect(CardObject source, Player target, int damageValue, bool pierce)
    {
        effectsQueue.Add(new VisualEffect(EffectType.DamageToPlayer, source, target, damageValue, pierce));
        if (!isAnimating) { ProgressEffectQueue(); }
    }

    public void ConcurrentDamageToPlayerBoatEffect(CardObject source, Player target, int damageValue, bool pierce)
    {
        effectsQueue.Add(new VisualEffect(EffectType.DamageToPlayer, source, target, damageValue, pierce, true));
        if (!isAnimating) { progressQueueAtEndOfFrame = true; }
    }

    void CompleteSlashEffect(VisualEffect slashEffect)
    {
        slashEffect.cardTarget.TakeDamage(slashEffect.strength, slashEffect.pierce);
        OnEffectCompletion(slashEffect);
    }

    void OnEffectCompletion(VisualEffect completedEffect)
    {
        effectsQueue.Remove(completedEffect);
        effectsRunning--;
        if (effectsRunning == 0)
        {
            isAnimating = false;
            ProgressEffectQueue();
        }
    }

    void ProgressEffectQueue()
    {
        Debug.Log("EFFECTS: Processing queue of size " + effectsQueue.Count);
        if(effectsQueue.Count > 0)
        {
            if (!isAnimating)
            {
                isAnimating = true;
                VisualEffect currentEffect = effectsQueue[0];

                //ConcurrentEffects
                if (currentEffect.concurrentWithOtherEffects)
                {
                    List<VisualEffect> concurrentEffects = new List<VisualEffect>();
                    concurrentEffects.Add(currentEffect);

                    for(int i = 1; i < effectsQueue.Count; i++)
                    {
                        if (effectsQueue[i].concurrentWithOtherEffects) { concurrentEffects.Add(effectsQueue[i]); }
                        else { break; }
                    }

                    Debug.Log("EFFECTS: Running " + concurrentEffects.Count + " concurrentEffects");

                    foreach(VisualEffect effect in concurrentEffects)
                    {
                        RunEffect(effect, 1f);
                    }
                }
                else
                {
                    RunEffect(currentEffect, 1f);
                }
            }
            else
            {
                Debug.Log("EFFECT ERROR: trying to progress the queue while still animating");
            }
        }
    }

    void RunEffect(VisualEffect effect, float speed)
    {
        switch (effect.typeTag)
        {
            case EffectType.DamageToCard:
                StartCoroutine(AnimatedSlashEffect(effect, speed));
                effectsRunning++;
                break;
            case EffectType.DamageToPlayer:
                StartCoroutine(AnimatedMonsterAttackEffect(effect, speed));
                effectsRunning++; 
                break;
        }
    }

    IEnumerator AnimatedSlashEffect(VisualEffect effect, float speedMultiplier)
    {
        float timeElapsed = 0f;

        Vector3 start = effect.cardSource.transform.position;
        Vector3 end = effect.cardTarget.transform.position;

        GameObject slashObj = Instantiate(attackEffectPrefab);
        slashObj.transform.SetParent(transform);
        slashObj.transform.position = start;

        Vector3 diff = (start - end);
        float zAngle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        slashObj.transform.localEulerAngles = new Vector3(0f, 0f, zAngle);

        while(timeElapsed < animationDuration)
        {
            float z = 0;
            float halfWayMarker = animationDuration / 2;
            if(timeElapsed <= halfWayMarker) { z = Mathf.Lerp(0f, zCurvePeak, timeElapsed / (animationDuration / 2)); }
            else {
                float t = timeElapsed - halfWayMarker;
                z = Mathf.Lerp(zCurvePeak, 0f, t / halfWayMarker);
            }

            slashObj.transform.position = Vector3.Lerp(start, end, timeElapsed / animationDuration) + new Vector3(0f, 0f, z);

            timeElapsed += Time.deltaTime * speedMultiplier;
            yield return null;
        }

        CompleteSlashEffect(effect);
        Destroy(slashObj);
    }

    IEnumerator AnimatedMonsterAttackEffect(VisualEffect effect, float speedMultiplier)
    {
        float timeElapsed = 0f;

        Vector3 start = effect.cardSource.transform.position;
        Vector3 end = effect.playerTarget.BoatModelLocation();
        end = new Vector3(Mathf.Lerp(end.x, start.x, 0.5f), end.y, end.z);

        GameObject slashObj = Instantiate(attackEffectPrefab);
        slashObj.transform.SetParent(transform);
        slashObj.transform.position = start;

        Vector3 diff = (start - end);
        float zAngle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        slashObj.transform.localEulerAngles = new Vector3(0f, 0f, zAngle);

        while (timeElapsed < animationDuration)
        {
            float z = 0;
            float halfWayMarker = animationDuration / 2;
            if (timeElapsed <= halfWayMarker) { z = Mathf.Lerp(0f, zCurvePeak, timeElapsed / (animationDuration / 2)); }
            else
            {
                float t = timeElapsed - halfWayMarker;
                z = Mathf.Lerp(zCurvePeak, 0f, t / halfWayMarker);
            }

            slashObj.transform.position = Vector3.Lerp(start, end, timeElapsed / animationDuration) + new Vector3(0f, 0f, z);

            timeElapsed += Time.deltaTime * speedMultiplier;
            yield return null;
        }

        OnEffectCompletion(effect);
        Destroy(slashObj);
    }

    
}

public class VisualEffect
{
    public CardObject cardSource;
    public CardObject cardTarget;
    public Player playerTarget;
    public int strength;
    public bool pierce;
    public EffectsManager.EffectType typeTag;
    public bool concurrentWithOtherEffects;

    public VisualEffect(EffectsManager.EffectType tag, CardObject source, CardObject target, int power, bool doesPierce)
    {
        typeTag = tag;
        cardSource = source;
        cardTarget = target;
        strength = power;
        pierce = doesPierce;
    }

    public VisualEffect(EffectsManager.EffectType tag, CardObject source, CardObject target, int power, bool doesPierce, bool concurrent)
    {
        typeTag = tag;
        cardSource = source;
        cardTarget = target;
        strength = power;
        pierce = doesPierce;
        concurrentWithOtherEffects = concurrent;
    }

    public VisualEffect(EffectsManager.EffectType tag, CardObject source, Player target, int power, bool doesPierce)
    {
        typeTag = tag;
        cardSource = source;
        playerTarget = target;
        strength = power;
        pierce = doesPierce;
    }

    public VisualEffect(EffectsManager.EffectType tag, CardObject source, Player target, int power, bool doesPierce, bool concurrent)
    {
        typeTag = tag;
        cardSource = source;
        playerTarget = target;
        strength = power;
        pierce = doesPierce;
        concurrentWithOtherEffects = concurrent;
    }
}
