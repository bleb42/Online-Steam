using UnityEngine;

/// <summary>
/// Висит на каждой заспавненной детали (пол/стена/рампа), и на клиенте, и на сервере.
/// Раньше вычислял только Level (этаж) из transform.position.y — этого хватало для
/// "во что попал луч", но не хватало, чтобы понять, КУДА эта деталь смотрит и в какой
/// именно клетке (X,Z) она стоит. Из-за этого PlayerBuilder был вынужден каждый раз
/// заново, неточно, восстанавливать эти данные из сырой точки попадания луча — отсюда
/// и рампа, которая не могла найти клетку для следующей рампы.
///
/// Всё по-прежнему вычисляется из transform (позиция/поворот и так реплицируются по
/// сети сами), поэтому НИКАКОЙ новой синхронизации не требуется. Единственное, что
/// нужно сделать руками — один раз проставить _toolType в инспекторе на каждом из трёх
/// префабов деталей (Floor -> Floor, Wall -> Wall, Ramp -> Ramp). На Ghost-префабах
/// этот компонент не нужен.
/// </summary>
public class GridPieceInfo : MonoBehaviour
{
    [Tooltip("Тип этой детали. Проставляется один раз на префабе, не на инстансе в сцене.")]
    [SerializeField] private BuildToolType _toolType;
    public BuildToolType ToolType => _toolType;

    public int Level { get; private set; }
    public int CellX { get; private set; }
    public int CellZ { get; private set; }

    /// <summary>
    /// Направление, в котором у рампы поднимается верхний край (0..3, см. GridDir).
    /// Для пола/стены геймплейно не используется.
    /// ВАЖНО: должно быть точной инверсией GetYawSnappedRotation в PlayerBuilder —
    /// если рампа в игре разворачивается не туда, куда целился игрок, поправляйте
    /// сопоставление в GridDir.FromYaw/ToYawRotation, а не тут.
    /// </summary>
    public int Direction { get; private set; }

    private void Awake()
    {
        float cs = GridSettings.CellSize;
        float ch = GridSettings.CellHeight;

        Level = Mathf.RoundToInt(transform.position.y / ch);

        // Pivot пола и рампы стоит в центре клетки по X/Z (см. PlayerBuilder.BuildAt) —
        // поэтому floor(pos/cellSize) даёт ровно cx, а не половину клетки мимо.
        // Для стен (pivot на грани клетки) эти два поля не надёжны — при работе со
        // стеной их не используем, ориентируемся на GetNearestEdge как и раньше.
        CellX = Mathf.FloorToInt(transform.position.x / cs);
        CellZ = Mathf.FloorToInt(transform.position.z / cs);

        Direction = GridDir.FromYaw(transform.eulerAngles.y);
    }
}