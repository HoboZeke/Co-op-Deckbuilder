using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugInfoViewer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] List<DeckMismatchHolder> deckMismatchHolders = new List<DeckMismatchHolder>();

    public void ParseDesyncEventString(string s)
    {
        string[] segments = s.Split('[',']', '}', '{');

        string[] words = segments[0].Split(' ');
        int currentIndex = 0;

        //PLayer X BOARD SYNC RESULT: DESYNC!
        titleText.text = words[0] + " " + words[1] + " " + words[2] + " " + words[3] + " " + words[4] + " " + words[5];
        currentIndex = 6;

        

        if(words.Length > currentIndex)
        {
            //Find the next series of words before we get to deck info
            string holderTitle = "";
            for (int i = currentIndex; i < words.Length; i++)
            {
                holderTitle += words[i] + " ";
            }

            holderTitle.Remove(holderTitle.Length - 1);

            DeckMismatchHolder holder = deckMismatchHolders[0];
            holder.Clear();
            holder.info.text = holderTitle;
            holder.leftColumnTitle.text = words[0] + " " + words[1];
            holder.rightColumnTitle.text = "My Boardstate";

            CompareDecks(holder, segments[1], segments[3]);
            holder.holder.SetActive(true);

            if(segments.Length >= 8)
            {
                RunAnotherDeckMismatchHolder(segments[4], words[0]+words[1], segments[5], segments[7], 1);
                deckMismatchHolders[1].holder.SetActive(true);
            }

            if (segments.Length >= 12)
            {
                RunAnotherDeckMismatchHolder(segments[8], words[0] + words[1], segments[9], segments[11], 2);
                deckMismatchHolders[2].holder.SetActive(true);
            }
            else
            {
                deckMismatchHolders[2].holder.SetActive(false);
            }

            if (segments.Length >= 16)
            {
                RunAnotherDeckMismatchHolder(segments[12], words[0] + words[1], segments[13], segments[15], 3);
                deckMismatchHolders[3].holder.SetActive(true);
            }
            else
            {
                deckMismatchHolders[3].holder.SetActive(false);
            }

            if (segments.Length >= 20)
            {
                RunAnotherDeckMismatchHolder(segments[16], words[0] + words[1], segments[17], segments[19], 4);
                deckMismatchHolders[4].holder.SetActive(true);
            }
            else
            {
                deckMismatchHolders[4].holder.SetActive(false);
            }

            if (segments.Length >= 24)
            {
                RunAnotherDeckMismatchHolder(segments[20], words[0] + words[1], segments[21], segments[23], 5);
                deckMismatchHolders[5].holder.SetActive(true);
            }
            else
            {
                deckMismatchHolders[5].holder.SetActive(false);
            }
        }
    }

    void RunAnotherDeckMismatchHolder(string holderTitle, string leftColumnTitle, string remoteSegment, string localSegment, int holderCount)
    {
        DeckMismatchHolder holder = deckMismatchHolders[holderCount];
        holder.Clear();
        holder.info.text = holderTitle;
        holder.leftColumnTitle.text = leftColumnTitle;
        holder.rightColumnTitle.text = "My Boardstate";

        CompareDecks(holder, remoteSegment, localSegment);
    }

    void CompareDecks(DeckMismatchHolder holder, string remoteSegment, string localSegment)
    {
        //Parse remote Deck
        string[] remoteDeck = remoteSegment.Split(",");
        //Trim [ chars, last string will just be ] so don't do the full for loop
        remoteDeck[0].Remove(0);
        for (int i = 0; i < remoteDeck.Length - 1; i++)
        {
            holder.leftColumnText.text += remoteDeck[i] + "\n";
        }

        //Parse local Deck
        string[] localDeck = localSegment.Split(",");
        //Trim [ chars, last string will just be ] so don't do the full for loop
        localDeck[0].Remove(0);
        for (int i = 0; i < localDeck.Length - 1; i++)
        {
            holder.rightColumnText.text += localDeck[i] + "\n";
        }

        //Compare decks
        int length = remoteDeck.Length - 1;
        if (localDeck.Length - 1 > length) { length = localDeck.Length - 1; }

        for (int i = 0; i < length; i++)
        {
            //If bothdecks have an entry for the index
            if (i < localDeck.Length - 1 && i < remoteDeck.Length - 1)
            {
                if (localDeck[i] == remoteDeck[i])
                {
                    holder.centreColumn.text += "<color=green>=</color>\n";
                }
                else
                {
                    string[] remoteCard = remoteDeck[i].Split("/");
                    string[] localCard = localDeck[i].Split("/");
                    if (remoteCard[0] == localCard[0])
                    {
                        holder.centreColumn.text += "<color=yellow>!=</color>\n";
                    }
                    else
                    {
                        holder.centreColumn.text += "<color=red>!=</color>\n";
                    }
                }
            }
            else
            {
                holder.centreColumn.text += "<color=red>!=</color>\n";
            }
        }
    }

}

[Serializable]
public class DeckMismatchHolder
{
    public GameObject holder;
    public TextMeshProUGUI info, leftColumnTitle, leftColumnText, rightColumnTitle, rightColumnText, centreColumn;

    public void Clear()
    {
        info.text = string.Empty;
        leftColumnTitle.text = string.Empty;
        rightColumnTitle.text = string.Empty;
        centreColumn.text = string.Empty;
        leftColumnText.text = string.Empty;
        rightColumnText.text = string.Empty;
    }
}
