using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using UniRx;
using Cysharp.Threading.Tasks;
using static Define;

public class TetrominoController : MonoBehaviour
{
    private const float MINO_PIXEL_SIZE = 65.0f; //테트로미노 65x65
    private const float QUICK_DOWN_FORCE = 65.0f; //테트로미노가 바로 떨어지게하는 힘의 양
    private const float AUTO_DOWN_INTERVAL_TIME = 3.0f;
    private const float SLOW_DOWN_INTERVAL_TIME = 0.2f;
    private const float QUICK_DOWN_INTERVAL_TIME = 0f;
    readonly private float[] MATRIX = Data.RotationMatrix;
    readonly private Vector2Int[,] I_MATRIX = Data.RotationMatrixI;
    readonly private Vector3 ROTATION_VECTOR = new Vector3(0, 0, 90);

    private CompositeDisposable _moveSubscriber = new();
    //private IDisposable _autoMoveSubscription;

    [SerializeField] private RectTransform _tetromino;
    [SerializeField] private RectTransform _gameMainCanvas;
    [SerializeField] private MinoController[] _minoControllers;
    [SerializeField] private BoardController _boardController;
    [SerializeField] private RectTransformResetter _rectTransformResetter;

    private float _intervalTime = 0f;
    private bool _isForceMoveDown = false;
    private bool _isDeleyLanding = false;
    private bool _isCompletionLanding = false;
    private Vector2 _curDragPosition = Vector2.zero;
    private Vector2 _PrevDragPosition = Vector2.zero;
    private Vector2Int[] _nextCell = new Vector2Int[4];
    private Vector2Int[] _curCell = new Vector2Int[4];
    private Vector2Int[] _prevCell = new Vector2Int[4];
    private Vector2Int[,] _wallKick;

    private MoveDir _moveDir = MoveDir.Idle;
    private ClickHorizonDir _clickHorizonDir = ClickHorizonDir.Idle; 
    private DragHorizonDir _dragHorizonDir = DragHorizonDir.Idle;
    private TetrominoType _tetrominoType = TetrominoType.Unknown;
    private RotationState _rotationState = RotationState.R0;

    private void Awake()
    {
        if(_tetrominoType == TetrominoType.Unknown)
        {
            var name =  (TetrominoType)(int)gameObject.name[0];
            _tetrominoType = name;
        }
        _wallKick = Data.WallKicks[_tetrominoType];

        //InputManager.Instance.TouchAction += TouchState;
        //InputManager.Instance.BeginDragAction += GetBeginDragPosition;
        //InputManager.Instance.IsEndDragAction += IsEndDrag;

        //for(int i = 0; i< _minoControllers.Length; i++)
        //{
        //    _curCell[i] = _minoControllers[i]._position;
        //    _prevCell[i] = _minoControllers[i]._position;
        //}

    }

    //private void Start()
    //{
    //    ProcessAsyncTaskDeleyLanding().Forget();
    //    ProcessAsyncTaskCompleteLanding().Forget();
    //    Invoke(nameof(StartMove), 0);
    //}

    private void OnEnable()
    {
        Initialize();
    }

    private void OnDisable()
    {
        ClearAll();
    }

    private void Initialize()
    {
        InputManager.Instance.TouchAction += TouchState;
        InputManager.Instance.BeginDragAction += GetBeginDragPosition;
        InputManager.Instance.IsEndDragAction += IsEndDrag;

        _isForceMoveDown = false;
        _isDeleyLanding = false;
        _isCompletionLanding = false;
        _rotationState = RotationState.R0;

        _rectTransformResetter.ResetToInitialState();
        for (int i = 0; i < _minoControllers.Length; i++)
        {
            _minoControllers[i].GetComponent<RectTransformResetter>().ResetToInitialState();
            _curCell[i] = _minoControllers[i]._position;
            _prevCell[i] = _minoControllers[i]._position;
        }

        ProcessAsyncTaskDeleyLanding().Forget();
        ProcessAsyncTaskCompleteLanding().Forget();
        Invoke(nameof(StartMove), 0);
    }

    private void ClearAll()
    {
        InputManager.Instance.TouchAction -= TouchState;
        InputManager.Instance.BeginDragAction -= GetBeginDragPosition;
        InputManager.Instance.IsEndDragAction -= IsEndDrag;
        _moveSubscriber.Dispose();
    }

    //아래로 한 칸씩 이동
    private void OneStepDownMove()
    {
        NextCellInitailize(0, -1);
        bool checkWall = _boardController.CheckOverlapWall(_nextCell);

        if (!checkWall)
        {
            float destPosY = _tetromino.anchoredPosition.y - MINO_PIXEL_SIZE;
            _tetromino.DOAnchorPosY(destPosY, 0);
            ApplyCell(0, -1);
        }
        else if(checkWall && !_isDeleyLanding) _isDeleyLanding = true;
        else if (checkWall && _isDeleyLanding) _isCompletionLanding = true;
    }

    // 좌/우 한 칸씩 이동
    private void OneStepHorizonMove()
    {
        int direction = _dragHorizonDir == DragHorizonDir.Left ? -1 : 1;
        NextCellInitailize(direction, 0);

        if (!_boardController.CheckOverlapWall(_nextCell))
        {
            float destPosX = _tetromino.anchoredPosition.x + MINO_PIXEL_SIZE * direction;
            _tetromino.DOAnchorPosX(destPosX, 0);
            ApplyCell(direction, 0);
        }

        _PrevDragPosition = _curDragPosition;
    }

    private void NextCellInitailize(int dirX, int dirY)
    {
        for (int i = 0; i < _minoControllers.Length; i++)
        {
            _nextCell[i].x = _curCell[i].x + dirX;
            _nextCell[i].y = _curCell[i].y + dirY;
        }
    }

    private void ApplyCell(int dirX, int dirY)
    {
        for (int i = 0; i < _minoControllers.Length; i++)
        {
            _curCell[i].x += dirX;
            _curCell[i].y += dirY;
            _prevCell[i].x += dirX;
            _prevCell[i].y += dirY;
        }
    }

    // input event
    void TouchState(TouchEvent touchEvent, ClickHorizonDir clickHorizonDir, Vector2 position, Vector2 delta)
    {
        switch (touchEvent)
        {
            case TouchEvent.Click:
                if (_tetrominoType == TetrominoType.O) break;

                _clickHorizonDir = clickHorizonDir;
                _rotationState = GetRotationState();
                Rotate();
                break;

            case TouchEvent.Drag:
                _moveDir = GetMoveDir(delta);
                UpdateMove(position, delta);
                break;
        }
    }

    #region Click Method
    RotationState GetRotationState()
    {
        var state = _rotationState;
        switch (_clickHorizonDir)
        {
            case ClickHorizonDir.Left:
                int leftRotation = (int)_rotationState + 1 == 4 ? 0 : (int)_rotationState + 1;
                state = (RotationState)leftRotation;
                break;
            case ClickHorizonDir.Right:
                int rightRotation = (int)_rotationState - 1 == -1 ? 3 : (int)_rotationState - 1;
                state = (RotationState)rightRotation;
                break;
        }
        return state;
    }
    void Rotate()
    {
        int wallKickIndex = GetWallKickIndex();
        ApplyRotationMatrix(wallKickIndex);

        int testIndex = GetWallKickTestIndex(wallKickIndex);

        //Unable to rotate
        if (testIndex == -1)
            Array.Copy(_prevCell, _curCell, _prevCell.GetLength(0));
        else
        {
            Array.Copy(_curCell, _prevCell, _curCell.GetLength(0));
            UpdateRotate(wallKickIndex, testIndex);
        }
    }

    int GetWallKickIndex()
    {
        int index = -1;
        switch (_rotationState)
        {
            case RotationState.R0:
                index = _clickHorizonDir == ClickHorizonDir.Left ? 6 : 1;
                break;
            case RotationState.R1:
                index = _clickHorizonDir == ClickHorizonDir.Left ? 0 : 3;
                break;
            case RotationState.R2:
                index = _clickHorizonDir == ClickHorizonDir.Left ? 2 : 5;
                break;
            case RotationState.R3:
                index = _clickHorizonDir == ClickHorizonDir.Left ? 4 : 7;
                break;
        }
        return index;
    }

    void ApplyRotationMatrix(int rotationIndex)
    {
        int zeroIntervalX = _curCell[0].x;
        int zeroIntervalY = _curCell[0].y;
        int direction = _clickHorizonDir == ClickHorizonDir.Left ? 1 : -1;

        for (int i = 0; i < _minoControllers.Length; i++)
        {
            switch (_tetrominoType)
            {
                case TetrominoType.I:
                    _curCell[i].x += I_MATRIX[rotationIndex, i].x;
                    _curCell[i].y += I_MATRIX[rotationIndex, i].y;
                    break;
                case TetrominoType.O:
                    break;
                default:
                    int zeroBasisX = _curCell[i].x - zeroIntervalX;
                    int zeroBasisY = _curCell[i].y - zeroIntervalY;
                    int conversionX = (int)((zeroBasisX * MATRIX[0] * direction) + (zeroBasisY * MATRIX[2] * direction));
                    int conversionY = (int)((zeroBasisX * MATRIX[1] * direction) + (zeroBasisY * MATRIX[3] * direction));
                    _curCell[i].x = conversionX + zeroIntervalX;
                    _curCell[i].y = conversionY + zeroIntervalY;
                    break;
            }
        }

    }

    int GetWallKickTestIndex(int wallKickIndex)
    {
        for (int i = 0; i < _wallKick.GetLength(1); i++)
        {
            Vector2Int translation = _wallKick[wallKickIndex, i];

            for (int m = 0; m < _curCell.Length; m++)
            {
                _curCell[m] += translation;
            }

            if (!_boardController.CheckOverlapWall(_curCell))
                return i;
            else
            {
                for (int m = 0; m < _curCell.Length; m++)
                {
                    _curCell[m] -= translation;
                }
            }
        }
        return -1;
    }

    //회전
    void UpdateRotate(int wallKickIndex, int testIndex)
    {
        float destPosX = _tetromino.anchoredPosition.x + _wallKick[wallKickIndex, testIndex].x * MINO_PIXEL_SIZE;
        float destPosY = _tetromino.anchoredPosition.y + _wallKick[wallKickIndex, testIndex].y * MINO_PIXEL_SIZE;

        switch (_clickHorizonDir)
        {
            case ClickHorizonDir.Left:
                _tetromino.transform.DORotate(_tetromino.rotation.eulerAngles + ROTATION_VECTOR, 0);
                _tetromino.DOAnchorPos(new Vector2(destPosX, destPosY), 0);
                break;
            case ClickHorizonDir.Right:
                _tetromino.transform.DORotate(_tetromino.rotation.eulerAngles - ROTATION_VECTOR, 0);
                _tetromino.DOAnchorPos(new Vector2(destPosX, destPosY), 0);
                break;
        }

    }

    #endregion


    #region Drag Method
    //드래그중일 때 상하, 좌우 중에 어떤걸로 움직이는지
    MoveDir GetMoveDir(Vector2 delta)
    {
        var dir = _moveDir;
        if(_moveDir == MoveDir.Idle)
        {
            dir = Mathf.Abs(delta.x) > Mathf.Abs(delta.y) ? MoveDir.HorizonDir :
                Mathf.Abs(delta.x) < Mathf.Abs(delta.y) ? MoveDir.VerticalDir : dir;
        }

        if (_moveDir == MoveDir.VerticalDir)
        {
            dir = (Mathf.Abs(delta.y) > QUICK_DOWN_FORCE) ? MoveDir.QuickDown : MoveDir.SlowDown;
        }

        return dir;
    }

    //이동로직
    void UpdateMove(Vector2 position, Vector2 delta)
    {
        _curDragPosition = UtilManager.ConvertScreentoLocalInRect(_gameMainCanvas,position);
        float distance = _PrevDragPosition.x - _curDragPosition.x;

        switch (_moveDir)
        {
            case MoveDir.HorizonDir:
                _dragHorizonDir = distance > 0 ? DragHorizonDir.Left : DragHorizonDir.Right;
                int force = (int)(Mathf.Round(Mathf.Abs(distance)) / MINO_PIXEL_SIZE);

                if (force >= 1)
                    OneStepHorizonMove();
                break;

            case MoveDir.SlowDown:
            case MoveDir.QuickDown:
                if (!_isForceMoveDown)
                {
                    _isForceMoveDown = true;
                    ClearMove();
                    StartMove();
                };
                break;
        }
    }
    //드래그 시작시 가하는 힘을 알기위한 포지션 변수 초기화
    void GetBeginDragPosition(Vector2 position)
    {
        _curDragPosition = UtilManager.ConvertScreentoLocalInRect(_gameMainCanvas, position);
        _PrevDragPosition = _curDragPosition;
    }

    void IsEndDrag(bool drag)
    {
         _moveDir = MoveDir.Idle;

        if (_isForceMoveDown)
        {
            _isForceMoveDown = false;
            ClearMove();
            StartMove();
        }
    }
    #endregion


    #region UniRx UniTask Method
    private void StartMove()
    {
        _moveSubscriber.Clear();

        switch (_moveDir)
        {
            case MoveDir.SlowDown:
                _intervalTime = SLOW_DOWN_INTERVAL_TIME;
                break;
            case MoveDir.QuickDown:
                _intervalTime = QUICK_DOWN_INTERVAL_TIME;
                break;
            default:
                _intervalTime = AUTO_DOWN_INTERVAL_TIME;
                break;
        }

        Observable.Interval(TimeSpan.FromSeconds(_intervalTime))
            .ObserveOn(Scheduler.MainThread)
            .Subscribe(_ =>
            {
                OneStepDownMove();
            }).AddTo(_moveSubscriber);
    }

    private void ClearMove()
    {
        _moveSubscriber.Clear();
    }

    private async UniTaskVoid ProcessAsyncTaskDeleyLanding()
    {
        await UniTask.WaitUntil(() => _isDeleyLanding);
        Debug.Log($"isDeleyLanding Success");
    }

    private async UniTask ProcessAsyncTaskCompleteLanding()
    {
        await UniTask.WaitUntil(() => _isCompletionLanding);
        Debug.Log($"isCompletionLanding Success");

        ClearAll();
        for (int i = 0; i < _minoControllers.Length; i++) 
        {
            _boardController.AddMinosList(_curCell[i], _minoControllers[i].gameObject);
        }
    }


    #endregion

    //private int PolicyId(int id)
    //{
    //    int adqwdqwd = 123456789;

    //    return HashCode.Combine(id, adqwdqwd);
    //}
}
