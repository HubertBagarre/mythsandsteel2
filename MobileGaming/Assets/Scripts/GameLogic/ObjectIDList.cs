using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectIDList : MonoBehaviour
{
    public List<ScriptableUnit> units;
    public List<ScriptableTile> tiles;

    public static ObjectIDList instance;
    
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }

        instance = this;
        
        DontDestroyOnLoad(this);
    }
}
