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
using System.IO;
using Gears.Interpreter.Core.Registrations;

namespace Gears.Interpreter.Data.Core
{
    public class Include
    {
        public Include(string fileName)
        {
            FileName = fileName;
        }

        public string FileName { get; set; }
        public string Description { get; set; }

        public IEnumerable<object> RecursiveRead(string parentPath)
        {
            if (string.IsNullOrEmpty(FileName))
            {
                throw new InvalidOperationException("An Include object was passed to the system without FileName specified");
            }

            var fullPath = FileFinder.Find(FileName);

            if (File.Exists(fullPath))
            {
                return new FileObjectAccess(fullPath, ServiceLocator.Instance.Resolve<ITypeRegistry>()).GetAll();
            }

            var directoryName = System.IO.Path.GetDirectoryName(parentPath);
            if (directoryName != null)
            {
                fullPath = System.IO.Path.Combine(directoryName, FileName);
                if (File.Exists(fullPath))
                {
                    return new FileObjectAccess(fullPath, ServiceLocator.Instance.Resolve<ITypeRegistry>()).GetAll();
                }
            }

            try
            {
                var includedData = new FileObjectAccess(FileFinder.Find(FileName), ServiceLocator.Instance.Resolve<ITypeRegistry>());

                return includedData.GetAll();
            }
            catch (Exception)
            {
                throw new IOException($"Included file '{FileName}' in '{this.ToString()}' was not found.");
            }
        }
    }
}
