using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLoggerNamespace
{
    public class LogContext
    {
        private static readonly AsyncLocal<LogContext> _currentContext = new AsyncLocal<LogContext>();
        private readonly Lazy<ConcurrentDictionary<string, string>> _contextParams =
            new Lazy<ConcurrentDictionary<string, string>>(() => new ConcurrentDictionary<string, string>());

        private LogContext() { }

        public static LogContext Current => _currentContext?.Value;

        public static LogContext CreateNewLogContext()
        {
            var logContext = new LogContext();
            _currentContext.Value = logContext;
            return logContext;
        }

        public static void ClearContext()
        {
            _currentContext.Value = null;
        }

        public bool IsEmpty()
        {
            return !_contextParams.IsValueCreated || _contextParams.Value.Count == 0;
        }

        public void SetParam(string key, string value)
        {
            if (String.IsNullOrEmpty(key))
                return;
            _contextParams.Value.AddOrUpdate(key, value, (k, v) => v);
        }

        public IDictionary<string, string> GetParams()
        {
            return _contextParams.Value;
        }

        public void SetOriginalOrderId(string originalOrderId)
        {
            SetParam("OriginalOrderId", originalOrderId);
        }

        public string GetOriginalOrderId()
        {
            return _contextParams.Value.TryGetValue("OriginalOrderId", out var originalOrderId) ? originalOrderId : null;
        }
    }
}
