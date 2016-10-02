#region LICENSE
/*
Copyright 2016 Ondrej Homola <ondra.homola@gmail.com>

This file is part of Gears, a software automation and assistance framework.

Gears is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Gears is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gears.Interpreter.Adapters.Interoperability;

namespace Gears.Interpreter.Applications.Debugging.Overlay
{
    public interface IOverlay
    {
        Graphics Graphics { get; set; }
        void Init();
        void Clear();
    }

    public class Overlay : IOverlay, IDisposable
    {
        private MasterForm _masterForm;
        private bool _initialized;
        private static bool _appInitialized;
        public Graphics Graphics { get; set; }

        public void Init()
        {
            if (_initialized)
            {
                return;
            }

            StaticInit();

            _masterForm = new MasterForm();
            _masterForm.InitializeAsInvisibleForm();
            Graphics = _masterForm.CreateGraphics();
            //Graphics.CompositingMode = CompositingMode.SourceCopy;
            Graphics.CompositingMode = CompositingMode.SourceOver;

            Clear();

            _initialized = true;

            
        }

        private static void StaticInit()
        {
            if (_appInitialized)
            {
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            _appInitialized = true;
        }

        public void Clear()
        {
            Graphics.Clear(Color.Black);
            Graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, 0, 255, 255)), 0, 0, UserInteropAdapter.VirtualScreenWidth, 50);
            Graphics.DrawString("HIGHLIGHTING", new Font(FontFamily.GenericSansSerif, 24), new SolidBrush(Color.FromArgb(255,0,200,200)),50, 8);

        }

        public void Dispose()
        {
            _masterForm.Dispose();
        }
    }
}
