using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    private Dictionary<Vector2,GameObject> _minoDic;

    public float LandSurvey(float x)
    {
        int roofY = 0;
        foreach (Vector2 key in _minoDic.Keys)
        {
            if(key.x == x)
            {
                roofY = (int)(roofY < key.y ? key.y : roofY);
            }
        }

        return roofY;
    }

    public void MinoAdd(Vector2 position, GameObject go)
    {

    }

    private void MinoRemove()
    {

    }

    private void MinoLineDown()
    {

    }
    
}
