using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Location : MonoBehaviour
{
    public static Location active;

    //Locations are defended by monsters and have nodes in them. The longer the monsters are left on the location the more nodes get cleared out and the less stuff the players get.
    //Nodes lose health at a rate equal to the total health of monsters still in the locations each turn

    public LocationEventManager eventManager;
    public LocationNode finalNode;
    public LocationNode destroyedNode;
    public LocationNode[] nodes;
    public GameObject nodePrefab;
    public Vector3[] nodeSites;
    List<LocationNodeObject> nodeObjects = new List<LocationNodeObject>();

    public enum NodeType { Loot, Settle, Raid, End, Destroyed };
    int activeNode;
    bool hasMovedInActiveNode;

    public GameObject islandObject, bossObject;

    public enum NodeEvent { GainLoot, GainRecruit, GainRelic, LoseACard, RepairShip, Hunt, PowerChallenge, CardSacrifice, Trade, LeaderLevelUp };
    public Sprite[] nodeEventSprites;
    public GameObject[] clanFigurines;
    public Vector3 exploreStartLoc, exploreStartRot, exploreFinalNodeLoc;

    Dictionary<int, GameObject> playerFigure = new Dictionary<int, GameObject>();

    [Header("Animation")]
    public float animationDuration;
    public float figureMoveCurveHeight;

    private void Awake()
    {
        active = this;
    }

    public void StartBattle(int numberOfNodes)
    {
        CleanUp();

        int[] toSpawn = NodeArray(numberOfNodes);
        BuildNodes(toSpawn);

        if (MutliplayerController.active.IsMultiplayerGame())
        {
            Client.active.PassNodeSelectionToServer(toSpawn);
        }
    }

    public void SetupBossBattle()
    {
        CleanUp();
        islandObject.SetActive(false);
        bossObject.SetActive(true);
    }

    void CleanUp()
    {
        ClearPlayerFigures();
    }

    int[] NodeArray(int numberOfNodes)
    {
        List<int> availableIndices = new List<int>();
        for(int i = 0; i < nodes.Length; i++) { availableIndices.Add(i); }

        int[] choice = new int[numberOfNodes];

        for(int i = 0; i < choice.Length; i++)
        {
            choice[i] = availableIndices[Random.Range(0, availableIndices.Count)];
            availableIndices.Remove(choice[i]);
        }

        return choice;
    }

    public void BuildNodes(int[] nodeArray)
    {
        ClearNodes();

        for(int i = 0; i < nodeArray.Length; i++)
        {
            LocationNode node = nodes[nodeArray[i]];
            node.index = i;

            GameObject prefab = Instantiate(nodePrefab);
            prefab.transform.SetParent(transform);
            prefab.transform.localPosition = nodeSites[i];

            LocationNodeObject script = prefab.GetComponent<LocationNodeObject>();
            nodeObjects.Add(script);
            script.Setup(node.name, node.health, node.type, i);
        }


        GameObject finalNodeObj = Instantiate(nodePrefab);
        finalNodeObj.transform.SetParent(transform);
        finalNodeObj.transform.localPosition = exploreFinalNodeLoc;

        LocationNodeObject finalScript = finalNodeObj.GetComponent<LocationNodeObject>();
        nodeObjects.Add(finalScript);
        finalScript.Setup(finalNode.name, finalNode.health, finalNode.type, nodeArray.Length);

        activeNode = 0;
    }

    public void ClearNodes()
    {
        foreach(LocationNodeObject nodeObj in nodeObjects)
        {
            Destroy(nodeObj.gameObject);
        }
        nodeObjects.Clear();
    }

    public void DoDamgeToNode(int amount)
    {
        if(activeNode >= nodeObjects.Count) { return; }

        if (nodeObjects[activeNode].TakeDamage(amount))
        {
            activeNode++;
        }
    }

    public void ConvertNodeToDestroyedNode(LocationNodeObject nodeObj)
    {
        nodeObj.Setup(destroyedNode.name, destroyedNode.health, destroyedNode.type, nodeObjects.IndexOf(nodeObj));
    }

    void CleanUpDestroyedNodes()
    {
        for(int i = nodeObjects.Count-1; i >= 0; i--)
        {
            if(nodeObjects[i] == null) { nodeObjects.RemoveAt(i); }
        }
    }

    public void EnterExploreMode()
    {
        playerFigure.Clear();
        CameraController.main.MoveCameraToExploreMode();
        UIController.main.ToggleEndTurnButton(false);

        CleanUpDestroyedNodes();
        activeNode = 0;
        foreach(LocationNodeObject nodeObject in nodeObjects)
        {
            nodeObject.ExpandNode();
        }

        if (MutliplayerController.active.IsMultiplayerGame())
        {
            Vector3 offset = Vector3.left;

            foreach(Player p in MutliplayerController.active.AllPlayers())
            {
                GameObject playerFigurine = Instantiate(clanFigurines[(int)p.clan]);
                playerFigurine.transform.SetParent(transform);
                playerFigurine.transform.localPosition = exploreStartLoc + offset;
                playerFigurine.transform.localEulerAngles = exploreStartRot;

                offset += Vector3.right;
                playerFigure.Add(MutliplayerController.active.GetPlayerNumber(p), playerFigurine);
            }
        }
        else
        {
            GameObject playerFigurine = Instantiate(clanFigurines[(int)Player.active.clan]);
            playerFigurine.transform.SetParent(transform);
            playerFigurine.transform.localPosition = exploreStartLoc;
            playerFigure.Add(0, playerFigurine);
        }


        foreach (LocationNodeObject nodeObject in nodeObjects)
        {
            nodeObject.transform.localScale = Vector3.one * 0.5f;
        }


        foreach (KeyValuePair<int, GameObject> figurine in playerFigure)
        {
            figurine.Value.transform.localScale = Vector3.one * 0.3f;
        }
    }

    void ClearPlayerFigures()
    {
        foreach (KeyValuePair<int, GameObject> figurine in playerFigure)
        {
            Destroy(figurine.Value);
        }
        playerFigure.Clear();
    }

    public bool CanReachNode(int nodeIndex)
    {
        if (hasMovedInActiveNode) { return false; }
        return nodeIndex == activeNode;
    }

    public void ActivePlayerCompletedCurrentNode()
    {
        if (MutliplayerController.active.IsMultiplayerGame())
        {
            Client.active.TellServerIHaveCompletedMyNode();
        }
        else
        {
            AllPlayersCompletedCurrentNode();
        }
    }

    public void AllPlayersCompletedCurrentNode()
    {
        UnlockNextNodesForExplore();
    }

    public void UnlockNextNodesForExplore()
    {
        activeNode++;
        hasMovedInActiveNode = false;
        eventManager.CleanUpForNextNodes();
        if(activeNode == nodeObjects.Count)
        {
            FinishExploringAndSetUpNextBattle();
        }
    }

    public void FinishExploringAndSetUpNextBattle()
    {
        CleanUp();
        EnemyDecks.main.playerSpawns += 2;
        EnemyDecks.main.locationSpawns += 3;
        GameController.main.StartNewRound();
    }

    public void MovePlayerFigureToLocation(int playerNumber, Vector3 targetLocalLoc, NodeEvent nodeEvent, bool myFigure)
    {
        if (myFigure && MutliplayerController.active.IsMultiplayerGame()) { Client.active.TellServerIHaveChosenANode(targetLocalLoc, nodeEvent); }
        StartCoroutine(MoveFigureTo(playerFigure[playerNumber].transform, targetLocalLoc, nodeEvent, myFigure));

        if(myFigure && nodeEvent == NodeEvent.Trade && MutliplayerController.active.IsMultiplayerGame()) { Client.active.TellServerIAmInTradeEvent(); }
    }

    public NodeEvent[] EventsForNode(NodeType type)
    {
        switch (type)
        {
            case NodeType.Loot:
                return new NodeEvent[] { NodeEvent.GainLoot, NodeEvent.GainRecruit, NodeEvent.GainRelic };
            case NodeType.Raid:
                return new NodeEvent[] { NodeEvent.LoseACard, NodeEvent.RepairShip, NodeEvent.Hunt };
            case NodeType.Settle:
                return new NodeEvent[] { NodeEvent.PowerChallenge, NodeEvent.CardSacrifice, NodeEvent.Trade };
            case NodeType.End:
                return new NodeEvent[] { NodeEvent.LeaderLevelUp };
            case NodeType.Destroyed:
                return new NodeEvent[] { NodeEvent.GainLoot };
            default:
                return new NodeEvent[] { NodeEvent.GainLoot, NodeEvent.GainRecruit, NodeEvent.GainRelic };
        }
    }

    public Sprite EventSprite(NodeEvent nodeEvent){
        return nodeEventSprites[(int)nodeEvent];
    }

    public void CardObjectInEventPressed(CardObject obj)
    {
        eventManager.CardObjectClicked(obj);
    }

    IEnumerator MoveFigureTo(Transform figure, Vector3 to, NodeEvent nodeEvent, bool myFigure)
    {
        float timeElapsed = 0f;
        Vector3 from = figure.localPosition;
        Vector3 mid = Vector3.Lerp(from, to, 0.5f) + (Vector3.back * figureMoveCurveHeight);

        while(timeElapsed < animationDuration)
        {
            float t = timeElapsed / animationDuration;
            figure.localPosition = Vector3.Lerp(Vector3.Lerp(from, mid, t), Vector3.Lerp(mid, to, t), t);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        figure.localPosition = to;

        if (myFigure)
        {
            eventManager.LoadEvent(nodeEvent, figure);
            hasMovedInActiveNode = true;
        }
    }
}

[System.Serializable]
public class LocationNode
{
    public string name;
    public int health;
    public int index;
    public Location.NodeType type;
}
