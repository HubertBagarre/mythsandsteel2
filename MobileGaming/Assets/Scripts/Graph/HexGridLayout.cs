using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class HexGridLayout : MonoBehaviour
{
   [BoxGroup("Grid Settings")] public Vector2Int gridSize;
   
   [BoxGroup("Tile Settings")]
   public float outerSize = 1f;
   public float innerSize = 0f;
   public float height = 1f;
   public bool isFlatTopped;
   public Material material;

   private void OnEnable()
   {
      LayoutGrid();
   }

   private void OnValidate()
   {
      if(Application.isPlaying) LayoutGrid();
   }

   private void LayoutGrid()
   {
      for (var y = 0; y < gridSize.y; y++)
      {
         for (var x = 0; x < gridSize.x; x++)
         {
            var tile = new GameObject()
         }
      }
   }
}
