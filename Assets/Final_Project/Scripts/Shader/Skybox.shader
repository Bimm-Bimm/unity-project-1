Shader "Unlit/Skybox"
{
    Properties
    {
        _SunRadius("Sun radius", Range(0,1)) = 0.05
        [NoScaleOffset] _SunZenithGrad("Sun-Zenith gradient", 2D) = "white" {}
        [NoScaleOffset] _ViewZenithGrad("View-Zenith gradient", 2D) = "white" {}
        [NoScaleOffset] _SunViewGrad("Sun-View gradient", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
        Cull Off ZWrite Off


        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 posOS    : POSITION;
            };

            struct v2f
            {
                float4 posCS        : SV_POSITION;
                float3 viewDirWS    : TEXCOORD0;
            };

            v2f Vertex(Attributes IN)
            {
                v2f OUT = (v2f)0;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(IN.posOS.xyz);

                OUT.posCS = vertexInput.positionCS;
                OUT.viewDirWS = vertexInput.positionWS;

                return OUT;
            }

            TEXTURE2D(_SunZenithGrad);      SAMPLER(sampler_SunZenithGrad);
            TEXTURE2D(_ViewZenithGrad);     SAMPLER(sampler_ViewZenithGrad);
            TEXTURE2D(_SunViewGrad);        SAMPLER(sampler_SunViewGrad);

            float3 _SunDir, _MoonDir;
            float _SunRadius;

            float GetSunMask(float sunViewDot, float sunRadius)
            {
                float stepRadius = 1 - sunRadius * sunRadius;
                return smoothstep(stepRadius - 0.02, stepRadius ,sunViewDot);
            }

            float4 Fragment(v2f IN) : SV_TARGET
            {
                float3 viewDir = normalize(IN.viewDirWS);
                
                float sunViewDot = dot(_SunDir, viewDir);
                float sunZenithDot = _SunDir.y;
                float viewZenithDot = viewDir.y;
                float sunMoonDot = dot(_SunDir, _MoonDir);

                float sunViewDot01 = (sunViewDot + 1.0) * 0.5;
                float sunZenithDot01 = (sunZenithDot + 1.0) * 0.5;
                
                float3 sunZenithColor = SAMPLE_TEXTURE2D(_SunZenithGrad, sampler_SunZenithGrad, float2(sunZenithDot01, 0.5)).rgb;

                float3 viewZenithColor = SAMPLE_TEXTURE2D(_ViewZenithGrad, sampler_ViewZenithGrad, float2(sunZenithDot01, 0.5)).rgb;
                float vzMask = pow(saturate(1.0 - viewZenithDot), 4);

                float3 sunViewColor = SAMPLE_TEXTURE2D(_SunViewGrad, sampler_SunViewGrad, float2(sunZenithDot01, 0.5)).rgb;
                float svMask = pow(saturate(sunViewDot), 4);

                float sunMask = GetSunMask(sunViewDot, _SunRadius);
                float3 sunColor = float3(1,1,1) * sunMask;

                float3 skyColor = sunZenithColor + vzMask * viewZenithColor + svMask * sunViewColor;

                float3 col = skyColor + sunColor;
                return float4(col, 1);
               /* float3 col = saturate(float3(step(0.9,dot(_SunDir, viewDir)), step(0.9,dot(_MoonDir, viewDir)), 0));*/
            }
            ENDHLSL
        }
    }
}
