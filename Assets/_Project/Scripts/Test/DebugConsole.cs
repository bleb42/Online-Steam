using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Persistent debug console. Auto-creates itself at game start (no need to place it
/// in any scene), survives scene loads, and is reachable everywhere via Instance.
///
/// Usage:
///     DebugConsole.Instance.Print("Something happened");
///     DebugConsole.Instance.Print("Bad thing", LogType.Error);
///     DebugConsole.Log("Static shortcut, safe even if Instance not ready yet");
///
/// All normal Debug.Log / LogWarning / LogError calls anywhere in the project
/// are also captured automatically and show up here.
///
/// Toggle visibility with the configured key (default: ` / backquote).
/// </summary>
public class DebugConsole : MonoBehaviour
{
    public static DebugConsole Instance { get; private set; }

    [Header("Toggle")]
    [SerializeField] private Key _toggleKey = Key.P;

    [Header("Settings")]
    [SerializeField] private int _maxEntries = 500;
    [SerializeField] private bool _captureUnityLogs = true;
    [SerializeField] private bool _startVisible = false;

    private struct LogEntry
    {
        public string Timestamp;
        public string Message;
        public LogType Type;
    }

    private readonly List<LogEntry> _entries = new List<LogEntry>();
    private readonly StringBuilder _displayBuilder = new StringBuilder();

    private bool _visible;
    private bool _dirty = true;
    private string _cachedDisplay = "";
    private Vector2 _scrollPosition;
    private bool _autoScroll = true;
    private string _filter = "";
    private Rect _windowRect = new Rect(20, 20, 750, 480);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null)
            return;

        var go = new GameObject("DebugConsole");
        go.AddComponent<DebugConsole>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        _visible = _startVisible;
    }

    private void OnEnable()
    {
        if (_captureUnityLogs)
            Application.logMessageReceived += HandleUnityLog;
    }

    private void OnDisable()
    {
        if (_captureUnityLogs)
            Application.logMessageReceived -= HandleUnityLog;
    }

    private void Update()
    {
        var keyboard = Keyboard.current;

        if (keyboard != null && keyboard[_toggleKey].wasPressedThisFrame)
        {
            _visible = !_visible;
        }
    }

    private void HandleUnityLog(string message, string stackTrace, LogType type)
    {
        Add(message, type);
    }

    public void Print(object message) => Add(message?.ToString() ?? "null", LogType.Log);

    public void Print(object message, LogType type) => Add(message?.ToString() ?? "null", type);

    public static void Log(object message)
    {
        if (Instance != null)
            Instance.Print(message);
    }

    public static void LogWarning(object message)
    {
        if (Instance != null)
            Instance.Print(message, LogType.Warning);
    }

    public static void LogError(object message)
    {
        if (Instance != null)
            Instance.Print(message, LogType.Error);
    }

    public void Clear()
    {
        _entries.Clear();
        _dirty = true;
    }

    public void Show() => _visible = true;
    public void Hide() => _visible = false;

    private void Add(string message, LogType type)
    {
        _entries.Add(new LogEntry
        {
            Timestamp = DateTime.Now.ToString("HH:mm:ss.fff"),
            Message = message,
            Type = type
        });

        if (_entries.Count > _maxEntries)
            _entries.RemoveAt(0);

        _dirty = true;
    }

    private void OnGUI()
    {
        if (!_visible)
            return;

        _windowRect = GUILayout.Window(GetInstanceID(), _windowRect, DrawWindow, "Debug Console");
    }

    private void DrawWindow(int id)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Filter:", GUILayout.Width(45));
        string newFilter = GUILayout.TextField(_filter, GUILayout.Width(200));
        if (newFilter != _filter)
        {
            _filter = newFilter;
            _dirty = true;
        }

        _autoScroll = GUILayout.Toggle(_autoScroll, "Auto-scroll", GUILayout.Width(95));

        if (GUILayout.Button("Copy All", GUILayout.Width(70)))
            GUIUtility.systemCopyBuffer = BuildFullText();

        if (GUILayout.Button("Clear", GUILayout.Width(60)))
            Clear();

        if (GUILayout.Button("Close", GUILayout.Width(60)))
            _visible = false;

        GUILayout.EndHorizontal();

        if (_dirty)
        {
            RebuildDisplay();
            _dirty = false;
        }

        if (_autoScroll)
            _scrollPosition.y = float.MaxValue;

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandHeight(true));
        GUILayout.Label(_cachedDisplay);
        GUILayout.EndScrollView();

        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }

    private void RebuildDisplay()
    {
        _displayBuilder.Clear();

        for (int i = 0; i < _entries.Count; i++)
        {
            var entry = _entries[i];

            if (!string.IsNullOrEmpty(_filter) &&
                entry.Message.IndexOf(_filter, StringComparison.OrdinalIgnoreCase) < 0)
            {
                continue;
            }

            string color = GetColor(entry.Type);
            _displayBuilder.Append("<color=").Append(color).Append(">[")
                .Append(entry.Timestamp).Append("] ")
                .Append(entry.Message).Append("</color>\n");
        }

        _cachedDisplay = _displayBuilder.ToString();
    }

    private string BuildFullText()
    {
        var sb = new StringBuilder();
        foreach (var entry in _entries)
            sb.Append('[').Append(entry.Timestamp).Append("] ").Append(entry.Message).Append('\n');
        return sb.ToString();
    }

    private static string GetColor(LogType type)
    {
        switch (type)
        {
            case LogType.Error:
            case LogType.Exception:
                return "#FF5555";
            case LogType.Warning:
                return "#FFD866";
            case LogType.Assert:
                return "#FF88FF";
            default:
                return "#DDDDDD";
        }
    }
}