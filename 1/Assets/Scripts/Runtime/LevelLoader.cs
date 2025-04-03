using System.Linq;
using UnityEngine;

namespace LevelEditor
{
    public static class LevelLoader
    {
        public static void LoadLevel(Level level)
        {
            int[,] blockArray = JsonExtension.GetIntArray(level.blockData);
            int[,] colorArray = JsonExtension.GetIntArray(level.colorData);

            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    int blockNumber = blockArray[x, y];

                    if (blockNumber > 0)
                    {
                        int color = colorArray[x, y];
                        Block block = level.blockPalette.blocks.FirstOrDefault(fd => fd.id == $"{blockNumber}_{color}");

                        Vector3 position = new(-1.76f, 1.77f, 0);
                        Vector3 offset = new(0.44f * y, 0.44f * x);

                        InstantiateItem(block.prefab, position, offset);
                    }
                }
            }
        }

        private static void InstantiateItem(GameObject prefab, Vector3 position, Vector3 offset)
        {
            GameObject instance = Object.Instantiate(prefab);
            float x = position.x + offset.x;
            float y = position.y - offset.y;
            float z = offset.z;
            instance.transform.position = new Vector3(x, y, z);
            instance.transform.localScale *= 0.77f;
        }
    }
}


