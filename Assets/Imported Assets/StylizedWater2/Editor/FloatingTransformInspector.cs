//Stylized Water 2
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
#if UNITY_2021_2
using PrefabStageUtility = UnityEditor.SceneManagement.PrefabStageUtility;
#else
using PrefabStageUtility = UnityEditor.Experimental.SceneManagement.PrefabStageUtility;
#endif

namespace StylizedWater2
{
    [CustomEditor(typeof(FloatingTransform))]
    public class FloatingTransformInspector : Editor
    {
        FloatingTransform script;

        SerializedProperty waterObject;
        SerializedProperty waterLevelSource;
        SerializedProperty waterLevel;
        SerializedProperty childTransform;

        SerializedProperty heightOffset;
        SerializedProperty rollAmount;

        SerializedProperty samples;

        private bool editSamples;
        private bool isRiver;
        
        private void OnEnable()
        {
            script = (FloatingTransform)target;

            waterObject = serializedObject.FindProperty("waterObject");
            waterLevelSource = serializedObject.FindProperty("waterLevelSource");
            waterLevel = serializedObject.FindProperty("waterLevel");
            childTransform = serializedObject.FindProperty("childTransform");
            heightOffset = serializedObject.FindProperty("heightOffset");
            rollAmount = serializedObject.FindProperty("rollAmount");
            samples = serializedObject.FindProperty("samples");

            if (script.waterObject && script.waterObject.material)
                isRiver = script.waterObject.material.IsKeywordEnabled("_RIVER");
        }

        private void OnDisable()
        {
            FloatingTransform.Disable = false;
            Tools.hidden = false;
        }

        public override void OnInspectorGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(EditorGUIUtility.labelWidth);
                FloatingTransform.EnableInEditor =
                    GUILayout.Toggle(FloatingTransform.EnableInEditor, new GUIContent(" Run in edit-mode (global)", EditorGUIUtility.IconContent(
                        (FloatingTransform.EnableInEditor ? "animationvisibilitytoggleon" : "animationvisibilitytoggleoff")).image), "Button");
            }
            
            EditorGUILayout.Space();

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            
            //Auto fetch if there is only one water body in the scene
            if (waterObject.objectReferenceValue == null && WaterObject.Instances.Count == 1)
            {
                waterObject.objectReferenceValue = WaterObject.Instances[0];
                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorGUILayout.PropertyField(waterObject);
            
            UI.DrawNotification(isRiver, "Material has river mode enabled, buoyancy only works for flat water bodies", MessageType.Error);
            
            if (script.waterObject && script.waterObject.material)
            {
                UI.DrawNotification((script.waterObject.material.GetFloat("_WorldSpaceUV") == 0f), "Material must use world-projected UV", "Change", ()=> script.waterObject.material.SetFloat("_WorldSpaceUV", 1f), MessageType.Error);
            }

            if(waterObject.objectReferenceValue == null)
            {
                UI.DrawNotification("A water object must be assigned!", MessageType.Error);
            }
            
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel("Water level source");
                waterLevelSource.intValue = GUILayout.Toolbar(waterLevelSource.intValue, new GUIContent[] { new GUIContent("Fixed Value"), new GUIContent("Water Object") });
            }
            if (waterLevelSource.intValue == (int)FloatingTransform.WaterLevelSource.FixedValue) EditorGUILayout.PropertyField(waterLevel);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(heightOffset);
            EditorGUILayout.PropertyField(rollAmount);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Sample positions", EditorStyles.boldLabel);
            
            if (samples.arraySize > 0)
            {
                editSamples =
                    GUILayout.Toggle(editSamples,
                        new GUIContent(" Edit samples", EditorGUIUtility.IconContent("sv_icon_dot0_pix16_gizmo").image),
                        "Button", GUILayout.MaxWidth(125f), GUILayout.MaxHeight(30f));
            }

            if (script.samples != null)
            {
                for (int i = 0; i < script.samples.Count; i++)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(i.ToString() + ":", GUILayout.MaxWidth(40f));
                        EditorGUI.BeginChangeCheck();
                        script.samples[i] = EditorGUILayout.Vector3Field("", script.samples[i]);
                        if (EditorGUI.EndChangeCheck())
                        {
                            EditorUtility.SetDirty(target);
                        }

                        if (GUILayout.Button(new GUIContent("",
                            EditorGUIUtility.IconContent("d_TreeEditor.Trash").image, "Delete item")))
                        {
                            script.samples.RemoveAt(i);
                            selectedSampleIndex = -1;
							
							EditorUtility.SetDirty(target);
                        }

                    }
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("Add", EditorGUIUtility.IconContent(EditorGUIUtility.isProSkin ? "d_Toolbar Plus" : "Toolbar Plus").image)))
                {
                    if(script.samples == null) script.samples = new List<Vector3>();
                    
                    script.samples.Add(Vector3.zero);
                    selectedSampleIndex = script.samples.Count-1;
					
					EditorUtility.SetDirty(target);
                }
            }
            
            EditorGUILayout.PropertyField(childTransform);
            if (childTransform.objectReferenceValue == null && samples.arraySize > 0)
                UI.DrawNotification("Assign a transform to rotate/scale the sample positions with");

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
            
            UI.DrawFooter();
        }

        private int selectedSampleIndex;
        Vector3 sampleWorldPos;
        Vector3 prevSampleWorldPos;

        private void OnSceneGUI()
        {
            if (!script) return;
            
            FloatingTransform.Disable = PrefabStageUtility.GetCurrentPrefabStage() != null || editSamples;
            
            if (editSamples)
            {
                //Mute default controls
                Tools.hidden = true;
                
                Handles.color = new Color(0.66f, 0.66f, 0.66f, 1);
                
                for (int i = 0; i < script.samples.Count; i++)
                {
                    sampleWorldPos = script.ConvertToWorldSpace(script.samples[i]);

                    float size = HandleUtility.GetHandleSize(sampleWorldPos) * 0.25f;
                    if (Handles.Button(sampleWorldPos, Quaternion.identity, size, size, Handles.SphereHandleCap))
                    {
                        selectedSampleIndex = i;
                    }
                }

                if (selectedSampleIndex > -1)
                {
                    sampleWorldPos = script.ConvertToWorldSpace(script.samples[selectedSampleIndex]);
                    prevSampleWorldPos = sampleWorldPos;
                    
                    sampleWorldPos = Handles.PositionHandle(sampleWorldPos, script.childTransform ? script.childTransform.rotation : script.transform.rotation );
                    script.samples[selectedSampleIndex] = script.ConvertToLocalSpace(sampleWorldPos);

                    //If moved
                    if (sampleWorldPos != prevSampleWorldPos)
                    {
                        prevSampleWorldPos = sampleWorldPos;
                        EditorUtility.SetDirty(target);
                    }
                }
            }
            else
            {
                selectedSampleIndex = -1;
                Tools.hidden = false;
                
                if (script.samples == null) return;

                Handles.color = new Color(1,1,1, 0.25f);
                for (int i = 0; i < script.samples.Count; i++)
                {
                    sampleWorldPos = script.ConvertToWorldSpace(script.samples[i]);
                    Handles.SphereHandleCap(0, sampleWorldPos, SceneView.lastActiveSceneView.camera.transform.rotation, HandleUtility.GetHandleSize(sampleWorldPos) * 0.25f, EventType.Repaint);
                }
            }
        }
    }
}