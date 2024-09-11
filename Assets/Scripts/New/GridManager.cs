using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GridManager : MonoBehaviour
{
    public GameObject initialStreetPrefab; // The first street to start the grid
    private StreetSpawner streetSpawner; // Reference to the StreetSpawner

    void Start()
    {
        // Find the StreetSpawner component
        streetSpawner = GetComponent<StreetSpawner>();

        // Place the first street at the origin
        GameObject firstStreet = Instantiate(initialStreetPrefab, Vector3.zero, Quaternion.identity);

        // Find the exit points of the first street
        ExitPoint[] firstStreetExits = firstStreet.GetComponentsInChildren<ExitPoint>();

        // Log the exits found
        Debug.Log("Initial street exits found: " + firstStreetExits.Length);

        // Spawn streets from each exit of the first street
        foreach (ExitPoint exit in firstStreetExits)
        {
            Debug.Log("Processing exit: " + exit.exitDirection);
            streetSpawner.SpawnStreetFromExit(exit);
        }
    }
}
