﻿using Gears.Interpreter.Library.Workflow;

namespace Gears.Interpreter.Library
{
    public class IsTrue : Keyword, IProtected
    {
        public bool Predicate { get; }

        public IsTrue(bool predicate)
        {
            this.Predicate = predicate;
        }

        public IsTrue()
        {
        }

        public string Message { get; set; }

        public override object DoRun()
        {
            return Predicate;
        }

        public override string ToString()
        {
            return this.GetType().Name +" ("+ Message+")";
        }
    }
}