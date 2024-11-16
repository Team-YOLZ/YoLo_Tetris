using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class TetrominoPool : MonoBehaviour
{
    class Pool
    {
        public Transform Root { get; set; }
        private Queue<TetrominoController> TETROMINO_POOL = new();

        public void InitalizePool(string name, int size = 7)
        {
            for (int i = 0; i<size; i++)
            {
                TETROMINO_POOL.Enqueue(CreateTetromino(name));
            }
        }

        private TetrominoController CreateTetromino(string name)
        {
            var obj = Managers.Resource.Instantiate($"Prefabs/{name}_Tetromino").GetComponent<TetrominoController>();
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(Root);

            return obj;
        }

        internal TetrominoController OnGetFromPool(string name)
        {
            var obj = TETROMINO_POOL.Count > 0 ? TETROMINO_POOL.Dequeue() : CreateTetromino(name);
            obj.gameObject.SetActive(true);
            return obj;
        }

        internal void OnReleaseToPool(TetrominoController tc)
        {
            tc.gameObject.SetActive(false);
            TETROMINO_POOL.Enqueue(tc);
        }
    }

    private Dictionary<string, Pool> _pools = new Dictionary<string, Pool>();

    private static TetrominoPool _instance;
    public static TetrominoPool Instance
    {
        get
        {
            if(_instance == null)
            {
                GameObject go = GameObject.Find("Tetrominos");
                _instance = go.GetComponent<TetrominoPool>();
            }
            return _instance;
        }
    }

    private void Start()
    {
        InitializePools();
    }

    private void InitializePools()
    {
        for(int i = 1; i <= 7; i++)
        {
            CreatePools(((TetrominoType)i).ToString());
        }
    }

    private void CreatePools(string name)
    {
        Pool pool = new();
        pool.Root = transform;
        pool.InitalizePool(name);

        _pools.Add(name, pool);
    }

    public static TetrominoController GetTetromino(string name) => Instance._pools[name].OnGetFromPool(name);

    public static void ReturnTetromino(TetrominoController tc)
    {
        string name = tc.gameObject.name.Substring(0, 1);
        Instance._pools[name].OnReleaseToPool(tc);
    }


}
