#region LICENSE
/*
Copyright 2016 Ondrej Homola <ondra.homola@gmail.com>

This file is part of Gears.

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
using System.Xml.Serialization;
using Gears.Interpreter.Adapters;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Applications.Configuration;
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

        
    }

    public enum KeywordStatus
    {
        Ok,
        Error,
        Skipped,
    }
}