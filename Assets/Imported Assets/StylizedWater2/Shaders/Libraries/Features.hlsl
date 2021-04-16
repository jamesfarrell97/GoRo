//Stylized Water 2
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

TEXTURE2D(_WaterProjectorDiffuse);
SAMPLER(sampler_WaterProjectorDiffuse);
float4 _WaterProjectorUV;

TEXTURE2D(_FoamTex);
SAMPLER(sampler_FoamTex);
TEXTURE2D(_BumpMapLarge);
SAMPLER(sampler_BumpMapLarge);

float3 SampleNormals(float2 uv, float3 wPos, float2 time, float2 flowmap, float speed, float slope) 
{
	float4 uvs = PackedUV(uv, time, flowmap, speed);
	float3 n1 = UnpackNormal(SAMPLE_TEX(_BumpMap, sampler_BumpMap, uvs.xy));
	float3 n2 = UnpackNormal(SAMPLE_TEX(_BumpMap, sampler_BumpMap, uvs.zw));

	float3 blendedNormals = BlendNormal(n1, n2);

#if _DISTANCE_NORMALS
	float pixelDist = length(_WorldSpaceCameraPos.xzz - wPos.xyz);
	float fadeFactor = saturate((_DistanceNormalParams.y - pixelDist) / (_DistanceNormalParams.y-_DistanceNormalParams.x));

	float3 largeBlendedNormals = 0;
	
	uvs = PackedUV(uv * _DistanceNormalParams.z, time, flowmap, speed * 0.5);
	float3 n1b = UnpackNormal(SAMPLE_TEX(_BumpMapLarge, sampler_BumpMapLarge, uvs.xy));
	#if _ADVANCED_SHADING //Use 2nd texture sample
	float3 n2b = UnpackNormal(SAMPLE_TEX(_BumpMapLarge, sampler_BumpMapLarge, uvs.zw));

	largeBlendedNormals = BlendNormal(n1b, n2b);
	#else
	largeBlendedNormals = n1b;
	#endif
	blendedNormals = lerp(largeBlendedNormals, blendedNormals, fadeFactor);
#endif
	
#if _RIVER
	uvs = PackedUV(uv, time, flowmap, speed * _SlopeParams.y);
	uvs.xy = uvs.xy * float2(1, 1-_SlopeParams.x);
	float3 n3 = UnpackNormal(SAMPLE_TEX(_BumpMap, sampler_BumpMap, uvs.xy));

	blendedNormals = lerp(n3, blendedNormals, slope);
#endif
	
	return blendedNormals;
}

float SampleIntersection(float2 uv, float gradient, float2 time)
{
	float inter = 0;

#if _SHARP_INERSECTION
	float sine = sin(time.y * 10 - (gradient * _IntersectionRippleDist)) * _IntersectionRippleStrength;
	float2 nUV = float2(uv.x, uv.y) * _IntersectionTiling;
	float noise = SAMPLE_TEX(_IntersectionNoise, sampler_IntersectionNoise, nUV + time.xy).r;

	float dist = saturate(gradient / _IntersectionFalloff);
	noise = saturate((noise + sine) * dist + dist);
	inter = step(_IntersectionClipping, noise);
#endif

#if _SMOOTH_INTERSECTION
	float noise1 = SAMPLE_TEX(_IntersectionNoise, sampler_IntersectionNoise, (float2(uv.x, uv.y) * _IntersectionTiling) + (time.xy )).r;
	float noise2 = SAMPLE_TEX(_IntersectionNoise, sampler_IntersectionNoise, (float2(uv.x, uv.y) * (_IntersectionTiling * 1.5)) - (time.xy )).r;

	float dist = saturate(gradient / _IntersectionFalloff);
	inter = saturate(noise1 + noise2 + dist) * dist;
#endif

	return saturate(inter);
}

float SampleFoam(float2 uv, float2 time, float2 flowmap, float clipping, float mask, float slope)
{
#if _FOAM
	float4 uvs = PackedUV(uv, time, flowmap, _FoamSpeed);
	float f1 = SAMPLE_TEX(_FoamTex, sampler_FoamTex, uvs.xy).r;
	float f2 = SAMPLE_TEX(_FoamTex, sampler_FoamTex, uvs.zw).r;

	float foam = saturate(f1 + f2) * mask;

	#if _RIVER
	uvs = PackedUV(uv, time, flowmap, _FoamSpeed * _SlopeParams.y);
	//Stretch UV vertically on slope
	uvs = uvs * float4(1, 1-_SlopeParams.x, 1, 1-_SlopeParams.x);

	//Cannot reuse the same UV, slope foam needs to be resampled and blended in
	float f3 = SAMPLE_TEX(_FoamTex, sampler_FoamTex, uvs.xy).r;
	float f4 = SAMPLE_TEX(_FoamTex, sampler_FoamTex, uvs.xy).r;

	foam = saturate(lerp(f3 + f4, f1 + f2, slope)) * mask;
	#endif
	
	foam = smoothstep(clipping, 1, foam);
	return foam;
#else
	return 0;
#endif
}

//Specular reflection in world-space
float4 SunSpecular(Light light, float3 viewDir, float3 normalWS, float perturbation, float size, float intensity)
{
	//return LightingSpecular(1, light.direction, normalWS, viewDir, 1, lerp(8196, 64, size));
	
	float3 viewLightTerm = normalize(light.direction + (normalWS * perturbation) + viewDir);
	
	float NdotL = saturate(dot(viewLightTerm, float3(0, 1, 0)));

	half specSize = lerp(8196, 64, size);
	float specular = (pow(NdotL, specSize));
	//Mask by shadows if available
	specular *= (light.distanceAttenuation * light.shadowAttenuation);

	float3 specColor = specular * light.color * intensity;

	return float4(specColor, specSize);
}

float2 BoundsToWorldUV(in float3 wPos, in float4 b)
{
	float2 uv = b.xy / b.z + (b.z / (b.z * b.z)) * wPos.xz;

	//TODO: Check if required per URP version
	uv.y = 1 - uv.y;

	return uv;
}

float BoundsEdgeMask(float2 position)
{
	float2 xz = abs(position * 14.0) - 6.0;
	float pos = length(max(xz, 0));
	float neg = min(max(xz.x, xz.y), 0);
	return  1-saturate(pos + neg);
}

//#define RECEIVE_PROJECTORS

float4 SampleDiffuseProjectors(float3 wPos)
{
#ifdef RECEIVE_PROJECTORS
	float2 uv = BoundsToWorldUV(wPos, _WaterProjectorUV);

	float4 sample = SAMPLE_TEX(_WaterProjectorDiffuse, sampler_WaterProjectorDiffuse, uv);
	sample.a *= BoundsEdgeMask(uv - 0.5);
	
	return sample;
#else
	return 0;
#endif
}