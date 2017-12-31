using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Gears.Interpreter.App.Workflow;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Adapters.UI;
using Gears.Interpreter.Core.Extensions;

namespace Gears.Interpreter.App.UI.Overlay
{
    public interface IBrowserOverlay
    {
        IBrowserOverlay HighlightElements(string label, Color color, params IBufferedElement[] element);
        IBrowserOverlay HighlightElements(string label, Color color, IEnumerable<IBufferedElement> elements);
        void ShowFor(int showTimeout, string title);
        IList<Artifact> Artifacts { get; set; }
        void Show(string title);
        void ShowUntilNextKeyword(string title);
        Point ShowUntilClicked(string title, int timeout);
    }

    public class BrowserOverlay : IBrowserOverlay
    {
        private readonly IInterpreter _interpreter;
        private Task<Point> _overlayTask;
        public IAnnotationOverlay AnnotationOverlay { get; set; }

        public BrowserOverlay(IAnnotationOverlay annotationOverlay, IInterpreter interpreter)
        {
            _interpreter = interpreter;
            AnnotationOverlay = annotationOverlay;
        }

        public IBrowserOverlay HighlightElements(string label, Color color, params IBufferedElement[] element)
        {
            return HighlightElements(label, color, element.ToList());

        }

        public IBrowserOverlay HighlightElements(string label, Color color, IEnumerable<IBufferedElement> elements)
        {
            var darkerColor = Color.FromArgb(ModifyColor(color.R), ModifyColor(color.G), ModifyColor(color.B));
            foreach (var element in elements)
            {
                AnnotationOverlay.Highlight(
                    label,
                    element.Rectangle,
                    color,
                    darkerColor);
            }

            return this;
        }

        private static int ModifyColor(byte modifyColor)
        {
            return (int)Math.Min(255, modifyColor*0.6);
        }

        public void ShowFor(int showTimeout, string title)
        {
            AnnotationOverlay.RunSync(title, showTimeout, clickThrough: true);
        }

        public IList<Artifact> Artifacts
        {
            get => AnnotationOverlay.Artifacts;
            set => AnnotationOverlay.Artifacts = value;
        }

        public void Show(string title)
        {
            AnnotationOverlay.RunSync(title, Int32.MaxValue, clickThrough: true);
        }

        public void ShowUntilNextKeyword(string title)
        {
            _overlayTask = AnnotationOverlay.RunAsync(title, Int32.MaxValue, clickThrough: true);

            _interpreter.StepStarted += HandleHide;
        }

        public Point ShowUntilClicked(string title, int timeout)
        {
            Console.Out.WriteColoredLine(ConsoleColor.White,"Please click on an element.");
            AnnotationOverlay.RunSync(title, Int32.MaxValue, clickThrough: false);
            return AnnotationOverlay.ClickResult;
        }

        private void HandleHide(object sender, StepEventArgs e)
        {
            if (e.Keyword is IProtected)
            {
                return;
            }

            _interpreter.StepStarted -= HandleHide;

            AnnotationOverlay.Stop();

            var result = _overlayTask.Result;
            if (_overlayTask != null && _overlayTask.Exception != null)
            {
                throw _overlayTask.Exception;
            }
        }
    }
}