using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(menuName = "Scriptables/Powerpoint Slide")]
public class ScriptablePowerPoint : ScriptableObject
{
    [Serializable]
    public class Slide
    {
        public string title;
        [TextArea(7,7)]public string text;
        public Sprite image;
        public VideoClip video;
        
    }

    public List<Slide> slides;
}
