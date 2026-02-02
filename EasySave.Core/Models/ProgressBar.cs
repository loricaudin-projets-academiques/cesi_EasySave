using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Core.Models
{
    internal class ProgressBar
    {
        private float percentage;

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
            this.percentage = 0;

            this.InitProgressBar("Démarrage du traitement...\n");

            // Simulation d'un traitement
            for (int i = 0; i <= 100; i++)
            {
                DrawProgressBar(i / 100.0);
                Thread.Sleep(50); // Pause pour simuler un travail
            }

            Console.WriteLine("\n\nTraitement terminé !");
        }

        public void InitProgressBar(string text)
        {
            this.percentage = 0;
            Console.WriteLine(text);
            this.DrawProgressBar(percentage);


        }

        public void SetProgressBar(float value)
        {
            this.percentage = value;
            this.DrawProgressBar(percentage);
        }
    }
}
