using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

[CreateAssetMenu]
public class TetrominoData : ScriptableObject
{
    public MinoType MinoType;
    public BoundaryData HorizonBoundary;

    [Serializable]
    public struct BoundaryData
    {
        public Vector2 R0;
        public Vector2 R1;
        public Vector2 R2;
        public Vector2 R3;
    }

}
