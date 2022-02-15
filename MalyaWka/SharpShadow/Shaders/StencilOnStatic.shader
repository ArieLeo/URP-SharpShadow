Shader "Hidden/MalyaWka/SharpShadow/StencilOnStatic"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="LightweightPipeline" "IgnoreProjector"="true" }
        Pass
        {
            Tags { "LightMode"="StencilOnStatic" }
            
            Stencil
            {
                PassFront IncrWrap
                PassBack DecrWrap
            }
            
            ColorMask 0
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma vertex StaticVertex
            #pragma fragment Fragment
            #include "Extrude.hlsl"
            ENDHLSL
        }
    }
}

