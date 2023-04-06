using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PassiveBlobSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject passivePointBlob;
    private int maxNumberOfBlobs;
    private float spawnRange;
    private bool cycleSpawn;
    [SerializeField]
    private List<GameObject> blobs;

    public void InnitSpawner()
    {
        spawnRange = 20f;
        maxNumberOfBlobs = 25;
        blobs = new List<GameObject>();
        innitBlobs();
        StartSpawning();
    }

    private void FixedUpdate()
    {
        cycleSpawning();
    }

    /// <summary>
    /// Gets current amount of blobs
    /// </summary>
    /// <returns>Number of current blobs</returns>
    public int GetCurrentNumberOfBlobs()
    {
        return blobs.Count;
    }

    /// <summary>
    /// Checks to see if theres not enough blobs and whether more should be spawned
    /// </summary>
    /// <returns>True if blobs should spawn, else false</returns>
    public bool ShouldSpawnBlob()
    {
        if (GetCurrentNumberOfBlobs() < maxNumberOfBlobs)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Starts the spawning cycle of blobs
    /// </summary>
    public void StartSpawning()
    {
        cycleSpawn = true;
    }

    /// <summary>
    /// Ends the spawning cycle of blobs
    /// </summary>
    public void EndSpawning()
    {
        cycleSpawn = false;
    }

    /// <summary>
    /// Checks and increase if nessesary the amount of blobs
    /// </summary>
    private void UpdateBlobCount()
    {
        int count = 0;
        while (count < blobs.Count)
        {
            if (blobs[count] == null)
            {
                blobs.RemoveAt(count);
                count--;
            }
            count++;
        }
    }

    /// <summary>
    /// Handles the spawning cycle of blobs
    /// </summary>
    public void cycleSpawning()
    {
        UpdateBlobCount();
        if (ShouldSpawnBlob() && cycleSpawn)
        {
            spawnBlob();
        }        
    }

    /// <summary>
    /// Spawns in one points blob
    /// </summary>
    public void spawnBlob()
    {
        Vector2 spawnPos = new Vector2(UnityEngine.Random.Range(-spawnRange, spawnRange), UnityEngine.Random.Range(-spawnRange, spawnRange));
        
        // Networked Spawning
        GameObject spawnedBlob = Instantiate(passivePointBlob, spawnPos, Quaternion.identity);
        blobs.Add(spawnedBlob);
        spawnedBlob.GetComponent<NetworkObject>().Spawn(true);
    }

    /// <summary>
    /// Destroys the blob
    /// </summary>
    public void killThis()
    {
        Destroy(passivePointBlob);
    }

    /// <summary>
    /// Sets up the initial game with blobs
    /// </summary>
    private void innitBlobs()
    {
        for (int i = 0; i < maxNumberOfBlobs; i++)
        {
            spawnBlob();
        }
    }
}
