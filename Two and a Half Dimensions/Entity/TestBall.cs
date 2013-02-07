using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using FarseerPhysics.Common;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;

namespace Two_and_a_Half_Dimensions.Entity
{
    class TestBall : BaseEntity 
    {
        public float Rotation = 0;
        public float radius = 1.0f;
        public override void Init()
        {
            this.Model = Resource.GetMesh("ball.obj");

            this.Mat = Resource.GetMaterial("levels/dirt");
            this.Mat.Properties.SpecularPower = 32.0f;
            this.Mat.Properties.SpecularIntensity = 5.0f;
            //this.Mat.SetShader("default");
           // this.drawMode = OpenTK.Graphics.OpenGL.BeginMode.Triangles;
            this.SetPos(new Vector3(0, 3.0f, -3.0f));


            Body bod = new Body(Levels.LevelManager.physWorld);
            bod.BodyType = BodyType.Dynamic;
            FarseerPhysics.Collision.Shapes.CircleShape circleshape = new FarseerPhysics.Collision.Shapes.CircleShape(radius, 0.1f);
            this.Physics = bod.CreateFixture(circleshape);
            this.Physics.Body.Position = new Microsoft.Xna.Framework.Vector2(30, 10);
            this.Physics.Body.AngularVelocity = -1f;
        }

        public override void Think()
        {
            //SetPos( new OpenTK.Vector3( (float)Math.Sin(Utilities.Time), Position.Y, Position.Z ));
            //Rotation = (float)Utilities.Time;
            //Console.WriteLine(Physics.Body.Rotation);

            this.SetAngle(Physics.Body.Rotation);
        }

    }
}
