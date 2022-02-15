using System;
using System.Collections.Generic;
using UnityEngine;

namespace MalyaWka.SharpShadow.Editor
{
    [Serializable]
    public class SharpShadowAsset : ScriptableObject
    {
        public class MeshAssetActive
        {
            public string origin;
            public Mesh mesh;
            public int useCount;
        }
        
        [Serializable]
        public class MeshAssetStatic
        {
            public string origin;
            public Mesh mesh;
            public int useCount;
            public float positionY;
            public float rotationY;
            public bool reverse;
        }
        
        public List<MeshAssetActive> activeMeshAssets = new List<MeshAssetActive>();
        public List<MeshAssetStatic> staticMeshAssets = new List<MeshAssetStatic>();
    }
}
