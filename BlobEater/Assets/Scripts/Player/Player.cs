using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;

public class Player : MonoBehaviour
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
    private float currentPoints = 1;
    private float currentSpeed;
    private float radius = 1;
    private float baseSpeed = 1;
    
    // Player Ui
    private int currentNumberOfKills = 0;
    public Text textKills;
    private int blobsEaten = 0;
    public Text textEaten;

    // Player Database Data
    private float highScore;
    private int totalKills;
    private int totalBlobsEaten;



    private void Start()
    {
        UpdateSpeed();

        // Innits the players camera orthographic settings
        currentOrthoSize = virtualCamera.m_Lens.OrthographicSize;
        targetOrthoSize = currentOrthoSize;
    }

    private void FixedUpdate()
    {
        Move();
    }

    /// <summary>
    /// Handles collition with blobs and other players
    /// </summary>
    /// <param name="collision">Entity that the player collided with</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Kill(collision.gameObject);
        UpdateUI();
    }

    /// <summary>
    /// Full 2D movement of the player
    /// </summary>
    private void Move()
    {
        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(currentSpeed * inputX, currentSpeed * inputY, 0);

        movement *= Time.deltaTime;

        player.transform.Translate(movement);
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

    private void UpdateUI()
    {
        textKills.text = string.Concat("KILLS: ", currentNumberOfKills);
        textEaten.text = string.Concat("Blobs Eaten: ", blobsEaten);
    }

    /// <summary>
    /// Calculates the player's size relitive to the player's points
    /// </summary>
    /// <returns></returns>
    private float CalculateSize()
    {
        radius += 10/currentPoints;
        return radius;
    }

    /// <summary>
    /// Calculates the player's speed relitive to the players points
    /// </summary>
    /// <returns>Float value speed</returns>
    private float CalculateSpeed()
    {
        float baseX = baseSpeed + (currentPoints * 0.01f);
        return 10f * Mathf.Pow(baseSpeed, -1f);
    }

    /// <summary>
    /// Handles positive collition with blobs and players, killing and gaining points
    /// </summary>
    /// <param name="entity">The GameObject the player collided with</param>
    public void Kill(GameObject entity)
    {
        if (entity.tag == "Points Blob")
        {
            currentPoints += 10;
            blobsEaten += 1;            
            Destroy(entity);
        }
        if (entity.tag == "Player")
        {
            currentPoints += 20;
            currentNumberOfKills += 1;
            Destroy(entity);
        }
        // Update player stats
        UpdateSize();
        UpdateSpeed();
    }

    /// <summary>
    /// Handles player's death
    /// </summary>
    public void Death()
    {

    }



}
