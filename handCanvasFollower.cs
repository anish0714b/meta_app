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

    private Vector3 lastHandPosition;
    public float handSpeedThreshold = 0.1f;
    public float handSpeedMultiplier = 2f;

    public float autoHideDelay = 5f;  // Time in seconds to auto-hide the canvas
    private float lastToggleTime;

    public KeyCode saveKey = KeyCode.S;
    public KeyCode loadKey = KeyCode.L;
    public string presetName = "DefaultPreset";

    private bool isThumbsUpGesture = false;
    private bool wasThumbsUpGesture = false;

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

        lastHandPosition = targetHandTransform.position;
        LoadOffsets(presetName);

        lastToggleTime = Time.time;
    }

    void Update()
    {
        if (targetHandTransform != null)
        {
            Vector3 handDelta = targetHandTransform.position - lastHandPosition;
            float handSpeed = handDelta.magnitude / Time.deltaTime;

            if (handSpeed > handSpeedThreshold)
            {
                Vector3 dynamicOffset = positionOffset * handSpeedMultiplier;
                transform.position = targetHandTransform.position + targetHandTransform.TransformDirection(dynamicOffset);
            }
            else
            {
                transform.position = targetHandTransform.position + targetHandTransform.TransformDirection(positionOffset);
            }

            transform.rotation = targetHandTransform.rotation * Quaternion.Euler(rotationOffset);

            if (canvas != null && scrollViewRectTransform != null)
            {
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(scrollViewRectTransform);
            }

            bool isIndexPinching = leftHand.GetFingerIsPinching(OVRHand.HandFinger.Index);
            if (isIndexPinching && !wasIndexPinching)
            {
                ToggleCanvasVisibility();
            }
            wasIndexPinching = isIndexPinching;
            lastHandPosition = targetHandTransform.position;

            // Auto-hide canvas after a delay
            if (isCanvasVisible && Time.time - lastToggleTime > autoHideDelay)
            {
                isCanvasVisible = false;
                canvas.enabled = false;
                Debug.Log("Canvas auto-hidden after delay.");
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleCanvasVisibility();
        }
        if (Input.GetKeyDown(saveKey))
        {
            SaveOffsets(presetName);
        }
        if (Input.GetKeyDown(loadKey))
        {
            LoadOffsets(presetName);
        }

        DetectThumbsUpGesture();
        if (isThumbsUpGesture && !wasThumbsUpGesture)
        {
            ToggleCanvasVisibility();
            Debug.Log("Canvas toggled with Thumbs Up gesture.");
        }
        wasThumbsUpGesture = isThumbsUpGesture;
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

    public void ToggleCanvasVisibility()
    {
        isCanvasVisible = !isCanvasVisible;
        canvas.enabled = isCanvasVisible;
        lastToggleTime = Time.time;
        Debug.Log("Canvas visibility toggled.");
    }

    public void SaveOffsets(string preset)
    {
        PlayerPrefs.SetFloat(preset + "_PositionOffsetX", positionOffset.x);
        PlayerPrefs.SetFloat(preset + "_PositionOffsetY", positionOffset.y);
        PlayerPrefs.SetFloat(preset + "_PositionOffsetZ", positionOffset.z);
        PlayerPrefs.SetFloat(preset + "_RotationOffsetX", rotationOffset.x);
        PlayerPrefs.SetFloat(preset + "_RotationOffsetY", rotationOffset.y);
        PlayerPrefs.SetFloat(preset + "_RotationOffsetZ", rotationOffset.z);
        PlayerPrefs.Save();
        Debug.Log("Offsets saved as " + preset);
    }

    public void LoadOffsets(string preset)
    {
        positionOffset.x = PlayerPrefs.GetFloat(preset + "_PositionOffsetX", 0);
        positionOffset.y = PlayerPrefs.GetFloat(preset + "_PositionOffsetY", 0);
        positionOffset.z = PlayerPrefs.GetFloat(preset + "_PositionOffsetZ", 0.1f);
        rotationOffset.x = PlayerPrefs.GetFloat(preset + "_RotationOffsetX", 0);
        rotationOffset.y = PlayerPrefs.GetFloat(preset + "_RotationOffsetY", 0);
        rotationOffset.z = PlayerPrefs.GetFloat(preset + "_RotationOffsetZ", 0);
        Debug.Log("Offsets loaded from " + preset);
    }

    private void DetectThumbsUpGesture()
    {
        isThumbsUpGesture = leftHand.GetFingerIsPinching(OVRHand.HandFinger.Thumb) &&
                            !leftHand.GetFingerIsPinching(OVRHand.HandFinger.Index) &&
                            !leftHand.GetFingerIsPinching(OVRHand.HandFinger.Middle) &&
                            !leftHand.GetFingerIsPinching(OVRHand.HandFinger.Ring) &&
                            !leftHand.GetFingerIsPinching(OVRHand.HandFinger.Pinky);
    }
}
