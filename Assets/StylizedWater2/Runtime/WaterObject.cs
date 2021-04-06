using System.Collections.Generic;
using UnityEngine;

namespace StylizedWater2
{
    /// <summary>
    /// Attached to every mesh using the Stylized Water 2 shader
    /// Provides a generic way of identifying water objects and accessing their properties
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("Stylized Water 2/Water Object")]
    public class WaterObject : MonoBehaviour
    {
        public static readonly List<WaterObject> Instances = new List<WaterObject>();
        
        public Material material;
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;
        
        private MaterialPropertyBlock _props;
        public MaterialPropertyBlock props
        {
            get
            {
                //Fetch when required, execution order makes it unreliable otherwise
                if (_props == null)
                {
                    _props = new MaterialPropertyBlock();
                    meshRenderer.GetPropertyBlock(_props);
                }
                return _props;
            }
        }
        
        private void OnEnable()
        {
            Instances.Add(this);
        }

        private void OnDisable()
        {
            Instances.Remove(this);
        }

        private void OnValidate()
        {
            if (!meshRenderer) meshRenderer = GetComponent<MeshRenderer>();
            if (!meshFilter) meshFilter = GetComponent<MeshFilter>();
            if (meshRenderer) material = meshRenderer.sharedMaterial;
        }

        public void ApplyInstancedProperties()
        {
            if(props != null) meshRenderer.SetPropertyBlock(props);
        }

        /// <summary>
        /// Checks if the position is below the maximum possible wave height. Can be used as a fast broad-phase check, before actually using the more expensive SampleWaves function
        /// </summary>
        /// <param name="position"></param>
        public bool CanTouch(Vector3 position)
        {
            return Buoyancy.CanTouchWater(position, this);
        }

        public void AssignMesh(Mesh mesh)
        {
            if (meshFilter) meshFilter.sharedMesh = mesh;
        }

        public void AssignMaterial(Material material)
        {
            if (meshRenderer) meshRenderer.sharedMaterial = material;
        }

        /// <summary>
        /// Creates a new GameObject with a MeshFilter, MeshRenderer and WaterObject component
        /// </summary>
        /// <param name="waterMaterial">If assigned, this material is automatically added to the MeshRenderer</param>
        /// <returns></returns>
        public static WaterObject New(Material waterMaterial = null, Mesh mesh = null)
        {
            GameObject go = new GameObject("Water Object", typeof(MeshFilter), typeof(MeshRenderer), typeof(WaterObject));
            WaterObject waterObject = go.GetComponent<WaterObject>();
            
            waterObject.meshRenderer = waterObject.gameObject.GetComponent<MeshRenderer>();
            waterObject.meshFilter = waterObject.gameObject.GetComponent<MeshFilter>();
            
            waterObject.meshFilter.sharedMesh = mesh;
            waterObject.meshRenderer.sharedMaterial = waterMaterial;
            waterObject.material = waterMaterial;

            return waterObject;
        }
    }
}