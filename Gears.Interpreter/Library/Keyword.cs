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
using Gears.Interpreter.Applications;
using Gears.Interpreter.Applications.Configuration;
using Gears.Interpreter.Applications.Debugging;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data;
using Gears.Interpreter.Data.Core;
using NUnit.Framework;

namespace Gears.Interpreter.Library
{
    public interface IKeyword
    {
    }

    public abstract class Keyword : Runnable, IKeyword
    {
        [XmlIgnore]
        [Wire]
        public ITypeRegistry TypeRegistry { get; set; }

        [Wire]
        [XmlIgnore]
        protected IDataContext Data { get; set; }

        [Wire]
        [XmlIgnore]
        public ISeleniumAdapter Selenium { get; set; }

        [XmlIgnore]
        public string Skip { get; set; }

        public string Description => ToString();

        public string Status { get; set; }
        
        public string StatusDetail { get; set; }

        public object Result { get; set; }

        public object Expect { get; set; }


        public object Execute()
        {
            var keyword = this;

            StringResolver.Resolve(keyword);

            var result = keyword.Run();

            keyword.Result = result;

            //TODO : this will need more thought - result triage is a totally separate concern
            if (keyword.Expect != null && keyword.Result != null &&
                keyword.Expect.ToString().ToLower() != keyword.Result.ToString().ToLower())
            {
                throw new ApplicationException(
                    $"{keyword} expected \'{keyword.Expect}\' but was \'{keyword.Result}\'");
            }
            else if (keyword.Expect != null && keyword.Result != null)
            {
                Console.Out.WriteColoredLine(ConsoleColor.Green, $"Result was {keyword.Result} as expected.");
            }

            return Result;
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