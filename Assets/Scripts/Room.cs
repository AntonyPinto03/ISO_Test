using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    /* The exit gameobjects inside the street prefabs */
    [SerializeField] GameObject northExit;
    [SerializeField] GameObject southExit;
    [SerializeField] GameObject westExit;
    [SerializeField] GameObject eastExit;

    public Vector2Int roomIndex { set; get; }


    //this method opens the door in the specified direction by activating the corresponding game object associated with that direction.
    public void OpenDoor(Vector2Int direction) 
    {
        if (direction == Vector2Int.up)
        {
            northExit.SetActive(true);
        }
        if (direction == Vector2Int.down)
        {
            southExit.SetActive(true);
        }
        if (direction == Vector2Int.left)
        {
            westExit.SetActive(true);
        }
        if (direction == Vector2Int.right)
        {
            eastExit.SetActive(true);
        }
    }
}