#version 150
// It was expressed that some drivers required this next line to function properly
precision highp float;

in vec3 ex_UV;
uniform samplerCube sampler;
out vec4 ex_FragColor;

void main()
{
	ex_FragColor = texture( sampler, ex_UV ); // * vec4(_color, gAlpha );
}