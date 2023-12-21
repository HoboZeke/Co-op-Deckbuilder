using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager main;
    public AudioSource drawAudio, coinAudio, attackAudio, recruitAudio, playedCardAudio, tauntAudio;
    public AudioSource uiBasicButtonAudio, mapAudio;

    private void Awake()
    {
        main = this;
    }

    public void UIButtonPressAudioEvent()
    {
        uiBasicButtonAudio.pitch = Random.Range(0.9f, 1.1f);
        uiBasicButtonAudio.Play();
    }

    public void OpenAndCloseMapAudioEvent()
    {
        mapAudio.pitch = Random.Range(0.9f, 1.1f);
        mapAudio.Play();
    }
    
    public void CardDrawnAudioEvent()
    {
        drawAudio.pitch = Random.Range(0.75f, 1.25f);
        drawAudio.Play();
    }

    public void CoinHittingWoodAudioEvent()
    {
        coinAudio.pitch = Random.Range(0.75f, 1.25f);
        coinAudio.Play();
    }

    public void AttackAudioEvent() 
    {
        attackAudio.pitch = Random.Range(0.75f, 1.25f);
        attackAudio.Play();
    }

    public void ChooseAudioEventForCardPlayed(CardObject playedCard)
    {
        List<CardEffect.EffectTag> tags = playedCard.OnPlayCardEffectTags();

        if (tags.Contains(CardEffect.EffectTag.Taunt))
        {
            TauntAudioEvent();
        }
        else if (tags.Contains(CardEffect.EffectTag.Recruit))
        {
            RecruitGainedAudioEvent();
        }
    }

    public void ChooseAudioEventForCardActivated(CardObject playedCard)
    {
        List<CardEffect.EffectTag> tags = playedCard.OnActivateCardEffectTags();

        if (tags.Contains(CardEffect.EffectTag.Taunt))
        {
            TauntAudioEvent();
        }
    }

    public void CardPlayAudioEvent()
    {
        playedCardAudio.pitch = Random.Range(0.75f, 1.25f);
        playedCardAudio.Play();
    }

    public void RecruitGainedAudioEvent()
    {
        recruitAudio.pitch = Random.Range(0.75f, 1.25f);
        recruitAudio.Play();
    }

    public void TauntAudioEvent()
    {
        tauntAudio.pitch = Random.Range(0.75f, 1.25f);
        tauntAudio.Play();
    }

}
