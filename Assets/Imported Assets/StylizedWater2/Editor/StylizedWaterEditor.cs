using System.Collections;
//Stylized Water 2
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
#if URP
using UnityEngine.Rendering.Universal;
#endif

namespace StylizedWater2
{
    public class StylizedWaterEditor : Editor
    {
        [MenuItem("Assets/Create/Water mesh")]
        private static void CreateWaterPlaneAsset()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New Watermesh.watermesh");

            File.Create(assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPathAndName);

            Selection.activeObject = mesh;
        }
        
        [MenuItem("CONTEXT/Transform/Add floating transform")]
        private static void AddFloatingTransform(MenuCommand cmd)
        {
            Transform t = (Transform)cmd.context;

            if (!t.gameObject.GetComponent<FloatingTransform>())
            {
                FloatingTransform component = t.gameObject.AddComponent<FloatingTransform>();
            }
        }

        public static void OpenGraphicsSettings()
        {
            SettingsService.OpenProjectSettings("Project/Graphics");
        }
        
        public static void SelectForwardRenderer()
        {
			#if URP
            if (!UniversalRenderPipeline.asset) return;

            System.Reflection.BindingFlags bindings = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            ScriptableRendererData[] m_rendererDataList = (ScriptableRendererData[])typeof(UniversalRenderPipelineAsset).GetField("m_RendererDataList", bindings).GetValue(UniversalRenderPipeline.asset);

            ForwardRendererData main = m_rendererDataList[0] as ForwardRendererData;
            Selection.activeObject = main;
			#endif
        }

        public static void EnableDepthTexture()
        {
			#if URP
            if (!UniversalRenderPipeline.asset) return;

            UniversalRenderPipeline.asset.supportsCameraDepthTexture = true;
            EditorUtility.SetDirty(UniversalRenderPipeline.asset);
			#endif
        }

        public static void EnableOpaqueTexture()
        {
			#if URP
            if (!UniversalRenderPipeline.asset) return;

            UniversalRenderPipeline.asset.supportsCameraOpaqueTexture = true;
            EditorUtility.SetDirty(UniversalRenderPipeline.asset);
			#endif
        }
        
        public class DWP2
        {
            private const string DataProviderName = "StylizedWaterDataProvider";
            
            public static bool isInstalled;
            public static bool dataProviderUnlocked;

            public static void CheckInstallation()
            {
                isInstalled = IsDWPInstalled();
                dataProviderUnlocked = DWPDataProviderUnlocked();
            }

            public static bool IsDWPInstalled()
            {
                string[] results = AssetDatabase.FindAssets("DWP_NUIEditor");

                if(results.Length > 0)
                {
                    isInstalled = true;
                    return true;
                }
                else
                {
                    isInstalled = false;
                }
                return false;

            }

            public static bool DWPDataProviderUnlocked()
            {
                string[] results = AssetDatabase.FindAssets(DataProviderName);

                if (results.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(results[0]);
                    return !path.Contains(".txt");
                }

                return false;
            }

            public static void UnlockDataProvider()
            {
                string[] results = AssetDatabase.FindAssets(DataProviderName);

                if (results.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(results[0]);
                    string absPath = Application.dataPath + path.Replace("Assets", string.Empty);
                    System.IO.File.Move(absPath, absPath.Replace(".txt", string.Empty));

                    AssetDatabase.Refresh();
                    dataProviderUnlocked = true;
                }
                else
                {
                    Debug.LogError("The file " + DataProviderName + ".cs could not be found. Check that you are using the latest version of DWP2");
                    return;
                }
                
                Debug.Log("StylizedWaterDataProvider component is now available");
            }
        }
    }
}