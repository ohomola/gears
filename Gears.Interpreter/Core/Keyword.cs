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
using System.Threading;
using System.Xml.Serialization;
using Castle.Core.Internal;
using Gears.Interpreter.App;
using Gears.Interpreter.Core.Adapters.UI;
using Gears.Interpreter.Core.ConfigObjects;
using Gears.Interpreter.Core.Data;
using Gears.Interpreter.Core.Data.Core;
using Gears.Interpreter.Core.Extensions;
using Gears.Interpreter.Core.Interpretation;
using Gears.Interpreter.Core.Library;
using Gears.Interpreter.Core.Registrations;
using JetBrains.Annotations;

namespace Gears.Interpreter.Core
{
    public abstract class Keyword : IKeyword
    {
        // TOP-LEVEL RICH SYNTACTIC PROPERTY
        [CanBeNull]
        public string Specification
        {
            set => FromString(value);
        }

        // SEMANTIC PROPERTIES:
        public virtual bool Skip { get; set; }
        public virtual object Expect { get; set; }
        public virtual int WaitAfter { get; set; }
        public virtual int WaitBefore { get; set; }
        public string ScreenshotAfter { get; set; }

        protected Keyword()
        {
            Guid = Guid.NewGuid();
            Data = new DataContext();
        }

        public virtual string CreateDocumentationMarkDown()
        {
            return $"## {this.GetType().Name}\n";
        }

        public virtual string CreateDocumentationTypeName()
        {
            return this.GetType().Name;
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

        public virtual string Status { get; set; } = KeywordStatus.NotExecuted.ToString();

        private string _additionalHelpDescription;

        [XmlIgnore]
        public virtual string HelpDescription
        {
            get
            {
                var attribute =
                    this.GetType().GetCustomAttributes(true).FirstOrDefault(x => x is HelpDescriptionAttribute) as
                        HelpDescriptionAttribute;

                return attribute?.Description + _additionalHelpDescription;
            }

            protected set => _additionalHelpDescription = value;
        }

        public virtual string StatusDetail { get; set; }

        public virtual object Result { get; set; }

        [XmlIgnore]
        public virtual double Time { get; set; }

        public virtual bool Matches(string textInstruction)
        {
            if (textInstruction == null)
            {
                return false;
            }

            var actualName = textInstruction.Split(' ').First().Trim();

            var expectedName = this.GetType().Name;

            return actualName.Equals(expectedName, StringComparison.OrdinalIgnoreCase);
        }

        public virtual void FromString(string textInstruction)
        {
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
            var keyword = this;

            try
            {
                if (ServiceLocator.IsInitialised())
                {
                    ServiceLocator.Instance.Resolve(keyword);
                }

                if (keyword is IAssertion && Data != null &&  Data.Contains<SkipAssertions>())
                {
                    keyword.Status = KeywordStatus.Skipped.ToString();
                    return new InformativeAnswer($"Assertions are turned off.\nSkipping:\n\t {keyword}");
                }

                keyword.Status = KeywordStatus.Ok.ToString();

                if (keyword.Skip)
                {
                    keyword.Status = KeywordStatus.Skipped.ToString();
                    return KeywordResultSpecialCases.Skipped;
                }

                if (WaitBefore != default(int))
                {
                    Thread.Sleep(WaitBefore);
                }

                DateTime start = DateTime.Now;

                var result = keyword.DoRun();

                DateTime end = DateTime.Now;

                keyword.Result = result;

                if (WaitAfter != default(int))
                {
                    Thread.Sleep(WaitAfter);
                }

                if (!ScreenshotAfter.IsNullOrEmpty())
                {
                    bool isActive = false;
                    var isBool= bool.TryParse(ScreenshotAfter, out isActive);
                    if (isBool && isActive)
                    {
                        
                    }

                    if (!isBool)
                    {
                        new SaveScreenshot(ScreenshotAfter).Execute();
                    }
                }

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
                if (Data.Contains<ErrorLogging>())
                {
                    Data.Get<ErrorLogging>().Log(keyword, "ErrorInStep_" +this.Interpreter.Iterator.Index + this.GetType().Name);
                }

                Status = KeywordStatus.Error.ToString();
                StatusDetail = ae.Message;
                throw;
            }
            catch (Exception exception)
            {
                if (Data.Contains<ErrorLogging>())
                {
                    Data.Get<ErrorLogging>().Log(keyword, "ErrorInStep_" + this.Interpreter.Iterator.Index + this.GetType().Name);
                }

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

        /// <summary>
        /// Targeted by proxy resolver
        /// </summary>
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

        //TODO move
        public static string ExtractSingleParameterFromTextInstruction(string textInstruction)
        {
            var strings = textInstruction.Split(' ');

            return string.Join(" ", strings.Skip(1));
        }

        protected static string ReverseExtractSingleParameterFromTextInstruction(string textInstruction)
        {
            var strings = textInstruction.Split(' ');

            return string.Join(" ", strings.Take(strings.Length-1));
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
}