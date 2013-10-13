using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using FarseerPhysics.Common;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;

using OlegEngine;
using OlegEngine.Entity;

namespace Gravity_Car.Entity
{
    class ent_testball : BaseEntity 
    {
        public float Rotation = 0;
        public float radius = 1.0f;
        public override void Init()
        {
            this.Model = Resource.GetMesh("engine/ball.obj");

            this.Material = Resource.GetMaterial("levels/dirt");
            this.Material.Properties.SpecularPower = 32.0f;
            this.Material.Properties.SpecularIntensity = 5.0f;
            //this.Mat.SetShader("default");
           // this.drawMode = OpenTK.Graphics.OpenGL.BeginMode.Triangles;
            this.SetPos(new Vector3(0, 3.0f, 0.0f));


            Body bod = new Body(Utilities.PhysicsWorld);
            bod.BodyType = BodyType.Dynamic;
            FarseerPhysics.Collision.Shapes.CircleShape circleshape = new FarseerPhysics.Collision.Shapes.CircleShape(radius, 0.1f);
            this.Physics = bod.CreateFixture(circleshape);
            this.Physics.Body.Position = new Microsoft.Xna.Framework.Vector2(30, 10);
            this.Physics.Body.AngularVelocity = -1f;
        }

    }
}
