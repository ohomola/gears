#region LICENSE
/*
Copyright 2016 Ondrej Homola <ondra.homola@gmail.com>

This file is part of Gears, a software automation and assistance framework.

Gears is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Gears is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Gears.Interpreter.Adapters;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Applications.Debugging;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data;
using Gears.Interpreter.Data.Core;
using Gears.Interpreter.Library.Workflow;

namespace Gears.Interpreter.Library
{
    public interface IKeyword
    {
        bool Matches(string textInstruction);
        IKeyword FromString(string textInstruction);
        object Execute();
        string Status { get; set; }
        object Expect { get; set; }
        string GetUserDescription();
        string CreateDocumentationMarkDown();
    }

    public enum KeywordResultSpecialCases
    {
        Skipped
    }
    

    public abstract class Keyword : IKeyword
    {
        protected Keyword()
        {
            Guid = Guid.NewGuid();
        }

        public virtual string CreateDocumentationMarkDown()
        {
            return $"## {this.GetType().Name}\n";
        }

        [XmlIgnore]
        [Wire]
        public virtual ITypeRegistry TypeRegistry { get; set; }

        [Wire]
        [XmlIgnore]
        protected virtual IDataContext Data { get; set; }

        [Wire]
        [XmlIgnore]
        public virtual ISeleniumAdapter Selenium { get; set; }

        [Wire]
        [XmlIgnore]
        public virtual IInterpreter Interpreter { get; set; }

        [XmlIgnore]
        public virtual string Skip { get; set; }

        public virtual string Status { get; set; } = KeywordStatus.NotExecuted.ToString();


        public string GetUserDescription()
        {
            var attribute = this.GetType().GetCustomAttributes(true).FirstOrDefault(x => x is UserDescriptionAttribute) as UserDescriptionAttribute;

            return attribute?.Description;
        }

        public virtual string StatusDetail { get; set; }

        public virtual object Result { get; set; }

        public virtual object Expect { get; set; }

        [XmlIgnore]
        public virtual double Time { get; set; }

        public virtual bool Matches(string textInstruction)
        {
            return textInstruction.ToLower().Trim().StartsWith(this.GetType().Name.ToLower());
        }

        public virtual IKeyword FromString(string textInstruction)
        {
            //return (IKeyword) Activator.CreateInstance(TypeRegistry.GetDTOTypes().First(x=> textInstruction.StartsWith(x.Name.ToLower())));
            var type = TypeRegistry.GetDTOTypes().First(x => textInstruction.Equals(x.Name, StringComparison.InvariantCultureIgnoreCase));
            return (IKeyword) ServiceLocator.Instance.Resolve(type);
        }

        public abstract object DoRun();

        public static bool IsLogged(IKeyword keyword)
        {
            if (keyword.GetType().GetCustomAttributes(true).Any(x => x is NotLoggedAttribute))
            {
                return false;
            }

            return true;
        }

        public virtual object Execute()
        {
            try
            {
                var keyword = this;

                if (ServiceLocator.IsInitialised())
                {
                    ServiceLocator.Instance.Resolve(keyword);
                }

                keyword.Status = KeywordStatus.Ok.ToString();

                if (!string.IsNullOrEmpty(keyword.Skip))
                {
                    keyword.Status = KeywordStatus.Skipped.ToString();
                    return KeywordResultSpecialCases.Skipped;
                }

                DateTime start = DateTime.Now;

                var result = keyword.DoRun();

                DateTime end = DateTime.Now;

                keyword.Result = result;

                //TODO : this will need more thought - result triage is a totally separate concern
                if (keyword.Expect != null && keyword.Result != null &&
                    keyword.Expect.ToString().ToLower() != keyword.Result.ToString().ToLower())
                {
                    throw new ApplicationException(
                        $"{keyword} expected \'{keyword.Expect}\' but was \'{keyword.Result}\'");
                }

                if (keyword.Expect != null && keyword.Result != null)
                {
                    keyword.Result = new SuccessAnswer(keyword.Result);
                    //Console.Out.WriteColoredLine(ConsoleColor.Green, $"Result was {keyword.Result} as expected.");
                }

                keyword.Time = (end - start).TotalSeconds;
            }
            catch (ApplicationException ae)
            {
                Status = KeywordStatus.Error.ToString();
                StatusDetail = ae.Message;
                throw;
            }
            catch (Exception exception)
            {
                Status = KeywordStatus.Error.ToString();
                StatusDetail = exception.Message;
                throw;
            }

            return Result;
        }

        public bool IsLazy()
        {
            return this.GetType().Name.Contains("Proxy");
        }

        public bool IsLazyHydrated()
        {
            return IsLazy() && IsHydrated;
        }

        public virtual bool IsHydrated { get; set; }

        [XmlIgnore]
        public virtual Guid Guid { get; set; }

        public virtual void Hydrate()
        {
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Keyword) obj);
        }

        protected bool Equals(Keyword other)
        {
            return Guid.Equals(other.Guid);
        }

        public override int GetHashCode()
        {
            return Guid.GetHashCode();
        }

        protected static string ExtractSingleParameterFromTextInstruction(string textInstruction)
        {
            var strings = textInstruction.Split(' ');

            return string.Join(" ", strings.Skip(1));
        }

        protected static string []ExtractTwoParametersFromTextInstruction(string textInstruction)
        {
            var strings = textInstruction.Split(' ');

            if (strings.Length < 3)
            {
                throw new ArgumentException("Must provide two parameters.");
            }

            return new []
            {
                strings.Skip(1).First(),
                string.Join(" ", strings.Skip(2))
            };
        }
    }
    public enum KeywordStatus
    {
        NotExecuted,
        Ok,
        Error,
        Skipped,
    }
}