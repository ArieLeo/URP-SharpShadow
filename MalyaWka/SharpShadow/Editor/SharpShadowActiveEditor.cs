using System;
using System.Threading.Tasks;
using MalyaWka.SharpShadow.Utils;
using MalyaWka.SharpShadow.Utils.Editor;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace MalyaWka.SharpShadow.Editor
{
    [CustomEditor(typeof(Runtime.SharpShadowActive)), CanEditMultipleObjects]
    public class SharpShadowActiveEditor : EditorDrawer
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            SerializedProperty m_MeshChild = serializedObject.FindProperty("meshChild");
            SerializedProperty m_MeshRenderer = serializedObject.FindProperty("meshRenderer");
            SerializedProperty m_MeshFilterSelf = serializedObject.FindProperty("meshFilterSelf");
            SerializedProperty m_Mesh = serializedObject.FindProperty("mesh");
            SerializedProperty m_DisableShadows = serializedObject.FindProperty("disableShadows");
            SerializedProperty m_BoundsFactor = serializedObject.FindProperty("boundsFactor");
            
            bool isChild = (GameObject)m_MeshChild.objectReferenceValue != null;
            bool create = false, remove = false;
            Object[] all = serializedObject.isEditingMultipleObjects ? targets : new [] { target} ;

            BeginBox("General");
            if (!isChild)
            {
                EditorGUILayout.HelpBox("Child object not created. Please, click 'Create' button for create child object.", MessageType.Warning);
                DrawBoldLabel("Mutable");
                m_BoundsFactor.floatValue = EditorGUILayout.Slider(new GUIContent("Bounds Factor"),m_BoundsFactor.floatValue, 0.0f, 1.0f);
                EditorGUILayout.PropertyField(m_DisableShadows);
                create = GUILayout.Button("Create");
            }
            else
            {
                GUI.enabled = false;
                EditorGUILayout.PropertyField(m_MeshChild);
                EditorGUILayout.PropertyField(m_MeshFilterSelf);
                EditorGUILayout.PropertyField(m_MeshRenderer);
                EditorGUILayout.PropertyField(m_Mesh);    
                GUI.enabled = true;

                DrawBoldLabel("Mutable");
                m_BoundsFactor.floatValue = EditorGUILayout.Slider(new GUIContent("Bounds Factor"),m_BoundsFactor.floatValue, 0.0f, 1.0f);
                EditorGUILayout.PropertyField(m_DisableShadows);
                remove = GUILayout.Button("Remove");
            }
            EndBox();
            
            serializedObject.ApplyModifiedProperties();

            if (create)
            {
                Create(all);
            }

            if (remove)
            {
                Remove(all);
            }
        }
        
        private async void Create(Object[] all)
        {
            for (var i = 0; i < all.Length; i++)
            {
                Object o = all[i];
                EditorUtility.DisplayProgressBar("Mesh Creator Working", $"Create mesh for {o.name}", i / all.Length > 0 ? all.Length : i);
                
                await Create(o);
                EditorUtility.SetDirty(o);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        private async void Remove(Object[] all)
        {
            for (var i = 0; i < all.Length; i++)
            {
                Object o = all[i];
                EditorUtility.DisplayProgressBar("Mesh Creator Working", $"Remove mesh for {o.name}", i / all.Length > 0 ? all.Length : i);
                
                await Remove(o);
                EditorUtility.SetDirty(o);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }
        
        private Task Create(Object current)
        {
            Runtime.SharpShadowActive self = (Runtime.SharpShadowActive)current;
            if (self)
            {
                
                if (self.meshChild)
                { 
                    Remove(current);
                }
                
                int childCount = self.transform.childCount;
                for (int i = childCount - 1; i >= 0; i--)
                {
                    GameObject childObj = self.transform.GetChild(i).gameObject;
                    if (childObj.name == "sharp_shadow_active")
                    {
                        DestroyImmediate(childObj);
                        i--;
                    }
                }
                
                MeshFilter meshFilterSelf = self.GetComponent<MeshFilter>();
                Mesh reference = meshFilterSelf.sharedMesh;
                if (!reference)
                {
                    Debug.LogWarning($"Reference mesh for {self.name} is null!");
                    return Task.CompletedTask;
                }
                
                Mesh mesh = CreateMesh(reference, self.boundsFactor);

                GameObject child = new GameObject($"sharp_shadow_active");
                child.hideFlags = HideFlags.HideInHierarchy;
                child.layer = 1;
                child.transform.SetParent(self.transform);
                child.transform.localPosition = Vector3.zero;
                child.transform.localRotation = Quaternion.identity;
                child.transform.localScale = Vector3.one;
                
                MeshFilter meshFilter = child.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = mesh;

                Material material = AssetDatabase.LoadAssetAtPath<Material>("Assets/MalyaWka/SharpShadow/Materials/StencilOnActive.mat");
                MeshRenderer meshRenderer = child.AddComponent<MeshRenderer>();
                meshRenderer.materials = new Material[1];
                meshRenderer.material = material;
                
                meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                meshRenderer.receiveShadows = false;
                meshRenderer.lightProbeUsage = LightProbeUsage.Off;
                meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                
                self.meshRenderer = meshRenderer;
                self.meshFilterSelf = meshFilterSelf;
                self.mesh = mesh;
                self.meshChild = child;
                return Task.CompletedTask;
            }
            throw new Exception($"Self object is null!");
        }
        
        private Task Remove(Object current)
        {
            Runtime.SharpShadowActive self = (Runtime.SharpShadowActive)current;
            if (self)
            {
                if (self.meshChild)
                {
                    Mesh reference = self.meshFilterSelf.sharedMesh;
                    RemoveMesh(reference, self.mesh);
                    
                    DestroyImmediate(self.meshChild);
                    self.meshChild = null;
                    self.meshFilterSelf = null;
                    self.mesh = null;
                    self.meshRenderer = null;
                }
                return Task.CompletedTask;
            }
            throw new Exception($"Self object is null!");
        }
        
        private Mesh CreateMesh(Mesh reference, float boundsFactor)
        {
            SharpShadowAsset meshAsset = GetSharpShadowAsset(reference, false);
            int foundedIdx = meshAsset.activeMeshAssets.FindIndex(x => x.origin == reference.name);

            if (foundedIdx == -1)
            {
                SharpShadowAsset.MeshAssetActive meshAssetActive = new SharpShadowAsset.MeshAssetActive();
                meshAssetActive.origin = reference.name;
                meshAssetActive.mesh = MeshCreatorActive.CreateMesh(reference, boundsFactor);
                meshAssetActive.useCount++;

                meshAsset.activeMeshAssets.Add(meshAssetActive);
                AssetDatabase.AddObjectToAsset(meshAssetActive.mesh, AssetDatabase.GetAssetPath(meshAsset));
                EditorUtility.SetDirty(meshAsset);
                
                return meshAssetActive.mesh;
            }
            else
            {
                SharpShadowAsset.MeshAssetActive meshAssetActive = meshAsset.activeMeshAssets[foundedIdx];
                meshAssetActive.useCount++;
                EditorUtility.SetDirty(meshAsset);
                
                return meshAssetActive.mesh;
            }
        }
        
        private void RemoveMesh(Mesh reference, Mesh created)
        {
            SharpShadowAsset meshAsset = GetSharpShadowAsset(reference, true);
            if (!meshAsset) return;
            
            int foundedIdx = meshAsset.activeMeshAssets.FindIndex(x => x.mesh == created);
            if (foundedIdx != -1)
            {
                SharpShadowAsset.MeshAssetActive meshAssetActive = meshAsset.activeMeshAssets[foundedIdx];
                meshAssetActive.useCount--;
                
                if (meshAssetActive.useCount <= 0)
                {
                    AssetDatabase.RemoveObjectFromAsset(meshAssetActive.mesh);
                    DestroyImmediate(meshAssetActive.mesh, true);
                    meshAsset.activeMeshAssets.RemoveAt(foundedIdx);
                }
                if (meshAsset.staticMeshAssets.Count <= 0 && meshAsset.activeMeshAssets.Count <= 0)
                {
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(meshAsset));
                }
                else
                {
                    EditorUtility.SetDirty(meshAsset);
                }
            }
            else
            {
                Debug.LogWarning($"Sharp Shadow asset for {reference.name} is missing..");
            }
        }
    }
}
