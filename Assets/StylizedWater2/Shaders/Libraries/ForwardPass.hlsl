//Stylized Water 2
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

//#define RESAMPLE_REFRACTION_DEPTH

//Note: Throws an error about a BLENDWEIGHTS vertex attribute on GLES when VR is enabled (fixed in URP 10+)
//Possibly related to: https://issuetracker.unity3d.com/issues/oculus-a-non-system-generated-input-signature-parameter-blendindices-cannot-appear-after-a-system-generated-value
#if UNDERWATER_ENABLED
half4 ForwardPassFragment(Varyings input, FRONT_FACE_TYPE vFace : FRONT_FACE_SEMANTIC) : SV_Target
#else
half4 ForwardPassFragment(Varyings input) : SV_Target
#endif
{
	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

	float3 finalColor = 0;
	float alpha = 1;

	float facing = 1.0;
	//0 = back face
	#if UNDERWATER_ENABLED
	facing = IS_FRONT_VFACE(vFace, true, false);
	//finalColor = lerp(float3(1,0,0), float3(0,1,0), IS_FRONT_VFACE(vFace, true, false));
	//return float4(finalColor.rgb, 1);
	#endif
	
	float4 vertexColor = input.color; //Mask already applied in vertex shader
	//return float4(vertexColor.bbb, 1);

	//Vertex normal in world-space
	float3 normalWS = normalize(input.normal.xyz);
#if _NORMALMAP
	float3 WorldTangent = input.tangent.xyz;
	float3 WorldBiTangent = input.bitangent.xyz;
	float3 wPos = float3(input.normal.w, input.tangent.w, input.bitangent.w);
#else
	float3 wPos = input.wPos;
#endif
	//Not normalized for depth-pos reconstruction. Normalization required for lighting (otherwise breaks on mobile)
	float3 viewDir = (_WorldSpaceCameraPos - wPos);
	float3 viewDirNorm = SafeNormalize(viewDir);
	//return float4(viewDir, 1);
	
	#if _FLAT_SHADING
	float3 dpdx = ddx(wPos.xyz);
	float3 dpdy = ddy(wPos.xyz);
	normalWS = normalize(cross(dpdy, dpdx));
	#endif

	//Returns mesh or world-space UV
	float2 uv = GetSourceUV(input.uv.xy, wPos.xz, _WorldSpaceUV);
	float2 flowMap = float2(1, 1);

	half slope = 0;
	#if _RIVER
	slope = GetSlopeInverse(normalWS);
	//return float4(slope, slope, slope, 1);
	#endif

	// Waves
	float height = 0;

	float3 waveNormal = normalWS;
#if _WAVES
	WaveInfo waves = GetWaveInfo(uv, TIME * _WaveSpeed, _WaveFadeDistance.x, _WaveFadeDistance.y);
	#if !_FLAT_SHADING
		//Flatten by blue vertex color weight
		waves.normal = lerp(waves.normal, normalWS, lerp(0, 1, vertexColor.b));
		//Blend wave/vertex normals in world-space
		waveNormal = BlendNormalWorldspaceRNM(waves.normal, normalWS, UP_VECTOR);
	#endif
	//return float4(waveNormal.xyz, 1);
	height = waves.position.y * 0.5 + 0.5;
	height *= lerp(1, 0, vertexColor.b);
	//return float4(height, height, height, 1);

	//vertices are already displaced on XZ, in this case the world-space UV needs the same treatment
	if(_WorldSpaceUV == 1) uv.xy -= waves.position.xz * HORIZONTAL_DISPLACEMENT_SCALAR * _WaveHeight;
	//return float4(frac(uv.xy), 0, 1);
#endif


	float4 ShadowCoords = float4(0, 0, 0, 0);
	#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && !defined(UNLIT)
	ShadowCoords = input.shadowCoord;
	#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS) && !defined(UNLIT)
	ShadowCoords = TransformWorldToShadowCoord(wPos);
	#endif

	Light mainLight = GetMainLight(ShadowCoords);

	half shadowMask = 1;
	#if _ADVANCED_SHADING
	shadowMask = GetShadows(wPos.xyz);
	//return float4(shadowMask,shadowMask,shadowMask,1);
	#endif

	//Normals
	float3 NormalsCombined = float3(0.5, 0.5, 1);
	float3 worldTangentNormal = waveNormal;
	
#if _NORMALMAP
	NormalsCombined = SampleNormals(uv * _NormalTiling, wPos, TIME, flowMap, _NormalSpeed, slope);
	//return float4((NormalsCombined.x * 0.5 + 0.5), (NormalsCombined.y * 0.5 + 0.5), 1, 1);

	worldTangentNormal = normalize(TransformTangentToWorld(NormalsCombined, half3x3(WorldTangent, WorldBiTangent, waveNormal)));
#endif

#ifdef SCREEN_POS
	float4 ScreenPos = input.screenPos;
#else
	float4 ScreenPos = 0;
#endif
	
	//return float4(depth.linear01, depth.linear01, depth.linear01, 1);

	#if _REFRACTION
	float4 refractedScreenPos = ScreenPos.xyzw + (float4(worldTangentNormal.xy, 0, 0) * (_RefractionStrength * 0.1));
	#endif

	float3 opaqueWorldPos = wPos;
	float opaqueDist = 1;
	float aborptionDist = opaqueDist;
	
#if !_DISABLE_DEPTH_TEX
	DepthData depth = SampleDepth(ScreenPos, wPos);
	opaqueWorldPos = ReconstructViewPos(ScreenPos, viewDir, depth);
	//return float4(frac(opaqueWorldPos.xyz), 1);

	//Invert normal when viewing backfaces
	float normalSign = ceil(dot(viewDirNorm, normalWS));
	normalSign = normalSign == 0 ? -1 : 1;
	
	opaqueDist = DepthDistance(wPos, opaqueWorldPos, normalWS * normalSign);
	//return float4(opaqueDist,opaqueDist,opaqueDist,1);
	
#if _ADVANCED_SHADING && _REFRACTION
	DepthData depthRefracted = SampleDepth(refractedScreenPos, wPos);
	float3 opaqueWorldPosRefracted = ReconstructViewPos(refractedScreenPos, viewDir, depthRefracted);

	//Reject any offset pixels above water
	float refractionMask = saturate((wPos - opaqueWorldPosRefracted).y);
	//return float4(refractionMask.xxx, 1.0);
	refractedScreenPos = lerp(ScreenPos, refractedScreenPos, refractionMask);
	
	//Double sample depth to avoid depth discrepancies (though this doesn't always offer the best result)
	#ifdef RESAMPLE_REFRACTION_DEPTH
	aborptionDist = DepthDistance(wPos, opaqueWorldPosRefracted, normalWS);
	#else
	aborptionDist = opaqueDist;
	#endif
	
#else
	aborptionDist = opaqueDist;
#endif
#endif

	float AbsorptionDepth = 1;
#if !_DISABLE_DEPTH_TEX
	AbsorptionDepth = saturate(lerp(aborptionDist / _Depth, 1-(exp(-aborptionDist) / _Depth), _DepthExp));
#endif
	//return float4(AbsorptionDepth,AbsorptionDepth,AbsorptionDepth,1);
	
	float intersection = 0;
#if _SHARP_INERSECTION || _SMOOTH_INTERSECTION
	float interSecGradient = 1-saturate(exp(opaqueDist) / _IntersectionLength);

	#if _DISABLE_DEPTH_TEX
	interSecGradient = 0;
	#endif
	
	if (_IntersectionSource == 1) interSecGradient = vertexColor.r;
	if (_IntersectionSource == 2) interSecGradient = saturate(interSecGradient + vertexColor.r);

	intersection = SampleIntersection(uv.xy, interSecGradient, TIME * _IntersectionSpeed);
	intersection *= _IntersectionColor.a;

	#if UNDERWATER_ENABLED
	intersection *= facing;
	#endif

	#if _WAVES
	//Prevent from peering through waves when camera is at the water level
	if(wPos.y < opaqueWorldPos.y) intersection = 0;
	#endif
	
	//Flatten normals on intersection foam
	waveNormal = lerp(waveNormal, normalWS, intersection);
#endif
	//return float4(intersection,intersection,intersection,1);

	float shoreSine = 0;
	/* Disabled, pretty much requires static depth info
	//Shoreline wave normals
	if (_DepthMode == 1) _ShoreLineLength = clamp(_ShoreLineLength, 0, 1);
	half shoreWaveDist = saturate(shoreDist / _ShoreLineLength);
	//shoreWaveDist = shoreWaveDist > 0.5 ? 0 : shoreWaveDist;
	//return float4(shoreWaveDist, shoreWaveDist, shoreWaveDist, 1);

	shoreSine = (sin((shoreWaveDist * _ShoreLineWaveDistance * _ShoreLineLength) + (TIME * _WaveSpeed * 0.25)) * 0.5 + 0.5) * shoreWaveDist;
	shoreSine *= _ShoreLineWaveStr * (1 - shoreWaveDist);
	height = saturate(height + shoreSine);
	//return float4(shoreSine, shoreSine, shoreSine, 1);
	*/
	
	//return float4(height, height, height, 1);

	//FOAM
	float foam = 0;
	#if _FOAM

	#if !_RIVER
	float foamWaveMask = lerp(1, saturate(height), _FoamWaveMask);
	foamWaveMask = pow(abs(foamWaveMask), _FoamWaveMaskExp);
	#else
	float foamWaveMask = 1;
	#endif
	
	foam = SampleFoam(uv * _FoamTiling, TIME, flowMap, _FoamSize, foamWaveMask, slope);
	
	#if _RIVER
	foam *= saturate(_FoamColor.a + 1-slope + vertexColor.b);
	#else
	foam *= saturate(_FoamColor.a);
	#endif	

	#if UNDERWATER_ENABLED
	foam *= facing;
	#endif
	
	//return float4(foam, foam, foam, 1);
	#endif

	//Albedo
	float4 baseColor = lerp(_ShallowColor, _BaseColor, AbsorptionDepth);
	baseColor.rgb += _WaveTint * saturate(height + (shoreSine * 1.0));
	
	finalColor.rgb = baseColor.rgb;
	alpha = baseColor.a;

	float3 sparkles = 0;
	#if _NORMALMAP
	float NdotL = saturate(dot(UP_VECTOR, worldTangentNormal));
	half sunAngle = saturate(dot(UP_VECTOR, mainLight.direction));
	half angleMask = saturate(sunAngle * 10); /* 1.0/0.10 = 10 */
	sparkles = saturate(step(_SparkleSize, (saturate(NormalsCombined.y) * NdotL))) * _SparkleIntensity * mainLight.color * angleMask;
	
	finalColor.rgb += sparkles.rgb;
	#endif
	//return float4(baseColor.rgb, alpha);

	half4 sunSpec = 0;
#ifndef _SPECULARHIGHLIGHTS_OFF
	float3 sunReflectionNormals = worldTangentNormal;

	#if _FLAT_SHADING //Use face normals
	sunReflectionNormals = waveNormal;
	#endif
	
	//Blinn-phong reflection
	sunSpec = SunSpecular(mainLight, viewDirNorm, sunReflectionNormals, _SunReflectionDistortion, _SunReflectionSize, _SunReflectionStrength);
	sunSpec.rgb *=  saturate((1-foam) * (1-intersection) * shadowMask); //Hide
#endif

	//Reflection probe
#ifndef _ENVIRONMENTREFLECTIONS_OFF
	float3 refWorldTangentNormal = lerp(waveNormal, normalize(waveNormal + worldTangentNormal), _ReflectionDistortion);

	#if _FLAT_SHADING //Skip, not a good fit
	refWorldTangentNormal = waveNormal;
	#endif
	
	float3 reflectionVector = reflect(-viewDirNorm , refWorldTangentNormal);
	float2 reflectionPerturbation = lerp(waveNormal.xz * 0.5, worldTangentNormal.xy, _ReflectionDistortion).xy;
	float3 reflections = SampleReflections(reflectionVector, _ReflectionBlur, ScreenPos.xyzw, refWorldTangentNormal, viewDirNorm, reflectionPerturbation);
	
	half reflectionFresnel = ReflectionFresnel(refWorldTangentNormal, viewDirNorm, _ReflectionFresnel);
	//return float4(reflectionFresnel.xxx, 1);
	finalColor.rgb = lerp(finalColor.rgb, reflections, _ReflectionStrength * reflectionFresnel);
	
	#if UNDERWATER_ENABLED
	float3 underwaterReflections = SampleUnderwaterReflections(reflectionVector, 0.0, normalize(waveNormal + 0), viewDirNorm, 0.0);
	//Underwater surface practically has no color, except for applied water density further down
	finalColor.rgb = lerp(underwaterReflections, finalColor.rgb, facing);
	#endif
	
	//return float4(finalColor.rgb, 1);
#endif

#if _CAUSTICS
	float3 caustics = SampleCaustics(opaqueWorldPos, TIME * _CausticsSpeed, _CausticsTiling) * _CausticsBrightness;
	#if _ADVANCED_SHADING
	caustics *= GetShadows(opaqueWorldPos);
	#endif
	//return float4(caustics, caustics, caustics, 1);

	float causticsMask = AbsorptionDepth;
	causticsMask = saturate(causticsMask + intersection);

	#if _RIVER
	//Reduce caustics visibility by supposed water turbulence
	causticsMask = lerp(1, causticsMask, slope);
	#endif
	finalColor = lerp(finalColor + caustics, finalColor, causticsMask );
#endif

	///////////////////////
	// Translucency
	//////////////////////
	TranslucencyData translucencyData = (TranslucencyData)0;
	#if _TRANSLUCENCY
	float waveHeight = saturate(height);
	#if !_WAVES || _FLAT_SHADING
	waveHeight = 1;
	#endif
	//Note value is subtracted
	float transmissionMask = (foam * 0.25) + (1-shadowMask); //Foam isn't 100% opaque

	//return float4(transmissionMask, transmissionMask, transmissionMask, 1);

	float3 sunDir = mainLight.direction;
	sunDir.x -= 0.05; //Fake a lower angle for effect to visible more often
	translucencyData = PopulateTranslucencyData(_ShallowColor.rgb * 20, sunDir, viewDirNorm, waveNormal, worldTangentNormal, waveHeight, transmissionMask, _TranslucencyParams);
	
	finalColor.rgb = ApplyTranslucency(finalColor.rgb, translucencyData);
	#endif

	//Foam application on top of everything up to this point
	#if _FOAM
	finalColor.rgb = lerp(finalColor.rgb, _FoamColor.rgb, foam);
	#endif

	#if _SHARP_INERSECTION || _SMOOTH_INTERSECTION
	//Layer intersection on top of everything
	finalColor.rgb = lerp(finalColor.rgb, _IntersectionColor.rgb, intersection);
	#endif

	//Full alpha on intersection and foam
	alpha = saturate(alpha + intersection + foam);

	#if _FLAT_SHADING //Skip, not a good fit
	worldTangentNormal = waveNormal;
	#else
	//At this point, normal strength should affect lighting
	half normalMask = saturate((intersection + foam + (shoreSine * 2.0)));
	worldTangentNormal = lerp(waveNormal, worldTangentNormal, saturate(_NormalStrength - normalMask));
	#endif
	
	//return float4(normalMask, normalMask, normalMask, 1);

	//Horizon color (note: not using normals, since they are perturbed by waves)
	half VdotN = 1.0 - saturate(dot(viewDirNorm, normalWS));
	float fresnel = saturate(pow(VdotN, _HorizonDistance));
	#if UNDERWATER_ENABLED
	fresnel *= facing;
	#endif
	finalColor.rgb = lerp(finalColor.rgb, _HorizonColor.rgb, fresnel * _HorizonColor.a);

	//Final alpha
	float edgeFade = saturate(opaqueDist / (_EdgeFade * 0.01));

	#if UNDERWATER_ENABLED
	edgeFade = lerp(1.0, edgeFade, facing);
	#endif

	#if _WAVES
	//Prevent from peering through waves when camera is at the water level
	if(wPos.y <= opaqueWorldPos.y) edgeFade = 1;
	#endif

	alpha *= edgeFade;

	//Not yet implemented
	float4 diffuseProjector = SampleDiffuseProjectors(wPos);
	finalColor.rgb = lerp(finalColor.rgb, diffuseProjector.rgb, diffuseProjector.a);
	
	SurfaceData surf = (SurfaceData)0;

	surf.albedo = finalColor.rgb;
	surf.specular = 0;
	surf.metallic = 0;
	surf.smoothness = sunSpec.a;
	surf.normalTS = NormalsCombined;
	surf.emission = sunSpec.rgb;
	surf.occlusion = 1;
	surf.alpha = alpha;

	InputData inputData;
	inputData.positionWS = wPos;
	inputData.viewDirectionWS = viewDirNorm;
	inputData.shadowCoord = ShadowCoords;
	inputData.normalWS = worldTangentNormal;
	inputData.fogCoord = input.fogFactorAndVertexLight.x;

	inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
	inputData.bakedGI = SAMPLE_GI(input.lightmapUVOrVertexSH.xy, input.lightmapUVOrVertexSH.xyz, inputData.normalWS);

	float4 color = ApplyLighting(surf, inputData, translucencyData);
	
	#if UNDERWATER_ENABLED
	ApplyUnderwaterShading(color.rgb, alpha, wPos, worldTangentNormal, viewDirNorm, _ShallowColor.rgb, _BaseColor.rgb, 1-facing);
	#endif
	
	#if _REFRACTION
		float3 refraction = SAMPLE_SCENE_COLOR(refractedScreenPos.xy / refractedScreenPos.w).rgb;
		//Chromatic
#if _ADVANCED_SHADING
		float red = SAMPLE_SCENE_COLOR(refractedScreenPos.xy / refractedScreenPos.w + float2((_ScreenParams.z - 1.0), 0)).r;
		float blue = SAMPLE_SCENE_COLOR(refractedScreenPos.xy / refractedScreenPos.w - float2((_ScreenParams.z - 1.0), 0)).b;
		refraction.r = red;
		refraction.b = blue;
#endif
		color.rgb = lerp(refraction, color.rgb, alpha);
		alpha = edgeFade;
	#endif

	color.a = alpha * saturate(alpha - vertexColor.g);
	ApplyFog(color.rgb, input.fogFactorAndVertexLight.x, ScreenPos, wPos);

	#if UNDERWATER_ENABLED
	ClipSurface(ScreenPos, input.positionCS.z, facing);
	#endif
	
	return color;
}