using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace StepDX
{
    class Stand:PolygonTextured
    {

        // Vertex data
        private List<Vector2> verticesB = new List<Vector2>();
        private VertexBuffer verticesV = null;

        // Object position
        private Vector2 p = new Vector2(5, 1);
        public Vector2 P { set { p = value; } get { return p; } }

        // The texture
        private Texture texture = null;

        public override List<Vector2> Vertices { get { return verticesB; } }

        public Stand()
        {
            // Head triangle  offset=0
            verticesB.Add(new Vector2(-0.3f, 0.6f));
            verticesB.Add(new Vector2(0, 1));
            verticesB.Add(new Vector2(0.3f, 0.6f));

            // Neck   offset=3
            verticesB.Add(new Vector2(-0.1f, 0.5f));
            verticesB.Add(new Vector2(-0.1f, 0.6f));
            verticesB.Add(new Vector2(0.1f, 0.6f));
            verticesB.Add(new Vector2(0.1f, 0.5f));

            // Body   offset=7
            verticesB.Add(new Vector2(-0.5f, -0.5f));
            verticesB.Add(new Vector2(-0.5f, 0.5f));
            verticesB.Add(new Vector2(0.5f, 0.5f));
            verticesB.Add(new Vector2(0.5f, -0.5f));

            // Leg 1  offset=11
            verticesB.Add(new Vector2(-0.6f, -0.8f));
            verticesB.Add(new Vector2(-0.4f, -0.5f));
            verticesB.Add(new Vector2(-0.1f, -0.5f));
            verticesB.Add(new Vector2(-0.3f, -0.8f));

            // Leg 2  offset=15
            verticesB.Add(new Vector2(0.3f, -0.8f));
            verticesB.Add(new Vector2(0.1f, -0.5f));
            verticesB.Add(new Vector2(0.4f, -0.5f));
            verticesB.Add(new Vector2(0.6f, -0.8f));
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
                gs.Write(new CustomVertex.PositionColoredTextured(v.X + p.X, v.Y + p.Y, 0, clr, tu, tv));
            }

            verticesV.Unlock();

            device.SetTexture(0, texture);
            device.SetStreamSource(0, verticesV, 0);
            device.VertexFormat = CustomVertex.PositionColoredTextured.Format;

            // Head, one triangle
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);

            // Neck, two triangles
            device.DrawPrimitives(PrimitiveType.TriangleFan, 3, 2);

            // Body, two triangles
            device.DrawPrimitives(PrimitiveType.TriangleFan, 7, 2);

            // Legs
            device.DrawPrimitives(PrimitiveType.TriangleFan, 11, 2);
            device.DrawPrimitives(PrimitiveType.TriangleFan, 15, 2);

            device.SetTexture(0, null);
        }
    }
}
