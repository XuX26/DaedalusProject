using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RoomCard : MonoBehaviour, ISelectHandler
{
    public void OnSelect(BaseEventData eventData)
    {
        InterfaceManager.instance.ShowPossibleExperience(transform.GetSiblingIndex());
    }
}
