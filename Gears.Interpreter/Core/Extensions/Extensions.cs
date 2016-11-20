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
using System.Diagnostics;

namespace Gears.Interpreter.Core.Extensions
{
    public static class ExceptionExtensions
    {
        [DebuggerStepThrough]
        public static string GetAllMessages(this Exception exp)
        {
            string message = string.Empty;
            Exception innerException = exp;

            do
            {
                message = message + "\n" + (string.IsNullOrEmpty(innerException.Message) ? string.Empty : innerException.Message);
                innerException = innerException.InnerException;
            }
            while (innerException != null);

            return message;
        }
    };
}