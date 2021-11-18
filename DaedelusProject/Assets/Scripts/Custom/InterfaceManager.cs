using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterfaceManager : MonoBehaviour
{
    public static InterfaceManager instance;
    [SerializeField] private GameObject roomSelectionPanel;

    private void Awake()
    {
        if(instance !=null && instance != this)
        {
            Destroy(instance);
        }
        instance = this;
    }


    public void ShowSelectionPanel(bool show)
    {
        if (show)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
        roomSelectionPanel.SetActive(show);
    }


}
