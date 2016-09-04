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
using System;
using System.Xml.Serialization;
using Gears.Interpreter.Core.Registrations;

namespace Gears.Interpreter.Core
{
    public interface IRunnable
    {
        object Run();
    }

   
    public abstract class Runnable : IRunnable
    {
        protected Runnable()
        {
            Guid = Guid.NewGuid();

            if (ServiceLocator.IsInitialised())
            {
                ServiceLocator.Instance.Resolve(this);
            }
        }

        [XmlIgnore]
        public Guid Guid { get; set; }
        
        public abstract object Run();

        public override bool Equals(object obj)
        {
            if (obj is Runnable)
            {
                return Guid.Equals(((Runnable)obj).Guid);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Guid.GetHashCode();
        }

        
    }
}