using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomInfo
{
    public string name;
    public int x;
    public int y;
}
public class RoomController : MonoBehaviour
{
    public static RoomController instance;

    string currentNeighbourhoodName = "Downtown";

    RoomInfo currentLoadRoomData;

    Queue<RoomInfo> loadRoomQueue = new Queue<RoomInfo>();

    public List<Room_Binding> loadedRooms = new List<Room_Binding>();

    bool isLoadingRoom = false;

    void Awake()
    {
        instance = this;

    }

    public void LoadRoom(string name, int x, int y)
    {
        RoomInfo newRoomData = new RoomInfo();
        newRoomData.name = name;
        newRoomData.x = x;
        newRoomData.y = y;

        loadRoomQueue.Enqueue(newRoomData);
    }

    IEnumerator LoadRoomRoutine(RoomInfo info)
    {
        string roomName = currentNeighbourhoodName + info.name;

        AsyncOperation loadRoom = SceneManager.LoadSceneAsync(roomName, LoadSceneMode.Additive);

        while (loadRoom.isDone == false)
        {
            yield return null;
        }
    }

    public void RegisterRoom (Room_Binding room)
    {
        room.transform.position = new Vector3(currentLoadRoomData.x * room.Width, currentLoadRoomData.x * room.Height, 0);
    }
   
    public bool DoesRoomExist(int x, int y) 
    {
        return loadedRooms.Find(item => item.X == x && item.Y == y) != null;
    }
}
