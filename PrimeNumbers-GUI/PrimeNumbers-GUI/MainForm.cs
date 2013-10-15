using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PrimeNumbers_GUI
{
    public partial class MainForm : Form
    {
        private bool cancelJob = false;

        public MainForm()
        {
            InitializeComponent();
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            // Prevent user from messing with certain controls while job is running
            
            progressBar1.Visible = true;
            cancelButton.Enabled = true;
            pauseButton.Enabled = true;
            pauseButton.Text = "Pause";
            startNumTextBox.Enabled = false;
            endNumTextBox.Enabled = false;
            startButton.Enabled = false;

            numbersTextBox.Clear();

            int first;
            int last;

            // Find all prime numbers starting between the first and last numbers
            bool isFirstNum = Int32.TryParse(startNumTextBox.Text, out first);
            bool isLastNum = Int32.TryParse(endNumTextBox.Text, out last);

            if (isFirstNum && isLastNum)
            {
                if (first > 0 && last > 0)
                {
                    if (first > last)
                    {
                        last = Convert.ToInt32(startNumTextBox.Text);
                        first = Convert.ToInt32(endNumTextBox.Text);
                    }

                    progressBar1.Minimum = first;
                    progressBar1.Maximum = last;

                    if (!backgroundWorker1.IsBusy)
                    {
                        backgroundWorker1.RunWorkerAsync();
                    }
                }
                else
                {
                    MessageBox.Show("Number must be larger than 0.");

                    startNumTextBox.Enabled = true;
                    endNumTextBox.Enabled = true;
                    startButton.Enabled = true;
                    progressBar1.Visible = false;
                    cancelButton.Enabled = false;
                    pauseButton.Enabled = false;
                }
            }
            else
            {
                MessageBox.Show("Invalid number in textbox.");

                startNumTextBox.Enabled = true;
                endNumTextBox.Enabled = true;
                startButton.Enabled = true;
                progressBar1.Visible = false;
                cancelButton.Enabled = false;
                pauseButton.Enabled = false;
            }


        }

        private delegate void UpdateLabelCallback(int i);

        private void AddNumberToLabel(int num)
        {
            // See if we are currently on the same thread as the control. 
            // We won't be if the DoWork() method calls this sub, but we will be 
            // when invoked via the delegate 
            if (numbersTextBox.InvokeRequired)
            {
                // Invoke the UpdateLabelCallback delegate on the UI thread 
                UpdateLabelCallback callback = new UpdateLabelCallback(AddNumberToLabel);
                try
                {
                    this.Invoke(callback, new object[] { num });
                }
                catch (ObjectDisposedException e)
                {
                    if (backgroundWorker1.IsBusy)
                    {
                        backgroundWorker1.CancelAsync();
                    }
                    
                }
            }
            else
                numbersTextBox.AppendText(num + "\n");
        } 

        private bool IsPrime(int num)
        {
            // Look for a number that evenly divides the num
            for (int i = 2; i < num / 2; i++)
                if (num % i == 0)
                    return false;

            // No divisors means the number is prime
            return true;
        }

        //PAUSE
        private void pauseButton_Click(object sender, EventArgs e)
        {
            // Pause or resume the current job
            if (pauseButton.Text == "Pause")
            {
                pauseButton.Text = "Resume";
            }
            else
            {
                pauseButton.Text = "Pause";
            }
            
        }

        //CANCEL
        private void cancelButton_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.WorkerSupportsCancellation)
                backgroundWorker1.CancelAsync();
        }

        //DOWORK
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            // Find all prime numbers starting between the first and last numbers
            int first = Convert.ToInt32(startNumTextBox.Text);
            int last = Convert.ToInt32(endNumTextBox.Text);

            if (first > last)
            {
                last = Convert.ToInt32(startNumTextBox.Text);
                first = Convert.ToInt32(endNumTextBox.Text);
            }

            // See which numbers are factors and append them to the numbers text box
            for (int i = first; i < last; i++)
            {
                // Leave the loop when the Cancel button has been pressed
                // (This won't actually work because the UI thread is tied-up.)
                if(worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    if (pauseButton.Text == "Pause")
                    {
                        // Make the progress bar show the percent we've completed
                        worker.ReportProgress(i);

                        if (IsPrime(i))
                            AddNumberToLabel(i);
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(25);
                    }

                    if (cancelJob)
                        break;

                }
            }
        }

        //PROGRESS CHANGED
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        //RUN WORKER COMPLETED
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
                startNumTextBox.Enabled = true;
                endNumTextBox.Enabled = true;
                startButton.Enabled = true;
                progressBar1.Visible = false;
                cancelButton.Enabled = false;
                pauseButton.Enabled = false;

           if (pauseButton.Text == "Resume")
            {
                pauseButton.Text = "Pause";
            }
        }
    }
}
