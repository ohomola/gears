using Gears.Interpreter.Applications;
using Gears.Interpreter.Applications.Debugging.Overlay;

namespace Gears.Interpreter.Library
{
    public class WhatIs : Keyword
    {
        private readonly int _x;
        private readonly int _y;

        private WhatIs(int x, int y)
        {
            _x = x;
            _y = y;
        }

        public WhatIs()
        {
        }

        public override IKeyword FromString(string textInstruction)
        {
            var args = ExtractTwoParametersFromTextInstruction(textInstruction);
            return new WhatIs(int.Parse(args[0]), int.Parse(args[1]));
        }

        public override object DoRun()
        {

            Highlighter.Prompt(Selenium);



            return new InformativeAnswer($"x={_x} y={_y}");
        }
    }
}