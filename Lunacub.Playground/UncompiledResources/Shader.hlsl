struct vertex {
    [[vk::location(0)]] float3 position : Position;
    [[vk::location(1)]] float4 color : Color;
    [[vk::location(2)]] float4 uv : TexCoord0;
};

struct fragment {
    float4 position : SV_Position;
    float4 color : Color;
    float2 uv : TexCoord0;
};

[[vk::binding(0, 0)]] cbuffer Transformation {
    float4x4 transformation;
}

[[vk::binding(1, 0)]] Texture2D texture;
[[vk::binding(2, 0)]] SamplerState main_sampler;

[shader("vertex")]
fragment vsmain(in vertex v) {
    fragment f = (fragment)0;
    
    f.position = mul(transformation, float4(v.position, 1.0));
    f.color = v.color;
    f.uv = v.uv;
    
    return f;
}

[shader("pixel")]
float4 psmain(in fragment f) : SV_Target0 {
    return f.color * texture.Sample(main_sampler, f.uv);
}