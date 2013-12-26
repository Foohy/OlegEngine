#version 150
// It was expressed that some drivers required this next line to function properly
precision highp float;

in vec3 ex_UV;

uniform float _time;
uniform sampler2D sampler; //For our purposes, this will be the 'color'
uniform sampler2D sampler_normal; //For our purposes, this will be the 'glow'
uniform vec3 gSunVector;

out vec4 ex_FragColor;

void main()
{
	vec3 V = normalize( ex_UV );
	vec3 L = normalize( gSunVector );
	
	float v1 = dot( V, L );
	
	vec4 Kc = texture2D( sampler, vec2((L.y + 1.0) / 2.0, 1.0 - V.y));
	vec4 Kg = texture2D( sampler_normal, vec2((L.y + 1.0)/2.0, 1.0 - v1 ));
	
	ex_FragColor = vec4(Kc.rgb + Kg.rgb * Kg.a / 2.0, Kc.a);
}