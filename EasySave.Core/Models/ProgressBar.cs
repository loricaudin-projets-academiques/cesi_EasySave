using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Core.Models
{
    internal class ProgressBar
    {
        protected double Percentage;

        private void DrawProgressBar(double progress, int barSize = 50)
        {
            if (progress < 0) progress = 0;
            if (progress > 1) progress = 1;

            int filledBars = (int)Math.Round(progress * barSize);
            int emptyBars = barSize - filledBars;

            // Construire la barre
            string bar = new string('█', filledBars) + new string('░', emptyBars);

            // Afficher sur la même ligne
            Console.Write($"\r[{bar}] {progress * 100:0.0}%");
        }

        public ProgressBar()
        {
            this.Percentage = 0;
        }

        public void InitProgressBar(string text)
        {
            this.Percentage = 0;
            Console.WriteLine(text);
            this.DrawProgressBar(Percentage);
        }

        public void SetProgressBar(double value)
        {
            this.Percentage = value;
            this.DrawProgressBar(Percentage);
        }
    }
}
