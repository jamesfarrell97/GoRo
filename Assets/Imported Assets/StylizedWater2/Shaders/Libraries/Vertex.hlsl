//Stylized Water 2
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

Varyings LitPassVertex(Attributes input)
{
	Varyings output = (Varyings)0;

	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_TRANSFER_INSTANCE_ID(input, output);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

	output.uv.xy = input.uv.xy;
	output.uv.z = _TimeParameters.x;
	output.uv.w = 0;

#if defined(CURVEDWORLD_IS_INSTALLED) && !defined(CURVEDWORLD_DISABLED_ON) 
#if defined(CURVEDWORLD_NORMAL_TRANSFORMATION_ON)
	CURVEDWORLD_TRANSFORM_VERTEX_AND_NORMAL(input.positionOS, input.normalOS.xyz, input.tangentOS)
#else
    CURVEDWORLD_TRANSFORM_VERTEX(input.positionOS)
#endif
#endif

	float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);

	VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS.xyz, input.tangentOS);
	
	float4 vertexColor = GetVertexColor(input.color, _VertexColorMask);
	
#if _WAVES
	//Returns mesh or world-space UV
	float2 uv = GetSourceUV(output.uv.xy, positionWS.xz, _WorldSpaceUV);

	//Vertex animation
	WaveInfo waves = GetWaveInfo(uv, TIME_VERTEX * _WaveSpeed,  _WaveFadeDistance.x, _WaveFadeDistance.y);
	//Offset in direction of normals (only when using mesh uv)
	if(_WorldSpaceUV == 0) waves.position *= normalInput.normalWS.xyz;
	positionWS.xz += waves.position.xz * HORIZONTAL_DISPLACEMENT_SCALAR * _WaveHeight;
	positionWS.y += waves.position.y * _WaveHeight * lerp(1, 0, vertexColor.b);
#endif

	output.positionCS = TransformWorldToHClip(positionWS);
	half fogFactor = CalculateFogFactor(output.positionCS.xyz);

#ifdef SCREEN_POS
	output.screenPos = ComputeScreenPos(output.positionCS);
#endif
	output.normal = float4(normalInput.normalWS, positionWS.x);
#if _NORMALMAP
	output.tangent = float4(normalInput.tangentWS, positionWS.y);
	output.bitangent = float4(normalInput.bitangentWS, positionWS.z);
#else
	output.wPos = positionWS.xyz;
#endif

	OUTPUT_SH(normalInput.normalWS.xyz, output.lightmapUVOrVertexSH.xyz);

	//Lambert shading
	half3 vertexLight = 0;
#ifdef _ADDITIONAL_LIGHTS_VERTEX
	vertexLight = VertexLighting(positionWS, normalInput.normalWS);
#endif

	output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
	output.color = vertexColor;

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
	VertexPositionInputs vertexInput = (VertexPositionInputs)0;
	vertexInput.positionWS = positionWS;
	vertexInput.positionCS = output.positionCS;
	output.shadowCoord = GetShadowCoord(vertexInput);
#endif
	return output;
}