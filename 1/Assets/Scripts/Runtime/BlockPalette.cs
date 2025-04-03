using System.Collections.Generic;
using UnityEngine;

namespace LevelEditor
{
    [CreateAssetMenu(fileName = "BlockPalette", menuName = "Level Editor/BlockPalette", order = 1)]
    public class BlockPalette : ScriptableObject
    {
        public List<Block> blocks;
    }
}