using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OlegEngine;
using OlegEngine.Entity;

using OpenTK;

namespace Balance.Entity
{
    class ent_ball : BaseEntity
    {
        public float Radius = 1.0f;
        public override void Init()
        {
            this.Model = Resource.GetMesh("circle.obj");
            this.Material = Resource.GetMaterial("models/circle");
            this.Scale = new Vector3(Radius);

        }
    }
}
