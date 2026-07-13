using UnityEngine;

/// <summary>
/// Раньше SetValid() каждый кадр вызывал r.materials (getter), а это создаёт
/// НОВЫЙ экземпляр материала при каждом обращении — то есть каждый кадр,
/// пока активен build mode, в память утекал материал. Теперь красим через
/// MaterialPropertyBlock и трогаем рендереры только при реальной смене состояния.
/// </summary>
public class BuildGhost : MonoBehaviour
{
    [SerializeField] private Renderer[] _renderers;
    [SerializeField] private Color _validColor = new Color(0f, 1f, 0f, 0.4f);
    [SerializeField] private Color _invalidColor = new Color(1f, 0f, 0f, 0.4f);

    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    private MaterialPropertyBlock _mpb;
    private bool? _lastValid;

    private void Awake()
    {
        _mpb = new MaterialPropertyBlock();
    }

    public void SetValid(bool isValid)
    {
        if (_lastValid == isValid) return;
        _lastValid = isValid;

        Color color = isValid ? _validColor : _invalidColor;
        foreach (var r in _renderers)
        {
            if (r == null) continue;
            r.GetPropertyBlock(_mpb);
            _mpb.SetColor(ColorId, color);
            _mpb.SetColor(BaseColorId, color);
            r.SetPropertyBlock(_mpb);
        }
    }
}