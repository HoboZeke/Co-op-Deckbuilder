using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnAtEndOfTurn : CardEffect
{
    public int cardToSpawnId;
    public int amountToSpawn;

    public override void EndOfTurn(CardObject version)
    {
        //Spawn on the next frame to avoid the newly spawned monster doing damage or triggering any end of turn effects.
        StartCoroutine(SpawnOnNextFrame(version));
    }

    IEnumerator SpawnOnNextFrame(CardObject version)
    {
        yield return null;

        if (!MutliplayerController.active.IsMultiplayerGame() || MutliplayerController.active.IAmHost())
        {
            if (version.zone == Zones.Type.Location)
            {
                for (int i = 0; i < amountToSpawn; i++)
                {
                    if (MutliplayerController.active.IsMultiplayerGame()) { EnemyDecks.main.MultiplayerSpawnBeastCardToLocationDuringGame(cardToSpawnId); }
                    else { EnemyDecks.main.SpawnBeastCardToLocation(cardToSpawnId); }
                }
            }
            else
            {
                for (int i = 0; i < amountToSpawn; i++)
                {
                    if (MutliplayerController.active.IsMultiplayerGame()) { EnemyDecks.main.MultiplayerSpawnBeastCardToPlayerDuringGame(cardToSpawnId, version.zoneScript.player); }
                    else { EnemyDecks.main.SpawnBeastCardToPlayer(cardToSpawnId, version.zoneScript.player); }
                }
            }
        }
    }
}
