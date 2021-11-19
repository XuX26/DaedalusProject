using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using Random = UnityEngine.Random;

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
    public Room currentRoom;

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
        //CreateDungeon(DungeonManager.instance.nbrCriticalRooms);

        bool dungeonCreated = false;
        while (dungeonCreated == false)
        {
            try
            {
                CreateDungeonBis(DungeonManager.instance.nbrCriticalRooms);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                DungeonManager.instance.allNodes.Clear();
                continue;
            }
            dungeonCreated = true;
        }
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

    // -------- NEW VERSION REGION --------
    #region newVersion 
    void CreateDungeonBis(int nbrRoom)
    {
        CreateCriticalPath(nbrRoom);
        CreateAllSidePathBis();
        InitRooms();
    }
    
    void CreateCriticalPath(int nbrRoom)
    {
        //Node lastNode = CreateNodeBis(NodeType.START, null);
        Node lastNode = new Node(Vector2Int.zero, NodeType.START);
        DungeonManager.instance.allNodes.Add(Vector2Int.zero, lastNode);

        for (int i = 1; i < nbrRoom - 1; ++i)
        {
            lastNode = CreateNodeBis(NodeType.DEFAULT, lastNode);
            if (lastNode == null)
                throw new Exception(message: "Attempt to create a critical node FAILED (freeLinks.Count was certainly == 0)");
        }

        if (CreateNodeBis(NodeType.END, lastNode) == null)
            throw new Exception(message: "Attempt to create the last critical Node FAILED (freeLinks.Count was certainly == 0)");
    }
    
    void CreateAllSidePathBis()
    {
        Node currentCriticalNode = DungeonManager.instance.allNodes[Vector2Int.zero].links[0].nodes[1];
        int criticalNodeLeft = DungeonManager.instance.allNodes.Count-2;
        int lockLeft = DungeonManager.instance.nbrLock;
        bool secretRoomCreated = false;
        int maxNode = (int)(DungeonManager.instance.nbrCriticalRooms * DungeonManager.instance.maxSideSizeCoef);
        int minNode = DungeonManager.instance.minSideSize;
        
        //while (currentCriticalNode.type != NodeType.END)
        while (criticalNodeLeft > 0)
        {
            prevPos = currentCriticalNode.position; // still used ?

            CheckAndUpdateAvailableLinkList(currentCriticalNode);
            if (currentCriticalNode.freeLinks.Count == 0)
            {
                currentCriticalNode = currentCriticalNode.links[1].nodes[1]; // get next critical node
                criticalNodeLeft--; // to del
                continue;
            }
            
            bool needKey = lockLeft > 0 && (lockLeft >= criticalNodeLeft || Random.Range(0f,1f) <= (float)lockLeft/criticalNodeLeft);
            
            if (needKey || (Random.Range(0f, 1f) < DungeonManager.instance.coefSidePath))
            {
                Node lastNode = CreateSidePath(Random.Range(minNode, maxNode + 1), currentCriticalNode);
                Debug.Log("New Side path created");
                if (needKey)
                {
                    lastNode.haveKey = true;
                    currentCriticalNode.links[1].hasLock = true;
                    
                    lockLeft--;
                    Debug.Log("Key added");
                }
            }

            if (lockLeft <= 1 && secretRoomCreated == false && currentCriticalNode.freeLinks.Count > 0) // need to recheck
            {
                Node newNode = CreateNodeBis(NodeType.SECRET, currentCriticalNode);
                secretRoomCreated = newNode != null;
                Debug.Log("Attempt to create Secret Room | result : = " + secretRoomCreated);
            }
            
            currentCriticalNode = currentCriticalNode.links[1].nodes[1];
            criticalNodeLeft--; //to del
            if (currentCriticalNode.type == NodeType.END)
                Debug.Log("Is the last Node, criticalNodeLeft = " + criticalNodeLeft);
        }
        
        if(lockLeft > 0 || secretRoomCreated == false)
            throw new Exception(message: "SidePath creation didn't succeed");

    }

    void InitRooms()
    {
        GameObject nodeRoom = null;

        foreach (var node in DungeonManager.instance.allNodes)
        {
            InitRoom(ref nodeRoom,  DungeonManager.instance.allNodes[node.Key]);
            if (node.Key == Vector2Int.zero)
                nodeRoom.GetComponent<Room>().isStartRoom = true;
        }
    }
    
    // /!\ This method is not called to create the first node
    Node CreateNodeBis(NodeType type, Node nodeFrom)
    {
        Node newNode = null;

        CheckAndUpdateAvailableLinkList(nodeFrom);
        int nbrFreeLinks = nodeFrom.freeLinks.Count;
        if (nbrFreeLinks == 0)
            return null;
            
        int randIndex;
        randIndex = Random.Range(0, nbrFreeLinks);
        dir = (LinkPos)nodeFrom.freeLinks[randIndex];
        Vector2Int newNodePos = GetPosOfNextNode(nodeFrom.position, (int)dir);
        newNode = new Node(newNodePos, type);
        LinkTwoNode(nodeFrom, newNode, dir, type == NodeType.SECRET);
        newNode.type = type;
            
        DungeonManager.instance.allNodes.Add(newNode.position, newNode);
        prevPos = newNodePos; // not even used ?
        return newNode;
    }

    // Add Create links for both nodes
    void LinkTwoNode(Node from, Node next, LinkPos dir, bool isNewSecretRoom = false)
    {
        if (isNewSecretRoom)
        {
            DungeonManager.instance.secretLink = from.AddNewLink(next, dir);
            DungeonManager.instance.secretLink.isSecret = true;
        }
        else
            from.AddNewLink(next, dir);

        next.AddNewLink(from, (LinkPos)GetMirrorPos((int)dir));
    }

    // Equivalent of "CheckAreaBeforeSettingLinkPos()"
    // It update the freeLinks list of the node
    void CheckAndUpdateAvailableLinkList(Node node)
    {
        Vector2Int toCheck = node.position;
        Node sideNode = null;
        bool nodeExists = false;

        int[] tmpFreeLink = node.freeLinks.ToArray();
        foreach (int freeLink in tmpFreeLink)
        {
            switch (freeLink)
            {
                case 0: //UP
                    toCheck.y++;
                    nodeExists = DungeonManager.instance.allNodes.TryGetValue(toCheck, out sideNode);
                    break;
                case 1: // DOWN
                    toCheck.y--;
                    nodeExists = DungeonManager.instance.allNodes.TryGetValue(toCheck, out sideNode);
                    break;
                case 2: // LEFT
                    toCheck.x--;
                    nodeExists = DungeonManager.instance.allNodes.TryGetValue(toCheck, out sideNode);
                    break;
                case 3: // RIGHT
                    toCheck.x++;
                    nodeExists = DungeonManager.instance.allNodes.TryGetValue(toCheck, out sideNode);
                    break;
            }

            if (nodeExists)
            {
                node.freeLinks.Remove(freeLink);
                sideNode.freeLinks.Remove(GetMirrorPos(freeLink)); // remove the freelink also in the next node
            }
            nodeExists = false;
            sideNode = null;
            toCheck = node.position;
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
    #endregion 
    // END OF REGION
    
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

        Door.STATE doorState;
        foreach (Link doorPos in node.links)
        {
            switch (doorPos.position)
            {
                case LinkPos.UP:
                    foreach (Door curDoor in doors)
                    {
                        if (curDoor.Orientation == Utils.ORIENTATION.NORTH)
                        {
                            doorState = doorPos.hasLock ? Door.STATE.CLOSED : Door.STATE.OPEN;
                            curDoor.SetState(doorPos.isSecret ? Door.STATE.SECRET : doorState);
                        }
                    }
                    break;
                case LinkPos.DOWN:
                    foreach (Door curDoor in doors)
                    {
                        if (curDoor.Orientation == Utils.ORIENTATION.SOUTH)
                        {
                            doorState = doorPos.hasLock ? Door.STATE.CLOSED : Door.STATE.OPEN;
                            curDoor.SetState(doorPos.isSecret ? Door.STATE.SECRET : doorState);
                        }
                    }
                    break;
                case LinkPos.LEFT:
                    foreach (Door curDoor in doors)
                    {
                        if (curDoor.Orientation == Utils.ORIENTATION.WEST)
                        {
                            doorState = doorPos.hasLock ? Door.STATE.CLOSED : Door.STATE.OPEN;
                            curDoor.SetState(doorPos.isSecret ? Door.STATE.SECRET : doorState);
                        }
                    }
                    break;
                case LinkPos.RIGHT:
                    foreach (Door curDoor in doors)
                    {
                        if (curDoor.Orientation == Utils.ORIENTATION.EAST)
                        {
                            doorState = doorPos.hasLock ? Door.STATE.CLOSED : Door.STATE.OPEN;
                            curDoor.SetState(doorPos.isSecret ? Door.STATE.SECRET : doorState);
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
        room = Instantiate(roomBase, new Vector3(node.position.x, node.position.y, 0), Quaternion.identity, dungeonParent);
        room.GetComponent<Room>().position = node.position;

        room.GetComponent<Room>().position = node.position;
        Vector3 size = room.GetComponent<Room>().GetLocalRoomBounds().size;
        room.transform.position = new Vector3(node.position.x * size.x, node.position.y * size.y, 0);

        room.GetComponent<Configuration>().type = node.type;

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
                        if (thisRoom.GetComponent<Configuration>().isFlexible)
                        {
                            possibleRooms.Add(thisRoom);
                        }
                        else
                        {
                            if(node.links.Count == thisRoom.GetComponent<Configuration>().numberOfPossibleDoors)
                            {
                                Dictionary<LinkPos, bool> doorsToCheck = new Dictionary<LinkPos, bool>();

                                for (int i = 0; i < node.links.Count; i++)
                                {
                                    doorsToCheck.Add(node.links[i].position, false);
                                }

                                for (int i = 0; i < thisRoom.transform.GetChild(0).childCount; i++)
                                {
                                    if (thisRoom.transform.GetChild(0).GetChild(i).CompareTag("Door"))
                                    {
                                        bool hasGoodDoor = false;
                                        bool outcome = false;
                                        bool shouldStop = false;
                                        Utils.ORIENTATION doorOrient = Utils.ORIENTATION.NONE;

                                        if (thisRoom.transform.GetChild(0).GetChild(i).position.x >= 10)
                                        {
                                            doorOrient = Utils.ORIENTATION.EAST;
                                        }
                                        else if (thisRoom.transform.GetChild(0).GetChild(i).position.x <= 1)
                                        {
                                            doorOrient = Utils.ORIENTATION.WEST;
                                        }
                                        else if (thisRoom.transform.GetChild(0).GetChild(i).position.y >= 5)
                                        {
                                            doorOrient = Utils.ORIENTATION.NORTH;
                                        }
                                        else
                                        {
                                            doorOrient = Utils.ORIENTATION.SOUTH;
                                        }


                                        switch (doorOrient)
                                        {
                                            case Utils.ORIENTATION.NONE:
                                                shouldStop = true;
                                                break;
                                            case Utils.ORIENTATION.NORTH:
                                                hasGoodDoor = doorsToCheck.TryGetValue(LinkPos.UP, out outcome);
                                                if (!hasGoodDoor)
                                                {
                                                    shouldStop = true;
                                                }
                                                else
                                                {
                                                    doorsToCheck[LinkPos.UP] = true;
                                                }
                                                break;
                                            case Utils.ORIENTATION.EAST:
                                                hasGoodDoor = doorsToCheck.TryGetValue(LinkPos.RIGHT, out outcome);
                                                if (!hasGoodDoor)
                                                {
                                                    shouldStop = true;
                                                }
                                                else
                                                {
                                                    doorsToCheck[LinkPos.UP] = true;
                                                }
                                                break;
                                            case Utils.ORIENTATION.SOUTH:
                                                hasGoodDoor = doorsToCheck.TryGetValue(LinkPos.DOWN, out outcome);
                                                if (!hasGoodDoor)
                                                {
                                                    shouldStop = true;
                                                }
                                                else
                                                {
                                                    doorsToCheck[LinkPos.UP] = true;
                                                }
                                                break;
                                            case Utils.ORIENTATION.WEST:
                                                hasGoodDoor = doorsToCheck.TryGetValue(LinkPos.LEFT, out outcome);
                                                if (!hasGoodDoor)
                                                {
                                                    shouldStop = true;
                                                }
                                                else
                                                {
                                                    doorsToCheck[LinkPos.UP] = true;
                                                }
                                                break;
                                            default:
                                                break;
                                        }
                                        if (shouldStop)
                                        {
                                            break;
                                        }
                                        foreach (bool hasDoor in doorsToCheck.Values)
                                        {
                                            if (!hasDoor)
                                            {
                                                break;
                                            }
                                        }
                                        possibleRooms.Add(thisRoom);
                                    }
                                }
                            }
                        }
                    }
                }
                break;
            case NodeType.DEFAULT:
                foreach (GameObject thisRoom in roomPrefabs)
                {
                    if (thisRoom != prevRoom && thisRoom.GetComponent<Configuration>().type == NodeType.DEFAULT)
                    {
                        if (thisRoom.GetComponent<Configuration>().isFlexible)
                        {
                            possibleRooms.Add(thisRoom);
                        }
                        else
                        {
                            if (node.links.Count == thisRoom.GetComponent<Configuration>().numberOfPossibleDoors)
                            {
                                Dictionary<LinkPos, bool> doorsToCheck = new Dictionary<LinkPos, bool>();

                                for (int i = 0; i < node.links.Count; i++)
                                {
                                    doorsToCheck.Add(node.links[i].position, false);
                                }

                                for (int i = 0; i < thisRoom.transform.GetChild(0).childCount; i++)
                                {
                                    if (thisRoom.transform.GetChild(0).GetChild(i).CompareTag("Door"))
                                    {
                                        bool hasGoodDoor = false;
                                        bool outcome = false;
                                        bool shouldStop = false;
                                        Utils.ORIENTATION doorOrient = Utils.ORIENTATION.NONE;

                                        if (thisRoom.transform.GetChild(0).GetChild(i).position.x >= 10)
                                        {
                                            doorOrient = Utils.ORIENTATION.EAST;
                                        }
                                        else if (thisRoom.transform.GetChild(0).GetChild(i).position.x <= 1)
                                        {
                                            doorOrient = Utils.ORIENTATION.WEST;
                                        }
                                        else if (thisRoom.transform.GetChild(0).GetChild(i).position.y >= 5)
                                        {
                                            doorOrient = Utils.ORIENTATION.NORTH;
                                        }
                                        else
                                        {
                                            doorOrient = Utils.ORIENTATION.SOUTH;
                                        }


                                        switch (doorOrient)
                                        {
                                            case Utils.ORIENTATION.NONE:
                                                shouldStop = true;
                                                break;
                                            case Utils.ORIENTATION.NORTH:
                                                hasGoodDoor = doorsToCheck.TryGetValue(LinkPos.UP, out outcome);
                                                if (!hasGoodDoor)
                                                {
                                                    shouldStop = true;
                                                }
                                                else
                                                {
                                                    doorsToCheck[LinkPos.UP] = true;
                                                }
                                                break;
                                            case Utils.ORIENTATION.EAST:
                                                hasGoodDoor = doorsToCheck.TryGetValue(LinkPos.RIGHT, out outcome);
                                                if (!hasGoodDoor)
                                                {
                                                    shouldStop = true;
                                                }
                                                else
                                                {
                                                    doorsToCheck[LinkPos.UP] = true;
                                                }
                                                break;
                                            case Utils.ORIENTATION.SOUTH:
                                                hasGoodDoor = doorsToCheck.TryGetValue(LinkPos.DOWN, out outcome);
                                                if (!hasGoodDoor)
                                                {
                                                    shouldStop = true;
                                                }
                                                else
                                                {
                                                    doorsToCheck[LinkPos.UP] = true;
                                                }
                                                break;
                                            case Utils.ORIENTATION.WEST:
                                                hasGoodDoor = doorsToCheck.TryGetValue(LinkPos.LEFT, out outcome);
                                                if (!hasGoodDoor)
                                                {
                                                    shouldStop = true;
                                                }
                                                else
                                                {
                                                    doorsToCheck[LinkPos.UP] = true;
                                                }
                                                break;
                                            default:
                                                break;
                                        }
                                        if (shouldStop)
                                        {
                                            break;
                                        }
                                        foreach (bool hasDoor in doorsToCheck.Values)
                                        {
                                            if (!hasDoor)
                                            {
                                                break;
                                            }
                                        }
                                        possibleRooms.Add(thisRoom);
                                    }
                                }
                            }
                        }
                    }
                }
                break;
            case NodeType.END:
                foreach (GameObject thisRoom in roomPrefabs)
                {
                    if (thisRoom.GetComponent<Configuration>().type == NodeType.END)
                    {
                        if (thisRoom.GetComponent<Configuration>().isFlexible)
                        {
                            possibleRooms.Add(thisRoom);
                        }
                        else
                        {
                            if (node.links.Count == thisRoom.GetComponent<Configuration>().numberOfPossibleDoors)
                            {
                                Dictionary<LinkPos, bool> doorsToCheck = new Dictionary<LinkPos, bool>();

                                for (int i = 0; i < node.links.Count; i++)
                                {
                                    doorsToCheck.Add(node.links[i].position, false);
                                }

                                for (int i = 0; i < thisRoom.transform.GetChild(0).childCount; i++)
                                {
                                    if (thisRoom.transform.GetChild(0).GetChild(i).CompareTag("Door"))
                                    {
                                        bool hasGoodDoor = false;
                                        bool outcome = false;
                                        bool shouldStop = false;
                                        Utils.ORIENTATION doorOrient = Utils.ORIENTATION.NONE;

                                        if (thisRoom.transform.GetChild(0).GetChild(i).position.x >= 10)
                                        {
                                            doorOrient = Utils.ORIENTATION.EAST;
                                        }
                                        else if (thisRoom.transform.GetChild(0).GetChild(i).position.x <= 1)
                                        {
                                            doorOrient = Utils.ORIENTATION.WEST;
                                        }
                                        else if (thisRoom.transform.GetChild(0).GetChild(i).position.y >= 5)
                                        {
                                            doorOrient = Utils.ORIENTATION.NORTH;
                                        }
                                        else
                                        {
                                            doorOrient = Utils.ORIENTATION.SOUTH;
                                        }


                                        switch (doorOrient)
                                        {
                                            case Utils.ORIENTATION.NONE:
                                                shouldStop = true;
                                                break;
                                            case Utils.ORIENTATION.NORTH:
                                                hasGoodDoor = doorsToCheck.TryGetValue(LinkPos.UP, out outcome);
                                                if (!hasGoodDoor)
                                                {
                                                    shouldStop = true;
                                                }
                                                else
                                                {
                                                    doorsToCheck[LinkPos.UP] = true;
                                                }
                                                break;
                                            case Utils.ORIENTATION.EAST:
                                                hasGoodDoor = doorsToCheck.TryGetValue(LinkPos.RIGHT, out outcome);
                                                if (!hasGoodDoor)
                                                {
                                                    shouldStop = true;
                                                }
                                                else
                                                {
                                                    doorsToCheck[LinkPos.UP] = true;
                                                }
                                                break;
                                            case Utils.ORIENTATION.SOUTH:
                                                hasGoodDoor = doorsToCheck.TryGetValue(LinkPos.DOWN, out outcome);
                                                if (!hasGoodDoor)
                                                {
                                                    shouldStop = true;
                                                }
                                                else
                                                {
                                                    doorsToCheck[LinkPos.UP] = true;
                                                }
                                                break;
                                            case Utils.ORIENTATION.WEST:
                                                hasGoodDoor = doorsToCheck.TryGetValue(LinkPos.LEFT, out outcome);
                                                if (!hasGoodDoor)
                                                {
                                                    shouldStop = true;
                                                }
                                                else
                                                {
                                                    doorsToCheck[LinkPos.UP] = true;
                                                }
                                                break;
                                            default:
                                                break;
                                        }
                                        if (shouldStop)
                                        {
                                            break;
                                        }
                                        foreach (bool hasDoor in doorsToCheck.Values)
                                        {
                                            if (!hasDoor)
                                            {
                                                break;
                                            }
                                        }
                                        possibleRooms.Add(thisRoom);
                                    }
                                }
                            }
                        }
                    }
                }
                break;
            default:
                break;
        }
        for (int i = 0; i < roomsSelected.Length; ++i)
        {
            if (possibleRooms.Count > 0) {
                roomsSelected[i] = possibleRooms[Random.Range(0, possibleRooms.Count)];
                possibleRooms.Remove(roomsSelected[i]);
            }
        }
    }

    public void AssignRoom(GameObject chosenRoom)
    {
        prevRoom = chosenRoom;
        currentRoom.name = chosenRoom.name;
        for (int i = 0; i < chosenRoom.transform.GetChild(0).childCount; ++i)
        {
            if (!chosenRoom.transform.GetChild(0).GetChild(i).CompareTag("Door"))
            {
                Instantiate(chosenRoom.transform.GetChild(0).GetChild(i), currentRoom.transform.GetChild(0));
            }
        }
        for (int i = 0; i < chosenRoom.transform.GetChild(1).childCount; ++i)
        {
            Instantiate(chosenRoom.transform.GetChild(1).GetChild(i), currentRoom.transform.GetChild(1));
        }
        currentRoom.GetComponent<Configuration>().diffucultyLevel = chosenRoom.GetComponent<Configuration>().diffucultyLevel;

        if(currentRoom.GetComponent<Configuration>().type == NodeType.END)
        {
            int freeLinksCount = DungeonManager.instance.allNodes[currentRoom.GetComponent<Room>().position].freeLinks.Count;
            LinkPos endDoor = (LinkPos)DungeonManager.instance.allNodes[currentRoom.GetComponent<Room>().position].freeLinks[Random.Range(0, freeLinksCount)];

            switch (endDoor)
            {
                case LinkPos.UP:
                    foreach (Transform child in currentRoom.transform.GetChild(0))
                    {
                        if (child.CompareTag("Door"))
                        {
                            if(child.GetComponent<Door>().Orientation == Utils.ORIENTATION.NORTH)
                            {
                                child.GetComponent<Door>().SetState(Door.STATE.END);
                            }
                        }
                    }
                    break;
                case LinkPos.DOWN:
                    foreach (Transform child in currentRoom.transform.GetChild(0))
                    {
                        if (child.CompareTag("Door"))
                        {
                            if (child.GetComponent<Door>().Orientation == Utils.ORIENTATION.SOUTH)
                            {
                                child.GetComponent<Door>().SetState(Door.STATE.END);
                            }
                        }
                    }
                    break;
                case LinkPos.LEFT:
                    foreach (Transform child in currentRoom.transform.GetChild(0))
                    {
                        if (child.CompareTag("Door"))
                        {
                            if (child.GetComponent<Door>().Orientation == Utils.ORIENTATION.WEST)
                            {
                                child.GetComponent<Door>().SetState(Door.STATE.END);
                            }
                        }
                    }
                    break;
                case LinkPos.RIGHT:
                    foreach (Transform child in currentRoom.transform.GetChild(0))
                    {
                        if (child.CompareTag("Door"))
                        {
                            if (child.GetComponent<Door>().Orientation == Utils.ORIENTATION.EAST)
                            {
                                child.GetComponent<Door>().SetState(Door.STATE.END);
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        if (Enemy.allEnemies.Count > 0)
        {
            foreach (Transform child in currentRoom.transform.GetChild(0))
            {
                if (child.CompareTag("Door"))
                {
                    child.GetComponent<Door>().previousState = child.GetComponent<Door>().State;
                    child.GetComponent<Door>().SetState(Door.STATE.WALL);
                }
            }
        }
    }

    public void ReturnDoorToState()
    {
        foreach (Transform child in currentRoom.transform.GetChild(0))
        {
            if (child.CompareTag("Door"))
            {
                child.GetComponent<Door>().SetState(child.GetComponent<Door>().previousState);
            }
        }
    }
    
    Node CreateNode(NodeType type)
    {
        Node node = null;
        int randIndex = 0;
        Link newLink = null;
        
        switch (type)
        {
            case NodeType.START:
                node = new Node(1, NodeType.START, 1);
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
        CreateDungeonBis(DungeonManager.instance.nbrCriticalRooms);
    }
}


