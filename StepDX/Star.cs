using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace StepDX
{
    class Star: PolygonTextured
    {
        public Star(int pts)
        {
            Points = pts;
        }
    }
}
