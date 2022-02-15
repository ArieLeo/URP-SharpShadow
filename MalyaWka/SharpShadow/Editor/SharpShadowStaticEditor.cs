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
    [CustomEditor(typeof(Runtime.SharpShadowStatic)), CanEditMultipleObjects]
    public class SharpShadowStaticEditor : EditorDrawer
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            SerializedProperty m_MeshChild = serializedObject.FindProperty("meshChild");
            SerializedProperty m_MeshFilter = serializedObject.FindProperty("meshFilter");
            SerializedProperty m_Mesh = serializedObject.FindProperty("mesh");
            SerializedProperty m_MeshRenderer = serializedObject.FindProperty("meshRenderer");
            
            SerializedProperty m_Ground = serializedObject.FindProperty("ground");
            SerializedProperty m_Offset = serializedObject.FindProperty("offset");
            SerializedProperty m_Reverse = serializedObject.FindProperty("reverse");
            
            bool isChild = (GameObject)m_MeshChild.objectReferenceValue != null;
            bool create = false, remove = false, refresh = false;
            Object[] all = serializedObject.isEditingMultipleObjects ? targets : new [] { target} ;

            BeginBox("General");
            if (!isChild)
            {
                EditorGUILayout.HelpBox("Child object not created. Please, click 'Create' button for create child object.", MessageType.Warning);
                DrawBoldLabel("Mutable");
                EditorGUILayout.PropertyField(m_Ground);
                m_Offset.floatValue = EditorGUILayout.Slider(new GUIContent("Offset"),m_Offset.floatValue, -0.5f, 0.5f);
                EditorGUILayout.PropertyField(m_Reverse);
                create = GUILayout.Button("Create");
            }
            else
            {
                GUI.enabled = false;
                EditorGUILayout.PropertyField(m_MeshChild);
                EditorGUILayout.PropertyField(m_MeshFilter);
                EditorGUILayout.PropertyField(m_Mesh);
                EditorGUILayout.PropertyField(m_MeshRenderer);
                GUI.enabled = true;

                DrawBoldLabel("Mutable");
                EditorGUILayout.PropertyField(m_Ground);
                m_Offset.floatValue = EditorGUILayout.Slider(new GUIContent("Offset"),m_Offset.floatValue, -0.5f, 0.5f);
                EditorGUILayout.PropertyField(m_Reverse);
                refresh = GUILayout.Button("Refresh");
                remove = GUILayout.Button("Remove");
            }
            EndBox();
            
            serializedObject.ApplyModifiedProperties();

            if (create)
            {
                Create(all);
            }

            if (refresh)
            {
                Refresh(all);
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
        
        private async void Refresh(Object[] all)
        {
            for (var i = 0; i < all.Length; i++)
            {
                Object o = all[i];
                EditorUtility.DisplayProgressBar("Mesh Creator Working", $"Refresh mesh for {o.name}", i / all.Length > 0 ? all.Length : i);
                
                await Refresh(o);
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
            Runtime.SharpShadowStatic self = (Runtime.SharpShadowStatic)current;
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
                    if (childObj.name == "sharp_shadow_static")
                    {
                        DestroyImmediate(childObj);
                        i--;
                    }
                }
                
                Mesh reference = self.GetComponent<MeshFilter>().sharedMesh;
                Light[] lights = Light.GetLights(LightType.Directional, 0);
                if (!reference)
                {
                    Debug.LogWarning($"Reference mesh for {self.name} is null!");
                    return Task.CompletedTask;
                }
                
                Mesh mesh = CreateMesh(reference, self.transform, lights[0], self.ground, self.offset, self.reverse);

                GameObject child = new GameObject($"sharp_shadow_static");
                child.hideFlags = HideFlags.HideInHierarchy;
                child.layer = 1;
                child.transform.SetParent(self.transform);
                child.transform.localPosition = Vector3.zero;
                child.transform.localRotation = Quaternion.identity;
                child.transform.localScale = Vector3.one;
                
                MeshFilter meshFilter = child.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = mesh;
                
                Material material = AssetDatabase.LoadAssetAtPath<Material>("Assets/MalyaWka/SharpShadow/Materials/StencilOnStatic.mat");
                MeshRenderer meshRenderer = child.AddComponent<MeshRenderer>();
                meshRenderer.materials = new Material[1];
                meshRenderer.material = material;
                
                meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                meshRenderer.receiveShadows = false;
                meshRenderer.lightProbeUsage = LightProbeUsage.Off;
                meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;

                self.meshChild = child;
                self.meshFilter = meshFilter;
                self.mesh = mesh;
                self.meshRenderer = meshRenderer;
                return Task.CompletedTask;
            }
            throw new Exception($"Self object is null!");
        }
        
        private Task Refresh(Object current)
        {
            Runtime.SharpShadowStatic self = (Runtime.SharpShadowStatic)current;
            if (self)
            {
                Mesh reference = self.GetComponent<MeshFilter>().sharedMesh;
                if (!reference)
                {
                    Debug.LogWarning($"Reference mesh for {self.name} is null!");
                    return Task.CompletedTask;
                }
                
                RemoveMesh(reference, self.mesh);
                self.mesh = null;
                
                Light[] lights = Light.GetLights(LightType.Directional, 0);
                Mesh mesh = CreateMesh(reference, self.transform, lights[0], self.ground, self.offset, self.reverse);
                
                self.meshFilter.sharedMesh = mesh;
                
                self.mesh = mesh;
                return Task.CompletedTask;
            }
            throw new Exception($"Self object is null!");
        }
        
        private Task Remove(Object current)
        {
            Runtime.SharpShadowStatic self = (Runtime.SharpShadowStatic)current;
            if (self)
            {
                if (self.meshChild)
                {
                    Mesh reference = self.GetComponent<MeshFilter>().sharedMesh;
                    RemoveMesh(reference, self.mesh);
                    
                    DestroyImmediate(self.meshChild);
                    self.meshChild = null;
                    self.meshFilter = null;
                    self.mesh = null;
                    self.meshRenderer = null;
                }
                return Task.CompletedTask;
            }
            throw new Exception($"Self object is null!");
        }
        
        private Mesh CreateMesh(Mesh reference, Transform transform, Light light, float ground, float offset, bool reverse)
        {
            SharpShadowAsset meshAsset = GetSharpShadowAsset(reference, false);
            int foundedIdx = meshAsset.staticMeshAssets.FindIndex(x =>
                x.origin == reference.name && x.reverse == reverse &&
                Mathf.Approximately(x.positionY, transform.position.y - ground) &&
                Mathf.Approximately(x.rotationY, NormalizeRotationY(transform.rotation.eulerAngles.y)));
            
            if (foundedIdx == -1)
            {
                SharpShadowAsset.MeshAssetStatic meshAssetStatic = new SharpShadowAsset.MeshAssetStatic();
                meshAssetStatic.origin = reference.name;
                meshAssetStatic.mesh = MeshCreatorStatic.CreateMesh(reference, transform, light, ground, offset, reverse);
                meshAssetStatic.positionY = transform.position.y - ground;
                meshAssetStatic.rotationY = NormalizeRotationY(transform.rotation.eulerAngles.y);
                meshAssetStatic.reverse = reverse;
                meshAssetStatic.useCount++;
                
                
                meshAsset.staticMeshAssets.Add(meshAssetStatic);
                AssetDatabase.AddObjectToAsset(meshAssetStatic.mesh, AssetDatabase.GetAssetPath(meshAsset));
                EditorUtility.SetDirty(meshAsset);

                return meshAssetStatic.mesh;
            }
            else
            {
                SharpShadowAsset.MeshAssetStatic meshAssetStatic = meshAsset.staticMeshAssets[foundedIdx];
                meshAssetStatic.useCount++;
                EditorUtility.SetDirty(meshAsset);
                
                return meshAssetStatic.mesh;
            }
        }
        
        private float NormalizeRotationY(float rotationY)
        {
            bool normalized = rotationY >= 0.0f && rotationY <= 360.0f;
            if (normalized) return rotationY;
            
            while (!normalized)
            {
                if (rotationY < 0.0f) rotationY += 360f;
                if (rotationY > 360.0f) rotationY -= 360f;
                normalized = rotationY >= 0.0f && rotationY <= 360.0f;
            }
            return rotationY;
        }
        
        private void RemoveMesh(Mesh reference, Mesh created)
        {
            SharpShadowAsset meshAsset = GetSharpShadowAsset(reference, true);
            if (!meshAsset) return;
            
            int foundedIdx = meshAsset.staticMeshAssets.FindIndex(x => x.mesh == created);

            if (foundedIdx != -1)
            {
                SharpShadowAsset.MeshAssetStatic meshAssetStatic = meshAsset.staticMeshAssets[foundedIdx];
                meshAssetStatic.useCount--;
                if (meshAssetStatic.useCount <= 0)
                {
                    AssetDatabase.RemoveObjectFromAsset(meshAssetStatic.mesh);
                    DestroyImmediate(meshAssetStatic.mesh, true);
                    meshAsset.staticMeshAssets.RemoveAt(foundedIdx);
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
