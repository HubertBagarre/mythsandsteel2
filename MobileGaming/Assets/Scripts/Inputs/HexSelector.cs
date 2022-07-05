using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexSelector : MonoBehaviour
{
    private PlayerInputManager inputManager;
    [SerializeField] private LayerMask hexLayer;
    private Camera cam;

    [Header("Managers")]
    [SerializeField] private HexGrid hexGrid;
    
    [Header("Debug Info")]
    [SerializeField] private Hex currentHex;

    private void Awake()
    {
        inputManager = PlayerInputManager.instance;
        cam = Camera.main;
    }

    private void OnEnable()
    {
        inputManager.OnStartTouch += ShootRay;
    }

    private void OnDisable()
    {
        inputManager.OnEndTouch -= ShootRay;
        hexLayer = ~hexLayer;
    }

    private void ShootRay(Vector2 screenPosition,float time)
    {
        var ray = cam.ScreenPointToRay(screenPosition);

        if (!Physics.Raycast(ray, out var hit,hexLayer)) return;
        var objectHit = hit.transform;
        

        if(currentHex != null) Debug.Log(Hex.DistanceBetween(currentHex,objectHit.GetComponent<Hex>()));
        currentHex = objectHit.GetComponent<Hex>();
    }
}
