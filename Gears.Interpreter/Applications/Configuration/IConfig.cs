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
namespace Gears.Interpreter.Applications.Configuration
{
    /// <summary>
    /// Configuration DTOs is a marker interface which distinguishes classes whose 
    /// purpose is to carry configuration data to the application.
    /// These objects are either pre-bound to the application on start-time, or are provided in run-time as part of main data feed.
    /// 
    /// All IConfig class instances are registered in IoC container under their type's service on start-up.
    /// </summary>
    public interface IConfig
    {
    }
}