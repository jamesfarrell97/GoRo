//Stylized Water 2
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

//#undef MATHEMATICS

using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Profiling;

#if MATHEMATICS
using Unity.Mathematics;
using static Unity.Mathematics.math;
using Vector4 = Unity.Mathematics.float4;
using Vector3 = Unity.Mathematics.float3;
using Vector2 = Unity.Mathematics.float2;
#endif

namespace StylizedWater2
{
    public static class Buoyancy
    {
        private static readonly int CustomTimeID = Shader.PropertyToID("_CustomTime");
        private static readonly int TimeParametersID = Shader.PropertyToID("_TimeParameters");

        //material
        private const string WavesKeyword = "_WAVES";
        private static readonly int WaveDistID = Shader.PropertyToID("_WaveDistance");
        private static readonly int WaveSpeedID = Shader.PropertyToID("_WaveSpeed");
        private static readonly int WaveHeightID = Shader.PropertyToID("_WaveHeight");
        private static readonly int WaveSteepnessID = Shader.PropertyToID("_WaveSteepness");
        private static readonly int WaveCountID = Shader.PropertyToID("_WaveCount");
        private static readonly int WaveDirectionID = Shader.PropertyToID("_WaveDirection");
        private static readonly int AnimationParamID = Shader.PropertyToID("_AnimationParams");

        private static bool Initialized;
        
        private static void Initialize()
       {
           Initialized = true;
       }

        private static void GetMaterialParameters(Material mat)
        {
            waveDistance = mat.GetFloat(WaveDistID);
            waveHeight = mat.GetFloat(WaveHeightID);
            waveSpeed = mat.GetFloat(WaveSpeedID);
            waveSteepness = mat.GetFloat(WaveSteepnessID) + 0.1f;
            waveCount = mat.GetInt(WaveCountID);
            waveDirection = mat.GetVector(WaveDirectionID);
            animationParams = mat.GetVector(AnimationParamID);
        }

        /// <summary>
        /// Returns the maximum possible wave height set on the material
        /// </summary>
        /// <param name="mat"></param>
        /// <returns></returns>
        public static float GetMaxWaveHeight(Material mat)
        {
            if (!mat) return 0f;
            
            if(!Initialized) Initialize();

            if (WavesEnabled(mat) == false) return 0f;
                
            return waveHeight = mat.GetFloat(WaveHeightID);
        }

        public static bool UseCustomTime = false;
        private static float customTimeValue = 0f;

        /// <summary>
        /// See "Shader" section of documentation. This is used for network synchronized waves.
        /// </summary>
        /// <param name="value"></param>
        public static void SetCustomTime(float value)
        {
            if(!Initialized) Initialize();
            
            customTimeValue = value;
            Shader.SetGlobalFloat(CustomTimeID, customTimeValue);
        }
        
        //Returns the same value as _TimeParameters.x
        private static float _TimeParameters
        {
            get
            {
                if (UseCustomTime) return customTimeValue;
                
#if UNITY_EDITOR
                return Application.isPlaying ? Time.time : Shader.GetGlobalVector(TimeParametersID).x;
#else
                return Time.time;
#endif
            }
        }
        
        private static float Dot2(Vector2 a, Vector2 b)
        {
#if MATHEMATICS
            return dot(a,b);
#else
            return Vector2.Dot(a, b);
#endif
        }
        
        private static float Dot4(Vector4 a, Vector4 b)
        {
#if MATHEMATICS
            return dot(a,b);
#else
            return Vector4.Dot(a, b);
#endif
        }
        
        private static float Sine(float t)
        {
#if MATHEMATICS
            return sin(t);
#else
            return Mathf.Sin(t);
#endif
        }
        
        private static float Cosine(float t)
        {
#if MATHEMATICS
            return cos(t);
#else
            return Mathf.Cos(t);
#endif
        }

        private static void Vector4Sin(ref Vector4 input, Vector4 a, Vector4 b)
        {
            input.x = Sine(a.x + b.x);
            input.y = Sine(a.y + b.y);
            input.z = Sine(a.z + b.z);
            input.w = Sine(a.w + b.w);
        }
        
        private static void Vector4Cosin(ref Vector4 input, Vector4 a, Vector4 b)
        {
            input.x = Cosine(a.x + b.x);
            input.y = Cosine(a.y + b.y);
            input.z = Cosine(a.z + b.z);
            input.w = Cosine(a.w + b.w);
        }

        private static Vector4 MultiplyVec4(Vector4 a, Vector4 b)
        {
#if MATHEMATICS
            return a * b;
#else
            return Vector4.Scale(a, b);
#endif
        }

        private static Vector4 sine;
        private static Vector4 cosine;
        private static Vector4 dotABCD;
        private static Vector4 AB;
        private static Vector4 CD;
        private static Vector4 direction1;
        private static Vector4 direction2;
        private static Vector4 TIME;
        private static Vector2 planarPosition;

        //material
        private static float waveDistance;
        private static float waveSpeed;
        private static float waveHeight;
        private static float waveSteepness;
        private static int waveCount;
        private static Vector4 waveDirection;
        private static Vector4 animationParams;

        private static Vector4 amp = new Vector4(0.3f, 0.35f, 0.25f, 0.25f);
        private static Vector4 speed = new Vector4(1.2f, 1.375f, 1.1f, 1);
        private static Vector4 dir1 = new Vector4(0.3f, 0.85f, 0.85f, 0.25f);
        private static Vector4 dir2 = new Vector4(0.1f, 0.9f, -0.5f, -0.5f);
        private static Vector4 steepness = new Vector4(12f,12f,12f,12f);

        //Real frequency value per wave layer
        private static Vector4 frequency;
        private static Vector4 realSpeed;

        public static bool WavesEnabled(Material waterMat)
        {
            return waterMat.IsKeywordEnabled(WavesKeyword);
        }

        /// <summary>
        /// Given a position in world-space, returns the wave height and normal
        /// </summary>
        /// <param name="position">Sample position in world-space</param>
        /// <param name="waterObject">Water object component, used to get the water material and level (height)</param>
        /// <param name="rollStrength">Multiplier for the the normal strength</param>
        /// <param name="normal">Output upwards normal vector</param>
        /// <returns>Wave height, in world-space.</returns>
        public static float SampleWaves(Vector3 position, WaterObject waterObject, float rollStrength, out UnityEngine.Vector3 normal)
        {
            return SampleWaves(position, waterObject.material, waterObject.transform.position.y, rollStrength,
                out normal);
        }
        
        /// <summary>
        /// Given a position in world-space, returns the wave height and normal
        /// </summary>
        /// <param name="position">Sample position in world-space</param>
        /// <param name="waterMat">Material using StylizedWater2 shader</param>
        /// <param name="waterLevel">Height of the reference water plane.</param>
        /// <param name="rollStrength">Multiplier for the the normal strength</param>
        /// <param name="normal">Output upwards normal vector</param>
        /// <returns>Wave height, in world-space.</returns>
        public static float SampleWaves(Vector3 position, Material waterMat, float waterLevel, float rollStrength, out UnityEngine.Vector3 normal)
        {
            Profiler.BeginSample("Buoyancy sampling");

            if(!waterMat)
            {
                normal = UnityEngine.Vector3.up;
                return waterLevel;
            }

            if(WavesEnabled(waterMat) == false)
            {
                normal = UnityEngine.Vector3.up;
                return waterLevel;
            }
            
            if(!Initialized) Initialize();

            GetMaterialParameters(waterMat);

            Vector4 freq = new Vector4(1.3f, 1.35f, 1.25f, 1.25f) * (1-waveDistance) * 3f;
            
            direction1 = MultiplyVec4(dir1, waveDirection);
            direction2 = MultiplyVec4(dir2, waveDirection);

            Vector3 offsets = Vector3.zero;
            frequency = freq;
            realSpeed.x *= animationParams.x;
            realSpeed.y *= animationParams.y;
            realSpeed.z *= animationParams.x;
            realSpeed.w *= animationParams.y;
            
            for (int i = 0; i <= waveCount; i++)
            {
                float t = 1f+((float)i / (float)waveCount);

                frequency *= t;

                AB.x = steepness.x * waveSteepness * direction1.x * amp.x;
                AB.y = steepness.x * waveSteepness * direction1.y * amp.x;
                AB.z = steepness.x * waveSteepness * direction1.z * amp.y;
                AB.w = steepness.x * waveSteepness * direction1.w * amp.y;

                CD.x = steepness.z * waveSteepness * direction2.x * amp.z;
                CD.y = steepness.z * waveSteepness * direction2.y * amp.z;
                CD.z = steepness.w * waveSteepness * direction2.z * amp.w;
                CD.w = steepness.w * waveSteepness * direction2.w * amp.w;
                
                planarPosition.x = position.x;
                planarPosition.y = position.z;
                
                #if MATHEMATICS
                dotABCD.x = Dot2(direction1.xy, planarPosition) * frequency.x;
                dotABCD.y = Dot2(direction1.zw, planarPosition) * frequency.y;
                dotABCD.z = Dot2(direction2.xy, planarPosition) * frequency.z;
                dotABCD.w = Dot2(direction2.zw, planarPosition) * frequency.w;
                #else
                dotABCD.x = Dot2(new Vector2(direction1.x, direction1.y), planarPosition) * frequency.x;
                dotABCD.y = Dot2(new Vector2(direction1.z, direction1.w), planarPosition) * frequency.y;
                dotABCD.z = Dot2(new Vector2(direction2.x, direction2.y), planarPosition) * frequency.z;
                dotABCD.w = Dot2(new Vector2(direction2.z, direction2.w), planarPosition) * frequency.w;
                #endif
                

                TIME = (_TimeParameters * animationParams.z * waveSpeed * speed);

                sine.x = Sine(dotABCD.x + TIME.x);
                sine.y = Sine(dotABCD.y + TIME.y);
                sine.z = Sine(dotABCD.z + TIME.z);
                sine.w = Sine(dotABCD.w + TIME.w);

                cosine.x = Cosine(dotABCD.x + TIME.x);
                cosine.y = Cosine(dotABCD.y + TIME.y);
                cosine.z = Cosine(dotABCD.z + TIME.z);
                cosine.w = Cosine(dotABCD.w + TIME.w);
                
                offsets.x += Dot4(cosine, new Vector4(AB.x, AB.z, CD.x, CD.z));
                offsets.y += Dot4(sine, amp);
                offsets.z += Dot4(cosine, new Vector4(AB.y, AB.w, CD.y, CD.w));
            }
            
            rollStrength *=  Mathf.Lerp(0.001f, 0.1f, waveSteepness);
            
            normal = new Vector3(-offsets.x * rollStrength * waveHeight, 2f, -offsets.z * rollStrength * waveHeight);
            
#if MATHEMATICS
            normal = normalize(normal);
#else
            normal = normal.normalized;
#endif

            //Average height
            offsets.y /= waveCount;
            
            Profiler.EndSample();
            return (offsets.y * waveHeight) + waterLevel;
        }

        /// <summary>
        /// Checks if the position is below the maximum possible wave height. Can be used as a fast broad-phase check, before actually using the more expensive SampleWaves function
        /// </summary>
        /// <param name="position"></param>
        /// <param name="waterObject"></param>
        /// <returns></returns>
        public static bool CanTouchWater(Vector3 position, WaterObject waterObject)
        {
            if (!waterObject) return false;
            
            waveHeight = GetMaxWaveHeight(waterObject.material);

            return position.y < (waterObject.transform.position.y + waveHeight);
        }
    }
}