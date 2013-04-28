using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using FarseerPhysics.Common;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;

namespace OlegEngine.Entity
{
    public class BaseEntity
    {
        public enum RenderModes
        {
            Opaque,
            Translucent
        }

        public enum MoveTypes
        {
            NONE,
            PHYSICS
        }

        public string Name { get; set; }
        public string Class { get; set; }
        public Mesh Model { get; set; }
        public Material Mat { get; set; }
        public RenderModes RenderMode { get; set; }
        public bool DisableLighting { get; set; }
        public OpenTK.Graphics.OpenGL.BeginMode drawMode = OpenTK.Graphics.OpenGL.BeginMode.Triangles;
        public bool WorldSpawn = false;
        public Vector3 Color { get; set; }
        public MoveTypes Movetype { get; set; }
        public bool ShouldDraw { get; set; }

        public Vector3 Position { get; private set; }
        public Vector3 Angle { get; private set; }
        public Vector3 Scale { get; set; }

        public Fixture Physics { get; set; }

        private Matrix4 modelview = Matrix4.Identity;
        public bool _toRemove = false;

        public void Spawn()
        {
            this.Color = Vector3.One;
            this.Scale = Vector3.One;
            this.DisableLighting = false;
            this.RenderMode = RenderModes.Opaque;
            this.Movetype = MoveTypes.PHYSICS;
            this.ShouldDraw = true;
            this.Init();

            if (this.Mat == null) this.Mat = Utilities.ErrorMat;
        }

        public virtual void Init()
        {
            this.Model = Resource.GetMesh("ball.obj");
            this.Mat = Utilities.ErrorMat;
        }

        public virtual void Remove()
        {
            _toRemove = true;
        }
        public virtual void Think()
        {

        }
        public virtual void Draw()
        {
            if (Model != null && this.ShouldDraw)
            {
                if (this.DisableLighting) GL.Disable(EnableCap.Lighting);

                Model.mat       = this.Mat;
                Model.Color     = this.Color;

                Model.Position  = this.Position;
                Model.Scale     = this.Scale;
                Model.Angle     = this.Angle;

                Model.Draw();

                if (this.DisableLighting) GL.Enable(EnableCap.Lighting);
            }
        }

        #region misc
        public void SetPos(Vector3 pos, bool setPhys = true)
        {
            Position = pos;
            if (this.Physics != null && setPhys )
            {
                this.Physics.Body.Position = new Microsoft.Xna.Framework.Vector2(pos.X, pos.Y );
            }
        }
        public void SetPos(Vector2 pos, bool setPhys = true)
        {
            SetPos(new Vector3(pos.X, pos.Y, Position.Z), setPhys);
        }

        public void SetAngle(Vector3 ang, bool phys = false)
        {
            Angle = ang;
            if (this.Physics != null && phys)
            {
                this.Physics.Body.Rotation = ang.Z;
            }
            //Update physics position
        }

        public void SetAngle(float ang, bool phys = false)
        {
            this.SetAngle(new Vector3( Angle.X, Angle.Y, ang ), phys);
        }

        public void SetModel(Mesh model)
        {
            if (Model != null) { Model.Remove(); }
            Model = model;
        }

        public void EmitSound(string soundpath, bool loop = false, float volume = 1.0f, int frequency = 44100)
        {
            Audio song = Audio.LoadSong(soundpath, loop, true, this);
            song.Play(true);
            song.SetVolume(volume);
            song.SetFrequency(frequency);
        }
        #endregion

    }
}
