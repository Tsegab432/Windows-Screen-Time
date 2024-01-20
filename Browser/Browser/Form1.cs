using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Browser
{
    public partial class Form1 : Form
    {
        public int hour;
        public int minute;
        public int second;
        private bool isTimerFinished = false;

        private string logFilePath = "usage_log.txt";

        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            // Check if the user has opened the computer today
            if (HasUserOpenedToday())
            {
                ShowAutoCloseMessageBox("You have already opened the computer today.", "Notification", 2000);
                await Task.Delay(3000);
                Process.Start("shutdown", "/s /t 0");
            }

            // Get the values from numericUpDown controls
            hour = (int)numericUpDown1.Value;
            minute = (int)numericUpDown2.Value;
            second = (int)numericUpDown3.Value;

            // Validate values to avoid negative or invalid inputs
            if (hour < 0 || minute < 0 || second < 0)
            {
                ShowAutoCloseMessageBox("Please enter valid values for hours, minutes, and seconds.", "Notification", 2000);
                return;
            }

            isTimerFinished = false; // Reset the flag

            for (int hours = 0; hours <= hour; hours++)
            {
                for (int minutes = 0; minutes <= minute; minutes++)
                {
                    for (int seconds = 0; seconds <= second; seconds++)
                    {
                        label1.Text = $"{hours.ToString("D2")}:{minutes.ToString("D2")}:{seconds.ToString("D2")}";
                        await Task.Delay(1000);

                        // Check if the timer has finished
                        if (hours == hour && minutes == minute && seconds == second)
                        {
                            isTimerFinished = true;
                            ShowAutoCloseMessageBox("Timer has finished!", "Notification", 2000);

                            // Countdown from 5 seconds and then shut down
                            for (int s = 5; s >= 0; s--)
                            {
                                ShowAutoCloseMessageBox($"Shutting down computer in {s} seconds.", "Countdown", 1000);
                                await Task.Delay(1000);
                            }

                            // Shut down the computer
                            Process.Start("shutdown", "/s /t 0");

                            // Log the usage for today
                            LogUserOpening();
                        }

                        // Notify user when half of the time has passed
                        if (hours == hour / 2 && minutes == minute / 2 && seconds == second / 2)
                        {
                            ShowAutoCloseMessageBox($"You have {hour / 2} hours, {minute / 2} minutes, and {second / 2} seconds left. Save all your work!", "Notification", 5000);
                        }
                    }
                }
            }
        }

        private bool HasUserOpenedToday()
        {
            // Check if the log file exists
            if (System.IO.File.Exists(logFilePath))
            {
                // Read the last usage date from the file
                string lastUsageDate = System.IO.File.ReadAllText(logFilePath);

                // Compare the last usage date with today's date
                return lastUsageDate == DateTime.Now.ToString("yyyy-MM-dd");
            }

            return false; // Log file doesn't exist, meaning the user has not opened the computer today
        }

        private void LogUserOpening()
        {
            // Write today's date to the log file
            System.IO.File.WriteAllText(logFilePath, DateTime.Now.ToString("yyyy-MM-dd"));
        }

        private void ShowAutoCloseMessageBox(string message, string caption, int duration)
        {
            var autoCloseForm = new AutoCloseForm(message, caption, duration);
            autoCloseForm.ShowDialog();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {

        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {

        }
    }

    public class AutoCloseForm : Form
    {
        private Timer timer = new Timer();

        public AutoCloseForm(string message, string caption, int duration)
        {
            InitializeComponent();

            label1.Text = message;
            Text = caption;

            timer.Interval = duration;
            timer.Tick += (sender, args) =>
            {
                timer.Stop();
                this.Close();
            };

            timer.Start();
        }

        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(300, 100);
            this.label1.TabIndex = 0;
            this.label1.Text = "Message";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // AutoCloseForm
            // 
            this.ClientSize = new System.Drawing.Size(300, 100);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AutoCloseForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Label label1;
    }
}
