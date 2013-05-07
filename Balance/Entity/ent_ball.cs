using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OlegEngine;
using OlegEngine.Entity;

using OpenTK;

using FarseerPhysics.Dynamics;

namespace Balance.Entity
{
    class ent_ball : BaseEntity
    {
        public float Radius = 1.0f;
        public override void Init()
        {
            this.Model = Resource.GetMesh("circle.obj");
            this.Mat = Resource.GetMaterial("models/circle");
            this.Scale = new Vector3(Radius);

            Body bod = new Body(Utilities.PhysicsWorld);
            bod.BodyType = BodyType.Dynamic;
            FarseerPhysics.Collision.Shapes.CircleShape circleshape = new FarseerPhysics.Collision.Shapes.CircleShape(Radius, 0.1f);
            this.Physics = bod.CreateFixture(circleshape);
            this.Physics.Body.Position = new Microsoft.Xna.Framework.Vector2(this.Position.X, this.Position.Y);
            this.Physics.Body.AngularVelocity = (float)Utilities.Rand.NextDouble( -3, 3);
        }
    }
}
