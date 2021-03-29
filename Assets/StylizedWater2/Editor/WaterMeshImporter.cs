//Stylized Water 2
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif
using UnityEngine;

namespace StylizedWater2
{
    [ScriptedImporter(1, "watermesh")]
    public class WaterMeshImporter : ScriptedImporter
    {
        [SerializeField] public WaterMesh plane;
        [HideInInspector] private Mesh mesh;

        public override void OnImportAsset(AssetImportContext context)
        {
            if (plane == null)
            {
                plane = new WaterMesh();
                mesh = plane.Rebuild();
            }
            else
            {
                mesh = plane.Rebuild();
            }

            context.AddObjectToAsset("mesh", mesh);
            context.SetMainObject(mesh);
        }
    }
}
