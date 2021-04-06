//Stylized Water 2
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

//Schlick's BRDF fresnel
float ReflectionFresnel(float3 worldNormal, float3 viewDir, float exponent)
{
	float cosTheta = saturate(dot(worldNormal, viewDir));
	return pow(max(0.0, 1.0 - cosTheta), exponent);
}

float3 SampleReflections(float3 reflectionVector, float smoothness, float4 screenPos, float3 normal, float3 viewDir, float2 pixelOffset)
{
	float3 probe = saturate(GlossyEnvironmentReflection(reflectionVector, smoothness, 1.0));

	#if !_RIVER //Planar reflections are pointless on curve surfaces, skip
	screenPos.xy += pixelOffset.xy;
	screenPos /= screenPos.w;

	float planarMask = 1-saturate(dot(viewDir, normal));
	planarMask = saturate(pow(planarMask, _PlanarReflectionsParams));
	
	float4 planarLeft = SAMPLE_TEX(_PlanarReflectionLeft, sampler_PlanarReflectionLeft, screenPos.xy);
	
	return lerp(probe, planarLeft.rgb, planarLeft.a * planarMask * _PlanarReflectionsEnabled);
	#else
	return probe;
	#endif
}

//Reusable for every light
struct TranslucencyData
{
	float3 transmissionColor;
	float waveHeight;
	float mask;
	float strength;

	float outputMask;
};

TranslucencyData PopulateTranslucencyData(float3 transmissionColor, float3 lightDir, float3 viewDir, float3 WorldNormal, float3 worldTangentNormal, float waveHeight, float mask, float4 params)
{
	TranslucencyData d = (TranslucencyData)0;
	//TODO: Modulate with light color
	d.transmissionColor = transmissionColor;
	d.waveHeight = waveHeight;
	d.mask = mask; //Shadows, foam, intersection, etc
	d.strength = params.x;

	float tMask = saturate(pow(saturate(dot(-viewDir, lightDir) - mask), params.y));
	float waveSlope = saturate(dot(WorldNormal, viewDir));
	float microWaveSlope = saturate(dot(worldTangentNormal, float3(0,1,0)));

	#if _ADVANCED_SHADING
	//Fade the effect out as the sun approaches the horizon (80 to 90 degrees)
	half sunAngle = saturate(dot(float3(0, 1, 0), lightDir));
	half angleMask = saturate(sunAngle * 10); /* 1.0/0.10 = 10 */
	tMask *= angleMask;
	#endif
	
	float transmission = tMask * waveHeight * waveSlope * microWaveSlope * params.x;

	d.outputMask = transmission;

	return d;
}

float3 ApplyTranslucency(in float3 baseColor, TranslucencyData data)
{
	//TODO: Modulate with light color?
	baseColor.rgb = lerp(baseColor.rgb, data.transmissionColor, data.outputMask);

	return baseColor.rgb;
}

//Based on UniversalFragmentBlinnPhong (no BRDF)
float4 ApplyLighting(SurfaceData surfaceData, InputData inputData, TranslucencyData translucencyData)
{
	float4 finalColor = 0;

#if _UNLIT
	return float4(surfaceData.albedo.rgb + surfaceData.emission.rgb, surfaceData.alpha);
#else

	Light mainLight = GetMainLight(inputData.shadowCoord);

	//Allow shadow strength to be overridden.
	mainLight.shadowAttenuation = saturate(mainLight.shadowAttenuation + (1.0 - _ShadowStrength));

	MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, half4(0, 0, 0, 0));

	half3 attenuatedLightColor = mainLight.color * (mainLight.distanceAttenuation * mainLight.shadowAttenuation);
	half3 diffuseColor = inputData.bakedGI + LightingLambert(attenuatedLightColor, mainLight.direction, inputData.normalWS);
	half3 specularColor = 0;

#ifdef _ADDITIONAL_LIGHTS //Per pixel lights
	uint pixelLightCount = GetAdditionalLightsCount();
	for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
	{
		Light light = GetAdditionalLight(lightIndex, inputData.positionWS);
		half3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
		diffuseColor += LightingLambert(attenuatedLightColor, light.direction, inputData.normalWS);

		#if _ADVANCED_SHADING && _TRANSLUCENCY
		//Direction not yet correct
		TranslucencyData lightTranslucencyData = PopulateTranslucencyData(translucencyData.transmissionColor, light.direction, inputData.viewDirectionWS, inputData.normalWS, inputData.normalWS, translucencyData.waveHeight, translucencyData.mask , translucencyData.strength * 16 * (light.distanceAttenuation));
		diffuseColor = ApplyTranslucency(diffuseColor, lightTranslucencyData);
		#endif

		//Fast blinn-phong specular (smoothness derived from sun reflection size)
		specularColor += LightingSpecular(attenuatedLightColor, light.direction, inputData.normalWS, inputData.viewDirectionWS, half4(light.color.rgb, 0), surfaceData.smoothness * 0.1);
	}
#endif

#ifdef _ADDITIONAL_LIGHTS_VERTEX //Previous calculated in vertex stage
	diffuseColor += inputData.vertexLighting;
#endif

	//Emission holds sun specular
	finalColor.rgb = diffuseColor * surfaceData.albedo + surfaceData.emission + specularColor;
	finalColor.a = surfaceData.alpha;
#endif

	return finalColor;
}

//Force retrieve shadows, even when shadows are disabled for the material
float GetShadows(float3 wPos)
{
	float4 shadowCoord = TransformWorldToShadowCoord(wPos.xyz);

	ShadowSamplingData shadowSamplingData = GetMainLightShadowSamplingData();
	half4 shadowParams = GetMainLightShadowParams();
	return SampleShadowmap(TEXTURE2D_ARGS(_MainLightShadowmapTexture, sampler_MainLightShadowmapTexture), shadowCoord, shadowSamplingData, shadowParams, false);
}