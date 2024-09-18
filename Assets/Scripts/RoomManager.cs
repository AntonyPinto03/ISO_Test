using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    // Define the possible exits
    public enum ExitDirection
    {
        North,
        South,
        East,
        West
    }

    // Serializable class to map exits to prefabs
    [System.Serializable]
    public class ExitPrefabPair
    {
        public ExitsKey exitsKey;
        public GameObject[] prefabs;
    }

    [System.Serializable]
    public class ExitsKey
    {
        public bool North;
        public bool South;
        public bool East;
        public bool West;

        // Override Equals and GetHashCode for dictionary key comparison
        public override bool Equals(object obj)
        {
            if (obj is ExitsKey other)
            {
                return North == other.North && South == other.South && East == other.East && West == other.West;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + North.GetHashCode();
            hash = hash * 23 + South.GetHashCode();
            hash = hash * 23 + East.GetHashCode();
            hash = hash * 23 + West.GetHashCode();
            return hash;
        }
    }

    [SerializeField] private ExitPrefabPair[] exitPrefabPairs;

    private Dictionary<ExitsKey, GameObject[]> exitPrefabDict = new Dictionary<ExitsKey, GameObject[]>();

    [SerializeField] private int maxStreets = 15; // Maximum number of streets to generate
    [SerializeField] private int minStreets = 10; // Minimum number of streets to generate

    [SerializeField] private int streetWidth = 20;        // Width of the street
    [SerializeField] private int streetHeight = 12;       // Height of the street

    private List<GameObject> streetObjects = new List<GameObject>(); // List to store the street objects

    private Queue<Vector2Int> streetQueue = new Queue<Vector2Int>(); // Queue to store the rooms to be generated

    [SerializeField] private int gridSizeX; // size of the grid in the x-axis
    [SerializeField] private int gridSizeY; // size of the grid in the y-axis
    private RoomData[,] roomGrid;           // 2D array to store the grid of rooms

    private int streetCount; // Counter to keep track of the number of streets generated

    private bool generationComplete = false; // Boolean to check if the generation is complete

    private Vector2Int[] directions = new Vector2Int[]
    {
        new Vector2Int(0, 1),   // North
        new Vector2Int(0, -1),  // South
        new Vector2Int(1, 0),   // East
        new Vector2Int(-1, 0)   // West
    };

    private Dictionary<Vector2Int, ExitDirection> dirToExit = new Dictionary<Vector2Int, ExitDirection>
    {
        { new Vector2Int(0, 1), ExitDirection.North },
        { new Vector2Int(0, -1), ExitDirection.South },
        { new Vector2Int(1, 0), ExitDirection.East },
        { new Vector2Int(-1, 0), ExitDirection.West }
    };

    private Dictionary<Vector2Int, ExitDirection> dirToOppositeExit = new Dictionary<Vector2Int, ExitDirection>
    {
        { new Vector2Int(0, 1), ExitDirection.South },
        { new Vector2Int(0, -1), ExitDirection.North },
        { new Vector2Int(1, 0), ExitDirection.West },
        { new Vector2Int(-1, 0), ExitDirection.East }
    };

    private Dictionary<ExitDirection, Vector2Int> exitToDir = new Dictionary<ExitDirection, Vector2Int>
    {
        { ExitDirection.North, new Vector2Int(0, 1) },
        { ExitDirection.South, new Vector2Int(0, -1) },
        { ExitDirection.East, new Vector2Int(1, 0) },
        { ExitDirection.West, new Vector2Int(-1, 0) }
    };

    // New variables to track the starting room, last room generated, and furthest room
    private Vector2Int startingRoomIndex;
    private RoomData lastRoomGenerated;

    // Reference to the Grid component
    private Grid grid;

    void Start()
    {
        grid = GetComponent<Grid>();

        // Initialize the exitPrefabDict
        foreach (var pair in exitPrefabPairs)
        {
            if (!exitPrefabDict.ContainsKey(pair.exitsKey))
            {
                exitPrefabDict.Add(pair.exitsKey, pair.prefabs);
            }
            else
            {
                Debug.LogWarning("Duplicate exit combination detected");
            }
        }

        roomGrid = new RoomData[gridSizeX, gridSizeY];
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                roomGrid[x, y] = new RoomData { gridIndex = new Vector2Int(x, y) };
            }
        }

        startingRoomIndex = new Vector2Int(gridSizeX / 2, gridSizeY / 4);
        StartRoomGenerationFromRoom(startingRoomIndex);
    }

    void Update()
    {
        if (streetQueue.Count > 0 && !generationComplete)
        {
            Vector2Int roomIndex = streetQueue.Dequeue();
            TryGenerateRoom(roomIndex);
        }
        else if (streetCount < minStreets && !generationComplete)
        {
            Debug.Log("Street count was less than the minimum amount. Regenerating...");
            RegenerateRooms();
        }
        else if (!generationComplete)
        {
            generationComplete = true;

            // Perform cleanup to remove exits leading to nowhere
            CleanupExits();

            // Find the furthest room from the starting room
            float maxDistance = -1f;
            RoomData furthestRoomData = null;

            foreach (var roomData in roomGrid)
            {
                if (roomData.hasRoom)
                {
                    float distance = Vector2Int.Distance(startingRoomIndex, roomData.gridIndex);
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        furthestRoomData = roomData;
                    }
                }
            }

            // Log the results
            Debug.Log($"Generation Complete, {streetCount} streets generated");

            if (lastRoomGenerated != null)
            {
                Debug.Log($"Last Room Generated: Index {lastRoomGenerated.gridIndex}");
            }
            if (furthestRoomData != null)
            {
                Debug.Log($"Furthest Room from Start: Index {furthestRoomData.gridIndex}, Distance {maxDistance}");
            }
        }
    }

    private void StartRoomGenerationFromRoom(Vector2Int roomIndex)
    {
        streetQueue.Enqueue(roomIndex);
        roomGrid[roomIndex.x, roomIndex.y].enqueued = true;
    }

    private void TryGenerateRoom(Vector2Int roomIndex)
    {
        int x = roomIndex.x;
        int y = roomIndex.y;

        // Check bounds
        if (x < 0 || x >= gridSizeX || y < 0 || y >= gridSizeY)
            return;

        RoomData currentRoom = roomGrid[x, y];

        // If this room already has been processed, return
        if (currentRoom.hasRoom)
            return;

        currentRoom.hasRoom = true;

        List<Vector2Int> availableDirs = new List<Vector2Int>();

        // Check for existing neighbors and set exits accordingly
        foreach (Vector2Int dir in directions)
        {
            int nx = x + dir.x;
            int ny = y + dir.y;

            // Check bounds
            if (nx < 0 || nx >= gridSizeX || ny < 0 || ny >= gridSizeY)
                continue;

            RoomData neighborRoom = roomGrid[nx, ny];

            // If neighbor room exists and has exit towards us
            if (neighborRoom.hasRoom && neighborRoom.exits.Contains(dirToOppositeExit[dir]))
            {
                currentRoom.exits.Add(dirToExit[dir]);
            }
            else if (!neighborRoom.hasRoom)
            {
                availableDirs.Add(dir);
            }
        }

        // Decide how many exits we want to create
        int maxExits = Mathf.Min(maxStreets - streetCount, 4);
        int exitsToCreate = Mathf.Min(Random.Range(1, maxExits + 1), availableDirs.Count);

        // Shuffle availableDirs
        for (int i = 0; i < availableDirs.Count; i++)
        {
            Vector2Int temp = availableDirs[i];
            int randomIndex = Random.Range(i, availableDirs.Count);
            availableDirs[i] = availableDirs[randomIndex];
            availableDirs[randomIndex] = temp;
        }

        // Check for adjacent rooms to prevent clusters of more than 3 rooms
        int adjacentRooms = CountAdjacentRooms(roomIndex);
        int maxRoomsToConnect = Mathf.Min(3 - adjacentRooms, exitsToCreate);

        exitsToCreate = Mathf.Min(exitsToCreate, maxRoomsToConnect);

        int exitsCreated = 0;

        for (int i = 0; i < availableDirs.Count && exitsCreated < exitsToCreate; i++)
        {
            Vector2Int dir = availableDirs[i];
            int nx = x + dir.x;
            int ny = y + dir.y;

            RoomData neighborRoom = roomGrid[nx, ny];

            // Only proceed if we haven't reached maxStreets
            if (streetCount + streetQueue.Count < maxStreets)
            {
                // Set our exit towards the direction
                currentRoom.exits.Add(dirToExit[dir]);

                // Set neighbor's exit towards us
                neighborRoom.exits.Add(dirToOppositeExit[dir]);

                // Enqueue neighbor for processing if not already enqueued
                if (!neighborRoom.enqueued)
                {
                    neighborRoom.enqueued = true;
                    streetQueue.Enqueue(new Vector2Int(nx, ny));
                }

                exitsCreated++;
            }
            else
            {
                // Can't create more rooms, so we skip adding this exit
                continue;
            }
        }

        // Now, create an ExitsKey for lookup
        ExitsKey currentExitsKey = new ExitsKey
        {
            North = currentRoom.exits.Contains(ExitDirection.North),
            South = currentRoom.exits.Contains(ExitDirection.South),
            East = currentRoom.exits.Contains(ExitDirection.East),
            West = currentRoom.exits.Contains(ExitDirection.West)
        };

        // Now, pick a prefab based on currentRoom.exits
        GameObject[] possiblePrefabs;
        if (exitPrefabDict.TryGetValue(currentExitsKey, out possiblePrefabs))
        {
            // Pick a random prefab
            GameObject prefabToInstantiate = possiblePrefabs[Random.Range(0, possiblePrefabs.Length)];

            // Instantiate the prefab as a child of the Grid GameObject
            Vector3 position = GetPositionFromGridIndex(roomIndex);
            GameObject roomInstance = Instantiate(prefabToInstantiate, position, Quaternion.identity, transform);

            // Set the room index
            Room roomScript = roomInstance.GetComponent<Room>();
            roomScript.roomIndex = roomIndex;
            roomScript.SetExits(currentExitsKey);

            // Name the room
            roomInstance.name = $"Street-{streetCount} {roomIndex}";

            // Add to streetObjects list
            streetObjects.Add(roomInstance);

            // Assign to currentRoom
            currentRoom.roomInstance = roomInstance;
        }
        else
        {
            // No prefab found for this exit combination
            Debug.LogWarning($"No prefab found for exits: N-{currentExitsKey.North}, S-{currentExitsKey.South}, E-{currentExitsKey.East}, W-{currentExitsKey.West}");
        }

        // Increment streetCount
        streetCount++;

        // Keep track of the last room generated
        lastRoomGenerated = currentRoom;
    }

    private void CleanupExits()
    {
        foreach (var roomData in roomGrid)
        {
            if (roomData.hasRoom)
            {
                HashSet<ExitDirection> validExits = new HashSet<ExitDirection>();

                int x = roomData.gridIndex.x;
                int y = roomData.gridIndex.y;

                foreach (ExitDirection exit in roomData.exits)
                {
                    Vector2Int dir = exitToDir[exit];
                    int nx = x + dir.x;
                    int ny = y + dir.y;

                    // Check bounds
                    if (nx < 0 || nx >= gridSizeX || ny < 0 || ny >= gridSizeY)
                        continue;

                    RoomData neighborRoom = roomGrid[nx, ny];

                    // If neighbor exists and has an exit towards us
                    if (neighborRoom.hasRoom && neighborRoom.exits.Contains(dirToOppositeExit[dir]))
                    {
                        validExits.Add(exit);
                    }
                }

                // Update the room's exits to only include valid exits
                roomData.exits = validExits;

                // Update the room's ExitsKey
                ExitsKey currentExitsKey = new ExitsKey
                {
                    North = roomData.exits.Contains(ExitDirection.North),
                    South = roomData.exits.Contains(ExitDirection.South),
                    East = roomData.exits.Contains(ExitDirection.East),
                    West = roomData.exits.Contains(ExitDirection.West)
                };

                // Update the Room component to reflect the changes
                if (roomData.roomInstance != null)
                {
                    Room roomScript = roomData.roomInstance.GetComponent<Room>();
                    roomScript.SetExits(currentExitsKey);

                    // Optionally, update the visuals of the room here
                }
            }
        }
    }

    private void RegenerateRooms()
    {
        // Destroy all the street objects in the list
        foreach (var obj in streetObjects)
        {
            Destroy(obj);
        }
        // Clear the list of street objects
        streetObjects.Clear();

        // Reset the room grid
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                roomGrid[x, y] = new RoomData { gridIndex = new Vector2Int(x, y) };
            }
        }

        // Clear the queue
        streetQueue.Clear();

        // Reset the street count
        streetCount = 0;
        generationComplete = false;

        Vector2Int initialRoomIndex = new Vector2Int(gridSizeX / 2, gridSizeY / 4);
        StartRoomGenerationFromRoom(initialRoomIndex);
    }

    private Vector3 GetPositionFromGridIndex(Vector2Int gridIndex)
    {
        int gridX = gridIndex.x;
        int gridY = gridIndex.y;
        float isoX = (gridX - gridY) * (streetWidth / 2f);
        float isoY = (gridX + gridY) * (streetHeight / 4f);
        return new Vector3(isoX, isoY, 0);
    }

    private void OnDrawGizmos()
    {
        Color gizmoColor = new Color(0, 1, 1, 0.05f);

        // Draw the grid where all the streets will be instantiated
        if (roomGrid != null)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    Gizmos.color = gizmoColor;
                    Gizmos.DrawWireCube(GetPositionFromGridIndex(new Vector2Int(x, y)), new Vector3(streetWidth, streetHeight, 0));
                }
            }
        }
    }

    // Class to hold data about each room in the grid
    private class RoomData
    {
        public Vector2Int gridIndex;
        public bool hasRoom = false;
        public bool enqueued = false;
        public HashSet<ExitDirection> exits = new HashSet<ExitDirection>();
        public GameObject roomInstance = null;
    }

    // Method to count adjacent rooms
    private int CountAdjacentRooms(Vector2Int roomIndex)
    {
        int x = roomIndex.x;
        int y = roomIndex.y;
        int count = 0;

        foreach (Vector2Int dir in directions)
        {
            int nx = x + dir.x;
            int ny = y + dir.y;

            // Check bounds
            if (nx < 0 || nx >= gridSizeX || ny < 0 || ny >= gridSizeY)
                continue;

            if (roomGrid[nx, ny].hasRoom)
                count++;
        }

        return count;
    }
}
