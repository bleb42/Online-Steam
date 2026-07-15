using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MenuPanel
{
    [Serializable]
    private class TabEntry
    {
        public Button TabButton;
        public GameObject TabContent;
    }

    [SerializeField] private Button _btnBack;
    [SerializeField] private List<TabEntry> _tabs;

    [Header("Tab Colors")]
    [SerializeField] private Color _selectedColor = Color.white;
    [SerializeField] private Color _normalColor = new Color(0.6f, 0.6f, 0.6f);

    public event Action OnBackClicked;

    private void Awake()
    {
        _btnBack.onClick.AddListener(() => OnBackClicked?.Invoke());

        foreach (var tab in _tabs)
        {
            var captured = tab;
            captured.TabButton.onClick.AddListener(() => ShowTab(captured));
        }
    }

    public override void Show()
    {
        base.Show();
        if (_tabs.Count > 0)
            ShowTab(_tabs[0]);
    }

    private void ShowTab(TabEntry selectedTab)
    {
        foreach (var tab in _tabs)
        {
            bool isSelected = tab == selectedTab;
            tab.TabContent.SetActive(isSelected);
            tab.TabButton.image.color = isSelected ? _selectedColor : _normalColor;
        }
    }
}