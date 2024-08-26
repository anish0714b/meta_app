using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class PanelMaterial : MonoBehaviour
{
    private Texture2D[] textures; 
    public GameObject content;
    public GameObject panel; 

    private List<GameObject> panels = new List<GameObject>(); 
    private int currentTextureIndex = 0; 
    private GridLayoutGroup gridLayout;
    void Start()
    {
        textures = Resources.LoadAll<Texture2D>("panels"); 
        gridLayout = content.GetComponent<GridLayoutGroup>();
        for (int i = 0; i < textures.Length; i++)
        {
            GameObject newPanel = Instantiate(panel, content.transform); 
            Image childImage = newPanel.GetComponent<Image>();
            childImage.sprite = Sprite.Create(textures[i], new Rect(0, 0, textures[i].width, textures[i].height), Vector2.one * 0.5f); // Set the texture to the Image component
            panels.Add(newPanel); 
        }

        AdjustContentSize(); 
        LoadSelectedTexture(); 
    }


        void Update()
    {

                if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            SwitchTexture(1); 
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            SwitchTexture(-1); 
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveSelectedTexture(); 
        }
    }


        void AdjustContentSize()
    {
        if (gridLayout != null)
        {
            int totalPanels = textures.Length;
            float totalWidth = totalPanels * (gridLayout.cellSize.x + gridLayout.spacing.x);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.sizeDelta = new Vector2(totalWidth, contentRect.sizeDelta.y);
        }
    }


        void SwitchTexture(int offset)
    {
        currentTextureIndex = (currentTextureIndex + offset + textures.Length) % textures.Length;
        Texture2D newTexture = textures[currentTextureIndex];

        foreach (GameObject panel in panels)
        {
            Image childImage = panel.GetComponent<Image>();
            childImage.sprite = Sprite.Create(newTexture, new Rect(0, 0, newTexture.width, newTexture.height), Vector2.one * 0.5f);
        }

        Debug.Log($"Switched to texture: {newTexture.name}");
    }


        void SaveSelectedTexture()
    {
        PlayerPrefs.SetInt("SelectedTextureIndex", currentTextureIndex);
        PlayerPrefs.Save();
        Debug.Log($"Saved texture index: {currentTextureIndex}");
    }


        void LoadSelectedTexture()
    {
        if (PlayerPrefs.HasKey("SelectedTextureIndex"))
        {
            currentTextureIndex = PlayerPrefs.GetInt("SelectedTextureIndex");
            SwitchTexture(0); 
            
            Debug.Log($"Loaded texture index: {currentTextureIndex}");
        }
        else
        {
            Debug.Log("No saved texture found. Using default.");
        }
    }
    public void ResetTextureSelection()
    {
        currentTextureIndex = 0;
        SwitchTexture(0);
        Debug.Log("Reset texture selection to default.");
    }
    public void RemoveAllPanels()
    {
        foreach (GameObject panel in panels)
        {
            Destroy(panel);
        }
        panels.Clear();
        Debug.Log("All panels removed.");
    }
}
