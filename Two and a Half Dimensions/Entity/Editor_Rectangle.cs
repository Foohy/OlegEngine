using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace Two_and_a_Half_Dimensions.Entity
{
    class Editor_Rectangle : BaseEntity 
    {
        public Vector3 TopLeft { get; set; }
        public Vector3 BottomRight { get; set; }
    }
}
