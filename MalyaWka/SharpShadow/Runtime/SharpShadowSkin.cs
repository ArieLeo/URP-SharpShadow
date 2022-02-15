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
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    [AddComponentMenu("MalyaWka/SharpShadow/Skin")]
    public class SharpShadowSkin : MonoBehaviour
    {
        public GameObject meshChild;
        public SkinnedMeshRenderer skinnedMeshRenderer;
        public SkinnedMeshRenderer skinnedMeshRendererSelf;
        public Mesh mesh;
        public bool disableShadows;
        
#if UNITY_EDITOR
        public float boundsFactor = 0.5f;
#endif
        protected static Material planarMaterial;

        protected Transform body;
        protected Matrix4x4 matrix;
        
        protected Mesh skinnedMesh = null;
        protected Mesh skinnedMeshSelf = null;

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
                    //matrix = body.localToWorldMatrix;
                    matrix = Matrix4x4.TRS(body.position, body.rotation, body.localScale);
                    skinnedMeshRendererSelf.BakeMesh(skinnedMeshSelf);
                    Graphics.DrawMesh(skinnedMeshSelf, matrix, planarMaterial, 1);
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
            if (!skinnedMesh)
            {
                skinnedMesh = new Mesh();
            }
            if (!skinnedMeshSelf)
            {
                skinnedMeshSelf = new Mesh();
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
