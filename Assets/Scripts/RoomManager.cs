using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    /* These are the prefabs for each type of street possible, but as for right now,
     it only instantiates the streetPrefab as a placeholder.*/


    [SerializeField] GameObject streetPrefab; //This is the only prefab that is being instantiated right now,
                                              //we need to randomize the instantiation of the prefabs below
                                              //depending on the neighbours of the street and their exits (OpenDoors)
    [SerializeField] GameObject[] N;
    [SerializeField] GameObject[] S;
    [SerializeField] GameObject[] E;
    [SerializeField] GameObject[] W;
    [SerializeField] GameObject[] NS;
    [SerializeField] GameObject[] EW;
    [SerializeField] GameObject[] NE;
    [SerializeField] GameObject[] NW;
    [SerializeField] GameObject[] SE;
    [SerializeField] GameObject[] SW;
    [SerializeField] GameObject[] NSW;
    [SerializeField] GameObject[] NEW;
    [SerializeField] GameObject[] NSE;
    [SerializeField] GameObject[] SEW;
    [SerializeField] GameObject[] NSEW;

    [SerializeField] private int maxStreets = 15; // Maximum number of streets to generate
    [SerializeField] private int minStreets = 10; // Minimum number of streets to generate

    [SerializeField] int streetWidth = 20;        // Width of the street
    [SerializeField] int streetHeight = 12;       // Height of the street


    private List<GameObject> streetObjects = new List<GameObject>(); // List to store the street objects 
                                                                     // Every time a room is instantiated, it is added to this list

    private Queue<Vector2Int> streetQueue = new Queue<Vector2Int>(); // Queue to store the rooms to be generated

    [SerializeField] int gridSizeX;// size of the grid in the x axis
    [SerializeField] int gridSizeY;// size of the grid in the y axis
    int[,] streetGrid;             // 2D array to store the grid of the street

    private int streetCount; // Counter to keep track of the number of streets generated

    private bool generationComplete = false; // Boolean to check if the generation is complete

    void Start()
    {
        streetGrid = new int[gridSizeX, gridSizeY];
        streetQueue = new Queue<Vector2Int>();

        Vector2Int initialRoomIndex = new Vector2Int(gridSizeX / 2, gridSizeY / 4);
        StartRoomGenerationFromRoom(initialRoomIndex);
    }

    void Update()
    {
        //If there are rooms in the queue and the street count is less than the maximum number of streets,
        // and the generationcomplete is false, generate more rooms
        if (streetQueue.Count > 0 && streetCount < maxStreets && !generationComplete)
        {
            Vector2Int roomIndex = streetQueue.Dequeue();
            int gridX = roomIndex.x;
            int gridY = roomIndex.y;

            //Try to generate a room to the left
            TryGenerateRoom(new Vector2Int(gridX - 1, gridY));
            //Try to generate a room to the right
            TryGenerateRoom(new Vector2Int(gridX + 1, gridY));
            //Try to generate a room to the top
            TryGenerateRoom(new Vector2Int(gridX, gridY + 1));
            //Try to generate a room to the bottom
            TryGenerateRoom(new Vector2Int(gridX, gridY - 1));
        }
        //If the street count is less than the minimum number of streets, regenerate the rooms
        else if (streetCount < minStreets)
        {
            Debug.Log("Roomcount was less than the minimum amount of rooms. Try Again");
            RegenerateRooms();
        }
        //If the generation is complete, log the number of rooms generated
        else if (!generationComplete)
        {
            Debug.Log($"Generation Complete, {streetCount} rooms generated");
            generationComplete = true;

            // Get the reference to the last room generated
            GameObject lastRoom = streetObjects[streetObjects.Count - 1];
            Room lastRoomScript = lastRoom.GetComponent<Room>();

            // Log the details of the last room (this can be used to generate a boss or whatever later)
            Debug.Log($"Last Room: {lastRoom.name}, Index: {lastRoomScript.roomIndex}");
        }
    }

    private void StartRoomGenerationFromRoom(Vector2Int roomIndex)
    {
        //Enqueue the initial room
        streetQueue.Enqueue(roomIndex);

        int x = roomIndex.x; // x coordinate of the room
        int y = roomIndex.y; // y coordinate of the room

        streetGrid[x, y] = 1; 
        streetCount++;        // Increment the street count

        //Instantiate the initial room (we have to randomize this), at the position of the grid index, with no rotation.
        var initialRoom = Instantiate(streetPrefab, GetPositionFromGridIndex(roomIndex), Quaternion.identity);

        //Name (in the hierarchy) of the instantiated street + the street count
        initialRoom.name = $"Street-{streetCount}"; 

        initialRoom.GetComponent<Room>().roomIndex = roomIndex;

        // Add the initial room to the list of street objects
        streetObjects.Add(initialRoom);
    }

    
    private bool TryGenerateRoom(Vector2Int roomIndex)
    {
        int x = roomIndex.x;
        int y = roomIndex.y;

        //If the street count is greater than or equal to the maximum number of streets, return false
        if (streetCount >= maxStreets) 
            return false;

        //This is basically saying that there is a 50% chance of not generating a room on whichever side there is next
        //so there is variety in the generation
        if (Random.value < 0.5 && roomIndex != Vector2Int.zero)
            return false;

        //If the number of adjacent rooms is greater than 1, return false
        if (CountAdjacentRooms(roomIndex) > 1) 
            return false;

        //Enqueue the next room 
        streetQueue.Enqueue(roomIndex);
        streetGrid[x, y] = 1;
        //Increment the street count
        streetCount++;

        //If all the conditions above are met, instantiate the next room (this has to be randomized too because
        //right now is instatiating the same streetPrefab) at the position of the grid index
        var newRoom = Instantiate(streetPrefab, GetPositionFromGridIndex(roomIndex), Quaternion.identity);

        //Set the room index of the new room to the room index
        newRoom.GetComponent<Room>().roomIndex = roomIndex;
        //Name (in the hierarchy) of the instantiated street + the street count
        newRoom.name = $"Street-{streetCount}";
        //Add the new room to the list of street objects
        streetObjects.Add(newRoom);

        //Open the doors of the new room based on the neighbours (this will probably be deleted later,
        //but we can use the same logic to spawn the correct prefab instead.)
        OpenDoors(newRoom, x, y);

        //Return true if the room was generated successfully
        return true;
    }

    //This method is called when the street count is less than the minimum number of streets
    //basically it destroys all the streets and regenerates them so that the generation can start again
    // and reach the minimum number of streets
    private void RegenerateRooms()
    {
        //Destroy all the street objects in the list
        streetObjects.ForEach(Destroy);
        //Clear the list of street objects
        streetObjects.Clear();
        //Clear the queue of street objects
        streetGrid = new int[gridSizeX, gridSizeY];
        streetQueue.Clear();
        //Reset the street count
        streetCount = 0;
        //Set the generation complete to false
        generationComplete = false;

        Vector2Int initialRoomIndex = new Vector2Int(gridSizeX / 2, gridSizeY / 4);
        //Start the room generation from the initial room index
        StartRoomGenerationFromRoom(initialRoomIndex);
    }

    //This method opens the doors (child gameobject inside the prefabs)
    //of the room based on the neighbours of the street
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

    //This method gets the room script at the specified index, 
    //basically can be used to get the neighbours of the room, or
    //to check if a room exists at the specified index, make a room
    // a special room, like a boss fight, a shop, etc.
    Room GetRoomScriptAt(Vector2Int roomIndex)
    {
        GameObject roomObject = streetObjects.Find(room => room.GetComponent<Room>().roomIndex == roomIndex);
        if (roomObject != null)
            return roomObject.GetComponent<Room>();
        return null;
    }

    //This method counts the number of adjacent rooms to the specified room index
    //basically to check if the room has more than one neighbour so you dont have
    //4 rooms connected to each other
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

    //This method gets the position of the street prefab based on the grid index
    private Vector3 GetPositionFromGridIndex(Vector2Int gridIndex)
    {
        int gridX = gridIndex.x;
        int gridY = gridIndex.y;
        float isoX = (gridX - gridY) * (streetWidth / 2f);
        float isoY = (gridX + gridY) * (streetHeight / 4f);
        return new Vector3(isoX, isoY, 0);
    }


    //This method is used to draw the gizmos in the scene view for debugging purposes
    private void OnDrawGizmos()
    {
        Color gizmoColor = new Color(0, 1, 1, 0.05f);
        

        //size of a street prefab
        for (int x = streetWidth; x >= 0; x--)
        {
            for (int y = 0; y < streetHeight; y++)
            {
                float xOffset = (x + y) / 2f;
                float yOffset = (x - y) / 4f;
                Vector3 position = GetPositionFromGridIndex(new Vector2Int(x, y));
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(new Vector3(xOffset, yOffset, 0), Vector3.one);
            }
        }

        //Draw the grid where all the streets will be instantiated
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int i = 0; i < gridSizeY; i++)
            {
                Gizmos.color = gizmoColor;
                Gizmos.DrawWireCube(GetPositionFromGridIndex(new Vector2Int(x, i)), new Vector3(streetWidth, streetHeight, 0));
            }
        }
    }
}