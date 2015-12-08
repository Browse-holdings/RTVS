﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.R.Host.Client;

namespace Microsoft.VisualStudio.R.Package.Repl.Session {

    [Export(typeof(IRSessionProvider))]
    public class RSessionProvider : IRSessionProvider {
        private readonly ConcurrentDictionary<int, IRSession> _sessions = new ConcurrentDictionary<int, IRSession>();

        public IRSession Create(int sessionId) {
            IRSession session = new RSession(sessionId);
            IRSession currentSession = this.Current;

            if (!_sessions.TryAdd(sessionId, session)) {
                Debug.Fail(string.Format(CultureInfo.InvariantCulture, "Session with id {0} is created already", sessionId));
                return _sessions[sessionId];
            }

            IRSession currentSessionAfterAdd = this.Current;

            if (!Equals(currentSession, currentSessionAfterAdd)) {
                CurrentSessionChanged?.Invoke(this, EventArgs.Empty);
            }

            return session;
        }

        public IReadOnlyDictionary<int, IRSession> GetSessions() {
            return new Dictionary<int, IRSession>(_sessions);
        }

        public IRSession Current => _sessions.Values.FirstOrDefault();

        public event EventHandler CurrentSessionChanged;

        public void Dispose() {
            foreach (var session in _sessions.Values) {
                session.Dispose();
            }

            _sessions.Clear();
        }
    }
}