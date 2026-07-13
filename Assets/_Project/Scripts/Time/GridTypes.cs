using System;
using UnityEngine;

public enum BuildToolType
{
    Floor,
    Wall,
    Ramp
}

/// <summary>
/// ≈дина€ конвенци€ направлений по гран€м клетки на весь проект: 0=+X, 1=+Z, 2=-X, 3=-Z.
/// –аньше это было "магией", размазанной по GetNearestEdge/GetWallPosition/GetEdgeRotation.
/// “еперь используетс€ и стенами (кака€ грань), и рампами (куда поднимаетс€ верхний край) Ч
/// именно отсутствие направлени€ у рампы было причиной, по которой "рампа после рампы"
/// не могла сама найти следующую клетку.
/// </summary>
public static class GridDir
{
    public const int PlusX = 0;
    public const int PlusZ = 1;
    public const int MinusX = 2;
    public const int MinusZ = 3;

    private static readonly int[] Dx = { 1, 0, -1, 0 };
    private static readonly int[] Dz = { 0, 1, 0, -1 };

    public static int DX(int dir) => Dx[((dir % 4) + 4) % 4];
    public static int DZ(int dir) => Dz[((dir % 4) + 4) % 4];

    /// <summary>Yaw (градусы, любой знак/диапазон) -> ближайшее из 4 направлений сетки.</summary>
    public static int FromYaw(float yawDegrees)
    {
        int snapped = Mathf.RoundToInt(yawDegrees / 90f) % 4;
        if (snapped < 0) snapped += 4;
        // yaw=0   => Vector3.forward => +Z => PlusZ
        // yaw=90  => +X               => PlusX
        // yaw=180 => -Z               => MinusZ
        // yaw=270 => -X               => MinusX
        return snapped switch
        {
            0 => PlusZ,
            1 => PlusX,
            2 => MinusZ,
            3 => MinusX,
            _ => PlusZ
        };
    }

    /// <summary>ќбратное преобразование Ч направление сетки -> поворот по Y. ƒолжно быть
    /// точной инверсией FromYaw, иначе Direction, прочитанный с уже сто€щей детали, не
    /// будет совпадать с тем, что видит игрок.</summary>
    public static Quaternion ToYawRotation(int dir)
    {
        float yaw = dir switch
        {
            PlusZ => 0f,
            PlusX => 90f,
            MinusZ => 180f,
            MinusX => 270f,
            _ => 0f
        };
        return Quaternion.Euler(0f, yaw, 0f);
    }
}

[Serializable]
public struct GridCoord : IEquatable<GridCoord>
{
    public int X;
    public int Y; // этаж
    public int Z;
    public int Edge; // -1 дл€ пола/рампы, 0..3 дл€ стены (см. GridDir)

    public GridCoord(int x, int y, int z, int edge = -1)
    {
        X = x;
        Y = y;
        Z = z;
        Edge = edge;
    }
    public bool Equals(GridCoord other) =>
        X == other.X && Y == other.Y && Z == other.Z && Edge == other.Edge;
    public override bool Equals(object obj) => obj is GridCoord other && Equals(other);
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + X;
            hash = hash * 31 + Y;
            hash = hash * 31 + Z;
            hash = hash * 31 + Edge;
            return hash;
        }
    }
    public override string ToString() => $"({X},{Y},{Z}, edge={Edge})";
}

public static class GridSettings
{
    // ќба значени€ теперь единолично выставл€ет BuildManager.Awake Ч раньше CellSize
    // жил только как приватное поле у PlayerBuilder и не был доступен GridPieceInfo,
    // из-за чего последний не мог вычислить свою собственную клетку по X/Z (только Y).
    public static float CellSize = 2f;
    public static float CellHeight = 2f;
}