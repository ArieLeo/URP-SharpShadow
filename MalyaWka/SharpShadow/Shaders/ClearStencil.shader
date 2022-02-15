Shader "Hidden/MalyaWka/SharpShadow/ClearStencil"
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
				Comp Always
				Pass Replace
			}
			ColorMask 0
			Cull Back
			ZWrite Off
			ZTest Always

            HLSLPROGRAM
            #pragma target 2.0
            
            #pragma multi_compile_instancing
            #pragma vertex ClearVertex
            #pragma fragment ClearFragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 position : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 position : SV_POSITION;
            };

            Varyings ClearVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                output.position = input.position;
                return output;
            }

            half4 ClearFragment(Varyings input) : SV_Target
            {
                return half4(0.0, 0.0, 0.0, 0.0);
            }
            ENDHLSL
        }
    }
}