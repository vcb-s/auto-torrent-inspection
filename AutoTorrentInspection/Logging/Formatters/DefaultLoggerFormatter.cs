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

namespace AutoTorrentInspection.Logging.Formatters
{
    internal class DefaultLoggerFormatter : ILoggerFormatter
    {
        public string ApplyFormat(LogMessage logMessage)
        {
#if DEBUG
            return $"{logMessage.DateTime:yyyy.MM.dd HH:mm:ss} [line: {logMessage.LineNumber} {logMessage.CallingClass} -> {logMessage.CallingMethod}()]: {logMessage.Level}: {logMessage.Text}";
#else
            return $"{logMessage.DateTime:yyyy.MM.dd HH:mm:ss} [{logMessage.CallingMethod}()]: {logMessage.Level}: {logMessage.Text}";
#endif
        }
    }
}
