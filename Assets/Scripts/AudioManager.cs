using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager main;
    [SerializeField] AudioSource drawAudio, coinAudio, attackAudio, recruitAudio, playedCardAudio, tauntAudio;
    [SerializeField] AudioSource uiBasicButtonAudio, mapAudio;
    [SerializeField] AudioSource backgroundAudio;
    [SerializeField] Slider volumeSlider;
    float volumeValue;

    private void Awake()
    {
        main = this;
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey("Volume")) { ChangeVolume(PlayerPrefs.GetFloat("Volume")); }
        volumeSlider.value = volumeValue;
    }

    public void ChangeVolume(float newValue)
    {
        drawAudio.volume = newValue;
        coinAudio.volume = newValue;
        attackAudio.volume = newValue;
        recruitAudio.volume = newValue * 0.5f;
        playedCardAudio.volume = newValue;
        tauntAudio.volume = newValue;
        uiBasicButtonAudio.volume = newValue;
        mapAudio.volume = newValue;
        backgroundAudio.volume = newValue;
        PlayerPrefs.SetFloat("Volume", newValue);
    }

    public void UIButtonPressAudioEvent()
    {
        uiBasicButtonAudio.pitch = Random.Range(0.9f, 1.1f);
        uiBasicButtonAudio.Play();
        Debug.Log("AUDIO played UI sound");
    }

    public void OpenAndCloseMapAudioEvent()
    {
        mapAudio.pitch = Random.Range(0.9f, 1.1f);
        mapAudio.Play();
        Debug.Log("AUDIO played Map sound");
    }
    
    public void CardDrawnAudioEvent()
    {
        drawAudio.pitch = Random.Range(0.75f, 1.25f);
        drawAudio.Play();
        Debug.Log("AUDIO played Card Draw sound");
    }

    public void CoinHittingWoodAudioEvent()
    {
        coinAudio.pitch = Random.Range(0.75f, 1.25f);
        coinAudio.Play();
        Debug.Log("AUDIO played Coin hitting Wood sound");
    }

    public void AttackAudioEvent() 
    {
        attackAudio.pitch = Random.Range(0.75f, 1.25f);
        attackAudio.Play();
        Debug.Log("AUDIO played attack sound");
    }

    public void ChooseAudioEventForCardPlayed(CardObject playedCard)
    {
        List<CardEffect.EffectTag> tags = playedCard.OnPlayCardEffectTags();

        if (tags.Contains(CardEffect.EffectTag.Taunt))
        {
            TauntAudioEvent();
        }
        else if (tags.Contains(CardEffect.EffectTag.Recruit) && playedCard.zoneScript.player == Player.active)
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
        Debug.Log("AUDIO played Card play sound");
    }

    public void RecruitGainedAudioEvent()
    {
        recruitAudio.pitch = Random.Range(0.75f, 1.25f);
        recruitAudio.Play();
        Debug.Log("AUDIO played recruit gained sound");
    }

    public void TauntAudioEvent()
    {
        tauntAudio.pitch = Random.Range(0.75f, 1.25f);
        tauntAudio.Play();
        Debug.Log("AUDIO played taunt sound");
    }

}
