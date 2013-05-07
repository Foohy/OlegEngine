using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OlegEngine;
using OlegEngine.Entity;
using OlegEngine.GUI;

using OpenTK;
using OpenTK.Input;

namespace Balance.Entity
{
    class ent_camera : BaseEntity 
    {
        Vector2d CamAngle = new Vector2d();
        Text StateText;
        BaseEntity BalanceEnt;
        float PushForce = 20.0f;

        public override void Init()
        {
            this.ShouldDraw = false; //Don't draw the player entity itself

            BalanceLevel.OnStateChange += new Action<BalanceLevel.LevelState>(BalanceLevel_OnStateChange);
            GUIManager.PostDrawHUD += new GUIManager.OnDrawHUD(GUIManager_PostDrawHUD);
            StateText = new Text("debug", "WAITING");
            StateText.SetColor(0, 0, 0);
        }

        void BalanceLevel_OnStateChange(BalanceLevel.LevelState state)
        {
            StateText.SetText(state.ToString());
        }

        public override void Think()
        {
            if (BalanceLevel.CurrentState == BalanceLevel.LevelState.WAITING)
            {
                StateText.SetText( String.Format("{0} ({1,3:N0} seconds left)", BalanceLevel.CurrentState.ToString(), BalanceLevel.Timer - Utilities.Time ) );
            }

            //Think about controlling the pole
            //Not an innuendo
            if (this.BalanceEnt != null)
            {
                if (Utilities.window.Keyboard[Key.A])
                {
                    this.BalanceEnt.Physics.Body.ApplyTorque(PushForce);
                }
                if (Utilities.window.Keyboard[Key.D])
                {
                    this.BalanceEnt.Physics.Body.ApplyTorque(-PushForce);
                }
            }
        }

        float RectWidthPerc = 0.4f;
        void GUIManager_PostDrawHUD(EventArgs e)
        {
            Vector2 pos = new Vector2(Utilities.window.Width * RectWidthPerc, 0);
            Vector2 dimensions = new Vector2(Utilities.window.Width * (1 - (2 * RectWidthPerc)), 20);
            Surface.DrawRect(pos, dimensions );

            StateText.SetPos(((Utilities.window.Width) / 2) - (StateText.GetTextLength() / 2), pos.Y);
            StateText.Draw();
        }

        //Since we're going to be the default source of where to point the camera, we have our own dedicated function for it
        public void CalcView()
        {
            if (this.BalanceEnt != null)
            {
                this.PoleView();
                return;
            }

            GameWindow window = Utilities.window;

            float multiplier = 8;
            if (window.Keyboard[Key.LShift])
                multiplier = 20;

            Vector3 NewPos = this.Position;

            if (window.Keyboard[Key.W])
            {
                NewPos.X += (float)Math.Cos(CamAngle.X) * (float)Utilities.Frametime * multiplier;
                NewPos.Y += (float)Math.Sin(CamAngle.Y) * (float)Utilities.Frametime * multiplier;
                NewPos.Z += (float)Math.Sin(CamAngle.X) * (float)Utilities.Frametime * multiplier;
            }

            if (window.Keyboard[Key.S])
            {
                NewPos.X -= (float)Math.Cos(CamAngle.X) * (float)Utilities.Frametime * multiplier;
                NewPos.Y -= (float)Math.Sin(CamAngle.Y) * (float)Utilities.Frametime * multiplier;
                NewPos.Z -= (float)Math.Sin(CamAngle.X) * (float)Utilities.Frametime * multiplier;
            }

            if (window.Keyboard[Key.D])
            {
                NewPos.X += (float)Math.Cos(CamAngle.X + Math.PI / 2) * (float)Utilities.Frametime * multiplier;
                NewPos.Z += (float)Math.Sin(CamAngle.X + Math.PI / 2) * (float)Utilities.Frametime * multiplier;
            }

            if (window.Keyboard[Key.A])
            {
                NewPos.X -= (float)Math.Cos(CamAngle.X + Math.PI / 2) * (float)Utilities.Frametime * multiplier;
                NewPos.Z -= (float)Math.Sin(CamAngle.X + Math.PI / 2) * (float)Utilities.Frametime * multiplier;
            }


            if (window.Keyboard[Key.Space])
            {
                if (window.Keyboard[Key.ControlLeft])
                {
                    NewPos.Y -= (float)Utilities.Frametime * multiplier;
                }
                else
                {
                    NewPos.Y += (float)Utilities.Frametime * multiplier;
                }
            }

            CamAngle += new Vector2d(Input.deltaX / 350f, Input.deltaY / -350f);
            CamAngle = new Vector2d((float)CamAngle.X, Utilities.Clamp((float)CamAngle.Y, 1.0f, -1.0f)); //Clamp it because I can't math correctly

            Input.LockMouse = true;
            this.SetPos(NewPos, false);
            this.SetAngle(new Vector3((float)CamAngle.X, (float)CamAngle.Y, 0));

            View.SetPos(this.Position);
            View.SetAngles(this.Angle);
        }

        private void PoleView()
        {
            View.SetPos(new Vector3(0, 8, 20));
            View.SetAngles(new Vector3((float)Math.PI / -2, 0, 0));
        }

        public void SetBalanceEntity(BaseEntity balance)
        {
            BalanceEnt = balance;
        }

        public override void Draw()
        {
            
        }
    }
}
