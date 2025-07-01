struct VSInput {
    @location(0) pos: vec3f,
    @location(1) uv0: vec2f,
};

struct PSInput {
    @builtin(position) pos: vec4f,
    @location(1) uv0: vec2f,
};

struct Transformation {
    mvp: mat4x4<f32>,
};

@group(1) @binding(0) var<uniform> _Transformation: Transformation;

@group(0) @binding(0) var _MainTexture: texture_2d<f32>;
@group(0) @binding(1) var _Sampler: sampler;
 
@vertex
fn vsmain(v: VSInput) -> PSInput {
    var output: PSInput;
    
    output.pos = _Transformation.mvp * vec4f(v.pos, 1);
    output.uv0 = v.uv0;
    
    return output;
}

@fragment
fn psmain(i: PSInput) -> @location(0) vec4<f32> {
    return textureSample(_MainTexture, _Sampler, i.uv0);
}