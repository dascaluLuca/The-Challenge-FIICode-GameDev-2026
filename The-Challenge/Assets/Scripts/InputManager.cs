using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private PlayerInput playerInput;
    private PlayerInput.OnFootActions onFoot;
    private PlayerMotor motor;
    private PlayerLook look;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        playerInput = new PlayerInput();
        onFoot=playerInput.OnFoot;
        motor = GetComponent<PlayerMotor>();
        look = GetComponent<PlayerLook>();
        onFoot.Grapple.started  += _ => motor.StartGrapple();
        onFoot.Grapple.canceled += _ => motor.StopGrapple();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //tell PlayerMotor to move using the value from the movement action
        motor.ProcessMove(onFoot.Movement.ReadValue<Vector2>());
    }
    void LateUpdate()
    {
        look.ProcessLook(onFoot.Look.ReadValue<Vector2>());
    }
    void Update()
    {
        if (onFoot.Jump.WasPressedThisFrame())
        {
            motor.ProcessJump();
        }
        if (onFoot.Slide.WasPressedThisFrame())
        {
            motor.StartSlide();
        }
        
    }

    private void OnEnable()
    {
        onFoot.Enable();
    }
    private void OnDisable()
    {
        onFoot.Disable();
    }
}
