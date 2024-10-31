using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinoController : MonoBehaviour
{

    //class SYJ{
    //    string HBD;
    //    Dictionary<string, string> BFF;

    //    public SYJ(){
    //        HBD = "951111";
    //        BFF.Add("진솔", "씨빠브라더~ 한잔해~");
    //        BFF.Add("오현", "햅삐 햅삐~ 해져라");
    //        BFF.Add("하경", "손흥민차은우신용재");
    //    }
    //}

    [SerializeField] private BoardController _boardController;

    [SerializeField] private Vector2 _position; 

    public void MinoMove(int x, int y)
    {
        _position += new Vector2(x, y);
    }

    public void Destroy()
    {
        Managers.Resource.Destroy(gameObject);
    }
}
