using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class BoardController : MonoBehaviour
{
    private const int INITIAL_POOL_SIZE = 200;
    private const int BOARD_WIDTH = 10;
    private const float MINO_PIXEL_SIZE = 65.0f;
    readonly private Cell EMPTY_CELL = new();

    [SerializeField] private Transform _TetrominosParent;

    private Queue<string> _spawnOrder = new Queue<string>();
    private List<Cell> _minos = new();
    private List<int> _lineToRemove = new();
    private CellPool _cellPool;

    private class CellPool
    {
        private Stack<Cell> pool;
        private int capacity;

        public CellPool(int initialCapacity)
        {
            capacity = initialCapacity;
            pool = new Stack<Cell>(initialCapacity);
            InitializePool();
        }

        private void InitializePool()
        {
            for (int i = 0; i < capacity; i++)
                pool.Push(new Cell());
        }

        public Cell Get(int x ,int y ,GameObject go)
        {
            Cell cell = pool.Count() > 0 ? pool.Pop() : new Cell();
            cell.Initialize(x, y, go);
            return cell;
        }

        public void Return(Cell cell)
        {
            if (pool.Count() < capacity)
                pool.Push(cell);
        }

    }

    private struct Cell
    {
        private int x;
        private int y;
        private GameObject go;

        public int X { get => x; set => x = value; }
        public int Y { get => y; set => y = value; }
        public GameObject Go { get => go; set => go = value; }

        public void Initialize(int x, int y, GameObject go)
        {
            this.x = x;
            this.y = y;
            this.go = go;
        }

        public void DecreaseY() { y--; }
    }


    private void Awake()
    {
        _cellPool = new CellPool(INITIAL_POOL_SIZE);
    }

    // pos 위치에서 바닥찾기
    public int FindLand(Vector2Int pos)
    {
        int maxY = 0;
        foreach(var mino in _minos)
        {
            bool checkLand = mino.X == pos.x && mino.Y < pos.y && mino.Y > maxY;
            if (checkLand)
                maxY = mino.Y;
        }

        return maxY;
    }

    //회전시 테트로미노가 벽에 부딪치는지 체크
    public bool CheckOverlapWall(Vector2Int[] minos)
    {
        bool check = false;
        foreach(var mino in minos)
        {
            int x = mino.x;
            int y = mino.y;
            Cell result = _minos.Find(m => m.X == x && m.Y == y);
            if (!result.Equals(EMPTY_CELL))
            {
                check = true;
                break;
            }
        }

        return check;
    }

    //테트로미노가 놓인 후 각 셀에 놓인 오브젝트를 list에 추가
    public void AddMinosList(Vector2Int pos, GameObject go)
    {
        Cell cell = _cellPool.Get(pos.x, pos.y, go);
        _minos.Add(cell);
        CheckLine();
    }

    //놓인 후 모든 줄 체크
    private void CheckLine()
    {
        var lineCount = _minos.GroupBy(m => m.Y)
                              .Select(g => new { Y = g.Key, Count = g.Count() });

        _lineToRemove.Clear();
        foreach (var line in lineCount)
        {
            if(line.Count == BOARD_WIDTH)
            {
                RemoveMinos(line.Y);
                _lineToRemove.Add(line.Y);
            }
        }

        if (_lineToRemove.Count() > 0)
        {
            LineDown();
        }
    }

    //한 라인에 10개의 오브젝트가 존재할 시 제거 
    private void RemoveMinos(int y)
    {
        var minosToRemove = _minos.Where(m => m.Y == y).ToList();
        foreach(var mino in minosToRemove)
        {
            //todo - 오브젝트풀링
            if (mino.Go != null) Destroy(mino.Go);
            _cellPool.Return(mino);
            _minos.Remove(mino);
        }
    }

    //제거 후 위에 남은 테트로미노들 아래로 이동
    private void LineDown()
    {
        _lineToRemove.Sort((a, b) => { return a.CompareTo(b); });

        foreach(var y in _lineToRemove)
        {
            var minosToMove = _minos.Where(m => m.Y > y).ToList();
            float destPosY = y - MINO_PIXEL_SIZE;
            foreach(var mino in minosToMove)
            {
                if(mino.Go != null)
                {
                    mino.Go.GetComponent<RectTransform>().DOAnchorPosY(destPosY, 0);
                    mino.DecreaseY();
                }
            }
        }
    }
    
    #region Spawn
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

    private void TetrominoSpawn() 
    {
        if(_spawnOrder.Count <= 3)
        {
            GetSpawnOrderSevenBag();
        }
        string path = $"{ _spawnOrder.Dequeue() }_Tetromino";

        //todo -오브젝트풀링
        Managers.Resource.Instantiate(path, _TetrominosParent);
    }
    #endregion


}
