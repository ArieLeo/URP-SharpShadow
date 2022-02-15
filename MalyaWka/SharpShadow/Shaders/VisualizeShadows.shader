Shader "Hidden/MalyaWka/SharpShadow/VisualizeShadows"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="LightweightPipeline" "IgnoreProjector"="true" }
        Pass
        {
            Tags { "LightMode"="LightweightForward" }

            Stencil
            {
                Ref 0
                Comp NotEqual
                Pass Replace
            }

            Cull Off
            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma target 2.0
            
            #pragma multi_compile_instancing
            #pragma vertex VisualizeShadowVertex
            #pragma fragment VisualizeShadowFragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            half _ME_ShadowIntensity;

            struct Attributes
            {
                float4 position : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 position : SV_POSITION;
            };

            Varyings VisualizeShadowVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                output.position = TransformObjectToHClip(input.position.xyz);
                return output;
            }

            half4 VisualizeShadowFragment(Varyings input) : SV_Target
            {
                return half4(0.0, 0.0, 0.0, _ME_ShadowIntensity);
            }
            ENDHLSL
        }
    }
}