struct VSInput {
    @location(0) pos: vec3f,
    @location(1) uv0: vec2f,
};
 
struct PSInput {
    @builtin(position) pos: vec4f,
    @location(1) uv0: vec2f,
};
 
@vertex
fn vsmain(v: VSInput) -> PSInput {
    var output: PSInput;
    
    output.pos = vec4f(v.pos, 1);
    output.uv0 = v.uv0;
    
    return output;
}

@fragment
fn psmain(i: PSInput) -> @location(0) vec4<f32> {
    return vec4f(i.uv0, 0, 1);
}