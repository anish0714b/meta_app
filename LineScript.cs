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

                    Material material = new Material(shader);
                    material.mainTexture = texture;
                    material.SetFloat("_Cull", (float)CullMode.Off);
                    float scaleFactor = 0.0005f;
                    float imageWidth = width * scaleFactor;
                    float imageHeight = height * scaleFactor;
                    float planeWidth = hitPlane.transform.localScale.x;
                    float planeHeight = hitPlane.transform.localScale.z;
                    float tileX = planeWidth / imageWidth;
                    float tileY = planeHeight / imageHeight;
                    material.mainTextureScale = new Vector2(tileX, tileY);
                    MeshRenderer planeRenderer = hitPlane.GetComponentInParent<MeshRenderer>();
                    planeRenderer.material = material;

                    if (text != null)
                    {
                        text.text = "Texture applied!";
                    }
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
    }

    private void ApplyTextureToPlane(GameObject plane)
    {
        Material material = new Material(shader);
        material.mainTexture = texture;
        material.SetFloat("_Cull", (float)CullMode.Off);

        float scaleFactor = 0.0005f;
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
}
