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
    private CompositeDisposable _subscriber = new();
    private IDisposable _disposableSlowDown;

    [SerializeField] private RectTransform _tetromino; // 자기 자신 위치
    [SerializeField] private RectTransform _gameMainCanvas;
    [SerializeField] private TetrominoData _data;
    [SerializeField] private MinoController[] _minoControllers; //** 없음

    private const float _autoDownSpeed = 400.0f; //떨어지는 속도
    private const float _slowDownSpeed = 80.0f; //떨어지는 속도
    private const float _quickDownForce = 9.5f; //테트로미노가 바로 떨어지게하는 힘의 양
    private const float _minoPixelSize = 65.0f; //테트로미노 65x65
    private bool _isLanding = false; //착지했는가?
    private const float _intervalTime = 3.0f;

    readonly private Data _data1;
    readonly private Vector3 _rotateVec = new Vector3(0, 0, 90);
    private Vector2 _curDragPosition = Vector2.zero;
    private Vector2 _PrevDragPosition = Vector2.zero;

    private Coroutine _autoDownCoroutine;
    private Coroutine _slowDownCoroutine;

    private MoveDir _moveDir = MoveDir.Idle;
    private ClickHorizonDir _clickHorizonDir = ClickHorizonDir.Idle; 
    private DragHorizonDir _dragHorizonDir = DragHorizonDir.Idle; //드래그 왼/오
    private TetrominoType _tetrominoType = TetrominoType.Unknown;
    private RotationState _rotationState = RotationState.R0;

    public RotationState MinoRotationState
    {
        get { return _rotationState; }
        set
        {
            if (_rotationState == value)
                return;
            _rotationState = value;
        }
    }


    void Awake()
    {
        if(_tetrominoType == TetrominoType.Unknown)
        {
            var name =  (TetrominoType)(int)gameObject.name[0];
            _tetrominoType = name;
        }

        InputManager.Instance.TouchAction += TouchState;
        InputManager.Instance.BeginDragAction += GetBeginDragPosition;
        InputManager.Instance.IsEndDragAction += IsEndDrag;

        //AutoMove();
        //StartAutoDown();
    }

    private void Start()
    {
        OnMove2().Forget();
        ProcessAsyncTask().Forget();
        Invoke(nameof(AutoMove), 2); // AutoMove();
    }

    void OnDestroy()
    {
        InputManager.Instance.TouchAction -= TouchState;
        InputManager.Instance.BeginDragAction -= GetBeginDragPosition;
        InputManager.Instance.IsEndDragAction -= IsEndDrag;
        _subscriber.Dispose();
    }

    //void StartAutoDown()
    //{
    //    StopAutoDown();
    //    _autoDownCoroutine = StartCoroutine(nameof(AutoDownMove));
    //}

    //void StopAutoDown()
    //{
    //    if (_autoDownCoroutine != null)
    //    {
    //        StopCoroutine(_autoDownCoroutine);
    //        _autoDownCoroutine = null;
    //    }
    //}

    void StartSlowDown()
    {
        StopSlowDown();
        _slowDownCoroutine = StartCoroutine(nameof(SlowDownMove));
    }

    void StopSlowDown()
    {
        if(_slowDownCoroutine != null)
        {
            StopCoroutine(_slowDownCoroutine);
            _slowDownCoroutine = null;
        }
    }

    //아래로 한 칸씩 이동
    void OneStepDownMove()
    {
        Debug.Log("OneStepDownMove");

        float destPosY = _tetromino.anchoredPosition.y - _minoPixelSize;
        _tetromino.DOAnchorPosY(destPosY, 0);

        foreach (var minos in _minoControllers)
        {
            minos._position.y -= 1;
        }
    }


    // 입력이벤트
    void TouchState(TouchEvent touchEvent, ClickHorizonDir leftRight, Vector2 position, Vector2 delta)
    {
        switch (touchEvent)
        {
            case TouchEvent.Click:
                _clickHorizonDir = leftRight;
                _rotationState = GetRotationState();
                UpdateRotate();
                UpdatePositionAfterClick();
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

    //회전
    void UpdateRotate()
    {
        switch (_clickHorizonDir)
        {
            case ClickHorizonDir.Left:
                _tetromino.transform.DORotate(_tetromino.rotation.eulerAngles + _rotateVec, 0);
                break;
            case ClickHorizonDir.Right:
                _tetromino.transform.DORotate(_tetromino.rotation.eulerAngles - _rotateVec, 0);
                break;
        }

    }

    //클릭 후 경계선 넘어로 삐져 나오는 놈 처리
    void UpdatePositionAfterClick()
    {
        Vector2 curBoundary = Vector2.zero;
        float curPositionX = _tetromino.anchoredPosition.x;

        switch (_rotationState)
        {
            case RotationState.R0:
                curBoundary = _data.HorizonBoundary.R0;
                break;
            case RotationState.R1:
                curBoundary = _data.HorizonBoundary.R1;
                break;
            case RotationState.R2:
                curBoundary = _data.HorizonBoundary.R2;
                break;
            case RotationState.R3:
                curBoundary = _data.HorizonBoundary.R3;
                break;
        }

        if (curPositionX < curBoundary.x)
            _tetromino.DOAnchorPosX(curBoundary.x, 0);
        else if (curPositionX > curBoundary.y)
            _tetromino.DOAnchorPosX(curBoundary.y, 0);

        
    }
    #endregion


    #region Drag Method
    //이동
    void UpdateMove(Vector2 position, Vector2 delta)
    {
        _curDragPosition = UtilManager.ConvertScreentoLocalInRect(_gameMainCanvas,position);
        float distance = _PrevDragPosition.x - _curDragPosition.x;

        switch (_moveDir)
        {
            case MoveDir.HorizonDir:
                int force = (int)(Mathf.Round(Mathf.Abs(distance)) / _minoPixelSize);
                //left
                if (distance > 0 && force >= 1)
                {
                    float destPos = _tetromino.anchoredPosition.x - _minoPixelSize * force;
                    _tetromino.DOAnchorPosX(GetClampPositionHorizonBoundary(destPos), 0);
                    _PrevDragPosition = _curDragPosition;
                    _dragHorizonDir = DragHorizonDir.Left;
                    foreach(var minos in _minoControllers)
                    {
                        minos._position.x -= 1;
                    }
                }
                //right
                else if (distance < 0 && force >= 1)
                {
                    float destPos = _tetromino.anchoredPosition.x + _minoPixelSize * force;
                    _tetromino.DOAnchorPosX(GetClampPositionHorizonBoundary(destPos), 0);
                    _PrevDragPosition = _curDragPosition;
                    _dragHorizonDir = DragHorizonDir.Right;
                    foreach (var minos in _minoControllers)
                    {
                        minos._position.x += 1;
                    }
                }
                break;

            case MoveDir.SlowDown:
                StartSlowDown();
                break;

            case MoveDir.QuickDown:
                //todo
                break;
        }
    }

    //드래그 시작시 가하는 힘을 알기위한 포지션 변수 초기화
    void GetBeginDragPosition(Vector2 position)
    {
        _curDragPosition = UtilManager.ConvertScreentoLocalInRect(_gameMainCanvas, position);
        _PrevDragPosition = _curDragPosition;
    }

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
            dir = (Mathf.Abs(delta.y) > _quickDownForce) ? MoveDir.QuickDown : MoveDir.SlowDown;
        }

        return dir;
    }

    void IsEndDrag(bool drag)
    {
        if (_moveDir == MoveDir.SlowDown)
            StopSlowDown();

        _moveDir = MoveDir.Idle;
        
    }

    //이동중에 경계 못 벗어나게 좌표 구하기
    float GetClampPositionHorizonBoundary(float positionX)
    {
        switch (_rotationState)
        {
            case RotationState.R0:
                positionX = Mathf.Clamp(positionX, _data.HorizonBoundary.R0.x, _data.HorizonBoundary.R0.y);
                break;
            case RotationState.R1:
                positionX = Mathf.Clamp(positionX, _data.HorizonBoundary.R1.x, _data.HorizonBoundary.R1.y);
                break;
            case RotationState.R2:
                positionX = Mathf.Clamp(positionX, _data.HorizonBoundary.R2.x, _data.HorizonBoundary.R2.y);
                break;
            case RotationState.R3:
                positionX = Mathf.Clamp(positionX, _data.HorizonBoundary.R3.x, _data.HorizonBoundary.R3.y);
                break;
        }
        return positionX;
    }
    #endregion


    #region Coroutine Method
    private void AutoMove()
    {
        Debug.Log($"[AutoDownMove] Start ");
        Observable.Interval(TimeSpan.FromSeconds(_intervalTime))
            .ObserveOn(Scheduler.MainThread)  
            .Subscribe(_ =>
            {
                OneStepDownMove();
            }).AddTo(_subscriber);

    }

    private async UniTask OnMove()
    {
        await UniTask.WaitForSeconds(1.0f);
    }

    private async UniTaskVoid OnMove2()
    {
        Debug.Log(1);
        await OnMove();
        Debug.Log(2);
        await OnMove();
        Debug.Log(3);
        await OnMove();
        Debug.Log(4);
        await OnMove();
        Debug.Log(5);
    }

    public async UniTask ProcessAsyncTask()
    {
        await UniTask.WaitUntil(() => _isLanding);

        Debug.Log($"Landing Success");
    }

    //IEnumerator AutoDownMove()
    //{
    //    Debug.Log($"[AutoDownMove] Start ");
    //    while (!_isLanding)
    //    {
    //        float intervalTime = (float)TimeSpan.FromSeconds(_intervalTime).TotalSeconds; //여기에 interval 시간 메서드 짜서 바꾸면intervalTime
    //        Debug.Log($"intervalTime = {intervalTime}");
    //        yield return new WaitForSeconds(intervalTime);
    //        OneStepDownMove();
    //    }
    //    StopCoroutine(AutoDownMove());
    //}

    IEnumerator SlowDownMove()
    {
        while (!_isLanding)
        {
            yield return new WaitForSeconds(_slowDownSpeed * Time.deltaTime);
            OneStepDownMove();
        }
        StopCoroutine(SlowDownMove());
    }
    #endregion

    //private int PolicyId(int id)
    //{
    //    int adqwdqwd = 123456789;

    //    return HashCode.Combine(id, adqwdqwd);
    //}
}
