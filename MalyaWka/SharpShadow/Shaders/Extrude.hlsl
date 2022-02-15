#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

float _ME_ShadowNear;
float _ME_ShadowFar;
float _ME_ShadowFloor;

struct Attributes
{
    float4 position     : POSITION;
    float3 normal       : NORMAL;
    float4 tangent      : TANGENT;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct AttributesGPU
{
    float4 position     : POSITION;
    float3 normal       : NORMAL;
    float4 tangent      : TANGENT;
    float2 indices : TEXCOORD1;
    float2 weights : TEXCOORD2;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 position     : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

Varyings StaticVertex(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    VertexPositionInputs vertex = GetVertexPositionInputs(input.position.xyz);
    output.position = TransformWorldToHClip(vertex.positionWS);
   
    return output;
}

Varyings PlanarVertex(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    VertexPositionInputs vertex = GetVertexPositionInputs(input.position.xyz);
    Light light = GetMainLight();
    if (vertex.positionWS.y > _ME_ShadowFloor)
    {
        float3 lightDirection = -normalize(light.direction); 
        float opposite = vertex.positionWS.y - _ME_ShadowFloor;
        float cosTheta = -lightDirection.y;
        float hypotenuse = opposite / cosTheta;
        float3 worldPos = vertex.positionWS.xyz + ( lightDirection * hypotenuse ); 
        output.position = mul(UNITY_MATRIX_VP, float4(worldPos.x, _ME_ShadowFloor, worldPos.z ,1));
    }
    else
    {
        output.position = TransformWorldToHClip(vertex.positionWS);
    }
    return output;
}

Varyings ExtrudeVertex(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);

    Light light = GetMainLight();
    VertexPositionInputs vertex = GetVertexPositionInputs(input.position.xyz);
    VertexNormalInputs normal = GetVertexNormalInputs(input.normal, input.tangent);
    bool far = dot(normal.normalWS, light.direction) < 0.0;
    float floorFar = _ME_ShadowFloor + _ME_ShadowFar;
    if (far && vertex.positionWS.y > floorFar)
    {
        float3 lightDirection = -normalize(light.direction); 
        float opposite = vertex.positionWS.y - floorFar;
        float cosTheta = -lightDirection.y;
        float hypotenuse = opposite / cosTheta;
        float3 worldPos = vertex.positionWS.xyz + ( lightDirection * hypotenuse ); 
        output.position = mul(UNITY_MATRIX_VP, float4(worldPos.x, floorFar, worldPos.z ,1));
    }
    else 
    {
        float3 worldPos = vertex.positionWS - light.direction * _ME_ShadowNear;
        output.position = TransformWorldToHClip(worldPos);
    }
   
    return output;
}

half4 Fragment(Varyings input) : SV_Target
{
    return half4(0.0, 0.0, 0.0, 1.0);
}
