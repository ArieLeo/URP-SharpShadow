Shader "Hidden/MalyaWka/SharpShadow/StencilOnPlanar"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="LightweightPipeline" "IgnoreProjector"="true" }
        Pass
        {

            Tags { "LightMode"="StencilOnPlanar" }
            
            Stencil
            {
                Pass IncrWrap
				ZFail Keep
            }
            
            ColorMask 0
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma vertex PlanarVertex
            #pragma fragment Fragment
            #include "Extrude.hlsl"
            ENDHLSL
        }
    }
}
