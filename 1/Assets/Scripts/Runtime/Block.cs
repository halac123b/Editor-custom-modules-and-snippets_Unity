using System;
using UnityEngine;

namespace LevelEditor
{
    [Serializable]
    public class Block
    {
        public string id;
        public GameObject prefab;
        public Texture icon;
    }
}
