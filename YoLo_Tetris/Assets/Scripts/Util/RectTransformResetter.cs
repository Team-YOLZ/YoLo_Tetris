using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectTransformResetter : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Vector2 _initialAnchoredPosition;
    private Vector2 _initialSizeDelta;
    private Vector2 _initialAnchorMin;
    private Vector2 _initialAnchorMax;
    private Vector2 _initialPivot;
    private Vector3 _initialLocalScale;
    private Quaternion _initialLocalRotation;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        SaveInitialValues();
    }

    private void SaveInitialValues()
    {
        // 초기값 저장
        _initialAnchoredPosition = _rectTransform.anchoredPosition;
        _initialSizeDelta = _rectTransform.sizeDelta;
        _initialAnchorMin = _rectTransform.anchorMin;
        _initialAnchorMax = _rectTransform.anchorMax;
        _initialPivot = _rectTransform.pivot;
        _initialLocalScale = _rectTransform.localScale;
        _initialLocalRotation = _rectTransform.localRotation;
    }

    public void ResetToInitialState()
    {
        // 저장된 초기값으로 복원
        _rectTransform.anchoredPosition = _initialAnchoredPosition;
        _rectTransform.sizeDelta = _initialSizeDelta;
        _rectTransform.anchorMin = _initialAnchorMin;
        _rectTransform.anchorMax = _initialAnchorMax;
        _rectTransform.pivot = _initialPivot;
        _rectTransform.localScale = _initialLocalScale;
        _rectTransform.localRotation = _initialLocalRotation;
    }

}