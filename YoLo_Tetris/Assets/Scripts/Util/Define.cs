using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define 
{
    public enum Scene
    {
        Unknown,
        Title,
        Main,
        Game,
    }

    public enum TouchEvent
    {
        None,
        Click,
        Drag,

    }

    public enum MoveDir
    {
        Idle,
        SlowDown,
        QuickDown,
        HorizonDir,
        VerticalDir,

    }

    public enum HorizonDir
    {
        Idle,
        Left,
        Right,
    }

    public enum RotationDir
    {
        Idle,
        Left,
        Right,
        
    }

    public enum MinoType
    {
        Unknown,
        O = 79,
        I = 73,
        S = 83,
        Z = 90,
        J = 74,
        L = 76,
        T = 84,

    }

    public enum MinoRotationState
    {
        R0, //초기상태 0 (0)
        R1, //회전 90   (90)
        R2, //회전 180  (-180)
        R3, //회전 270  (-90)

    }

    


}
