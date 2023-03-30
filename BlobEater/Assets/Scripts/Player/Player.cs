using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.Windows;
using static Unity.Collections.AllocatorManager;

public class Player : NetworkBehaviour
{
    // Player
    public GameObject player;

    // Camera
    [SerializeField]
    private CinemachineVirtualCamera virtualCamera;
    private float minOrthoSize = 5f;
    private float maxOrthoSize = 15f;
    private float scoreThreshold = 500f;
    private float zoomSpeed = 1f;
    private float targetOrthoSize;
    private float currentOrthoSize;

    // Player Game Data
    [SerializeField]
    private NetworkVariable<float> currentPoints = new NetworkVariable<float>(1,NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private float currentSpeed;
    private float radius = 1;
    private float baseSpeed = 1;
    
    // Player Ui
    private int currentNumberOfKills = 0;
    public Text textKills;
    private int blobsEaten = 0;
    public Text textEaten;

    // Data to calulate the players speed
    // Max points is the largers amount of points a player can get before
    // their speed is not affected by points gained
    private float maxPointsForSpeedGain = 1500f;
    // Range is the range of values to be mapped inbetween
    private float speedXValueRange = 15f;

    // Player Database Data
    private float highScore;
    private int totalKills;
    private int totalBlobsEaten;



    private void Start()
    {
        UpdateSpeed();
        // Innits the players camera orthographic settings
        virtualCamera.gameObject.SetActive(false);  
        currentOrthoSize = virtualCamera.m_Lens.OrthographicSize;
        targetOrthoSize = currentOrthoSize;
    }

    public override void OnNetworkSpawn()
    {
        currentPoints.Value = 10f;
        base.OnNetworkSpawn();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        if(IsOwner)
        {
            virtualCamera.gameObject.SetActive(true);
        }
        Move();        
    }

    

    /// <summary>
    /// Handles collition with blobs and other players
    /// </summary>
    /// <param name="collision">Entity that the player collided with</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Kill(collision);
        UpdateUI();        
    }

    /// <summary>
    /// Full 2D movement of the player
    /// </summary>
    private void Move()
    {
        float inputX = UnityEngine.Input.GetAxis("Horizontal");
        float inputY = UnityEngine.Input.GetAxis("Vertical");

        if (inputX != 0.0 || inputY != 0.0)
        {
            Vector3 movement = new Vector3(currentSpeed * inputX, currentSpeed * inputY, 0);

            movement *= Time.deltaTime;
            transform.Translate(movement);
        }
    }


    /// <summary>
    /// Sets the current speed to the new speed relitive to the players points
    /// </summary>
    private void UpdateSpeed()
    {
        currentSpeed = CalculateSpeed();
    }

    /// <summary>
    /// Increase/decrease the players size relitive to their points
    /// </summary>
    private void UpdateSize()
    {
        radius = CalculateSize();
        player.transform.localScale = new Vector3(radius, radius, 1f);
        UpdateCameraZoom();
    }

    /// <summary>
    /// Zooms in/out the players camera relitive to their points
    /// </summary>
    private void UpdateCameraZoom()
    {
        // Calculate the target orthographic size based on the player's score
        float lerpValue = Mathf.Clamp01(currentPoints.Value / scoreThreshold);
        targetOrthoSize = Mathf.Lerp(minOrthoSize, maxOrthoSize, lerpValue);

        // Interpolate the current orthographic size to the target orthographic size and set it as the current size
        virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(currentOrthoSize, targetOrthoSize, zoomSpeed * Time.deltaTime);
    }

    // TODO edit once ui is working for each player
    private void UpdateUI()
    {
        GameObject canvas = GameObject.Find("Canvas");
        GameObject textKills = canvas.transform.GetChild(0).gameObject;
        GameObject textEaten = canvas.transform.GetChild(1).gameObject;

        textKills.GetComponent<Text>().text = string.Concat("Kills: ", currentNumberOfKills);
        textEaten.GetComponent<Text>().text = string.Concat("Blbos Eaten: ", blobsEaten);
    }

    /// <summary>
    /// Calculates the player's size relitive to the player's points
    /// </summary>
    /// <returns></returns>
    private float CalculateSize()
    {
        float newRadius = radius + (10/currentPoints.Value);
        return newRadius;
    }

    /// <summary>
    /// Calculates the player's speed relitive to the players points
    /// </summary>
    /// <returns>Float value speed</returns>
    private float CalculateSpeed()
    {
        //float speedFactor = MathF.Pow(baseSpeed, (currentPoints * 0.001f));        
        if (currentPoints.Value < 10) { currentPoints.Value = 10; }
        float divider = maxPointsForSpeedGain / speedXValueRange;
        float xValue;
        // If points reach greater than cieling stop speed decline
        if (currentPoints.Value < maxPointsForSpeedGain)
        {
            xValue = currentPoints.Value / divider;
        }
        else
        {
            xValue = 15;
        }
        // https://www.desmos.com/calculator/93k7dmnj3m
        // Negitive logarithmic function to map points to speed non-linearly
        float speed = -5f * MathF.Log10(xValue) + 10;
        return speed;
    }

    /// <summary>
    /// Handles positive collition with blobs and players, killing and gaining points
    /// </summary>
    /// <param name="entity">The GameObject the player collided with</param>
    public void Kill(Collision2D entity)
    {
        if (entity.gameObject.tag == "Points Blob")
        {            
            currentPoints.Value += 10;
            blobsEaten += 1;
            DespawnBlobsServerRPC(entity.gameObject.GetComponent<NetworkObject>().NetworkObjectId);
            
            // Update player stats
            UpdateSize();
            UpdateSpeed();
        }
        else 
        { 
            if(currentPoints.Value > (2*entity.gameObject.GetComponent<Player>().currentPoints.Value))
            {
                currentPoints.Value += entity.gameObject.GetComponent<Player>().currentPoints.Value;
                entity.gameObject.GetComponent<Player>().Death();

                // Update player stats
                currentNumberOfKills += 1;
                UpdateSize();
                UpdateSpeed();
            }
        }
    }


    [ServerRpc]
    public void DespawnBlobsServerRPC(ulong id)
    {
        GetNetworkObject(id).Despawn(true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DespawnenPlayerServerRPC()
    {
        if (NetworkManager.ConnectedClients.ContainsKey(OwnerClientId))
        {
            var client = NetworkManager.ConnectedClients[OwnerClientId];
            Destroy(client.PlayerObject.gameObject);
        }
    }

    /// <summary>
    /// Handles player's death
    /// </summary>
    public void Death()
    {
        DespawnenPlayerServerRPC();
    }



}
