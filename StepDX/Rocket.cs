using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace StepDX
{
    class Rocket:PolygonTextured
    {
        // Object position
        private Vector2 p = new Vector2(0, 0);
        public Vector2 P { set { p = value; } get { return p; } }

        // The texture
        private Texture texture = null;


        public Rocket()
        {
            // Head triangle  offset=0
            verticesB.Add(new Vector2(7f, 2.6f));
            verticesB.Add(new Vector2(7.3f, 3f));
            verticesB.Add(new Vector2(7.6f, 2.6f));


            // Body   offset=3
            verticesB.Add(new Vector2(7f, 2.6f));
            verticesB.Add(new Vector2(7.6f, 2.6f));
            verticesB.Add(new Vector2(7.6f, 1.8f));
            verticesB.Add(new Vector2(7f, 1.8f));

            // Leg 1  offset=7
            verticesB.Add(new Vector2(6.7f, 1.8f));
            verticesB.Add(new Vector2(7f, 2f));
            verticesB.Add(new Vector2(7f, 1.8f));


            // Leg 2  offset=10
            verticesB.Add(new Vector2(7.6f, 2f));
            verticesB.Add(new Vector2(7.9f, 1.8f));
            verticesB.Add(new Vector2(7.6f, 1.8f));
            
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

                texture = TextureLoader.FromFile(device, "../../textures/stack.bmp");
            }

            GraphicsStream gs = verticesV.Lock(0, 0, 0);// Lock the background vertex list
            int clr = System.Drawing.Color.Transparent.ToArgb();   // Don't you love all those color names?

            foreach (Vector2 v in verticesB)
            {
                float tu = 0.5f + v.X / 1.8f;
                float tv = 1 - (v.Y - (-0.8f)) / 1.8f;
                gs.Write(new CustomVertex.PositionColored(v.X + p.X, v.Y + p.Y, 0, clr));
            }

            verticesV.Unlock();
            device.SetTexture(0, texture);
            device.SetStreamSource(0, verticesV, 0);
            device.VertexFormat = CustomVertex.PositionColoredTextured.Format;

            // Head, one triangle
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);

            // Body, two triangles
            device.DrawPrimitives(PrimitiveType.TriangleFan, 3, 2);

            // Legs
            device.DrawPrimitives(PrimitiveType.TriangleList, 7, 1);
            device.DrawPrimitives(PrimitiveType.TriangleList, 10, 1);
            device.SetTexture(0, null);
        }

    }
}
