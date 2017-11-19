using System;
using System.Diagnostics;
using System.Linq;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Data;
using Gears.Interpreter.Core.Data.Core;
using Gears.Interpreter.Core.Interpretation;
using Gears.Interpreter.Core.Registrations;

namespace Gears.Interpreter.App.Workflow.Library
{
    [NotLogged]
    [HelpDescription("open <file> \t-\t loads steps from a scenario file")]
    public class Open : Keyword, IHasTechnique
    {
        public string File { get; set; }

        public Technique Technique { get; set; }

        public override string CreateDocumentationMarkDown()
        {
            return base.CreateDocumentationMarkDown() +
                   $@"Opens a specified scenario file.
#### Console usage
    open c:/mytest.csv";
        }

        public override object DoRun()
        {
            if (string.IsNullOrEmpty(File))
            {
                throw new ArgumentException("Filename must be specified.");
            }

            if (File.ToLower().EndsWith(".dll"))
            {
                var file = FileFinder.Find(File);

                if (Technique == Technique.Show)
                {
                    return new InformativeAnswer($"Dll file exists {file}");
                }

                var pluggedInTypes = TypeRegistry.Register(file);

                if (pluggedInTypes.Any())
                {
                    return new SuccessAnswer($"Loaded {pluggedInTypes.Count()} plugins from \n\t {file}");
                }
                else
                {
                    return new WarningAnswer("No plugins were loaded.");
                }
            }
            else
            {
                var file = FileFinder.Find(File);

                if (Technique == Technique.Show)
                {
                    Process.Start("explorer.exe", file);
                    return new InformativeAnswer($"Opening file {file}");
                }

                var fileObjectAccess = new FileObjectAccess(file, ServiceLocator.Instance.Resolve<ITypeRegistry>());
                var data = fileObjectAccess.GetAll();

                Interpreter.Data.Include(fileObjectAccess);

                Interpreter.Plan = data.OfType<IKeyword>();

                var corruptObjects = data.OfType<CorruptObject>();
                if (corruptObjects.Any())
                {
                    return
                        new ExceptionAnswer("Invalid Keywords found in file.").With(
                            new ExceptionAnswer(string.Join("\n\t", corruptObjects)));
                }

                return new SuccessAnswer("File loaded successfully.");
            }
        }

        public override IKeyword FromString(string textInstruction)
        {
            return new Open()
            {
                File = ExtractSingleParameterFromTextInstruction(textInstruction)
            };
        }

        
    }
}