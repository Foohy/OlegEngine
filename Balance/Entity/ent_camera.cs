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
                if (Utilities.engine.Keyboard[Key.A])
                {
                    //this.BalanceEnt.Physics.Body.ApplyTorque(PushForce);
                }
                if (Utilities.engine.Keyboard[Key.D])
                {
                    //this.BalanceEnt.Physics.Body.ApplyTorque(-PushForce);
                }
            }
        }

        float RectWidthPerc = 0.4f;
        void GUIManager_PostDrawHUD(EventArgs e)
        {
            Vector2 pos = new Vector2(Utilities.engine.Width * RectWidthPerc, 0);
            Vector2 dimensions = new Vector2(Utilities.engine.Width * (1 - (2 * RectWidthPerc)), 20);
            Surface.DrawRect(pos, dimensions );

            StateText.SetPos(((Utilities.engine.Width) / 2) - (StateText.GetTextLength() / 2), pos.Y);
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

            GameWindow window = Utilities.engine;

            float multiplier = 8;
            if (window.Keyboard[Key.LShift])
                multiplier = 20;

            Vector3 NewPos = this.Position;
            //Calculate the new angle of the camera
            this.SetAngle(this.Angles + new Angle(Input.deltaY / -15f, Input.deltaX / 15f, 0));

            Vector3 Forward, Right, Up;
            this.Angles.AngleVectors(out Forward, out Up, out Right);

            //Calculate the new position
            if (window.Keyboard[Key.W])
            {
                NewPos += Forward * (float)Utilities.ThinkTime * multiplier;
            }

            if (window.Keyboard[Key.S])
            {
                NewPos -= Forward * (float)Utilities.ThinkTime * multiplier;
            }

            if (window.Keyboard[Key.D])
            {
                NewPos -= Right * (float)Utilities.ThinkTime * multiplier;
            }

            if (window.Keyboard[Key.A])
            {
                NewPos += Right * (float)Utilities.ThinkTime * multiplier;
            }

            if (window.Keyboard[Key.Space])
            {
                if (window.Keyboard[Key.ControlLeft])
                {
                    NewPos.Y -= (float)Utilities.ThinkTime * multiplier;
                }
                else
                {
                    NewPos.Y += (float)Utilities.ThinkTime * multiplier;
                }
            }


            this.SetPos(NewPos, false);

            View.SetPos(this.Position);
            View.SetAngles(this.Angles);

            Input.LockMouse = true;
        }

        private void PoleView()
        {
            View.SetPos(new Vector3(0, 8, 20));
            View.SetAngles(new Angle(0, -90, 0));
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
