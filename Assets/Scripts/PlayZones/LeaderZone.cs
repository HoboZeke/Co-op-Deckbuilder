using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderZone : MonoBehaviour
{
    public Zones zone;
    public CardObject leaderCard;

    public float animationDuration;
    public bool leaderSpent;

    public void Setup(int leader)
    {
        if(leaderCard != null)
        {
            Destroy(leaderCard.gameObject);
            leaderCard = null;
        }

        leaderCard = LeaderArchive.main.SpawnLeader(leader);
        leaderCard.zoneScript = zone;
        leaderCard.referenceIndex = -1;

        leaderCard.transform.SetParent(transform);
        leaderCard.transform.localPosition = Vector3.zero;
        leaderCard.transform.localScale = Vector3.one * 1.5f;

        leaderCard.SetAsLeaderCard();
        leaderCard.FaceTowards(zone.hand.focusPoint);
        leaderCard.zone = Zones.Type.Leader;
    }

    public void ReturnUsedLeaderCardToZone()
    {
        leaderCard.SetZone(Zones.Type.Moving, Zones.Type.Leader);
        leaderCard.transform.SetParent(transform);
        StartCoroutine(AnimateMoveCardToZone(leaderCard, true));
        leaderSpent = true;
    }

    public void RefreshLeader()
    {
        Debug.Log("Refreshing leader for player " + zone.player.name);
        StartCoroutine(RefreshLeader(leaderCard, false));
        leaderSpent = false;
    }

    IEnumerator RefreshLeader(CardObject card, bool spent)
    {
        while(card.zone == Zones.Type.Moving)
        {
            yield return null;
        }

        StartCoroutine(AnimateMoveCardToZone(card, spent));
    }

    IEnumerator AnimateMoveCardToZone(CardObject card, bool spent)
    {
        float timeElapsed = 0f;
        Vector3 start = card.transform.localPosition;
        Vector3 startRot = card.transform.localEulerAngles;

        Vector3 end = Vector3.zero;
        Vector3 endRot = Vector3.zero;
        if (spent)
        {
            endRot = new Vector3(0f, 180f, 0f);
        }

        while (timeElapsed < animationDuration)
        {
            card.transform.localPosition = Vector3.Lerp(start, end, timeElapsed / animationDuration);
            card.transform.localEulerAngles = Vector3.Lerp(startRot, endRot, timeElapsed / animationDuration);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        card.transform.localPosition = end;
        card.transform.localEulerAngles = endRot;
        if(!spent) card.FaceTowards(zone.hand.focusPoint);
        card.zone = Zones.Type.Leader;
    }
}
