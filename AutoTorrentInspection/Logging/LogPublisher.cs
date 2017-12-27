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
using System.Collections.Generic;

namespace AutoTorrentInspection.Logging
{
    internal class FilteredHandler : ILoggerHandler
    {
        public Predicate<LogMessage> Filter { get; set; }
        public ILoggerHandler Handler { get; set; }

        public void Publish(LogMessage logMessage)
        {
            if (Filter(logMessage))
                Handler.Publish (logMessage);
        }
    }

    internal class LogPublisher : ILoggerHandlerManager
    {
        private readonly IList<ILoggerHandler> _loggerHandlers;
        private readonly IList<LogMessage> _messages;

        public LogPublisher()
        {
            _loggerHandlers = new List<ILoggerHandler>();
            _messages = new List<LogMessage>();
            StoreLogMessages = false;
        }

        public LogPublisher(bool storeLogMessages)
        {
            _loggerHandlers = new List<ILoggerHandler>();
            _messages = new List<LogMessage>();
            StoreLogMessages = storeLogMessages;
        }

        public void Publish(LogMessage logMessage)
        {
            if (StoreLogMessages)
                _messages.Add(logMessage);
            foreach (var loggerHandler in _loggerHandlers)
                loggerHandler.Publish(logMessage);
        }

        public ILoggerHandlerManager AddHandler(ILoggerHandler loggerHandler)
        {
            if (loggerHandler != null)
                _loggerHandlers.Add(loggerHandler);
            return this;
        }

        public ILoggerHandlerManager AddHandler(ILoggerHandler loggerHandler, Predicate<LogMessage> filter)
        {
            if (filter == null || loggerHandler == null)
                return this;

            return AddHandler(new FilteredHandler() {
                Filter = filter,
                Handler = loggerHandler
            });
        }

        public bool RemoveHandler(ILoggerHandler loggerHandler)
        {
            return _loggerHandlers.Remove(loggerHandler);
        }

        public IEnumerable<LogMessage> Messages => _messages;

        public bool Clear()
        {
            if (!StoreLogMessages)
                return false;
            _messages.Clear();
            return true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="LogPublisher"/> store log messages.
        /// </summary>
        /// <value><c>true</c> if store log messages; otherwise, <c>false</c>. By default is <c>false</c></value>
        public bool StoreLogMessages { get; set; }
    }
}
