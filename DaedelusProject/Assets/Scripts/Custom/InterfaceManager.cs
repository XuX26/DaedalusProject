using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class InterfaceManager : MonoBehaviour
{
    public static InterfaceManager instance;
    [Header("Room Cards")]
    [SerializeField] private GameObject roomSelectionPanel;
    [SerializeField] private GameObject[] roomCards;
    [SerializeField] private Transform[] iconsPanels;
    [Header("Experience Bars")]
    [SerializeField] private Image fillBar;
    [SerializeField] private Image fillExp;
    [SerializeField] private Image fillBadExp;
    [Header("Event System")]
    [SerializeField] private EventSystem eventSystem;
    GameObject[] rooms = new GameObject[3] { null, null, null };
    [SerializeField] private GameObject endPanel;
    [SerializeField] private Text scoreText;
    [SerializeField] private Button restartButton;

    private void Awake()
    {
        if(instance != null && instance != this)
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
        fillBadExp.fillAmount = 0;
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
        if (rooms[index] != null)
        {
            float diff = rooms[index].GetComponent<Configuration>().diffucultyLevel;
            if (diff < 4)
            {
                float amountToLoose = (((rooms[index].GetComponent<Configuration>().diffucultyLevel * 1 / 3) * Player.Instance.fivePercent) - Player.Instance.fivePercent) + (Player.Instance.fivePercent * 0.5f);
                fillExp.fillAmount = 0;
                fillBadExp.fillAmount = Player.Instance.currentExperience / Player.Instance.maxExperience;
                fillBar.fillAmount = (Player.Instance.currentExperience - amountToLoose) / Player.Instance.maxExperience;
            }
            else
            {
                fillBadExp.fillAmount = 0;
                fillBar.fillAmount = Player.Instance.currentExperience / Player.Instance.maxExperience;
                fillExp.fillAmount = (Player.Instance.currentExperience + ((rooms[index].GetComponent<Configuration>().diffucultyLevel * (Player.Instance.fivePercent * 0.5f)) / 3)) / Player.Instance.maxExperience;
            }
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

            roomCards[0].SetActive(true);
            roomCards[1].SetActive(true);
            roomCards[2].SetActive(true);

            for (int i = 0; i < roomCards.Length; ++i)
            {
                if (rooms[i] != null)
                {
                    roomCards[i].transform.GetChild(0).GetComponent<Text>().text = rooms[i].GetComponent<Configuration>().diffucultyLevel.ToString();
                    roomCards[i].transform.GetChild(1).GetComponent<Text>().text = rooms[i].name;

                    //check for locked doors
                    bool hasWhatWereLookingFor = false;
                    foreach (Transform child in rooms[i].transform.GetChild(0))
                    {
                        if (child.CompareTag("Door"))
                        {
                            if (child.GetComponent<Door>().State == Door.STATE.CLOSED)
                            {
                                hasWhatWereLookingFor = true;
                                break;
                            }
                        }
                    }
                    ChangeOpacity(hasWhatWereLookingFor, iconsPanels[i].GetChild(0).GetComponent<Image>());

                    //TODO check for secret doors

                    //Check for healing potion
                    CheckObjectForIcon(i, 2, 1, "Health");

                    //Check for key
                    CheckObjectForIcon(i, 3, 1, "Key");

                    //Check for spikes
                    CheckObjectForIcon(i, 4, 0, "Spike");

                    //Check for enemies
                    CheckObjectForIcon(i, 5, 1, "Enemy");
                }
                else
                {
                    roomCards[i].transform.GetChild(0).GetComponent<Text>().text = "room unavailable";
                    roomCards[i].SetActive(false);
                }
            }
            ShowPossibleExperience(0);
            eventSystem.SetSelectedGameObject(roomCards[0].gameObject);
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
        roomSelectionPanel.SetActive(show);
    }

    void ChangeOpacity(bool isVisible, Image iconToChange)
    {
        if (isVisible)
        {
            iconToChange.color = new Color32(255, 255, 255, 255);
        }
        else
        {
            iconToChange.color = new Color32(255, 255, 255, 100);
        }
    }

    void CheckObjectForIcon(int cardIndex, int iconIndex, int roomChildIndex, string thingToSearch)
    {
        bool hasWhatWereLookingFor = false;
        foreach (Transform child in rooms[cardIndex].transform.GetChild(roomChildIndex))
        {
            if (child.CompareTag(thingToSearch))
            {
                hasWhatWereLookingFor = true;
                break;
            }
        }
        ChangeOpacity(hasWhatWereLookingFor, iconsPanels[cardIndex].GetChild(iconIndex).GetComponent<Image>());
    }

    public void ShowEndPanel()
    {
        restartButton.Select();
        scoreText.text = "Score : " + Player.Instance.currentExperience;
        endPanel.SetActive(true);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(0);
    }

}
