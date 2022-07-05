using System;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-1)]
public class PlayerInputManager : MonoBehaviour
{
    private TouchControls touchControls;

    public delegate void StartTouchEvent(Vector2 position, float time);
    public event StartTouchEvent OnStartTouch;
    public delegate void EndTouchEvent(Vector2 position, float time);
    public event EndTouchEvent OnEndTouch;

    public static PlayerInputManager singleton;
    
    private void Awake()
    {
        if (singleton != null)
        {
            Destroy(this);
            return;
        }
        singleton = this;
        
        touchControls = new TouchControls();
    }

    private void OnEnable()
    {
        touchControls.Enable();
    }

    private void OnDisable()
    {
        touchControls.Disable();
    }

    private void Start()
    {
        touchControls.Touch.TouchPress.started += StartTouch;
        touchControls.Touch.TouchPress.canceled += EndTouch;
    }

    private void StartTouch(InputAction.CallbackContext context)
    {
        Debug.Log($"Touch Stared {touchControls.Touch.TouchPosition.ReadValue<Vector2>()}");
        OnStartTouch?.Invoke(touchControls.Touch.TouchPosition.ReadValue<Vector2>(), (float) context.startTime);
    }
    
    private void EndTouch(InputAction.CallbackContext context)
    {
        Debug.Log($"Touch Ended {touchControls.Touch.TouchPosition.ReadValue<Vector2>()}");
        OnEndTouch?.Invoke(touchControls.Touch.TouchPosition.ReadValue<Vector2>(), (float) context.time);
    }
}
