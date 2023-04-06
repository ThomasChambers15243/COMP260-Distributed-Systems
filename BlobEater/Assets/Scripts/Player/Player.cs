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
    private float currentPoints = 10;
    private float currentSpeed;
    private float radius = 1;
    
    // Data to calulate the players speed
    // Max points is the largers amount of points a player can get before
    // their speed is not affected by points gained
    private float maxPointsForSpeedGain = 1500f;
    // Range is the range of values to be mapped inbetween
    private float speedXValueRange = 15f;
    
    // Player Ui
    private int currentNumberOfKills = 0;
    private int blobsEaten = 0;

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

    

    private void FixedUpdate()
    {
        // Ensures independent control from client 
        if (!IsOwner) return;
        if(IsOwner)
        {
            // Sets up the clients own, personal camera
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

        // If there is any imput, let the server handle the new game state
        if (inputX != 0.0 || inputY != 0.0)
        {            
            SendMovementServerRPC(inputX * currentSpeed, inputY * currentSpeed);
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
        float lerpValue = Mathf.Clamp01(currentPoints / scoreThreshold);
        targetOrthoSize = Mathf.Lerp(minOrthoSize, maxOrthoSize, lerpValue);

        // Interpolate the current orthographic size to the target orthographic size and set it as the current size
        virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(currentOrthoSize, targetOrthoSize, zoomSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Sets the scores UI text for the player
    /// </summary>
    private void UpdateUI()
    {
        GameObject canvas = GameObject.Find("Canvas");
        GameObject textKills = canvas.transform.GetChild(0).gameObject;
        GameObject textEaten = canvas.transform.GetChild(1).gameObject;

        // Write new text to the players UI
        textKills.GetComponent<Text>().text = string.Concat("Kills: ", currentNumberOfKills);
        textEaten.GetComponent<Text>().text = string.Concat("Blobs Eaten: ", blobsEaten);
    }

    /// <summary>
    /// Calculates the player's size relitive to the player's points
    /// </summary>
    /// <returns>Float value of the new raduis</returns>
    private float CalculateSize()
    {
        float newRadius = radius + 10/currentPoints;
        return newRadius;
    }

    /// <summary>
    /// Calculates the player's speed relitive to the players points
    /// </summary>
    /// <returns>Float value of speed</returns>
    private float CalculateSpeed()
    {
        // Sets a minimium points so that the functions is bound
        if(currentPoints < 10) { currentPoints = 10; }

        float divider = maxPointsForSpeedGain / speedXValueRange;

        // X postion on curve 
        float xValue = 1;

        // If points reach a value greater than the cieling, stop speed decline
        if (currentPoints < maxPointsForSpeedGain) 
        {
            xValue = currentPoints / divider;
        }
        else
        {
            // Maximium value means that the speed can't increase
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
            // Update the player's points with the value of a points blob
            PointsUpdateServerRPC(10);
            blobsEaten += 1;

            // Remove blob so a new one can be spawned in
            DespawnBlobsServerRPC(entity.gameObject.GetComponent<NetworkObject>().NetworkObjectId);

            // Update player stats
            UpdateSize();
            UpdateSpeed();
        }
        else 
        { 
            if(currentPoints > (2*entity.gameObject.GetComponent<Player>().currentPoints))
            {
                // Update the player's points with the value of the player they colllided with
                PointsUpdateServerRPC((int)entity.gameObject.GetComponent<Player>().currentPoints);
                currentNumberOfKills += 1;
                
                // Remove the other player
                entity.gameObject.GetComponent<Player>().Death();          
                
                // Update player stats
                UpdateSize();
                UpdateSpeed();
            }
        }
    }
    /// <summary>
    /// Remove network object from the game
    /// </summary>
    /// <param name="id">Clients Id</param>
    [ServerRpc(RequireOwnership = false)]
    public void DespawnBlobsServerRPC(ulong id)
    {
        GetNetworkObject(id).Despawn(true);
    }

    /// <summary>
    /// Update players points
    /// </summary>
    /// <param name="points">Amount to update by</param>
    /// <param name="serverRpcParams"></param>
    [ServerRpc(RequireOwnership = false)]
    public void PointsUpdateServerRPC(int points, ServerRpcParams serverRpcParams = default)
    {        
        // Get client
        var client = NetworkManager.ConnectedClients[OwnerClientId];

        client.PlayerObject.gameObject.GetComponent<Player>().currentPoints += points;
    }

    /// <summary>
    /// Move player server side
    /// </summary>
    /// <param name="inputX">X-axis float value</param>
    /// <param name="inputY">Y-axis float value</param>
    /// <param name="serverRpcParams"></param>
    [ServerRpc(RequireOwnership = false)]
    public void SendMovementServerRPC(float inputX, float inputY, ServerRpcParams serverRpcParams = default)
    {
        // If client is still connected
        if (NetworkManager.ConnectedClients.ContainsKey(OwnerClientId))
        {
            var client = NetworkManager.ConnectedClients[OwnerClientId];

            // Calulate clients movement vector relitivve to server delta time
            Vector3 movement = new Vector3(currentSpeed * inputX, currentSpeed * inputY, 0);
            movement *= NetworkManager.ServerTime.FixedDeltaTime;
            
            // Scale down vector
            movement = movement / 15;
            
            // Translate player server side
            client.PlayerObject.transform.Translate(movement);
        }
    }

    /// <summary>
    /// Remove connect player
    /// </summary>
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
