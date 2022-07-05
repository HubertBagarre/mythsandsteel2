using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTouch : MonoBehaviour
{
    private PlayerInputManager inputManager;

    private void Awake()
    {
        inputManager = PlayerInputManager.singleton;
    }

    private void OnEnable()
    {
        inputManager.OnStartTouch += Move;
    }

    private void OnDisable()
    {
        inputManager.OnEndTouch -= Move;
    }

    private void Move(Vector2 screenPosition,float time)
    {
        var cam = Camera.main;
        Vector3 worldCoordinates = cam.ViewportToWorldPoint(screenPosition);
        worldCoordinates.z = 0;
        transform.position = worldCoordinates;
    }
}
