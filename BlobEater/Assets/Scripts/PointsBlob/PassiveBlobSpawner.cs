using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveBlobSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject passivePointBlob;
    private float spawnRange;
    private int maxNumberOfBlobs;
    private int currentNumberOfBlobs;
    private bool cycleSpawn;

    public PassiveBlobSpawner()
    {
        spawnRange = 20f;
        maxNumberOfBlobs = 25;
        innitBlobs();
        StartSpawning();
    }

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

    public void StartSpawning()
    {
        cycleSpawn = true;
    }

    public void EndSpawning()
    {
        cycleSpawn = false;
    }

    public void cycleSpawning()
    {
        if (ShouldSpawnBlob() && cycleSpawn)
        {
            spawnBlob();
        }
    }

    public void spawnBlob()
    {
        Vector2 spawnPos = new Vector2(UnityEngine.Random.RandomRange(-spawnRange, spawnRange), UnityEngine.Random.RandomRange(-spawnRange, spawnRange));
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
