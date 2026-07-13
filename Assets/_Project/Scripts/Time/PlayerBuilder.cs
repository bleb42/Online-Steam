using UnityEngine;

public class PlayerBuilder : MonoBehaviour
{
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private Camera _camera;
    [SerializeField] private BuildPieceDefinition[] _pieces; // 0=Floor, 1=Wall, 2=Ramp

    [Header("Aim")]
    [Tooltip("Всё, во что можно целиться: террейн, статика карты, уже построенные детали. " +
             "Обычно достаточно ОДНОГО слоя (или вообще Everything). " +
             "Ghost-превью должен быть на Ignore Raycast, иначе будет целиться сам в себя.")]
    [SerializeField] private LayerMask _aimMask;

    [Header("Grid")]
    [SerializeField] private float _cellSize = 2f;
    [SerializeField] private float _cellHeight = 2f;
    [SerializeField] private float _maxBuildDistance = 8f;
    [SerializeField] private float _maxReachFromPlayer = 6f; // защита от луча, улетевшего в щель на дальний фон

    public bool IsBuildModeActive { get; private set; }

    private int _selectedPieceIndex;
    private GameObject _ghostInstance;
    private BuildGhost _ghostVisual;

    private bool _placementValid;
    private bool _placementRequiresSupport;
    private GridCoord _placementCoord;
    private Vector3 _placementPosition;
    private Quaternion _placementRotation;

    // ==== DEBUG (Scene view gizmos) ====
    private Vector3 _dbgAimOrigin;
    private Vector3 _dbgAimHit;
    private bool _dbgHasAimHit;

    private void OnEnable()
    {
        _inputReader.OnBuildTogglePerformed += ToggleBuildMode;
        _inputReader.OnBuildSlot1Performed += () => SelectPiece(0);
        _inputReader.OnBuildSlot2Performed += () => SelectPiece(1);
        _inputReader.OnBuildSlot3Performed += () => SelectPiece(2);
        _inputReader.OnThrowStarted += HandleClickStarted;
    }

    private void OnDisable()
    {
        if (_inputReader == null) return;
        _inputReader.OnBuildTogglePerformed -= ToggleBuildMode;
        _inputReader.OnThrowStarted -= HandleClickStarted;
    }

    private void Update()
    {
        if (!IsBuildModeActive) return;
        UpdatePreview();
    }

    private void HandleClickStarted()
    {
        if (!IsBuildModeActive) return;
        TryPlace();
    }

    private void ToggleBuildMode()
    {
        IsBuildModeActive = !IsBuildModeActive;
        if (IsBuildModeActive) SpawnGhost();
        else DestroyGhost();
    }

    private void SelectPiece(int index)
    {
        if (!IsBuildModeActive) return;
        if (index < 0 || index >= _pieces.Length) return;
        _selectedPieceIndex = index;
        DestroyGhost();
        SpawnGhost();
    }

    private void SpawnGhost()
    {
        var def = _pieces[_selectedPieceIndex];
        if (def.GhostPrefab == null)
        {
            Debug.LogError($"[PlayerBuilder] GhostPrefab не назначен для {def.PieceName}");
            return;
        }
        _ghostInstance = Instantiate(def.GhostPrefab);
        _ghostVisual = _ghostInstance.GetComponent<BuildGhost>();
    }

    private void DestroyGhost()
    {
        if (_ghostInstance != null) Destroy(_ghostInstance);
        _ghostInstance = null;
        _ghostVisual = null;
    }

    private void UpdatePreview()
    {
        if (_ghostInstance == null) return;

        if (!TryGetPlacement(out GridCoord coord, out Vector3 pos, out Quaternion rot, out bool requiresSupport))
        {
            _dbgHasAimHit = false;
            _ghostInstance.SetActive(false);
            _placementValid = false;
            return;
        }

        _ghostInstance.SetActive(true);
        _ghostInstance.transform.SetPositionAndRotation(pos, rot);

        _placementCoord = coord;
        _placementPosition = pos;
        _placementRotation = rot;
        _placementRequiresSupport = requiresSupport;

        // Клетка уже занята — тут показывать ghost вообще не нужно, а не просто красить
        // его красным. Строить всё равно нельзя, а красный полупрозрачный силуэт поверх
        // уже стоящей детали только мешает смотреть.
        if (BuildManager.Instance.IsOccupied(coord))
        {
            _ghostInstance.SetActive(false);
            _placementValid = false;
            return;
        }

        // Строим в воздух (мимо реальной геометрии) — обязательно нужна опора рядом,
        // иначе получаются оторванные платформы, как на скриншотах. Тут ghost всё же
        // показываем красным — в отличие от "занято", это подсказка "подойди ближе
        // к своей же постройке", а не "тут в принципе нельзя".
        bool valid = !requiresSupport || BuildManager.Instance.HasNeighborSupport(coord);

        _placementValid = valid;
        _ghostVisual?.SetValid(_placementValid);
    }

    /// <summary>
    /// Определяет клетку/позицию/поворот для текущей наводки.
    ///
    /// Раньше и попадание лучом в существующую деталь, и промах "в воздух" считали
    /// горизонтальную ячейку почти напрямую из сырой мировой точки попадания — из-за
    /// этого рампа не находила клетку для следующей рампы, а деталь, поставленная на
    /// пол, улетала на этаж выше вместо того, чтобы начинаться от него. Теперь, если
    /// луч попал в уже стоящую деталь, мы используем ЕЁ СОБСТВЕННЫЕ данные (клетку и
    /// направление из GridPieceInfo), а не пересчитываем их заново из точки на её
    /// поверхности — это и есть главное отличие.
    /// </summary>
    private bool TryGetPlacement(out GridCoord coord, out Vector3 worldPos, out Quaternion rot, out bool requiresSupport)
    {
        coord = default;
        worldPos = default;
        rot = Quaternion.identity;
        requiresSupport = false;

        var def = _pieces[_selectedPieceIndex];
        Ray aimRay = new Ray(_camera.transform.position, _camera.transform.forward);
        _dbgAimOrigin = aimRay.origin;

        Vector3 probe;
        int? forcedLevel = null;
        int? forcedCx = null;
        int? forcedCz = null;
        Quaternion? forcedRampRotation = null;

        if (Physics.Raycast(aimRay, out RaycastHit hit, _maxBuildDistance, _aimMask))
        {
            if (Vector3.Distance(transform.position, hit.point) > _maxReachFromPlayer)
                return false;

            _dbgHasAimHit = true;
            _dbgAimHit = hit.point;

            // Целимся в реальную поверхность (террейн/уже построенную деталь) —
            // опора и так физически есть, доп. проверка не нужна.
            requiresSupport = false;

            // Небольшой сдвиг вдоль нормали — чтобы попасть в клетку ПЕРЕД поверхностью,
            // а не внутрь той детали/террейна, в который целимся. Используется только
            // как запасной вариант ниже, когда попали не в деталь, а в голый террейн.
            probe = hit.point + hit.normal * 0.05f;

            GridPieceInfo hitPiece = hit.collider.GetComponentInParent<GridPieceInfo>();
            if (hitPiece != null)
            {
                bool aimingTop = Vector3.Dot(hit.normal, Vector3.up) > 0.5f;

                if (aimingTop && hitPiece.ToolType == BuildToolType.Ramp)
                {
                    // Верхний край рампы физически ведёт в СОСЕДНЮЮ клетку этажом выше —
                    // именно туда встаёт следующая деталь (рампа-продолжение, площадка-
                    // приземление или стена), а не в клетку прямо над той рампой, куда
                    // попал луч. Берём направление подъёма из данных самой рампы, а не
                    // из сырой точки на наклонной поверхности — так рампа за рампой
                    // всегда идёт туда, куда нужно, независимо от того, в какое именно
                    // место наклонной грани вы целитесь.
                    forcedCx = hitPiece.CellX + GridDir.DX(hitPiece.Direction);
                    forcedCz = hitPiece.CellZ + GridDir.DZ(hitPiece.Direction);
                    forcedLevel = hitPiece.Level + 1;
                    forcedRampRotation = GridDir.ToYawRotation(hitPiece.Direction);
                }
                else if (aimingTop && hitPiece.ToolType == BuildToolType.Floor)
                {
                    if (def.ToolType == BuildToolType.Floor)
                    {
                        // Новый пол — второй этаж прямо над тем, на который смотрим.
                        forcedLevel = hitPiece.Level + 1;
                    }
                    else if (def.ToolType == BuildToolType.Wall)
                    {
                        // Стена растёт ВВЕРХ от уровня этого пола, а не с этажа выше —
                        // раньше тут всегда прибавлялся +1, из-за чего стена (и рампа
                        // ниже) повисала в воздухе на клетку выше пола.
                        forcedLevel = hitPiece.Level;
                    }
                    else // Ramp
                    {
                        // Рампа не может встать в ту же клетку, что и пол под ней (та
                        // клетка уже занята полом) — она должна начинаться с той же
                        // высоты, но в соседней клетке, в ту сторону, к какому краю
                        // тайла вы подошли и целитесь. Это и есть "не могу поставить
                        // рампу на пол" — раньше она всегда пыталась встать этажом выше
                        // пола и либо конфликтовала, либо повисала в воздухе.
                        Vector3 floorCellOrigin = new Vector3(
                            hitPiece.CellX * _cellSize, hitPiece.Level * _cellHeight, hitPiece.CellZ * _cellSize);
                        int stepEdge = GetNearestEdge(hit.point, floorCellOrigin);
                        forcedCx = hitPiece.CellX + GridDir.DX(stepEdge);
                        forcedCz = hitPiece.CellZ + GridDir.DZ(stepEdge);
                        forcedLevel = hitPiece.Level;
                    }
                }
                else
                {
                    // Смотрим не на верхнюю грань (низ рампы, бок стены и т.п.) —
                    // берём этаж детали как есть, горизонталь — из сырой точки попадания.
                    forcedLevel = hitPiece.Level;
                }
            }
            // hitPiece == null: попали в голый террейн/статику карты — считаем всё
            // по сырой точке попадания, как и раньше.
        }
        else
        {
            // Луч улетел мимо всей реальной геометрии — строим в воздух. Раньше здесь
            // всегда брался только этаж под ногами игрока, поэтому ни "пол над собой"
            // (плоскость оказывалась позади луча, смотрящего вверх), ни "рампа после
            // рампы, если чуть промазать мимо её же меша" не работали. Вместо одной
            // фиксированной высоты ищем ближайшую по лучу горизонтальную "полку" среди
            // нескольких уровней вокруг того, на котором стоит игрок: смотришь вверх —
            // естественно найдётся уровень выше, смотришь вперёд/вниз — текущий или ниже.
            if (!TryFindAerialPlane(aimRay, out Vector3 planePoint, out int aerialLevel))
                return false;

            if (Vector3.Distance(transform.position, planePoint) > _maxReachFromPlayer)
                return false;

            _dbgHasAimHit = true;
            _dbgAimHit = planePoint;
            requiresSupport = true;

            probe = planePoint + Vector3.up * 0.05f; // попасть в клетку НАД плоскостью
            forcedLevel = aerialLevel;
        }

        int cx = forcedCx ?? Mathf.FloorToInt(probe.x / _cellSize);
        int level = forcedLevel ?? Mathf.FloorToInt(probe.y / _cellHeight);
        int cz = forcedCz ?? Mathf.FloorToInt(probe.z / _cellSize);

        Vector3 cellOrigin = new Vector3(cx * _cellSize, level * _cellHeight, cz * _cellSize);
        Vector3 cellCenter = cellOrigin + new Vector3(_cellSize, _cellHeight, _cellSize) * 0.5f;

        switch (def.ToolType)
        {
            case BuildToolType.Floor:
                coord = new GridCoord(cx, level, cz, -1);
                worldPos = new Vector3(cellCenter.x, cellOrigin.y, cellCenter.z);
                rot = Quaternion.identity;
                break;

            case BuildToolType.Ramp:
                coord = new GridCoord(cx, level, cz, -1);
                worldPos = new Vector3(cellCenter.x, cellOrigin.y, cellCenter.z);
                // Если мы продолжаем существующую рампу — держим ровно тот же наклон,
                // а не пересчитываем его заново из текущего (более шумного, потому что
                // камера гуляет во время ходьбы по рампе) поворота камеры.
                rot = forcedRampRotation ?? GetYawSnappedRotation();
                break;

            case BuildToolType.Wall:
                int edge = GetNearestEdge(probe, cellOrigin);
                // Позиция/поворот считаются от "сырой" грани — это просто геометрия,
                // она одинакова с любой стороны. А вот в словарь занятости кладём
                // КАНОНИЧЕСКУЮ координату, иначе одна и та же стена, поставленная
                // "с другой стороны", получит другой ключ и не будет считаться занятой.
                coord = CanonicalizeWallCoord(cx, level, cz, edge);
                worldPos = GetWallPosition(cellCenter, edge);
                rot = GetEdgeRotation(edge);
                break;
        }

        return true;
    }

    /// <summary>
    /// Луч не попал ни во что физическое — ищем, на каком из нескольких горизонтальных
    /// "этажей" вокруг текущего уровня игрока луч вообще может что-то пересечь, и берём
    /// ближайшее по лучу пересечение (наименьшее t). Диапазон -1..+4 от этажа игрока с
    /// запасом: -1 позволяет застраивать обрыв под ногами, +4 — не упереться в потолок,
    /// если луч перелетел через несколько рамп подряд.
    /// </summary>
    private bool TryFindAerialPlane(Ray aimRay, out Vector3 point, out int level)
    {
        point = default;
        level = 0;

        if (Mathf.Abs(aimRay.direction.y) < 0.0001f)
            return false; // взгляд почти строго горизонтален — ни одну горизонтальную плоскость не пересечь

        int playerLevel = Mathf.FloorToInt(transform.position.y / _cellHeight);

        bool found = false;
        float bestT = float.MaxValue;
        int bestLevel = 0;
        Vector3 bestPoint = default;

        for (int l = playerLevel - 1; l <= playerLevel + 4; l++)
        {
            float planeY = l * _cellHeight;
            float t = (planeY - aimRay.origin.y) / aimRay.direction.y;
            if (t <= 0f || t > _maxBuildDistance) continue;

            if (t < bestT)
            {
                bestT = t;
                bestLevel = l;
                bestPoint = aimRay.origin + aimRay.direction * t;
                found = true;
            }
        }

        if (!found) return false;
        point = bestPoint;
        level = bestLevel;
        return true;
    }

    /// <summary>
    /// Какая из 4 граней клетки ближе к точке прицеливания. Это определяет,
    /// куда встанет стена — по месту наведения, а не по повороту камеры.
    /// 0=+X, 1=+Z, 2=-X, 3=-Z.
    /// </summary>
    private int GetNearestEdge(Vector3 worldPoint, Vector3 cellOrigin)
    {
        float lx = (worldPoint.x - cellOrigin.x) / _cellSize; // 0..1 внутри клетки
        float lz = (worldPoint.z - cellOrigin.z) / _cellSize;

        float distPosX = 1f - lx;
        float distNegX = lx;
        float distPosZ = 1f - lz;
        float distNegZ = lz;

        float min = Mathf.Min(Mathf.Min(distPosX, distNegX), Mathf.Min(distPosZ, distNegZ));

        if (min == distPosX) return 0;
        if (min == distPosZ) return 1;
        if (min == distNegX) return 2;
        return 3;
    }

    /// <summary>
    /// Грань "-X" клетки (cx,cz) физически — это ТА ЖЕ САМАЯ стена, что и грань "+X"
    /// клетки (cx-1,cz). Без этой нормализации словарь занятости хранил бы одну стену
    /// под двумя разными ключами в зависимости от того, с какой стороны на неё смотрели
    /// при постройке — из-за этого можно было поставить "второй" экземпляр в то же место.
    /// </summary>
    private GridCoord CanonicalizeWallCoord(int cx, int level, int cz, int edge)
    {
        switch (edge)
        {
            case 2: return new GridCoord(cx - 1, level, cz, 0); // -X (cx,cz) == +X (cx-1,cz)
            case 3: return new GridCoord(cx, level, cz - 1, 1); // -Z (cx,cz) == +Z (cx,cz-1)
            default: return new GridCoord(cx, level, cz, edge); // 0 и 1 уже канонические
        }
    }

    private Vector3 GetWallPosition(Vector3 cellCenter, int edge)
    {
        float half = _cellSize * 0.5f;
        Vector3 pos = cellCenter;
        switch (edge)
        {
            case 0: pos.x += half; break; // +X
            case 1: pos.z += half; break; // +Z
            case 2: pos.x -= half; break; // -X
            case 3: pos.z -= half; break; // -Z
        }
        return pos;
    }

    private Quaternion GetEdgeRotation(int edge)
    {
        // Стены на +X/-X и +Z/-Z смотрят перпендикулярно разным осям.
        switch (edge)
        {
            case 0: return Quaternion.Euler(0f, 90f, 0f); // +X
            case 1: return Quaternion.identity;           // +Z
            case 2: return Quaternion.Euler(0f, 90f, 0f); // -X
            case 3: return Quaternion.identity;           // -Z
        }
        return Quaternion.identity;
    }

    private Quaternion GetYawSnappedRotation()
    {
        float yaw = _camera.transform.eulerAngles.y;
        float snapped = Mathf.Round(yaw / 90f) * 90f;
        return Quaternion.Euler(0f, snapped, 0f);
    }

    private void TryPlace()
    {
        if (!_placementValid) return;
        BuildManager.Instance.RequestPlace(_selectedPieceIndex, _placementCoord, _placementPosition, _placementRotation, _placementRequiresSupport);
    }

    // ==== DEBUG ====
    private void OnDrawGizmos()
    {
        if (!IsBuildModeActive) return;

        if (_dbgHasAimHit)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(_dbgAimOrigin, _dbgAimHit);
            Gizmos.DrawSphere(_dbgAimHit, 0.06f);
        }

        Gizmos.color = _placementValid ? Color.green : Color.red;
        Gizmos.DrawWireCube(_placementPosition, Vector3.one * _cellSize * 0.7f);
    }
}