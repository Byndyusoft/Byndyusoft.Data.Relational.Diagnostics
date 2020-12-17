using System;
using System.Collections.Generic;
using Microsoft.Data.Diagnostics;

namespace Byndyusoft.Data.Relational.Diagnostics.Tests.Unit
{
    public static class DbDiagnosticSession
    {
        public static (string eventName, object payload) Execute(DbDiagnosticSource source, Action action)
        {
            string eventName = null;
            object payload = null;
            using var observer = source.Subscribe(new Observer(pair => (eventName, payload) = pair));
            action();
            return (eventName, payload);
        }

        private class Observer : IObserver<KeyValuePair<string, object>>
        {
            private readonly Action<KeyValuePair<string, object>> _onNext;

            public Observer(Action<KeyValuePair<string, object>> onNext = null)
            {
                _onNext = onNext;
            }

            public void OnCompleted()
            {
            }

            public void OnError(Exception error)
            {
            }

            public void OnNext(KeyValuePair<string, object> value)
            {
                _onNext.Invoke(value);
            }
        }
    }
}