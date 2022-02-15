using UnityEditor;
using UnityEngine;

namespace MalyaWka.SharpShadow.Editor
{
    [InitializeOnLoad]
    public class SharpShadowHierarchyIcon
    {
        private static readonly Texture2D IconStatic;
        private static readonly Texture2D IconStaticBroken;
        private static readonly Texture2D IconActive;
        private static readonly Texture2D IconActiveBroken;
        private static readonly Texture2D IconSkin;
        private static readonly Texture2D IconSkinBroken;

        private static readonly GUIStyle Style;

        static SharpShadowHierarchyIcon()
        {
            IconStatic = AssetDatabase.LoadAssetAtPath("Assets/MalyaWka/SharpShadow/Gizmos/IconStatic.png", typeof(Texture2D)) as Texture2D;
            IconStaticBroken = AssetDatabase.LoadAssetAtPath("Assets/MalyaWka/SharpShadow/Gizmos/IconStaticBroken.png", typeof(Texture2D)) as Texture2D;
            IconActive = AssetDatabase.LoadAssetAtPath("Assets/MalyaWka/SharpShadow/Gizmos/IconActive.png", typeof(Texture2D)) as Texture2D;
            IconActiveBroken = AssetDatabase.LoadAssetAtPath("Assets/MalyaWka/SharpShadow/Gizmos/IconActiveBroken.png", typeof(Texture2D)) as Texture2D;
            IconSkin = AssetDatabase.LoadAssetAtPath("Assets/MalyaWka/SharpShadow/Gizmos/IconSkin.png", typeof(Texture2D)) as Texture2D;
            IconSkinBroken = AssetDatabase.LoadAssetAtPath("Assets/MalyaWka/SharpShadow/Gizmos/IconSkinBroken.png", typeof(Texture2D)) as Texture2D;

            if (!IconStatic ||!IconStaticBroken || !IconActive || !IconActiveBroken || !IconSkin || !IconSkinBroken)
            {
                return;
            } 

            EditorApplication.hierarchyWindowItemOnGUI += DrawIconOnWindowItem;
        }
        
        private static void DrawIconOnWindowItem(int instanceID, Rect rect)
        {
            if (!IconStatic || !IconStaticBroken || !IconActive || !IconActiveBroken)
            {
                return;
            }

            GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

            if (gameObject == null)
            {
                return;
            }
            
            Runtime.SharpShadowStatic meshStatic = gameObject.GetComponent<Runtime.SharpShadowStatic>();
            if (meshStatic != null)
            {
                float iconWidth = 15;
                EditorGUIUtility.SetIconSize(new Vector2(iconWidth, iconWidth));
                Rect iconDrawRect = new Rect(rect.xMin - 1, rect.yMin, rect.width, rect.height);
                GUIContent iconGUIContent = new GUIContent(meshStatic.meshChild && meshStatic.mesh ? IconStatic : IconStaticBroken);
                EditorGUI.LabelField(iconDrawRect, iconGUIContent);
                EditorGUIUtility.SetIconSize(Vector2.zero);
            }
            
            Runtime.SharpShadowActive meshActive = gameObject.GetComponent<Runtime.SharpShadowActive>();
            if (meshActive != null)
            {
                float iconWidth = 15;
                EditorGUIUtility.SetIconSize(new Vector2(iconWidth, iconWidth));
                Rect iconDrawRect = new Rect(rect.xMin - 1, rect.yMin, rect.width, rect.height);
                GUIContent iconGUIContent = new GUIContent(meshActive.meshChild && meshActive.mesh ? IconActive : IconActiveBroken);
                EditorGUI.LabelField(iconDrawRect, iconGUIContent);
                EditorGUIUtility.SetIconSize(Vector2.zero);
            }
            
            Runtime.SharpShadowSkin meshSkin = gameObject.GetComponent<Runtime.SharpShadowSkin>();
            if (meshSkin != null)
            {
                float iconWidth = 15;
                EditorGUIUtility.SetIconSize(new Vector2(iconWidth, iconWidth));
                Rect iconDrawRect = new Rect(rect.xMin - 1, rect.yMin, rect.width, rect.height);
                GUIContent iconGUIContent = new GUIContent(meshSkin.meshChild && meshSkin.mesh ? IconSkin : IconSkinBroken);
                EditorGUI.LabelField(iconDrawRect, iconGUIContent);
                EditorGUIUtility.SetIconSize(Vector2.zero);
            }
        }
    }
}
