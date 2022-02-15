using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using MalyaWka.SharpShadow.Utils;
using UnityEngine.Events;

namespace MalyaWka.SharpShadow.Renders
{
    [Serializable]
    public class SharpShadowSettings
    {
        [Serializable] public enum StaticShadowType { Disabled = 0, Enabled = 1}
        [Serializable] public enum ActiveShadowType { Disabled = 0, Planar = 1, Active = 2 }

        [SerializeField] internal RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        [SerializeField] internal SingleLayer layer = 1;
            
        [SerializeField] internal StaticShadowType staticShadowType = StaticShadowType.Enabled;
        [SerializeField] internal ActiveShadowType activeShadowType = ActiveShadowType.Active;
        [SerializeField, Delayed] internal float shadowNearExtrude = 0.02f;
        [SerializeField, Delayed] internal float shadowFarExtrude = -0.02f;
        [SerializeField, Delayed] internal float shadowFloorHeight = 0.0035f;
        [SerializeField, Delayed] internal float intensity = 0.5f;
    }
    
    public class SharpShadowRender : ScriptableRendererFeature
    {
        public static SharpShadowSettings.StaticShadowType staticShadowType = SharpShadowSettings.StaticShadowType.Disabled;
        public static SharpShadowSettings.ActiveShadowType activeShadowType = SharpShadowSettings.ActiveShadowType.Disabled;
        public static Action renderCreated = null;
        
        public SharpShadowSettings Settings => m_Settings;
        
        [SerializeField, HideInInspector] private Shader m_VisualShader = null;
        [SerializeField] private SharpShadowSettings m_Settings = new SharpShadowSettings();
        
        private Material m_Material;
        private ScreenRenderPass m_SVRPass = null;          //Visualize Shadows
        private SharpShadowRenderPass m_SSRPass = null;      //Static Shadows
        private SharpShadowRenderPass m_SARPass = null;      //Planar or Active Shadows

        private const string k_ShaderVisualName = "Hidden/MalyaWka/SharpShadow/VisualizeShadows";
        
        public override void Create()
        {
            staticShadowType = m_Settings.staticShadowType;
            activeShadowType = m_Settings.activeShadowType;

            m_SVRPass = new ScreenRenderPass(m_Settings.renderPassEvent + 10, "Visualize Shadows");

            GetMaterial();

            if (m_Settings.staticShadowType != SharpShadowSettings.StaticShadowType.Disabled)
            {
                m_SSRPass = new SharpShadowRenderPass(
                    m_Settings.renderPassEvent + 5, 
                    new[] { "StencilOnStatic" }, 
                    "Static Shadows");
            }

            if (m_Settings.activeShadowType != SharpShadowSettings.ActiveShadowType.Disabled)
            {
                m_SARPass = new SharpShadowRenderPass(
                    m_Settings.renderPassEvent + 6, 
                    new[] { "StencilOnPlanar", "StencilOnActive" },
                    "Planar & Active Shadows");
            }
        }
        
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if ((m_Settings.staticShadowType != SharpShadowSettings.StaticShadowType.Disabled ||
                 m_Settings.activeShadowType != SharpShadowSettings.ActiveShadowType.Disabled) &&
                renderingData.lightData.mainLightIndex != -1)
            {
                
                if (m_Settings.staticShadowType == SharpShadowSettings.StaticShadowType.Enabled)
                {
                    m_SSRPass.layer = 1 << m_Settings.layer;
                    renderer.EnqueuePass(m_SSRPass);
                }

                if (m_Settings.activeShadowType == SharpShadowSettings.ActiveShadowType.Planar)
                {
                    Shader.SetGlobalFloat("_ME_ShadowFloor", m_Settings.shadowFloorHeight);
                    m_SARPass.layer = 1 << m_Settings.layer;
                    renderer.EnqueuePass(m_SARPass);
                }
                else if (m_Settings.activeShadowType == SharpShadowSettings.ActiveShadowType.Active)
                {
                    Shader.SetGlobalFloat("_ME_ShadowNear", m_Settings.shadowNearExtrude);
                    Shader.SetGlobalFloat("_ME_ShadowFar", m_Settings.shadowFarExtrude);
                    Shader.SetGlobalFloat("_ME_ShadowFloor", m_Settings.shadowFloorHeight);
                    m_SARPass.layer = 1 << m_Settings.layer;
                    renderer.EnqueuePass(m_SARPass);
                }

                if (GetMaterial())
                {
                    Shader.SetGlobalFloat("_ME_ShadowIntensity", m_Settings.intensity);
                    renderer.EnqueuePass(m_SVRPass);
                }
            }
        }
        
        private bool GetMaterial()
        {
            if (m_Material != null && m_SVRPass.material != null)
            {
                return true;
            }

            if (m_VisualShader == null)
            {
                m_VisualShader = Shader.Find(k_ShaderVisualName);
                if (m_VisualShader == null)
                {
                    return false;
                }
            }

            m_Material = CoreUtils.CreateEngineMaterial(m_VisualShader);
            m_SVRPass.material = m_Material;
            return m_Material != null;
        }
        
        public static bool RenderRefreshed()
        {
            if (renderCreated != null)
            {
                renderCreated.Invoke();
                return true;
            }
            return false;
        }
        
        private class SharpShadowRenderPass : ScriptableRenderPass
        {
            internal LayerMask layer;
            
            private List<ShaderTagId> tags;
            private ProfilingSampler sampler;
            
            internal SharpShadowRenderPass(RenderPassEvent renderPassEvent, string[] tags, string sampler)
            {
                this.renderPassEvent = renderPassEvent;
                this.sampler = new ProfilingSampler(sampler);
                this.tags = new List<ShaderTagId>();
                foreach (var tag in tags)
                {
                    this.tags.Add(new ShaderTagId(tag));
                }
            }
            
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (tags == null || tags.Count == 0)
                {
                    Debug.LogError($"{GetType().Name}.Execute(): Missing tags. {GetType().Name} render pass " +
                                   $"will not execute. Check for missing reference in the renderer resources.");
                    return;
                }
                
                CommandBuffer cmd = CommandBufferPool.Get("Effects Renderers Pass");
                
                using (new ProfilingScope(cmd, sampler))
                {
                    DrawingSettings drawingSettings = CreateDrawingSettings(tags, ref renderingData, SortingCriteria.CommonOpaque);
                    drawingSettings.enableDynamicBatching = true;
                    FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque, layer);
                    RenderStateBlock renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
                    context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings, ref renderStateBlock);
                }
                
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
            
            public override void OnCameraCleanup(CommandBuffer cmd)
            {
                if (cmd == null)
                {
                    throw new ArgumentNullException("cmd");
                }
            }
        }
        
        private class ScreenRenderPass : ScriptableRenderPass
        {
            internal Material material;
            
            private ProfilingSampler sampler;

            internal ScreenRenderPass(RenderPassEvent renderPassEvent, string sampler)
            {
                this.renderPassEvent = renderPassEvent;
                this.sampler = new ProfilingSampler(sampler);
            }
            
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (material == null)
                {
                    Debug.LogError($"{GetType().Name}.Execute(): Missing material. {GetType().Name} render pass " +
                                   $"will not execute. Check for missing reference in the renderer resources.");
                    return;
                }

                CommandBuffer cmd = CommandBufferPool.Get("Screen Render Pass");
                Camera camera = renderingData.cameraData.camera;

                using (new ProfilingScope(cmd, sampler))
                {
                    cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
                    cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, material);
                    cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
                }
                
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
            
            public override void OnCameraCleanup(CommandBuffer cmd)
            {
                if (cmd == null)
                {
                    throw new ArgumentNullException(nameof(cmd));
                }
            }
        }
    }
}
