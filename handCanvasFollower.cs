using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class handCanvasFollower : MonoBehaviour
{
    public OVRHand leftHand;
    public Vector3 positionOffset = new Vector3(0, 0, 0.1f);
    public Vector3 rotationOffset = new Vector3(0, 0, 0);
    private Canvas canvas;
    private RectTransform scrollViewRectTransform;
    private bool isCanvasVisible = false;
    private bool wasIndexPinching = false;
    private Transform targetHandTransform;
    private int toggleCount = 0;
    public Slider positionXSlider;
    public Slider positionYSlider;
    public Slider positionZSlider;
    public Slider rotationXSlider;
    public Slider rotationYSlider;
    public Slider rotationZSlider;

    void Start()
    {
        targetHandTransform = leftHand.transform;
        canvas = GetComponent<Canvas>();
        ScrollRect scrollRect = GetComponentInChildren<ScrollRect>();
        if (scrollRect != null)
        {
            scrollViewRectTransform = scrollRect.GetComponent<RectTransform>();
        }
        canvas.enabled = isCanvasVisible;

        if (positionXSlider != null) positionXSlider.onValueChanged.AddListener(UpdatePositionOffsetX);
        if (positionYSlider != null) positionYSlider.onValueChanged.AddListener(UpdatePositionOffsetY);
        if (positionZSlider != null) positionZSlider.onValueChanged.AddListener(UpdatePositionOffsetZ);
        if (rotationXSlider != null) rotationXSlider.onValueChanged.AddListener(UpdateRotationOffsetX);
        if (rotationYSlider != null) rotationYSlider.onValueChanged.AddListener(UpdateRotationOffsetY);
        if (rotationZSlider != null) rotationZSlider.onValueChanged.AddListener(UpdateRotationOffsetZ);
    }

    void Update()
    {
        if (targetHandTransform != null)
        {
            transform.position = targetHandTransform.position + targetHandTransform.TransformDirection(positionOffset);
            transform.rotation = targetHandTransform.rotation * Quaternion.Euler(rotationOffset);

            if (canvas != null && scrollViewRectTransform != null)
            {
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(scrollViewRectTransform);
            }

            bool isIndexPinching = leftHand.GetFingerIsPinching(OVRHand.HandFinger.Index);
            if (isIndexPinching && !wasIndexPinching)
            {
                isCanvasVisible = !isCanvasVisible;
                canvas.enabled = isCanvasVisible;
                toggleCount++;
                Debug.Log("Canvas toggled " + toggleCount + " times.");
            }
            wasIndexPinching = isIndexPinching;
        }
    }

    void UpdatePositionOffsetX(float value)
    {
        positionOffset.x = value;
        Debug.Log("Position X Offset: " + value);
    }

    void UpdatePositionOffsetY(float value)
    {
        positionOffset.y = value;
        Debug.Log("Position Y Offset: " + value);
    }

    void UpdatePositionOffsetZ(float value)
    {
        positionOffset.z = value;
        Debug.Log("Position Z Offset: " + value);
    }

    void UpdateRotationOffsetX(float value)
    {
        rotationOffset.x = value;
        Debug.Log("Rotation X Offset: " + value);
    }

    void UpdateRotationOffsetY(float value)
    {
        rotationOffset.y = value;
        Debug.Log("Rotation Y Offset: " + value);
    }

    void UpdateRotationOffsetZ(float value)
    {
        rotationOffset.z = value;
        Debug.Log("Rotation Z Offset: " + value);
    }

    public void ResetCanvasPosition()
    {
        positionOffset = new Vector3(0, 0, 0.1f);
        rotationOffset = new Vector3(0, 0, 0);
        Debug.Log("Canvas position and rotation reset.");
    }
}
