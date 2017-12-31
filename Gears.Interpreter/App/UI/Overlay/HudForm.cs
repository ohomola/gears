using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Gears.Interpreter.Core.Adapters.UI.Interoperability.ExternalMethodBindings;

namespace Gears.Interpreter.App.UI.Overlay
{
    public class HudForm : Form
    {
        public const string FormName = "GearsOverlayForm";
        public const string ProcessName = "Gears Overlay";

        public HudForm(bool clickThrough)
        {
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Location = new System.Drawing.Point(-1280, 0);
            this.Name = FormName;
            this.Text = ProcessName;
            this.StartPosition = FormStartPosition.Manual;
            this.TopMost = true;
            //this.WindowState = System.Windows.Forms.FormWindowState.Maximized;

            this.TransparencyKey = System.Drawing.Color.FromArgb(
                ((int)(((byte)(0)))), 
                ((int)(((byte)(0)))), 
                ((int)(((byte)(0)))));


            var screenRectangles = Screen.AllScreens.Select(x=>x.WorkingArea);

            this.SetBounds(screenRectangles.Min(x=>x.X), screenRectangles.Min(x => x.Y), screenRectangles.Max(x => x.X+x.Width), screenRectangles.Max(x => x.Y + x.Height));
            
            if (clickThrough)
            {
                SetFormAsClickThrough();
            }
            else
            {
                SetFormAsClickable();
            }
            
        }

        private void SetFormAsClickThrough()
        {
            var style = UserBindings.GetWindowLong(Handle, (int)UserBindings.GetWindowLongConst.GWL_EXSTYLE);
            UserBindings.SetWindowLong(Handle, (int)UserBindings.GetWindowLongConst.GWL_EXSTYLE, Convert.ToInt32(style 
                | (uint)UserBindings.WindowStyles.WS_EX_LAYERED 
                | (uint)UserBindings.WindowStyles.WS_EX_TRANSPARENT
                | (uint)UserBindings.WindowStyles.WS_EX_TOPMOST
                ));
            UserBindings.SetLayeredWindowAttributes(this.Handle, 0x0, 125, 0x3);
        }

        private void SetFormAsClickable()
        {
            UserBindings.SetLayeredWindowAttributes(this.Handle, 0x0, 125, 0x3);
        }

        public Point ClickPosition { get; set; }

        protected override void OnClick(EventArgs e)
        {
            var mouseEvent = (MouseEventArgs) e;

            if (mouseEvent.Button == MouseButtons.Left)
            {
                ClickPosition = mouseEvent.Location;
            }
        }

    }
}