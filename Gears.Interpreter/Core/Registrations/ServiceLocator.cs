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
using Castle.Windsor;

namespace Gears.Interpreter.Core.Registrations
{
    //TODO: ---- Remove dependency on Windsor from core and pull up to application
    //TODO: --- wrap staticcontainer to some 'servicelocator' interface which is optional for implementation
    public static class ServiceLocator
    {
        private static bool _isInitialised;

        public static WindsorContainer Instance;

        public static bool IsInitialised()
        {
            return _isInitialised;
        }

        public static void Initialise(WindsorContainer container)
        {
            Instance = container;

            _isInitialised = true;
        }
    }
}
