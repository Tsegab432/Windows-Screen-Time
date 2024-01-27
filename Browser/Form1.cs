using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Linq;
using SHDocVw;
using System.Windows.Forms.DataVisualization.Charting;

namespace Browser
{
    public partial class Form1 : Form
    {
        public int hour;
        public int minute;
        public int second;
        private bool isTimerFinished = false;
        private bool isClicked = false;

        private string logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "usage_log.txt");

        private Timer timer = new Timer();

        public Form1()
        {
            InitializeComponent();
            buttonclickchecker();
            this.TransparencyKey = Color.Empty;
        }

        private async void buttonclickchecker()
        {
            if (!isClicked)
            {
                ShowAutoCloseMessageBox("Click the button in 3 seconds or it will auto start!", "Warning", 1000);
                await Task.Delay(3000);
                button1.PerformClick();

            }
        }
        #region //Main_button (start button)
        private async void button1_Click(object sender, EventArgs e)
        {
            isClicked = true;

            if (isClicked)
            {
                button1.Enabled = false;
            }

            // Validate values to avoid negative or invalid inputs
            if (hour < 0 || minute < 0 || second < 0)
            {
                ShowAutoCloseMessageBox("Please enter valid values for hours, minutes, and seconds.", "Notification", 2000);
                return;
            }

            // Check if the user has opened the program today
            if (HasUserOpenedToday())
            {
                // Show an auto-closing message box and shut down after 3 seconds
                ShowAutoCloseMessageBox("You've already opened the program today. Shutting down in 3 seconds.", "Notification", 3000);
                await Task.Delay(3000);
                // Shut down the computer
                Process.Start("shutdown", "/s /t 0");
                return;
            }

            isTimerFinished = false; // Reset the flag

            // Set initial values based on weekend or weekday
            int initialHours, initialMinutes;
            if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday || DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
            {
                initialHours = (int)numericUpDown3.Value;
                initialMinutes = (int)numericUpDown4.Value;
                ShowAutoCloseMessageBox("It's the weekend! Shutting down in " + initialHours +":"+ initialMinutes + " hours and minutes.", "Weekend Notification", 3000);
            }
            else // Weekdays (Monday to Friday)
            {
                initialHours = (int)numericUpDown1.Value;
                initialMinutes = (int)numericUpDown2.Value;
                ShowAutoCloseMessageBox("It's a school day. Shutting down in " + initialHours + ":" + initialMinutes + " hours and minutes.", "School Day Notification", 3000);
            }

            int totalSeconds = (initialHours * 3600) + (initialMinutes * 60) + second;

            timer.Interval = 1000;
            timer.Start();

            for (int elapsedSeconds = 0; elapsedSeconds <= totalSeconds; elapsedSeconds++)
            {
                int remainingSeconds = totalSeconds - elapsedSeconds;

                int remainingHours = remainingSeconds / 3600;
                int remainingMinutes = (remainingSeconds % 3600) / 60;
                int remainingSecs = remainingSeconds % 60;

                label1.Text = $"{remainingHours:D2}:{remainingMinutes:D2}:{remainingSecs:D2}";
                notifyIcon1.BalloonTipText = $"{remainingHours:D2}:{remainingMinutes:D2}:{remainingSecs:D2}";
                this.Refresh(); // Force the label to refresh immediately
                await Task.Delay(1000);

                if (elapsedSeconds == totalSeconds)
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
                if (elapsedSeconds == totalSeconds / 2)
                {
                    ShowAutoCloseMessageBox($"You have {remainingHours} hours, {remainingMinutes} minutes, and {remainingSecs} seconds left. Half time has passed!", "Notification", 5000);
                }
            }
        }
        #endregion

        private bool HasUserOpenedToday()
        {
            // Check if the log file exists
            if (System.IO.File.Exists(logFilePath))
            {
                // Read the last usage date from the file
                string lastUsageDate = System.IO.File.ReadAllText(logFilePath);

                // Parse the last usage date to DateTime
                if (DateTime.TryParseExact(lastUsageDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime lastUsageDateTime))
                {
                    // Compare the last usage date with today's date
                    return lastUsageDateTime.Date == DateTime.Now.Date;
                }
            }

            return false; // Log file doesn't exist or failed to parse, meaning the user has not opened the program today
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

        private async void Form1_Load(object sender, EventArgs e)
        {
            ControlBox = false;
            this.FormClosing += Form1_FormClosing;
        }


        private void numericUpDown1_ValueChanged_1(object sender, EventArgs e)
        {
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true; // Cancel the closing event
            this.Hide();
            notifyIcon1.Visible = true;
            if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true; // Cancel the closing event
            }

        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Alt | Keys.F4))
            {
                // Handle Alt + F4 by preventing the form from closing
                this.Hide();
                return true; // Indicate that the key has been handled
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }


        private void ChartUpdater_Click(object sender, EventArgs e)
        {

            try
            {
                // Read the content of the log file
                if (File.Exists(logFilePath))
                {
                    string logContent = File.ReadAllText(logFilePath);

                    // Now, logContent contains the text content of the file
                    // You can use this content for further processing or display

                    // For example, let's display the content in a message box
                    label2.Text = $"Your Status:\n\n{logContent}";
                }
                else
                {
                    label2.Text = "Log file not found.";
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions, such as file access errors
                label2.Text = $"Error reading log file: {ex.Message}";
            }
        }



        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            string password = textBox1.Text;

            if(password == "#7!pLzXq@9w$5rJ2Y3tU6iO8lK0nM4bV1cGfAxDhEjR5yT6uI8oP0zL1kN3jH4gB7vC2F9mQ2eR4xW7sD1uZ8\r\n")
            {
                textBox1.Visible = false;
                button2.Visible = false;
                panel1.Visible = false;
                label3.Visible = false;
            }
            else
            {
                label3.ForeColor = Color.Red;
                label3.Text = "That is not the correct password!";
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void numericUpDown1_ValueChanged_2(object sender, EventArgs e)
        {

        }

        private void numericUpDown2_ValueChanged_1(object sender, EventArgs e)
        {

        }

        private void numericUpDown3_ValueChanged_1(object sender, EventArgs e)
        {

        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {

        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            
            File.WriteAllText(logFilePath, string.Empty);
            label2.ForeColor = Color.Green;
            label2.Text = "Status clear succusfully!";
            label1.ForeColor = Color.Black;

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void chart1_Click_1(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                // Read lines from the file and count them using LINQ.
                int numberOfLines = File.ReadLines(logFilePath).Count();

                // Clear existing series from the chart.
                chart1.Series.Clear();

                // Create a new Series and add data points.
                Series series = new Series("Number of Lines");
                series.Points.AddY(numberOfLines);

                // Add the Series to the Chart.
                chart1.Series.Add(series);
            }
            catch (IOException ex)
            {
                MessageBox.Show($"An error occurred while reading the file: {ex.Message}");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {

        }



        private void button6_Click(object sender, EventArgs e)
        {
            string password = textBox2.Text;

            if (password == "#7!pLzXq@9w$5rJ2Y3tU6iO8lK0nM4bV1cGfAxDhEjR5yT6uI8oP0zL1kN3jH4gB7vC2F9mQ2eR4xW7sD1uZ8\r\n")
            {
                textBox1.Visible = false;
                button2.Visible = false;
                panel1.Visible = false;
                label3.Visible = false;
            }
            else
            {
                label3.ForeColor = Color.Red;
                label3.Text = "That is not the correct password!";
            }
        }


        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
        #region //Change_graph
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void button5_Click_1(object sender, EventArgs e)
        {
            string selectedChartType = comboBox1.SelectedItem.ToString();

            if (selectedChartType == "Column")
            {
                chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;
            }
            else if (selectedChartType == "Bar")
            {
                chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Bar;
            }
            else if (selectedChartType == "Line")
            {
                chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            }
            else if (selectedChartType == "Spline")
            {
                chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            }
            else if (selectedChartType == "Area")
            {
                chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Area;
            }
            else if (selectedChartType == "Pie")
            {
                chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            }
            else if (selectedChartType == "Doughnut")
            {
                chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Doughnut;
            }
            else if (selectedChartType == "Bubble")
            {
                chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Bubble;
            }
            else if (selectedChartType == "Candlestick")
            {
                chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Candlestick;
            }
            else if (selectedChartType == "Range")
            {
                chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Range;
            }
            else if (selectedChartType == "RangeColumn")
            {
                chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.RangeColumn;
            }
            else if (selectedChartType == "RangeBar")
            {
                chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.RangeBar;
            }
            else if (selectedChartType == "Radar")
            {
                chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Radar;
            }
            else if (selectedChartType == "Polar")
            {
                chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Polar;
            }
        }
        #endregion
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
