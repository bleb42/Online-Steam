using UnityEngine;

[CreateAssetMenu(menuName = "Building/Piece Definition", fileName = "NewBuildPiece")]
public class BuildPieceDefinition : ScriptableObject
{
    public string PieceName;
    public GameObject Prefab; 
    public GameObject GhostPrefab;
    public BuildToolType ToolType;
}