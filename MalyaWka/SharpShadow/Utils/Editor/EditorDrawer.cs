using System.IO;
using MalyaWka.SharpShadow.Editor;
using UnityEngine;
using UnityEditor;

namespace MalyaWka.SharpShadow.Utils.Editor
{
    public class EditorDrawer : UnityEditor.Editor
    {
        protected void BeginBox(string boxTitle = "")
        {
            GUIStyle style = new GUIStyle("HelpBox");
            style.padding.left = 5;
            style.padding.right = 5;
            style.padding.top = 5;
            style.padding.bottom = 5;

            GUILayout.BeginVertical(style);

            if (!string.IsNullOrEmpty(boxTitle))
            {
                DrawBoldLabel(boxTitle);
            }
        }
        
        protected void EndBox()
        {
            GUILayout.EndVertical();
        }
        
        protected void DrawBoldLabel(string text)
        {
            EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
        }

        #region Path Helper

        protected string GetDirectoryName(string path)
        {
            return string.IsNullOrEmpty(path) ? string.Empty : Path.GetDirectoryName(path)?.Replace('\\', '/');
        }

        protected string GetFileNameWithoutExtension(string path)
        {
            return string.IsNullOrEmpty(path) ? string.Empty : Path.GetFileNameWithoutExtension(path);
        }

        protected string CombinePath(params string[] paths)
        {
            return paths.Length == 0 ? string.Empty : Path.Combine(paths).Replace('\\', '/');
        }

        protected string GetSharpShadowAssetPath(string assetPath)
        {
            return string.IsNullOrEmpty(assetPath) ? string.Empty : CombinePath(GetDirectoryName(assetPath), GetFileNameWithoutExtension(assetPath) + "_sharp_shadow.asset");
        }
        
        protected SharpShadowAsset GetSharpShadowAsset(Mesh reference, bool remove)
        {
            string assetPath = AssetDatabase.GetAssetPath(reference);
            string path = GetSharpShadowAssetPath(assetPath);
            if (string.IsNullOrEmpty(path)) return null;
            SharpShadowAsset asset = AssetDatabase.LoadAssetAtPath<SharpShadowAsset>(path);
            
            if (!asset)
            {
                if (remove) return null;
                
                if (File.Exists(path))
                {
                    return null;
                }
                asset = CreateInstance<SharpShadowAsset>();
            }

            if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(asset)))
            {
                if (remove) return null;
                
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
            }
            
            return asset;
        }

        #endregion
    }
}
