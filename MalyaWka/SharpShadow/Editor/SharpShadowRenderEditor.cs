using MalyaWka.SharpShadow.Utils.Editor;
using UnityEngine;
using UnityEditor;

namespace MalyaWka.SharpShadow.Editor
{
    [CustomEditor(typeof(Renders.SharpShadowRender))]
    public class SharpShadowRenderEditor : EditorDrawer
    {
        #region Serialized Properties
        private SerializedProperty m_RenderPassEvent;
        private SerializedProperty m_Layer;
        private SerializedProperty m_StaticShadowType;
        private SerializedProperty m_ActiveShadowType;
        private SerializedProperty m_ShadowNearExtrude;
        private SerializedProperty m_ShadowFarExtrude;
        private SerializedProperty m_ShadowFloorHeight;
        private SerializedProperty m_Intensity;
        #endregion
        
        private bool m_IsInitialized = false;
        
        private struct Styles
        {
            public static GUIContent RenderPassEvent = EditorGUIUtility.TrTextContent("Render Pass Event", "...");
            public static GUIContent Layer = EditorGUIUtility.TrTextContent("Render Layer", "...");
            public static GUIContent StaticShadowType = EditorGUIUtility.TrTextContent("Static Shadows", "...");
            public static GUIContent ActiveShadowType = EditorGUIUtility.TrTextContent("Planar & Active Shadows", "...");
            public static GUIContent ShadowNearExtrude = EditorGUIUtility.TrTextContent("Near Extrude", "...");
            public static GUIContent ShadowFarExtrude = EditorGUIUtility.TrTextContent("Far Extrude", "...");
            public static GUIContent ShadowFloorHeight = EditorGUIUtility.TrTextContent("Floor Height", "...");
            public static GUIContent Intensity = EditorGUIUtility.TrTextContent("Intensity", "...");
        }
        
        private void Init()
        {
            SerializedProperty settings = serializedObject.FindProperty("m_Settings");
            m_RenderPassEvent = settings.FindPropertyRelative("renderPassEvent");
            m_Layer = settings.FindPropertyRelative("layer");
            m_StaticShadowType = settings.FindPropertyRelative("staticShadowType");
            m_ActiveShadowType = settings.FindPropertyRelative("activeShadowType");
            m_ShadowNearExtrude = settings.FindPropertyRelative("shadowNearExtrude");
            m_ShadowFarExtrude = settings.FindPropertyRelative("shadowFarExtrude");
            m_ShadowFloorHeight = settings.FindPropertyRelative("shadowFloorHeight");
            m_Intensity = settings.FindPropertyRelative("intensity");
            m_IsInitialized = true;
        }
        
        public override void OnInspectorGUI()
        {
            if (!m_IsInitialized)
            {
                Init();
            }
            
            serializedObject.Update();
            
            BeginBox("General");
            EditorGUILayout.PropertyField(m_RenderPassEvent, Styles.RenderPassEvent);
            EditorGUILayout.PropertyField(m_Layer, Styles.Layer);
            EndBox();

            BeginBox("Shadows");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_StaticShadowType, Styles.StaticShadowType);
            EditorGUILayout.PropertyField(m_ActiveShadowType, Styles.ActiveShadowType);
            if (m_StaticShadowType.enumValueIndex != 0 || m_ActiveShadowType.enumValueIndex != 0)
            {
                m_Intensity.floatValue = EditorGUILayout.Slider(Styles.Intensity,
                    m_Intensity.floatValue, 0.0f, 1.0f);
            }
            if (m_ActiveShadowType.enumValueIndex != 0)
            {
                m_ShadowFloorHeight.floatValue = EditorGUILayout.Slider(Styles.ShadowFloorHeight,
                    m_ShadowFloorHeight.floatValue, -10.0f, 10.0f);
                m_ShadowNearExtrude.floatValue = EditorGUILayout.Slider(Styles.ShadowNearExtrude,
                    m_ShadowNearExtrude.floatValue, -1.0f, 1.0f);
                m_ShadowFarExtrude.floatValue = EditorGUILayout.Slider(Styles.ShadowFarExtrude,
                    m_ShadowFarExtrude.floatValue, -1.0f, 1.0f);
            }
            EndBox();

            bool changed = EditorGUI.EndChangeCheck();
            serializedObject.ApplyModifiedProperties();
            
            if (changed)
            {
                Renders.SharpShadowRender.RenderRefreshed();
            }
        }
    }
}
