using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static Define;
using System;

public class MinoController : MonoBehaviour
{
    [SerializeField] private RectTransform _mino; // 자기 자신 위치 
    public float _fallingSpeed = 500.0f; //떨어지는 속도

    //private bool _isMoving = false; 
    private bool _isLanding = false; //착지했는가?

    private InputManager _inputManager;


    private MoveDir _move = MoveDir.Idle;
    public MoveDir MinoMove
    {
        get { return _move; }
        set
        {
            if (_move == value)
                return;

            _move = value;
        }
    }

    private RotationDir _rotation = RotationDir.Idle;
    public RotationDir MinoRotation
    {
        get { return _rotation; }
        set
        {
            if (_rotation == value)
                return;

            _rotation = value;
        }
    }

    void Awake()
    {
        if (_mino == null)
        {
            _mino = gameObject.GetComponent<RectTransform>();
        }

        if (_inputManager == null)
        {
            _inputManager = GameObject.Find("InputScreen").GetComponent<InputManager>();
        }
        _inputManager.TouchAction += TouchState;

        StartCoroutine(AutoDownMove());

    }

    // 화면 : 드래그 or 클릭
    void TouchState(TouchEvent touchEvent, Vector2 position)
    {
        switch (touchEvent)
        {
            case TouchEvent.Click:
                if (Camera.main.ScreenToWorldPoint(position).x >= 0)
                    _rotation = RotationDir.Right;
                else
                    _rotation = RotationDir.Left;
                UpdateRotation();
                break;

            case TouchEvent.Drag:
                UpdateMove(position);
                break;
        }
    }

    //화면 클릭 -> 회전
    void UpdateRotation()
    {
        switch (MinoRotation)
        {
            case RotationDir.Left:
                //todo
                break;
            case RotationDir.Right:
                //todo
                break;
        }

    }

    //화면 드래그 -> 이동
    void UpdateMove(Vector2 position)
    {
        switch (MinoMove)
        {
            case MoveDir.Left:
                //todo
                break;
            case MoveDir.Right:
                //todo
                break;
            case MoveDir.SlowDown:
                //todo
                break;
            case MoveDir.QuickDown:
                //todo
                break;
        }
    }


    IEnumerator AutoDownMove()
    {
        while (!_isLanding)
        {
            float destPosY = _mino.anchoredPosition.y - 60f; //아래로 한 칸씩 이동
            _mino.DOAnchorPosY(destPosY, 0);
            yield return new WaitForSeconds(_fallingSpeed * Time.deltaTime);
        }
        StopCoroutine(AutoDownMove());
    }



}
