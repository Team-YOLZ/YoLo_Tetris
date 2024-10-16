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

    private Vector2 curDragPosition = Vector2.zero;
    private Vector2 PrevDragPosition = Vector2.zero;
    private InputManager _inputManager;

    private float _fallingSpeed = 300.0f; //떨어지는 속도
    private float _quickDownForce = 8.0f; //테트로미노가 바로 떨어지게하는 힘의 양
    private float _minoPixelSize = 70.0f; //테트로미노 70x70

    private bool _isLanding = false; //착지했는가?

    readonly private Vector3 _rotateVec = new Vector3(0, 0, 90);


    private MoveDir _move = MoveDir.Idle;
    //public MoveDir MinoMove
    //{
    //    get { return _move; }
    //    set
    //    {
    //        if (_move == value)
    //            return;

    //        _move = value;
    //    }
    //}

    private RotationDir _rotation = RotationDir.Idle;
    //public RotationDir MinoRotation
    //{
    //    get { return _rotation; }
    //    set
    //    {
    //        if (_rotation == value)
    //            return;

    //        _rotation = value;
    //    }
    //}


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

        _inputManager.TouchAction += TouchState;
        _inputManager.IsEndDragAction += IsEndDrag;
        _inputManager.BeginDragAction += GetBeginDragPosition;

        StartCoroutine(AutoDownMove());

    }

    // 화면 : 드래그 or 클릭
    void TouchState(TouchEvent touchEvent, Vector2 position, Vector2 delta)
    {
        switch (touchEvent)
        {
            case TouchEvent.Click:
                GetRotationDir(position);
                UpdateRotation();
                break;

            case TouchEvent.Drag:
                GetMoveDir(delta);
                UpdateMove(position);
                break;
        }
    }


    #region Click Method
    // 클릭이벤트 -> 회전
    void UpdateRotation()
    {
        switch (_rotation)
        {
            case RotationDir.Left:
                _mino.GetComponent<Transform>().DORotate(_mino.rotation.eulerAngles - _rotateVec, 0);
                break;
            case RotationDir.Right:
                _mino.GetComponent<Transform>().DORotate(_mino.rotation.eulerAngles + _rotateVec, 0);
                break;
        }

    }

    void GetRotationDir(Vector2 position)
    {
        if (Camera.main.ScreenToWorldPoint(position).x >= 0)
            _rotation = RotationDir.Right;
        else
            _rotation = RotationDir.Left;
    }
    #endregion


    #region Drag Method
    // 드래그이벤트 -> 이동
    void UpdateMove(Vector2 position)
    {
        curDragPosition = ConvertScreentoLocalInRect(position);

        switch (_move)
        {
            case MoveDir.HorizonDir:
                //todo
                float distance = PrevDragPosition.x - curDragPosition.x;
                int force = (int)(Mathf.Round(distance) / _minoPixelSize);
                //left
                if (distance > 0 && Mathf.Round(distance) >= _minoPixelSize)
                {
                    float destPosY = _mino.anchoredPosition.y - _minoPixelSize * force;
                    _mino.DOAnchorPosX(destPosY, 0.01f);
                }
                //right
                else if (distance < 0 && Mathf.Round(distance) >= _minoPixelSize)
                {
                    float destPosY = _mino.anchoredPosition.y + _minoPixelSize * force;
                    _mino.DOAnchorPosX(destPosY, 0.01f);
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

    //드래그를 위한 포지션변수 초기화
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


    #region Coroutine Method
    IEnumerator AutoDownMove()
    {
        while (!_isLanding)
        {
            float destPosY = _mino.anchoredPosition.y - _minoPixelSize; //아래로 한 칸씩 이동
            _mino.DOAnchorPosY(destPosY, 0);
            yield return new WaitForSeconds(_fallingSpeed * Time.deltaTime);
        }
        StopCoroutine(AutoDownMove());
    }
    #endregion


}
