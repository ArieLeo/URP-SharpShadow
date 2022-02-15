using MalyaWka.SharpShadow.Renders;
using UnityEngine;

namespace MalyaWka.SharpShadow.Runtime
{
#if UNITY_EDITOR
    [ExecuteAlways] 
#endif
    [DisallowMultipleComponent] 
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    [AddComponentMenu("MalyaWka/SharpShadow/Static")]
    public class SharpShadowStatic : MonoBehaviour
    {
        public GameObject meshChild;
#if UNITY_EDITOR
        public MeshFilter meshFilter;
        public Mesh mesh;
        public MeshRenderer meshRenderer;

        public float ground = -0.25f;
        public float offset = 0.03f;
        public bool reverse = false;
#endif
        
        private void OnEnable()
        {
            Init();
            SharpShadowRender.renderCreated += Init;
#if UNITY_EDITOR
            if (meshChild)
            {
                meshChild.hideFlags = HideFlags.HideInHierarchy;
            }
#endif
        }

        private void OnDisable()
        {
            SharpShadowRender.renderCreated -= Init;
        }

        private void Init()
        {
            if (meshChild)
            {
                meshChild.SetActive(SharpShadowRender.staticShadowType == SharpShadowSettings.StaticShadowType.Enabled);
            }
        }
    }
}
