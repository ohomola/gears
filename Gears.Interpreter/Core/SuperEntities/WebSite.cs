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
namespace Gears.Interpreter.Core.SuperEntities
{
    public class WebSite
    {
        public override string ToString()
        {
            return this.GetType().Name + " with URL: " + BaseUrl;
        }

        public string BaseUrl { get; set; }
        
        public string LoginPage { get; set; }

        public string LoginPageUrl
        {
            get { return BaseUrl + LoginPage ?? ""; }
        }
    }
}