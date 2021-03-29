//Stylized Water 2
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

#ifdef UNITY_CAN_COMPILE_TESSELLATION

struct TessellationFactors
{
	float edge[3] : SV_TessFactor;
	float inside  : SV_InsideTessFactor;
};

float CalcDistanceTessFactor(float4 vertex, float minDist, float maxDist, float tess, float4x4 o2w, float3 cameraPos)
{
	float3 wpos = mul(o2w, vertex).xyz;
	float dist = distance(wpos, cameraPos);
	float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
	return f;
}

float4 CalcTriEdgeTessFactors(float3 triVertexFactors)
{
	float4 tess;
	tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
	tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
	tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
	tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
	return tess;
}


float4 DistanceBasedTess(float4 v0, float4 v1, float4 v2, float tess, float minDist, float maxDist, float4x4 o2w, float3 cameraPos)
{
	float3 f;
	f.x = CalcDistanceTessFactor(v0, minDist, maxDist, tess, o2w, cameraPos);
	f.y = CalcDistanceTessFactor(v1, minDist, maxDist, tess, o2w, cameraPos);
	f.z = CalcDistanceTessFactor(v2, minDist, maxDist, tess, o2w, cameraPos);

	return CalcTriEdgeTessFactors(f);
}

TessellationFactors TessellationFunction(InputPatch<Attributes, 3> v)
{
	TessellationFactors o;
	float4 tf = 1;
	float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;

	tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos);

	o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
	return o;
}

[domain("tri")]
[partitioning("fractional_odd")]
[outputtopology("triangle_cw")]
[patchconstantfunc("TessellationFunction")]
[outputcontrolpoints(3)]
Attributes HullFunction(InputPatch<Attributes, 3> patch, uint id : SV_OutputControlPointID)
{
	return patch[id];
}

[domain("tri")]
Varyings DomainFunction(TessellationFactors factors, OutputPatch<Attributes, 3> patch, float3 bary : SV_DomainLocation)
{
	Attributes o = (Attributes)0;
	o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
	o.normal = patch[0].normal * bary.x + patch[1].normal * bary.y + patch[2].normal * bary.z;
	o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
	o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
	o.color = patch[0].color * bary.x + patch[1].color * bary.y + patch[2].color * bary.z;

	UNITY_TRANSFER_INSTANCE_ID(patch[0], o);

	return LitPassVertex(o);
}

#endif