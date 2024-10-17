using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static Define;
using System;

public class MinoController : MonoBehaviour
{
    [SerializeField] private RectTransform _mino; // 자기 자신 위치
    [SerializeField] private RectTransform _gameMainCanvas;

    [SerializeField] private TetrominoBoundaryData _data;

    private Vector2 curDragPosition = Vector2.zero;
    private Vector2 PrevDragPosition = Vector2.zero;
    private InputManager _inputManager;

    private float _fallingSpeed = 1000.0f; //떨어지는 속도
    private float _quickDownForce = 8.0f; //테트로미노가 바로 떨어지게하는 힘의 양
    private float _minoPixelSize = 70.0f; //테트로미노 70x70
    private bool _isLanding = false; //착지했는가?

    readonly private Vector3 _rotateVec = new Vector3(0, 0, 90);

    private MoveDir _move = MoveDir.Idle;
    private RotationDir _rotation = RotationDir.Idle;
    private HorizonDir _horizon = HorizonDir.Idle;
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
        if (_mino == null)
        {
            _mino = gameObject.GetComponent<RectTransform>();
        }

        if(_gameMainCanvas == null)
        {
            _gameMainCanvas = gameObject.transform.root.GetComponent<RectTransform>();
        }

        if (_inputManager == null)
        {
            _inputManager = GameObject.Find("InputScreen").GetComponent<InputManager>();
        }

        if(_minoType == MinoType.Unknown)
        {
            var name =  (MinoType)(int)gameObject.name[0];
            _minoType = name;
        }



        _inputManager.TouchAction += TouchState;
        _inputManager.IsEndDragAction += IsEndDrag;
        _inputManager.BeginDragAction += GetBeginDragPosition;

        StartCoroutine(AutoDownMove());

    }

    // 입력이벤트
    void TouchState(TouchEvent touchEvent, Vector2 position, Vector2 delta)
    {
        switch (touchEvent)
        {
            case TouchEvent.Click:
                GetRotationDir(position);
                GetRotationState();
                UpdateRotate();
                break;

            case TouchEvent.Drag:
                GetMoveDir(delta);
                UpdateMove(position);
                break;
        }
    }


    #region Click Method
    void GetRotationDir(Vector2 position)
    {
        if (Camera.main.ScreenToWorldPoint(position).x >= 0)
            _rotation = RotationDir.Right;
        else
            _rotation = RotationDir.Left;
    }

    void GetRotationState()
    {
        switch (_rotation)
        {
            case RotationDir.Left:
                int leftRotation = (int)_minoRotationState + 1 == 4 ? 0 : (int)_minoRotationState + 1;
                _minoRotationState = (MinoRotationState)leftRotation;
                break;
            case RotationDir.Right:
                int rightRotation = (int)_minoRotationState - 1 == -1 ? 3 : (int)_minoRotationState - 1;
                _minoRotationState = (MinoRotationState)rightRotation;
                break;
        }
    }

    void UpdateRotate()
    {
        switch (_rotation)
        {
            case RotationDir.Left:
                _mino.GetComponent<Transform>().DORotate(_mino.rotation.eulerAngles + _rotateVec, 0);
                break;
            case RotationDir.Right:
                _mino.GetComponent<Transform>().DORotate(_mino.rotation.eulerAngles - _rotateVec, 0);
                break;
        }

    }
    #endregion


    #region Drag Method
    // 드래그이벤트 -> 이동
    void UpdateMove(Vector2 position)
    {
        curDragPosition = ConvertScreentoLocalInRect(position);
        float distance = PrevDragPosition.x - curDragPosition.x;

        switch (_move)
        {
            case MoveDir.HorizonDir:
                if (SafeArea(distance))
                {
                    int force = (int)(Mathf.Round(Mathf.Abs(distance)) / _minoPixelSize);
                    Debug.Log(_mino.anchoredPosition.x);
                    //left
                    if (distance > 0 && force >= 1)
                    {
                        float destPos = _mino.anchoredPosition.x - _minoPixelSize * force;
                        _mino.DOAnchorPosX(destPos, 0);
                        PrevDragPosition = curDragPosition;
                        _horizon = HorizonDir.Left;
                    }
                    //right
                    else if (distance < 0 && force >= 1)
                    {
                        float destPos = _mino.anchoredPosition.x + _minoPixelSize * force;
                        _mino.DOAnchorPosX(destPos, 0);
                        PrevDragPosition = curDragPosition;
                        _horizon = HorizonDir.Right;
                    }
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

    //드래그할 때 가하는 힘을 알기위한 포지션 변수 초기화
    void GetBeginDragPosition(Vector2 position)
    {
        curDragPosition = ConvertScreentoLocalInRect(position);
        PrevDragPosition = curDragPosition;
    }

    //드래그중일 때 상하, 좌우 중에 어떤걸로 움직이는지
    void GetMoveDir(Vector2 delta)
    {
        if(_move == MoveDir.Idle)
        {
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                _move = MoveDir.HorizonDir;
            else if (Mathf.Abs(delta.x) < Mathf.Abs(delta.y))
                _move = MoveDir.VerticalDir;
            else
                _move = MoveDir.Idle;
        }

        if(_move == MoveDir.VerticalDir)
        {
            if (Mathf.Abs(delta.y) > _quickDownForce)
                _move = MoveDir.QuickDown;
        }

    }

    //좌우드래그(or 수직낙하드래그)가 끝나지 않았는데 수직낙하(or 좌우드래그) 방지
    void IsEndDrag(bool drag)
    {
        _move = MoveDir.Idle;
    }
    #endregion

    //화면좌표 -> 캔버스 좌표
    Vector2 ConvertScreentoLocalInRect(Vector2 position)
    {
        Vector2 canvasVec = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_gameMainCanvas, position, Camera.main, out canvasVec);
        return canvasVec;
    }

    //경계 못넘어 가게
    bool SafeArea(float distance)
    {
        //방향 전환
        if (distance > 0 && _horizon == HorizonDir.Right) return true;
        if (distance < 0 && _horizon == HorizonDir.Left) return true;

        var curPositionX = _mino.anchoredPosition.x;
        bool isSafe = true;
        switch (_minoRotationState)
        {
            case MinoRotationState.R0:
                if(_data.HorizonBoundary.R0.x >= curPositionX)
                {
                    isSafe = false;
                    if (_data.HorizonBoundary.R0.x == curPositionX)
                        break;

                    _horizon = HorizonDir.Left;
                    PrevDragPosition = new Vector2(_data.HorizonBoundary.R0.x, _mino.anchoredPosition.y);
                    _mino.DOAnchorPos(PrevDragPosition, 0);
                }
                if(_data.HorizonBoundary.R0.y <= curPositionX)
                {
                    isSafe = false;
                    if (_data.HorizonBoundary.R0.y == curPositionX)
                        break;

                    _horizon = HorizonDir.Right;
                    PrevDragPosition = new Vector2(_data.HorizonBoundary.R0.y, _mino.anchoredPosition.y);
                    _mino.DOAnchorPos(PrevDragPosition, 0);

                }

                break;
            case MinoRotationState.R1:

                break;
            case MinoRotationState.R2:

                break;
            case MinoRotationState.R3:

                break;
        }

        return isSafe;
    }


    #region Coroutine Method
    IEnumerator AutoDownMove()
    {
        while (!_isLanding)
        {
            yield return new WaitForSeconds(_fallingSpeed * Time.deltaTime);

            //아래로 한 칸씩 이동
            float destPosY = _mino.anchoredPosition.y - _minoPixelSize;
            _mino.DOAnchorPosY(destPosY, 0);
        }
        StopCoroutine(AutoDownMove());
    }
    #endregion


}
