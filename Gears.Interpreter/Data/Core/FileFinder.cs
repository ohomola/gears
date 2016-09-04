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
using System.IO;
using System.Linq;
using System.Reflection;

namespace Gears.Interpreter.Data.Core
{
    public static class FileFinder
    {
        private static string[] GetFilesFrom(string root, string path)
        {
            string absoluteRoot = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), root);

            if (!Directory.Exists(absoluteRoot))
            {
                return null;
            }

            return Directory.GetFiles(absoluteRoot, path, SearchOption.AllDirectories);
        }

        public static string Find(string fileName)
        {
            if (File.Exists(fileName))
            {
                var fullPath = Path.GetFullPath(fileName);
                return fullPath;
            }

            var files = GetFilesFrom(".", fileName);

            if (files == null || !files.Any())
            {
                files = GetFilesFrom("../bin", fileName);
            }

            return files.FirstOrDefault();
        }
    }
}
