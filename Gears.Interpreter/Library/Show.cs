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
        public override object DoRun()
        {
            throw new NotImplementedException();
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
                (keyword as IHasTechnique).Technique = Technique.HighlightOnly;
                return keyword;
            }
            
            throw new ArgumentException($"Cannot highlight {keyword.GetType().Name} because it does not support this operation. Only the following keywords are supported: \n\t{string.Join("\n\t", Interpreter.Language.Keywords.OfType<IHasTechnique>().Select(x=>" - "+x.GetType().Name))}\n");
        }
    }
}