using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AbilityKeywords
{
    public enum Keyword { Taunt, Impact, Recruit, Shield, Stun, Repair, Pierce, Bounty, Draw }

    public static string KeywordDescription(Keyword keyword)
    {
        switch(keyword)
        {
            case Keyword.Taunt:
                return "Moves the targetted enemy card into your play zone.";
            case Keyword.Impact:
                return "Deals X damage to all enemies in your play zone when deployed.";
            case Keyword.Recruit:
                return "Gain X extra recruits so you can deploy more cards this turn.";
            case Keyword.Shield:
                return "Prevents X damage to the active player, any excess shield is removed at the end of the turn.";
            case Keyword.Stun:
                return "Prevents the target card from attacking or activating any of it's abilities for a turn.";
            case Keyword.Repair:
                return "Restore X health to the active players boat.";
            case Keyword.Pierce:
                return "Ignores armour when dealing damage.";
            case Keyword.Bounty:
                return "Each player gains X coin when this card dies.";
            case Keyword.Draw:
                return "Draws X cards";
            default:
                return "No desciptor available";
        }
    }

    public static bool IsStringAKeyword(string s)
    {
        bool answer = false;
        string[] keywords = System.Enum.GetNames(typeof(Keyword));

        foreach (string keyword in keywords)
        {
            if (s == keyword) { answer = true; break; }
        }

        return answer;
    }
}
