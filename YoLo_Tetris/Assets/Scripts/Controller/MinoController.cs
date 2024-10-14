using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static Define;

public class MinoController : MonoBehaviour
{
    [SerializeField] private RectTransform _mino; // 자기 자신 위치 
    public float _fallingSpeed = 500.0f; //떨어지는 속도

    //private bool _isMoving = false; 
    private bool _isLanding = false; //착지했는가?

    private InputState _inputState = InputState.None;
    public InputState MinoState
    {
        get { return _inputState; }
        set
        {
            if (_inputState == value)
                return;

            _inputState = value;
        }
    }

    //MoveDir _move = MoveDir.Idle;
    //RotationDir _rotation = RotationDir.Idle;

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
    }

    void Start()
    {
        StartCoroutine(AutoDownMove());
    }

    void Update()
    {
        UpdateController();
    }

    void UpdateController()
    {
        switch (MinoState)
        {
            case InputState.Click:
                UpdateRotation();
                break;
            case InputState.Drag:
                UpdateMove();
                break;
        }
    }

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
    void UpdateMove()
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
