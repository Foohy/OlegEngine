#version 150
// It was expressed that some drivers required this next line to function properly
precision highp float;

uniform struct FogParameters
{
	int Enabled;
	
	vec3 Color;
	
	float Start;
	float End;
	float Density;
	int FogType; //0 = Linear, 1 = Exp, 2 = Exp2
} gFogParams;

in vec2 ex_UV;
in vec3 ex_Normal;
uniform mat4 _pmatrix;
uniform mat4 _vmatrix;
uniform float _time;
uniform sampler2D sampler;
uniform vec3 _color = vec3( 1.0, 1.0, 1.0);
uniform float gAlpha = 1.0;

out vec4 ex_FragColor;

float CalcFogLinear( float fogStart, float fogEnd, float fogCoord )
{
	return clamp( (fogEnd-fogCoord)/(fogEnd-fogStart), 0.0, 1.0);
}

float CalcFogExp( float fogDensity, float fogCoord )
{
	return clamp( exp(-fogDensity*fogCoord), 0.0, 1.0);
}

float CalcFogExp2( float fogDensity, float fogCoord )
{
	return clamp(exp(-pow(fogDensity*fogCoord, 2.0)), 0.0, 1.0);
}

vec4 CalcFog(vec4 pixelColor)
{
	if (gFogParams.Enabled == 0 ) return pixelColor;

	float z = (gl_FragCoord.z / gl_FragCoord.w);
	
	if (gFogParams.FogType == 0) 
		return mix( vec4( gFogParams.Color, 0), pixelColor, CalcFogLinear( gFogParams.Start, gFogParams.End, z ) );
	else if (gFogParams.FogType == 1)
		return mix( vec4( gFogParams.Color, 0), pixelColor, CalcFogExp(gFogParams.Density, z ) ); 
	else if (gFogParams.FogType == 2)
		return mix( vec4( gFogParams.Color, 0), pixelColor, CalcFogExp2(gFogParams.Density, z ) ); 
	
	return pixelColor;
}

void main()
{
	ex_FragColor = CalcFog( texture2D( sampler, ex_UV.st) * vec4(_color, gAlpha ) );
}