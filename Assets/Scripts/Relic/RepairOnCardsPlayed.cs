using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairOnCardsPlayed : RelicEffect
{
    [SerializeField] int repairCountThreshold;

    private void Start()
    {
        StatTracker.local.OnCardPlayed += ACardHasBeenPlayed;
    }

    private void ACardHasBeenPlayed()
    {
        bool heldByActivePlayer = false;
        foreach(Relic r in attachedRelics)
        {
            if(r.currentlyOwnedByPLayerList.Contains(Player.active)) heldByActivePlayer = true;
        }
        if(!heldByActivePlayer) { return; }

        int count = StatTracker.local.GetCardsPlayedThisRound(Player.active);
        if(count >= repairCountThreshold)
        {
            Player.active.GainHealth(strength);
            if (MutliplayerController.active.IsMultiplayerGame()) { Client.active.TellServerIHaveGainedHealth(strength); }
            count -= repairCountThreshold;
        }


       foreach(Relic r in attachedRelics)
        {
            r.UpdateRelicText(count.ToString());
        }
    }
}
