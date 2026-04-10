using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;
using NUnit.Framework;
using UnityEngine.InputSystem.Controls;
public class PlayerMotor : MonoBehaviour
{
    private bool isGrounded;
    private CharacterController controller;
    private Vector3 playerVelocity;
    public float Speed=5f;
    public float Gravity= -9.8f;
    public float JumpHigh=3f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller=GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded=controller.isGrounded;
    }

    //recieve inputs from InputManager.cs and apply on Character Controller component
    public void ProcessMove(Vector2 input)
    {
        Vector3 moveDirection = Vector3.zero;
        moveDirection.x = input.x;
        moveDirection.z= input.y;
        controller.Move(transform.TransformDirection(moveDirection)* Speed * Time.deltaTime);
        playerVelocity.y += Gravity*Time.deltaTime;
        if(isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y= -2f;
        }
        controller.Move(playerVelocity * Time.deltaTime);
        
    }
    public void ProcessJump()
    {
        if (isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(JumpHigh * -2f * Gravity);    
        }
        
        
    }
}
