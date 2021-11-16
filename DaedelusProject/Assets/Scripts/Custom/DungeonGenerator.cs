using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    private List<int> possibleLinkPos = new List<int> { 0, 1, 2, 3 };

    private void Start()
    {
        CreateDungeon(2);
    }

    void CreateDungeon(int nbrRoom)
    {
        Node thisNode = CreateNode(NodeType.START);


        DungeonManager.instance.allNodes.Add(thisNode.position, thisNode);


        Vector2 prevPos = thisNode.position;
        LinkPos prevLinkPos = thisNode.linksPosition[0];

        for (int i = 1; i < nbrRoom - 1; ++i)
        {
            thisNode = CreateNode(NodeType.DEFAULT);
            //DungeonManager.instance.allNodes.Add(thisNode.position, thisNode);
            thisNode.linksPosition[0] = prevLinkPos;
            possibleLinkPos.Add((int)prevLinkPos);
            prevPos = thisNode.position;
            prevLinkPos = thisNode.linksPosition[2];
            //DungeonManager.instance.allNodes.Add(thisNode.position, thisNode);
        }
        CreateNode(NodeType.END);
    }

    Node CreateNode(NodeType type)
    {
        Node node = null;
        int randIndex = 0;
        switch (type)
        {
            case NodeType.START:
                node = new Node(1, NodeType.START, Difficulty.EASY);
                node.position = Vector2.zero;
                randIndex = Random.Range(0, possibleLinkPos.Count);
                node.linksPosition.Add((LinkPos)possibleLinkPos[randIndex]);
                if(possibleLinkPos[randIndex] == 0 || possibleLinkPos[randIndex] == 2)
                {
                    possibleLinkPos.Remove(possibleLinkPos[randIndex] + 1);
                }
                break;
            
            case NodeType.END:
                node = new Node(1, NodeType.END);
                break;
            
            case NodeType.DEFAULT:
                node = new Node(2, NodeType.DEFAULT); 
                randIndex = Random.Range(0, possibleLinkPos.Count);
                node.linksPosition.Add((LinkPos)possibleLinkPos[randIndex]);
                node.linksPosition.Add((LinkPos)possibleLinkPos[randIndex]);
                possibleLinkPos.Remove(randIndex);
                break;
        }
        return node;
    }

    void CreateLink(Node firstNode)
    {
        
        
    }
    
}


