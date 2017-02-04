using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Gears.Interpreter.Adapters.Interoperability;

namespace Gears.Interpreter.Applications.Debugging.Overlay
{
    partial class OverlayForm2
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            IsStopped = true;
            Thread.Sleep(100);
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                
                while (!IsStopped)
                {
                    //                OverlayContainer.Render();
                    this.PerformLayout();
                    Thread.Sleep(50);
                }
            }
            catch (Exception)
            {
            }

            Application.Exit();
        }

        public void Stop()
        {
            this.IsStopped = true;
        }

        public bool IsStopped { get; set; }
        BackgroundWorker bw = new BackgroundWorker();

        public void StartRenderingLoop()
        {
            BackgroundWorker tmpBw = new BackgroundWorker();
            tmpBw.DoWork += new DoWorkEventHandler(bw_DoWork);

            this.bw = tmpBw;

            this.bw.RunWorkerAsync();
        }

        private void MaximizeEverything()
        {
            this.Location = new Point(50, 50);
            this.Size = new Size(UserInteropAdapter.VirtualScreenWidth, UserInteropAdapter.VirtualScreenHeight);

            SendMessage(this.Handle, 0x0112, (UIntPtr)0xf120, (IntPtr)0x5073d);
        }


        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, uint wMsg, UIntPtr wParam, IntPtr lParam); //used for maximizing the screen

        int oldWindowLong;
        public void Init()
        {
            this.SuspendLayout();
            InitializeComponent();
            
            this.ResumeLayout(false);
            this.PerformLayout();

            //MaximizeEverything();

            //this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            //this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            //this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            //this.ClientSize = new System.Drawing.Size(284, 262);
            //this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            //this.Location = new System.Drawing.Point(-1280, 0);
            //this.Name = "MasterForm";
            //this.Text = "HIGHLIGHT Overlay";
            //this.TopMost = true;
            this.TransparencyKey = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            //this.WindowState = System.Windows.Forms.FormWindowState.Maximized;

            //oldWindowLong = GetWindowLong(Handle, (int)MasterForm.GetWindowLongConst.GWL_EXSTYLE);
            //SetWindowLong(Handle, (int)MasterForm.GetWindowLongConst.GWL_EXSTYLE, Convert.ToInt32(oldWindowLong | (uint)MasterForm.WindowStyles.WS_EX_LAYERED | (uint)MasterForm.WindowStyles.WS_EX_TRANSPARENT));
            SetLayeredWindowAttributes(this.Handle, 0x0, 125, 0x3);

            
        }

        [DllImport("user32.dll")]
        static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Text = "OverlayForm";


            //this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Location = new System.Drawing.Point(-1280, 0);
            this.Name = "MasterForm";
            this.Text = "HIGHLIGHT Overlay";
            this.TopMost = true;
            //this.TransparencyKey = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            //this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.MasterForm_Load);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        protected override void OnClick(EventArgs e)
        {
            Console.Out.Write("--- On Click called ---");

            //base.OnClick(e);

            X = 456;
        }

        private void MasterForm_Load(object sender, EventArgs e)
        {

        }

        #endregion
    }
}