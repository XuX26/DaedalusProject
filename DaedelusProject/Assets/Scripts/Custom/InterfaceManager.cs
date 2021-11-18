using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InterfaceManager : MonoBehaviour
{
    public static InterfaceManager instance;
    [SerializeField] private GameObject roomSelectionPanel;
    [SerializeField] private GameObject[] roomCards;
    [SerializeField] private Image fillBar;
    [SerializeField] private Image fillExp;
    [SerializeField] private Image fillBadExp;
    [SerializeField] private EventSystem eventSystem;
    GameObject[] rooms = new GameObject[3] { null, null, null };

    private void Awake()
    {
        if(instance !=null && instance != this)
        {
            Destroy(instance);
        }
        instance = this;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Start()
    {
        fillBar.fillAmount = 0;
        fillExp.fillAmount = 0;
    }
    private void Update()
    {
        if(eventSystem.currentSelectedGameObject == null)
        {
            roomCards[0].GetComponent<Button>().Select();
        }
    }

    //public void AddExperience()
    //{
    //    fillBar.fillAmount = Player.Instance.currentExperience / Player.Instance.maxExperience;
    //}

    public void ShowPossibleExperience(int index)
    {
        float diff = rooms[index].GetComponent<Configuration>().diffucultyLevel;
        if (diff < 3)
        {
            fillBadExp.fillAmount = Player.Instance.currentExperience / Player.Instance.maxExperience;
            fillBar.fillAmount = fillExp.fillAmount = (Player.Instance.currentExperience - (diff * 10)) / Player.Instance.maxExperience;
        }
        else
        {
            fillBar.fillAmount = Player.Instance.currentExperience / Player.Instance.maxExperience;
            fillExp.fillAmount = (Player.Instance.currentExperience + (diff * 10)) / Player.Instance.maxExperience;
        }
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
            eventSystem.SetSelectedGameObject(roomCards[0].gameObject);
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
        roomSelectionPanel.SetActive(show);
    }


}
