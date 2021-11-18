using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Configuration : MonoBehaviour
{
    public NodeType type = NodeType.START;
    [Range(1, 10)]
    public int diffucultyLevel = 1;
    [Range(1, 4)]
    public int numberOfPossibleDoors = 2;
    public bool isFlexible = true;

}
