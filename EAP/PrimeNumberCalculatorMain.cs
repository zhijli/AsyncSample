using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace AsyncOperationManagerExample
{
    // This form tests the PrimeNumberCalculator component.
    public class PrimeNumberCalculatorMain : System.Windows.Forms.Form
    {
        /////////////////////////////////////////////////////////////
        // Private fields
        //
        #region Private fields

        private PrimeNumberCalculator primeNumberCalculator1;
        private System.Windows.Forms.GroupBox taskGroupBox;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader taskIdColHeader;
        private System.Windows.Forms.ColumnHeader progressColHeader;
        private System.Windows.Forms.ColumnHeader currentColHeader;
        private System.Windows.Forms.Panel buttonPanel;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button startAsyncButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.ColumnHeader testNumberColHeader;
        private System.Windows.Forms.ColumnHeader resultColHeader;
        private System.Windows.Forms.ColumnHeader firstDivisorColHeader;
        private System.ComponentModel.IContainer components;
        private int progressCounter;
        private int progressInterval = 100;


        #endregion // Private fields

        /////////////////////////////////////////////////////////////
        // Construction and destruction
        //
        #region Private fields
        public PrimeNumberCalculatorMain()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            // Hook up event handlers.
            this.primeNumberCalculator1.CalculatePrimeCompleted +=
                new CalculatePrimeCompletedEventHandler(
                primeNumberCalculator1_CalculatePrimeCompleted);

            this.primeNumberCalculator1.ProgressChanged +=
                new ProgressChangedEventHandler(
                primeNumberCalculator1_ProgressChanged);

            this.listView1.SelectedIndexChanged +=
                new EventHandler(listView1_SelectedIndexChanged);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #endregion // Construction and destruction


        /////////////////////////////////////////////////////////////
        //
        #region Implementation

        // This event handler selects a number randomly to test
        // for primality. It then starts the asynchronous 
        // calculation by calling the PrimeNumberCalculator
        // component's CalculatePrimeAsync method.
        private void startAsyncButton_Click(
            System.Object sender, System.EventArgs e)
        {
            // Randomly choose test numbers 
            // up to 200,000 for primality.
            Random rand = new Random();
            int testNumber = rand.Next(200000);

            // Task IDs are Guids.
            Guid taskId = Guid.NewGuid();
            this.AddListViewItem(taskId, testNumber);

            // Start the asynchronous task.
            this.primeNumberCalculator1.CalculatePrimeAsync(
                testNumber,
                taskId);
        }

        private void listView1_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            this.cancelButton.Enabled = CanCancel();
        }

        // This event handler cancels all pending tasks that are
        // selected in the ListView control.
        private void cancelButton_Click(
            System.Object sender,
            System.EventArgs e)
        {
            Guid taskId = Guid.Empty;

            // Cancel all selected tasks.
            foreach (ListViewItem lvi in this.listView1.SelectedItems)
            {
                // Tasks that have been completed or canceled have 
                // their corresponding ListViewItem.Tag property
                // set to null.
                if (lvi.Tag != null)
                {
                    taskId = (Guid)lvi.Tag;
                    this.primeNumberCalculator1.CancelAsync(taskId);
                    lvi.Selected = false;
                }
            }

            cancelButton.Enabled = false;
        }

        // This event handler updates the ListView control when the
        // PrimeNumberCalculator raises the ProgressChanged event.
        //
        // On fast computers, the PrimeNumberCalculator can raise many
        // successive ProgressChanged events, so the user interface 
        // may be flooded with messages. To prevent the user interface
        // from hanging, progress is only reported at intervals. 
        private void primeNumberCalculator1_ProgressChanged(
            ProgressChangedEventArgs e)
        {
            if (this.progressCounter++ % this.progressInterval == 0)
            {
                Guid taskId = (Guid)e.UserState;

                if (e is CalculatePrimeProgressChangedEventArgs)
                {
                    CalculatePrimeProgressChangedEventArgs cppcea =
                        e as CalculatePrimeProgressChangedEventArgs;

                    this.UpdateListViewItem(
                        taskId,
                        cppcea.ProgressPercentage,
                        cppcea.LatestPrimeNumber);
                }
                else
                {
                    this.UpdateListViewItem(
                        taskId,
                        e.ProgressPercentage);
                }
            }
            else if (this.progressCounter > this.progressInterval)
            {
                this.progressCounter = 0;
            }
        }

        // This event handler updates the ListView control when the
        // PrimeNumberCalculator raises the CalculatePrimeCompleted
        // event. The ListView item is updated with the appropriate
        // outcome of the calculation: Canceled, Error, or result.
        private void primeNumberCalculator1_CalculatePrimeCompleted(
            object sender,
            CalculatePrimeCompletedEventArgs e)
        {
            Guid taskId = (Guid)e.UserState;

            if (e.Cancelled)
            {
                string result = "Canceled";

                ListViewItem lvi = UpdateListViewItem(taskId, result);

                if (lvi != null)
                {
                    lvi.BackColor = Color.Pink;
                    lvi.Tag = null;
                }
            }
            else if (e.Error != null)
            {
                string result = "Error";

                ListViewItem lvi = UpdateListViewItem(taskId, result);

                if (lvi != null)
                {
                    lvi.BackColor = Color.Red;
                    lvi.ForeColor = Color.White;
                    lvi.Tag = null;
                }
            }
            else
            {
                bool result = e.IsPrime;

                ListViewItem lvi = UpdateListViewItem(
                    taskId,
                    result,
                    e.FirstDivisor);

                if (lvi != null)
                {
                    lvi.BackColor = Color.LightGray;
                    lvi.Tag = null;
                }
            }
        }

        #endregion // Implementation

        /////////////////////////////////////////////////////////////
        //
        #region Private Methods

        private ListViewItem AddListViewItem(
            Guid guid,
            int testNumber)
        {
            ListViewItem lvi = new ListViewItem();
            lvi.Text = testNumber.ToString(
                CultureInfo.CurrentCulture.NumberFormat);

            lvi.SubItems.Add("Not Started");
            lvi.SubItems.Add("1");
            lvi.SubItems.Add(guid.ToString());
            lvi.SubItems.Add("---");
            lvi.SubItems.Add("---");
            lvi.Tag = guid;

            this.listView1.Items.Add(lvi);

            return lvi;
        }

        private ListViewItem UpdateListViewItem(
            Guid guid,
            int percentComplete,
            int current)
        {
            ListViewItem lviRet = null;

            foreach (ListViewItem lvi in this.listView1.Items)
            {
                if (lvi.Tag != null)
                {
                    if ((Guid)lvi.Tag == guid)
                    {
                        lvi.SubItems[1].Text =
                            percentComplete.ToString(
                            CultureInfo.CurrentCulture.NumberFormat);
                        lvi.SubItems[2].Text =
                            current.ToString(
                            CultureInfo.CurrentCulture.NumberFormat);
                        lviRet = lvi;
                        break;
                    }
                }
            }

            return lviRet;
        }

        private ListViewItem UpdateListViewItem(
            Guid guid,
            int percentComplete,
            int current,
            bool result,
            int firstDivisor)
        {
            ListViewItem lviRet = null;

            foreach (ListViewItem lvi in this.listView1.Items)
            {
                if ((Guid)lvi.Tag == guid)
                {
                    lvi.SubItems[1].Text =
                        percentComplete.ToString(
                        CultureInfo.CurrentCulture.NumberFormat);
                    lvi.SubItems[2].Text =
                        current.ToString(
                        CultureInfo.CurrentCulture.NumberFormat);
                    lvi.SubItems[4].Text =
                        result ? "Prime" : "Composite";
                    lvi.SubItems[5].Text =
                        firstDivisor.ToString(
                        CultureInfo.CurrentCulture.NumberFormat);

                    lviRet = lvi;

                    break;
                }
            }

            return lviRet;
        }

        private ListViewItem UpdateListViewItem(
            Guid guid,
            int percentComplete)
        {
            ListViewItem lviRet = null;

            foreach (ListViewItem lvi in this.listView1.Items)
            {
                if (lvi.Tag != null)
                {
                    if ((Guid)lvi.Tag == guid)
                    {
                        lvi.SubItems[1].Text =
                            percentComplete.ToString(
                            CultureInfo.CurrentCulture.NumberFormat);
                        lviRet = lvi;
                        break;
                    }
                }
            }

            return lviRet;
        }

        private ListViewItem UpdateListViewItem(
            Guid guid,
            bool result,
            int firstDivisor)
        {
            ListViewItem lviRet = null;

            foreach (ListViewItem lvi in this.listView1.Items)
            {
                if (lvi.Tag != null)
                {
                    if ((Guid)lvi.Tag == guid)
                    {
                        lvi.SubItems[4].Text =
                            result ? "Prime" : "Composite";
                        lvi.SubItems[5].Text =
                            firstDivisor.ToString(
                            CultureInfo.CurrentCulture.NumberFormat);
                        lviRet = lvi;
                        break;
                    }
                }
            }

            return lviRet;
        }

        private ListViewItem UpdateListViewItem(
            Guid guid,
            string result)
        {
            ListViewItem lviRet = null;

            foreach (ListViewItem lvi in this.listView1.Items)
            {
                if (lvi.Tag != null)
                {
                    if ((Guid)lvi.Tag == guid)
                    {
                        lvi.SubItems[4].Text = result;
                        lviRet = lvi;
                        break;
                    }
                }
            }

            return lviRet;
        }

        private bool CanCancel()
        {
            bool oneIsActive = false;

            foreach (ListViewItem lvi in this.listView1.SelectedItems)
            {
                if (lvi.Tag != null)
                {
                    oneIsActive = true;
                    break;
                }
            }

            return (oneIsActive == true);
        }

        #endregion

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.taskGroupBox = new System.Windows.Forms.GroupBox();
            this.buttonPanel = new System.Windows.Forms.Panel();
            this.cancelButton = new System.Windows.Forms.Button();
            this.startAsyncButton = new System.Windows.Forms.Button();
            this.listView1 = new System.Windows.Forms.ListView();
            this.testNumberColHeader = new System.Windows.Forms.ColumnHeader();
            this.progressColHeader = new System.Windows.Forms.ColumnHeader();
            this.currentColHeader = new System.Windows.Forms.ColumnHeader();
            this.taskIdColHeader = new System.Windows.Forms.ColumnHeader();
            this.resultColHeader = new System.Windows.Forms.ColumnHeader();
            this.firstDivisorColHeader = new System.Windows.Forms.ColumnHeader();
            this.panel2 = new System.Windows.Forms.Panel();
            this.primeNumberCalculator1 = new AsyncOperationManagerExample.PrimeNumberCalculator(this.components);
            this.taskGroupBox.SuspendLayout();
            this.buttonPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // taskGroupBox
            // 
            this.taskGroupBox.Controls.Add(this.buttonPanel);
            this.taskGroupBox.Controls.Add(this.listView1);
            this.taskGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.taskGroupBox.Location = new System.Drawing.Point(0, 0);
            this.taskGroupBox.Name = "taskGroupBox";
            this.taskGroupBox.Size = new System.Drawing.Size(608, 254);
            this.taskGroupBox.TabIndex = 1;
            this.taskGroupBox.TabStop = false;
            this.taskGroupBox.Text = "Tasks";
            // 
            // buttonPanel
            // 
            this.buttonPanel.Controls.Add(this.cancelButton);
            this.buttonPanel.Controls.Add(this.startAsyncButton);
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonPanel.Location = new System.Drawing.Point(3, 176);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Size = new System.Drawing.Size(602, 75);
            this.buttonPanel.TabIndex = 1;
            // 
            // cancelButton
            // 
            this.cancelButton.Enabled = false;
            this.cancelButton.Location = new System.Drawing.Point(128, 24);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(88, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // startAsyncButton
            // 
            this.startAsyncButton.Location = new System.Drawing.Point(24, 24);
            this.startAsyncButton.Name = "startAsyncButton";
            this.startAsyncButton.Size = new System.Drawing.Size(88, 23);
            this.startAsyncButton.TabIndex = 0;
            this.startAsyncButton.Text = "Start New Task";
            this.startAsyncButton.Click += new System.EventHandler(this.startAsyncButton_Click);
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                this.testNumberColHeader,
                this.progressColHeader,
                this.currentColHeader,
                this.taskIdColHeader,
                this.resultColHeader,
                this.firstDivisorColHeader});
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.Location = new System.Drawing.Point(3, 16);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(602, 160);
            this.listView1.TabIndex = 0;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // testNumberColHeader
            // 
            this.testNumberColHeader.Text = "Test Number";
            this.testNumberColHeader.Width = 80;
            // 
            // progressColHeader
            // 
            this.progressColHeader.Text = "Progress";
            // 
            // currentColHeader
            // 
            this.currentColHeader.Text = "Current";
            // 
            // taskIdColHeader
            // 
            this.taskIdColHeader.Text = "Task ID";
            this.taskIdColHeader.Width = 200;
            // 
            // resultColHeader
            // 
            this.resultColHeader.Text = "Result";
            this.resultColHeader.Width = 80;
            // 
            // firstDivisorColHeader
            // 
            this.firstDivisorColHeader.Text = "First Divisor";
            this.firstDivisorColHeader.Width = 80;
            // 
            // panel2
            // 
            this.panel2.Location = new System.Drawing.Point(200, 128);
            this.panel2.Name = "panel2";
            this.panel2.TabIndex = 2;
            // 
            // PrimeNumberCalculatorMain
            // 
            this.ClientSize = new System.Drawing.Size(608, 254);
            this.Controls.Add(this.taskGroupBox);
            this.Name = "PrimeNumberCalculatorMain";
            this.Text = "Prime Number Calculator";
            this.taskGroupBox.ResumeLayout(false);
            this.buttonPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion
    }


    /////////////////////////////////////////////////////////////
    #region PrimeNumberCalculator Implementation

    public delegate void ProgressChangedEventHandler(
        ProgressChangedEventArgs e);

    public delegate void CalculatePrimeCompletedEventHandler(
        object sender,
        CalculatePrimeCompletedEventArgs e);

    public class CalculatePrimeProgressChangedEventArgs :
            ProgressChangedEventArgs
    {
        private int latestPrimeNumberValue = 1;

        public CalculatePrimeProgressChangedEventArgs(
            int latestPrime,
            int progressPercentage,
            object userToken) : base(progressPercentage, userToken)
        {
            this.latestPrimeNumberValue = latestPrime;
        }

        public int LatestPrimeNumber
        {
            get
            {
                return latestPrimeNumberValue;
            }
        }
    }

    public class CalculatePrimeCompletedEventArgs :
        AsyncCompletedEventArgs
    {
        private int numberToTestValue = 0;
        private int firstDivisorValue = 1;
        private bool isPrimeValue;

        public CalculatePrimeCompletedEventArgs(
            int numberToTest,
            int firstDivisor,
            bool isPrime,
            Exception e,
            bool canceled,
            object state) : base(e, canceled, state)
        {
            this.numberToTestValue = numberToTest;
            this.firstDivisorValue = firstDivisor;
            this.isPrimeValue = isPrime;
        }

        public int NumberToTest
        {
            get
            {
                // Raise an exception if the operation failed or 
                // was canceled.
                RaiseExceptionIfNecessary();

                // If the operation was successful, return the 
                // property value.
                return numberToTestValue;
            }
        }

        public int FirstDivisor
        {
            get
            {
                // Raise an exception if the operation failed or 
                // was canceled.
                RaiseExceptionIfNecessary();

                // If the operation was successful, return the 
                // property value.
                return firstDivisorValue;
            }
        }

        public bool IsPrime
        {
            get
            {
                // Raise an exception if the operation failed or 
                // was canceled.
                RaiseExceptionIfNecessary();

                // If the operation was successful, return the 
                // property value.
                return isPrimeValue;
            }
        }
    }


    #endregion


}