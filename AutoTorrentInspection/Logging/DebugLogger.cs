// ****************************************************************************
//
// Copyright (C) 2014 jirkapenzes (jirkapenzes@gmail.com)
// Copyright (C) 2017 TautCony (TautCony@vcb-s.com)
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.If not, see<http://www.gnu.org/licenses/>.
//
// ****************************************************************************

using System;

namespace AutoTorrentInspection.Logging
{
    public class DebugLogger
    {
        private const Logger.Level DebugLevel = Logger.Level.Debug;

        public void Log()
        {
            Log("There is no message");
        }

        public void Log(string message)
        {
            Logger.Log(DebugLevel, message);
        }

        public void Log(Exception exception)
        {
            Logger.Log(DebugLevel, exception.Message);
        }

        public void Log<TClass>(Exception exception) where TClass : class
        {
            var message = $"Log exception -> Message: {exception.Message}\nStackTrace: {exception.StackTrace}";
            Logger.Log<TClass>(DebugLevel, message);
        }

        public void Log<TClass>(string message) where TClass : class
        {
            Logger.Log<TClass>(DebugLevel, message);
        }
    }
}
