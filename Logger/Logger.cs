
using NLog;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System.Collections.Specialized;
using System.Text;
using System.Xml;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Newtonsoft.Json;
using MyLoggerNamespace.Models;
using MyLoggerNamespace.Enums;
using MyLoggerNamespace.Helpers;
using LogLevel = NLog.LogLevel;

namespace MyLoggerNamespace
{
    public interface IMyLogger
    {
        bool IsWritable { get; }

        public void WriteLine(MessageType messageType, string message, params object[] arrParams);
        public void WriteLine(Exception p_objExeption, string message, params object[] arrParams);
    }

    public class Logger : IMyLogger
    {
        private static readonly object _locker = new object();
        protected NLog.Logger m_logger;

        private bool m_bIsClosed = false;
        private bool m_bIsStoped = false;

        public static RabbitMQSettings RabbitMQSettings;
        public static ConnectionFactory Factory;
        public static IConnection Connection;
        public static IModel Channel;

        public static string MachineName = Environment.MachineName;
        static Logger()
        {
        }

        public string Name { get; set; }
        public string Format { get; set; }
        public MessageType MessageTypes { get; set; }
        public bool IsStoped { get; set; }
        private Logger()
        {
            MessageTypes = MessageType.Everything;
        }

        public Logger(string sName)
            : this()
        {
            this.Name = sName;
            m_logger = GetLogger(sName);
            Loggers.AddLogger(this);
        }

        public Logger(XmlNode xndLogger, string sName, RabbitMQSettings rabbitMQSettings = null) 
            : this()
        {
            this.Name = sName;
            if (rabbitMQSettings != null) { RabbitMQSettings = rabbitMQSettings; }
            if (xndLogger == null)
            {
                throw (new ArgumentNullException("p_nodLogger"));
            }

            this.InitTypes(xndLogger);

            XmlNodeList ndlOut = xndLogger.SelectNodes("Out");

            //ну и какой смысл от этого блядского цикла, который все равно перезаписывает по последнему значению, а?
            //сначала опрашиваем все ауты на предмет наличия спец флагов. потом  уже фигачим по списку.
            foreach (XmlNode nodOut in ndlOut)
            {
                string sType = nodOut.Attributes["Type"].Value;

                if (sType.ToLower() == "rabbitmq")
                {
                    RabbitMQSettings = GetRabbitMQSettings(nodOut);
                }
            }
            m_logger = GetLogger(sName);
            try
            {
                //проверка во избежании дублей создания конекшенов
                if ((RabbitMQSettings != null) && (Channel == null))
                {
                    Factory = new ConnectionFactory() { UserName = RabbitMQSettings.UserName, Password = RabbitMQSettings.Password };
                    Factory.AutomaticRecoveryEnabled = true;
                    Factory.NetworkRecoveryInterval = new TimeSpan(0, 0, 3);

                    var dir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
                    string curFolder = dir.Substring(dir.LastIndexOf('\\') + 1);
                    Connection = Factory.CreateConnection(RabbitMQSettings.HostName.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries), curFolder + "_Logger");
                    Connection.ConnectionShutdown += Connection_ConnectionShutdown;
                    Channel = Connection.CreateModel();
                }
            }
            catch (Exception e)
            {
                m_logger.Log(LogLevel.Error, "[Logger] Check Rabbit config. Rabbit on connect throw {0}", e.Message);
            }

            Loggers.AddLogger(this);
        }

        private void Connection_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            m_logger.Error($"RabbitMQ Connection shutdown: {e.Cause}");
        }

        private RabbitMQSettings GetRabbitMQSettings(XmlNode nodOut)
        {
            RabbitMQSettings temp = null;
            if (nodOut.Attributes["HostName"] == null) { return temp; }
            try
            {
                temp = new RabbitMQSettings
                {
                    Password = nodOut.Attributes["Password"] != null ? nodOut.Attributes["Password"].Value : "guest",
                    UserName = nodOut.Attributes["UserName"] != null ? nodOut.Attributes["UserName"].Value : "guest",
                    HostName = nodOut.Attributes["HostName"].Value,
                    Port = nodOut.Attributes["Port"] != null ? int.Parse(nodOut.Attributes["Port"].Value) : 5672,
                    Start = nodOut.Attributes["Start"] != null ? bool.Parse(nodOut.Attributes["Start"].Value) : true,
                    Exchange = nodOut.Attributes["Exchange"] != null ? nodOut.Attributes["Exchange"].Value : "logstash-rabbitmq",
                    RoutingKey = nodOut.Attributes["RoutingKey"] != null ? nodOut.Attributes["RoutingKey"].Value : "logstash-key",
                    RoutingKeyTimingAPI = nodOut.Attributes["RoutingKeyTimingAPI"] != null ? nodOut.Attributes["RoutingKeyTimingAPI"].Value : "timingapilog-key",
                    RoutingKeyTimingProcessings = nodOut.Attributes["RoutingKeyTimingProcessings"] != null ? nodOut.Attributes["RoutingKeyTimingProcessings"].Value : "timingprocessingslog-key",
                };
            }
            catch (Exception) {/*что поделать, я не знаю что написать в этом кетче   */}
            return temp;
        }

        private NLog.Logger GetLogger(string sName)
        {
            if (Loggers.Contains(sName))
                return Loggers.GetLogger(sName).m_logger;

            NLog.Logger logger;
            lock (_locker)
            {

                //LoggingConfiguration config = new LoggingConfiguration();
                FileTarget fileTarget = new FileTarget();
                //config.AddTarget( sName, fileTarget );

                // Step 3. Set target properties 
                fileTarget.FileName = "${basedir}/log/${date:format=yyyy-MM-dd}-${logger}.log";
                fileTarget.Layout = "${message}";
                SplitGroupTarget t = new SplitGroupTarget();
                t.Targets.Add(fileTarget);
                //fileTarget.ArchiveFileName = "${basedir}/log/lastweek/${date:format=yyyy-MM-dd}-${logger}.{#####}.log";
                //fileTarget.ArchiveEvery = FileArchivePeriod.Day;
                //fileTarget.MaxArchiveFiles = 7;
                //fileTarget.ArchiveNumbering = ArchiveNumberingMode.Sequence;
                //fileTarget.ConcurrentWrites = false;
                //fileTarget.KeepFileOpen = false;
                //fileTarget.ArchiveAboveSize = 100000000;
                //fileTarget.CreateDirs = true;
                //fileTarget.AutoFlush = false;
                fileTarget.KeepFileOpen = false;
                //fileTarget.KeepFileOpen = false;
                fileTarget.Encoding = Encoding.UTF8;
                AsyncTargetWrapper wrapper = new AsyncTargetWrapper();

                wrapper.WrappedTarget = t;
                wrapper.QueueLimit = 50000;
                wrapper.OverflowAction = AsyncTargetWrapperOverflowAction.Grow;


                //NLog.Config.SimpleConfigurator.ConfigureForTargetLogging( wrapper, LogLevel.Debug );
                NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(wrapper, LogLevel.Debug);
                /*
                config = new LoggingConfiguration();
                config.AddTarget( "File", wrapper );
                
                LoggingRule rule2 = new LoggingRule( "*", LogLevel.Debug, fileTarget );


                config.LoggingRules.Add( rule2 );

                LogManager.Configuration = config;
                */
                logger = LogManager.GetLogger(sName);
            }


            //NLog.Config.SimpleConfigurator.ConfigureForTargetLogging( wrapper, LogLevel.Debug ); 


            // Step 5. Activate the configuration
            //LogManager.Configuration = config;
            return logger;
        }

        public void InitTypes(XmlNode p_xndLogger)
        {
            if (p_xndLogger.SelectSingleNode("Types") == null)
            {
                return;
            }
            MessageTypes = MessageType.Nothing;
            XmlNodeList ndlType = p_xndLogger.SelectNodes("Types/Type");
            StringCollection colTypeNames = new StringCollection();
            colTypeNames.AddRange(Enum.GetNames(typeof(MessageType)));

            foreach (XmlNode nodType in ndlType)
            {
                string sMessageType = nodType.Attributes["Name"].Value;

                if (colTypeNames.Contains(sMessageType))
                {
                    MessageTypes |= (MessageType)Enum.Parse(typeof(MessageType), sMessageType, true);
                }
            }
        }

        private string GetPrefix(MessageType p_enuMessageType, string dateTime, int pid)
        {
            StringBuilder sblOut = new StringBuilder();

            string sTypePrefix = "";
            switch (p_enuMessageType)
            {
                case MessageType.Debug:
                    sTypePrefix = "[D]";
                    break;
                case MessageType.Error:
                    sTypePrefix = "[E]";
                    break;
                case MessageType.Info:
                    sTypePrefix = "[I]";
                    break;
                case MessageType.Verbose:
                    sTypePrefix = "[V]";
                    break;
                case MessageType.Warning:
                    sTypePrefix = "[W]";
                    break;
                case MessageType.Nothing:
                case MessageType.Everything:
                    sTypePrefix = "[-]";
                    break;
            }
            sblOut.Append(sTypePrefix);
            sblOut.Append(' ');
            sblOut.Append('[' + string.Format("{0,3}", pid) + ']');
            sblOut.Append(' ');

            sblOut.Append(dateTime);
            sblOut.Append('\t');

            return (sblOut.ToString());
        }

        /// <summary>
        /// Будет ли что либо записано в лог
        /// </summary>
        public bool IsWritable
        {
            get
            {
                return m_logger != null;
            }
        }

        /// <summary>
        /// Будет ли что-либо записано в лог для этого типа сообщений
        /// </summary>
        /// <param name="p_eMessageType"></param>
        /// <returns></returns>
        public bool IsTypeWritable(MessageType p_eMessageType)
        {
            return (MessageTypes & p_eMessageType) == p_eMessageType;
        }


        public void Start()
        {
            this.WriteLine(MessageType.Nothing, "Logging started ...");
        }

        public void Flush()
        {
            try
            {
                Channel.WaitForConfirmsOrDie();
                Channel.Close();
                Connection.Close();
                GC.Collect();
            }
            catch (Exception) { }
        }

        public void Close()
        {

            m_bIsClosed = true;

            Loggers.RemoveLogger(this);
            try
            {
                Channel.WaitForConfirmsOrDie();
                Channel.Close();
                Connection.Close();
                GC.Collect();
            }
            catch (Exception) { }
        }

        public void Write(Exception p_objExeption)
        {
            this.WriteLine(MessageType.Error, "[{1}]{0}{2}", Environment.NewLine, Thread.CurrentThread.GetHashCode(), this.PrintException(p_objExeption, true));
        }

        public void WriteLine(MessageType messageType, string message, params object[] arrParams)
        {
            if (arrParams == null)
            {
                arrParams = new object[] { };
            }

            WriteLine(string.Empty, messageType, string.Empty, message, arrParams);
        }

        public void WriteLine(Exception p_objExeption, string message, params object[] arrParams)
        {
            if (p_objExeption != null)
            {
                message = string.Format("{0} with Full Exception = [{1}]", message, this.PrintException(p_objExeption, true));
            }
            WriteLine(string.Empty, MessageType.Error, string.Empty, message, arrParams);
        }

        private DateTime _lastPublishFailTime;
        private int _reconnectDelaySec = 10;

        public void WriteLine(string ProcessId, MessageType MessageType, string Title, string sMessage, params object[] arrParams)
        {
            if (!this.IsCanAndNeedToWrite(MessageType))
            {
                return;
            }
            if (arrParams == null)
            {
                arrParams = new object[] { };
            }
            else if (arrParams.Length > 0)
            {
                sMessage = string.Format(sMessage, arrParams);
            }

            sMessage = sMessage.MaskedLog();

            StringBuilder sb = new StringBuilder();

            string logDateTime = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff");
            string processId = string.IsNullOrEmpty(ProcessId) ? "" : string.Format("ProcId:{0,10}\t", ProcessId);
            int pid = Thread.CurrentThread.GetHashCode();
            sb.Append(GetPrefix(MessageType, logDateTime, pid)).
                Append(processId);
            if (!string.IsNullOrEmpty(Title))
            {
                sb.Append("*** ").Append(Title).Append(" ***\t");
            }
            sb.Append(sMessage);
            sb.Append($" {GetLogContext()}");
            m_logger.Log(LogLevel.Debug, sb.ToString());

            if (RabbitMQSettings != null)
            {
                Task task = new Task(() =>
                {
                    try
                    {
                        if ((DateTime.Now - _lastPublishFailTime).TotalSeconds > _reconnectDelaySec)
                        {
                            byte[] body = Encoding.UTF8.GetBytes(GetMessageInner(m_logger.Name, sMessage, logDateTime, MessageType, pid));
                            if (Channel.IsOpen)
                            {
                                string rKey = RabbitMQSettings.RoutingKey;
                                if (m_logger.Name == "TimingAPI")
                                    rKey = RabbitMQSettings.RoutingKeyTimingAPI;
                                if (m_logger.Name.StartsWith("Timing_Processing"))
                                    rKey = RabbitMQSettings.RoutingKeyTimingProcessings;
                                Channel.BasicPublish(RabbitMQSettings.Exchange, rKey, null, body);
                            }
                            else
                            {
                                m_logger.Log(LogLevel.Info, "[Server] RabbitMQ channel is closed. Waiting reconnect...");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _lastPublishFailTime = DateTime.Now;
                        m_logger.Log(LogLevel.Error, "[Server] RabbitMQ is not available and we will not be publish messages during next {1} seconds. Message: {0}. StackTrace: {2}", e.Message, _reconnectDelaySec, e.StackTrace);
                    }
                });

                task.Start();
            }
        }

        /// <summary>
        /// Specific method for TimingAPI Logger. Write log message only without prefix.
        /// </summary>
        /// <param name="MessageType"></param>
        /// <param name="sMessage"></param>
        public void WriteLineWithoutPrefix(MessageType MessageType, string sMessage)
        {
            if (!this.IsCanAndNeedToWrite(MessageType))
            {
                return;
            }
            m_logger.Debug(sMessage);
        }

        /// <summary>
        /// Формируем json объект для эластика
        /// </summary>
        public static string GetMessageInner(string loggerName, string message, string logDateTime, MessageType type, int trace)
        {
            var logLine = new LogLine
            {
                TimeStampISO8601 = logDateTime,
                Message = message,
                MachineName = MachineName,
                LoggerName = loggerName,
                Type = type.ToString(),
                Trace = trace,
                Service = Path.GetFileName(Directory.GetCurrentDirectory()),
                OriginalOrderId = LogContext.Current?.GetOriginalOrderId()
            };
            return JsonConvert.SerializeObject(logLine);
        }

        public bool IsCanAndNeedToWrite(MessageType p_enuMessageType)
        {
            if (!this.IsWritable)
            {
                return false;
            }

            if (m_logger != null && (MessageTypes & p_enuMessageType) == p_enuMessageType && !m_bIsClosed && !m_bIsStoped)
            {
                return true;
            }

            if (p_enuMessageType == MessageType.Everything)
            {
                return true;
            }

            return false;
        }

        private string PrintException(Exception p_objException, bool p_bMain)
        {
            if (p_objException == null)
            {
                return "";
            }

            string[] asTrace = new string[12];

            if (p_objException.InnerException != null)
            {
                asTrace[0] = this.PrintException(p_objException.InnerException, false);
                if (!string.IsNullOrEmpty(asTrace[0]))
                {
                    asTrace[0] = asTrace[0].Replace("{", " ").Replace("}", " ");
                }
            }

            asTrace[1] = p_objException.GetType().FullName;
            asTrace[5] = ": ";
            asTrace[6] = p_objException.Message;
            if (!string.IsNullOrEmpty(asTrace[6]))
            {
                asTrace[6] = asTrace[6].Replace("{", " ").Replace("}", " ");
            }

            asTrace[7] = Environment.NewLine;
            asTrace[8] = p_objException.StackTrace;
            if (!string.IsNullOrEmpty(asTrace[8]))
            {
                asTrace[8] = asTrace[8].Replace("{", " ").Replace("}", " ");
            }

            if (p_bMain == false)
            {
                asTrace[9] = Environment.NewLine;
                asTrace[10] = "--------- End of Inner Exception ---------------";
                asTrace[11] = Environment.NewLine;
            }

            return string.Concat(asTrace);
        }

        private string GetLogContext()
        {
            if (LogContext.Current == null || LogContext.Current.IsEmpty())
                return String.Empty;

            return String.Join(", ", LogContext.Current.GetParams().Select(kvp => $"{kvp.Key} = [{kvp.Value}]"));
        }

        public static Logger InitLogger(string loggerName)
        {
            try
            {
                return new Logger(loggerName);
            }
            catch (Exception exc)
            {
                System.IO.File.WriteAllText(
                    Path.Combine("log", $"{loggerName}_{DateTime.Now.Ticks}.log"), exc.Message);
                return null;
            }
        }
    }
    
}