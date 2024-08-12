using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecruitsForCardDraw : RelicEffect
{
    public override void ApplyEffect(Player toPlayer)
    {
        toPlayer.recruits += strength;
        toPlayer.cardsDrawnAtStartOfTurn -= strength*2;
    }
}
