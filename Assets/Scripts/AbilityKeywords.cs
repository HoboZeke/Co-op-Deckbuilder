using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AbilityKeywords
{
    public enum Keyword { Taunt, Impact, Recruit, Shield, Stun, Repair, Pierce, Bounty, Draw }

    public static string KeywordDescription(Keyword keyword, string str)
    {
        switch(keyword)
        {
            case Keyword.Taunt:
                return "Moves the targetted enemy card into your play zone.";
            case Keyword.Impact:
                return "Deals " + str + " damage to all enemies in your play zone when deployed.";
            case Keyword.Recruit:
                return "Gain " + str + " extra recruits so you can deploy more cards this turn.";
            case Keyword.Shield:
                return "Prevents " + str + " damage to the active player, any excess shield is removed at the end of the turn.";
            case Keyword.Stun:
                return "Prevents the target card from attacking or activating any of it's abilities for a turn.";
            case Keyword.Repair:
                return "Restore " + str + " health to the active players boat.";
            case Keyword.Pierce:
                return "Ignores armour when dealing damage.";
            case Keyword.Bounty:
                return "Each player gains " + str + " coin when this card dies.";
            case Keyword.Draw:
                return "Draws " + str + " cards";
            default:
                return "No desciptor available";
        }
    }

    public static string KeywordDescription(Keyword keyword)
    {
        return KeywordDescription(keyword, "X");
    }

    public static string KeywordDescription(Keyword keyword, int strength)
    {
        return KeywordDescription(keyword, strength.ToString());
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
