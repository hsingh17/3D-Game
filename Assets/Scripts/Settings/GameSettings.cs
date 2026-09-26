using System;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    [Serializable]
    public struct MouseSensitivity
    {
        public float x;
        public float y;
    }

    // Audio
    // Graphics

    [Header("Gameplay/Controls")]
    [SerializeField]
    private MouseSensitivity sensitivity;

    public MouseSensitivity Sensitivity
    {
        get { return sensitivity; }
        set { sensitivity = value; }
    }

    private static GameSettings instance;

    private void Awake()
    {
        instance = instance != null ? instance : this;
    }

    public static GameSettings Instance()
    {
        return instance;
    }
}
