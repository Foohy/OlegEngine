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
        public Mesh Model { get; protected set; }
        public Material Material { get; set; }
        public RenderModes RenderMode { get; set; }
        public bool DisableLighting { get; set; }
        public OpenTK.Graphics.OpenGL.BeginMode drawMode = OpenTK.Graphics.OpenGL.BeginMode.Triangles;
        public bool WorldSpawn = false;
        public Vector3 Color { get; set; }
        public float Alpha { get; set; }
        public MoveTypes Movetype { get; set; }
        public bool ShouldDraw { get; set; }

        public Vector3 Position 
        {
            get
            {
                return (this.Parent != null) ? this.Parent.Position + this.posOffset : this.posOffset;
            }
            private set 
            { 
                this.posOffset = value; 
            } 
        }

        public Angle Angles 
        {
            get
            {
                return (this.Parent != null) ? this.Parent.Angles + this.angleOffset : this.angleOffset;
            }
            private set
            {
                this.angleOffset = value;   
            } 
        }
        public Vector3 Scale { get; set; }

        public BaseEntity Parent { get; private set; }

        public Fixture Physics { get; set; }

        private Matrix4 modelview = Matrix4.Identity;
        private Vector3 posOffset;
        private Angle angleOffset;
        public bool _toRemove = false;

        public void Spawn()
        {
            this.Color = Vector3.One;
            this.Alpha = 1.0f;
            this.Scale = Vector3.One;
            this.DisableLighting = false;
            this.RenderMode = RenderModes.Opaque;
            this.Movetype = MoveTypes.PHYSICS;
            this.ShouldDraw = true;
            this.Init();

            if (this.Material == null) this.Material = Utilities.ErrorMat;
        }

        public virtual void Init()
        {
            this.Material   = Utilities.ErrorMat;
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
                Model.mat               = this.Material;
                Model.Color             = this.Color;
                Model.Alpha             = this.Alpha;

                Model.Position          = this.Parent == null ? this.Position : this.Parent.Position;
                Model.PositionOffset    = this.Parent == null ? Vector3.Zero : this.posOffset;
                Model.Scale             = this.Scale;
                Model.Angles            = this.Angles;

                Model.Draw();
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

        public void SetAngle(Angle ang, bool phys = false)
        {
            this.Angles = ang;
            if (this.Physics != null && phys)
            {
                this.Physics.Body.Rotation = ang.Roll;
            }
            //Update physics position
        }

        public void SetAngle(float ang, bool phys = false)
        {
            this.SetAngle(this.Angles.SetRoll(ang), phys);
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

        public void SetParent(BaseEntity parent)
        {
            Vector3 worldPos = this.Position;
            this.Parent = parent;

            //Get local coordinates from the parent 
            if (this.Parent != null)
                this.SetPos(worldPos - parent.Position);
            else //Or just set us to where we are
                this.SetPos(worldPos);
        }
        #endregion

    }
}
