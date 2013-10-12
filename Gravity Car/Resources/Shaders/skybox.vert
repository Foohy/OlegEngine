#version 150

in vec3 _Position;
uniform mat4 _mmatrix;
uniform mat4 _pmatrix;
uniform mat4 _vmatrix;
uniform float _time;

out vec3 ex_UV;

vec4 vert;
void main() {
	vert = vec4( _Position.x, _Position.y, _Position.z, 1.0);
    gl_Position = (_mmatrix * _pmatrix * _vmatrix * vert).xyww;

	ex_UV = _Position;
}