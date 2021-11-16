using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    private List<int> possibleLinkPos = new List<int> { 0, 1, 2, 3 };
    private LinkPos prevLinkPos;
    private LinkPos dir;
    Vector2 prevPos;

    private void Start()
    {
        CreateDungeon(5);
    }

    void CreateDungeon(int nbrRoom)
    {
        Node thisNode = CreateNode(NodeType.START);
        DungeonManager.instance.allNodes.Add(thisNode.position, thisNode);

        for (int i = 1; i < nbrRoom - 1; ++i)
        {
            thisNode = CreateNode(NodeType.DEFAULT);
            DungeonManager.instance.allNodes.Add(thisNode.position, thisNode);
        }
        thisNode = CreateNode(NodeType.END);
        DungeonManager.instance.allNodes.Add(thisNode.position, thisNode);
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
                //Link Pos
                randIndex = Random.Range(0, possibleLinkPos.Count);
                dir = (LinkPos)possibleLinkPos[randIndex];
                node.linksPosition.Add(dir);
                //next node can't have inverse link pos (can't be left if prev was right)
                if (possibleLinkPos[randIndex] == 0 || possibleLinkPos[randIndex] == 2)
                {
                    prevLinkPos = (LinkPos)possibleLinkPos[randIndex] + 1;
                    possibleLinkPos.Remove(possibleLinkPos[randIndex] + 1);
                }
                else
                {
                    prevLinkPos = (LinkPos)possibleLinkPos[randIndex] - 1;
                    possibleLinkPos.Remove(possibleLinkPos[randIndex] - 1);
                }
                prevPos = node.position;
                break;
            
            case NodeType.END:
                node = new Node(1, NodeType.END);
                switch (dir)
                {
                    case LinkPos.UP:
                        node.position = new Vector2(prevPos.x, prevPos.y + 1);
                        break;
                    case LinkPos.DOWN:
                        node.position = new Vector2(prevPos.x, prevPos.y - 1);
                        break;
                    case LinkPos.LEFT:
                        node.position = new Vector2(prevPos.x - 1, prevPos.y);
                        break;
                    case LinkPos.RIGHT:
                        node.position = new Vector2(prevPos.x + 1, prevPos.y);
                        break;
                    default:
                        break;
                }
                break;
            
            case NodeType.DEFAULT:
                node = new Node(2, NodeType.DEFAULT);
                switch (dir)
                {
                    case LinkPos.UP:
                        node.position = new Vector2(prevPos.x, prevPos.y + 1);
                        break;
                    case LinkPos.DOWN:
                        node.position = new Vector2(prevPos.x, prevPos.y - 1);
                        break;
                    case LinkPos.LEFT:
                        node.position = new Vector2(prevPos.x - 1, prevPos.y);
                        break;
                    case LinkPos.RIGHT:
                        node.position = new Vector2(prevPos.x + 1, prevPos.y);
                        break;
                    default:
                        break;
                }


                List<LinkPos> invalids = new List<LinkPos>();
                Node outNode;
                bool nodeExists = false;
                Vector2 toCheck = Vector2.zero;
                int length = possibleLinkPos.Count;
                for (int i = 0; i < length; ++i)
                {
                    LinkPos check = (LinkPos)possibleLinkPos[i];
                    switch (check)
                    {
                        case LinkPos.UP:
                            toCheck = new Vector2(prevPos.x, prevPos.y + 1);
                            nodeExists = DungeonManager.instance.allNodes.TryGetValue(toCheck, out outNode);
                            if(nodeExists)
                            {
                                invalids.Add(check);
                            }
                            break;
                        case LinkPos.DOWN:
                            toCheck = new Vector2(prevPos.x, prevPos.y - 1);
                            nodeExists = DungeonManager.instance.allNodes.TryGetValue(toCheck, out outNode);
                            if (nodeExists)
                            {
                                invalids.Add(check);
                            }
                            break;
                        case LinkPos.LEFT:
                            toCheck = new Vector2(prevPos.x - 1, prevPos.y);
                            nodeExists = DungeonManager.instance.allNodes.TryGetValue(toCheck, out outNode);
                            if (nodeExists)
                            {
                                invalids.Add(check);
                            }
                            break;
                        case LinkPos.RIGHT:
                            toCheck = new Vector2(prevPos.x + 1, prevPos.y);
                            nodeExists = DungeonManager.instance.allNodes.TryGetValue(toCheck, out outNode);
                            if (nodeExists)
                            {
                                invalids.Add(check);
                            }
                            break;
                        default:
                            break;
                    }
                }

                for (int i = 0; i < invalids.Count; ++i)
                {
                    possibleLinkPos.Remove((int)invalids[i]);
                }
                randIndex = Random.Range(0, possibleLinkPos.Count);
                dir = (LinkPos)possibleLinkPos[randIndex];

                for (int i = 0; i < invalids.Count; ++i)
                {
                    possibleLinkPos.Add((int)invalids[i]);
                }

                node.linksPosition.Add(prevLinkPos);
                node.linksPosition.Add(dir); 
                if (possibleLinkPos[randIndex] == 0 || possibleLinkPos[randIndex] == 2)
                {
                    prevLinkPos = (LinkPos)possibleLinkPos[randIndex] + 1;
                    possibleLinkPos.Remove(possibleLinkPos[randIndex] + 1);
                }
                else
                {
                    prevLinkPos = (LinkPos)possibleLinkPos[randIndex] - 1;
                    possibleLinkPos.Remove(possibleLinkPos[randIndex] - 1);
                }
                prevPos = node.position;
                break;
        }
        print("info : " + node.position + " " + node.type);
        //for (int i = 0; i < node.linksPosition.Count; ++i)
        //{
        //    print("info : " + node.position + " " + i + " " + node.linksPosition[i]);
        //}

        return node;
    }
    /// <summary>
    /// Pour chaque noeud, spawner salle qui correspond. Liste de toutes les salles possibles, filtres liste selon param. 
    /// chaque salle prefab aura component configuration pour filtrer avec param rempli dedans 
    /// Porte optionnelles obligatoire
    /// </summary>
    /// <param name="firstNode"></param>

    void CreateLink(Node firstNode)
    {
        
        
    }
    
}


