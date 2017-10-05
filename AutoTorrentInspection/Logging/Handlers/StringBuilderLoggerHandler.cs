// ****************************************************************************
//
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
using System.Text;
using AutoTorrentInspection.Logging.Formatters;

namespace AutoTorrentInspection.Logging.Handlers
{
    public class StringBuilderLoggerHandler : ILoggerHandler
    {
        private readonly ILoggerFormatter _loggerFormatter;

        private readonly StringBuilder _builder;

        public StringBuilderLoggerHandler() : this(new DefaultLoggerFormatter()) { }

        public StringBuilderLoggerHandler(ILoggerFormatter loggerFormatter)
        {
            _loggerFormatter = loggerFormatter;
        }

        public StringBuilderLoggerHandler(StringBuilder builder) : this(new DefaultLoggerFormatter())
        {
            _builder = builder;
        }

        public StringBuilderLoggerHandler(ILoggerFormatter loggerFormatter, StringBuilder builder) : this(loggerFormatter)
        {
            _builder = builder;
        }

        public void Publish(LogMessage logMessage)
        {
            _builder.AppendLine(_loggerFormatter.ApplyFormat(logMessage));
        }
    }
}