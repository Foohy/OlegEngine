using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using OpenTK;
using OpenTK.Graphics.OpenGL;

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
        public List<BaseEntity> Children = new List<BaseEntity>();

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
        }
        public void SetPos(Vector2 pos, bool setPhys = true)
        {
            SetPos(new Vector3(pos.X, pos.Y, Position.Z), setPhys);
        }

        public void SetAngle(Angle ang, bool phys = false)
        {
            this.Angles = ang;
        }

        public void SetAngle(float ang, bool phys = false)
        {
            this.SetAngle(this.Angles.SetRoll(ang), phys);
        }

        public void SetModel(Mesh model, bool autoSetMaterial = true)
        {
            if (Model != null) { Model.Remove(); }
            Model = model;

            //Most models have corresponding materials to them, automatically load it in
            if (autoSetMaterial && !string.IsNullOrEmpty(model.SourceFileName))
            {
                string filename = string.Format("models{0}{1}{2}{3}", Path.DirectorySeparatorChar, Path.GetDirectoryName(model.SourceFileName), Path.DirectorySeparatorChar, Path.GetFileNameWithoutExtension(model.SourceFileName) );
                this.Material = Resource.GetMaterial(filename);
            }
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
            if (this.Parent != null)
                this.Parent.Children.Remove(this);

            Vector3 worldPos = this.Position;
            this.Parent = parent;

            //Get local coordinates from the parent 
            if (this.Parent != null)
            {
                this.SetPos(worldPos - parent.Position);
                this.Parent.Children.Add(this);
            }
            else //Or just set us to where we are
                this.SetPos(worldPos);
        }
        #endregion

    }
}
