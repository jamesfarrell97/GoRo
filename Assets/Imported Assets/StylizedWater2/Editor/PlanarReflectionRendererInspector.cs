using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;

namespace StylizedWater2
{
    [CustomEditor(typeof(PlanarReflectionRenderer))]
    public class PlanarReflectionRendererInspector : Editor
    {
        private PlanarReflectionRenderer renderer;
        
        //Rendering
        private SerializedProperty cullingMask;
        private SerializedProperty rendererIndex;
        private SerializedProperty offset;
        private SerializedProperty includeSkybox;
        
        //Quality
        private SerializedProperty renderRange;
        private SerializedProperty renderScale;
        
        private SerializedProperty waterObjects;

        private Bounds curBounds;

        private void OnEnable()
        {
#if URP
            PipelineUtilities.RefreshRendererList();
            
            renderer = (PlanarReflectionRenderer)target;

            cullingMask = serializedObject.FindProperty("cullingMask");
            rendererIndex = serializedObject.FindProperty("rendererIndex");
            offset = serializedObject.FindProperty("offset");
            includeSkybox = serializedObject.FindProperty("includeSkybox");
            renderRange = serializedObject.FindProperty("renderRange");
            renderScale = serializedObject.FindProperty("renderScale");
            waterObjects = serializedObject.FindProperty("waterObjects");
            
            if (renderer.waterObjects.Count == 0 && WaterObject.Instances.Count == 1)
            {
                renderer.waterObjects.Add(WaterObject.Instances[0]);
                renderer.RecalculateBounds();
                renderer.EnableMaterialReflectionSampling();
                
                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }

            curBounds = renderer.CalculateBounds();
#endif
        }

        public override void OnInspectorGUI()
        {
#if !URP
            UI.DrawNotification("The Universal Render Pipeline package v" + AssetInfo.MIN_URP_VERSION + " or newer is not installed", MessageType.Error);
#else
            UI.DrawNotification(XRSettings.enabled, "Not supported with VR rendering", MessageType.Error);

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.LabelField("Rendering", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(cullingMask);
            EditorGUI.BeginChangeCheck();
            UI.DrawRendererProperty(rendererIndex);
            if (EditorGUI.EndChangeCheck())
            {
                renderer.SetRendererIndex(rendererIndex.intValue);
            }
            EditorGUILayout.PropertyField(offset);
            EditorGUILayout.PropertyField(includeSkybox);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Quality", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(renderRange);
            EditorGUILayout.PropertyField(renderScale);
            
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Affected water objects", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(waterObjects);
            

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
            
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if(GUILayout.Button(new GUIContent("Auto-find", "Assigns all active water objects current in the scene"), EditorStyles.miniButton))
                {
                    renderer.waterObjects = new List<WaterObject>(WaterObject.Instances);
 
                    renderer.RecalculateBounds();
                    renderer.EnableMaterialReflectionSampling();
                    
                    EditorUtility.SetDirty(target);
                }
                if(GUILayout.Button("Clear", EditorStyles.miniButton))
                {
                    renderer.DisableReflections();
                    renderer.waterObjects.Clear();
                    renderer.RecalculateBounds();
                    
                    EditorUtility.SetDirty(target);
                }
            }
            
            if (renderer.waterObjects != null)
            {
                UI.DrawNotification(renderer.waterObjects.Count == 0, "Assign at least one WaterObject", MessageType.Info);
                
                if (renderer.waterObjects.Count > 0)
                {
                    UI.DrawNotification(curBounds.size != renderer.bounds.size || curBounds.center != renderer.bounds.center, "Water objects have moved, bounds needs to be recalculated", "Recalculate",() => RecalculateBounds(), MessageType.Error);
                }
            }

#endif
            
            UI.DrawFooter();
        }

        private void RecalculateBounds()
        {
#if URP
            renderer.RecalculateBounds();
            curBounds = renderer.bounds;
            EditorUtility.SetDirty(target);
#endif
        }
    }
}
