using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace LudicWorlds
{
    public class DebugPanel : MonoBehaviour
    {
        private static Canvas _canvas;
        private static Text _debugText;
        private static Text _fpsText;
        private static Text _statusText;
        private static Text _memoryText;  
        private static Text _logCountText; 
        private static Text _errorText; 
        private static Button _clearButton;
        private static Dropdown _logFilterDropdown; 

        private float _elapsedTime;
        private uint _fpsSamples;
        private float _sumFps;
        private int _logCount;  
        private const int MAX_LINES = 23;
        private const float MEMORY_UPDATE_INTERVAL = 1.0f;  
        private const string MEMORY_FORMAT = "Memory: {0} MB"; 

        private Transform _cameraTransform;
        private Vector3 _dirToPlayer = Vector3.zero;

        private bool _isVerboseLogging = false;  
        private static DebugPanel _instance;

        void Awake()
        {
            AcquireObjects();
            _elapsedTime = 0;
            _fpsSamples = 0;
            _fpsText.text = "0";
            _logCount = 0;  
            _memoryText.text = string.Format(MEMORY_FORMAT, GetMemoryUsage());  
            _errorText.text = ""; 

            Application.logMessageReceived += HandleLog;
            _instance = this;  
        }

        void Start()
        {
            _cameraTransform = Camera.main.transform;
            StartCoroutine(UpdateMemoryUsage());  

            SetupClearButton(); 
            SetupLogFilterDropdown(); 
        }

        void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void AcquireObjects()
        {
            _canvas = this.gameObject.GetComponent<Canvas>();
            Transform ui = this.transform.Find("UI");

            _debugText = ui.Find("DebugText").GetComponent<Text>();
            _fpsText = ui.Find("FpsText").GetComponent<Text>();
            _statusText = ui.Find("StatusText").GetComponent<Text>();
            _memoryText = ui.Find("MemoryText").GetComponent<Text>();  
            _logCountText = ui.Find("LogCountText").GetComponent<Text>();  
            _errorText = ui.Find("ErrorText").GetComponent<Text>(); 

            _clearButton = ui.Find("ClearButton").GetComponent<Button>(); 
            _logFilterDropdown = ui.Find("LogFilterDropdown").GetComponent<Dropdown>(); 
        }

        void HandleLog(string message, string stackTrace, LogType type)
        {
            _logCount++;  
            UpdateLogCount(); 

            if (_isVerboseLogging || type == LogType.Error || type == LogType.Exception)
            {
                _debugText.text += (message + "\n");
                TrimText();
            }

            if (type == LogType.Error || type == LogType.Exception)
            {
                _errorText.text = message; 
            }
        }

        void Update()
        {
            _elapsedTime += Time.deltaTime;

            if (_elapsedTime > 0.5f)
            {                
                _fpsText.text = (Mathf.Round((_sumFps / _fpsSamples))).ToString();

                _elapsedTime = 0f;
                _sumFps = 0f;
                _fpsSamples = 0;
            }

            _sumFps += (1.0f / Time.smoothDeltaTime);
            _fpsSamples++;
            _dirToPlayer = (this.transform.position - _cameraTransform.position).normalized;
            _dirToPlayer.y = 0; 
            this.transform.rotation = Quaternion.LookRotation(_dirToPlayer);
        }

        private void SetupClearButton()
        {
            _clearButton.onClick.AddListener(Clear); 
        }

        private void SetupLogFilterDropdown()
        {
            _logFilterDropdown.options.Clear();
            _logFilterDropdown.options.Add(new Dropdown.OptionData("All Logs"));
            _logFilterDropdown.options.Add(new Dropdown.OptionData("Errors Only"));
            _logFilterDropdown.options.Add(new Dropdown.OptionData("Verbose Only"));
            _logFilterDropdown.onValueChanged.AddListener(delegate { FilterLogs(_logFilterDropdown.value); });
        }

        private void FilterLogs(int filterType)
        {
            switch (filterType)
            {
                case 0:
                    _isVerboseLogging = true;
                    break;
                case 1:
                    _isVerboseLogging = false; 
                case 2:
                    _isVerboseLogging = true; 
                    break;
            }
        }

        public static void Clear()
        {
            if (_debugText == null) return;
            _debugText.text = "";
            _instance._logCount = 0;  
            _instance.UpdateLogCount();  
        }

        public static void Show()
        {
            SetVisibility(true);
        }

        public static void Hide()
        {
            SetVisibility(false);
        }

        public static void SetVisibility(bool visible)
        {
            if (_canvas == null) return;
            _canvas.enabled = visible;
        }

        public static void ToggleVisibility()
        {
            if (_canvas == null) return;
            _canvas.enabled = !_canvas.enabled;
        }

        public static void SetStatus(string message)
        {
            if (_statusText == null) return;
            _statusText.text = (message);
        }

        private static void TrimText()
        {
            string[] lines = _debugText.text.Split('\n');

            if (lines.Length > MAX_LINES)
            {
                _debugText.text = string.Join("\n", lines, lines.Length - MAX_LINES, MAX_LINES);
            }
        }

        private IEnumerator UpdateMemoryUsage()
        {
            while (true)
            {
                _memoryText.text = string.Format(MEMORY_FORMAT, GetMemoryUsage());  
                yield return new WaitForSeconds(MEMORY_UPDATE_INTERVAL);
            }
        }

        private float GetMemoryUsage()
        {
            return (float)System.GC.GetTotalMemory(false) / (1024 * 1024);
        }

        private void UpdateLogCount()
        {
            if (_logCountText != null)
            {
                _logCountText.text = $"Logs: {_logCount}";
            }
        }

        public static void ToggleVerboseLogging()
        {
            if (_instance != null)
            {
                _instance._isVerboseLogging = !_instance._isVerboseLogging;
                Debug.Log("Verbose logging " + (_instance._isVerboseLogging ? "enabled" : "disabled"));
            }
        }

        public static void DisplayCustomPanel(string title, string content)
        {
            GameObject customPanel = new GameObject("CustomPanel");
            customPanel.transform.SetParent(_canvas.transform);

            RectTransform rectTransform = customPanel.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(300, 200);

            Text panelText = customPanel.AddComponent<Text>();
            panelText.font = _debugText.font;
            panelText.fontSize = 14;
            panelText.alignment = TextAnchor.UpperLeft;
            panelText.text = $"{title}\n{content}";

            Image panelImage = customPanel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.7f);

            rectTransform.anchoredPosition = Vector2.zero;
        }
    }
}
