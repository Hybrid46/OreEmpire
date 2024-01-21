Shader "InstancedIndirect/Unlit"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1,1,1,1)
        _BaseTexture("Base Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline"}

        Pass
        {
            Cull Back
            ZTest Less
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half3 _BaseColor;
                sampler2D _BaseTexture;
                float4 _BaseTexture_ST;
                
                StructuredBuffer<float4x4> _AllInstancesTransformBuffer;
            CBUFFER_END
 
            Varyings vert(Attributes IN, uint instanceID : SV_InstanceID)
            {
                Varyings OUT;

                float4 positionWS = mul(mul(unity_WorldToObject, _AllInstancesTransformBuffer[instanceID]), IN.positionOS);

                //OUT.positionCS = TransformWorldToHClip(positionWS);
                OUT.positionCS = TransformObjectToHClip(positionWS);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                //half4 tex = SAMPLE_TEXTURE2D(_BaseTexture, IN.uv) * _BaseColor;
                return half4(_BaseColor, 1);
            }
            ENDHLSL
        }
    }
}