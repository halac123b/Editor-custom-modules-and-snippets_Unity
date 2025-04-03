using System;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LevelEditor
{
    public static class JsonExtension
    {
        [Serializable]
        private class ItemList<T>
        {
            public T[] Items;
        }

        public static T[] GetFromJson<T>(string json)
        {
            return JsonUtility.FromJson<ItemList<T>>(json).Items;
        }

        public static string SetToJson<T>(T[] array)
        {
            return JsonUtility.ToJson(new ItemList<T> { Items = array });
        }

        public static int[,] GetIntArray(string matrixString)
        {
            // Split the input string into lines
            string[] rows = matrixString.Split('\n');

            // Initialize the 2D array
            int[,] mapArray = new int[9, 9];

            if (!string.IsNullOrEmpty(matrixString))
            {
                // Loop through each row and split by comma to get the integers
                for (int i = 0; i < 9; i++)
                {
                    int[] rowValues = rows[i].Split(',').Select(int.Parse).ToArray();
                    for (int j = 0; j < 9; j++)
                    {
                        mapArray[i, j] = rowValues[j];
                    }
                }
            }
            else
            {
                Debug.Log($"Null bruh");
            }

            return mapArray;
        }

        public static string ArrayToFormattedString(int[,] arr)
        {
            StringBuilder sb = new();

            for (int i = 0; i < arr.GetLength(0); i++)
            {
                for (int j = 0; j < arr.GetLength(1); j++)
                {
                    sb.Append(arr[i, j]);

                    if (j < arr.GetLength(1) - 1)
                        sb.Append(","); // Comma between elements in a row
                }

                if (i < arr.GetLength(0) - 1)
                    sb.AppendLine(); // Newline after each row except the last one
            }

            return sb.ToString();
        }

        public static void ClearArray(int[,] sourceArray)
        {
            int[,] targetArray = new int[9, 9]; // Initialize a 9x9 array with all elements set to 0

            // Copy values from sourceArray to targetArray within bounds
            for (int i = 0; i < Math.Min(targetArray.GetLength(0), 9); i++)
            {
                for (int j = 0; j < Math.Min(targetArray.GetLength(1), 9); j++)
                {
                    sourceArray[i, j] = targetArray[i, j];
                }
            }
        }
    }

}

