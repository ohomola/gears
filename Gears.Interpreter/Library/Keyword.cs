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
using System.Xml.Serialization;
using Gears.Interpreter.Adapters;
using Gears.Interpreter.Applications.Debugging;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data;
using Gears.Interpreter.Data.Core;

namespace Gears.Interpreter.Library
{
    public interface IKeyword
    {
    }

    public enum KeywordResultSpecialCases
    {
        Skipped
    }
    

    public abstract class Keyword : Runnable, IKeyword
    {
        [XmlIgnore]
        [Wire]
        public virtual ITypeRegistry TypeRegistry { get; set; }

        [Wire]
        [XmlIgnore]
        protected virtual IDataContext Data { get; set; }

        [Wire]
        [XmlIgnore]
        public virtual ISeleniumAdapter Selenium { get; set; }

        [XmlIgnore]
        public virtual string Skip { get; set; }

        public virtual string Status { get; set; }
        
        public virtual string StatusDetail { get; set; }

        public virtual object Result { get; set; }

        public virtual object Expect { get; set; }

        [XmlIgnore]
        public virtual double Time { get; set; }


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

                var result = keyword.Run();

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
                    Console.Out.WriteColoredLine(ConsoleColor.Green, $"Result was {keyword.Result} as expected.");
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

        public virtual void Hydrate()
        {
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