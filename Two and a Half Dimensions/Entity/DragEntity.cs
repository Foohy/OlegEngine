using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Two_and_a_Half_Dimensions.Entity
{
    class DragEntity : BaseEntity 
    {
        public override void Init()
        {
            this.Mat = Resource.GetMaterial("Resources/Materials/speaker.png");
            this.drawMode = OpenTK.Graphics.OpenGL.BeginMode.Triangles;
            this.Model = Resource.GetMesh("Resources/Models/speaker.obj");
        }
    }
}
