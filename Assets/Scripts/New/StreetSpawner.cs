using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreetSpawner : MonoBehaviour
{
    public GameObject[] streetPrefabs; // Array of street prefabs
    public float streetLength = 10f; // The length/width of one street block

    // Method to spawn a street from an exit point
    public void SpawnStreetFromExit(ExitPoint exitPoint)
    {
        if (exitPoint.isConnected)
        {
            Debug.LogWarning("This exit is already connected: " + exitPoint.exitDirection);
            return; // Exit is already connected to another street
        }

        Debug.Log("Spawning street from exit: " + exitPoint.exitDirection);

        // Calculate the position to spawn the next street based on the exit's position and direction
        Vector3 spawnPosition = CalculateNextPosition(exitPoint);

        // Choose a random street prefab (or use more sophisticated logic)
        GameObject chosenStreetPrefab = streetPrefabs[Random.Range(0, streetPrefabs.Length)];

        Debug.Log("Chosen street prefab: " + chosenStreetPrefab.name);

        // Instantiate the new street at the spawn position
        GameObject newStreet = Instantiate(chosenStreetPrefab, spawnPosition, Quaternion.identity);

        // Get the ExitPoints of the newly spawned street
        ExitPoint[] newStreetExits = newStreet.GetComponentsInChildren<ExitPoint>();

        // Find the opposite exit on the new street and mark it as connected
        foreach (ExitPoint newExit in newStreetExits)
        {
            if (IsOppositeDirection(exitPoint.exitDirection, newExit.exitDirection))
            {
                newExit.isConnected = true; // Mark the opposite exit as connected
                Debug.Log("Connected new street's opposite exit: " + newExit.exitDirection);
                break;
            }
        }

        // Mark the original exit point as connected
        exitPoint.isConnected = true;
    }

    // Calculate the position of the next street based on the exit point's direction
    private Vector3 CalculateNextPosition(ExitPoint exitPoint)
    {
        // Get the world position of the exit point as the base position
        Vector3 spawnPosition = exitPoint.transform.position;

        // Calculate the offset based on the exit direction and the length of a street
        switch (exitPoint.exitDirection)
        {
            case ExitPoint.Direction.North:
                spawnPosition += new Vector3(0, 0, streetLength);
                break;
            case ExitPoint.Direction.South:
                spawnPosition += new Vector3(0, 0, -streetLength);
                break;
            case ExitPoint.Direction.East:
                spawnPosition += new Vector3(streetLength, 0, 0);
                break;
            case ExitPoint.Direction.West:
                spawnPosition += new Vector3(-streetLength, 0, 0);
                break;
        }

        return spawnPosition;
    }

    // Check if two directions are opposite
    private bool IsOppositeDirection(ExitPoint.Direction dir1, ExitPoint.Direction dir2)
    {
        return (dir1 == ExitPoint.Direction.North && dir2 == ExitPoint.Direction.South) ||
               (dir1 == ExitPoint.Direction.South && dir2 == ExitPoint.Direction.North) ||
               (dir1 == ExitPoint.Direction.East && dir2 == ExitPoint.Direction.West) ||
               (dir1 == ExitPoint.Direction.West && dir2 == ExitPoint.Direction.East);
    }
}
