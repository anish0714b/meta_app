using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static OVRInput;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class LineScript : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private Texture2D texture;
    [SerializeField] private Shader shader;
    int width = 1200;
    int height = 13874;
    
    private Dictionary<string, Vector2> textureScales = new Dictionary<string, Vector2>(); 
    private float defaultScaleFactor = 0.0005f; 
    void Start()
    {
        if (text != null)
        {
            text.text = "Ready to interact";
        }
    }

    void Update()
    {
        if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
        {
            Vector3 controllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
            Quaternion controllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
            Vector3 rayDirection = controllerRotation * Vector3.forward;

            RaycastHit hit;
            if (Physics.Raycast(controllerPosition, rayDirection, out hit))
            {
                GameObject hitPlane = hit.collider.gameObject;

                if (hitPlane.transform.parent.name == "WALL_FACE")
                {
                    Debug.Log($"Hit: {hitPlane.transform.parent.name}");

                    ApplyTextureToPlane(hitPlane);
                }
                else
                {
                    Debug.Log($"{hitPlane.transform.parent.name} is not a wall");

                    if (text != null)
                    {
                        text.text = $"{hitPlane.transform.parent.name} is not a valid target.";
                    }
                }
            }
        }

        if (OVRInput.Get(OVRInput.Button.One))
        {
            GameObject[] wallFaces = GameObject.FindGameObjectsWithTag("WALL_FACE");
            foreach (GameObject wallFace in wallFaces)
            {
                ApplyTextureToPlane(wallFace);
            }
        }

        if (OVRInput.Get(OVRInput.Button.Two))
        {
            ResetAllPlanes();
        }

        if (OVRInput.Get(OVRInput.Button.Start))
        {
            SaveTextureSettings();
        }

        if (OVRInput.Get(OVRInput.Button.Back))
        {
            LoadTextureSettings();
        }
    }

    private void ApplyTextureToPlane(GameObject plane)
    {
        Material material = new Material(shader);
        material.mainTexture = texture;
        material.SetFloat("_Cull", (float)CullMode.Off);

        float scaleFactor = defaultScaleFactor;
        if (textureScales.ContainsKey(plane.name))
        {
            Vector2 savedScale = textureScales[plane.name];
            scaleFactor = savedScale.x / width; 
        }

        float imageWidth = width * scaleFactor;
        float imageHeight = height * scaleFactor;

        float planeWidth = plane.transform.localScale.x;
        float planeHeight = plane.transform.localScale.z;

        float tileX = planeWidth / imageWidth;
        float tileY = planeHeight / imageHeight;
        material.mainTextureScale = new Vector2(tileX, tileY);

        MeshRenderer planeRenderer = plane.GetComponentInParent<MeshRenderer>();
        planeRenderer.material = material;

        if (text != null)
        {
            text.text = $"Applied texture to {plane.name}";
        }
    }

    private void ResetAllPlanes()
    {
        GameObject[] wallFaces = GameObject.FindGameObjectsWithTag("WALL_FACE");
        foreach (GameObject wallFace in wallFaces)
        {
            MeshRenderer planeRenderer = wallFace.GetComponentInParent<MeshRenderer>();
            planeRenderer.material = null;

            if (text != null)
            {
                text.text = $"Reset {wallFace.name}";
            }
        }
    }

    private void SaveTextureSettings()
    {
        GameObject[] wallFaces = GameObject.FindGameObjectsWithTag("WALL_FACE");
        foreach (GameObject wallFace in wallFaces)
        {
            MeshRenderer planeRenderer = wallFace.GetComponentInParent<MeshRenderer>();
            if (planeRenderer != null && planeRenderer.material != null)
            {
                Vector2 textureScale = planeRenderer.material.mainTextureScale;
                textureScales[wallFace.name] = textureScale;
                PlayerPrefs.SetFloat($"{wallFace.name}_TileX", textureScale.x);
                PlayerPrefs.SetFloat($"{wallFace.name}_TileY", textureScale.y);
                Debug.Log($"Saved texture scale for {wallFace.name}: {textureScale}");
            }
        }
        PlayerPrefs.Save();
        if (text != null)
        {
            text.text = "Texture settings saved!";
        }
    }

    private void LoadTextureSettings()
    {
        GameObject[] wallFaces = GameObject.FindGameObjectsWithTag("WALL_FACE");
        foreach (GameObject wallFace in wallFaces)
        {
            if (PlayerPrefs.HasKey($"{wallFace.name}_TileX") && PlayerPrefs.HasKey($"{wallFace.name}_TileY"))
            {
                float tileX = PlayerPrefs.GetFloat($"{wallFace.name}_TileX");
                float tileY = PlayerPrefs.GetFloat($"{wallFace.name}_TileY");
                textureScales[wallFace.name] = new Vector2(tileX, tileY);
                Debug.Log($"Loaded texture scale for {wallFace.name}: {tileX}, {tileY}");
                ApplyTextureToPlane(wallFace); // Reapply texture with loaded settings
            }
        }
        if (text != null)
        {
            text.text = "Texture settings loaded!";
        }
    }

    public void AdjustScaleFactor(float newScaleFactor)
    {
        defaultScaleFactor = newScaleFactor;
        Debug.Log($"Default scale factor adjusted to {defaultScaleFactor}");
    }

    public void PrintDebugInfo()
    {
        Debug.Log("Current texture scales:");
        foreach (var kvp in textureScales)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value}");
        }
    }

    public void ChangeShader(Shader newShader)
    {
        shader = newShader;
        Debug.Log($"Shader changed to {newShader.name}");
    }
}
