using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;
using NUnit.Framework;
using UnityEngine.InputSystem.Controls;
public class PlayerMotor : MonoBehaviour
{
    
    private bool isGrounded;
    private bool isDiving = false;
    private CharacterController controller;
    private Vector3 playerVelocity;
    public float Speed=5f;
    public float Gravity= -9.8f;
    public float JumpHigh=3f;


     // Slide settings
    public float SlideSpeed = 10f;          // initial slide speed
    public float SlideDeceleration = 8f;    // how fast it slows down
    public float SlideStopThreshold = 1.5f; // stops when speed drops below this
    public float SlideCrouchHeight = 0.9f;  // collider height while sliding

    private bool isSliding = false;
    private float currentSlideSpeed = 0f;
    private Vector3 slideDirection;
    private float originalHeight;
    private Vector3 originalCenter;

    //Grapple settings 
     public float GrapplePullForce = 20f;    // acceleration toward the point
    public float GrappleMaxSpeed = 25f;     // cap so you don't fly forever
    public float GrappleArrivalDistance = 2f; // auto-release when this close
    public Camera playerCamera;             // assign in Inspector

    private bool isGrappling;
    private Vector3 grapplePoint;
    private LineRenderer ropeRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller=GetComponent<CharacterController>();
        originalHeight = controller.height;
        originalCenter = controller.center;
        // Completely rebuild the LineRenderer from scratch via code
        ropeRenderer = GetComponent<LineRenderer>();

        //inspector things
        ropeRenderer.positionCount = 2;
        ropeRenderer.useWorldSpace = true;
        ropeRenderer.startWidth = 0.15f;
        ropeRenderer.endWidth = 0.15f;
        ropeRenderer.numCapVertices = 4;
        ropeRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        ropeRenderer.receiveShadows = false;

        

        ropeRenderer.enabled = false;
    }


    void LateUpdate()
    {
        if (isGrappling){
            ropeRenderer.enabled = true;
            UpdateRope();
        }
    }
    // Update is called once per frame
    void Update()
    {
        isGrounded=controller.isGrounded;
        
    }

    

    //recieve inputs from InputManager.cs and apply on Character Controller component
    public void ProcessMove(Vector2 input)
    {
        if (isDiving)
        {
            playerVelocity.y -= 40f * Time.deltaTime; // accelerate downward
            controller.Move(playerVelocity * Time.deltaTime);

            // Stop dive when grounded
            if (isGrounded)
            {
                isDiving = false;
                controller.height = originalHeight;
                controller.center = originalCenter;
            }
            return; // skip all other movement while diving
        }
        if (!isSliding)
        {
            Vector3 moveDirection = Vector3.zero;
            moveDirection.x = input.x;
            moveDirection.z= input.y;
            controller.Move(transform.TransformDirection(moveDirection)* Speed * Time.deltaTime);
        }
        else
        {
            if (!isGrounded)
        {
            playerVelocity.y -= 40f * Time.deltaTime; // extra gravity while sliding in air
        }   

            //decelerate slide
            currentSlideSpeed -= SlideDeceleration * Time.deltaTime;

            if (currentSlideSpeed <= SlideStopThreshold)
            {
                StopSlide();
            }
            else
            {
                controller.Move(slideDirection * currentSlideSpeed * Time.deltaTime);
            }
        }
        
        if (isGrappling)
        {
            ApplyGrapplePull();
            controller.Move(playerVelocity * Time.deltaTime);
        }
        else
        {
            //gravity
            // Drain horizontal velocity when not grappling
            playerVelocity.x = Mathf.Lerp(playerVelocity.x, 0f, Time.deltaTime * 1.2f);
            playerVelocity.z = Mathf.Lerp(playerVelocity.z, 0f, Time.deltaTime * 1.2f);
            playerVelocity.y += Gravity*Time.deltaTime;
            if(isGrounded && playerVelocity.y < 0)
            {
                playerVelocity.y= -1f;
            }
            controller.Move(playerVelocity * Time.deltaTime);
        }
    }

   public void StartSlide()
{
    // Grapple cancel
    if (!isGrounded)
    {
        if (isGrappling)
        {
            isDiving = true;
            StopGrapple();
        }
        else
        {
            isDiving = true;
        }
        playerVelocity = new Vector3(0, -30f, 0);
        controller.height = SlideCrouchHeight;
        controller.center = new Vector3(0, SlideCrouchHeight / 2f, 0);
        return;
    }


    // Normal ground slide
    if (isSliding) return;
    isSliding = true;
    currentSlideSpeed = SlideSpeed;
    slideDirection = transform.forward;
    controller.height = SlideCrouchHeight;
    controller.center = new Vector3(0, SlideCrouchHeight / 2f, 0);
    playerVelocity.y = -20f;
}
    public void StopSlide()
    {
        if (!isSliding) return;

        isSliding = false;
        currentSlideSpeed = 0f;

        // Restore collider
        controller.height = originalHeight;
        controller.center = originalCenter;
    }
    public void ProcessJump()
    {
        if (isGrounded)
        {
            if (isSliding) StopSlide();
            playerVelocity.y = Mathf.Sqrt(JumpHigh * -2f * Gravity);    
        }
        else if (isGrappling)
        {
            // Release grapple mid-air and keep all momentum (titanfall style, allows slingshots)
            StopGrapple();
        }
        
    }
    public void StartGrapple()
{
    int layerMask = ~LayerMask.GetMask("NoGrapple");
    Ray ray = playerCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

    if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
    {
        //Debug.Log("Grappled: " + hit.collider.gameObject.name + " layer: " + hit.collider.gameObject.layer);
        playerVelocity = Vector3.zero;
        isGrappling = true;
        grapplePoint = hit.point;
        ropeRenderer.enabled = true;
    }
}
    public void StopGrapple()
    {
        
        isGrappling = false;
        ropeRenderer.enabled = false;
        // Keep momentum but cap it so you don't get launched into orbit
        if (playerVelocity.magnitude > Speed * 2f)
            playerVelocity = playerVelocity.normalized * Speed * 2f;
    }

    private void ApplyGrapplePull()
    {
        Vector3 directionToPoint = (grapplePoint - transform.position).normalized;
        float distanceToPoint = Vector3.Distance(transform.position, grapplePoint);

        // Auto-release when close enough
        if (distanceToPoint <= GrappleArrivalDistance)
        {
            StopGrapple();
            return;
        }

        // Add velocity toward grapple point — gravity is replaced by pull while grappling
        playerVelocity += directionToPoint * GrapplePullForce * Time.deltaTime;

        // Clamp to max speed so it doesn't accelerate indefinitely
        if (playerVelocity.magnitude > GrappleMaxSpeed)
            playerVelocity = playerVelocity.normalized * GrappleMaxSpeed;
    }

   private void UpdateRope()
{
    // Offset to simulate rope coming from left hand/wrist
    Vector3 offset = playerCamera.transform.right * -0.3f  // left
                   + playerCamera.transform.up    * -0.2f  // slightly down
                   + playerCamera.transform.forward * 0.5f; // in front of clip plane

    ropeRenderer.SetPosition(0, playerCamera.transform.position + offset);
    ropeRenderer.SetPosition(1, grapplePoint);
}
}

