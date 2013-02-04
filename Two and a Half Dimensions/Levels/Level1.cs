using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using FarseerPhysics;
using FarseerPhysics.Dynamics;

using Two_and_a_Half_Dimensions.Entity;

namespace Two_and_a_Half_Dimensions.Levels
{
    class Level1 : LevelBase
    {
        Mesh levelmodel;
        bool SetShadow = true;
        Vector3 Angle = new Vector3();
        Vector3 Pos = new Vector3();
        public override void Preload()
        {
            levelmodel = Resource.GetMesh("Levels/level1.obj");
            levelmodel.mat = Resource.GetMaterial("engine/white.png", "default_lighting");

            //Create the physics mesh on the ground
            Body level = FarseerPhysics.Factories.BodyFactory.CreateRectangle(LevelManager.physWorld, 1000, 100, 1.0f);
            level.Position = new Microsoft.Xna.Framework.Vector2(-500, -52);
            level.BodyType = BodyType.Static;


            BaseEntity ent = EntManager.Create<DepthScreen>();
            ent.Spawn();

            Utilities.window.ply.SetPos(new Vector3(0, 0, 0));
            Utilities.window.shadows.Enable();
            Utilities.window.Keyboard.KeyDown += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyDown);
            Utilities.window.shadows.SetLights += new ShadowTechnique.SetLightsHandler(shadows_SetLights);
        }

        void Keyboard_KeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.Key == OpenTK.Input.Key.F2)
            {
                SetShadow = !SetShadow;
            }
            if (e.Key == OpenTK.Input.Key.Q)
            {
                Entity.TestBall ball = (Entity.TestBall)Entity.EntManager.Create<Entity.TestBall>();
                //ball.radius = rand.Next(0, 3000) / (float)1000;
                ball.Spawn();
                ball.SetPos(new Vector2(Player.ply.Pos.X, Player.ply.Pos.Y + 3.0f));
            }
        }

        public override void Think(FrameEventArgs e)
        {
            base.Think(e);

            Angle = new Vector3((float)Math.Cos( Utilities.Time / 10), -(float)Math.Abs(Math.Sin( Utilities.Time / 10)), 0.0f);


            //Create a camera matrix
            //Matrix4 shadowmat = Matrix4.LookAt(Pos - (Angle * 70), Pos + Angle - (Angle * 70), Vector3.UnitY);
            //Utilities.window.shadows.SetLightMatrix(shadowmat);
        }

        public override void Draw(FrameEventArgs e)
        {
            /*
            SpotLight[] sp = new SpotLight[1];
            sp[0].AmbientIntensity = 0.4f;
            sp[0].Color = new Vector3(1.0f, 1.0f, 1.0f);
            sp[0].Constant = 1.0f;
            sp[0].Cutoff = 20.0f;
            sp[0].Direction = Angle;
            sp[0].Position = Pos - (Angle * 70);

            Utilities.window.effect.SetSpotlights(sp);
             * */
            Utilities.window.effect.SetLights += new LightingTechnique.SetLightsHandler(effect_SetLights);


            if (levelmodel != null)
            {
                levelmodel.Render(Matrix4.Identity);
            }
        }

        void effect_SetLights(object sender, EventArgs e)
        {
            //how well are events perfomance wise? are they good for stuff like this?

            if (SetShadow)
            {
                SpotLight[] sp = new SpotLight[1];
                sp[0].AmbientIntensity = 0.4f;
                sp[0].Color = new Vector3(1.0f, 1.0f, 1.0f);
                sp[0].Constant = 1.0f;
                sp[0].Cutoff = 20.0f;
                sp[0].Direction = Player.ply.ViewNormal;
                sp[0].Position = Player.ply.Pos;

                Utilities.window.effect.SetSpotlights(sp);
            }

        }

        void shadows_SetLights(object sender, EventArgs e)
        {
            if (SetShadow)
            {
                Pos = Player.ply.Pos;
                Matrix4 shadowmat = Matrix4.LookAt(Pos, Pos + Player.ply.ViewNormal, Vector3.UnitY);
                Utilities.window.shadows.SetLightMatrix(shadowmat);
            }
        }
    }
}
