using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindShadowEffect : MonoBehaviour
{
    public float shadowMoveSpeed = 3.0f;  // Speed of the shadow movement
    public float shadowMoveAmount = 0.05f;  // Amount of shadow movement

    private Vector3 initialDirection;  // Store the original light direction
    private float timer = 0.0f;

    void Start()
    {
        initialDirection = transform.eulerAngles;
    }

    void Update()
    {
        // Calculate a small oscillation for the shadow movement
        float offset = Mathf.Sin(timer) * shadowMoveAmount;

        // Apply the oscillation to the light's direction to create the wind effect
        transform.eulerAngles = new Vector3(initialDirection.x + offset, initialDirection.y, initialDirection.z);

        // Increment the timer based on the shadow move speed
        timer += Time.deltaTime * shadowMoveSpeed;
    }
}
