using MyLoggerNamespace.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLoggerNamespace.Models
{
    internal abstract class ALoggerWriter
    {
        public abstract void WriteLine(string ProcessId, MessageType MessageType, string Title, string sMessage);

        public abstract void Start();

        public abstract void Flush();

        public abstract void Close();
    }
}
