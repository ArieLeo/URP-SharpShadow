using System.Collections;
using System.Collections.Generic;
using MalyaWka.SharpShadow.Renders;
using UnityEngine;

namespace MalyaWka.SharpShadow.Runtime
{
#if UNITY_EDITOR
    [ExecuteAlways] 
#endif
    [DisallowMultipleComponent] 
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    [AddComponentMenu("MalyaWka/SharpShadow/Active")]
    public class SharpShadowActive : MonoBehaviour
    {
        public GameObject meshChild;
        public MeshRenderer meshRenderer;
        public MeshFilter meshFilterSelf;
        public Mesh mesh;
        public bool disableShadows;
        
#if UNITY_EDITOR
        public float boundsFactor = 0.5f;
#endif
        
        protected static Material planarMaterial;

        protected Transform body;
        protected Matrix4x4 matrix;

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

        private void LateUpdate()
        {
            if (meshChild)
            {
                if (SharpShadowRender.activeShadowType == SharpShadowSettings.ActiveShadowType.Planar && !disableShadows)
                {
                    matrix = body.localToWorldMatrix;
                    //matrix = Matrix4x4.TRS(body.position, body.rotation, body.localScale);
                    Graphics.DrawMesh(meshFilterSelf.sharedMesh, matrix, planarMaterial, 1);
                }
            }
        }

        private void Init()
        {
            if (meshChild)
            {
                meshChild.SetActive(SharpShadowRender.activeShadowType == SharpShadowSettings.ActiveShadowType.Active && !disableShadows);
            }
            if (!body)
            {
                body = gameObject.GetComponent<Transform>();    
            }
            if (!planarMaterial)
            {
                planarMaterial = new Material(Shader.Find("Hidden/MalyaWka/SharpShadow/StencilOnPlanar"))
                {
                    enableInstancing = true
                };
            }
        }
    }
}
