using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Gears.Interpreter.Applications.Debugging.Overlay
{
    internal class ClickableHudForm : Form
    {
        public ClickableHudForm()
        {
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Location = new System.Drawing.Point(-1280, 0);
            this.Name = "GearsOverlayForm";
            this.Text = "Gears Overlay";
            this.StartPosition = FormStartPosition.Manual;
            this.TopMost = true;
            //this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.TransparencyKey = System.Drawing.Color.FromArgb(
                ((int)(((byte)(0)))), 
                ((int)(((byte)(0)))), 
                ((int)(((byte)(0)))));


            var screenRectangles = Screen.AllScreens.Select(x=>x.WorkingArea);

            this.SetBounds(screenRectangles.Min(x=>x.X), screenRectangles.Min(x => x.Y), screenRectangles.Max(x => x.X+x.Width), screenRectangles.Max(x => x.Y + x.Height));
            //oldWindowLong = GetWindowLong(Handle, (int)MasterForm.GetWindowLongConst.GWL_EXSTYLE);
            //SetWindowLong(Handle, (int)MasterForm.GetWindowLongConst.GWL_EXSTYLE, Convert.ToInt32(oldWindowLong | (uint)MasterForm.WindowStyles.WS_EX_LAYERED | (uint)MasterForm.WindowStyles.WS_EX_TRANSPARENT));

            //SetLayeredWindowAttributes(this.Handle, 0x0, 125, 0x3);
            SetLayeredWindowAttributes(this.Handle, 0x0, 60, 0x3);
        }

        public Point ClickPosition { get; set; }

        protected override void OnClick(EventArgs e)
        {
            var mouseEvent = (MouseEventArgs) e;

            if (mouseEvent.Button == MouseButtons.Left)
            {
                ClickPosition = mouseEvent.Location;
                this.Close();
            }
        }



        [DllImport("user32.dll")]
        static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);



    }
}