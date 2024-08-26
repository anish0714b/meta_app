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
    [SerializeField] private List<Texture2D> textures; 
    [SerializeField] private float transparency = 1.0f; 
    private int currentTextureIndex = 0; 
    int width = 1200;
    int height = 13874;
    
    private Dictionary<string, Vector2> textureScales = new Dictionary<string, Vector2>(); 
    private Stack<string> undoStack = new Stack<string>(); 
    private float defaultScaleFactor = 0.0005f; 
    private bool debugMode = false; 

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
                    undoStack.Push(hitPlane.name); 
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
                undoStack.Push(wallFace.name);
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

        if (OVRInput.GetDown(OVRInput.Button.Four)) 
        {
            CycleTexture();
        }

        if (OVRInput.GetDown(OVRInput.Button.Three)) 
        {
            UndoLastTexture();
        }

        if (OVRInput.Get(OVRInput.Button.PrimaryThumbstick)) 
        {
            AdjustTransparency(OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x);
        }

        if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger)) 
        {
            ToggleDebugMode();
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
        material.color = new Color(1, 1, 1, transparency); // Set transparency

        MeshRenderer planeRenderer = plane.GetComponentInParent<MeshRenderer>();
        planeRenderer.material = material;

        if (text != null)
        {
            text.text = $"Applied texture to {plane.name}";
        }

        if (debugMode)
        {
            Debug.Log($"Applied texture to {plane.name} with scale: {tileX}, {tileY}");
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

            if (debugMode)
            {
                Debug.Log($"Reset texture for {wallFace.name}");
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

    private void CycleTexture()
    {
        currentTextureIndex = (currentTextureIndex + 1) % textures.Count;
        texture = textures[currentTextureIndex];
        if (text != null)
        {
            text.text = $"Texture changed to {texture.name}";
        }
    }

    private void UndoLastTexture()
    {
        if (undoStack.Count > 0)
        {
            string lastPlaneName = undoStack.Pop();
            GameObject lastPlane = GameObject.Find(lastPlaneName);
            if (lastPlane != null)
            {
                MeshRenderer planeRenderer = lastPlane.GetComponentInParent<MeshRenderer>();
                planeRenderer.material = null;
                if (text != null)
                {
                    text.text = $"Undid texture on {lastPlaneName}";
                }
                if (debugMode)
                {
                    Debug.Log($"Undid texture on {lastPlaneName}");
                }
            }
        }
    }

    private void AdjustTransparency(float adjustment)
    {
        transparency = Mathf.Clamp01(transparency + adjustment * 0.1f);
        if (text != null)
        {
            text.text = $"Transparency adjusted to {transparency}";
        }
    }

    private void ToggleDebugMode()
    {
        debugMode = !debugMode;
        Debug.Log($"Debug mode: {debugMode}");
    }
}
