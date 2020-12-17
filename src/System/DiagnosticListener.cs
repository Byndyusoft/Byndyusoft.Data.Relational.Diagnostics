#if NETSTANDARD

using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace System.Diagnostics
{
    public class DiagnosticListener : IObservable<KeyValuePair<string, object>>
    {
        public DiagnosticListener(string name)
        {
        }

        public IDisposable Subscribe(IObserver<KeyValuePair<string, object>> observer)
        {
            throw new NotSupportedException();
        }

        public bool IsEnabled()
        {
            return false;
        }

        public bool IsEnabled(string name)
        {
            return false;
        }

        internal void Write(string sqlBeforeExecuteCommand, object p)
        {
            throw new NotImplementedException();
        }
    }
}
#endif