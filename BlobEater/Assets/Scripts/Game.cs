using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Game : MonoBehaviour
{
    public GameObject playerM;

    private PassiveBlobSpawner pointsBlobSpawner = new PassiveBlobSpawner();

    private void Start()
    {
        Instantiate(playerM, new Vector2(0, 0), Quaternion.identity);

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        MainPlayer.Move();
        pointsBlobSpawner.cycleSpawning();        
    }
}
