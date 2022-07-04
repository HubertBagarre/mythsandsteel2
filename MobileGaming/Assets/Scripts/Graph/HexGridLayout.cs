using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class HexGridLayout : MonoBehaviour
{
   [Header("Grid Settings")] public Vector2Int gridSize;
   
   [Header("Tile Settings")]
   public float outerSize = 1f;
   public float innerSize = 0f;
   public float height = 1f;
   public bool isFlatTopped;
   public Material material;
   private float squareRt3;

   private void Start()
   {
      squareRt3 = Mathf.Sqrt(3);
   }
   
   [Button]
   private void LayoutGrid()
   {
      for (var y = 0; y < gridSize.y; y++)
      {
         for (var x = 0; x < gridSize.x; x++)
         {
            var tile = new GameObject($"Hex {x},{y}", typeof(HexRenderer));
            tile.transform.position = GetPositionForHexFromCoordinate(new Vector2Int(x, y));

            var hexRenderer = tile.GetComponent<HexRenderer>();
            hexRenderer.outerSize = outerSize;
            hexRenderer.innerSize = innerSize;
            hexRenderer.height = height;
            hexRenderer.isFlatTopped = isFlatTopped;
            hexRenderer.SetMaterial(material);
            hexRenderer.DrawMesh();
            
            tile.transform.SetParent(transform,true);
         }
      }
   }

   private Vector3 GetPositionForHexFromCoordinate(Vector2Int coordinate)
   {
      var column = coordinate.x;
      var row = coordinate.y;
      float width;
      float height;
      float xPosition;
      float yPosition;
      bool shouldOffset;
      float horizontalDistance;
      float verticalDistance;
      float offset;
      var size = outerSize;

      if (!isFlatTopped)
      {
         shouldOffset = (row % 2) == 0;

         width = squareRt3 * size;
         height = 2f * size;

         horizontalDistance = width;
         verticalDistance = height * (3f / 4f);

         offset = (shouldOffset) ? width / 2 : 0;

         xPosition = (column * (horizontalDistance)) + offset;
         yPosition = (row * verticalDistance);
      }
      else
      {
         shouldOffset = (column % 2) == 0;
         
         height = squareRt3 * size;
         width = 2f * size;
         
         horizontalDistance =  height * (3f / 4f);;
         verticalDistance = width;
         
         offset = (shouldOffset) ? height / 2 : 0;

         xPosition = (column * (horizontalDistance));
         yPosition = (row * verticalDistance) - offset;


      }

      return new Vector3(xPosition, 0, -yPosition);

   }
}
