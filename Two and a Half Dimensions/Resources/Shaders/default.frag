#version 150
// It was expressed that some drivers required this next line to function properly
precision highp float;

in vec2 ex_UV;
in vec3 ex_Normal;
uniform mat4 _pmatrix;
uniform mat4 _vmatrix;
uniform float _time;
uniform sampler2D sampler;

out vec4 gl_FragColor;
void main()
{
	gl_FragColor = texture2D( sampler, ex_UV.st);
    //gl_FragColor = vec4( ex_Normal.x, ex_Normal.y, ex_Normal.z, 1.0 );
	//gl_FragColor = vec4( sin(_time) / 2.0 + 0.5, sin(_time) / 2.0 + 0.5, sin(_time) / 2.0 + 0.5, 1.0 );
}