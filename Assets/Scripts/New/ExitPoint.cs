using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitPoint : MonoBehaviour
{
    public enum Direction { North, South, East, West }
    public Direction exitDirection; // The direction of the exit

    [HideInInspector]
    public bool isConnected = false; // Check if this exit is already connected to another street
}
