Shader "Custom/Water"
{
    Properties
    {
        _WaterDepth("Water Depth", Float) = 1
        _ShallowColour("Shallow Colour", Color) = (0.1109825, 0.8113207, 0.790536, 1)
        _DeepColour("Deep Colour", Color) = (0, 0.0483309, 0.3490566, 1)

        _WaveAmplitude("Wave Amplitude", Range(0, 1)) = 0.25
        _WaveSpeed("Wave Speed", Range(0, 10)) = 1
        _WaveFrequency("Wave Frequency", Range(0, 10)) = 1
        [HideInInspector] _GlobalWaterTime("Global Water Time", Float) = 0

        _SpecularSpeed("Specular Speed", Range(0, 10)) = 0.5
        _SpecularDensity("Specular Density", Range(0, 10)) = 3
        _SpecularThickness("Specular Thickness", Range(0, 10)) = 5
        _SpecularColour("Specular Colour", Color) = (0.5727127, 0.6836024, 0.8490566, 1)

        _Tiling("Tiling", Vector, 2) = (1, 1, 0, 0)
        _Offset("Offset", Vector, 2) = (0, 0, 0, 0)

        _TessellationAmount("Tessellation Amount", Range(1, 64)) = 1
        _TessellationFadeStart("Tessellation Fade Start", Float) = 25
        _TessellationFadeEnd("Tessellation Fade End", Float) = 50

        _FoamCutoff("Foam Cutoff", Range(0, 1)) = 0.777
        _FoamMaxDistance("Foam Maximum Distance", Float) = 0.4
		_FoamMinDistance("Foam Minimum Distance", Float) = 0.04		
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
        }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma hull hull
            #pragma domain domain
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Hashes.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            float2 VoronoiRandomVectorDeterministic(float2 uv, float offset)
            {
                Hash_Tchou_2_2_float(uv, uv);
                return float2(sin(uv.y * offset), cos(uv.x * offset)) * 0.5 + 0.5;
            }

            float3 Voronoi(float2 uv, float angleOffset, float cellDensity)
            {
                float2 g = floor(uv * cellDensity);
                float2 f = frac(uv * cellDensity);
                float3 res = float3(8.0, 0.0, 0.0);
                for (int y = -1; y <= 1; y++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        float2 lattice = float2(x, y);
                        float2 offset = VoronoiRandomVectorDeterministic(lattice + g, angleOffset);
                        float d = distance(lattice + offset, f);
                        if (d < res.x)
                        {
                            res = float3(d, offset.x, offset.y);
                        }
                    }
                }
                return res;
            }

            float2 TilingAndOffset(float2 uv, float2 tiling, float2 offset)
            {
                return uv * tiling + offset;
            }

            float4 AlphaBlend(float4 top, float4 bottom)
            {
                float3 colour = (top.rgb * bottom.a) + (bottom.rgb * (1 - top.a));
                float alpha = top.a + bottom.a * (1 - top.a);
                return float4(colour, alpha);
            }

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct TessellationControlPoint
            {
                float3 positionWS : INTERNALTESSPOS;
                float2 uv : TEXCOORD0;
            };

            struct TesselationFactors
            {
                float edge[3] : SV_TessFactor;
                float inside : SV_InsideTessFactor;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 positionSS : TEXCOORD1;
                float3 normal : NORMAL;
            };

            CBUFFER_START(UnityPerMaterial)
                float _WaterDepth; 
                float4 _ShallowColour;
                float4 _DeepColour;

                float _WaveAmplitude;
                float _WaveSpeed;
                float _WaveFrequency;
                float _GlobalWaterTime;

                float _SpecularSpeed;
                float _SpecularDensity;
                float _SpecularThickness;
                float4 _SpecularColour;

                float2 _Tiling;
                float2 _Offset;

                float _TessellationAmount;
                float _TessellationFadeStart;
                float _TessellationFadeEnd;

                float _FoamCutoff;
                float _FoamMaxDistance;
                float _FoamMinDistance;
            CBUFFER_END

            TessellationControlPoint vert(Attributes IN)
            {
                TessellationControlPoint OUT;
                ZERO_INITIALIZE(TessellationControlPoint, OUT);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.uv = TilingAndOffset(IN.uv, _Tiling, _Offset);
                return OUT;
            }

            [domain("tri")]
            [outputcontrolpoints(3)]
            [outputtopology("triangle_cw")]
            [partitioning("integer")]
            [patchconstantfunc("patchConstantFunc")]
            TessellationControlPoint hull(InputPatch<TessellationControlPoint, 3> PATCH, uint ID : SV_OutputControlPointID)
            {
                return PATCH[ID];
            }

            TesselationFactors patchConstantFunc(InputPatch<TessellationControlPoint, 3> PATCH)
            {
                TesselationFactors OUT;
                ZERO_INITIALIZE(TesselationFactors, OUT);
                float3 triPos0 = PATCH[0].positionWS;
                float3 triPos1 = PATCH[1].positionWS;
                float3 triPos2 = PATCH[2].positionWS;
                float3 edgePos0 = (triPos1 + triPos2) / 2.0;
                float3 edgePos1 = (triPos0 + triPos2) / 2.0;
                float3 edgePos2 = (triPos0 + triPos1) / 2.0;
                float3 cameraPos = _WorldSpaceCameraPos;
                float dist0 = distance(edgePos0, cameraPos);
                float dist1 = distance(edgePos1, cameraPos);
                float dist2 = distance(edgePos2, cameraPos);
                float fadeDistance = _TessellationFadeEnd - _TessellationFadeStart;
                float edgeFactor0 = saturate(1.0 - (dist0 - _TessellationFadeStart) / fadeDistance);
                float edgeFactor1 = saturate(1.0 - (dist1 - _TessellationFadeStart) / fadeDistance);
                float edgeFactor2 = saturate(1.0 - (dist2 - _TessellationFadeStart) / fadeDistance);
                OUT.edge[0] = max(edgeFactor0 * _TessellationAmount, 1.0);
                OUT.edge[1] = max(edgeFactor1 * _TessellationAmount, 1.0);
                OUT.edge[2] = max(edgeFactor2 * _TessellationAmount, 1.0);
                OUT.inside = (OUT.edge[0] + OUT.edge[1] + OUT.edge[2]) / 3.0;
                return OUT;
            };

            float Wave(float3 positionWS)
            {
                return sin(positionWS.x * _WaveFrequency + positionWS.z * _WaveFrequency + _GlobalWaterTime * _WaveSpeed) * _WaveAmplitude;
            }

            [domain("tri")]
            Varyings domain(TesselationFactors FACTORS, OutputPatch<TessellationControlPoint, 3> PATCH, float3 BARYCENTRIC_COORDINATES : SV_DomainLocation)
            {
                Varyings OUT;
                ZERO_INITIALIZE(Varyings, OUT);
                float epsilon = 0.01;
                float3 positionWS = PATCH[0].positionWS * BARYCENTRIC_COORDINATES.x + PATCH[1].positionWS * BARYCENTRIC_COORDINATES.y + PATCH[2].positionWS * BARYCENTRIC_COORDINATES.z;
                float2 uv = PATCH[0].uv * BARYCENTRIC_COORDINATES.x + PATCH[1].uv * BARYCENTRIC_COORDINATES.y + PATCH[2].uv * BARYCENTRIC_COORDINATES.z;
                float3 newPositionWS = positionWS;
                newPositionWS.y += Wave(newPositionWS);
                float3 newPositionXWS = positionWS + float3(epsilon, 0, 0);
                newPositionXWS.y += Wave(newPositionXWS);
                float3 newPositionZWS = positionWS + float3(0, 0, epsilon);
                newPositionZWS.y += Wave(newPositionZWS);
                float3 newEdgeX = newPositionXWS - newPositionWS;
                float3 newEdgeZ = newPositionZWS - newPositionWS;
                float3 normal = normalize(cross(newEdgeZ, newEdgeX));
                OUT.positionHCS = TransformWorldToHClip(newPositionWS);
                OUT.uv = uv;
                OUT.positionSS = ComputeScreenPos(OUT.positionHCS);
                OUT.normal = normal;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float2 screenUV = IN.positionSS.xy / IN.positionSS.w;
                float rawDepth = SampleSceneDepth(screenUV);
                float existingDepthLinear = LinearEyeDepth(rawDepth, _ZBufferParams);
                float depthDifference = existingDepthLinear - IN.positionSS.w;
                float waterDepthDifference = saturate(depthDifference / _WaterDepth);
                float4 waterColour = lerp(_ShallowColour, _DeepColour, waterDepthDifference);
                float3 voronoi = Voronoi(IN.uv, _Time.y * _SpecularSpeed, _SpecularDensity);
                float specularHighlight = pow(voronoi.x, _SpecularThickness);
                float4 specularColour = specularHighlight * _SpecularColour;
                float3 existingNormals = SampleSceneNormals(screenUV);
                float3 viewNormal = normalize(mul((float3x3)UNITY_MATRIX_V, IN.normal));
                float3 normalDot = saturate(dot(existingNormals, viewNormal));
                float foamDistance = lerp(_FoamMaxDistance, _FoamMinDistance, normalDot);
                float foamDepthDifference = saturate(depthDifference / foamDistance);
                float foamNoiseCutoff = foamDepthDifference * _FoamCutoff;
                float foamNoise = specularHighlight > foamNoiseCutoff ? 1 : 0;
                return AlphaBlend(foamNoise, AlphaBlend(specularColour, waterColour));
            }
            ENDHLSL
        }
    }
}
