using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public static DungeonGenerator instance;

    private List<int> possibleLinkPos = new List<int> { 0, 1, 2, 3 };
    private Link prevLink;
    private LinkPos dir;
    private Vector2Int prevPos;
    [SerializeField] private Transform dungeonParent;
    [SerializeField] private GameObject roomBase;
    [SerializeField] private List<GameObject> roomPrefabs = new List<GameObject>();
    private GameObject prevRoom = null;

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(instance);
        }
        instance = this;
    }

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
        DungeonManager.instance.currentNode = thisNode;
        DungeonManager.instance.allNodes.Add(thisNode.position, thisNode);

        for (int i = 1; i < nbrRoom - 1; ++i)
        {
            thisNode = CreateNode(NodeType.DEFAULT);
            InitRoom(ref nodeRoom, thisNode);
            DungeonManager.instance.allNodes.Add(thisNode.position, thisNode);
        }
        thisNode = CreateNode(NodeType.END);
        InitRoom(ref nodeRoom, thisNode);
        DungeonManager.instance.allNodes.Add(thisNode.position, thisNode);
        
        //CreateAllSidePaths();
    }
    
    Node CreateNode(NodeType type)
    {
        Node node = null;
        int randIndex = 0;
        Link newLink = null;
        switch (type)
        {
            case NodeType.START:
                node = new Node(1, NodeType.START, Difficulty.EASY);
                node.position = Vector2Int.zero;
                //Link Pos
                randIndex = Random.Range(0, possibleLinkPos.Count);
                dir = (LinkPos)possibleLinkPos[randIndex];
                newLink = new Link(dir, node);
                node.links.Add(newLink);
                //next node can't have inverse link pos (can't be left if prev was right)
                RemoveInverseLinkPosFromPossibilities(node);
                prevPos = node.position;
                break;
            
            case NodeType.END:
                node = new Node(1, NodeType.END);
                SetNodePosition(node);
                newLink = new Link(prevLink.position, node);
                newLink.nodes[1] = prevLink.nodes[0];
                node.links.Add(newLink);
                break;
            
            case NodeType.DEFAULT:
                node = new Node(2, NodeType.DEFAULT);
                SetNodePosition(node);
                prevPos = node.position;

                bool nodeIsValid = CheckAreaBeforeSettingLinkPos();

                if (prevLink.nodes[0].type == NodeType.START)
                {
                    DungeonManager.instance.allNodes[Vector2Int.zero].links[0].nodes[1] = node;
                }
                prevLink.nodes[1] = node;
                newLink = new Link(prevLink.position, node);
                newLink.nodes[1] = prevLink.nodes[0];
                node.links.Add(newLink);
                newLink = new Link(dir, node);
                node.links.Add(newLink);

                RemoveInverseLinkPosFromPossibilities(node);
                if (!nodeIsValid)
                {
                    node = null;
                }
                break;
        }
        return node;
    }

    //new version
    #region newVersion
    void CreateDungeonBis(int nbrRoom)
    {
        CreateCriticalPath(nbrRoom);
        CreateAllSidePathBis();
        
        // GameObject nodeRoom = null;
        // Node thisNode = CreateNode(NodeType.START);
        // InitRoom(ref nodeRoom, thisNode);
        // nodeRoom.GetComponent<Room>().isStartRoom = true;
        // DungeonManager.instance.currentNode = thisNode;
        // DungeonManager.instance.allNodes.Add(thisNode.position, thisNode);
        //
        // for (int i = 1; i < nbrRoom - 1; ++i)
        // {
        //     thisNode = CreateNode(NodeType.DEFAULT);
        //     InitRoom(ref nodeRoom, thisNode);
        //     DungeonManager.instance.allNodes.Add(thisNode.position, thisNode);
        // }
        // thisNode = CreateNode(NodeType.END);
        // InitRoom(ref nodeRoom, thisNode);
        // DungeonManager.instance.allNodes.Add(thisNode.position, thisNode);
        //
        // CreateAllSidePaths();
    }
    
    void CreateCriticalPath(int nbrRoom)
    {
        Node lastNode = CreateNodeBis(NodeType.START, null);
        DungeonManager.instance.allNodes.Add(Vector2Int.zero, lastNode);

        for (int i = 1; i < nbrRoom - 1; ++i)
        {
            lastNode = CreateNodeBis(NodeType.DEFAULT, lastNode);
            if (lastNode == null)
                ReGenerateDungeon(new string("Attempt to create a critical node failed"));
            
            DungeonManager.instance.allNodes.Add(lastNode.position, lastNode);
        }
        
        lastNode = CreateNodeBis(NodeType.END, lastNode);
        DungeonManager.instance.allNodes.Add(lastNode.position, lastNode);
    }
    
    void CreateAllSidePathBis()
    {
        Node currentCriticalNode = DungeonManager.instance.allNodes[Vector2Int.zero].links[0].nodes[1];
        int criticalNodeLeft = DungeonManager.instance.allNodes.Count-1;
        int lockLeft = DungeonManager.instance.nbrLock;
        int maxNode = (int)(DungeonManager.instance.nbrCriticalRooms * DungeonManager.instance.maxSideSize);
        
        while (currentCriticalNode.type != NodeType.END)
        {
            prevPos = currentCriticalNode.position;

            CheckAndUpdateAvailableLinkList(currentCriticalNode);
            if (currentCriticalNode.freeLinks.Count == 0)
            {
                currentCriticalNode = currentCriticalNode.links[1].nodes[1]; // get next critical node
                continue;
            }
            
            bool needKey = lockLeft >= criticalNodeLeft || Random.Range(0f,1f) >= (float)lockLeft/criticalNodeLeft;
            
            if (needKey || (Random.Range(0f, 1f) < DungeonManager.instance.coefSidePath))
            {
                Node lastNode = CreateSidePath(Random.Range(1, maxNode + 1), currentCriticalNode);
                if (needKey)
                {
                    lastNode.haveKey = true;
                    currentCriticalNode.links[1].hasLock = true;
                }
            }
            currentCriticalNode = currentCriticalNode.links[1].nodes[1];
            Debug.Log("New path created");
        }
    }
    
    Node CreateNodeBis(NodeType type, Node nodeFrom)
    {
        Node newNode = null;
        int randIndex;

        if (type == NodeType.START)
        {
            newNode = new Node(Vector2Int.zero, NodeType.START, Difficulty.EASY);
            randIndex = Random.Range(0, newNode.freeLinks.Count);
            dir = (LinkPos)newNode.freeLinks[randIndex];
            prevLink = newNode.AddNewLink(dir);
        }

        else
        {
            CheckAndUpdateAvailableLinkList(nodeFrom);
            int nbrFreeLinks = nodeFrom.freeLinks.Count;
            if (nbrFreeLinks == 0)
                return null;
            
            randIndex = Random.Range(0, nbrFreeLinks);
            Vector2Int newNodePos = GetPosOfNextNode(nodeFrom.position, nodeFrom.freeLinks[randIndex]);
            newNode = new Node(newNodePos);
            prevLink.nodes[1] = newNode;
            dir = (LinkPos)newNode.freeLinks[randIndex];
            prevLink = newNode.AddNewLink(dir);
            
            //prevPos = newNodePos;
        }
        return newNode;
    }

    // Equivalent of "CheckAreaBeforeSettingLinkPos()"
    // It update the freeLinks list of the node
    void CheckAndUpdateAvailableLinkList(Node node)
    {
        Vector2Int toCheck = Vector2Int.zero;
        Node sideNode = null;
        bool nodeExists = false;
        
        foreach (int freeLink in node.freeLinks)
        {
            switch (freeLink)
            {
                case 0: //UP
                    toCheck = new Vector2Int(prevPos.x, prevPos.y + 1);
                    nodeExists = DungeonManager.instance.allNodes.TryGetValue(toCheck, out sideNode);
                    break;
                case 1: // DOWN
                    toCheck = new Vector2Int(prevPos.x, prevPos.y - 1);
                    nodeExists = DungeonManager.instance.allNodes.TryGetValue(toCheck, out sideNode);
                    break;
                case 2: // LEFT
                    toCheck = new Vector2Int(prevPos.x - 1, prevPos.y);
                    nodeExists = DungeonManager.instance.allNodes.TryGetValue(toCheck, out sideNode);
                    break;
                case 3: // RIGHT
                    toCheck = new Vector2Int(prevPos.x + 1, prevPos.y);
                    nodeExists = DungeonManager.instance.allNodes.TryGetValue(toCheck, out sideNode);
                    break;
            }

            if (nodeExists)
            {
                node.freeLinks.Remove(freeLink);
                sideNode.freeLinks.Remove(GetMirrorPos(freeLink));
            }
            nodeExists = false;
        }
    }
      
    public int GetMirrorPos(int fromPos)
    {
        if (fromPos % 2 == 0)
            return fromPos + 1;

        return fromPos - 1;
    }
    
    Node CreateSidePath(int nodesToCreate, Node lastNode)
    {
        Node tmpNode = lastNode;
        while (nodesToCreate > 0 && tmpNode != null)
        {
            tmpNode = CreateNodeBis(NodeType.DEFAULT, lastNode);
            lastNode = tmpNode ?? lastNode;
            nodesToCreate--;
        }
        return lastNode;
    }
    
    Vector2Int GetPosOfNextNode(Vector2Int nodePos, int dir)
    {
        switch ((LinkPos)dir)
        {
            case LinkPos.UP:
                nodePos.y++;
                break;
            case LinkPos.DOWN:
                nodePos.y--;
                break;
            case LinkPos.LEFT:
                nodePos.x--;
                break;
            case LinkPos.RIGHT:
                nodePos.x++;
                break;
        }
        return nodePos;
    }
    #endregion // END OF REGION

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
        foreach (Link doorPos in node.links)
        {
            switch (doorPos.position)
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
        //List<GameObject> possibleRooms = new List<GameObject>();
        //switch (node.type)
        //{
        //    case NodeType.START:
        //        foreach (GameObject thisRoom in roomPrefabs)
        //        {
        //            if(thisRoom != prevRoom && thisRoom.GetComponent<Configuration>().type == NodeType.START)
        //            {
        //                possibleRooms.Add(thisRoom);
        //            }
        //        }
        //        break;
        //    case NodeType.DEFAULT:
        //        foreach (GameObject thisRoom in roomPrefabs)
        //        {
        //            if (thisRoom != prevRoom && thisRoom.GetComponent<Configuration>().type == NodeType.DEFAULT)
        //            {
        //                possibleRooms.Add(thisRoom);
        //            }
        //        }
        //        break;
        //    case NodeType.END:
        //        foreach (GameObject thisRoom in roomPrefabs)
        //        {
        //            if (thisRoom.GetComponent<Configuration>().type == NodeType.DEFAULT)
        //            {
        //                if (thisRoom.GetComponent<Configuration>().numberOfPossibleDoors == 1)
        //                {
        //                    possibleRooms.Add(thisRoom);
        //                }
        //            }
        //        }
        //        break;
        //    default:
        //        break;
        //}
        //prevRoom = possibleRooms[Random.Range(0, possibleRooms.Count)];
        
        room = Instantiate(roomBase, new Vector3(node.position.x, node.position.y, 0), Quaternion.identity, dungeonParent);
        room.GetComponent<Room>().position = node.position;

        room.GetComponent<Room>().position = node.position;
        Vector3 size = room.GetComponent<Room>().GetLocalRoomBounds().size;
        room.transform.position = new Vector3(node.position.x * size.x, node.position.y * size.y, 0);

        InitDoors(room, node);
    }

    public void ChooseThreeRooms(ref GameObject[] roomsSelected, Node node)
    {
        for (int i = 0; i < roomsSelected.Length; ++i)
        {
            roomsSelected[i] = null;
        }
        List<GameObject> possibleRooms = new List<GameObject>();
        switch (node.type)
        {
            case NodeType.START:
                foreach (GameObject thisRoom in roomPrefabs)
                {
                    if (thisRoom != prevRoom && thisRoom.GetComponent<Configuration>().type == NodeType.START)
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
        for (int i = 0; i < roomsSelected.Length; ++i)
        {
            if (possibleRooms.Count > 0) {
                roomsSelected[i] = possibleRooms[Random.Range(0, possibleRooms.Count)];
                possibleRooms.Remove(roomsSelected[i]);
            }
        }

        //GameObject room = Instantiate(prevRoom, new Vector3(node.position.x, node.position.y, 0), Quaternion.identity, dungeonParent);
        //room.GetComponent<Room>().position = node.position;

        //Vector3 size = room.GetComponent<Room>().GetLocalRoomBounds().size;
        //room.transform.position = new Vector3(node.position.x * size.x, node.position.y * size.y, 0);

        //InitDoors(room, node);
    }

    // void CreateAllSidePaths()
    // {
    //     Node currentCriticalNode = DungeonManager.instance.allNodes[Vector2Int.zero].links[0].nodes[1];
    //     int criticalNodeLeft = DungeonManager.instance.allNodes.Count-1;
    //     int lockLeft = DungeonManager.instance.nbrLock;
    //     int maxNode = (int)(DungeonManager.instance.nbrCriticalRooms * DungeonManager.instance.maxSideSize);
    //     
    //     
    //     bool canAddNewPath;
    //     while (currentCriticalNode.type != NodeType.END)
    //     {
    //         prevPos = currentCriticalNode.position;
    //
    //         canAddNewPath = CheckAreaBeforeSettingLinkPos();
    //         if (!canAddNewPath)
    //         {
    //             currentCriticalNode = currentCriticalNode.links[1].nodes[1]; // get next critical node
    //             continue;
    //         }
    //         
    //         bool needKey = lockLeft >= criticalNodeLeft || Random.Range(0f,1f) >= (float)lockLeft/criticalNodeLeft;
    //         
    //         if (needKey || (Random.Range(0f, 1f) < DungeonManager.instance.coefSidePath))
    //         {
    //             Node lastNode = CreateSidePath(Random.Range(1, maxNode + 1), needKey);
    //             if (needKey)
    //             {
    //                 lastNode.haveKey = true;
    //                 currentCriticalNode.links[0].hasLock = true;
    //             }
    //         }
    //         currentCriticalNode = currentCriticalNode.links[1].nodes[1];
    //         Debug.Log("New path created");
    //     }
    // }
    
    /// <summary>
    /// Pour chaque noeud, spawner salle qui correspond. Liste de toutes les salles possibles, filtres liste selon param. 
    /// chaque salle prefab aura component configuration pour filtrer avec param rempli dedans 
    /// Porte optionnelles obligatoire
    /// </summary>
    /// <param name="firstNode"></param>
    /// 
    bool CheckAreaBeforeSettingLinkPos()
    {
        int rand = 0;
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
                    if (nodeExists)
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
            }
        }

        for (int i = 0; i < invalids.Count; ++i)
        {
            possibleLinkPos.Remove((int)invalids[i]);
        }
        rand = Random.Range(0, possibleLinkPos.Count);
        if(possibleLinkPos.Count == 0)
        {
            print("error no place");
            return false;
        }
        dir = (LinkPos)possibleLinkPos[rand];

        for (int i = 0; i < invalids.Count; ++i)
        {
            possibleLinkPos.Add((int)invalids[i]);
        }
        possibleLinkPos.Add((int)prevLink.position);
        return true;
    }

    void RemoveInverseLinkPosFromPossibilities(Node node)
    {
        LinkPos invalidPos;
        if ((int)dir == 0 || (int)dir == 2)
        {
            invalidPos = (LinkPos)((int)dir + 1);
            prevLink = new Link(invalidPos, node);
            possibleLinkPos.Remove((int)invalidPos);
        }
        else
        {
            invalidPos = (LinkPos)((int)dir - 1);
            prevLink = new Link(invalidPos, node);
            possibleLinkPos.Remove((int)invalidPos);
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

    

    void ReGenerateDungeon(string error)
    {
        Debug.LogWarning("Dungeon creation error : " + error + "\n Dungeon erasing!");
        
        DungeonManager.instance.allNodes.Clear();
        CreateDungeon(DungeonManager.instance.nbrCriticalRooms);
    }
}


