float4 _WaterShallowColor;
float4 _WaterDeepColor;

float _HorizontalDensity;
float _VerticalDensity;
float _StartDistance;
float _WaterLevel;
float _MaxDepth;

float _ClipOffset;
#define CLIP_DISTANCE _ClipOffset

#if !SHADERGRAPH_PREVIEW
TEXTURE2D_X(_UnderwaterMask);
SAMPLER(sampler_UnderwaterMask);
#endif

float ComputeDistance(float3 wPos, float multiplier)
{
	//Radial distance
	float dist = length(_WorldSpaceCameraPos.xyz - wPos);

	//Start distance
	dist -= _ProjectionParams.y + _StartDistance;
	dist *= _HorizontalDensity * multiplier;
	
	float density = 1-(exp2(-dist));
	
	return saturate(density);
}

#define HEIGHT _WaterHeight
#define HEIGHT_DENSITY _VerticalDensity

float ComputeHeight(float3 wpos, float waterLevel)
{
	float3 wsDir = _WorldSpaceCameraPos.xyz - wpos;
	float FH = _WaterLevel; //Height
	float3 C = _WorldSpaceCameraPos;
	float3 V = wsDir;
	float3 P = wpos;
	float FdotC = _WorldSpaceCameraPos.y - _WaterLevel; //Camera/fog plane height difference
	float k = (FdotC <= 0.0f ? 1.0f : 0.0f); //Is camera below height fog
	float FdotP = P.y - FH;
	float FdotV = wsDir.y;
	float c1 = k * (FdotP + FdotC);
	float c2 = (1 - 2 * k) * FdotP;
	float g = min(c2, 0.0);
	g = -HEIGHT_DENSITY * (c1 - g * g / abs(FdotV + 1.0e-5f));
	return 1-exp(-g);
}

void ComputeDensity(float3 wPos, float heightLevel, float distanceMultiplier, out float distance, out float height)
{
	distance = ComputeDistance(wPos, distanceMultiplier) ;
	height = saturate( ComputeHeight(wPos, heightLevel));
}

void GetWaterDensity_float(float4 wPos, out float density)
{
	density = saturate(ComputeDistance(wPos.xyz, 1.0) + ComputeHeight(wPos.xyz, _WaterLevel));
}

float3 SampleCaustics(float3 depthPos, float2 time, float tiling)
{
	//Sun projection coords
	//float4 lightSpaceUVs = mul(_MainLightWorldToShadow[0], float4(DepthPos.xyz, 1)) ;
	
	//Planar depth projection
	float3 caustics1 = SAMPLE_TEX(_CausticsTex, sampler_LinearRepeat, depthPos.xz* tiling + (time.xy )).rgb;
	float3 caustics2 = SAMPLE_TEX(_CausticsTex, sampler_LinearRepeat, (depthPos.xz* tiling * 0.8) -(time.xy)).rgb;

	float3 caustics = min(caustics1, caustics2);

	return caustics;
}

void ApplyUnderwaterShading(inout float3 color, inout float alpha, float3 worldPos, float3 normal, float3 viewDir, float3 shallowColor, float3 deepColor, float mask)
{
	float fogDistFactor = 0;
	float fogHeightFactor = 0;
	ComputeDensity(worldPos, _WaterLevel, 4, fogDistFactor, fogHeightFactor);
	float waterDensity = saturate((fogDistFactor + fogHeightFactor));
	
	//Vertical
	float3 waterColor = lerp(shallowColor.rgb, deepColor.rgb, fogHeightFactor * mask);
	//Horizontal
	waterColor = lerp(color, deepColor.rgb, fogDistFactor * mask);

	//TODO: Bring alpha to full when viewing on the water level
	float fresnel = 1-pow(saturate(dot(float3(0,-1,0), unity_CameraToWorld._13_23_33)), 1);

	fresnel = saturate((worldPos.y - _WorldSpaceCameraPos.y) / 2.0);
	//waterColor = lerp(deepColor.rgb, color.rgb, saturate(fresnel * 1-mask));
    alpha = lerp(alpha, fogDistFactor, mask * 1.0);
	color = waterColor;

	//color = fresnel;

}

float3 SampleUnderwaterReflections(float3 reflectionVector, float smoothness, float3 normal, float3 viewDir, float2 pixelOffset)
{
	//Mirror since the normal is that of the top surface
	reflectionVector.y = -reflectionVector.y;
	float3 probe = saturate(GlossyEnvironmentReflection(reflectionVector, 0.05, 1.0));

	float factor = saturate(pow(reflectionVector.y, 1));
	float3 reflection = lerp(_WaterDeepColor.rgb, probe, 1);

	return reflection;
}

#define CAM_UP unity_CameraToWorld._12_22_32
#define CAMERA_FORWARD unity_CameraToWorld._13_23_33
#define CAM_FOV unity_CameraInvProjection._m11

//Clip the water using a fake near-clipping plane.
void ClipSurface(float4 screenPos, float clipZ, float vFace)
{
	#if UNDERWATER_ENABLED //Build error otherwise due to SAMPLE_TEX
	float clipDepth = saturate((LINEAR_EYE_DEPTH(clipZ )) / CLIP_DISTANCE+0.3);
	float underwaterMask = SAMPLE_TEX(_UnderwaterMask, sampler_UnderwaterMask, (screenPos.xy / screenPos.w)).r;

	float mask = floor(clipDepth);
	//Clip space depth is not enough since vertex density is likely lower than the underwater mask
	mask = lerp(underwaterMask, 1-underwaterMask, vFace);

	clip(mask - 0.5);
	#endif
}
