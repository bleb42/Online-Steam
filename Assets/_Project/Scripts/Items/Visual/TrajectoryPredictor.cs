using UnityEngine;

public class TrajectoryPredictor : MonoBehaviour
{
    [SerializeField] private GameObject _dotPrefab;
    [SerializeField] private int _steps = 30;
    [SerializeField] private float _stepSize = 0.1f;

    private GameObject[] _dots;

    private void Awake()
    {
        _dots = new GameObject[_steps];
        for (int i = 0; i < _steps; i++)
        {
            _dots[i] = Instantiate(_dotPrefab, transform);
            _dots[i].SetActive(false);
        }
    }

    public void ShowTrajectory(Vector3 startPos, Vector3 startVelocity)
    {
        Vector3 pos = startPos;
        Vector3 vel = startVelocity;

        for (int i = 0; i < _steps; i++)
        {
            _dots[i].SetActive(true);
            _dots[i].transform.position = pos;

            float t = 1f - (float)i / _steps;
            _dots[i].transform.localScale = Vector3.one * t * 0.2f;

            vel += Physics.gravity * _stepSize;
            pos += vel * _stepSize;
        }
    }

    public void HideTrajectory()
    {
        foreach (var dot in _dots)
            dot.SetActive(false);
    }
}