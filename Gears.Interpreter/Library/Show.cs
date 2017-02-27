using System;
using System.Linq;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Library.Workflow;

namespace Gears.Interpreter.Library
{
    [NotLogged]
    [UserDescription("show (command)\t-\t shows a current keyword or another command (if specified)")]
    public class Show : Keyword
    {
        private IKeyword keyword;

        public override object DoRun()
        {
            var tech = (keyword as IHasTechnique);
            var oldTechnique = tech.Technique;
            tech.Technique = Technique.HighlightOnly;
            keyword.Execute();
            tech.Technique = oldTechnique;
            return new SuccessAnswer("Done.");
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
            this.keyword = keyword;
        }

        public override IKeyword FromString(string textInstruction)
        {
            var cmd = ExtractSingleParameterFromTextInstruction(textInstruction);

            IKeyword keyword = null;

            if (string.IsNullOrEmpty(cmd))
            {
                keyword = Interpreter.Iterator.Current;
            }
            else if (Interpreter.Language.HasKeywordFor(cmd))
            {
                keyword = Interpreter.Language.ResolveKeyword(cmd);
            }
            
            if (keyword == null)
            {
                throw new ArgumentException("Incorrect syntax. Use show as a prefix for other commands. E.g. show click 1st Submit button, or when a loaded step is selected.");
            }

            if (keyword is IHasTechnique)
            {
                return new Show(keyword);
            }
            
            throw new ArgumentException($"Cannot highlight {keyword.GetType().Name} because it does not support this operation. Only the following keywords are supported: \n\t{string.Join("\n\t", Interpreter.Language.Keywords.OfType<IHasTechnique>().Select(x=>" - "+x.GetType().Name))}\n");
        }
    }
}