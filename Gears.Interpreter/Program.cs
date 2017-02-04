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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Castle.Core.Internal;
using Gears.Interpreter.Applications.Debugging.Overlay;
using Gears.Interpreter.Applications.Registrations;
using Gears.Interpreter.Core.Extensions;
using Gears.Interpreter.Library.Config;

namespace Gears.Interpreter
{
    public class Program
    {
        private static OverlayForm form;
        public const int CriticalErrorStatusCode = -2;
        public const int ScenarioFailureStatusCode = -1;
        public const int OkStatusCode = 0;

        private static void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                form = new OverlayForm();
                //form.Init();
                Application.Run(form);
                form.Dispose();
            }
            catch (Exception)
            {
            }

            Application.Exit();
        }

        private static int Main(string[] args)
        {
            

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            //var ff = new MasterForm();
            //ff.InitializeAsInvisibleForm();
            //var gg = ff.CreateGraphics();
            //gg.FillRectangle(new SolidBrush(Color.Red), 5, 5, 500, 500);


            

            //Application.Run(form);
            var bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            
            bw.RunWorkerAsync();


            Thread.Sleep(1000);
            var Graphics = form.CreateGraphics();
            Graphics.CompositingMode = CompositingMode.SourceOver;
            Graphics.FillRectangle(new SolidBrush(Color.Red), 5, 5, 500, 500);
            Thread.Sleep(5000);
            var asd = form.X;
            Thread.Sleep(5000);
            //Graphics.Clear(Color.Black);
            //Graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, 0, 255, 255)), 0, 0, 400, 50);
            //Graphics.DrawString("HIGHLIGHTING", new Font(FontFamily.GenericSansSerif, 24), new SolidBrush(Color.FromArgb(255, 0, 200, 200)), 50, 8);


            //form.Activate();

            //form.StartRenderingLoop();



            return 0;
        }
        private static int Main3(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Out.WriteLine("----------------------------------------");
            Console.Out.WriteLine("Gears Scenario Debugger");
            Console.Out.WriteLine("Copyright 2016 Ondrej Homola");
            Console.Out.WriteLine("----------------------------------------");
            Console.ResetColor();

            try
            {
                Bootstrapper.Register(args);

                var interpreter = Bootstrapper.ResolveInterpreter();

                ConsoleView.Render(interpreter.Please("start"));

                ConsoleView.Render(interpreter.Please("help"));

                while (interpreter.IsAlive)
                {
                    ConsoleView.Render(interpreter.Please("status"));

                    var answer = interpreter.Please(interpreter.Continue());

                    ConsoleView.Render(answer);
                }

                return OkStatusCode;
            }
            catch (Exception e)
            {
                ConsoleView.Render(ConsoleColor.Red, "Error running application: " + e.GetAllMessages());
                
                return CriticalErrorStatusCode;
            }
            finally
            {
                Thread.Sleep(1000);

                Bootstrapper.Release();
            }
        }
    }

    internal class TestCaseDefinition
    {
        public string JUnitReportPath { get; set; }
    }
}
