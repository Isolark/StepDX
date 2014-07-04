using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace StepDX
{
    public class GameSprite: PolygonTextured
    {
        private Vector2 p = new Vector2(0, 0);  // Position
        private Vector2 v = new Vector2(0, 0);  // Velocity
        private Vector2 a = new Vector2(0, -9.8f);  // Acceleration

        public Vector2 P { set { p = value; } get { return p; } }
        public Vector2 V { set { v = value; } get { return v; } }
        public Vector2 A { set { a = value; } get { return a; } }

        private Vector2 pSave;  // Position
        private Vector2 vSave;  // Velocity
        private Vector2 aSave;  // Acceleration

        private int spriteNum = 0;
        private float spriteTime = 10000;
        private float spriteRate = 24;   // 6 per second

        int invc = 0;
        public int Invc { set { invc = value; } get { return invc; } }

        // Whether or not the player is jumping
        bool jumping = false;
        public bool Jumping { set { jumping = value; }  get { return jumping; } }

        // Only can limit jump if jump using spacebar (not boost or fall).
        bool jumpControl = true;
        public bool JumpControl { set { jumpControl = value; } get { return jumpControl; } }

        // Whether or not the player is in collision
        bool collide = false;
        public bool Collide { set { collide = value; } get { return collide; } }

        // Whether or not the player is on a moving platform
        bool onPlat = false;
        public bool OnPlat { set { onPlat = value; } get { return onPlat; } }

        // The player direction
        // return true right, false left
        bool dir = true;
        public bool Dir { set { dir = value; } get { return dir; } }

        // The player speed
        float speed = 2.5f;
        public float Speed { set { speed = value; } get { return speed; } }

        float maxspeed = 2.5f;
        public float Maxspeed { set { maxspeed = value; } get { return maxspeed; } }

        // The player kinetic energy for movement
        float kinetic = 0;
        public float Kinetic { set { kinetic = value; } get { return kinetic; } }

        public void SaveState()
        {
            pSave = p;
            vSave = v;
            aSave = a;
        }

        public void RestoreState()
        {
            p = pSave;
            v = vSave;
            a = aSave;
        }

        protected List<Vector2> verticesM = new List<Vector2>();  // The vertices

        public override List<Vector2> Vertices { get { return verticesM; } }

        public override void Advance(float dt)
        {
            // Euler steps
            if (!onPlat)
            {
                v.X += a.X * dt;
                v.Y += a.Y * dt;
            }
            p.X += v.X * dt;
            p.Y += v.Y * dt;

            spriteRate = 24;

            if (Dir)
            {
                spriteTime -= dt * speed/2;
            }
            else
            {
                spriteTime += dt * speed/2;
            }

            spriteNum = (int)(spriteTime * spriteRate) % 12; 

            // Create the texture vertices

            textureC.Clear();

            if (invc > 0)
            {
                textureC.Add(new Vector2((12 + spriteNum) * 0.04166666666667f, 0.25f));
                textureC.Add(new Vector2((12 + spriteNum) * 0.04166666666667f, 0.75f));
                textureC.Add(new Vector2(((12 + spriteNum) + 0.25f) * 0.04166666666667f, 1));
                textureC.Add(new Vector2(((12 + spriteNum) + 0.75f) * 0.04166666666667f, 1));
                textureC.Add(new Vector2(((12 + spriteNum) + 1) * 0.04166666666667f, 0.75f));
                textureC.Add(new Vector2(((12 + spriteNum) + 1) * 0.04166666666667f, 0.25f));
                textureC.Add(new Vector2(((12 + spriteNum) + 0.75f) * 0.04166666666667f, 0));
                textureC.Add(new Vector2(((12 + spriteNum) + 0.25f) * 0.04166666666667f, 0));

                //textureC.Add(new Vector2((12 + spriteNum) * 0.04166666666667f, 1));
                //textureC.Add(new Vector2((12 + spriteNum) * 0.04166666666667f, 0));
                //textureC.Add(new Vector2((12 + (spriteNum + 1)) * 0.04166666666667f, 0));
                //textureC.Add(new Vector2((12 + (spriteNum + 1)) * 0.04166666666667f, 1));
            }
            else
            {
                //textureC.Add(new Vector2(spriteNum * 0.04166666666667f, 1));
                //textureC.Add(new Vector2(spriteNum * 0.04166666666667f, 0));
                //textureC.Add(new Vector2((spriteNum + 1) * 0.04166666666667f, 0));
                //textureC.Add(new Vector2((spriteNum + 1) * 0.04166666666667f, 1));

                textureC.Add(new Vector2(spriteNum * 0.04166666666667f, 0.25f));
                textureC.Add(new Vector2(spriteNum * 0.04166666666667f, 0.75f));
                textureC.Add(new Vector2((spriteNum + 0.25f) * 0.04166666666667f, 1));
                textureC.Add(new Vector2((spriteNum + 0.75f) * 0.04166666666667f, 1));
                textureC.Add(new Vector2((spriteNum + 1) * 0.04166666666667f, 0.75f));
                textureC.Add(new Vector2((spriteNum + 1) * 0.04166666666667f, 0.25f));
                textureC.Add(new Vector2((spriteNum + 0.75f) * 0.04166666666667f, 0));
                textureC.Add(new Vector2((spriteNum + 0.25f) * 0.04166666666667f, 0));
            }

            // Move the vertices
            verticesM.Clear();
            foreach (Vector2 x in verticesB)
            {
                verticesM.Add(new Vector2(x.X + p.X, x.Y + p.Y));
            }
        }
    }
}
