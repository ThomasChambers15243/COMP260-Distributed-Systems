using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveBlobSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject passivePointBlob;
    private float spawnRange;
    private int maxNumberOfBlobs;
    private bool cycleSpawn;
    [SerializeField]
    private List<GameObject> blobs;

    private void Start()
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

    public int GetCurrentNumberOfBlobs()
    {
        return blobs.Count;
    }

    public bool ShouldSpawnBlob()
    {
        if (GetCurrentNumberOfBlobs() < maxNumberOfBlobs)
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

    private void UpdateBlobCount()
    {
        int count = 0;
        while (count < blobs.Count)
        {
            if (blobs[count] == null)
            {
                Debug.Log(blobs[count]);
                blobs.RemoveAt(count);
                count--;
            }
            count++;
        }
    }

    public void cycleSpawning()
    {
        UpdateBlobCount();
        if (ShouldSpawnBlob() && cycleSpawn)
        {
            Debug.Log("In succussful cycle");
            spawnBlob();
        }        
    }

    public void spawnBlob()
    {
        Vector2 spawnPos = new Vector2(UnityEngine.Random.RandomRange(-spawnRange, spawnRange), UnityEngine.Random.RandomRange(-spawnRange, spawnRange));
        blobs.Add(Instantiate(passivePointBlob, spawnPos, Quaternion.identity));
    }

    public void killThis()
    {
        Destroy(passivePointBlob);
    }

    private void innitBlobs()
    {
        for (int i = 0; i < maxNumberOfBlobs; i++)
        {
            spawnBlob();
        }
    }
}
