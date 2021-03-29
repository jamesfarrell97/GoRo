//#define RENDER_FEATURE

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if URP
using UnityEngine.Rendering.Universal;
#endif
using UnityEngine.Serialization;

namespace StylizedWater2
{
    [ExecuteInEditMode]
    [AddComponentMenu("Stylized Water 2/Planar Reflection Renderer")]
    public class PlanarReflectionRenderer : MonoBehaviour
    {
#if URP
        public static List<PlanarReflectionRenderer> Instances = new List<PlanarReflectionRenderer>();
        public Dictionary<Camera, Camera> reflectionCameras = new Dictionary<Camera, Camera>();

        #if RENDER_FEATURE
        //Allows the render pass to get the related instance
        public static Dictionary<Camera, PlanarReflectionRenderer> ReflectionRenderers =
         new Dictionary<Camera, PlanarReflectionRenderer>();

        public ForwardRendererData ForwardRendererData;
        private const string ForwardRendererDataGUID = "9769b595a3a38a342bc60810360664f8";
        #endif

        //Rendering
        [Tooltip("Set the layers that should be rendered into the reflection. The \"Water\" layer is always excluded")]
        public LayerMask cullingMask = -1;
        [Tooltip("The renderer used by the reflection camera. It's recommend to create a separate renderer, so any custom render features aren't executed for the reflection")]
        public int rendererIndex = -1;
#if RENDER_FEATURE
        public uint renderingLayerMask = 0xFFFFFFFF;
#endif
        public float offset = 0.05f;
        [Tooltip("When disabled, the skybox reflection comes from a Reflection Probe. This has the benefit of being omni-directional rather than flat/planar. Enabled this to render the skybox into the planar reflection anyway")]
        public bool includeSkybox;

        //Quality
        [Tooltip("Objects beyond this range aren't rendered into the reflection. Note that this may causes popping for large/tall objects.")]
		public float renderRange = 500f;
        [Range(0.25f, 1f)] 
        [Tooltip("A multiplier for the rendering resolution, based on the current screen resolution")]
		public float renderScale = 0.75f;
#if RENDER_FEATURE
        [Range(0, 8)]
        public int maxLights = 0;
        public bool vertexLights = true;
#endif
        
        [SerializeField]
        public List<WaterObject> waterObjects = new List<WaterObject>();
        [HideInInspector]
        public Bounds bounds;

        private Camera reflectionCamera;
        private static RenderTexture currentBuffer;
        private float m_renderScale = 1f;
        private float m_renderRange;
        private static bool m_reflectionsEnabled;
        public static bool ReflectionsEnabled { get { return m_reflectionsEnabled; } }
        private static int _PlanarReflectionsEnabledID = Shader.PropertyToID("_PlanarReflectionsEnabled");
        private static int _PlanarReflectionLeftID = Shader.PropertyToID("_PlanarReflectionLeft");
		private static UniversalAdditionalCameraData cameraData;
        
        private void OnEnable()
        {
#if UNITY_EDITOR && RENDER_FEATURE
            //Ensure the renderer is serialized
            if (ForwardRendererData == null)
            {
                ForwardRendererData = UnityEditor.AssetDatabase.LoadAssetAtPath<ForwardRendererData>(
                        UnityEditor.AssetDatabase.GUIDToAssetPath(ForwardRendererDataGUID));
            }
#endif

            InitializeValues();

            Instances.Add(this);
            EnableReflections();
        }

        private void OnDisable()
        {
            Instances.Remove(this);
            DisableReflections();
        }

        public void InitializeValues()
        {
            m_renderScale = renderScale;
            m_renderRange = renderRange;
        }

        /// <summary>
        /// Toggle reflections or set the render scale for all reflection renderers. This can be tied into performance scaling or graphics settings in menus
        /// </summary>
        /// <param name="enableReflections">Toggles rendering of reflections, and toggles it on all the assigned water objects</param>
        /// <param name="renderScale">A multiplier for the current screen resolution. Note that the render scale configured in URP is also taken into account</param>
        /// <param name="renderRange">Objects beyond this range aren't rendered into the reflection</param>
        public static void SetQuality(bool enableReflections, float renderScale = -1f, float renderRange = -1f)
        {
            foreach (PlanarReflectionRenderer renderer in Instances)
            {
                if (renderScale > 0) renderer.renderScale = renderScale;
                if (renderRange > 0) renderer.renderRange = renderRange;
                renderer.InitializeValues();

                if (enableReflections) renderer.EnableReflections();
                if (!enableReflections) renderer.DisableReflections();
            }
        }

        public void EnableReflections()
        {
            if (m_reflectionsEnabled) return;

            RenderPipelineManager.beginCameraRendering += OnWillRenderCamera;
            ToggleMaterialReflectionSampling(true);
            m_reflectionsEnabled = true;
        }

        public void DisableReflections()
        {
            if (!m_reflectionsEnabled) return;

            RenderPipelineManager.beginCameraRendering -= OnWillRenderCamera;
            ToggleMaterialReflectionSampling(false);
            m_reflectionsEnabled = false;

            //Clear cameras
            foreach (var kvp in reflectionCameras)
            {
                if (kvp.Value == null) continue;

                if (kvp.Value)
                {
                    RenderTexture.ReleaseTemporary(kvp.Value.targetTexture);
                    DestroyImmediate(kvp.Value.gameObject);
                }
            }

            reflectionCameras.Clear();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }

        public Bounds CalculateBounds()
        {
            Bounds m_bounds = new Bounds(Vector3.zero, Vector3.zero);
            
            if (waterObjects == null) return m_bounds;
            if (waterObjects.Count == 0) return m_bounds;

            if (waterObjects.Count == 1) return waterObjects[0].meshRenderer.bounds;

            m_bounds = waterObjects[0].meshRenderer.bounds;

            for (int i = 0; i < waterObjects.Count; i++)
            {
                if (waterObjects[i]) bounds.Encapsulate(waterObjects[i].meshRenderer.bounds);
            }

            return m_bounds;
        }

        public void RecalculateBounds()
        {
            bounds = CalculateBounds();
        }

        private void OnWillRenderCamera(ScriptableRenderContext context, Camera camera)
        {
#if SWS_DEV
            UnityEngine.Profiling.Profiler.BeginSample("Planar Reflections");
#endif
            //Skip for any special use camera's (except scene view camera)
            if (camera.cameraType != CameraType.SceneView && (camera.cameraType == CameraType.Reflection ||
                                                              camera.cameraType == CameraType.Preview ||
                                                              camera.hideFlags != HideFlags.None)) return;

            //Note: Scene camera still rendering even if window not focused!
            
            if (IsVisible(camera) == false) return;

            cameraData = camera.GetComponent<UniversalAdditionalCameraData>();
            if (cameraData && cameraData.renderType == CameraRenderType.Overlay) return;

            reflectionCameras.TryGetValue(camera, out reflectionCamera);
            if (reflectionCamera == null) CreateReflectionCamera(camera);
            
            //It's possible it is destroyed at this point when disabling reflections
            if (!reflectionCamera) return;
            
            UpdateWaterProperties(reflectionCamera);
            
            if (renderScale != m_renderScale)
            {
                RenderTexture.ReleaseTemporary(reflectionCamera.targetTexture);

                RenderTexture currentBuffer = RenderTexture.GetTemporary(
                    Mathf.RoundToInt(camera.scaledPixelWidth * GetRenderScale()),
                    Mathf.RoundToInt(camera.scaledPixelHeight * GetRenderScale()), 0,
                    camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);
                reflectionCamera.targetTexture = currentBuffer;

                m_renderScale = renderScale;
            }
            
            UpdatePerspective(camera, reflectionCamera);

#if UNITY_EDITOR
            //Screen pos outside of frustrum
            if (Vector3.Dot(Vector3.forward, reflectionCamera.transform.forward) > 0.9999f) return;
#endif
            reflectionCamera.clearFlags = includeSkybox ? CameraClearFlags.Skybox : CameraClearFlags.Depth;
            
            GL.invertCulling = true;
            UniversalRenderPipeline.RenderSingleCamera(context, reflectionCamera);
            GL.invertCulling = false;
            
#if SWS_DEV
            UnityEngine.Profiling.Profiler.EndSample();
#endif
        }

        private float GetRenderScale()
        {
            return Mathf.Clamp(renderScale * UniversalRenderPipeline.asset.renderScale, 0.25f, 1f);
        }

        /// <summary>
        /// Should the renderer index be changed at runtime, this function must be called to update any reflection cameras
        /// </summary>
        /// <param name="index"></param>
        public void SetRendererIndex(int index)
        {
            index = PipelineUtilities.ValidateRenderer(index);

            foreach (var kvp in reflectionCameras)
            {
                if (kvp.Value == null) continue;
                
                cameraData = kvp.Value.GetComponent<UniversalAdditionalCameraData>();
                cameraData.SetRenderer(index);
            }
        }

        private void CreateReflectionCamera(Camera source)
        {
            GameObject go = new GameObject(source.name + "_reflection_left");
            go.hideFlags = HideFlags.HideAndDontSave;
            Camera newCamera = go.AddComponent<Camera>();
            newCamera.hideFlags = HideFlags.DontSave;
            newCamera.CopyFrom(source);
            //Always exclude water layer
            newCamera.cullingMask = cullingMask = ~(1 << 4) & cullingMask;;
            //newCamera.cameraType = CameraType.Reflection; //Will cause shadow pass to execute twice?!
            newCamera.depth = -99f;
            newCamera.rect = new Rect(0,0,1,1);
            newCamera.enabled = false;
            newCamera.clearFlags = includeSkybox ? CameraClearFlags.Skybox : CameraClearFlags.Depth;
            newCamera.useOcclusionCulling = false;

            UniversalAdditionalCameraData data = newCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            data.requiresDepthTexture = true;
            data.requiresColorTexture = false;
            data.renderShadows = false;

            rendererIndex = PipelineUtilities.ValidateRenderer(rendererIndex);
            data.SetRenderer(rendererIndex);
            
#if RENDER_FEATURE
            cameraLeft.cullingMask = 0;
            data.SetRenderer(1);
            ReflectionRenderers[newCamera] = this;
#endif

            currentBuffer = RenderTexture.GetTemporary(Mathf.RoundToInt(source.scaledPixelWidth * GetRenderScale()),
                Mathf.RoundToInt(source.scaledPixelHeight * GetRenderScale()), 0,
                source.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);
            newCamera.targetTexture = currentBuffer;
            //newCamera.forceIntoRenderTexture = true;
            
            reflectionCameras[source] = newCamera;
        }

        private static readonly Plane[] frustrumPlanes = new Plane[6];
        
        public bool IsVisible(Camera camera)
        {
            GeometryUtility.CalculateFrustumPlanes(camera.projectionMatrix, frustrumPlanes);

            return GeometryUtility.TestPlanesAABB(frustrumPlanes, bounds);
        }

        /// <summary>
        /// Enables planar reflections on the MeshRenderers of the assigned water objects
        /// </summary>
        public void EnableMaterialReflectionSampling()
        {
            ToggleMaterialReflectionSampling(m_reflectionsEnabled);
        }
        
        private void ToggleMaterialReflectionSampling(bool state)
        {
            if (waterObjects == null) return;

            for (int i = 0; i < waterObjects.Count; i++)
            {
                if (waterObjects[i] == null) continue;
                
                waterObjects[i].props.SetFloat(_PlanarReflectionsEnabledID, state ? 1f : 0f);
                waterObjects[i].ApplyInstancedProperties();
            }
        }

        //Assigns the render target of the current reflection camera
        private void UpdateWaterProperties(Camera cam)
        {
            for (int i = 0; i < waterObjects.Count; i++)
            {
                if (waterObjects[i] == null) continue;
                
                waterObjects[i].props.SetTexture(_PlanarReflectionLeftID, cam.targetTexture);
                waterObjects[i].ApplyInstancedProperties();
            }
        }

        private static Vector4 reflectionPlane;
        private static Matrix4x4 reflectionBase;
        private static Vector3 oldCamPos;

        private static Matrix4x4 worldToCamera;
        private static Matrix4x4 viewMatrix;
        private static Matrix4x4 projectionMatrix;
        private static Vector4 clipPlane;
        private static readonly float[] layerCullDistances = new float[32];
        
        private void UpdatePerspective(Camera source, Camera reflectionCamera)
        {
            if (!source || !reflectionCamera) return;
            
            Vector3 position = bounds.center + (Vector3.up * offset);

            var d = -Vector3.Dot(Vector3.up, position);
            reflectionPlane = new Vector4(Vector3.up.x, Vector3.up.y, Vector3.up.z, d);

            reflectionBase = Matrix4x4.identity;
            reflectionBase *= Matrix4x4.Scale(new Vector3(1, -1, 1));

            // View
            CalculateReflectionMatrix(ref reflectionBase, reflectionPlane);
            oldCamPos = source.transform.position - new Vector3(0, position.y * 2, 0);
            reflectionCamera.transform.forward = Vector3.Scale(source.transform.forward, new Vector3(1, -1, 1));

            worldToCamera = source.worldToCameraMatrix;
            viewMatrix = worldToCamera * reflectionBase;

            //Reflect position
            oldCamPos.y = -oldCamPos.y;
            reflectionCamera.transform.position = oldCamPos;

            clipPlane = CameraSpacePlane(reflectionCamera.worldToCameraMatrix, position - Vector3.up * 0.1f,
                Vector3.up, 1.0f);
            projectionMatrix = source.CalculateObliqueMatrix(clipPlane);
            
            //Settings
#if !RENDER_FEATURE
            reflectionCamera.cullingMask = cullingMask;
#else
            //reflectionCamera.cullingMask = 0;
#endif

            //Only re-apply on value change
            if (m_renderRange != renderRange)
            {
                m_renderRange = renderRange;
                
                for (int i = 0; i < layerCullDistances.Length; i++)
                {
                    layerCullDistances[i] = renderRange;
                }
            }
            
            reflectionCamera.projectionMatrix = projectionMatrix;
            reflectionCamera.worldToCameraMatrix = viewMatrix;
            reflectionCamera.layerCullDistances = layerCullDistances;
            reflectionCamera.layerCullSpherical = true;
        }

        // Calculates reflection matrix around the given plane
        private void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
        {
            reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
            reflectionMat.m01 = (-2F * plane[0] * plane[1]);
            reflectionMat.m02 = (-2F * plane[0] * plane[2]);
            reflectionMat.m03 = (-2F * plane[3] * plane[0]);

            reflectionMat.m10 = (-2F * plane[1] * plane[0]);
            reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
            reflectionMat.m12 = (-2F * plane[1] * plane[2]);
            reflectionMat.m13 = (-2F * plane[3] * plane[1]);

            reflectionMat.m20 = (-2F * plane[2] * plane[0]);
            reflectionMat.m21 = (-2F * plane[2] * plane[1]);
            reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
            reflectionMat.m23 = (-2F * plane[3] * plane[2]);

            reflectionMat.m30 = 0F;
            reflectionMat.m31 = 0F;
            reflectionMat.m32 = 0F;
            reflectionMat.m33 = 1F;
        }
        
        // Given position/normal of the plane, calculates plane in camera space.
        private Vector4 CameraSpacePlane(Matrix4x4 worldToCameraMatrix, Vector3 pos, Vector3 normal, float sideSign)
        {
            var offsetPos = pos + normal * offset;
            var cameraPosition = worldToCameraMatrix.MultiplyPoint(offsetPos);
            var cameraNormal = worldToCameraMatrix.MultiplyVector(normal).normalized * sideSign;
            return new Vector4(cameraNormal.x, cameraNormal.y, cameraNormal.z,
                -Vector3.Dot(cameraPosition, cameraNormal));
        }
#endif
    }
}
