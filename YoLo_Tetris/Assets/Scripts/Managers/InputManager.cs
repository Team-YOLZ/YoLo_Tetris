using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
using static Define;
using UnityEngine.UI;
using TMPro;

public class InputManager : MonoBehaviour ,IPointerClickHandler , IPointerDownHandler, IDragHandler, IEndDragHandler
{
    public Action<TouchEvent, Vector2, Vector2> TouchAction = null;
    public Action<bool> IsEndDragAction = null;
    public Action<Vector2> BeginDragAction = null;

    private bool _isDrag = false;

    //test
    public Vector2 TouchPosition = Vector2.zero;
    public TMP_Text text1; 
    public TMP_Text text2;
    public RectTransform canvasRect;
    public GameObject mino;

    //test
    void Start()
    {
        Init();
    }

    //test
    public void Init()
    {
        text1 = GameObject.Find("TestText1").GetComponent<TMP_Text>();
        text2 = GameObject.Find("TestText2").GetComponent<TMP_Text>();

        text1.text = "Init Success!";
        text2.text = "Init Success!";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isDrag)
            return;
        TouchAction.Invoke(TouchEvent.Click, eventData.position, eventData.delta);


        //test
        //화면상의 좌표
        TouchPosition = eventData.position;
        //화면좌표 -> 월드좌표
        Vector2 worldVec = Camera.main.ScreenToWorldPoint(TouchPosition);
        //월드좌표 -> 다시 스크린좌표
        Vector2 screenVec = Camera.main.WorldToScreenPoint(worldVec);
        //스크린좌표 -> canvas내의 좌표로 변환
        Vector2 rectVec = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenVec, Camera.main, out rectVec);
        mino.GetComponent<RectTransform>().anchoredPosition = rectVec;
        //Debug.Log("화면 : "+TouchPosition.ToString());
        //Debug.Log("월드 : " + worldVec.ToString());
        //Debug.Log("다시 화면 : " + screenVec.ToString());
        //Debug.Log("canvas 좌표 : " + rectVec.ToString());

        text1.text = screenVec.ToString();
        text2.text = TouchPosition.ToString();

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        BeginDragAction.Invoke(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        TouchAction.Invoke(TouchEvent.Drag, eventData.position, eventData.delta);
        _isDrag = true;


        //test
        TouchPosition = eventData.position;
        TouchPosition = Camera.main.ScreenToWorldPoint(TouchPosition);
        //Debug.Log("Drag");
        text1.text = "Drag";
        text2.text = TouchPosition.ToString();
        //Debug.Log(eventData.delta);

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

