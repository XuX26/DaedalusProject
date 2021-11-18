using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InterfaceManager : MonoBehaviour
{
    public static InterfaceManager instance;
    [SerializeField] private GameObject roomSelectionPanel;
    [SerializeField] private GameObject[] roomCards;
    GameObject[] rooms = new GameObject[3] { null, null, null };

    private void Awake()
    {
        if(instance !=null && instance != this)
        {
            Destroy(instance);
        }
        instance = this;
    }

    public void ChooseRoom(int roomIndex)
    {
        DungeonGenerator.instance.AssignRoom(rooms[roomIndex]);
    }


    public void ShowSelectionPanel(bool show)
    {
        if (show)
        {
            DungeonGenerator.instance.ChooseThreeRooms(ref rooms, DungeonManager.instance.currentNode);
            for (int i = 0; i < roomCards.Length; ++i)
            {
                if (rooms[i] != null)
                {
                    roomCards[i].transform.GetChild(0).GetComponent<Text>().text = rooms[i].GetComponent<Configuration>().diffucultyLevel.ToString();
                    roomCards[i].transform.GetChild(1).GetComponent<Text>().text = rooms[i].name;
                }
                else
                {
                    roomCards[i].transform.GetChild(0).GetComponent<Text>().text = "room unavailable";
                }
            }
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
        roomSelectionPanel.SetActive(show);
    }


}
