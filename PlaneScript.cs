using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static OVRInput;
using TMPro;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.Rendering;
using Meta.XR.MRUtilityKit;

public class PlaneScript : MonoBehaviour
{
    [SerializeField] private Texture2D texture;
    [SerializeField] private Shader shader;
    int width = 1200;
    int height = 13874;

    public Slider scaleSlider;
    public TextMeshProUGUI materialPropertiesText;
    public AudioClip hitSound;
    private AudioSource audioSource;

    private GameObject selectionIndicator;
    private Material lastAppliedMaterial;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        CreateSelectionIndicator();
    }

    void Update()
    {
        if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger))
        {
            Debug.Log("press");
            Vector3 controllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
            Quaternion controllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch);
            Vector3 rayDirection = controllerRotation * Vector3.forward;

            RaycastHit hit;
            if (Physics.Raycast(controllerPosition, rayDirection, out hit))
            {
                GameObject hitPlane = hit.collider.gameObject;

                OVRSemanticClassification anchor = hitPlane.GetComponentInParent<OVRSemanticClassification>();
                List<string> labelsList = (List<string>)anchor.Labels;
                Debug.Log(labelsList.ToString());

                if (labelsList.Contains("WALL_FACE"))
                {
                    Debug.Log($"Hit: {string.Join(", ", anchor.Labels)}");

                    PlayHitSound();

                    Material material = new Material(shader);
                    material.mainTexture = texture;
                    material.SetFloat("_Cull", (float)CullMode.Off);

                    float scaleFactor = scaleSlider != null ? scaleSlider.value : 0.0005f;
                    float imageWidth = width * scaleFactor;
                    float imageHeight = height * scaleFactor;

                    float planeWidth = hitPlane.transform.localScale.x;
                    float planeHeight = hitPlane.transform.localScale.z;

                    float tileX = planeWidth / imageWidth;
                    float tileY = planeHeight / imageHeight;
                    material.mainTextureScale = new Vector2(tileX, tileY);

                    MeshRenderer planeRenderer = hitPlane.GetComponentInParent<MeshRenderer>();
                    lastAppliedMaterial = material;
                    planeRenderer.material = material;

                    UpdateMaterialPropertiesText(material);
                    DisplaySelectionIndicator(hitPlane);
                }
            }
        }

        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            ClearMaterial();
        }
    }

    private void PlayHitSound()
    {
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
    }

    private void CreateSelectionIndicator()
    {
        selectionIndicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
        selectionIndicator.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        selectionIndicator.GetComponent<Renderer>().material.color = Color.green;
        selectionIndicator.SetActive(false);
    }

    private void DisplaySelectionIndicator(GameObject hitPlane)
    {
        selectionIndicator.SetActive(true);
        selectionIndicator.transform.position = hitPlane.transform.position + new Vector3(0, hitPlane.transform.localScale.y / 2 + 0.1f, 0);
        selectionIndicator.transform.localScale = hitPlane.transform.localScale * 1.1f;
    }

    private void UpdateMaterialPropertiesText(Material material)
    {
        if (materialPropertiesText != null)
        {
            materialPropertiesText.text = $"Material Properties:\n" +
                                          $"Texture: {material.mainTexture.name}\n" +
                                          $"Scale: {material.mainTextureScale.x:F2}, {material.mainTextureScale.y:F2}\n" +
                                          $"Shader: {material.shader.name}";
        }
    }

    private void ClearMaterial()
    {
        if (lastAppliedMaterial != null)
        {
            Destroy(lastAppliedMaterial);
            lastAppliedMaterial = null;
            selectionIndicator.SetActive(false);
            materialPropertiesText.text = "No Material Applied";
        }
    }

    public void AdjustMaterialProperty(float value)
    {
        if (lastAppliedMaterial != null)
        {
            float newScaleFactor = value;
            float newWidth = width * newScaleFactor;
            float newHeight = height * newScaleFactor;

            lastAppliedMaterial.mainTextureScale = new Vector2(newWidth, newHeight);
            UpdateMaterialPropertiesText(lastAppliedMaterial);
        }
    }

    public void ResetScale()
    {
        if (scaleSlider != null)
        {
            scaleSlider.value = 0.0005f;
            AdjustMaterialProperty(scaleSlider.value);
        }
    }
}
