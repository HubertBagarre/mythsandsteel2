using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tiles;

[CreateAssetMenu(menuName = "Scriptables/Board")]
public class ScriptableBoard : ScriptableObject
{
    public BaseTile[,] tiles = new BaseTile[10,10];
}
