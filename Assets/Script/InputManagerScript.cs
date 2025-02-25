using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManagerScript : MonoBehaviour
{
    public static PlayerInput PlayerInput; 
    public static Vector2 Movement; 
    public static bool  JumpWasPressed;
    public static bool JumpIsHeld; 
    public static bool JumpWasReleased; 
    public static bool RunIsHeld; 
    
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _runAction;

    void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();
        // PlayerInput = FindObjectOfType<PlayerInput>();
        if (PlayerInput == null)
        {
            Debug.LogError("PlayerInput component is missing from this GameObject!");
            return;
        }
        _moveAction = PlayerInput.actions["Move"];
        _jumpAction = PlayerInput.actions["Jump"];
        _runAction = PlayerInput.actions["Run"];

    }

    void Update()
    {
        Movement = _moveAction.ReadValue<Vector2>();
        
        JumpWasPressed = _jumpAction.WasPressedThisFrame();
        JumpIsHeld = _jumpAction.IsPressed();
        JumpWasReleased = _jumpAction.WasReleasedThisFrame();

        RunIsHeld = _runAction.IsPressed();
    }


}
