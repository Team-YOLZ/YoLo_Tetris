using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static Define;
using System;

public class TetrominoController : MonoBehaviour
{
    [SerializeField] private RectTransform _tetromino; // 자기 자신 위치
    [SerializeField] private RectTransform _gameMainCanvas;
    [SerializeField] private TetrominoBoundaryData _data;
    [SerializeField] private RectTransform[] _minos;

    private const float _fallingSpeed = 1000.0f; //떨어지는 속도
    private const float _quickDownForce = 6.5f; //테트로미노가 바로 떨어지게하는 힘의 양
    private const float _minoPixelSize = 70.0f; //테트로미노 70x70
    private bool _isLanding = false; //착지했는가?

    readonly private Vector3 _rotateVec = new Vector3(0, 0, 90);
    private Vector2 _curDragPosition = Vector2.zero;
    private Vector2 _PrevDragPosition = Vector2.zero;

    private Coroutine _autoDownCoroutine;

    private MoveDir _move = MoveDir.Idle;
    private RotationDir _rotation = RotationDir.Idle; 
    private HorizonDir _horizonDir = HorizonDir.Idle; //드래그 왼/오
    private MinoType _minoType = MinoType.Unknown;
    private MinoRotationState _minoRotationState = MinoRotationState.R0;
    public MinoRotationState MinoRotationState
    {
        get { return _minoRotationState; }
        set
        {
            if (_minoRotationState == value)
                return;
            _minoRotationState = value;
        }
    }


    void Awake()
    {
        if(_minoType == MinoType.Unknown)
        {
            var name =  (MinoType)(int)gameObject.name[0];
            _minoType = name;
        }

        InputManager.Instance.TouchAction += TouchState;
        InputManager.Instance.BeginDragAction += GetBeginDragPosition;
        InputManager.Instance.IsEndDragAction += IsEndDrag;

        StartAutoDown();
    }

    void OnDestroy()
    {
        InputManager.Instance.TouchAction -= TouchState;
        InputManager.Instance.BeginDragAction -= GetBeginDragPosition;
        InputManager.Instance.IsEndDragAction -= IsEndDrag;
    }

    void StartAutoDown()
    {
        StopAutoDown();
        _autoDownCoroutine = StartCoroutine(nameof(AutoDownMove));
    }

    void StopAutoDown()
    {
        if (_autoDownCoroutine != null)
        {
            StopCoroutine(_autoDownCoroutine);
            _autoDownCoroutine = null;
        }
    }


    // 입력이벤트
    void TouchState(TouchEvent touchEvent, RotationDir leftRight, Vector2 position, Vector2 delta)
    {
        switch (touchEvent)
        {
            case TouchEvent.Click:
                _rotation = leftRight;
                _minoRotationState = GetRotationState();
                UpdateRotate();
                UpdatePositionAfterClick();
                break;

            case TouchEvent.Drag:
                _move = GetMoveDir(delta);
                UpdateMove(position);
                break;
        }
    }

    #region Click Method
    MinoRotationState GetRotationState()
    {
        var state = _minoRotationState;
        switch (_rotation)
        {
            case RotationDir.Left:
                int leftRotation = (int)_minoRotationState + 1 == 4 ? 0 : (int)_minoRotationState + 1;
                state = (MinoRotationState)leftRotation;
                break;
            case RotationDir.Right:
                int rightRotation = (int)_minoRotationState - 1 == -1 ? 3 : (int)_minoRotationState - 1;
                state = (MinoRotationState)rightRotation;
                break;
        }
        return state;
    }

    //회전
    void UpdateRotate()
    {
        switch (_rotation)
        {
            case RotationDir.Left:
                _tetromino.transform.DORotate(_tetromino.rotation.eulerAngles + _rotateVec, 0);
                break;
            case RotationDir.Right:
                _tetromino.transform.DORotate(_tetromino.rotation.eulerAngles - _rotateVec, 0);
                break;
        }

    }

    //클릭 후 경계선 넘어로 삐져 나오는 놈 처리
    void UpdatePositionAfterClick()
    {
        Vector2 curBoundary = Vector2.zero;
        float curPositionX = _tetromino.anchoredPosition.x;

        switch (_minoRotationState)
        {
            case MinoRotationState.R0:
                curBoundary = _data.HorizonBoundary.R0;
                break;
            case MinoRotationState.R1:
                curBoundary = _data.HorizonBoundary.R1;
                break;
            case MinoRotationState.R2:
                curBoundary = _data.HorizonBoundary.R2;
                break;
            case MinoRotationState.R3:
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
    void UpdateMove(Vector2 position)
    {
        _curDragPosition = UtilManager.ConvertScreentoLocalInRect(_gameMainCanvas,position);
        float distance = _PrevDragPosition.x - _curDragPosition.x;

        switch (_move)
        {
            case MoveDir.HorizonDir:
                int force = (int)(Mathf.Round(Mathf.Abs(distance)) / _minoPixelSize);
                //left
                if (distance > 0 && force >= 1)
                {
                    float destPos = _tetromino.anchoredPosition.x - _minoPixelSize * force;
                    _tetromino.DOAnchorPosX(GetClampPositionHorizonBoundary(destPos), 0);
                    _PrevDragPosition = _curDragPosition;
                    _horizonDir = HorizonDir.Left;
                }
                //right
                else if (distance < 0 && force >= 1)
                {
                    float destPos = _tetromino.anchoredPosition.x + _minoPixelSize * force;
                    _tetromino.DOAnchorPosX(GetClampPositionHorizonBoundary(destPos), 0);
                    _PrevDragPosition = _curDragPosition;
                    _horizonDir = HorizonDir.Right;
                }
                break;

            case MoveDir.SlowDown:
                //todo
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
        var dir = _move;
        if(_move == MoveDir.Idle)
        {
            dir = Mathf.Abs(delta.x) > Mathf.Abs(delta.y) ? MoveDir.HorizonDir :
                Mathf.Abs(delta.x) < Mathf.Abs(delta.y) ? MoveDir.VerticalDir : dir;
        }

        if (_move == MoveDir.VerticalDir)
        {
            dir = (Mathf.Abs(delta.y) > _quickDownForce) ? MoveDir.QuickDown : dir;
        }

        return dir;
    }

    //좌우드래그(or 수직낙하드래그)가 끝나지 않았는데 수직낙하(or 좌우드래그) 방지
    void IsEndDrag(bool drag)
    {
        _move = MoveDir.Idle;
    }

    //이동중에 경계 못 벗어나게 좌표 구하기
    float GetClampPositionHorizonBoundary(float positionX)
    {
        switch (_minoRotationState)
        {
            case MinoRotationState.R0:
                positionX = Mathf.Clamp(positionX, _data.HorizonBoundary.R0.x, _data.HorizonBoundary.R0.y);
                break;
            case MinoRotationState.R1:
                positionX = Mathf.Clamp(positionX, _data.HorizonBoundary.R1.x, _data.HorizonBoundary.R1.y);
                break;
            case MinoRotationState.R2:
                positionX = Mathf.Clamp(positionX, _data.HorizonBoundary.R2.x, _data.HorizonBoundary.R2.y);
                break;
            case MinoRotationState.R3:
                positionX = Mathf.Clamp(positionX, _data.HorizonBoundary.R3.x, _data.HorizonBoundary.R3.y);
                break;
        }
        return positionX;
    }
    #endregion


    #region Coroutine Method
    IEnumerator AutoDownMove()
    {
        while (!_isLanding)
        {
            yield return new WaitForSeconds(_fallingSpeed * Time.deltaTime);

            //아래로 한 칸씩 이동
            float destPosY = _tetromino.anchoredPosition.y - _minoPixelSize;
            _tetromino.DOAnchorPosY(destPosY, 0);
        }
        StopCoroutine(AutoDownMove());
    }
    #endregion


}
