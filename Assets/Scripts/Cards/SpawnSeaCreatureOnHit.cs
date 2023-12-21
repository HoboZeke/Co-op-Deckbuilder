using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSeaCreatureOnHit : CardEffect
{
    public int seaCreatureIndex;

    public override void OnTakeDamage(CardObject version)
    {
        if (version.isStunned) { return; }

        if (!MutliplayerController.active.IAmInSomeoneElsesGame())
        {
            int i = Random.Range(0, MutliplayerController.active.playerCount);
            Player randomlyChosenPlayer = MutliplayerController.active.GetPlayerScript(i);

            EnemyDecks.main.SpawnSeaCreatureCardToPlayer(seaCreatureIndex, randomlyChosenPlayer);
            Client.active.TellServerIHaveSpawnedSeaCreatureForPlayer(seaCreatureIndex, i);
        }
    }
}
