#version 150
// It was expressed that some drivers required this next line to function properly
precision highp float;

in vec2 ex_UV;
in vec3 ex_Normal;
uniform mat4 _pmatrix;
uniform mat4 _vmatrix;
uniform float _time;
uniform sampler2D sampler;
uniform sampler2D sampler_normal;
uniform sampler2D sampler_shadow;

out vec4 gl_FragColor;
void main()
{
    gl_FragColor = texture2D(sampler, ex_UV);
}