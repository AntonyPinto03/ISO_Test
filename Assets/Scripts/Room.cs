using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public bool hasNorthExit = false;
    public bool hasSouthExit = false;
    public bool hasEastExit = false;
    public bool hasWestExit = false;

    public Vector2Int roomIndex { get; set; }

    public void SetExits(RoomManager.ExitsKey exitsKey)
    {
        hasNorthExit = exitsKey.North;
        hasSouthExit = exitsKey.South;
        hasEastExit = exitsKey.East;
        hasWestExit = exitsKey.West;
        // Optionally, update visuals or logic based on exits
    }
}