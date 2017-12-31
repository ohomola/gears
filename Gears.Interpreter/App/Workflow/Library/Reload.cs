using System.Linq;
using Castle.Core.Internal;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Data;
using Gears.Interpreter.Core.Interpretation;

namespace Gears.Interpreter.App.Workflow.Library
{
    [NotLogged]
    [HelpDescription("reload (x) \t-\t re-reads all input files. Specify X to only reload file with specified text in it's path.")]
    public class Reload : Keyword
    {
        public Reload()
        {
        }

        public Reload(string what)
        {
            What = what;
        }

        public string What { get; set; }

        public override string Instruction
        {
            set { What = value; }
        }


        public override string CreateDocumentationMarkDown()
        {
            return base.CreateDocumentationMarkDown() +
                   $@"Reloads the scenario. Use this along with a shared xls document to build automated tests on-the-fly. Specify a text parameter to only reload a specific file which contains the parameter in it's path.
#### Console usage
    reload
    open c:/myfile_withTest123.xlsx
    reload myfile
";
        }

        public override object DoRun()
        {
            var response = "Reload completed.";

            if (What.IsNullOrEmpty())
            {
                var fileObjectAccesses = Interpreter.Data.DataAccesses.OfType<FileObjectAccess>();
                foreach (var dataAccess1 in fileObjectAccesses)
                {
                    dataAccess1.ForceReload();
                }
                response += $"\n{fileObjectAccesses.Count()} files reloaded.";
            }
            else
            {
                var foa = Interpreter.Data.DataAccesses.OfType<FileObjectAccess>()
                    .FirstOrDefault(x => x.Path.ToLower().Contains(What.ToLower()));

                if (foa == null)
                {
                    return new ExceptionAnswer(
                        $"File with '{What}' is not registered. You must specify a full name or a substring of a loaded file (case-insensitive, no spaces). The following files are registered: \n\t"+string.Join("\n\t", Interpreter.Data.DataAccesses.OfType<FileObjectAccess>().Select(x=>x.Path)));
                }

                foa?.ForceReload();

                response += $"\n File reloaded: {foa.Path}";
            }


            Interpreter.Plan = Interpreter.Data.GetAll<Keyword>().ToList();

            if (Interpreter.Iterator.Index >= Interpreter.Plan.ToList().Count)
            {
                Interpreter.Iterator.Index = 0;
            }
            
            return new SuccessAnswer(response);
        }
    }
}