struct VSInput {
    @location(0) pos: vec3f,
    @location(1) uv0: vec2f,
};
 
struct PSInput {
    @builtin(position) pos: vec4f,
    @location(1) uv0: vec2f,
};
 
@vertex
fn foo(v: VSInput) -> PSInput {
    var output: PSInput = 0;
    
    o.pos = vec4f(v.pos, 1);
    o.uv0 = v.uv0;
    
    return o;
}

@fragment
fn bar(i: PSInput) -> vec4<f32> {
    return vec4f(i.uv0, 0, 1);
}