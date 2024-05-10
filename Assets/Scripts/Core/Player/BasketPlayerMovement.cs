using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class BasketPlayerMovement : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    // [SerializeField] private Transform basketTransform;
    [SerializeField] private Rigidbody2D rb;

    [Header("Settings")]
    [SerializeField] private float movementSpeed = 10f;
    [SerializeField] private float maximumVelocity = 5f;
    [SerializeField] private float jumpAmount = 6f;
    [SerializeField] private float gravityScale = 1f;
    [SerializeField] private float fallingGravityScale = 2f;

    private float previousMovementInput;

    private int numberOfJumps;

    private bool isGrounded;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        inputReader.MoveEvent += HandleMove;
        inputReader.JumpEvent += HandleJump;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;

        inputReader.MoveEvent -= HandleMove;
        inputReader.JumpEvent -= HandleJump;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!IsOwner) return;

        // Handle double jump logic
        if (numberOfJumps >= 2) 

        // Handle falling jump logic
        if (rb.velocity.y >= 0)
        {
            rb.gravityScale = gravityScale;
        }
        else if (rb.velocity.y < 0)
        {
            rb.gravityScale = fallingGravityScale;
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        // Handle max speed
        if (math.abs(rb.velocity.x) < maximumVelocity)
        {
            rb.AddForce(Vector2.right * previousMovementInput * movementSpeed, ForceMode2D.Force);
        }
    }

    public void HandleMove(float movementInput)
    {
        previousMovementInput = movementInput;
    }

    private void HandleJump(bool obj)
    {
        if (isGrounded)
        {
            rb.AddForce(Vector2.up * jumpAmount, ForceMode2D.Impulse);
            isGrounded = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Ground")
        {
            if (rb.velocity.y <= 0)
            {
                isGrounded = true;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.tag == "Ground")
        {
            isGrounded = false;
        }
    }
}
