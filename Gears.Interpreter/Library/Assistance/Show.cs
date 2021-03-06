﻿using System;
using System.Linq;
using System.Xml.Serialization;
using Gears.Interpreter.App;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Extensions;
using Gears.Interpreter.Core.Interpretation;
using Gears.Interpreter.Library.UI;

namespace Gears.Interpreter.Library.Assistance
{
    [NotLogged]
    [HelpDescription("show (command)\t-\t shows a current keyword or another command (if specified)")]
    public class Show : Keyword
    {
        [Wire]
        [XmlIgnore]
        public virtual IInterpreter Interpreter { get; set; }

        private IKeyword _keyword;

        public override object DoRun()
        {
            if (_keyword == null)
            {
                _keyword = ParseInstruction(_instruction, Interpreter);
            }

            var tech = (_keyword as IHasTechnique);
            var oldTechnique = tech.Technique;
            tech.Technique = Technique.Show;

            var childAnswer = _keyword.Execute() as IInformativeAnswer;
            tech.Technique = oldTechnique;
            return new SuccessAnswer("Done.").With(childAnswer);
        }

        public override string CreateDocumentationMarkDown()
        {
            return $@"
{base.CreateDocumentationMarkDown()}
Displays the inteded action. Used for troubleshooting or while developing new tests.

When used without parameter, will try to show selected plan keyword. Note that not all keywords are 'showable' (typically only UI actions are showable)

Other usage is prefixing other keyword console commands with the word show. This will perform the same as above, except against the specified command.

#### Console usage
    show
    show click 1st login button
    show fill textfield above password
    show csvscenarioreport

> Note: Show will use an overlay window to indicate the location of the element. This will stay displayed until a console prompt is confirmed. Note that this blocks your test until user performs an action so it is not recommended to use in live tests.
";
        }

        public Show()
        {
        }

        public Show(IKeyword keyword)
        {
            this._keyword = keyword;
        }

        public string What
        {
            set => Instruction = value;
        }


        private string _instruction;
        public override string Instruction
        {
            set
            {
                _instruction = value;
            }
        }

        private static IKeyword ParseInstruction(string value, IInterpreter interpreter)
        {
            var cmd = value;

            IKeyword keyword = null;

            if (string.IsNullOrEmpty(cmd))
            {
                keyword = interpreter.Iterator.Current;
            }
            else if (interpreter.Language.CanParse(cmd))
            {
                keyword = interpreter.Language.ParseKeyword(cmd);
            }

            if (keyword == null)
            {
                throw new ArgumentException(
                    "Incorrect syntax. Use show as a prefix for other commands. E.g. show click 1st Submit button, or when a loaded step is selected.");
            }
            if (!(keyword is IHasTechnique))
            {
                throw new ArgumentException(
                    $"Cannot highlight {keyword.GetType().Name} because it does not support this operation. Only the following keywords are supported: \n\t{string.Join("\n\t", interpreter.Language.Keywords.OfType<IHasTechnique>().Select(x => " - " + x.GetType().Name))}\n");
            }

            return keyword;
        }
    }
}