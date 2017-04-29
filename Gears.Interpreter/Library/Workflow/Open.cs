﻿using System;
using System.Linq;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data;
using Gears.Interpreter.Data.Core;

namespace Gears.Interpreter.Library.Workflow
{
    [NotLogged]
    [UserDescription("open <file> \t-\t loads steps from a scenario file")]
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

            var fileObjectAccess = new FileObjectAccess(File, ServiceLocator.Instance.Resolve<ITypeRegistry>());
            var data = fileObjectAccess.GetAll();

            Interpreter.Data.Include(fileObjectAccess);

            Interpreter.Plan = data.OfType<IKeyword>();

            var corruptObjects = data.OfType<CorruptObject>();
            if (corruptObjects.Any())
            {
                return new ExceptionAnswer("Invalid Keywords found in file.").With(new ExceptionAnswer(string.Join("\n\t", corruptObjects)));
            }

            return new SuccessAnswer("File loaded successfully.");
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