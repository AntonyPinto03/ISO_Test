using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    // Serialized fields to set map dimensions and the tile prefabs in the Unity editor
    [SerializeField] private int mapWidth, mapHeight;
    [SerializeField] private int streetWidth = 3; // New serialized field for street width
    [SerializeField] private GameObject streetTile;
    [SerializeField] private GameObject sidewalkTile;
    [SerializeField] private GameObject yellowLineTile;
    [SerializeField] private GameObject invertedYellowLineTile;
    [SerializeField] private GameObject xIntersectionTile;

    private void Update()
    {
        // Check if the space key is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Generate the map when space key is pressed
            GenerateMap();
        }
    }

    void GenerateMap()
    {
        // Clear existing tiles if necessary
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Randomly select a street pattern
        int pattern = Random.Range(0, 3); // Adjust the range based on the number of patterns

        // Switch case to handle different street patterns
        switch (pattern)
        {
            case 0:
                GenerateStraightStreet(); // Generate a straight street
                break;
            case 1:
                GenerateFourWayIntersection(); // Generate a four-way intersection
                break;
                // Add more cases for additional patterns
        }
    }

    // Method to generate a straight street
    void GenerateStraightStreet()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                // Calculate isometric offsets
                float xOffset = (x + y) / 2f;
                float yOffset = (x - y) / 4f;

                if (Mathf.Abs(y - mapHeight / 2) < streetWidth / 2)
                {
                    // Instantiate the street tile and yellow line tile at the calculated position
                    InstantiateTile(streetTile, xOffset, yOffset);
                    if (y == mapHeight / 2)
                    {
                        InstantiateTile(yellowLineTile, xOffset, yOffset);
                    }
                }
                else
                {
                    // Instantiate the sidewalk tile at the calculated position
                    InstantiateTile(sidewalkTile, xOffset, yOffset);
                }
            }
        }
    }

    // Method to generate a four-way intersection
    void GenerateFourWayIntersection()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                // Calculate isometric offsets
                float xOffset = (x + y) / 2f;
                float yOffset = (x - y) / 4f;

                if (Mathf.Abs(x - mapWidth / 2) < streetWidth / 2 && Mathf.Abs(y - mapHeight / 2) < streetWidth / 2)
                {
                    if (x == mapWidth / 2 && y == mapHeight / 2)
                    {
                        // Instantiate the X-shaped tile in the very center of the intersection
                        InstantiateTile(xIntersectionTile, xOffset, yOffset);
                    }
                    else if (x == mapWidth / 2)
                    {
                        // Instantiate the street tile and yellow line tile at the calculated position
                        InstantiateTile(streetTile, xOffset, yOffset);
                        InstantiateTile(invertedYellowLineTile, xOffset, yOffset);
                    }
                    else if (y == mapHeight / 2)
                    {
                        // Instantiate the street tile and inverted yellow line tile at the calculated position
                        InstantiateTile(streetTile, xOffset, yOffset);
                        InstantiateTile(yellowLineTile, xOffset, yOffset);
                    }
                    else
                    {
                        // Instantiate the street tile at the calculated position
                        InstantiateTile(streetTile, xOffset, yOffset);
                    }
                }
                else
                {
                    // Instantiate the sidewalk tile at the calculated position
                    InstantiateTile(sidewalkTile, xOffset, yOffset);
                }
            }
        }
    }

    // Helper method to instantiate a tile at a given position
    void InstantiateTile(GameObject tilePrefab, float xOffset, float yOffset)
    {
        Instantiate(tilePrefab, new Vector3(xOffset, yOffset, 0), Quaternion.identity, transform);
    }

    // Add more methods for additional street patterns
}

