using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Data
    public Transform player;

    private float currentPoints = 1;
    private float currentSpeed;
    private float radius = 1;
    private int numberOfKills = 0;
    private float highScore;

    private float baseSpeed = 1;

    private void Start()
    {
        currentSpeed = CalculateSpeed();
        
    }

    private void UpdateSize()
    {
        radius = CalculateSize();
        player.localScale = new Vector3(radius, radius,1f);
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Points Blob")
        {
            Kill(collision.gameObject);
        }
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
    /// Full 2D movement of the player
    /// </summary>
    private void Move()
    {
        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(currentSpeed * inputX, currentSpeed * inputY, 0);

        movement *= Time.deltaTime;

        player.Translate(movement);
    }



    public void Kill(GameObject entity)
    {
        if (entity.tag == "Points Blob")
        {
            currentPoints += 10;
        }

        UpdateSize();
        currentSpeed = CalculateSpeed();
        Destroy(entity);
    }

    public void Death()
    {

    }



}
