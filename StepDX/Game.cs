using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace StepDX
{
    public partial class Game : Form
    {
        /// <summary>
        /// The DirectX device we will draw on
        /// </summary>
        private Device device = null;

        /// <summary>
        /// Vertex buffer for our drawing
        /// </summary>
        private VertexBuffer vertices = null;

        /// <summary>
        /// The background image class
        /// </summary>
        private Background background = null;

        /// <summary>
        /// Initialize game
        /// </summary>
        /// <returns>true if game entered</returns>
        private bool gamestarted = false;

        private Flag flag = new Flag();

        /// <summary>
        /// End Game
        /// </summary>
        /// <returns>true if timer ends or goal reached</returns>
        private bool gameover = false;
        
        /// <summary>
        /// Datetime for game timer
        /// </summary>
        private DateTime mTime;

        private int endIn;

        /// <summary>
        /// Main game sounds
        /// </summary>
        private GameSounds gamesounds = null;

        /// <summary>
        /// The collision testing subsystem
        /// </summary>
        Collision collision = new Collision();
        

        /// <summary>
        /// Initialize the Direct3D device for rendering
        /// </summary>
        /// <returns>true if successful</returns>
        private bool InitializeDirect3D()
        {
            try
            {
                // Now let's setup our D3D stuff
                PresentParameters presentParams = new PresentParameters();
                presentParams.Windowed = true;
                presentParams.SwapEffect = SwapEffect.Discard;

                device = new Device(0, DeviceType.Hardware, this, CreateFlags.SoftwareVertexProcessing, presentParams);
            }
            catch (DirectXException)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// All of the polygons that make up our world
        /// </summary>
        List<Polygon> world = new List<Polygon>();

        //private bool jumping = false;

        /// <summary>
        /// Our player sprite
        /// </summary>
        GameSprite player = new GameSprite();

        private int points = 0;
        private int timer = 200;
        private int lasttimer = 0;

        /// <summary>
        /// What the last time reading was
        /// </summary>
        private long lastTime;
        /// <summary>
        /// A stopwatch to use to keep track of time
        /// </summary>
        private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        //Flag
        //Flag flag = new Flag();

        //Rocket
        //Rocket rocket = new Rocket();

        public void Render()
        {
            if (device == null)
                return;

            device.Clear(ClearFlags.Target, System.Drawing.Color.Blue, 1.0f, 0);

            int wid = Width;                            // Width of our display window
            int hit = Height;                           // Height of our display window.
            float aspect = (float)wid / (float)hit;     // What is the aspect ratio?

            device.RenderState.ZBufferEnable = false;   // We'll not use this feature
            device.RenderState.Lighting = false;        // Or this one...
            device.RenderState.CullMode = Cull.None;    // Or this one...

            float widP = playingH * aspect;         // Total width of window

            float winCenter = player.P.X;
            if (winCenter - widP / 2 < 0)
                winCenter = widP / 2;
            else if (winCenter + widP / 2 > playingW)
                winCenter = playingW - widP / 2;

            device.Transform.Projection = Matrix.OrthoOffCenterLH(winCenter - widP / 2,
                                                                  winCenter + widP / 2,
                                                                  0, playingH, 0, 1);
            //Begin the scene
            device.BeginScene();

            // Render the background
            background.Render();

            // Render the player
            player.Render(device);
            
            // Render flag
            flag.Render(device);

            // Render rocket
            //rocket.Render(device);

            foreach (Polygon p in world)
            {
                p.Render(device);
            }
          

            //End the scene
            device.EndScene();
            device.Present();
        }

        /// <summary>
        /// Height of our playing area (meters)
        /// </summary>
        private float playingH = 4;

        /// <summary>
        /// Width of our playing area (meters)
        /// </summary>
        private float playingW = 128;

        protected override void OnKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                this.Close(); // Esc was pressed

            if (gamestarted)
            {
                if (e.KeyCode == Keys.Right)
                {
                    player.Dir = true;
                }
                else if (e.KeyCode == Keys.Left)
                {
                    player.Dir = false;
                }
                if (e.KeyCode == Keys.Space)
                {
                    if (!player.Jumping)
                    {
                        Vector2 v = player.V;
                        v.Y = 5 + player.Kinetic;
                        player.V = v;
                        player.A = new Vector2(0, -9.8f);
                        player.Jumping = true;
                        player.JumpControl = true;
                        player.OnPlat = false;
                        player.Kinetic = 0;

                        gamesounds.Jump();
                    }
                }
                if (e.KeyCode == Keys.Enter) //PAUSE THE GAME
                {
                    gamestarted = false;
                    gamesounds.MusicStop();
                    stopwatch.Stop();
                }

                // Mute test
                if (e.KeyCode == Keys.Back)
                {
                    gamesounds.MusicMute();
                }
            }
            else
            {
                if (e.KeyCode == Keys.Enter) //START/UNPAUSE THE GAME
                {
                    if (gameover)
                    {
                        Application.Restart();
                    }
                    else
                    {
                        gamestarted = true;
                        stopwatch.Start();
                    }
                }
            }
        }

        protected override void OnKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            if (gamestarted)
            {
                if (e.KeyCode == Keys.Space)
                {
                    Vector2 v = player.V;

                    if (v.Y > 0 && player.Jumping && player.JumpControl)
                    {
                        v.Y = v.Y / 5.0f;
                        player.V = v;
                        player.A = new Vector2(0, -9.8f);
                    }
                }
            }
        }

        /// <summary>
        /// Advance the game in time
        /// </summary>
        public void Advance()
        {   
            long time = stopwatch.ElapsedMilliseconds;
            float delta = (time - lastTime) * 0.001f;       // Delta time in milliseconds
            lastTime = time;

            if (gamestarted)
            {
                if (stopwatch.Elapsed.Seconds != lasttimer && timer > 0)
                {
                    timer--;
                    lasttimer = stopwatch.Elapsed.Seconds;
                    mTime.AddSeconds(-stopwatch.Elapsed.Seconds).ToString("mm:ss");
                }

                if (timer == 0)
                {
                    GameOver();
                }
                if (endIn > 0)
                {
                    endIn--;
                    if (endIn == 0)
                    {
                        points += timer;
                        GameOver();
                    }
                }

                //End game produce score without time bonus
                lblTime.Text = "Time: " + timer.ToString();

                lblScore.Text = "Score: " + points;
                gamesounds.Music();
                // How much time change has there been?

                while (delta > 0)
                {
                    float step = delta;
                    if (step > 0.05f)
                        step = 0.05f;

                    float maxspeed = Math.Max(Math.Abs(player.V.X), Math.Abs(player.V.Y));
                    if (maxspeed > 0)
                    {
                        step = (float)Math.Min(step, 0.05 / maxspeed);
                    }

                    foreach (Polygon p in world)
                        p.Advance(step);

                    player.Advance(step);

                    bool currOnPlat = false;
                    bool hCol = false;

                    foreach (Polygon p in world)
                    {
                        if (p.Deleted)
                            continue;

                        // Test for collision with Star objects
                        if (p.GetType().Name == "FinalStar")
                        {
                            if (collision.Test(player, p))
                            {
                                float depth = collision.P1inP2 ?
                                         collision.Depth : -collision.Depth;
                                player.P = player.P + collision.N * depth;
                                Vector2 v = player.V;

                                if (collision.N.Y != 0 || collision.N.X != 0)
                                {
                                    points += p.Points;
                                    endIn = 5;
                                    gamesounds.Goal();
                                    p.Delete();
                                }
                            }
                        }
                        else if (p.GetType().Name == "Star")
                        {
                            if (collision.Test(player, p))
                            {
                                float depth = collision.P1inP2 ?
                                         collision.Depth : -collision.Depth;
                                player.P = player.P + collision.N * depth;
                                Vector2 v = player.V;

                                if (collision.N.Y != 0 || collision.N.X != 0)
                                {
                                    points += p.Points;
                                    gamesounds.GetStar(p.Points);
                                    p.Delete();
                                }
                            }
                        }
                        else if (p.GetType().Name == "Spike")
                        {
                            if (collision.Test(player, p))
                            {
                                float depth = collision.P1inP2 ?
                                         collision.Depth : -collision.Depth;
                                player.P = player.P + collision.N * depth;

                                if (collision.N.X != 0 || collision.N.Y != 0)
                                {
                                    if (player.Invc == 0)
                                    {
                                        player.Invc = -5;
                                    }
                                    else if (player.Invc == -1)
                                    {
                                        player.Invc = 100;
                                        points -= 10;

                                        gamesounds.Hurt();
                                        Vector2 v3 = player.V;
                                        v3.Y = 7;
                                        player.A = new Vector2(0, -9.8f);
                                        player.Jumping = true;
                                        player.JumpControl = false;
                                        player.OnPlat = false;
                                        player.Kinetic = 0;

                                        player.V = v3;
                                    }

                                }
                            }
                        }
                        // Test for collision with Horizontal Boost objects
                        else if (p.GetType().Name == "HBoost")
                        {
                            if (collision.Test(player, p))
                            {
                                float depth = collision.P1inP2 ?
                                         collision.Depth : -collision.Depth;
                                player.P = player.P + collision.N * depth;
                                Vector2 v = player.V;

                                if (collision.N.Y > 0)      //If N is positive, know you are standing on something & can jump again.
                                {
                                    player.Jumping = false;
                                    player.JumpControl = false;
                                    player.Speed = 8f;
                                    gamesounds.Boost();
                                    v.Y = 0;
                                    //player.A = new Vector2(, 0);
                                }
                                else if (collision.N.Y != 0 && player.V.Y > 0)
                                {
                                    v.Y = 0;
                                    
                                }
                                else if (collision.N.X != 0 &&
                                    ((player.P.X < p.Leftedge) && player.Dir) ||
                                    ((player.P.X > p.Rightedge) && !player.Dir))
                                {
                                    hCol = true;
                                }

                                player.V = v;
                                player.Advance(0);
                            }
                        }
                        // Test for collision with Vertical Boost objects
                        else if (p.GetType().Name == "VBoost")
                        {
                            if (collision.Test(player, p))
                            {
                                float depth = collision.P1inP2 ?
                                         collision.Depth : -collision.Depth;
                                player.P = player.P + collision.N * depth;
                                Vector2 v = player.V;

                                if (collision.N.Y > 0)      //If N is positive, know you are standing on something & can jump again.
                                {
                                    v.Y = 8;
                                    player.V = v;
                                    player.A = new Vector2(0, -9.8f);
                                    player.Jumping = true;
                                    player.JumpControl = false;
                                    player.OnPlat = false;
                                    player.Kinetic = 0;

                                    gamesounds.Boost();
                                }
                                else if (collision.N.Y != 0 && player.V.Y > 0)
                                {
                                    v.Y = 0;
                                    player.V = v;
                                }
                                else if (collision.N.X != 0 &&
                                    ((player.P.X < p.Leftedge) && player.Dir) ||
                                    ((player.P.X > p.Rightedge) && !player.Dir))
                                {
                                    hCol = true;
                                }

                                player.Advance(0);
                            }
                        }
                        // Test for collision with moving Platform objects
                        else if (p.GetType().Name == "Platform")
                        {
                            if (collision.Test(player, p))
                            {
                                float depth = collision.P1inP2 ?
                                         collision.Depth : -collision.Depth;
                                player.P = player.P + collision.N * depth;
                                Vector2 v = player.V;

                                if (collision.N.Y > 0)      //If N is positive, know you are standing on something & can jump again.
                                {
                                    player.Jumping = false;
                                    player.JumpControl = false;
                                    player.OnPlat = true;
                                    currOnPlat = true;
                                    player.Kinetic = p.Vy;
                                    player.A = new Vector2(0, 0);
                                }
                                else if (collision.N.Y != 0 && player.V.Y > 0)
                                {
                                    v.Y = 0;
                                    player.V = v;
                                }
                                else if (collision.N.X != 0 &&
                                    ((player.P.X < p.Leftedge) && player.Dir) ||
                                    ((player.P.X > p.Rightedge) && !player.Dir))
                                {
                                    player.V = v;
                                    hCol = true;
                                }

                                player.Advance(0);
                            }
                        }
                        else if (p.GetType().Name == "HPlatform")
                        {
                            if (collision.Test(player, p))
                            {
                                float depth = collision.P1inP2 ?
                                         collision.Depth : -collision.Depth;
                                player.P = player.P + collision.N * depth;
                                Vector2 v = player.V;

                                if (collision.N.Y > 0)      //If N is positive, know you are standing on something & can jump again.
                                {
                                    player.Jumping = false;
                                    player.JumpControl = false;
                                    player.OnPlat = true;
                                    currOnPlat = true;
                                    player.Kinetic = p.Vy;
                                    player.A = new Vector2(0, 0);
                                }
                                else if (collision.N.Y != 0 && player.V.Y > 0)
                                {
                                    v.Y = 0;
                                    player.V = v;
                                }
                                else if (collision.N.X != 0)
                                {
                                    player.V = v;
                                    hCol = true;
                                }

                                player.Advance(0);
                            }
                        }
                        else if(p.GetType().Name == "SpecPolyTextured")
                        {
                            if (collision.Test(player, p))
                            {
                                player.Collide = true;

                                float depth = collision.P1inP2 ?
                                      collision.Depth : -collision.Depth;
                                player.P = player.P + collision.N * depth;
                                Vector2 v = player.V;

                                if (collision.N.Y > 0)      //If N is positive, know you are standing on something & can jump again.
                                {
                                    player.Jumping = false;
                                    player.JumpControl = false;
                                    v.Y = 0;
                                    lblCollide.Text = p.GetType().Name;
                                }
                                else if (collision.N.Y != 0 && player.V.Y > 0)
                                {
                                    v.Y = 0;
                                    player.V = v;
                                }
                                else if (collision.N.X != 0)
                                {
                                    hCol = true;
                                }

                                player.V = v;
                                player.Advance(0);
                            }
                        }
                        else if (collision.Test(player, p))
                        {
                            player.Collide = true;

                            float depth = collision.P1inP2 ?
                                      collision.Depth : -collision.Depth;
                            player.P = player.P + collision.N * depth;
                            Vector2 v = player.V;

                            if (collision.N.Y > 0)      //If N is positive, know you are standing on something & can jump again.
                            {
                                player.Jumping = false;
                                player.JumpControl = false;
                                v.Y = 0;
                                lblCollide.Text = p.GetType().Name;
                            }
                            else if (collision.N.Y != 0 && player.V.Y > 0)
                            {
                                v.Y = 0;
                                player.V = v;
                            }
                            else if ((collision.N.X == 1 || collision.N.X == -1) &&
                                ((player.P.X < p.Leftedge) && player.Dir) ||
                                ((player.P.X > p.Rightedge) && !player.Dir))
                            {
                                hCol = true;
                            }

                            player.V = v;
                            player.Advance(0);
                        }
                    }

                    Vector2 p2 = player.P;
                    Vector2 v2 = player.V;

                    if (player.Dir)
                    {
                        if (hCol)
                        {
                            gamesounds.Hit();
                            player.Dir = false;
                            v2.X = -v2.X;

                        }
                        else
                            v2.X = player.Speed;
                    }
                    else
                    {
                        if (hCol)
                        {
                            gamesounds.Hit();
                            player.Dir = true;
                            v2.X = -v2.X;
                        }
                        else
                            v2.X = -player.Speed;
                    }

                    player.V = v2;

                    if (player.Speed > player.Maxspeed)
                    {
                        player.Speed -= 0.01f;
                        if (player.Speed < player.Maxspeed)
                            player.Speed = player.Maxspeed;
                    }
                    else if (player.Speed < player.Maxspeed)
                    {
                        player.Speed += 0.02f;
                        if (player.Speed > player.Maxspeed)
                            player.Speed = player.Maxspeed;
                    }

                    if (player.OnPlat)
                    {
                        if (currOnPlat == false)
                        {
                            player.OnPlat = false;
                            if (player.Dir == true)
                                player.V = new Vector2(player.Speed, 0);
                            else
                                player.V = new Vector2(player.Speed, 0);

                            player.Kinetic = 0;
                            player.A = new Vector2(0, -9.8f);
                        }
                    }
                    if (player.Invc > 0)
                        player.Invc -= 1;

                    else if(player.Invc < 0)
                        player.Invc += 1;

                    if (player.V.Y != 0 && !player.OnPlat)
                        player.Jumping = true;


                    if (player.P.Y < -0.6f)
                    {
                        if (player.Invc == 0)
                        {
                            points -= 10;
                            gamesounds.Hurt();
                            player.Invc = 100;
                        }

                        Vector2 v3 = player.V;
                        v3.Y = 7;
                        player.A = new Vector2(0, -9.8f);
                        player.Jumping = true;
                        player.JumpControl = false;
                        player.OnPlat = false;
                        player.Kinetic = 0;
                        
                        player.V = v3;       
                    }
                    delta -= step;
                }

                if (!player.Collide && player.Jumping)
                    lblCollide.Text = string.Empty;
            }
        }



        public Game()
        {
            InitializeComponent();

            if (!InitializeDirect3D())
                return;

            // Static datetime set to 5 minutes on 12/11/12
            //mTime = new DateTime(2012, 12, 11, 0, 5, 0);

            // We want the datetime at which the score is achieved
            mTime = DateTime.Now;

            // Player sprite
            Texture spritetexture = TextureLoader.FromFile(device, "../../sprites/playerball.png");
            player.Tex = spritetexture;
            player.AddVertex(new Vector2(-0.2f, 0.1f));
            player.AddTex(new Vector2(0, 0.75f));
            player.AddVertex(new Vector2(-0.2f, 0.3f));
            player.AddTex(new Vector2(0, 0.25f));
            player.AddVertex(new Vector2(-0.1f, 0.4f));
            player.AddTex(new Vector2(0.25f, 0));
            player.AddVertex(new Vector2(0.1f, 0.4f));
            player.AddTex(new Vector2(0.75f, 0));

            player.AddVertex(new Vector2(0.2f, 0.3f));
            player.AddTex(new Vector2(1, 0.25f));
            player.AddVertex(new Vector2(0.2f, 0.1f));
            player.AddTex(new Vector2(1, 0.75f));
            player.AddVertex(new Vector2(0.1f, 0));
            player.AddTex(new Vector2(0.75f, 1));
            player.AddVertex(new Vector2(-0.1f, 0));
            player.AddTex(new Vector2(0.25f, 1));

            player.Color = Color.Transparent;
            player.Transparent = true;
            player.P = new Vector2(0.5f, 4); // 0.5

            // Bar Menu Items
            lblScore.ForeColor = Color.RoyalBlue;
            lblScore.BackColor = Color.Black;
            lblTime.BackColor = Color.Black;
            lblTime.ForeColor = Color.RoyalBlue;
            lblScore.Text = "Score: 0";
            lblTime.Text = "Time: 200";
            lblCollide.Hide();

            // Boundaries of stage
            AddObstacle(-0.2f, 0, -0.2f, playingH + 2, Color.Empty);
            AddObstacle(playingW, playingW + 0.2f, -0.6f, playingH + 2, Color.Empty);

            //Floors. Should always be vertically -0.2 to 0.2f
            AddTexture(0, 42, -0.2f, 0.2f, Color.Transparent, "../../textures/ground2.bmp");
            AddTexture(49, 61.5f, -0.2f, 0.2f, Color.Transparent, "../../textures/ground2.bmp");
            AddTexture(69, 73, -0.2f, 0.2f, Color.Transparent, "../../textures/ground2.bmp");
            AddTexture(81.6f, 96, -0.2f, 0.2f, Color.Transparent, "../../textures/ground2.bmp");

            AddLRPlatform(62.5f, 63.5f, 0.6f, 0.8f, Color.Transparent, "../../textures/gear.bmp");
            AddRLPlatform(66.5f, 67.5f, 0.6f, 0.8f, Color.Transparent, "../../textures/gear.bmp");

            AddTexture(49.5f, 50.5f, 0.2f, 1.5f, Color.Transparent, "../../textures/brickscube.bmp");
            AddTexture(50.75f, 51.75f, 0.2f, 0.5f, Color.Transparent, "../../textures/brickscube.bmp");

            AddStar(62.9f, 1.1f, Color.Transparent, "../../textures/star.png");
            AddStar(63.7f, 1.8f, Color.Transparent, "../../textures/star.png");
            AddStar(64.6f, 3.2f, Color.Transparent, "../../textures/star.png");
            AddStar(65f, 3.5f, Color.Transparent, "../../textures/star.png");
            AddStar(65.4f, 3.2f, Color.Transparent, "../../textures/star.png");
            AddBlueStar(65f, 2.85f, Color.Transparent, "../../textures/bluestar.png");
            AddStar(65.9f, 1.8f, Color.Transparent, "../../textures/star.png");
            AddStar(66.7f, 1.1f, Color.Transparent, "../../textures/star.png");

            AddTextureLeftTri(71, 73, 0.2f, 2, Color.Transparent, "../../textures/earth.bmp");
            AddDiamond(70, 1.6f, Color.Transparent, "../../textures/earth.bmp");

            AddStar(71.5f, 3.3f, Color.Transparent, "../../textures/star.png");
            AddBlueStar(70, 3.5f, Color.Transparent, "../../textures/bluestar.png");
            AddStar(68.5f, 3.3f, Color.Transparent, "../../textures/star.png"); 

            AddPlatform(74.5f, 75.5f, 1f, 1.2f, Color.Transparent, "../../textures/gear.bmp");

            AddStar(75, 1.8f, Color.Transparent, "../../textures/star.png");

            AddTexture(76.8f, 77.2f, 0.8f, 2.8f, Color.Transparent, "../../textures/stack.bmp");
            AddTexture(77.8f, 78.2f, -1, 1.8f, Color.Transparent, "../../textures/stack.bmp");
            AddTexture(81f, 81.4f, 0.7f, 2.8f, Color.Transparent, "../../textures/stack.bmp");
            AddTexture(81.4f, 82f, 0.7f, 0.9f, Color.Transparent, "../../textures/metal.bmp");

            AddTexture(76.8f, 81.4f, 2.8f, 3, Color.Transparent, "../../textures/metal.bmp");
            AddTexture(77.8f, 80.2f, 1.6f, 1.8f, Color.Transparent, "../../textures/metal.bmp");

            AddBlueStar(78, 2.05f, Color.Transparent, "../../textures/bluestar.png");
            AddStar(78.5f, 2.25f, Color.Transparent, "../../textures/star.png");
            AddBlueStar(79f, 2.05f, Color.Transparent, "../../textures/bluestar.png");
            AddStar(79.5f, 2.25f, Color.Transparent, "../../textures/star.png");
            AddBlueStar(80, 2.05f, Color.Transparent, "../../textures/bluestar.png");

            AddHBoost(80.2f, 81.6f, 0, 0.2f, Color.Transparent, "../../textures/boost.bmp");

            AddBlueStar(78.55f, 1.1f, Color.Transparent, "../../textures/bluestar.png");
            AddBlueStar(79f, 0.95f, Color.Transparent, "../../textures/bluestar.png");
            AddBlueStar(79.7f, 0.7f, Color.Transparent, "../../textures/bluestar.png");

            AddDiamond(84, 0.4f, Color.Transparent, "../../textures/earth.bmp");
            AddDiamond(86, 1.2f, Color.Transparent, "../../textures/earth.bmp");
            AddDiamond(88, 2, Color.Transparent, "../../textures/earth.bmp");

            AddHBoost(90, 91f, 2.8f, 3f, Color.Transparent, "../../textures/boost.bmp");

            AddTexture(90.6f, 91, -1, 2.8f, Color.Transparent, "../../textures/stack.bmp");
            AddTexture(92.8f, 93.2f, 0.7f, 5f, Color.Transparent, "../../textures/stack.bmp");

            AddTextureSpike(91f, 91.6f, 2.8f, 3.1f, Color.Transparent, "../../textures/stack.bmp");
            AddTextureSpike(92.2f, 92.8f, 2.8f, 3.1f, Color.Transparent, "../../textures/stack.bmp");
            AddTextureSpike(91, 91.4f, 0.2f, 0.6f, Color.Transparent, "../../textures/stack.bmp");

            AddTexture(93.2f, 93.8f, 0.8f, 1f, Color.Transparent, "../../textures/metal.bmp");

            AddLRPlatform(94.4f, 95.4f, 1.5f, 1.7f, Color.Transparent, "../../textures/gear.bmp");
            AddPlatform(97, 98f, 1.9f, 2.1f, Color.Transparent, "../../textures/gear.bmp");

            AddTexture(99, 99.4f, -1, 3.5f, Color.Transparent, "../../textures/stack.bmp");

            AddStar(100.4f, 3.85f, Color.Transparent, "../../textures/star.png");
            AddStar(101.8f, 0.4f, Color.Transparent, "../../textures/star.png");
            AddStar(102.8f, 0.8f, Color.Transparent, "../../textures/star.png");

            AddHBoost(101.3f, 102.3f, 0, 0.2f, Color.Transparent, "../../textures/boost.bmp");
            AddVBoost(102.3f, 103.3f, 0, 0.2f, Color.Transparent, "../../textures/boostup.bmp");

            AddBlueStar(104.3f, 2.2f, Color.Transparent, "../../textures/bluestar.png");
            AddBlueStar(106.3f, 3.2f, Color.Transparent, "../../textures/bluestar.png");
            AddBlueStar(107.3f, 3.55f, Color.Transparent, "../../textures/bluestar.png");

            AddStar(113.5f, 0.4f, Color.Transparent, "../../textures/star.png");
            AddStar(114.5f, 0.8f, Color.Transparent, "../../textures/star.png");

            AddHBoost(113f, 114f, 0, 0.2f, Color.Transparent, "../../textures/boost.bmp");
            AddVBoost(114, 115, 0, 0.2f, Color.Transparent, "../../textures/boostup.bmp");

            AddBlueStar(116, 2.2f, Color.Transparent, "../../textures/bluestar.png");
            AddBlueStar(118, 3.2f, Color.Transparent, "../../textures/bluestar.png");
            AddBlueStar(119, 3.55f, Color.Transparent, "../../textures/bluestar.png");

            AddBlueStar(123f, 3.1f, Color.Transparent, "../../textures/bluestar.png");
            AddBlueStar(124f, 3.1f, Color.Transparent, "../../textures/bluestar.png");

            AddTexture(122, 125f, 2.6f, 2.9f, Color.Transparent, "../../textures/metal.bmp");
            AddTexture(124.5f, 125, -0.2f, 2.6f, Color.Transparent, "../../textures/stack.bmp");
            AddTexture(125, 128, 0.2f, 0.4f, Color.Transparent, "../../textures/metal.bmp");
            AddTexture(127.5f, 128, -0.2f, 6, Color.Transparent, "../../textures/stack.bmp");

            //Messages/title
            AddTexture(1.2f, 5.2f, 1, 3.2f, Color.Transparent, "../../images/title.png");
            AddStool(7.34f, 0, Color.Transparent, "../../textures/earth.bmp");
            AddTexture(9.25f, 12.25f, 2.75f, 3.5f, Color.Transparent, "../../images/starmessage.png");
            AddTexture(18.5f, 21.7f, 3.25f, 4, Color.Transparent, "../../images/boostmessage.png");

            //Regular platforms (If needed)
            /*AddObstacle(17, 18, 1.2f, 1.4f, Color.Crimson);*/

            //Textured platforms
            //Use brickscube or bricks for ground pieces (cube for taller pieces).
            //Use metal for floating platforms, or stack for floating large pieces.
            AddTextureLeftTri(17, 19, 0, 1.4f, Color.Transparent, "../../textures/brickscube.bmp");
            AddTextureRightTri(19, 21, 0, 0.7f, Color.Transparent, "../../textures/brickscube.bmp");

            //AddSept(5, 0.2f, Color.Transparent, "../../textures/stack.bmp");

            AddDiamond(25, 0.2f, Color.Transparent, "../../textures/earth.bmp");
            AddDiamond(30, 0.2f, Color.Transparent, "../../textures/earth.bmp");

            AddBlueStar(27.5f, 3, Color.Transparent, "../../textures/bluestar.png");

            AddStar(24.5f, 0.35f, Color.Transparent, "../../textures/star.png");
            AddStar(25.5f, 0.35f, Color.Transparent, "../../textures/star.png");
            AddStar(29.5f, 0.35f, Color.Transparent, "../../textures/star.png");
            AddStar(30.5f, 0.35f, Color.Transparent, "../../textures/star.png");

            AddStar(24.3f, 2.2f, Color.Transparent, "../../textures/star.png");
            AddStar(25f, 2.45f, Color.Transparent, "../../textures/star.png");
            AddStar(25.7f, 2.2f, Color.Transparent, "../../textures/star.png");
            AddStar(29.3f, 2.2f, Color.Transparent, "../../textures/star.png");
            AddStar(30f, 2.45f, Color.Transparent, "../../textures/star.png");
            AddStar(30.7f, 2.2f, Color.Transparent, "../../textures/star.png");

            AddBlueStar(22f, 3, Color.Transparent, "../../textures/bluestar.png");

            AddHBoost(33f, 34f, 1f, 1.2f, Color.Transparent, "../../textures/boost.bmp");
            AddTexture(35.6f, 36.6f, 0.2f, 0.5f, Color.Transparent, "../../textures/brickscube.bmp");

            AddTexture(8.5f, 13f, 1.8f, 2.1f, Color.Transparent, "../../textures/metal.bmp");
            AddTexture(14.45f, 15.45f, 2.9f, 3.2f, Color.Transparent, "../../textures/metal.bmp");

            AddTexture(43.5f, 44.5f, 0.8f, 1, Color.Transparent, "../../textures/metal.bmp");

            AddTextureSpike(43.5f, 43.75f, 1, 1.3f, Color.Transparent, "../../textures/stack.bmp");
            AddTextureSpike(43.75f, 44, 1, 1.3f, Color.Transparent, "../../textures/stack.bmp");
            AddTextureSpike(44f, 44.25f, 1, 1.3f, Color.Transparent, "../../textures/stack.bmp");
            AddTextureSpike(44.25f, 44.5f, 1, 1.3f, Color.Transparent, "../../textures/stack.bmp");

            //Moving platforms AddRLPlatform move up from right to left, LR vice versa
            AddPlatform(45, 46, 0.8f, 1.0f, Color.Transparent, "../../textures/gear.bmp");
            AddPlatform(48, 49, 0.8f, 1.0f, Color.Transparent, "../../textures/gear.bmp");
            AddRLPlatform(14.8f, 15.8f, 0.8f, 1.0f, Color.Transparent, "../../textures/gear.bmp");

            //Horizontal Boosters
            AddHBoost(22f, 23f, 0.2f, 0.4f, Color.Transparent, "../../textures/boost.bmp");

            //Vertical Boosters
            AddVBoost(42, 43f, 0.8f, 1.0f, Color.Transparent, "../../textures/boostup.bmp");

            //Star
            //To to keep same convention. About 0.1 above a platform if it's on one
            //width = 0.4f, height = 0.3f (otherwise they'll look bad).

            AddFinalStar(126.3f, 1.2f, Color.Transparent, "../../textures/greenstar.png");

            // Texture 1
            AddBlueStar(8.9f, 2.35f, Color.Transparent, "../../textures/bluestar.png");
            AddStar(9.5f, 2.35f, Color.Transparent, "../../textures/star.png");
            AddStar(10.1f, 2.35f, Color.Transparent, "../../textures/star.png");
            AddStar(10.7f, 2.35f, Color.Transparent, "../../textures/star.png");
            AddStar(11.3f, 2.35f, Color.Transparent, "../../textures/star.png");
            AddStar(11.9f, 2.35f, Color.Transparent, "../../textures/star.png");
            AddStar(12.5f, 2.35f, Color.Transparent, "../../textures/star.png");

            // Texture 2
            AddBlueStar(9.6f, 3.75f, Color.Transparent, "../../textures/bluestar.png");
            AddStar(10.2f, 3.75f, Color.Transparent, "../../textures/star.png");
            AddStar(10.8f, 3.75f, Color.Transparent, "../../textures/star.png");
            AddStar(11.4f, 3.75f, Color.Transparent, "../../textures/star.png");
            AddStar(12f, 3.75f, Color.Transparent, "../../textures/star.png");

            // Texture 3
            AddBlueStar(14.95f, 3.45f, Color.Transparent, "../../textures/bluestar.png");

            AddStar(42.5f, 3.3f, Color.Transparent, "../../textures/star.png");
            AddStar(45.5f, 2.9f, Color.Transparent, "../../textures/star.png");
            AddBlueStar(45.5f, 3.45f, Color.Transparent, "../../textures/bluestar.png");
            AddStar(48.5f, 3.5f, Color.Transparent, "../../textures/star.png");

            AddTexture(39.5f, 42f, 3.25f, 4f, Color.Transparent, "../../images/spikemessage.png");

            AddStar(41.5f, 4.2f, Color.Transparent, "../../textures/star.png");
            AddBlueStar(41f, 4.2f, Color.Transparent, "../../textures/bluestar.png");
            AddBlueStar(40f, 4.2f, Color.Transparent, "../../textures/bluestar.png");
            AddBlueStar(39.5f, 4.2f, Color.Transparent, "../../textures/bluestar.png");

            AddTexture(60, 62.75f, 3.25f, 4f, Color.Transparent, "../../images/timemessage.png");

            AddStar(60.5f, 4.2f, Color.Transparent, "../../textures/star.png");
            AddBlueStar(61f, 4.2f, Color.Transparent, "../../textures/bluestar.png");
            AddBlueStar(61.5f, 4.2f, Color.Transparent, "../../textures/bluestar.png");
            AddBlueStar(62f, 4.2f, Color.Transparent, "../../textures/bluestar.png");         

            AddVBoost(52f, 53f, 1.9f, 2.1f, Color.Transparent, "../../textures/boostup.bmp");
            AddVBoost(56f, 57f, 1.9f, 2.1f, Color.Transparent, "../../textures/boostup.bmp");
            AddHBoost(58, 59, 1.8f, 2f, Color.Transparent, "../../textures/boost.bmp");

            AddStar(52.5f, 2.3f, Color.Transparent, "../../textures/star.png");
            AddStar(56.5f, 2.3f, Color.Transparent, "../../textures/star.png");
            AddStar(56.5f, 2.7f, Color.Transparent, "../../textures/star.png");
            AddStar(58.5f, 2.7f, Color.Transparent, "../../textures/star.png");
            AddBlueStar(58.5f, 2.2f, Color.Transparent, "../../textures/bluestar.png");

            vertices = new VertexBuffer(typeof(CustomVertex.PositionColored), // Type of vertex
                                        4,      // How many
                                        device, // What device
                                        0,      // No special usage
                                        CustomVertex.PositionColored.Format,
                                        Pool.Managed);

            background = new Background(device, playingW, playingH);
            
            gamesounds = new GameSounds(FindForm());

            // Determine the last time
            stopwatch.Stop();
            lastTime = stopwatch.ElapsedMilliseconds;
        }
        /// <summary>
        /// The end of the game, by time or goal
        /// </summary>
        public void GameOver()
        {
            // User control for new game, high scores
            lblScore.Text = "Score: " + points.ToString();
            gameover = true;
            gamestarted = false;
            gamesounds.MusicStop();
            stopwatch.Stop();
            Score score = new Score(points, timer, mTime);
            score.write();
        }

        public void AddObstacle(float left, float right, float bottom, float top, Color clr)
        {
            Polygon obstacle = new Polygon();
            obstacle.AddVertex(new Vector2(left, bottom));
            obstacle.AddVertex(new Vector2(left, top));
            obstacle.AddVertex(new Vector2(right, top));
            obstacle.AddVertex(new Vector2(right, bottom));
            obstacle.Color = clr;
            world.Add(obstacle);
        }

        public void AddPlatform(float left, float right, float bottom, float top, Color x, string filename)
        {
            Platform pt = new Platform();
            TextureRectangle(left, right, bottom, top, x, filename, pt);
        }
        public void AddStool(float midX, float midY, Color x, string filename)
        {
            PolygonTextured pt = new PolygonTextured();
            TextureEquiTriangle(midX-0.4f, midX-.15f, midY+0.2f, midY+.5f, x, filename, pt);

            PolygonTextured pt2 = new PolygonTextured();
            TextureEquiTriangle(midX+.15f, midX+.4f, midY+0.2f, midY + .5f, x, filename, pt2);

            PolygonTextured pt3 = new PolygonTextured();
            TextureRectangle(midX - 0.5f, midX +0.5f, midY+.4f, midY + 0.6f, x, filename, pt3);
        }
        public void AddRLPlatform(float left, float right, float bottom, float top, Color x, string filename)
        {
            HPlatform ht = new HPlatform();
            ht.Dir = false;
            TextureRectangle(left, right, bottom, top, x, filename, ht);
        }
        public void AddLRPlatform(float left, float right, float bottom, float top, Color x, string filename)
        {
            HPlatform ht = new HPlatform();
            ht.Dir = true;
            TextureRectangle(left, right, bottom, top, x, filename, ht);
        }

        public void AddHBoost(float left, float right, float bottom, float top, Color x, string filename)
        {
            HBoost hb = new HBoost();
            TextureRectangle(left, right, bottom, top, x, filename, hb);

        }
        public void AddVBoost(float left, float right, float bottom, float top, Color x, string filename)
        {            
            VBoost vb = new VBoost();
            TextureRectangle(left, right, bottom, top, x, filename, vb);
        }

        public void AddStar(float midX, float midY, Color x, string filename)
        {
            Star st = new Star(5);
            TextureStar(midX, midY, x, filename, st);
        }
        public void AddBlueStar(float midX, float midY, Color x, string filename)
        {
            Star st = new Star(20);
            TextureStar(midX, midY, x, filename, st);       
        }
        public void AddFinalStar(float midX, float midY, Color x, string filename)
        {
            FinalStar st = new FinalStar(100);
            TextureFinalStar(midX, midY, x, filename, st);
        }
        public void AddDiamond(float midX, float midY, Color x, string filename)
        {
            SpecPolyTextured pt = new SpecPolyTextured();
            TextureDiamond(midX, midY, x, filename, pt);
        }
        public void AddSept(float midX, float midY, Color x, string filename)
        {
            SpecPolyTextured pt = new SpecPolyTextured();
            TextureSept(midX, midY, x, filename, pt);
        }
        public void AddTexture(float left, float right, float bottom, float top, Color x, string filename)
        {
            PolygonTextured pt = new PolygonTextured();
            TextureRectangle(left, right, bottom, top, x, filename, pt);
        }
        public void AddTextureLeftTri(float left, float right, float bottom, float top, Color x, string filename)
        {
            PolygonTextured pt = new PolygonTextured();
            TextureLeftTriangle(left, right, bottom, top, x, filename, pt);
        }
        public void AddTextureRightTri(float left, float right, float bottom, float top, Color x, string filename)
        {
            PolygonTextured pt = new PolygonTextured();
            TextureRightTriangle(left, right, bottom, top, x, filename, pt);
        }
        public void AddTextureSpike(float left, float right, float bottom, float top, Color x, string filename)
        {
            Spike pt = new Spike();
            TextureEquiTriangle(left, right, bottom, top, x, filename, pt);
        }        

        public void TextureRectangle(float left, float right, float bottom, float top, Color x, string filename, PolygonTextured pt)
        {
            pt.Rightedge = right;
            pt.Leftedge = left;
            Texture texture = TextureLoader.FromFile(device, filename);
            pt.Tex = texture;
            pt.AddVertex(new Vector2(left, top));
            pt.AddTex(new Vector2(0, 1));
            pt.AddVertex(new Vector2(right, top));
            pt.AddTex(new Vector2(0, 0));
            pt.AddVertex(new Vector2(right, bottom));
            pt.AddTex(new Vector2(1, 0));
            pt.AddVertex(new Vector2(left, bottom));
            pt.AddTex(new Vector2(1, 1));
            pt.Color = Color.Transparent;
            world.Add(pt);       
        }

        public void TextureLeftTriangle(float left, float right, float bottom, float top, Color x, string filename, PolygonTextured pt)
        {
            Texture texture = TextureLoader.FromFile(device, filename);
            pt.Tex = texture;
            pt.AddVertex(new Vector2(right, top));
            pt.AddTex(new Vector2(0, 0));
            pt.AddVertex(new Vector2(right, bottom));
            pt.AddTex(new Vector2(1, 0));
            pt.AddVertex(new Vector2(left, bottom));
            pt.AddTex(new Vector2(1, 1));
            pt.Color = Color.Transparent;
            world.Add(pt);
        }
        public void TextureRightTriangle(float left, float right, float bottom, float top, Color x, string filename, PolygonTextured pt)
        {
            Texture texture = TextureLoader.FromFile(device, filename);
            pt.Tex = texture;
            pt.AddVertex(new Vector2(left, top));
            pt.AddTex(new Vector2(0, 0));
            pt.AddVertex(new Vector2(right, bottom));
            pt.AddTex(new Vector2(1, 1));
            pt.AddVertex(new Vector2(left, bottom));
            pt.AddTex(new Vector2(0, 1));
            pt.Color = Color.Transparent;
            world.Add(pt);
        }
        public void TextureEquiTriangle(float left, float right, float bottom, float top, Color x, string filename, PolygonTextured pt)
        {
            Texture texture = TextureLoader.FromFile(device, filename);
            pt.Tex = texture;
            pt.AddVertex(new Vector2(left, bottom));
            pt.AddTex(new Vector2(0, 1));
            pt.AddVertex(new Vector2((left + right) / 2, top));
            pt.AddTex(new Vector2(1, 0));
            pt.AddVertex(new Vector2(right, bottom));
            pt.AddTex(new Vector2(1, 1));
            pt.Color = Color.Transparent;

            world.Add(pt);
        }
        public void TextureStar(float midX, float midY, Color clr, string filename, PolygonTextured pt)
        {
            Texture texture = TextureLoader.FromFile(device, filename);
            pt.Tex = texture;

            pt.AddVertex(new Vector2(midX, midY + 0.15f));
            pt.AddTex(new Vector2(0.5f, 1));
            pt.AddVertex(new Vector2(midX + 0.2f, midY + 0.045f));
            pt.AddTex(new Vector2(1, 0.65f));
            pt.AddVertex(new Vector2(midX + 0.12f, midY - 0.15f));
            pt.AddTex(new Vector2(0.8f, 0));
            pt.AddVertex(new Vector2(midX - 0.12f, midY - 0.15f));
            pt.AddTex(new Vector2(0.2f, 0));
            pt.AddVertex(new Vector2(midX - 0.2f, midY + 0.045f));
            pt.AddTex(new Vector2(0, 0.65f));

            pt.Color = Color.Transparent;
            pt.Transparent = true;

            world.Add(pt);
        }
        public void TextureFinalStar(float midX, float midY, Color clr, string filename, PolygonTextured pt)
        {
            Texture texture = TextureLoader.FromFile(device, filename);
            pt.Tex = texture;

            pt.AddVertex(new Vector2(midX, midY + 0.3f));
            pt.AddTex(new Vector2(0.5f, 1));
            pt.AddVertex(new Vector2(midX + 0.4f, midY + 0.09f));
            pt.AddTex(new Vector2(1, 0.65f));
            pt.AddVertex(new Vector2(midX + 0.24f, midY - 0.3f));
            pt.AddTex(new Vector2(0.8f, 0));
            pt.AddVertex(new Vector2(midX - 0.24f, midY - 0.3f));
            pt.AddTex(new Vector2(0.2f, 0));
            pt.AddVertex(new Vector2(midX - 0.4f, midY + 0.09f));
            pt.AddTex(new Vector2(0, 0.65f));

            pt.Color = Color.Transparent;
            pt.Transparent = true;

            world.Add(pt);
        }
        public void TextureDiamond(float midX, float midY, Color clr, string filename, SpecPolyTextured pt)
        {
            Texture texture = TextureLoader.FromFile(device, filename);
            pt.Tex = texture;

            pt.AddVertex(new Vector2(midX + 1.5f, midY + 1.3f));
            pt.AddTex(new Vector2(1, 0.2f));

            pt.AddVertex(new Vector2(midX + 1.12f, midY + 1f));
            pt.AddTex(new Vector2(0.8f, 0.05f));
            pt.AddVertex(new Vector2(midX, midY+0.5f));
            pt.AddTex(new Vector2(0.5f, 0));
            pt.AddVertex(new Vector2(midX - 1.12f, midY + 1f));
            pt.AddTex(new Vector2(0.2f, 0.05f));

            pt.AddVertex(new Vector2(midX - 1.5f, midY + 1.3f));
            pt.AddTex(new Vector2(0, 0.2f));


            pt.AddVertex(new Vector2(midX, midY + 1.6f));
            pt.AddTex(new Vector2(0.5f, 1));
            pt.Color = Color.Transparent;
            pt.Transparent = true;

            world.Add(pt);
        }

        public void TextureSept(float midX, float midY, Color clr, string filename, SpecPolyTextured pt)
        {
            Texture texture = TextureLoader.FromFile(device, filename);
            pt.Tex = texture;

            pt.AddVertex(new Vector2(midX + 1.5f, midY + 1.3f));
            pt.AddTex(new Vector2(1, 0.2f));

            pt.AddVertex(new Vector2(midX + 1.12f, midY + 1f));
            pt.AddTex(new Vector2(0.8f, 0.05f));
            pt.AddVertex(new Vector2(midX + 0.6f, midY + 0.5f));
            pt.AddTex(new Vector2(0.6f, 0));
            pt.AddVertex(new Vector2(midX - 0.6f, midY + 0.5f));
            pt.AddTex(new Vector2(0.4f, 0));
            pt.AddVertex(new Vector2(midX - 1.12f, midY + 1f));
            pt.AddTex(new Vector2(0.2f, 0.05f));

            pt.AddVertex(new Vector2(midX - 1.5f, midY + 1.3f));
            pt.AddTex(new Vector2(0, 0.2f));


            pt.AddVertex(new Vector2(midX, midY + 1.6f));
            pt.AddTex(new Vector2(0.5f, 1));
            pt.Color = Color.Transparent;
            pt.Transparent = true;

            world.Add(pt);
        }
    }
}
