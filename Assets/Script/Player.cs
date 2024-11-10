using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float startSpeed = 5f; // Speed during the initial straight movement
    public float diagonalSpeed = 5f; // Speed of the diagonal movement
    public float fallSpeed = 5f; // Speed of falling
    public float initialDistance = 3f; // Distance for the initial straight movement
    private bool isFalling = false; // Check if the player started falling
    private bool movingUp = false; // Direction control
    private Vector2 moveDirection;
    private Vector3 startPosition;

    void Start()
    {
        // Initial movement on the x-axis
        moveDirection = Vector2.right * startSpeed;
        startPosition = transform.position;
    }

    void Update()
    {
        if (!isFalling)
        {
            // Move the player initially on the x-axis
            transform.Translate(moveDirection * Time.deltaTime, Space.World);

            // After covering the specified distance, start falling diagonally
            if (Vector3.Distance(startPosition, transform.position) >= initialDistance)
            {
                isFalling = true;
                moveDirection = new Vector2(diagonalSpeed, -fallSpeed);
            }
        }
        else
        {
            // Move diagonally
            transform.Translate(moveDirection * Time.deltaTime, Space.World);

            // If the screen is being tapped and held, change direction
            if (Input.GetMouseButton(0)) // Holding the screen
            {
                movingUp = true;
                moveDirection.y = fallSpeed; // Move upward
            }
            else
            {
                movingUp = false;
                moveDirection.y = -fallSpeed; // Move downward
            }
        }

        // Rotate the player to face the movement direction
        RotatePlayer();
    }

    void RotatePlayer()
    {
        // Calculate the angle between the player's forward direction and the movement direction
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;

        // Apply the rotation to the player
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }
}
