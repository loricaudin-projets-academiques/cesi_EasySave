namespace EasySave.Core.ProgressBar
{
    /// <summary>
    /// Console-based progress bar display.
    /// </summary>
    internal class ProgressBar
    {
        protected double Percentage;

        public ProgressBar()
        {
            this.Percentage = 0;
        }

        /// <summary>
        /// Initializes the progress bar with a title message.
        /// </summary>
        /// <param name="text">Title to display above the progress bar.</param>
        public void InitProgressBar(string text)
        {
            this.Percentage = 0;
            Console.WriteLine(text);
            this.DrawProgressBar(Percentage);
        }

        /// <summary>
        /// Updates the progress bar to the specified value.
        /// </summary>
        /// <param name="value">Progress value between 0 and 1.</param>
        public void SetProgressBar(double value)
        {
            this.Percentage = value;
            this.DrawProgressBar(Percentage);
        }

        private void DrawProgressBar(double progress, int barSize = 50)
        {
            if (progress < 0) progress = 0;
            if (progress > 1) progress = 1;

            int filledBars = (int)Math.Round(progress * barSize);
            int emptyBars = barSize - filledBars;

            string bar = new string('█', filledBars) + new string('░', emptyBars);
            Console.Write($"\r[{bar}] {progress * 100:0.0}%");
        }
    }
}
