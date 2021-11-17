using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    private List<int> possibleLinkPos = new List<int> { 0, 1, 2, 3 };
    private LinkPos prevLinkPos;
    private LinkPos dir;
    private Vector2Int prevPos;
    [SerializeField] private Transform dungeonParent;
    [SerializeField] private List<GameObject> roomPrefabs = new List<GameObject>();
    private GameObject prevRoom = null;

    private void Start()
    {
        CreateDungeon(DungeonManager.instance.nbrCriticalRooms);
    }

    public void CreateDungeon(int nbrRoom)
    {
        GameObject nodeRoom = null;
        Node thisNode = CreateNode(NodeType.START);
        InitRoom(ref nodeRoom, thisNode);
        nodeRoom.GetComponent<Room>().isStartRoom = true;


        for (int i = 1; i < nbrRoom - 1; ++i)
        {
            thisNode = CreateNode(NodeType.DEFAULT);
            InitRoom(ref nodeRoom, thisNode);
            InitDoors(nodeRoom, thisNode);
        }
        thisNode = CreateNode(NodeType.END);
        InitRoom(ref nodeRoom, thisNode);
        InitDoors(nodeRoom, thisNode);
    }

    void InitDoors(GameObject room, Node node)
    {
        List<Door> doors = new List<Door>();
        for (int i = 0; i < room.transform.GetChild(0).childCount; ++i)
        {
            if (room.transform.GetChild(0).GetChild(i).CompareTag("Door"))
            {
                doors.Add(room.transform.GetChild(0).GetChild(i).GetComponent<Door>());
                room.transform.GetChild(0).GetChild(i).GetComponent<Door>().SetOrientation();
            }
        }
        foreach (LinkPos doorPos in node.linksPosition)
        {
            switch (doorPos)
            {
                case LinkPos.UP:
                    foreach (Door curDoor in doors)
                    {
                        if (curDoor.Orientation == Utils.ORIENTATION.NORTH)
                        {
                            curDoor.SetState(Door.STATE.OPEN);
                        }
                    }
                    break;
                case LinkPos.DOWN:
                    foreach (Door curDoor in doors)
                    {
                        if (curDoor.Orientation == Utils.ORIENTATION.SOUTH)
                        {
                            curDoor.SetState(Door.STATE.OPEN);
                        }
                    }
                    break;
                case LinkPos.LEFT:
                    foreach (Door curDoor in doors)
                    {
                        if (curDoor.Orientation == Utils.ORIENTATION.WEST)
                        {
                            curDoor.SetState(Door.STATE.OPEN);
                        }
                    }
                    break;
                case LinkPos.RIGHT:
                    foreach (Door curDoor in doors)
                    {
                        if (curDoor.Orientation == Utils.ORIENTATION.EAST)
                        {
                            curDoor.SetState(Door.STATE.OPEN);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        
    }

    void InitRoom(ref GameObject room, Node node)
    {
        DungeonManager.instance.allNodes.Add(node.position, node);
        List<GameObject> possibleRooms = new List<GameObject>();
        switch (node.type)
        {
            case NodeType.START:
                foreach (GameObject thisRoom in roomPrefabs)
                {
                    if(thisRoom != prevRoom && thisRoom.GetComponent<Configuration>().type == NodeType.START)
                    {
                        possibleRooms.Add(thisRoom);
                    }
                }
                break;
            case NodeType.DEFAULT:
                foreach (GameObject thisRoom in roomPrefabs)
                {
                    if (thisRoom != prevRoom && thisRoom.GetComponent<Configuration>().type == NodeType.DEFAULT)
                    {
                        possibleRooms.Add(thisRoom);
                    }
                }
                break;
            case NodeType.END:
                foreach (GameObject thisRoom in roomPrefabs)
                {
                    if (thisRoom.GetComponent<Configuration>().type == NodeType.DEFAULT)
                    {
                        if (thisRoom.GetComponent<Configuration>().numberOfPossibleDoors == 1)
                        {
                            possibleRooms.Add(thisRoom);
                        }
                    }
                }
                break;
            default:
                break;
        }
        prevRoom = possibleRooms[Random.Range(0, possibleRooms.Count)];
        room = Instantiate(prevRoom, new Vector3(node.position.x, node.position.y, 0), Quaternion.identity, dungeonParent);
        room.GetComponent<Room>().position = node.position;

        room.GetComponent<Room>().position = node.position;
        Vector3 size = room.GetComponent<Room>().GetLocalRoomBounds().size;
        room.transform.position = new Vector3(node.position.x * size.x, node.position.y * size.y, 0);

        InitDoors(room, node);
    }

    void CreateCriticalRooms()
    {
        // add codes of CreateDungeon
    }
    
    

    void CreateAdditionalRooms()
    {
        Node currentCriticalNode = DungeonManager.instance.allNodes[Vector2Int.zero];
        int criticalNodeLeft = DungeonManager.instance.allNodes.Count-1;
        int maxNode = (int)(DungeonManager.instance.nbrCriticalRooms * DungeonManager.instance.maxSideSize);
        
        int lockLeft = DungeonManager.instance.nbrLock;
        
        while (currentCriticalNode.type != NodeType.END)
        {
            bool needKey = lockLeft >= criticalNodeLeft || Random.Range(0f,1f) > (float)lockLeft/criticalNodeLeft;
            
            prevPos = currentCriticalNode.position;
            
            
            int nodeLeft = Random.Range(1, maxNode + 1);
            for (int i = 0; i < nodeLeft; i++)
            {
                CreateNode(NodeType.DEFAULT);
            }
            currentCriticalNode = currentCriticalNode.links[0].nodes[1]; // get next critical node
        }
    }
    
    Node CreateNode(NodeType type)
    {
        Node node = null;
        int randIndex = 0;
        switch (type)
        {
            case NodeType.START:
                node = new Node(1, NodeType.START, Difficulty.EASY);
                node.position = Vector2Int.zero;
                //Link Pos
                randIndex = Random.Range(0, possibleLinkPos.Count);
                dir = (LinkPos)possibleLinkPos[randIndex];
                node.linksPosition.Add(dir);
                //next node can't have inverse link pos (can't be left if prev was right)
                RemoveInverseLinkPosFromPossibilities(randIndex);
                prevPos = node.position;
                break;
            
            case NodeType.END:
                node = new Node(1, NodeType.END);
                SetNodePosition(node);
                node.linksPosition.Add(prevLinkPos);
                break;
            
            case NodeType.DEFAULT:
                node = new Node(2, NodeType.DEFAULT);
                SetNodePosition(node);


                List<LinkPos> invalids = new List<LinkPos>();
                Node outNode;
                bool nodeExists = false;
                Vector2Int toCheck = Vector2Int.zero;
                int length = possibleLinkPos.Count;
                for (int i = 0; i < length; ++i)
                {
                    LinkPos check = (LinkPos)possibleLinkPos[i];
                    switch (check)
                    {
                        case LinkPos.UP:
                            toCheck = new Vector2Int(prevPos.x, prevPos.y + 1);
                            nodeExists = DungeonManager.instance.allNodes.TryGetValue(toCheck, out outNode);
                            if(nodeExists)
                            {
                                invalids.Add(check);
                            }
                            break;
                        case LinkPos.DOWN:
                            toCheck = new Vector2Int(prevPos.x, prevPos.y - 1);
                            nodeExists = DungeonManager.instance.allNodes.TryGetValue(toCheck, out outNode);
                            if (nodeExists)
                            {
                                invalids.Add(check);
                            }
                            break;
                        case LinkPos.LEFT:
                            toCheck = new Vector2Int(prevPos.x - 1, prevPos.y);
                            nodeExists = DungeonManager.instance.allNodes.TryGetValue(toCheck, out outNode);
                            if (nodeExists)
                            {
                                invalids.Add(check);
                            }
                            break;
                        case LinkPos.RIGHT:
                            toCheck = new Vector2Int(prevPos.x + 1, prevPos.y);
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
                possibleLinkPos.Add((int)prevLinkPos);

                node.linksPosition.Add(prevLinkPos);
                node.linksPosition.Add(dir);
                RemoveInverseLinkPosFromPossibilities(randIndex);
                prevPos = node.position;
                break;
        }
        return node;
    }
    /// <summary>
    /// Pour chaque noeud, spawner salle qui correspond. Liste de toutes les salles possibles, filtres liste selon param. 
    /// chaque salle prefab aura component configuration pour filtrer avec param rempli dedans 
    /// Porte optionnelles obligatoire
    /// </summary>
    /// <param name="firstNode"></param>
    /// 

    void RemoveInverseLinkPosFromPossibilities(int index)
    {
        if (possibleLinkPos[index] == 0 || possibleLinkPos[index] == 2)
        {
            prevLinkPos = (LinkPos)possibleLinkPos[index] + 1;
            possibleLinkPos.Remove(possibleLinkPos[index] + 1);
        }
        else
        {
            prevLinkPos = (LinkPos)possibleLinkPos[index] - 1;
            possibleLinkPos.Remove(possibleLinkPos[index] - 1);
        }
    }
    void SetNodePosition(Node currentNode)
    {
        switch (dir)
        {
            case LinkPos.UP:
                currentNode.position = new Vector2Int(prevPos.x, prevPos.y + 1);
                break;
            case LinkPos.DOWN:
                currentNode.position = new Vector2Int(prevPos.x, prevPos.y - 1);
                break;
            case LinkPos.LEFT:
                currentNode.position = new Vector2Int(prevPos.x - 1, prevPos.y);
                break;
            case LinkPos.RIGHT:
                currentNode.position = new Vector2Int(prevPos.x + 1, prevPos.y);
                break;
            default:
                break;
        }
    }
}


