using System.Collections.Generic;
using Gears.Interpreter.App.UI.Overlay;

namespace Gears.Interpreter.Core.Interpretation
{
    public class OverlayAnswer : InformativeAnswer
    {
        public IEnumerable<Artifact> Artifacts { get; }

        public OverlayAnswer(IEnumerable<Artifact> artifacts, object response) : base(response)
        {
            Artifacts = artifacts;
        }
    }
}