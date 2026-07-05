using UnityEngine;

public class PanelRouter : MonoBehaviour
{
    [SerializeField] private MenuPanel[] _panels;
    [SerializeField] private MenuPanel _defaultPanel;

    private void Start()
    {
        if (_defaultPanel != null)
            Show(_defaultPanel);
    }

    public void Show(MenuPanel panel)
    {
        foreach (var p in _panels)
            p.Hide();

        panel.Show();
    }
}