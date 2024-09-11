using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [SerializeField] GameObject streetPrefab;
    [SerializeField] private int maxStreets = 15;
    [SerializeField] private int minStreets = 10;

    [SerializeField] int streetWidth = 20;
    [SerializeField] int streetHeight = 12;

    [SerializeField] int gridSizeX = 10;
    [SerializeField] int gridSizeY = 10;

    private List<GameObject> streetObjects = new List<GameObject>();

    private Queue<Vector2Int> streetQueue = new Queue<Vector2Int>();

    int[,] streetGrid;

    private int streetCount;

    private bool generationComplete = false;

    void Start()
    {
        streetGrid = new int[gridSizeX, gridSizeY];
        streetQueue = new Queue<Vector2Int>();

        Vector2Int initialRoomIndex = new Vector2Int(gridSizeX / 2, gridSizeY / 2);
        StartRoomGenerationFromRoom(initialRoomIndex);
    }

    void Update()
    {
        if (streetQueue.Count > 0 && streetCount < maxStreets && !generationComplete)
        {
            Vector2Int roomIndex = streetQueue.Dequeue();
            int gridX = roomIndex.x;
            int gridY = roomIndex.y;

            TryGenerateRoom(new Vector2Int(gridX - 1, gridY));
            TryGenerateRoom(new Vector2Int(gridX + 1, gridY));
            TryGenerateRoom(new Vector2Int(gridX, gridY + 1));
            TryGenerateRoom(new Vector2Int(gridX, gridY - 1));
        }
        else if (streetCount < minStreets)
        {
            Debug.Log("Roomcount was less than the minimum amount of rooms. Try Again");
            RegenerateRooms();
        }
        else if (!generationComplete)
        {
            Debug.Log($"Generation Complete, {streetCount} rooms generated");
            generationComplete = true;
        }
    }

    private void StartRoomGenerationFromRoom(Vector2Int roomIndex)
    {
        streetQueue.Enqueue(roomIndex);
        int x = roomIndex.x;
        int y = roomIndex.y;
        streetGrid[x, y] = 1;
        streetCount++;
        var initialRoom = Instantiate(streetPrefab, GetPositionFromGridIndex(roomIndex), Quaternion.identity);
        initialRoom.name = $"Room-{streetCount}";
        initialRoom.GetComponent<Room>().roomIndex = roomIndex;
        streetObjects.Add(initialRoom);
    }

    private bool TryGenerateRoom(Vector2Int roomIndex)
    {
        int x = roomIndex.x;
        int y = roomIndex.y;

        if (streetCount >= maxStreets)
            return false;

        if (Random.value < 0.5 && roomIndex != Vector2Int.zero)
            return false;

        if (CountAdjacentRooms(roomIndex) > 1)
            return false;

        streetQueue.Enqueue(roomIndex);
        streetGrid[x, y] = 1;
        streetCount++;

        var newRoom = Instantiate(streetPrefab, GetPositionFromGridIndex(roomIndex), Quaternion.identity);
        newRoom.GetComponent<Room>().roomIndex = roomIndex;
        newRoom.name = $"Room-{streetCount}";
        streetObjects.Add(newRoom);

        OpenDoors(newRoom, x, y);

        return true;
    }


    private void RegenerateRooms()
    {
        streetObjects.ForEach(Destroy);
        streetObjects.Clear();
        streetGrid = new int[gridSizeX, gridSizeY];
        streetQueue.Clear();
        streetCount = 0;
        generationComplete = false;

        Vector2Int initialRoomIndex = new Vector2Int(gridSizeX / 2, gridSizeY / 2);
        StartRoomGenerationFromRoom(initialRoomIndex);
    }

    void OpenDoors(GameObject room, int x, int y)
    {
        Room newRoomScript = room.GetComponent<Room>();

        //neighbours
        Room leftRoomScript = GetRoomScriptAt(new Vector2Int(x - 1, y));
        Room rightRoomScript = GetRoomScriptAt(new Vector2Int(x + 1, y));
        Room topRoomScript = GetRoomScriptAt(new Vector2Int(x, y + 1));
        Room bottomRoomScript = GetRoomScriptAt(new Vector2Int(x, y - 1));

        //determine which doors to open based on neighbours
        if (x > 0 && streetGrid[x - 1, y] != 0)
        {
            //Neighbour to the left
            newRoomScript.OpenDoor(Vector2Int.left);
            leftRoomScript.OpenDoor(Vector2Int.right);
        }
        if (x < gridSizeX - 1 && streetGrid[x + 1, y] != 0)
        {
            //Neighbor to the right
            newRoomScript.OpenDoor(Vector2Int.right);
            rightRoomScript.OpenDoor(Vector2Int.left);
        }
        if (y > 0 && streetGrid[x, y - 1] != 0)
        {
            //Neighbour to the bottom
            newRoomScript.OpenDoor(Vector2Int.down);
            bottomRoomScript.OpenDoor(Vector2Int.up);
        }
        if (y < gridSizeY - 1 && streetGrid[x, y + 1] != 0)
        {
            //Neighbour to the top
            newRoomScript.OpenDoor(Vector2Int.up);
            topRoomScript.OpenDoor(Vector2Int.down);
        }

    }

    Room GetRoomScriptAt(Vector2Int roomIndex)
    {
        GameObject roomObject = streetObjects.Find(room => room.GetComponent<Room>().roomIndex == roomIndex);
        if (roomObject != null)
            return roomObject.GetComponent<Room>();
        return null;
    }

    private int CountAdjacentRooms(Vector2Int roomIndex)
    {
        int x = roomIndex.x;
        int y = roomIndex.y;
        int count = 0;

        if (x > 0 && streetGrid[x - 1, y] != 0) count++; //Left neighbour
        if (x < gridSizeX - 1 && streetGrid[x + 1, y] != 0) count++; //Right neighbour 
        if (y > 0 && streetGrid[x, y - 1] != 0) count++; //Bottom neighbour
        if (y < gridSizeY - 1 && streetGrid[x, y + 1] != 0) count++; //Top neighbour

        return count;

    }

    private Vector3 GetPositionFromGridIndex(Vector2Int gridIndex)
    {
        int gridX = gridIndex.x;
        int gridY = gridIndex.y;
        float isoX = (gridX - gridY) * (streetWidth / 2f);
        float isoY = (gridX + gridY) * (streetHeight / 2f);
        return new Vector3(isoX, isoY, 0);
    }

    private void OnDrawGizmos()
    {
        Color gizmoColor = new Color(0, 1, 1, 0.05f);
        Gizmos.color = gizmoColor;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 position = GetPositionFromGridIndex(new Vector2Int(x, y));
                Gizmos.DrawWireCube(position, new Vector3(streetWidth, streetHeight, 1));
            }
        }
    }
}