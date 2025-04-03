using LevelEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "Level Editor/Level", order = 0)]
public class Level : ScriptableObject
{
    public BlockPalette blockPalette;
    public string blockData;
    public string colorData;
}
