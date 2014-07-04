using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace StepDX
{
    class Score
    {
        /// <summary>
        /// The points for the score
        /// </summary>
        private int points = 0;
           
        /// <summary>
        /// The timer position at end of game
        /// </summary>
        private int timer;

        /// <summary>
        /// Datetime for game timer
        /// </summary>
        private DateTime dTime;

        /// <summary>
        /// Where the scores are stored
        /// </summary>
        private string filename = "../../scores.txt";

        /// <summary>
        /// The High score
        /// </summary>
        private string highScore;
        private int hScore;

        /// <summary>
        /// The main score of the current game.
        /// </summary>
        public Score(int pts, int t, DateTime d)
        {
            points = pts;
            timer = t;
            dTime = d;
        }

        /// <summary>
        /// Add to the current score
        /// </summary>
        public int AddScore(int amount)
        {
            return (points + amount);
        }

        /// <summary>
        /// Subtract from the current score
        /// </summary>
        public int SubScore(int amount)
        {
            return (points - amount);
        }

        /// <summary>
        /// Write the score/datetime at end game to file
        /// </summary>
        public void write()
        {
            
            List<string> scores;

            if (File.Exists(filename))
            {
                scores = File.ReadAllLines(filename).ToList();
                string hs;
                string[] hrS;
                using (StreamReader reader = new StreamReader(filename))
                    hs = reader.ReadLine();
                hrS = hs.Split(new char[] { ':' });
                highScore = hrS[1];
                hScore = Convert.ToInt32(highScore);
            }
            else
            {
                scores = new List<string>();
                hScore = 0;
            }
            // Add our current datettime and points to the list
            scores.Add(dTime + ":" + points);
            // Reorder scores and write
            var sortScores = scores.OrderByDescending(sS => int.Parse(sS.Substring(sS.LastIndexOf(":") + 1)));
            File.WriteAllLines(filename, sortScores.ToArray());
            read();

            DialogResult hScoreShow;

            // Cheap endgame messages
            /*if (points > hScore)
            {
                
                hScoreShow = MessageBox.Show("A new high score of: " + points + "!\r\n Press ENTER to replay!",
                    "Game Over.  New High Score!",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                //if (hScoreShow == DialogResult.OK)
            }*/
            //else if (points < hScore)
            //{
                hScoreShow = MessageBox.Show("Level Complete! You earned " + points + " points!",
                    "Game Over",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.None);
                //if (lScoreShow == DialogResult.OK)
            //}

            string scoreOut = null;

            for (int i = 0; i < scores.Count && i <= 10; i++)
            {
                if (i == (scores.Count - 1) || i == 10)
                {
                    scoreOut += "\n Your Score: ";
                    scoreOut += scores[scores.Count - 1] + "\n";
                    break;
                }
                scoreOut += scores[i] + "\n"; 
            }

            hScoreShow = MessageBox.Show("High Scores:\n------------\n" + scoreOut + "\n\nHit ENTER to play again!",
                "High Scores",
                MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation);
        }

        private void read()
        {
            List<string> scores = File.ReadAllLines(filename).ToList();
            foreach (string score in scores)
            {
                Console.WriteLine(score);
            }
        }
    }
}
