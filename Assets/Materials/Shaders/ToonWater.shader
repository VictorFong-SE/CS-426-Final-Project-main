Shader "CS426/Water"
{
    Properties
    {	
        _DepthShallow("Depth Shallow", Color) = (0.0902, 0.165, 0.988, 0.725)
        _DepthDeep("Depth Deep", Color) = (0.325, 0.0588, 1, 0.749)
        _DepthMaxDistance("Depth Maximum Distance", Float) = 1
        _SurfaceNoise("Surface Noise", 2D) = "white" {}
        _SurfaceNoiseCutoff("Surface Noise Cutoff", Range(0, 1)) = 0.777
        _FoamDistance("Foam Distance", Float) = 0.04
        _SurfaceNoiseScroll("Surface Noise Scroll Amount", Vector) = (0.03, 0.03, 0, 0)
        _SurfaceDistortion("Surface Distortion", 2D) = "white" {}
        _SurfaceDistortionAmount("Surfface Distortion Amount", Range(0, 1)) = 0.27
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
        }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
			CGPROGRAM
            #define SMOOTHSTEP_AA 0.01
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD2;
                float2 noiseUV : TEXCOORD0;
                float2 distortUV : TEXCOORD1;
            };

            sampler2D _SurfaceNoise;
            float4 _SurfaceNoise_ST;
            sampler2D _SurfaceDistortion;
            float4 _SurfaceDistortion_ST;

            v2f vert (appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.noiseUV = TRANSFORM_TEX(v.uv, _SurfaceNoise);
                o.distortUV = TRANSFORM_TEX(v.uv, _SurfaceDistortion);

                return o;
            }

            float4 _DepthShallow;
            float4 _DepthDeep;

            float _DepthMaxDistance;

            sampler2D _CameraDepthTexture;

            float _SurfaceNoiseCutoff;

            float _FoamDistance;

            float2 _SurfaceNoiseScroll;

            float _SurfaceDistortionAmount; 

            float4 frag (v2f i) : SV_Target
            {
                float existingDepth = tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)).r;
                float existingDepthLin = LinearEyeDepth(existingDepth);

                float depthDif = existingDepthLin - i.screenPos.w;

                float waterDepthDifference = saturate(depthDif / _DepthMaxDistance);
                float4 waterColor = lerp(_DepthShallow, _DepthDeep, waterDepthDifference);

                float2 distortSample = (tex2D(_SurfaceDistortion, i.distortUV).xy * 2 - 1) * _SurfaceDistortionAmount;
                
                float2 noiseUV = float2((i.noiseUV.x + _Time.y * _SurfaceNoiseScroll.x) + distortSample.x, (i.noiseUV.y + _Time.y * _SurfaceNoiseScroll.y) + distortSample.y);

                float surfaceNoiseSample = tex2D(_SurfaceNoise, noiseUV).r;

                float foamDepthDif = saturate(depthDif / _FoamDistance);
                float surfaceNoiseCutoff = foamDepthDif * _SurfaceNoiseCutoff;

                float surfaceNoise = smoothstep(surfaceNoiseCutoff - SMOOTHSTEP_AA, surfaceNoiseCutoff + SMOOTHSTEP_AA, surfaceNoiseSample);

				return waterColor + surfaceNoise;
            }
            ENDCG
        }
    }
}