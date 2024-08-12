using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllPlayersDrawOnPlay : CardEffect
{
    public override void EnterPlay(CardObject version)
    {
        if (!MutliplayerController.active.IAmInSomeoneElsesGame())
        {
            Client.active.TellServerEveryoneDrawsACard();
        }
    }
}
