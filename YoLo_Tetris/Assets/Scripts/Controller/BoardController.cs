using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    [SerializeField] private Transform _TetrominosParent;

    private Queue<string> _spawnOrder = new Queue<string>();
    private List<Cell> _minos = new();

    private struct Cell
    {
        int id;
        GameObject mino;
    }

    private void Start()
    {

    }

    public int FindLand(int id) // or bool IsLanding()
    {

        return 0;
    }

    private void DicAdd(Vector2 position, GameObject go)
    {

    }

    private void GetSpawnOrderSevenBag()
    {
        char[] bag = "IJLOSTZ".ToCharArray();
        for (int i = 0; i < 7; i++)
        {
            int random = Random.Range(0, bag.Length);
            _spawnOrder.Enqueue(bag[random].ToString());
            bag = bag.Where(str => str != bag[random]).ToArray();
        }

    }

    //풀링으로 바꿀 예정
    private void TetrominoSpawn() 
    {
        if(_spawnOrder.Count <= 3)
        {
            GetSpawnOrderSevenBag();
        }
        string path = _spawnOrder.Peek() + "_Tetromino";
        _spawnOrder.Dequeue();

        Managers.Resource.Instantiate(path, _TetrominosParent);
    }

    private void MinoRemove()
    {

    }

    private void MinoLineDown()
    {

    }
    
}
