using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Data 
{
    private static readonly float Cos = Mathf.Cos(Mathf.PI / 2f);
    private static readonly float Sin = Mathf.Sin(Mathf.PI / 2f);
    public static readonly float[] RotationMatrix = new float[] { Cos, Sin, -Sin, Cos };

    //I_Tetromino의 회전 기준점만 달라서
    public static readonly Vector2Int[,] RotationMatrixI = {
        {new Vector2Int(2,  -1), new Vector2Int(1,  0), new Vector2Int(0,  1), new Vector2Int(-1,  2) }, // 0>>1
        {new Vector2Int(-2,  1), new Vector2Int(-1, 0), new Vector2Int(0, -1), new Vector2Int(1,  -2) }, // 1>>0
        {new Vector2Int(1,   2), new Vector2Int(0,  1), new Vector2Int(-1, 0), new Vector2Int(-2, -1) }, // 1>>2
        {new Vector2Int(-1, -2), new Vector2Int(0, -1), new Vector2Int(1,  0), new Vector2Int(2,   1) }, // 2>>1
        {new Vector2Int(-2,  1), new Vector2Int(-1, 0), new Vector2Int(0, -1), new Vector2Int(1,  -2) }, // 2>>3                                                                                          
        {new Vector2Int(2,  -1), new Vector2Int(1,  0), new Vector2Int(0,  1), new Vector2Int(-1,  2) }, // 3>>2
        {new Vector2Int(-1, -2), new Vector2Int(0, -1), new Vector2Int(1,  0), new Vector2Int(2,   1) }, // 3>>0
        {new Vector2Int(1,   2), new Vector2Int(0,  1), new Vector2Int(-1, 0), new Vector2Int(-2, -1) }, // 0>>3      
    };

    //test 5time
    private static readonly Vector2Int[,] WallKicksI = new Vector2Int[,] {
        { new Vector2Int(0, 0), new Vector2Int(-2, 0), new Vector2Int( 1, 0), new Vector2Int(-2,-1), new Vector2Int( 1, 2) },  // 0>>1
        { new Vector2Int(0, 0), new Vector2Int( 2, 0), new Vector2Int(-1, 0), new Vector2Int( 2, 1), new Vector2Int(-1,-2) },  // 1>>0
        { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int( 2, 0), new Vector2Int(-1, 2), new Vector2Int( 2,-1) },  // 1>>2
        { new Vector2Int(0, 0), new Vector2Int( 1, 0), new Vector2Int(-2, 0), new Vector2Int( 1,-2), new Vector2Int(-2, 1) },  // 2>>1
        { new Vector2Int(0, 0), new Vector2Int( 2, 0), new Vector2Int(-1, 0), new Vector2Int( 2, 1), new Vector2Int(-1,-2) },  // 2>>3
        { new Vector2Int(0, 0), new Vector2Int(-2, 0), new Vector2Int( 1, 0), new Vector2Int(-2,-1), new Vector2Int( 1, 2) },  // 3>>2
        { new Vector2Int(0, 0), new Vector2Int( 1, 0), new Vector2Int(-2, 0), new Vector2Int( 1,-2), new Vector2Int(-2, 1) },  // 3>>0
        { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int( 2, 0), new Vector2Int(-1, 2), new Vector2Int( 2,-1) },  // 0>>3
    };

    //test 5time
    private static readonly Vector2Int[,] WallKicksJLOSTZ = new Vector2Int[,] {
        { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int(-1, 1), new Vector2Int(0,-2), new Vector2Int(-1,-2) },  // 0>>1
        { new Vector2Int(0, 0), new Vector2Int( 1, 0), new Vector2Int( 1,-1), new Vector2Int(0, 2), new Vector2Int( 1, 2) },  // 1>>0
        { new Vector2Int(0, 0), new Vector2Int( 1, 0), new Vector2Int( 1,-1), new Vector2Int(0, 2), new Vector2Int( 1, 2) },  // 1>>2
        { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int(-1, 1), new Vector2Int(0,-2), new Vector2Int(-1,-2) },  // 2>>1
        { new Vector2Int(0, 0), new Vector2Int( 1, 0), new Vector2Int( 1, 1), new Vector2Int(0,-2), new Vector2Int( 1,-2) },  // 2>>3
        { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int(-1,-1), new Vector2Int(0, 2), new Vector2Int(-1, 2) },  // 3>>2
        { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int(-1,-1), new Vector2Int(0, 2), new Vector2Int(-1, 2) },  // 3>>0
        { new Vector2Int(0, 0), new Vector2Int( 1, 0), new Vector2Int( 1, 1), new Vector2Int(0,-2), new Vector2Int( 1,-2) },  // 0>>3
    };

    public static readonly Dictionary<TetrominoType, Vector2Int[,]> WallKicks = new Dictionary<TetrominoType, Vector2Int[,]>()
    {
        { TetrominoType.I, WallKicksI },
        { TetrominoType.J, WallKicksJLOSTZ },
        { TetrominoType.L, WallKicksJLOSTZ },
        { TetrominoType.O, WallKicksJLOSTZ },
        { TetrominoType.S, WallKicksJLOSTZ },
        { TetrominoType.T, WallKicksJLOSTZ },
        { TetrominoType.Z, WallKicksJLOSTZ },
    };

}
