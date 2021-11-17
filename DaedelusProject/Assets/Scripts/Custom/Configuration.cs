using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Configuration : MonoBehaviour
{
    public NodeType type = NodeType.START;
    public Difficulty diffucultyLevel = Difficulty.EASY;
    [Range(1, 4)]
    public int numberOfPossibleDoors = 2;
}
