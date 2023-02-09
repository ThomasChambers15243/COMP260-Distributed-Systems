using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveBlobSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject passivePointBlob;
    private float spawnRange = 20f;
    private int maxNumberOfBlobs = 25;
    private int currentNumberOfBlobs;

    public int GetCurrentNumberOfBlobs()
    {
        return currentNumberOfBlobs;
    }

    public bool ShouldSpawnBlob()
    {
        if (currentNumberOfBlobs < maxNumberOfBlobs)
        {
            return true;
        }
        return false;
    }

    private void Start()
    {
        innitBlobs();
    }

    private void FixedUpdate()
    {
        if (ShouldSpawnBlob())
        {
            spawnBlob();
        }
    }

    public void spawnBlob()
    {
        Vector2 spawnPos = new Vector2(Random.RandomRange(-spawnRange, spawnRange), Random.RandomRange(-spawnRange, spawnRange));
        Instantiate(passivePointBlob, spawnPos, Quaternion.identity);
        currentNumberOfBlobs += 1;
    }

    private void innitBlobs()
    {
        for(int i = 0; i < maxNumberOfBlobs; i++)
        {
            spawnBlob();            
        }
    }
}
