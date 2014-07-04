using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.DirectSound;

namespace StepDX
{
    class GameSounds
    {
        private Device SoundDevice = null;

        private SecondaryBuffer music = null;
        private SecondaryBuffer jump = null;
        private SecondaryBuffer hit = null;
        private SecondaryBuffer hurt = null;
        private SecondaryBuffer boost = null;
        private SecondaryBuffer goal = null;
        private SecondaryBuffer[] getstar = new SecondaryBuffer[10];
        private SecondaryBuffer[] getstarlong = new SecondaryBuffer[10];

        //int clankToUse = 0;

        public GameSounds(Form form)
        {
            SoundDevice = new Device();
            SoundDevice.SetCooperativeLevel(form, CooperativeLevel.Priority);
            
            Load(ref music, "../../audio/digital_strider2.wav");
            Load(ref jump, "../../audio/jump.wav");
            Load(ref hit, "../../audio/hit.wav");
            Load(ref hurt, "../../audio/hurt2.wav");
            Load(ref boost, "../../audio/boost.wav");
            Load(ref goal, "../../audio/goal.wav");

            for (int i = 0; i < getstar.Length; i++)
                Load(ref getstar[i], "../../audio/getstar.wav");

            for (int i = 0; i < getstarlong.Length; i++)
                Load(ref getstarlong[i], "../../audio/getstarlong.wav");
        }

        private void Load(ref SecondaryBuffer buffer, string filename)
        {
            try
            {
                buffer = new SecondaryBuffer(filename, SoundDevice);
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to load " + filename,
                                "Danger, Will Robinson", MessageBoxButtons.OK);
                buffer = null;
            }
        }

        public void Music()
        {
            if (music == null)
                return;

            if (!music.Status.Playing)
            {
                music.Play(0, BufferPlayFlags.Default);
            }
        }

        public void MusicStop()
        {
            if (music == null)
                return;

            if (music.Status.Playing)
            {
                music.Stop();
            }
        }

        public void MusicMute()
        {
            if (music == null)
                return;
            if (music.Status.Playing && music.Volume == -10000)
            {
                music.Volume = 0;
            }
            else if (music.Status.Playing && music.Volume == 0)
            {
                music.Volume = -10000;
            }
        }

        public void Jump()
        {
            if (jump == null)
                return;

            if (!jump.Status.Playing)
            {
                jump.Play(0, BufferPlayFlags.Default);
            }
        }

        public void Hit()
        {
            if (hit == null)
                return;

            if (!hit.Status.Playing)
            {
                hit.Play(0, BufferPlayFlags.Default);
            }
        }
        public void GetStar(int points)
        {
            if (getstar == null || getstarlong == null)
                return;

            if (points != 5)
            {
                for (int i = 0; i < getstarlong.Length; i++)
                {
                    if (!getstarlong[i].Status.Playing)
                    { 
                        getstarlong[i].Play(0, BufferPlayFlags.Default);
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < getstar.Length; i++)
                {
                    if (!getstar[i].Status.Playing)
                    { 
                        getstar[i].Play(0, BufferPlayFlags.Default);
                        break;
                    }
                }
            }
        }
        public void Hurt()
        {
            if (hurt == null)
                return;

            if (!hurt.Status.Playing)
            {
                hurt.Play(0, BufferPlayFlags.Default);
            }
        }
        public void Boost()
        {
            if (boost == null)
                return;

            if (!boost.Status.Playing)
            {
                boost.Play(0, BufferPlayFlags.Default);
            }
        }
        public void Goal()
        {
            if (goal == null)
                return;

            if (!goal.Status.Playing)
            {
                goal.Play(0, BufferPlayFlags.Default);
            }
        }

        /*public void ArmsEnd()
        {
            if (arms == null)
                return;

            if (arms.Status.Playing)
                arms.Stop();
        }*/

    }
}