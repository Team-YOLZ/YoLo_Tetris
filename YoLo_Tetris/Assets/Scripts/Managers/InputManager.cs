using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
using static Define;
using UnityEngine.UI;
using TMPro;

public class InputManager : Singleton<InputManager>, IPointerClickHandler , IPointerDownHandler, IDragHandler, IEndDragHandler
{
    public Action<TouchEvent, ClickHorizonDir, Vector2, Vector2> TouchAction = null;
    public Action<bool> IsEndDragAction = null;
    public Action<Vector2> BeginDragAction = null;

    private bool _isDrag = false;


    protected override void Init()
    {
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isDrag)
            return;

        var leftRight = Camera.main.ScreenToWorldPoint(eventData.position).x >= 0 ? ClickHorizonDir.Right : ClickHorizonDir.Left;
        TouchAction.Invoke(TouchEvent.Click, leftRight, eventData.position, eventData.delta);

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        BeginDragAction.Invoke(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        TouchAction.Invoke(TouchEvent.Drag, ClickHorizonDir.Idle, eventData.position, eventData.delta);
        _isDrag = true;

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        IsEndDragAction.Invoke(_isDrag);
        _isDrag = false;
    }

    public void Clear()
    {
        TouchAction = null;
    }

}

