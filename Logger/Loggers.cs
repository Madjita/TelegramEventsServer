using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLoggerNamespace
{
    public class Loggers
    {
        private static Dictionary<string, Logger> colLoggers = new Dictionary<string, Logger>();

        public static void AddLogger(Logger p_objLogger)
        {
            lock (colLoggers)
            {
                colLoggers[p_objLogger.Name] = p_objLogger;
                p_objLogger.Start();
            }
        }

        public static void RemoveLogger(Logger p_objLogger)
        {
            lock (colLoggers)
            {
                colLoggers.Remove(p_objLogger.Name);
            }
        }

        public static Logger GetLogger(string p_sName)
        {
            lock (colLoggers)
            {
                return colLoggers[p_sName];
            }
        }

        public static void CloseAll()
        {
            lock (colLoggers)
            {
                foreach (Logger log in colLoggers.Values)
                {
                    log.Close();
                }
            }
        }

        public static bool Contains(string Name)
        {
            lock (colLoggers)
            {
                return colLoggers.ContainsKey(Name) && colLoggers[Name] != null;
            }
        }
    }
}
