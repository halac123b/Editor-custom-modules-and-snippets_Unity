#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace LevelEditor
{
    // Window Editor này sẽ override lại window Inspector khi click vào ScriptableObject Level
    [CustomEditor(typeof(Level))]
    public class LevelEditor : Editor
    {
        private SerializedProperty blockPalette;
        private SerializedProperty blockData;
        private SerializedProperty colorData;
        private int selectedItem = 0;
        private int selectedColor = 0;
        private GUIStyle _iconButtonStyle;
        const int NUM_ROW = 9;
        const int NUM_COL = 9;

        // Đếm số lượng board mỗi loại đã đc sắp trên board
        private Dictionary<string, int> _countBlock = new();

        // Editor class cũng có hàm OnEnable()
        /// Chạy mỗi khi component đc chọn và hiện Inspector này
        //// Khi exit Play mode
        private void OnEnable()
        {
            // Ref đến các field của class Leveld
            blockPalette = serializedObject.FindProperty("blockPalette");
            blockData = serializedObject.FindProperty("blockData");
            colorData = serializedObject.FindProperty("colorData");
        }

        // Hàm này cũng đc gọi khi chọn component đó
        /// Khi user interact và thay đổi giá trị trong Inspector
        /// Khi thay đổi kích thước window, vào hoặc thoát Play mode,..
        public override void OnInspectorGUI()
        {
            // Update lại data mới nhất từ component
            serializedObject.Update();
            CountNumberBlock();
            // Tạo 1 field trên Window (EditorGUILayout)
            /// ObjectField: field để kéo thả object vào
            /// Input: tên field, data đc kéo vào link đến field nào trong class, type đc yêu cầu, có cho phép kéo thả obj từ scene vào k
            blockPalette.objectReferenceValue = (BlockPalette)EditorGUILayout.ObjectField("Block Palette",
                blockPalette.objectReferenceValue, typeof(BlockPalette), false);
            // Gán 1 biến cho dễ gọi
            BlockPalette newPalette = (BlockPalette)blockPalette.objectReferenceValue;
            if (newPalette == null)
            {
                // Viết 1 đoạn text trên window
                EditorGUILayout.LabelField("Please assign a palette");
                return;
            }

            // Tạo 1 khoảng trống trên window 
            EditorGUILayout.Space(20);

            // Tạo 1 custom CSS-like style cho button k có padding
            _iconButtonStyle = new GUIStyle(GUI.skin.button)
            {
                padding = new RectOffset(0, 0, 0, 0)
            };

            // Start 1 Horizontal layout
            EditorGUILayout.BeginHorizontal();
            int currentType = 1;
            for (int i = 0; i < newPalette.blocks.Count; i++)
            {
                Debug.Log($"Block id {newPalette.blocks[i].id}");
                string[] parts = newPalette.blocks[i].id.Split('_');
                if (int.Parse(parts[0]) > currentType)
                {
                    currentType++;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }
                // Tạo 1 button, gán icon, set Style, kích thước
                // if (button) có nghĩa là trong lần interact này, nút đó đã đc nhấn
                if (GUILayout.Button(newPalette.blocks[i].icon, _iconButtonStyle, GUILayout.Width(35), GUILayout.Height(35)))
                {
                    selectedItem = currentType;
                    selectedColor = int.Parse(parts[1]);
                }

                if (currentType > 1)
                {
                    Debug.Log($"color {currentType}_{parts[1]}");
                    int count = _countBlock.GetValueOrDefault($"{currentType}_{parts[1]}", 0);
                    EditorGUILayout.LabelField($"{count}", GUILayout.Width(30));
                }
            }

            // Tạo 1 button để xoá, dùng icon có sẵn của Unity
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_Grid.EraserTool@2x"), GUILayout.Width(35), GUILayout.Height(35)))
            {
                selectedItem = 0;
                selectedColor = 0;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(20);

            try
            {
                int[,] blockArray = JsonExtension.GetIntArray(blockData.stringValue);
                int[,] colorArray = JsonExtension.GetIntArray(colorData.stringValue);

                GenerateGrid(blockArray, colorArray);

                if (GUILayout.Button("Save Level", GUILayout.Width(100), GUILayout.Height(35)))
                {
                    // Mark obj đó dirty (tức đã bị thay đổi và chưa đc lưu)
                    EditorUtility.SetDirty(this);
                }

                GenerateTargetList();

                // Save leveldata vừa lưu
                blockData.stringValue = JsonExtension.ArrayToFormattedString(blockArray);
                colorData.stringValue = JsonExtension.ArrayToFormattedString(colorArray);
            }
            catch (Exception ex)
            {
                Debug.Log($"Exception {ex.Message}");
            }

            EditorGUILayout.LabelField("Board Matrix");
            blockData.stringValue = EditorGUILayout.TextArea(blockData.stringValue, GUILayout.Height(140));
            EditorGUILayout.LabelField("Color Matrix");
            colorData.stringValue = EditorGUILayout.TextArea(colorData.stringValue, GUILayout.Height(140));

            // Apply các thay đổi vừa thực hiện vào serializedObject
            serializedObject.ApplyModifiedProperties();
        }

        private void GenerateGrid(int[,] blockData, int[,] colorData)
        {
            // Nhấn clear data
            if (GUILayout.Button("Clear"))
            {
                JsonExtension.ClearArray(blockData);
                JsonExtension.ClearArray(colorData);
            }

            EditorGUILayout.Space(10);
            Color backgroundColor = GUI.backgroundColor;
            BlockPalette palette = blockPalette.objectReferenceValue as BlockPalette;

            for (int x = 0; x < NUM_COL; x++)
            {
                EditorGUILayout.BeginHorizontal();

                for (int y = 0; y < NUM_ROW; y++)
                {
                    // Lấy data tại địa chỉ đó
                    int blockNumber = blockData[x, y];

                    Texture icon = default;
                    // Nếu data có block, fill ô đó bằng icon của block
                    if (blockNumber > 0)
                    {
                        int color = colorData[x, y];
                        icon = palette.blocks.FirstOrDefault(fd => fd.id == $"{blockNumber}_{color}")?.icon;
                    }

                    bool button = GUILayout.Button(icon, _iconButtonStyle, GUILayout.Width(35), GUILayout.Height(35));
                    // Nếu block đc fill, update lại data của block đó
                    if (button)
                    {
                        blockData[x, y] = selectedItem < 0 ? 0 : selectedItem;
                        colorData[x, y] = selectedItem < 0 ? 0 : selectedColor;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }
            GUI.backgroundColor = backgroundColor;
        }

        private void CountNumberBlock()
        {
            _countBlock.Clear();
            try
            {
                int[,] blockArray = JsonExtension.GetIntArray(blockData.stringValue);
                int[,] colorArray = JsonExtension.GetIntArray(colorData.stringValue);

                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        int value = blockArray[i, j];

                        if (value >= 2)
                        {
                            string key = $"{value}_{colorArray[i, j]}";

                            // If the key exists, increment its value; otherwise, initialize it to 1
                            if (_countBlock.ContainsKey(key))
                            {
                                _countBlock[key] += 1;
                            }
                            else
                            {
                                _countBlock[key] = 1;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"Exception {ex.Message}");
            }
        }

        private void GenerateTargetList()
        {
            EditorGUILayout.LabelField("Target List");

            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < 5; x++)
            {
                bool button = GUILayout.Button(image: null, _iconButtonStyle, GUILayout.Width(35), GUILayout.Height(35));
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif