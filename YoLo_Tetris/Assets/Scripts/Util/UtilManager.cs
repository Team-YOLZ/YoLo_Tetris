using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilManager
{
    public static Vector2 ConvertScreentoLocalInRect(RectTransform canvas, Vector2 position)
    {
        Vector2 canvasVec = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, position, Camera.main, out canvasVec);
        return canvasVec;
    }
}
