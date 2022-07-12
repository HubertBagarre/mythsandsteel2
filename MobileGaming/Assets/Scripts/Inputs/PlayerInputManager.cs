using System;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-1)]
public class PlayerInputManager : MonoBehaviour
{
    private TouchControls touchControls;

    [SerializeField] private Animator cursorAnimator;
    [SerializeField] private Transform cursorTransform;

    public delegate void StartTouchEvent(Vector2 position, float time);
    public event StartTouchEvent OnStartTouch;
    public delegate void EndTouchEvent(Vector2 position, float time);
    public event EndTouchEvent OnEndTouch;

    public static PlayerInputManager instance;
    private static readonly int Trigger = Animator.StringToHash("Trigger");

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this;
        
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
        var position = touchControls.Touch.TouchPosition.ReadValue<Vector2>();
        cursorAnimator.ResetTrigger(Trigger);
        cursorAnimator.SetTrigger(Trigger);
        cursorTransform.position = position;
        OnStartTouch?.Invoke(position, (float) context.startTime);
    }
    
    private void EndTouch(InputAction.CallbackContext context)
    {
        OnEndTouch?.Invoke(touchControls.Touch.TouchPosition.ReadValue<Vector2>(), (float) context.time);
    }
}
