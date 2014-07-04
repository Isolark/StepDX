using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace StepDX
{
    class Flag:Polygon
    {
        // Object position
        private Vector2 p = new Vector2(0, 0);
        public Vector2 P { set { p = value; } get { return p; } }

        // The texture
        private Texture texture = null;

        override public List<Vector2> Vertices { get { return verticesB; } }

        public Flag()
        {
            // Head triangle  offset=0    
            verticesB.Add(new Vector2(136f, 1f));
            verticesB.Add(new Vector2(137f, 1f));    
            verticesB.Add(new Vector2(137f, 1.5f));
                  
        
            // Body   offset=3
            verticesB.Add(new Vector2(137f, 0)); 
            verticesB.Add(new Vector2(137f, 1.5f));
            verticesB.Add(new Vector2(137.25f, 1.5f));
            verticesB.Add(new Vector2(137.25f, 0));
            
                        
        }


         override public void Render(Device device)
         {
            if (verticesV == null)
            {
                verticesV = new VertexBuffer(typeof(CustomVertex.PositionColoredTextured),    // Type of vertex
                   verticesB.Count,      // How many
                   device, // What device
                   0,      // No special usage
                   CustomVertex.PositionColoredTextured.Format,
                   Pool.Managed);

                texture = TextureLoader.FromFile(device, "../../textures/earth.bmp");
            }

            GraphicsStream gs = verticesV.Lock(0, 0, 0);// Lock the background vertex list
            int clr = System.Drawing.Color.Transparent.ToArgb();   // Don't you love all those color names?

            foreach (Vector2 v in verticesB)
            {
                float tu = 0.5f + v.X / 1.8f;
                float tv = 1 - (v.Y - (-0.8f)) / 1.8f;
                gs.Write(new CustomVertex.PositionColoredTextured(v.X, v.Y , 0, clr, tu, tv));
            }

            verticesV.Unlock();
            device.SetTexture(0, texture);
            device.SetStreamSource(0, verticesV, 0);
            device.VertexFormat = CustomVertex.PositionColoredTextured.Format;

            // Head, one triangle
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);



            // Body, two triangles
            device.DrawPrimitives(PrimitiveType.TriangleFan, 3, 2);

            
            device.SetTexture(0, null);
        }
    
    }
}
