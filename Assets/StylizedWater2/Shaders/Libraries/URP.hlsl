//Stylized Water 2
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

#ifndef _DISABLE_DEPTH_TEX
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#endif

#ifdef _REFRACTION
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
#endif


SamplerState sampler_LinearRepeat;
#define Repeat sampler_LinearRepeat
SamplerState sampler_LinearClamp;
#define Clamp sampler_LinearClamp

#ifndef _DISABLE_DEPTH_TEX
#define SAMPLE_DEPTH(uv) SampleSceneDepth(uv)
#define SAMPLE_DEPTH_LOD(uv) SAMPLE_TEXTURE2D_X_LOD(_CameraDepthTexture, sampler_CameraDepthTexture, UnityStereoTransformScreenSpaceTex(uv), 0).r;
#define LINEAR_DEPTH(depth) Linear01Depth(depth, _ZBufferParams)
#define LINEAR_EYE_DEPTH(depth) LinearEyeDepth(depth, _ZBufferParams)
#endif

#ifdef _REFRACTION
#define SAMPLE_SCENE_COLOR(uv) SampleSceneColor(uv)
#endif

#define SAMPLE_TEX(textureName, sampler, uv) SAMPLE_TEXTURE2D(textureName, sampler, uv)
#define SAMPLE_TEX_LOD(textureName, sampler, uv, mip) SAMPLE_TEXTURE2D_LOD(textureName, sampler, uv, mip)
 
// Deprecated in URP 11+ https://github.com/Unity-Technologies/Graphics/pull/2529. Keep function for backwards compatibility
// Compute Normalized Device Coordinate here (this is normally done in GetVertexPositionInputs, but clip and world-space coords are done manually already)
#if VERSION_GREATER_EQUAL(11,0) && !defined(UNITY_SHADER_VARIABLES_FUNCTIONS_DEPRECATED_INCLUDED)
float4 ComputeScreenPos(float4 positionCS)
{
    return ComputeNormalizedDeviceCoordinates(positionCS);
}
#endif