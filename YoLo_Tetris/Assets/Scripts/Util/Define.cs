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
        Left,
        Right,  
        SlowDown,
        QuickDown,

    }

    public enum RotationDir
    {
        Idle,
        Left,
        Right,
        
    }

    


}
