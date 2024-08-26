using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using static OVRInput;
using SelfButton = UnityEngine.UI.Button;

public class NewPanel : MonoBehaviour
{
    private Texture2D[] textures;
    public GameObject content;
    public GameObject panel;
    public GameObject selectedPanelDisplay;
    private Texture2D currentTexture;
    [SerializeField] private Shader shader;
    private GameObject[] borderPanels;
    int width = 1200;
    int height = 13874;
    public OVRHand hand;
    private Vector3 originalScale;

    private bool isDragging = false;
    private Vector3 dragStartPos;
    private GameObject selectedPanel;

    void Start()
    {
        textures = Resources.LoadAll<Texture2D>("panels");
        borderPanels = new GameObject[textures.Length];

        for (int i = 0; i < textures.Length; i++)
        {
            GameObject newPanel = Instantiate(panel, content.transform);
            Image childImage = newPanel.GetComponent<Image>();
            childImage.sprite = Sprite.Create(textures[i], new Rect(0, 0, textures[i].width, textures[i].height), Vector2.one * 0.5f);

            SelfButton button = newPanel.GetComponent<SelfButton>();
            int temp = i;
            button.onClick.AddListener(() => OnPanelClick(temp));

            borderPanels[i] = newPanel;
            newPanel.transform.localPosition = new Vector3(0, -i * (textures[i].height + 10), 0);
        }

        currentTexture = textures[0];
        originalScale = borderPanels[0].transform.localScale;
        UpdateSelectedPanelDisplay();
    }

    void Update()
    {
        HandleInput();

        if (isDragging)
        {
            Vector3 controllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
            selectedPanel.transform.position = controllerPosition + dragStartPos;
        }
    }

    void HandleInput()
    {
        if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) || 
            OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger) ||
            hand.GetFingerIsPinching(OVRHand.HandFinger.Index))
        {
            Vector3 controllerPosition;
            Quaternion controllerRotation;
            bool isHandPinching = hand.GetFingerIsPinching(OVRHand.HandFinger.Index);

            if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger))
            {
                controllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
                controllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch);
            }
            else if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
            {
                controllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
                controllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
            }
            else if (isHandPinching)
            {
                controllerPosition = hand.PointerPose.position;
                controllerRotation = hand.PointerPose.rotation;
                Debug.Log("Index finger is pinching");
            }
            else
            {
                return;
            }

            Vector3 rayDirection = controllerRotation * Vector3.forward;
            RaycastHit hit;

            if (Physics.Raycast(controllerPosition, rayDirection, out hit))
            {
                GameObject hitPlane = hit.collider.gameObject;
                if (hitPlane.transform.parent == null) return;
                if (hitPlane.transform.parent.name == "WALL_FACE")
                {
                    Debug.Log($"Hit: {hitPlane.transform.parent.name}");

                    Material material = new Material(shader);
                    material.mainTexture = currentTexture;
                    material.SetFloat("_Cull", (float)CullMode.Off);
                    material.SetFloat("_EnvironmentDepthBias", 0.06f);

                    float scaleFactor = 0.0001f;
                    float imageWidth = width * scaleFactor;
                    float imageHeight = height * scaleFactor;

                    float planeWidth = hitPlane.transform.localScale.x;
                    float planeHeight = hitPlane.transform.localScale.z;

                    float tileX = planeWidth / imageWidth;
                    float tileY = planeHeight / imageHeight;
                    material.mainTextureScale = new Vector2(tileX, tileY);

                    MeshRenderer planeRenderer = hitPlane.GetComponentInParent<MeshRenderer>();
                    planeRenderer.material = material;
                }
                else
                {
                    Debug.Log($"{hitPlane.transform.parent.name} is not a wall");
                }
            }
        }

        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            Debug.Log("One button pressed");
            if (selectedPanel != null)
            {
                isDragging = true;
                dragStartPos = selectedPanel.transform.position - OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
            }
        }

        if (OVRInput.GetUp(OVRInput.Button.One))
        {
            Debug.Log("One button released");
            isDragging = false;
        }
    }

    void OnPanelClick(int t)
    {
        Debug.Log(t);
        currentTexture = textures[t];
        UpdateSelectedPanelDisplay();

        for (int i = 0; i < textures.Length; i++)
        {
            if (i == t)
            {
                borderPanels[i].transform.localScale = new Vector3(originalScale.x * 1.2f, originalScale.y * 1.2f, originalScale.z);
                selectedPanel = borderPanels[i];
            }
            else
            {
                borderPanels[i].transform.localScale = originalScale;
            }
        }
    }

    void UpdateSelectedPanelDisplay()
    {
        Image displayImage = selectedPanelDisplay.GetComponent<Image>();
        displayImage.sprite = Sprite.Create(currentTexture, new Rect(0, 0, currentTexture.width, currentTexture.height), Vector2.one * 0.5f);
    }

    public void ResetPanelPositions()
    {
        for (int i = 0; i < borderPanels.Length; i++)
        {
            borderPanels[i].transform.localPosition = new Vector3(0, -i * (textures[i].height + 10), 0);
        }
    }
}
