using UnityEngine;
using System;

namespace DGD306.Character
{
    [Serializable]
    public class BoxData
    {
        [Header("Box Transform")]
        public Vector2 offset = Vector2.zero;
        public Vector2 size = Vector2.one;
        
        [Header("Timing")]
        public float startFrame = 0f;
        public float endFrame = 10f;
        
        [Header("Combat")]
        public float damage = 10f;
        public LayerMask targetLayers = -1;
        
        [Header("Debug")]
        public Color debugColor = Color.red;
        public bool showInEditor = true;
    }

    [Serializable]
    public class StateHitboxData
    {
        public string stateName;
        public BoxData[] hitboxes;
        
        public StateHitboxData(string name)
        {
            stateName = name;
            hitboxes = new BoxData[0];
        }
    }

    [Serializable]
    public class StateHurtboxData
    {
        public string stateName;
        public BoxData[] hurtboxes;
        
        public StateHurtboxData(string name)
        {
            stateName = name;
            hurtboxes = new BoxData[0];
        }
    }
} 