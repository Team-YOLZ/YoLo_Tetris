using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
using static Define;
using UnityEngine.UI;
using TMPro;

public class InputManager : MonoBehaviour ,IPointerClickHandler ,IDragHandler,IEndDragHandler
{
    public Action<TouchEvent, Vector2> TouchAction = null;

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
        TouchAction.Invoke(TouchEvent.Click, eventData.position);

        //test
        TouchPosition = eventData.position;
        TouchPosition = Camera.main.ScreenToWorldPoint(TouchPosition);
        Vector2 screenVec = Camera.main.WorldToScreenPoint(TouchPosition);
        Vector2 rectVec = Vector2.zero;
        //스크린좌표 -> canvas내의 좌표로 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenVec, Camera.main,out rectVec);
        mino.GetComponent<RectTransform>().anchoredPosition = rectVec;
        Debug.Log("Click");
        text1.text = screenVec.ToString();
        text2.text = TouchPosition.ToString();

    }

    public void OnDrag(PointerEventData eventData)
    {
        TouchAction.Invoke(TouchEvent.Drag, eventData.position);
        _isDrag = true;

        //test
        TouchPosition = eventData.position;
        TouchPosition = Camera.main.ScreenToWorldPoint(TouchPosition);
        Debug.Log("Drag");
        text1.text = "Drag";
        text2.text = TouchPosition.ToString();

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _isDrag = false;
    }

    public void Clear()
    {
        TouchAction = null;
    }

}

