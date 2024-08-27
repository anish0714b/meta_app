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
    private bool isScrolling = false;
    private Vector3 initialTouchPosition;
    private float scrollSpeed = 0.1f;
    private float scrollLimit = 100f;

    void Start()
    {
        textures = Resources.LoadAll<Texture2D>("panels"); 
        gridLayout = content.GetComponent<GridLayoutGroup>();
        for (int i = 0; i < textures.Length; i++)
        {
            GameObject newPanel = Instantiate(panel, content.transform); 
            Image childImage = newPanel.GetComponent<Image>();
            childImage.sprite = Sprite.Create(textures[i], new Rect(0, 0, textures[i].width, textures[i].height), Vector2.one * 0.5f); 
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
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetTextureSelection(); 
        }
        if (Input.GetMouseButtonDown(0))
        {
            isScrolling = true;
            initialTouchPosition = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0))
        {
            isScrolling = false;
        }
        if (isScrolling)
        {
            Vector3 currentTouchPosition = Input.mousePosition;
            Vector3 delta = currentTouchPosition - initialTouchPosition;
            content.transform.Translate(-delta.x * scrollSpeed, 0, 0);
            initialTouchPosition = currentTouchPosition;
            if (Mathf.Abs(content.transform.localPosition.x) > scrollLimit)
            {
                content.transform.localPosition = new Vector3(Mathf.Sign(content.transform.localPosition.x) * scrollLimit, content.transform.localPosition.y, content.transform.localPosition.z);
            }
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            RemoveAllPanels();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            AddPanel();
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
    }

    void SaveSelectedTexture()
    {
        PlayerPrefs.SetInt("SelectedTextureIndex", currentTextureIndex);
        PlayerPrefs.Save();
    }

    void LoadSelectedTexture()
    {
        if (PlayerPrefs.HasKey("SelectedTextureIndex"))
        {
            currentTextureIndex = PlayerPrefs.GetInt("SelectedTextureIndex");
            SwitchTexture(0); 
        }
    }

    public void ResetTextureSelection()
    {
        currentTextureIndex = 0;
        SwitchTexture(0);
    }

    public void RemoveAllPanels()
    {
        foreach (GameObject panel in panels)
        {
            Destroy(panel);
        }
        panels.Clear();
    }

    public void AddPanel()
    {
        if (textures.Length > 0)
        {
            GameObject newPanel = Instantiate(panel, content.transform);
            Image childImage = newPanel.GetComponent<Image>();
            childImage.sprite = Sprite.Create(textures[currentTextureIndex], new Rect(0, 0, textures[currentTextureIndex].width, textures[currentTextureIndex].height), Vector2.one * 0.5f);
            panels.Add(newPanel);
            AdjustContentSize();
        }
    }

    public void RandomizePanelOrder()
    {
        for (int i = 0; i < panels.Count; i++)
        {
            GameObject temp = panels[i];
            int randomIndex = Random.Range(i, panels.Count);
            panels[i] = panels[randomIndex];
            panels[randomIndex] = temp;
        }

        for (int i = 0; i < panels.Count; i++)
        {
            panels[i].transform.SetSiblingIndex(i);
        }
    }

    public void RotatePanels(float angle)
    {
        foreach (GameObject panel in panels)
        {
            panel.transform.Rotate(Vector3.forward, angle);
        }
    }

    public void SetScrollSpeed(float speed)
    {
        scrollSpeed = speed;
    }

    public void EnableScrolling(bool enable)
    {
        isScrolling = enable;
    }

    public void SetScrollLimit(float limit)
    {
        scrollLimit = limit;
    }

    public void ResetScrollPosition()
    {
        content.transform.localPosition = Vector3.zero;
    }
public void SetPanelColor(Color color)
{
    foreach (GameObject panel in panels)
    {
        Image image = panel.GetComponent<Image>();
        image.color = color;
    }
}

public void HighlightPanel(int index, Color highlightColor)
{
    if (index >= 0 && index < panels.Count)
    {
        Image image = panels[index].GetComponent<Image>();
        image.color = highlightColor;
    }
}

public void RevertPanelHighlight(int index)
{
    if (index >= 0 && index < panels.Count)
    {
        Image image = panels[index].GetComponent<Image>();
        image.color = Color.white;
    }
}

public void SetPanelRotation(int index, float angle)
{
    if (index >= 0 && index < panels.Count)
    {
        panels[index].transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}

public void ResetPanelRotation(int index)
{
    if (index >= 0 && index < panels.Count)
    {
        panels[index].transform.rotation = Quaternion.identity;
    }
}

public void SetPanelScale(int index, float scale)
{
    if (index >= 0 && index < panels.Count)
    {
        panels[index].transform.localScale = Vector3.one * scale;
    }
}

public void ResetPanelScale(int index)
{
    if (index >= 0 && index < panels.Count)
    {
        panels[index].transform.localScale = Vector3.one;
    }
}

public void SetPanelBorder(int index, float borderWidth, Color borderColor)
{
    if (index >= 0 && index < panels.Count)
    {
        Image image = panels[index].GetComponent<Image>();
        image.material = new Material(Shader.Find("UI/Default"));
        image.material.SetFloat("_Border", borderWidth);
        image.material.SetColor("_BorderColor", borderColor);
    }
}

public void ResetPanelBorder(int index)
{
    if (index >= 0 && index < panels.Count)
    {
        Image image = panels[index].GetComponent<Image>();
        image.material.SetFloat("_Border", 0);
    }
}

public void SetPanelText(int index, string text)
{
    if (index >= 0 && index < panels.Count)
    {
        Text panelText = panels[index].GetComponentInChildren<Text>();
        if (panelText != null)
        {
            panelText.text = text;
        }
    }
}

public void SetPanelTextColor(int index, Color color)
{
    if (index >= 0 && index < panels.Count)
    {
        Text panelText = panels[index].GetComponentInChildren<Text>();
        if (panelText != null)
        {
            panelText.color = color;
        }
    }
}

public void ResetPanelText(int index)
{
    if (index >= 0 && index < panels.Count)
    {
        Text panelText = panels[index].GetComponentInChildren<Text>();
        if (panelText != null)
        {
            panelText.text = string.Empty;
        }
    }
}

public void SetPanelTextSize(int index, int size)
{
    if (index >= 0 && index < panels.Count)
    {
        Text panelText = panels[index].GetComponentInChildren<Text>();
        if (panelText != null)
        {
            panelText.fontSize = size;
        }
    }
}

}
