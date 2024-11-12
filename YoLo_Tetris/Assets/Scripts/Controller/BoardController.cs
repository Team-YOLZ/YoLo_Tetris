using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    private const int INITIAL_POOL_SIZE = 200;
    private const int BOARD_WIDTH = 10;

    [SerializeField] private Transform _TetrominosParent;

    private Queue<string> _spawnOrder = new Queue<string>();
    private List<Cell> _minos = new();
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
            if (mino.X == pos.x && mino.Y < pos.y && mino.Y > maxY)
                maxY = mino.Y;
        }

        return maxY;
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

        List<int> lineToRemove = new();
        foreach(var line in lineCount)
        {
            if(line.Count == BOARD_WIDTH)
            {
                RemoveMinos(line.Y);
                lineToRemove.Add(line.Y);
            }
        }

        if (lineToRemove.Count() > 0)
        {
            LineDown(lineToRemove);
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
    private void LineDown(List<int> lineToRemove)
    {
        lineToRemove.Sort((a, b) => { return a.CompareTo(b); });

        foreach(var y in lineToRemove)
        {
            var minosToMove = _minos.Where(m => m.Y > y).ToList();
            foreach(var mino in minosToMove)
            {
                if(mino.Go != null)
                {
                    //todo 미노오브젝트 한칸 밑으로 내리기
                    mino.DecreaseY();
                }
            }
        }
    }
    
    #region Create
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
        string path = _spawnOrder.Peek() + "_Tetromino";
        _spawnOrder.Dequeue();

        //todo -오브젝트풀링
        Managers.Resource.Instantiate(path, _TetrominosParent);
    }
    #endregion


}
