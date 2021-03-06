﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gears.Interpreter.App.Configuration;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Interpretation;

namespace Gears.Interpreter.App.Workflow.Library
{
    [NotLogged]
    [HelpDescription("use (X)\t-\t adds Keyword X to context. (e.g. 'use junitscenarioreport' to register ScenarioReport.")]
    public class Use : Keyword
    {
        public Use(string what)
        {
            What = what;
        }

        public Use()
        {
        }


        public override string CreateDocumentationMarkDown()
        {
            return $@"
{base.CreateDocumentationMarkDown()}
Adds object of specified Type to Context. Use this keyword to turn on a configuration.

#### Console usages
    use SkipAssertions
    run
    off SkipAssertions

### Additional properties
* [Common Keyword properties](Documentation#common-keyword-properties)  
";
        }

        public override object DoRun()
        {
            object instance = null;
            var arguments = new List<object>();
            if (Interpreter.Language.CanParse(What.ToLower()))
            {
                instance = Interpreter.Language.ParseKeyword(What.ToLower());
            }
            else
            {
                var type = TypeRegistry.Types.FirstOrDefault(x => x.Name.ToLower() == What.ToLower());
                if (type == null && What.Contains(" "))
                {
                    var firstPart = What.Split(' ')[0];
                    arguments.Add(What.Substring(firstPart.Length+1));
                    type = TypeRegistry.Types.FirstOrDefault(x => x.Name.ToLower() == firstPart.ToLower());
                }
                if (type == null)
                {
                    throw new ArgumentException($"{What} is not recognized.");
                }
                
                instance = Activator.CreateInstance(type, args:arguments.ToArray());
            }

            Data.Add(instance);

            (instance as IAutoRegistered)?.Register(Interpreter);

            if (instance is IKeyword)
            {
                Interpreter.AddToPlan(instance as IKeyword);
            }

            return new SuccessAnswer($"Added {instance} to Data Context.");
        }

        public override string Instruction
        {
            set { this.What = value; }
        }

        public string What { get; set; }

        public override string ToString()
        {
            return $"Use '{What}'";
        }
    }
}