using System;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

/// <summary>
/// Держит на себе GridCoord, который ему присвоили при спавне. Нужен только на
/// сервере, чтобы при уничтожении детали (разрушение, деспавн и т.п.) можно было
/// освободить ячейку в словаре BuildManager'а. Клиентам это не нужно, поэтому
/// НЕ NetworkBehaviour — обычный компонент, добавляется сервером.
/// </summary>
public class GridPieceHandle : MonoBehaviour
{
    public GridCoord Coord;
    public event Action OnRemoved;
    private void OnDestroy() => OnRemoved?.Invoke();
}

public class BuildManager : NetworkBehaviour
{
    public static BuildManager Instance { get; private set; }

    [SerializeField] private BuildPieceDefinition[] _pieces;

    // Раньше только _cellHeight публиковался в GridSettings — _cellSize жил отдельным
    // приватным полем в PlayerBuilder и нигде больше. GridPieceInfo теперь тоже считает
    // свою клетку по X/Z, поэтому CellSize должен быть доступен из одного авторитетного
    // места, а не дублироваться по инспекторам разных компонентов.
    [SerializeField] private float _cellSize = 2f;
    [SerializeField] private float _cellHeight = 2f;

    private readonly SyncDictionary<GridCoord, int> _occupied = new SyncDictionary<GridCoord, int>();

    private void Awake()
    {
        Instance = this;
        GridSettings.CellSize = _cellSize;
        GridSettings.CellHeight = _cellHeight;
    }

    public bool IsOccupied(GridCoord coord) => _occupied.ContainsKey(coord);

    public bool HasNeighborSupport(GridCoord coord)
    {
        GridCoord here = new GridCoord(coord.X, coord.Y, coord.Z, -1);
        GridCoord below = new GridCoord(coord.X, coord.Y - 1, coord.Z, -1);
        GridCoord above = new GridCoord(coord.X, coord.Y + 1, coord.Z, -1);
        GridCoord north = new GridCoord(coord.X, coord.Y, coord.Z + 1, -1);
        GridCoord south = new GridCoord(coord.X, coord.Y, coord.Z - 1, -1);
        GridCoord east = new GridCoord(coord.X + 1, coord.Y, coord.Z, -1);
        GridCoord west = new GridCoord(coord.X - 1, coord.Y, coord.Z, -1);
        return _occupied.ContainsKey(here) || _occupied.ContainsKey(below) || _occupied.ContainsKey(above)
            || _occupied.ContainsKey(north) || _occupied.ContainsKey(south)
            || _occupied.ContainsKey(east) || _occupied.ContainsKey(west);
    }

    public void RequestPlace(int pieceIndex, GridCoord coord, Vector3 position, Quaternion rotation, bool requiresSupport)
    {
        RequestPlaceServerRpc(pieceIndex, coord, position, rotation, requiresSupport);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPlaceServerRpc(int pieceIndex, GridCoord coord, Vector3 position, Quaternion rotation, bool requiresSupport, NetworkConnection sender = null)
    {
        if (pieceIndex < 0 || pieceIndex >= _pieces.Length) return;
        if (_occupied.ContainsKey(coord)) return;
        if (requiresSupport && !HasNeighborSupport(coord)) return;

        var def = _pieces[pieceIndex];
        if (def.Prefab == null) return;

        GameObject instance = Instantiate(def.Prefab, position, rotation);
        GridPieceHandle handle = instance.GetComponent<GridPieceHandle>();
        if (handle == null) handle = instance.AddComponent<GridPieceHandle>();
        handle.Coord = coord;
        handle.OnRemoved += () => _occupied.Remove(coord);
        _occupied.Add(coord, pieceIndex);

        InstanceFinder.ServerManager.Spawn(instance, sender);
    }
}