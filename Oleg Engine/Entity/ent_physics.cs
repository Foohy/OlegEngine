using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OlegEngine.Entity
{
    public class ent_physics : BaseEntity 
    {

        public override void Init()
        {
            base.Init();

            //Try to create a physics object given our mesh
            if (this.Model != null)
            {
                CreatePhysics();
            }
        }

        new public void SetModel(Mesh m)
        {
            if (this.Model != m)
            {
                this.Model = m;
                CreatePhysics();
            }
        }

        private void CreatePhysics()
        {
            //Body bod = FarseerPhysics.Factories.BodyFactory.CreateRectangle(Utilities.PhysicsWorld, this.Model.BBox.Positive.X - this.Model.BBox.Negative.X, this.Model.BBox.Positive.Y - this.Model.BBox.Negative.Y, 1.0f);
            //this.Physics = bod.FixtureList[0];
        }
    }
}
